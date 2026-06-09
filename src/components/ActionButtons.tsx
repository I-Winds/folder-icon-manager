import { invoke } from "@tauri-apps/api/core";
import type { StatusType } from "../App";

interface ActionButtonsProps {
  folderPath: string;
  icoPath: string;
  loading: boolean;
  setLoading: (v: boolean) => void;
  showStatus: (message: string, type: StatusType) => void;
}

function ActionButtons({
  folderPath,
  icoPath,
  loading,
  setLoading,
  showStatus,
}: ActionButtonsProps) {
  const handleApplyIcon = async () => {
    if (!folderPath.trim()) {
      showStatus("请先选择目标文件夹", "error");
      return;
    }
    if (!icoPath.trim()) {
      showStatus("请先选择 ICO 图标文件", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await invoke<string>("apply_icon", {
        folderPath: folderPath.trim(),
        icoPath: icoPath.trim(),
      });
      showStatus(result, "success");
    } catch (e) {
      showStatus(String(e), "error");
    } finally {
      setLoading(false);
    }
  };

  const handleRestoreDefault = async () => {
    if (!folderPath.trim()) {
      showStatus("请先选择目标文件夹", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await invoke<string>("restore_default_icon", {
        folderPath: folderPath.trim(),
      });
      showStatus(result, "success");
    } catch (e) {
      showStatus(String(e), "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="actions-row">
      <button
        className="btn btn-primary"
        onClick={handleApplyIcon}
        disabled={loading}
      >
        {loading ? "处理中..." : "应用图标"}
      </button>
      <button
        className="btn btn-secondary"
        onClick={handleRestoreDefault}
        disabled={loading}
      >
        恢复默认图标
      </button>
    </div>
  );
}

export default ActionButtons;
