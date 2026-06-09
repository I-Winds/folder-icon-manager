interface FolderSelectorProps {
  folderPath: string;
  onFolderSelect: (path: string) => void;
  disabled: boolean;
}

function FolderSelector({ folderPath, onFolderSelect, disabled }: FolderSelectorProps) {
  const handleSelectFolder = async () => {
    try {
      const { open } = await import("@tauri-apps/plugin-dialog");
      const selected = await open({
        directory: true,
        multiple: false,
        title: "选择目标文件夹",
      });
      if (selected && typeof selected === "string") {
        onFolderSelect(selected);
      }
    } catch (e) {
      console.error("Folder selection failed:", e);
    }
  };

  return (
    <div className="selector-group">
      <label className="selector-label">目标文件夹</label>
      <div className="selector-row">
        <input
          type="text"
          className="selector-input"
          value={folderPath}
          placeholder="选择要应用图标的文件夹..."
          readOnly
        />
        <button
          className="selector-btn"
          onClick={handleSelectFolder}
          disabled={disabled}
        >
          浏览...
        </button>
      </div>
    </div>
  );
}

export default FolderSelector;
