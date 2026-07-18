import {ButtonSeverity} from 'primeng/button';

export type HeaderAction = {
  label: string;
  icon?: string | undefined;
  outlined?: boolean;
  text?: boolean;
  severity?: ButtonSeverity;
  disabled?: boolean;
  loading?: boolean;
  command?: () => void;
};
