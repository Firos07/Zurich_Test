import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../../environments/environment';
import { ClaimsService } from './claims.service';
import { Claim } from '../models/claim';

describe('ClaimsService', () => {
  let service: ClaimsService;
  let httpMock: HttpTestingController;

  const baseUrl = `${environment.apiUrl}/claims`;

  const claim: Claim = {
    id: 1,
    policyNumber: 'POL-1001',
    insuredName: 'Ana Pérez',
    claimType: 'Accidente',
    estimatedAmount: 1200,
    status: 'OPEN',
    createdAt: '2026-01-10T10:00:00',
    updatedAt: '2026-01-10T10:00:00',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ClaimsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getClaims sends a GET to /claims', () => {
    service.getClaims().subscribe((claims) => expect(claims).toEqual([claim]));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys().length).toBe(0);
    req.flush([claim]);
  });

  it('getClaims appends status and policyNumber query params when provided', () => {
    service.getClaims('IN_REVIEW', ' POL-1001 ').subscribe();

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('status')).toBe('IN_REVIEW');
    expect(req.request.params.get('policyNumber')).toBe('POL-1001');
    req.flush([]);
  });

  it('getClaim sends a GET to /claims/:id', () => {
    service.getClaim(1).subscribe((result) => expect(result).toEqual(claim));

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(claim);
  });

  it('createClaim POSTs the payload', () => {
    const payload = {
      policyNumber: 'POL-2000',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 500,
    };
    service.createClaim(payload).subscribe((result) => expect(result).toEqual(claim));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(claim);
  });

  it('updateClaim PUTs to /claims/:id', () => {
    const payload = {
      policyNumber: 'POL-2000',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 600,
    };
    service.updateClaim(1, payload).subscribe((result) => expect(result).toEqual(claim));

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush(claim);
  });

  it('deleteClaim DELETEs /claims/:id', () => {
    service.deleteClaim(1).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('changeStatus PATCHes /claims/:id/status', () => {
    service.changeStatus(1, { newStatus: 'IN_REVIEW' }).subscribe((result) => expect(result).toEqual(claim));

    const req = httpMock.expectOne(`${baseUrl}/1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ newStatus: 'IN_REVIEW' });
    req.flush(claim);
  });

  it('getStatusHistory GETs /claims/:id/history', () => {
    const history = [
      { id: 1, claimId: 1, previousStatus: null, newStatus: 'OPEN' as const, changedAt: '2026-01-10T10:00:00' },
    ];
    service.getStatusHistory(1).subscribe((result) => expect(result).toEqual(history));

    const req = httpMock.expectOne(`${baseUrl}/1/history`);
    expect(req.request.method).toBe('GET');
    req.flush(history);
  });
});