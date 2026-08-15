import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Claim } from '../../models/claim';
import { ClaimTableComponent } from './claim-table.component';

describe('ClaimTableComponent', () => {
  let fixture: ComponentFixture<ClaimTableComponent>;
  let component: ClaimTableComponent;

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
    await TestBed.configureTestingModule({
      imports: [ClaimTableComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(ClaimTableComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('claims', claims);
  });

  it('renders one row per claim', () => {
    fixture.detectChanges();
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('shows the "Cambiar a" button only when a next status exists', () => {
    fixture.detectChanges();
    const changeButtons = (fixture.nativeElement as HTMLElement).querySelectorAll('.btn--sm:not(.btn--secondary):not(.btn--danger)');
    expect(changeButtons.length).toBe(1);
    expect(changeButtons[0]?.textContent?.trim()).toBe('IN_REVIEW');
  });

  it('emits view, edit and remove events with the claim', () => {
    let view!: Claim;
    let edit!: Claim;
    let remove!: Claim;
    component.view.subscribe((c) => (view = c));
    component.edit.subscribe((c) => (edit = c));
    component.remove.subscribe((c) => (remove = c));

    fixture.detectChanges();
    const firstRowButtons = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr:first-child button');

    (firstRowButtons[0] as HTMLButtonElement).click(); // Ver
    (firstRowButtons[1] as HTMLButtonElement).click(); // Editar
    (firstRowButtons[firstRowButtons.length - 1] as HTMLButtonElement).click(); // Eliminar

    expect(view.id).toBe(1);
    expect(edit.id).toBe(1);
    expect(remove.id).toBe(1);
  });

  it('emits changeStatus with the next allowed status', () => {
    let payload: { claim: Claim; nextStatus: string } | undefined;
    component.changeStatus.subscribe((p) => (payload = p));

    component.onChangeStatus(claims[0]);

    expect(payload).toEqual({ claim: claims[0], nextStatus: 'IN_REVIEW' });
  });

  it('does not emit changeStatus for CLOSED claims', () => {
    let emitted = false;
    component.changeStatus.subscribe(() => (emitted = true));

    component.onChangeStatus(claims[1]);

    expect(emitted).toBeFalse();
  });
});