import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ClaimsService } from '../../services/claims.service';

@Component({
  selector: 'app-claim-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './claim-form.component.html',
  styleUrl: './claim-form.component.scss',
})
export class ClaimFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly claimsService = inject(ClaimsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly claimId = Number(this.route.snapshot.paramMap.get('id'));

  readonly isEdit = Number.isFinite(this.claimId) && this.claimId > 0;

  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    policyNumber: ['', [Validators.required, Validators.maxLength(50)]],
    insuredName: ['', [Validators.required, Validators.maxLength(200)]],
    claimType: ['', [Validators.required, Validators.maxLength(100)]],
    estimatedAmount: [null as number | null, [Validators.required, Validators.min(0.01)]],
  });

  ngOnInit(): void {
    if (this.isEdit) {
      this.loadClaim();
    }
  }

  private loadClaim(): void {
    this.loading.set(true);
    this.error.set(null);

    this.claimsService.getClaim(this.claimId).subscribe({
      next: (claim) => {
        this.form.patchValue({
          policyNumber: claim.policyNumber,
          insuredName: claim.insuredName,
          claimType: claim.claimType,
          estimatedAmount: claim.estimatedAmount,
        });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('No fue posible cargar el siniestro.');
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      policyNumber: value.policyNumber!.trim(),
      insuredName: value.insuredName!.trim(),
      claimType: value.claimType!.trim(),
      estimatedAmount: value.estimatedAmount!,
    };

    this.submitting.set(true);
    this.error.set(null);

    const operation = this.isEdit
      ? this.claimsService.updateClaim(this.claimId, request)
      : this.claimsService.createClaim(request);

    operation.subscribe({
      next: (claim) => {
        this.submitting.set(false);
        void this.router.navigate(['/claims', claim.id], {
          state: {
            message: this.isEdit
              ? 'El siniestro fue actualizado correctamente.'
              : 'El siniestro fue creado correctamente.',
          },
        });
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.error.set(this.extractError(err));
      },
    });
  }

  private extractError(err: HttpErrorResponse): string {
    const detail = (err.error as { detail?: string } | null)?.detail;
    return detail ?? 'No fue posible completar la operación.';
  }
}