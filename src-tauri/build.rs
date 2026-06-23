fn main() {
    // Expose the build target triple to the crate so we can locate the
    // PyInstaller sidecar (`binaries/psd_handler-<triple>`) in dev/test.
    println!("cargo:rustc-env=BUILD_TARGET_TRIPLE={}", std::env::var("TARGET").unwrap_or_default());
    tauri_build::build()
}
