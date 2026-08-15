import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RouterTestingHarness } from '@angular/router/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { Claim } from '../../models/claim';
import { ClaimListComponent } from './claim-list.component';

const baseUrl = `${environment.apiUrl}/claims`;

describe('ClaimListComponent', () => {
  let harness: RouterTestingHarness;
  let httpMock: HttpTestingController;

  const claims: Claim[] = [
    {
      id: 1,
      policyNumber: 'POL-1001',
      insuredName: 'Ana Pérez',
      claimType: 'Accidente',
      estimatedAmount: 1200,
      status: 'OPEN',
      createdAt: '2026-01-10T10:00:00',
      updatedAt: '2026-01-10T10:00:00',
    },
    {
      id: 2,
      policyNumber: 'POL-1005',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 800,
      status: 'CLOSED',
      createdAt: '2026-02-01T09:00:00',
      updatedAt: '2026-02-05T12:00:00',
    },
  ];

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [ClaimListComponent],
      providers: [
        provideRouter([{ path: 'claims', component: ClaimListComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    harness = await RouterTestingHarness.create();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('loads and renders the claims on init', async () => {
    const component = await harness.navigateByUrl('/claims', ClaimListComponent);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(claims);

    await harness.fixture.whenStable();
    harness.fixture.detectChanges();

    const rows = (harness.fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(component.claims().length).toBe(2);
  });

  it('requests filtered claims when filters are applied', async () => {
    const component = await harness.navigateByUrl('/claims', ClaimListComponent);
    httpMock.expectOne(baseUrl).flush(claims);

    component.onFiltersApplied({ status: 'IN_REVIEW', policyNumber: 'POL-1005' });

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('status')).toBe('IN_REVIEW');
    expect(req.request.params.get('policyNumber')).toBe('POL-1005');
    req.flush([claims[1]]);
  });

  it('deletes a claim after confirmation and removes it from the list', async () => {
    const component = await harness.navigateByUrl('/claims', ClaimListComponent);
    httpMock.expectOne(baseUrl).flush(claims);

    component.onDeleteRequest(claims[0]);
    expect(component.pendingDelete()).toEqual(claims[0]);

    component.onDeleteConfirmed();

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(component.pendingDelete()).toBeNull();
    expect(component.claims().map((c) => c.id)).toEqual([2]);
  });

  it('advances the status when a transition is requested', async () => {
    const component = await harness.navigateByUrl('/claims', ClaimListComponent);
    httpMock.expectOne(baseUrl).flush(claims);

    component.onChangeStatus({ claim: claims[0], nextStatus: 'IN_REVIEW' });

    const req = httpMock.expectOne(`${baseUrl}/1/status`);
    expect(req.request.method).toBe('PATCH');
    req.flush({ ...claims[0], status: 'IN_REVIEW' });

    expect(component.claims()[0].status).toBe('IN_REVIEW');
  });
});