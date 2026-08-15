import { ChangeDetectionStrategy, Component, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgFor } from '@angular/common';
import { CLAIM_STATUSES, ClaimStatus } from '../../models/claim-status';

export interface ClaimFiltersValue {
  status: ClaimStatus | null;
  policyNumber: string;
}

const ALL = 'ALL';

@Component({
  selector: 'app-claim-filters',
  standalone: true,
  imports: [FormsModule, NgFor],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-filters.component.html',
  styleUrl: './claim-filters.component.scss',
})
export class ClaimFiltersComponent {
  protected readonly statuses = CLAIM_STATUSES;

  readonly status = signal<ClaimStatus | null>(null);
  readonly policyNumber = signal('');

  readonly applied = output<ClaimFiltersValue>();
  readonly cleared = output<void>();

  onStatusChange(value: string): void {
    this.status.set(value === ALL ? null : (value as ClaimStatus));
  }

  onApply(): void {
    this.applied.emit({ status: this.status(), policyNumber: this.policyNumber().trim() });
  }

  onClear(): void {
    this.status.set(null);
    this.policyNumber.set('');
    this.cleared.emit();
  }
}