import { useState } from "react";
import FolderSelector from "./components/FolderSelector";
import IconSelector from "./components/IconSelector";
import ActionButtons from "./components/ActionButtons";
import StatusBar from "./components/StatusBar";
import "./App.css";

export type StatusType = "idle" | "success" | "error";

export interface StatusState {
  message: string;
  type: StatusType;
}

function App() {
  const [folderPath, setFolderPath] = useState<string>("");
  const [icoPath, setIcoPath] = useState<string>("");
  const [status, setStatus] = useState<StatusState>({
    message: "",
    type: "idle",
  });
  const [loading, setLoading] = useState(false);

  const showStatus = (message: string, type: StatusType) => {
    setStatus({ message, type });
    if (type !== "idle") {
      setTimeout(() => setStatus({ message: "", type: "idle" }), 5000);
    }
  };

  return (
    <div className="app-card">
      <h1 className="app-title">Folder Icon Manager</h1>

      <FolderSelector
        folderPath={folderPath}
        onFolderSelect={setFolderPath}
        disabled={loading}
      />

      <IconSelector
        icoPath={icoPath}
        onIconSelect={setIcoPath}
        disabled={loading}
      />

      <ActionButtons
        folderPath={folderPath}
        icoPath={icoPath}
        loading={loading}
        setLoading={setLoading}
        showStatus={showStatus}
      />

      <StatusBar status={status} />
    </div>
  );
}

export default App;
