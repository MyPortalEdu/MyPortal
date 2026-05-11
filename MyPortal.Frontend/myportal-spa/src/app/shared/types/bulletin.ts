// Matches MyPortal.Common.Enums.BulletinAudienceKind on the backend (TINYINT 1-4).
export enum BulletinAudienceKind {
  AllStaff = 1,
  AllPupils = 2,
  AllParents = 3,
  StudentGroup = 4,
}

export interface BulletinAudienceRequest {
  audienceKind: BulletinAudienceKind;
  studentGroupId?: string | null;
}

export interface BulletinAudienceResponse {
  id: string;
  audienceKind: BulletinAudienceKind;
  studentGroupId?: string | null;
  studentGroupName?: string | null;
}

export interface BulletinCategoryResponse {
  id: string;
  name: string;
  icon: string;
  colourCode: string;
  displayOrder: number;
  active: boolean;
  isSystem: boolean;
  version: number;
}

export interface BulletinCategoryUpsertRequest {
  name: string;
  icon: string;
  colourCode: string;
  displayOrder: number;
  active: boolean;
  expectedVersion: number;
}

export interface BulletinSummaryResponse {
  id: string;
  expiresAt?: string | null;
  pinnedAt?: string | null;
  title: string;
  detail: string;
  createdByName: string;
  createdAt?: string | null;
  categoryId: string;
  categoryName: string;
  categoryIcon: string;
  categoryColourCode: string;
  requiresAcknowledgement: boolean;
  hasAcknowledged?: boolean | null;
}

export interface BulletinDetailsResponse {
  id: string;
  directoryId: string;
  expiresAt?: string | null;
  pinnedAt?: string | null;
  title: string;
  detail: string;
  requiresAcknowledgement: boolean;
  categoryId: string;
  categoryName: string;
  categoryIcon: string;
  categoryColourCode: string;
  createdById: string;
  createdByName: string;
  createdByIpAddress: string;
  createdAt: string;
  lastModifiedById: string;
  lastModifiedByName: string;
  lastModifiedByIpAddress: string;
  lastModifiedAt: string;
  version: number;
  audiences: BulletinAudienceResponse[];
  hasAcknowledged?: boolean | null;
  acknowledgedCount?: number | null;
}

export interface BulletinUpsertRequest {
  expiresAt?: string | null;
  categoryId: string;
  title: string;
  detail: string;
  requiresAcknowledgement: boolean;
  isPinned: boolean;
  audiences: BulletinAudienceRequest[];
  expectedVersion: number;
}

export interface BulletinPinRequest {
  isPinned: boolean;
  expectedVersion: number;
}

export interface BulletinAllowedGroupResponse {
  studentGroupId: string;
  code: string;
  name: string;
}

export interface BulletinSettingsResponse {
  allowedAudienceGroups: BulletinAllowedGroupResponse[];
}

export interface BulletinSettingsUpdateRequest {
  allowedAudienceGroupIds: string[];
}

export interface PageResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface IdResponse {
  id: string;
}
