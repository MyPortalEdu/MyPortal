import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { BulletinsDataService } from './bulletins-data.service';
import {
  BulletinAudienceKind,
  BulletinCategoryResponse,
  BulletinCategoryUpsertRequest,
  BulletinDetailsResponse,
  BulletinPinRequest,
  BulletinSettingsResponse,
  BulletinSettingsUpdateRequest,
  BulletinSummaryResponse,
  BulletinUpsertRequest,
  PageResult,
} from '../types/bulletin';

describe('BulletinsDataService', () => {
  let service: BulletinsDataService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), BulletinsDataService],
    });
    service = TestBed.inject(BulletinsDataService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('list() GETs /api/bulletins with page+pageSize query', () => {
    const expected: PageResult<BulletinSummaryResponse> = { items: [], totalItems: 0 };
    service.list(2, 50).subscribe(r => expect(r).toEqual(expected));

    const req = http.expectOne(r => r.url === '/api/v1/bulletins' && r.method === 'GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('50');
    req.flush(expected);
  });

  it('list() defaults to page 1, pageSize 25', () => {
    service.list().subscribe();

    const req = http.expectOne(r => r.url === '/api/v1/bulletins');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('25');
    req.flush({ items: [], totalItems: 0 });
  });

  it('getById() GETs /api/bulletins/{id}', () => {
    const detail = { id: 'b1', title: 'X' } as BulletinDetailsResponse;
    service.getById('b1').subscribe(r => expect(r).toEqual(detail));

    const req = http.expectOne('/api/v1/bulletins/b1');
    expect(req.request.method).toBe('GET');
    req.flush(detail);
  });

  it('create() POSTs /api/bulletins with the model body', () => {
    const model: BulletinUpsertRequest = {
      title: 'T', detail: 'D', categoryId: 'cat',
      requiresAcknowledgement: false, isPinned: false,
      audiences: [{ audienceKind: BulletinAudienceKind.AllStaff }],
      expectedVersion: 0,
    };
    service.create(model).subscribe(r => expect(r).toEqual({ id: 'new' }));

    const req = http.expectOne('/api/v1/bulletins');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(model);
    req.flush({ id: 'new' });
  });

  it('update() PUTs /api/bulletins/{id}', () => {
    const model = {} as BulletinUpsertRequest;
    service.update('b1', model).subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toBe(model);
    req.flush(null);
  });

  it('pin() PUTs /api/bulletins/{id}/pin', () => {
    const model: BulletinPinRequest = { isPinned: true, expectedVersion: 3 };
    service.pin('b1', model).subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1/pin');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(model);
    req.flush(null);
  });

  it('acknowledge() POSTs /api/bulletins/{id}/acknowledge with an empty body', () => {
    service.acknowledge('b1').subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1/acknowledge');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush(null);
  });

  it('delete() DELETEs /api/bulletins/{id}', () => {
    service.delete('b1').subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('listCategories(false) caches the active-only response across subscribers', () => {
    const categories: BulletinCategoryResponse[] = [
      { id: 'c1', name: 'A', icon: 'i', colourCode: '#000000', displayOrder: 1, active: true, isSystem: false, version: 1 },
    ];

    service.listCategories().subscribe(r => expect(r).toEqual(categories));
    const first = http.expectOne(r => r.url === '/api/v1/bulletincategories' && !r.params.has('includeInactive'));
    first.flush(categories);

    // Second subscriber must replay the cached value without a fresh HTTP call.
    let second: BulletinCategoryResponse[] | undefined;
    service.listCategories().subscribe(r => (second = r));
    http.expectNone('/api/v1/bulletincategories');
    expect(second).toEqual(categories);
  });

  it('listCategories(true) bypasses the cache and adds includeInactive=true', () => {
    service.listCategories(true).subscribe();

    const req = http.expectOne(r => r.url === '/api/v1/bulletincategories');
    expect(req.request.params.get('includeInactive')).toBe('true');
    req.flush([]);
  });

  it('createCategory() POSTs and invalidates the active-categories cache', () => {
    // Prime the cache.
    service.listCategories().subscribe();
    http.expectOne('/api/v1/bulletincategories').flush([]);

    const model: BulletinCategoryUpsertRequest = {
      name: 'N', icon: 'i', colourCode: '#FFFFFF', displayOrder: 1, active: true, expectedVersion: 0,
    };
    service.createCategory(model).subscribe(r => expect(r).toEqual({ id: 'new' }));

    const post = http.expectOne('/api/v1/bulletincategories');
    expect(post.request.method).toBe('POST');
    expect(post.request.body).toEqual(model);
    post.flush({ id: 'new' });

    // Next active-only call must re-fetch.
    service.listCategories().subscribe();
    http.expectOne('/api/v1/bulletincategories').flush([]);
  });

  it('updateCategory() PUTs and invalidates the active-categories cache', () => {
    service.listCategories().subscribe();
    http.expectOne('/api/v1/bulletincategories').flush([]);

    service.updateCategory('c1', {} as BulletinCategoryUpsertRequest).subscribe();
    const put = http.expectOne('/api/v1/bulletincategories/c1');
    expect(put.request.method).toBe('PUT');
    put.flush(null);

    service.listCategories().subscribe();
    const refetch = http.expectOne('/api/v1/bulletincategories');
    expect(refetch.request.method).toBe('GET');
    refetch.flush([]);
  });

  it('deleteCategory() DELETEs and invalidates the active-categories cache', () => {
    service.listCategories().subscribe();
    http.expectOne('/api/v1/bulletincategories').flush([]);

    service.deleteCategory('c1').subscribe();
    const del = http.expectOne('/api/v1/bulletincategories/c1');
    expect(del.request.method).toBe('DELETE');
    del.flush(null);

    service.listCategories().subscribe();
    const refetch = http.expectOne('/api/v1/bulletincategories');
    expect(refetch.request.method).toBe('GET');
    refetch.flush([]);
  });

  it('getSettings() GETs /api/bulletins/settings', () => {
    const expected = { allowedAudienceGroups: [] } as BulletinSettingsResponse;
    service.getSettings().subscribe(r => expect(r).toEqual(expected));

    const req = http.expectOne('/api/v1/bulletins/settings');
    expect(req.request.method).toBe('GET');
    req.flush(expected);
  });

  it('updateSettings() PUTs /api/bulletins/settings with the model body', () => {
    const model: BulletinSettingsUpdateRequest = { allowedAudienceGroupIds: ['g1', 'g2'] };
    service.updateSettings(model).subscribe();

    const req = http.expectOne('/api/v1/bulletins/settings');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(model);
    req.flush(null);
  });
});
