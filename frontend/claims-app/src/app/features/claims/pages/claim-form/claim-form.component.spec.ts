import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { Claim } from '../../models/claim';
import { ClaimFormComponent } from './claim-form.component';

const baseUrl = `${environment.apiUrl}/claims`;

const existing: Claim = {
  id: 7,
  policyNumber: 'POL-1007',
  insuredName: 'María Ruiz',
  claimType: 'Incendio',
  estimatedAmount: 3500,
  status: 'OPEN',
  createdAt: '2026-01-12T09:00:00',
  updatedAt: '2026-01-12T09:00:00',
};

describe('ClaimFormComponent (create)', () => {
  let harness: RouterTestingHarness;
  let httpMock: HttpTestingController;
  let router: Router;

  const created: Claim = { ...existing, id: 1, policyNumber: 'POL-2000' };

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [ClaimFormComponent],
      providers: [
        provideRouter([{ path: 'claims/new', component: ClaimFormComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    harness = await RouterTestingHarness.create();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  it('does not submit when the form is invalid', async () => {
    const component = await harness.navigateByUrl('/claims/new', ClaimFormComponent);
    component.form.controls.policyNumber.setValue('');

    component.onSubmit();

    expect(component.form.controls.policyNumber.touched).toBeTrue();
    httpMock.expectNone(baseUrl);
  });

  it('creates a claim and navigates to its detail on success', async () => {
    const navigateSpy = spyOn(router, 'navigate').and.resolveTo(true);
    const component = await harness.navigateByUrl('/claims/new', ClaimFormComponent);

    component.form.setValue({
      policyNumber: 'POL-2000',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 500,
    });
    component.onSubmit();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      policyNumber: 'POL-2000',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 500,
    });
    req.flush(created);

    expect(navigateSpy).toHaveBeenCalledWith(['/claims', 1], {
      state: { message: 'El siniestro fue creado correctamente.' },
    });
  });

  it('shows the server error message when the creation fails', async () => {
    const component = await harness.navigateByUrl('/claims/new', ClaimFormComponent);

    component.form.setValue({
      policyNumber: 'POL-2000',
      insuredName: 'Luis Gómez',
      claimType: 'Robo',
      estimatedAmount: 500,
    });
    component.onSubmit();

    const req = httpMock.expectOne(baseUrl);
    req.flush({ detail: 'Ya existe una póliza con ese número.' }, { status: 400, statusText: 'Bad Request' });

    expect(component.error()).toContain('Ya existe una póliza');
    expect(component.submitting()).toBeFalse();
  });
});

describe('ClaimFormComponent (edit)', () => {
  let harness: RouterTestingHarness;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [ClaimFormComponent],
      providers: [
        provideRouter([{ path: 'claims/:id/edit', component: ClaimFormComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    harness = await RouterTestingHarness.create();
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('pre-fills the form and updates the claim on submit', async () => {
    const component = await harness.navigateByUrl('/claims/7/edit', ClaimFormComponent);
    expect(component.isEdit).toBeTrue();

    const getReq = httpMock.expectOne(`${baseUrl}/7`);
    expect(getReq.request.method).toBe('GET');
    getReq.flush(existing);

    expect(component.form.getRawValue()).toEqual({
      policyNumber: 'POL-1007',
      insuredName: 'María Ruiz',
      claimType: 'Incendio',
      estimatedAmount: 3500,
    });

    component.form.controls.estimatedAmount.setValue(4000);
    component.onSubmit();

    const putReq = httpMock.expectOne(`${baseUrl}/7`);
    expect(putReq.request.method).toBe('PUT');
    expect(putReq.request.body).toEqual({
      policyNumber: 'POL-1007',
      insuredName: 'María Ruiz',
      claimType: 'Incendio',
      estimatedAmount: 4000,
    });
    putReq.flush({ ...existing, estimatedAmount: 4000 });
  });
});