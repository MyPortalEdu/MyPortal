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

// Mirrors MyPortal.Contracts.Models.System.Users.UserChangePasswordRequest.
// Self-service change (requires the current password); distinct from the admin
// set-password on /api/users/{id}/password.
export interface MeChangePasswordRequest {
  currentPassword: string;
  password: string;
}
