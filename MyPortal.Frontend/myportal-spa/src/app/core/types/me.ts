import {UserType} from './user-type';

export interface Me {
  id: string;
  username: string;
  email?: string;
  userType: UserType;
  isEnabled: boolean;
  isSystem: boolean;
  displayName: string;
  permissions?: string[];
}

export interface MeChangePasswordRequest {
  currentPassword: string;
  password: string;
}
