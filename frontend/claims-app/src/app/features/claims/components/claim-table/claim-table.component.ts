import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CurrencyPipe, DatePipe, NgFor } from '@angular/common';
import { Claim } from '../../models/claim';
import { ClaimStatus, nextStatus } from '../../models/claim-status';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

@Component({
  selector: 'app-claim-table',
  standalone: true,
  imports: [NgFor, DatePipe, CurrencyPipe, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-table.component.html',
  styleUrl: './claim-table.component.scss',
})
export class ClaimTableComponent {
  readonly claims = input<Claim[]>([]);

  readonly view = output<Claim>();
  readonly edit = output<Claim>();
  readonly remove = output<Claim>();
  readonly changeStatus = output<{ claim: Claim; nextStatus: ClaimStatus }>();

  protected readonly nextStatus = nextStatus;

  trackById(_index: number, claim: Claim): number {
    return claim.id;
  }

  onView(claim: Claim): void {
    this.view.emit(claim);
  }

  onEdit(claim: Claim): void {
    this.edit.emit(claim);
  }

  onDelete(claim: Claim): void {
    this.remove.emit(claim);
  }

  onChangeStatus(claim: Claim): void {
    const next = nextStatus(claim.status);
    if (next) {
      this.changeStatus.emit({ claim, nextStatus: next });
    }
  }
}