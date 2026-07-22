import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import { PostUpsertRequest, PostsResponse } from '../types/staff-setup';

@Injectable({ providedIn: 'root' })
export class PostsDataService {
  private readonly http = inject(HttpClient);

  getPosts(): Observable<PostsResponse> {
    return this.http.get<PostsResponse>('/api/v1/posts');
  }

  create(payload: PostUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/posts', payload);
  }

  update(postId: string, payload: PostUpsertRequest): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/posts/${postId}`, payload);
  }

  delete(postId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/posts/${postId}`);
  }
}
