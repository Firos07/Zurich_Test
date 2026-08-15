import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ClaimStatus, statusLabel } from '../../models/claim-status';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
})
export class StatusBadgeComponent {
  readonly status = input.required<ClaimStatus>();
  protected readonly statusLabel = statusLabel;
}