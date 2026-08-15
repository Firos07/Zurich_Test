import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatusBadgeComponent } from './status-badge.component';

describe('StatusBadgeComponent', () => {
  let fixture: ComponentFixture<StatusBadgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusBadgeComponent],
    }).compileComponents();
  });

  it('shows the Spanish label of the provided status', () => {
    fixture = TestBed.createComponent(StatusBadgeComponent);
    fixture.componentRef.setInput('status', 'IN_REVIEW');
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('En revisión');
  });
});