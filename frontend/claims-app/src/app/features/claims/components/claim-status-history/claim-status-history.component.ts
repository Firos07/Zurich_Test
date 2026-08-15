import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { DatePipe, NgFor } from '@angular/common';
import { ClaimStatusHistory } from '../../models/claim';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

@Component({
  selector: 'app-claim-status-history',
  standalone: true,
  imports: [NgFor, DatePipe, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-status-history.component.html',
  styleUrl: './claim-status-history.component.scss',
})
export class ClaimStatusHistoryComponent {
  readonly history = input<ClaimStatusHistory[]>([]);

  trackById(_index: number, item: ClaimStatusHistory): number {
    return item.id;
  }
}