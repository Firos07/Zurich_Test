import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RouterTestingHarness } from '@angular/router/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { Claim, ClaimStatusHistory } from '../../models/claim';
import { ClaimDetailComponent } from './claim-detail.component';

const baseUrl = `${environment.apiUrl}/claims`;

describe('ClaimDetailComponent', () => {
  let harness: RouterTestingHarness;
  let httpMock: HttpTestingController;

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

  const history: ClaimStatusHistory[] = [
    { id: 1, claimId: 1, previousStatus: null, newStatus: 'OPEN', changedAt: '2026-01-10T10:00:00' },
  ];

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [ClaimDetailComponent],
      providers: [
        provideRouter([{ path: 'claims/:id', component: ClaimDetailComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    harness = await RouterTestingHarness.create();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('loads the claim and its status history on init', async () => {
    const component = await harness.navigateByUrl('/claims/1', ClaimDetailComponent);

    const getReq = httpMock.expectOne(`${baseUrl}/1`);
    expect(getReq.request.method).toBe('GET');
    getReq.flush(claim);

    const historyReq = httpMock.expectOne(`${baseUrl}/1/history`);
    historyReq.flush(history);

    await harness.fixture.whenStable();
    harness.fixture.detectChanges();

    expect(component.claim()?.id).toBe(1);
    expect(component.history().length).toBe(1);

    const badge = (harness.fixture.nativeElement as HTMLElement).querySelector('app-status-badge');
    expect(badge).toBeTruthy();

    const timelineItems = (harness.fixture.nativeElement as HTMLElement).querySelectorAll('.timeline__item');
    expect(timelineItems.length).toBe(1);
  });

  it('shows a "Cambiar a" action only when the claim can advance', async () => {
    const component = await harness.navigateByUrl('/claims/1', ClaimDetailComponent);
    httpMock.expectOne(`${baseUrl}/1`).flush(claim);
    httpMock.expectOne(`${baseUrl}/1/history`).flush(history);
    await harness.fixture.whenStable();
    harness.fixture.detectChanges();

    const changeButton = Array.from(
      (harness.fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((b) => b.textContent?.includes('Cambiar a'));

    expect(changeButton).toBeTruthy();
  });

  it('advances the status and reloads the history on success', async () => {
    const component = await harness.navigateByUrl('/claims/1', ClaimDetailComponent);
    httpMock.expectOne(`${baseUrl}/1`).flush(claim);
    httpMock.expectOne(`${baseUrl}/1/history`).flush(history);
    await harness.fixture.whenStable();

    component.onChangeStatus();

    const patchReq = httpMock.expectOne(`${baseUrl}/1/status`);
    expect(patchReq.request.method).toBe('PATCH');
    expect(patchReq.request.body).toEqual({ newStatus: 'IN_REVIEW' });
    patchReq.flush({ ...claim, status: 'IN_REVIEW' });

    httpMock.expectOne(`${baseUrl}/1/history`).flush([
      ...history,
      { id: 2, claimId: 1, previousStatus: 'OPEN', newStatus: 'IN_REVIEW', changedAt: '2026-01-11T10:00:00' },
    ]);

    expect(component.claim()?.status).toBe('IN_REVIEW');
    expect(component.history().length).toBe(2);
  });
});