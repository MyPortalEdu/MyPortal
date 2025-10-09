import {UserType} from '../enums/user-type';

export interface Me {
  id: string;
  userName: string;
  email?: string;
  userType: UserType;
  isEnabled: boolean;
  displayName: string;
  permissions?: string[];
}
