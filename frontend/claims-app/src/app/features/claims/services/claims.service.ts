import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ChangeClaimStatusRequest,
  Claim,
  ClaimStatusHistory,
  CreateClaimRequest,
  UpdateClaimRequest,
} from '../models/claim';
import { ClaimStatus } from '../models/claim-status';

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/claims`;

  getClaims(status?: ClaimStatus | null, policyNumber?: string | null): Observable<Claim[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    if (policyNumber && policyNumber.trim()) {
      params = params.set('policyNumber', policyNumber.trim());
    }
    return this.http.get<Claim[]>(this.baseUrl, { params });
  }

  getClaim(id: number): Observable<Claim> {
    return this.http.get<Claim>(`${this.baseUrl}/${id}`);
  }

  createClaim(request: CreateClaimRequest): Observable<Claim> {
    return this.http.post<Claim>(this.baseUrl, request);
  }

  updateClaim(id: number, request: UpdateClaimRequest): Observable<Claim> {
    return this.http.put<Claim>(`${this.baseUrl}/${id}`, request);
  }

  deleteClaim(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  changeStatus(id: number, request: ChangeClaimStatusRequest): Observable<Claim> {
    return this.http.patch<Claim>(`${this.baseUrl}/${id}/status`, request);
  }

  getStatusHistory(id: number): Observable<ClaimStatusHistory[]> {
    return this.http.get<ClaimStatusHistory[]>(`${this.baseUrl}/${id}/history`);
  }
}