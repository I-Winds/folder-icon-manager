interface IconSelectorProps {
  icoPath: string;
  onIconSelect: (path: string) => void;
  disabled: boolean;
}

function IconSelector({ icoPath, onIconSelect, disabled }: IconSelectorProps) {
  const handleSelectIcon = async () => {
    try {
      const { open } = await import("@tauri-apps/plugin-dialog");
      const selected = await open({
        multiple: false,
        title: "选择 ICO 图标文件",
        filters: [
          {
            name: "ICO 图标文件",
            extensions: ["ico"],
          },
        ],
      });
      if (selected && typeof selected === "string") {
        onIconSelect(selected);
      }
    } catch (e) {
      console.error("Icon selection failed:", e);
    }
  };

  return (
    <div className="selector-group">
      <label className="selector-label">ICO 图标文件</label>
      <div className="selector-row">
        <input
          type="text"
          className="selector-input"
          value={icoPath}
          placeholder="选择 .ico 图标文件..."
          readOnly
        />
        <button
          className="selector-btn"
          onClick={handleSelectIcon}
          disabled={disabled}
        >
          浏览...
        </button>
      </div>
    </div>
  );
}

export default IconSelector;
