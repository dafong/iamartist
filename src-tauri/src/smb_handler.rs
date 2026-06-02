use anyhow::{Context, Result};
use serde::{Deserialize, Serialize};
use std::path::Path;
use std::process::Command;

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct SmbConfig {
    pub host: String,
    /// SMB share name — the top‑level shared folder on the server, e.g. "g_pop"
    pub share: String,
    pub username: String,
    pub password: String,
    /// Remote directory within the share, e.g. "exports/psd"
    pub remote_dir: String,
    /// Domain / workgroup for Windows net use (optional)
    pub workgroup: Option<String>,
}

// ─── Public API ────────────────────────────────────────────────────────────────

pub fn upload_files(config: &SmbConfig, local_files: &[String]) -> Result<Vec<(String, String)>> {
    #[cfg(target_os = "windows")]
    return platform::upload(config, local_files);

    #[cfg(target_os = "macos")]
    return platform::upload(config, local_files);

    #[cfg(not(any(target_os = "windows", target_os = "macos")))]
    anyhow::bail!("unsupported platform");
}

pub fn test_connection(config: &SmbConfig) -> Result<Vec<String>> {
    #[cfg(target_os = "windows")]
    return platform::test(config);

    #[cfg(target_os = "macos")]
    return platform::test(config);

    #[cfg(not(any(target_os = "windows", target_os = "macos")))]
    anyhow::bail!("unsupported platform");
}

// ─── Windows ──────────────────────────────────────────────────────────────────

#[cfg(target_os = "windows")]
mod platform {
    use super::*;

    /// Authenticate the share once, then write via UNC paths.
    pub fn upload(config: &SmbConfig, local_files: &[String]) -> Result<Vec<(String, String)>> {
        let unc = unc_share(config);
        net_use(config, &unc)?;

        let remote_base = Path::new(&unc).join(win_dir(&config.remote_dir));
        std::fs::create_dir_all(&remote_base).with_context(|| format!("mkdir {}", remote_base.display()))?;

        let mut results = Vec::new();
        for local in local_files {
            let name = file_name(local)?;
            let dst = remote_base.join(name);
            std::fs::copy(local, &dst).with_context(|| format!("copy → {}", dst.display()))?;
            results.push((local.clone(), dst.to_string_lossy().into_owned()));
        }
        Ok(results)
    }

    pub fn test(config: &SmbConfig) -> Result<Vec<String>> {
        let unc = unc_share(config);
        net_use(config, &unc)?;

        let remote = Path::new(&unc).join(win_dir(&config.remote_dir));
        let list_path = if remote.exists() { remote } else { Path::new(&unc).to_path_buf() };

        let entries = std::fs::read_dir(list_path)?
            .filter_map(|e| e.ok().and_then(|e| e.file_name().into_string().ok()))
            .collect();
        Ok(entries)
    }

    fn unc_share(config: &SmbConfig) -> String {
        format!("\\\\{}\\{}", config.host, config.share)
    }

    fn net_use(config: &SmbConfig, unc: &str) -> Result<()> {
        let user = match &config.workgroup {
            Some(wg) if !wg.is_empty() => format!("{}\\{}", wg, config.username),
            _ => config.username.clone(),
        };
        let status = Command::new("net")
            .args(["use", unc, &config.password, &format!("/user:{user}")])
            .status()
            .context("failed to run net use")?;
        if !status.success() {
            anyhow::bail!("net use 失败，请检查主机/凭据");
        }
        Ok(())
    }

    /// Convert Unix-style path to Windows-style (forward → backslash, strip leading slash).
    fn win_dir(dir: &str) -> String {
        dir.trim_matches('/').replace('/', "\\")
    }
}

// ─── macOS ────────────────────────────────────────────────────────────────────

#[cfg(target_os = "macos")]
mod platform {
    use super::*;

    pub fn upload(config: &SmbConfig, local_files: &[String]) -> Result<Vec<(String, String)>> {
        let mp = MountPoint::mount(config)?;

        let remote_base = mp.path.join(config.remote_dir.trim_matches('/'));
        let _ = std::fs::create_dir_all(&remote_base);

        let mut results = Vec::new();
        for local in local_files {
            let name = file_name(local)?;
            let dst = remote_base.join(name);
            std::fs::copy(local, &dst).with_context(|| format!("copy → {}", dst.display()))?;
            results.push((local.clone(), dst.to_string_lossy().into_owned()));
        }
        Ok(results)
        // MountPoint::drop unmounts automatically
    }

    pub fn test(config: &SmbConfig) -> Result<Vec<String>> {
        let mp = MountPoint::mount(config)?;

        let target = mp.path.join(config.remote_dir.trim_matches('/'));
        let list_path = if target.exists() { target } else { mp.path.clone() };

        let entries = std::fs::read_dir(list_path)?
            .filter_map(|e| e.ok().and_then(|e| e.file_name().into_string().ok()))
            .collect();
        Ok(entries)
    }

    struct MountPoint {
        path: std::path::PathBuf,
        /// true → we mounted it, so we must unmount on drop
        owned: bool,
    }

    impl MountPoint {
        fn mount(config: &SmbConfig) -> Result<Self> {
            // 1. Check if this share is already mounted somewhere on the system
            if let Some(existing) = find_existing_mount(config) {
                return Ok(Self {
                    path: existing,
                    owned: false, // don't unmount — we didn't mount it
                });
            }

            // 2. Fresh mount — mount_smbfs requires the directory to exist already
            let dir = format!("/tmp/iamartist_smb_{}", std::process::id());
            std::fs::create_dir_all(&dir)?;

            let url = build_smb_url(config);

            let status = Command::new("mount_smbfs")
                .args([&url, &dir])
                .status()
                .context("failed to run mount_smbfs")?;

            if !status.success() {
                let _ = std::fs::remove_dir(&dir);
                anyhow::bail!("mount_smbfs 失败，请检查主机/凭据");
            }
            Ok(Self {
                path: dir.into(),
                owned: true,
            })
        }
    }

    impl Drop for MountPoint {
        fn drop(&mut self) {
            if self.owned {
                let _ = Command::new("umount").arg(&self.path).status();
                let _ = std::fs::remove_dir(&self.path);
            }
        }
    }

    /// Search the system mount table for an existing mount of the same SMB share.
    fn find_existing_mount(config: &SmbConfig) -> Option<std::path::PathBuf> {
        if let Ok(out) = Command::new("mount").output() {
            let info = String::from_utf8_lossy(&out.stdout);
            // mount output shows the URL: //domain;user@host/share or //user@host/share
            let user_part = match config.username.split_once('\\') {
                Some((d, u)) => format!("{};{}", d, u),
                None => config.username.clone(),
            };
            let needle = format!("//{}@{}", user_part, config.host);
            for line in info.lines() {
                if line.contains(&needle) && line.contains(&config.share) {
                    // mount output: //user@host/share on /path (smbfs, ...)
                    if let Some(part) = line.split(" on ").nth(1) {
                        let path = part.split_whitespace().next().unwrap_or("");
                        if !path.is_empty() {
                            return Some(path.into());
                        }
                    }
                }
            }
        }
        None
    }

    /// Build the SMB URL, handling domain\user format → domain;user.
    fn build_smb_url(config: &SmbConfig) -> String {
        let user = &config.username;
        let (domain, user) = match user.split_once('\\') {
            Some((d, u)) => (Some(d), u),
            None => (None, user.as_str()),
        };
        let user_part = match domain {
            Some(d) => format!("{};{}", d, user),
            None => user.to_string(),
        };
        format!(
            "smb://{}:{}@{}/{}",
            percent_encode(&user_part),
            percent_encode(&config.password),
            config.host,
            config.share,
        )
    }

    /// Percent-encode characters that would break an SMB URL.
    fn percent_encode(s: &str) -> String {
        s.bytes()
            .flat_map(|b| {
                if b.is_ascii_alphanumeric() || b"-._~".contains(&b) {
                    vec![b]
                } else {
                    format!("%{b:02X}").into_bytes()
                }
            })
            .map(|b| b as char)
            .collect()
    }
}

// ─── Shared helpers ───────────────────────────────────────────────────────────

fn file_name(path: &str) -> Result<&str> {
    Path::new(path)
        .file_name()
        .and_then(|n| n.to_str())
        .with_context(|| format!("invalid filename: {path}"))
}

#[cfg(test)]
mod tests {
    use std::env;

    use super::{test_connection, upload_files, SmbConfig};

    #[test]
    fn test_smb_upload() {
        let cfg = SmbConfig {
            host: "disk.happyelements.net".to_string(),
            share: "g_pop".to_string(),
            username: "xinlei.fan".to_string(),
            password: "Oi9klk97&".to_string(),
            remote_dir: "unity资源".to_string(),
            workgroup: None,
        };
        let upload_file = env::current_dir().unwrap().join("..").join("export.png");

        upload_files(&cfg, &vec![upload_file.display().to_string()]).unwrap();
    }

    #[test]
    fn test_smb_conn() {
        let cfg = SmbConfig {
            host: "disk.happyelements.net".to_string(),
            share: "g_pop".to_string(),
            username: "xinlei.fan".to_string(),
            password: "Oi9klk97&".to_string(),
            remote_dir: "unity资源".to_string(),
            workgroup: None,
        };
        test_connection(&cfg).expect("connect failed");
    }
}
