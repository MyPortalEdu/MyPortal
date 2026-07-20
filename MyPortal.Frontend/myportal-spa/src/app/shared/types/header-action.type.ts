export type HeaderActionSeverity =
  | 'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'danger' | 'error' | 'help' | 'contrast';

export type HeaderAction = {
  label: string;
  icon?: string | undefined;
  outlined?: boolean;
  text?: boolean;
  severity?: HeaderActionSeverity;
  disabled?: boolean;
  loading?: boolean;
  command?: () => void;
};
