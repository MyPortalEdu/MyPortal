import { LookupResponse } from './lookup';

export interface PersonAddressResponse {
  addressPersonId: string;
  addressId: string;
  typeId: string;
  isMain: boolean;
  buildingNumber?: string | null;
  buildingName?: string | null;
  apartment?: string | null;
  street: string;
  district?: string | null;
  town: string;
  county: string;
  postcode: string;
  country: string;
  sharedCount: number;
  startDate?: string | null;
  endDate?: string | null;
}

export interface AddressMatchResponse {
  addressId: string;
  buildingNumber?: string | null;
  buildingName?: string | null;
  apartment?: string | null;
  street: string;
  district?: string | null;
  town: string;
  county: string;
  postcode: string;
  country: string;
  linkedPersonCount: number;
}

export interface AddressListResponse {
  addresses: PersonAddressResponse[];
  addressTypes: LookupResponse[];
}

export type AddressEditMode = 'FixInPlace' | 'Moved';

export interface PersonAddressUpsertRequest {
  existingAddressId?: string | null;
  typeId: string;
  isMain: boolean;
  buildingNumber?: string | null;
  buildingName?: string | null;
  apartment?: string | null;
  street?: string | null;
  district?: string | null;
  town?: string | null;
  county?: string | null;
  postcode?: string | null;
  country?: string | null;
  startDate?: string | null;
  endDate?: string | null;
}

export interface PersonAddressUpdateRequest {
  typeId: string;
  isMain: boolean;
  mode: AddressEditMode;
  buildingNumber?: string | null;
  buildingName?: string | null;
  apartment?: string | null;
  street: string;
  district?: string | null;
  town: string;
  county: string;
  postcode: string;
  country: string;
  startDate?: string | null;
  endDate?: string | null;
}
