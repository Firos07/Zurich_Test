export type ClaimStatus = 'OPEN' | 'IN_REVIEW' | 'CLOSED';

export const CLAIM_STATUSES: readonly ClaimStatus[] = ['OPEN', 'IN_REVIEW', 'CLOSED'];

/** Estado siguiente permitido según la regla OPEN → IN_REVIEW → CLOSED. */
export const NEXT_STATUS: Readonly<Record<ClaimStatus, ClaimStatus | null>> = {
  OPEN: 'IN_REVIEW',
  IN_REVIEW: 'CLOSED',
  CLOSED: null,
};

export const STATUS_LABELS: Readonly<Record<ClaimStatus, string>> = {
  OPEN: 'Abierto',
  IN_REVIEW: 'En revisión',
  CLOSED: 'Cerrado',
};

export function statusLabel(status: ClaimStatus): string {
  return STATUS_LABELS[status];
}

export function nextStatus(status: ClaimStatus): ClaimStatus | null {
  return NEXT_STATUS[status];
}