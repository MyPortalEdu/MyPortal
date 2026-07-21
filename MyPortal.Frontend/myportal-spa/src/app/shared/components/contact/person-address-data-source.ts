import { Observable } from 'rxjs';

import { IdResponse } from '../../types/bulletin';
import {
  AddressListResponse,
  AddressMatchResponse,
  PersonAddressUpdateRequest,
  PersonAddressUpsertRequest,
} from '../../types/staff-address';

export abstract class PersonAddressDataSource {
  abstract getAddresses(personEntityId: string): Observable<AddressListResponse>;
  abstract searchAddressMatches(
    personEntityId: string,
    query: string,
  ): Observable<AddressMatchResponse[]>;
  abstract addAddress(
    personEntityId: string,
    payload: PersonAddressUpsertRequest,
  ): Observable<IdResponse>;
  abstract updateAddress(
    personEntityId: string,
    addressPersonId: string,
    payload: PersonAddressUpdateRequest,
  ): Observable<IdResponse>;
  abstract removeAddress(personEntityId: string, addressPersonId: string): Observable<void>;
}
