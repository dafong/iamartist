import { useState } from "react";
import { smbTest } from "../api";
import type { SmbConfig } from "../types";

interface Props {
  config: SmbConfig;
  onChange: (c: SmbConfig) => void;
}

export default function SmbConfigPanel({ config, onChange }: Props) {
  const [testing, setTesting] = useState(false);
  const [testMsg, setTestMsg] = useState<string | null>(null);

  const field = (key: keyof SmbConfig) => (
    <div className="field">
      <label>{key}</label>
      <input
        type={key === "password" ? "password" : "text"}
        value={config[key] ?? ""}
        onChange={(e) => onChange({ ...config, [key]: e.target.value })}
        placeholder={key === "workgroup" ? "WORKGROUP (optional)" : undefined}
      />
    </div>
  );

  async function handleTest() {
    setTesting(true);
    setTestMsg(null);
    try {
      const entries = await smbTest(config);
      setTestMsg(`连接成功，远程目录包含 ${entries.length} 个文件/文件夹`);
    } catch (e) {
      setTestMsg(`连接失败: ${e}`);
    } finally {
      setTesting(false);
    }
  }

  return (
    <section className="panel">
      <h2>SMB 网盘配置</h2>
      {field("host")}
      {field("share")}
      {field("username")}
      {field("password")}
      {field("remote_dir")}
      {field("workgroup")}
      <button onClick={handleTest} disabled={testing || !config.host}>
        {testing ? "测试中…" : "测试连接"}
      </button>
      {testMsg && (
        <p className={testMsg.startsWith("连接成功") ? "msg-ok" : "msg-err"}>
          {testMsg}
        </p>
      )}
    </section>
  );
}
