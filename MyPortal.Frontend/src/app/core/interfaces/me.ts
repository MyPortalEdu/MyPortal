export interface Me {
  id: string;
  userName: string;
  email?: string;
  userType: 'Staff' | 'Student' | 'Parent';
  isEnabled: boolean;
  permissions: string[];
}
