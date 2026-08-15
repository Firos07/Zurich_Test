import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Claim, ClaimStatusHistory } from '../../models/claim';
import { ClaimStatus, nextStatus } from '../../models/claim-status';
import { ClaimsService } from '../../services/claims.service';
import { StatusBadgeComponent } from '../../components/status-badge/status-badge.component';
import { ClaimStatusHistoryComponent } from '../../components/claim-status-history/claim-status-history.component';

interface UiMessage {
  text: string;
  type: 'success' | 'error';
}

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    StatusBadgeComponent,
    ClaimStatusHistoryComponent,
    ConfirmDialogComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-detail.component.html',
  styleUrl: './claim-detail.component.scss',
})
export class ClaimDetailComponent implements OnInit {
  private readonly claimsService = inject(ClaimsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly claimId = Number(this.route.snapshot.paramMap.get('id'));

  protected readonly nextStatus = nextStatus;

  readonly claim = signal<Claim | null>(null);
  readonly history = signal<ClaimStatusHistory[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly message = signal<UiMessage | null>(null);
  readonly changing = signal(false);
  readonly pendingDelete = signal<Claim | null>(null);

  ngOnInit(): void {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras.state as { message?: string } | null;
    if (state?.message) {
      this.showMessage(state.message, 'success');
    }
    this.loadDetail();
  }

  private loadDetail(): void {
    this.claimsService
      .getClaim(this.claimId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claim) => this.claim.set(claim),
        error: () => this.error.set('No fue posible cargar el siniestro.'),
      });

    this.loadHistory();
  }

  private loadHistory(): void {
    this.claimsService.getStatusHistory(this.claimId).subscribe({
      next: (history) => this.history.set(history),
      error: () => this.history.set([]),
    });
  }

  onChangeStatus(): void {
    const claim = this.claim();
    const target = claim ? nextStatus(claim.status) : null;
    if (!claim || !target) {
      return;
    }

    this.changing.set(true);
    this.claimsService.changeStatus(claim.id, { newStatus: target }).subscribe({
      next: (updated) => {
        this.claim.set(updated);
        this.loadHistory();
        this.changing.set(false);
        this.showMessage(`El siniestro pasó de ${claim.status} a ${target}.`, 'success');
      },
      error: () => {
        this.changing.set(false);
        this.showMessage('La transición de estado no es válida.', 'error');
      },
    });
  }

  onDelete(): void {
    const claim = this.pendingDelete();
    if (!claim) {
      return;
    }

    this.claimsService.deleteClaim(claim.id).subscribe({
      next: () =>
        void this.router.navigate(['/claims'], {
          state: { message: 'El siniestro fue eliminado correctamente.' },
        }),
      error: () => {
        this.pendingDelete.set(null);
        this.showMessage('No fue posible eliminar el siniestro.', 'error');
      },
    });
  }

  private showMessage(text: string, type: UiMessage['type']): void {
    this.message.set({ text, type });
    setTimeout(() => this.message.set(null), 4000);
  }
}