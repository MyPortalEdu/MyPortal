import {ButtonSeverity} from 'primeng/button';

export type HeaderAction = {
  label: string;
  icon?: string | undefined;
  outlined?: boolean;
  severity?: ButtonSeverity;
  disabled?: boolean;
  command?: () => void;
};
