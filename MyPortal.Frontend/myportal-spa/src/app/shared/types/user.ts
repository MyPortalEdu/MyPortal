import { UserType } from '../../core/types/user-type';

// Mirrors MyPortal.Contracts.Models.System.Users. UserType serialises as an integer.

export interface UserSummaryResponse {
  id: string;
  createdAt: string;
  personId: string | null;
  userType: UserType;
  isEnabled: boolean;
  isSystem: boolean;
  personFullName: string | null;
  username: string | null;
  email: string | null;
  phoneNumber: string | null;
  twoFactorEnabled: boolean;
  lockoutEnabled: boolean;
}

export interface UserDetailsResponse extends UserSummaryResponse {
  roleIds: string[];
}

// Create payload — carries a password.
export interface UserUpsertRequest {
  personId: string | null;
  userType: UserType;
  isEnabled: boolean;
  username: string;
  email: string | null;
  password: string;
  roleIds: string[];
}

// Update payload — no password (changed via the dedicated set-password endpoint).
export interface UserUpdateRequest {
  personId: string | null;
  userType: UserType;
  isEnabled: boolean;
  username: string;
  email: string | null;
  roleIds: string[];
}

export interface UserSetPasswordRequest {
  password: string;
}

// Mirrors MyPortal.Contracts.Models.People.PersonSearchResponse.
export interface PersonSearchResponse {
  personId: string;
  title: string | null;
  firstName: string;
  middleName: string | null;
  lastName: string;
  preferredFirstName: string | null;
  preferredLastName: string | null;
  dob: string | null;
}
