import { UserType } from '../../core/types/user-type';

export interface RoleSummaryResponse {
  id: string;
  description: string | null;
  isSystem: boolean;
  isDefault: boolean;
  userType: UserType;
  name: string | null;
}

export interface RoleDetailsResponse {
  id: string;
  description: string | null;
  isSystem: boolean;
  isDefault: boolean;
  userType: UserType;
  name: string | null;
  permissionIds: string[];
}

export interface RoleUpsertRequest {
  name: string;
  description: string | null;
  userType: UserType;
  permissionIds: string[];
}

export interface PermissionResponse {
  id: string;
  name: string;
  friendlyName: string;
  area: string;
  userType: UserType;
}
