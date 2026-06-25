// Mirrors MyPortal.Contracts.Models.People.* for the addresses section of the
// Contact Details area. Addresses are shared entities — see the granular endpoints
// and the FixInPlace/Moved edit mode.
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
  // How many people share this address, including this one. > 1 means shared.
  sharedCount: number;
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

// Serialised as strings by the API (JsonStringEnumConverter).
export type AddressEditMode = 'FixInPlace' | 'Moved';

export interface PersonAddressUpsertRequest {
  // Set to link an existing address; the fields below are ignored when it is.
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
}
