import type { StatusState } from "../App";

interface StatusBarProps {
  status: StatusState;
}

function StatusBar({ status }: StatusBarProps) {
  if (status.type === "idle" && !status.message) {
    return <div className="status-bar status-idle">就绪</div>;
  }

  const className = `status-bar status-${status.type}`;
  return <div className={className}>{status.message}</div>;
}

export default StatusBar;
