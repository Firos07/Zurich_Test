import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Claim } from '../../models/claim';
import { ClaimStatus } from '../../models/claim-status';
import { ClaimFiltersComponent, ClaimFiltersValue } from '../../components/claim-filters/claim-filters.component';
import { ClaimTableComponent } from '../../components/claim-table/claim-table.component';
import { ClaimsService } from '../../services/claims.service';

interface UiMessage {
  text: string;
  type: 'success' | 'error';
}

@Component({
  selector: 'app-claim-list',
  standalone: true,
  imports: [RouterLink, ClaimFiltersComponent, ClaimTableComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-list.component.html',
  styleUrl: './claim-list.component.scss',
})
export class ClaimListComponent implements OnInit {
  private readonly claimsService = inject(ClaimsService);
  private readonly router = inject(Router);

  readonly claims = signal<Claim[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly message = signal<UiMessage | null>(null);

  readonly pendingDelete = signal<Claim | null>(null);
  readonly deleting = signal(false);

  ngOnInit(): void {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras.state as { message?: string } | null;
    if (state?.message) {
      this.showMessage(state.message, 'success');
    }
    this.loadClaims();
  }

  private loadClaims(status?: ClaimStatus | null, policyNumber?: string | null): void {
    this.loading.set(true);
    this.error.set(null);

    this.claimsService
      .getClaims(status, policyNumber)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claims) => this.claims.set(claims),
        error: () => this.error.set('No fue posible cargar los siniestros.'),
      });
  }

  onFiltersApplied(filters: ClaimFiltersValue): void {
    this.loadClaims(filters.status, filters.policyNumber);
  }

  onFiltersCleared(): void {
    this.loadClaims();
  }

  onView(claim: Claim): void {
    void this.router.navigate(['/claims', claim.id]);
  }

  onEdit(claim: Claim): void {
    void this.router.navigate(['/claims', claim.id, 'edit']);
  }

  onDeleteRequest(claim: Claim): void {
    this.pendingDelete.set(claim);
  }

  onDeleteConfirmed(): void {
    const claim = this.pendingDelete();
    if (!claim) {
      return;
    }

    this.deleting.set(true);
    this.claimsService
      .deleteClaim(claim.id)
      .pipe(finalize(() => this.deleting.set(false)))
      .subscribe({
        next: () => {
          this.claims.update((list) => list.filter((c) => c.id !== claim.id));
          this.pendingDelete.set(null);
          this.showMessage('El siniestro fue eliminado correctamente.', 'success');
        },
        error: () => {
          this.pendingDelete.set(null);
          this.showMessage('No fue posible eliminar el siniestro.', 'error');
        },
      });
  }

  onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  onChangeStatus(event: { claim: Claim; nextStatus: ClaimStatus }): void {
    this.claimsService.changeStatus(event.claim.id, { newStatus: event.nextStatus }).subscribe({
      next: (updated) => {
        this.claims.update((list) => list.map((c) => (c.id === updated.id ? updated : c)));
        this.showMessage(`El siniestro ${updated.policyNumber} pasó a ${event.nextStatus}.`, 'success');
      },
      error: () => this.showMessage('La transición de estado no es válida.', 'error'),
    });
  }

  private showMessage(text: string, type: UiMessage['type']): void {
    this.message.set({ text, type });
    setTimeout(() => this.message.set(null), 4000);
  }
}