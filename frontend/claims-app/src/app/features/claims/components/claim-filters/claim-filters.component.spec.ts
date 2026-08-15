import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClaimFiltersComponent, ClaimFiltersValue } from './claim-filters.component';

describe('ClaimFiltersComponent', () => {
  let fixture: ComponentFixture<ClaimFiltersComponent>;
  let component: ClaimFiltersComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClaimFiltersComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(ClaimFiltersComponent);
    component = fixture.componentInstance;
  });

  it('emits the selected status and policy number on apply', () => {
    let emitted: ClaimFiltersValue | undefined;
    component.applied.subscribe((value) => (emitted = value));

    component.onStatusChange('CLOSED');
    component.policyNumber.set(' POL-1001 ');
    component.onApply();

    expect(emitted).toEqual({ status: 'CLOSED', policyNumber: 'POL-1001' });
  });

  it('emits null status when ALL is selected', () => {
    let emitted: ClaimFiltersValue | undefined;
    component.applied.subscribe((value) => (emitted = value));

    component.onStatusChange('ALL');
    component.onApply();

    expect(emitted?.status).toBeNull();
  });

  it('emits cleared and resets its signals on clear', () => {
    let cleared = false;
    component.cleared.subscribe(() => (cleared = true));

    component.onStatusChange('OPEN');
    component.policyNumber.set('POL-1');
    component.onClear();

    expect(cleared).toBeTrue();
    expect(component.status()).toBeNull();
    expect(component.policyNumber()).toBe('');
  });
});