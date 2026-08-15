import { ClaimStatus } from './claim-status';

export interface Claim {
  id: number;
  policyNumber: string;
  insuredName: string;
  claimType: string;
  estimatedAmount: number;
  status: ClaimStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateClaimRequest {
  policyNumber: string;
  insuredName: string;
  claimType: string;
  estimatedAmount: number;
}

export interface UpdateClaimRequest {
  policyNumber: string;
  insuredName: string;
  claimType: string;
  estimatedAmount: number;
}

export interface ChangeClaimStatusRequest {
  newStatus: ClaimStatus;
}

export interface ClaimStatusHistory {
  id: number;
  claimId: number;
  previousStatus: ClaimStatus | null;
  newStatus: ClaimStatus;
  changedAt: string;
}