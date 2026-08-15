import { nextStatus, statusLabel, NEXT_STATUS, STATUS_LABELS } from './claim-status';

describe('claim-status model', () => {
  it('only allows the transitions OPEN → IN_REVIEW → CLOSED', () => {
    expect(NEXT_STATUS.OPEN).toBe('IN_REVIEW');
    expect(NEXT_STATUS.IN_REVIEW).toBe('CLOSED');
    expect(NEXT_STATUS.CLOSED).toBeNull();
  });

  it('nextStatus returns null for CLOSED claims', () => {
    expect(nextStatus('OPEN')).toBe('IN_REVIEW');
    expect(nextStatus('IN_REVIEW')).toBe('CLOSED');
    expect(nextStatus('CLOSED')).toBeNull();
  });

  it('provides a Spanish label for every status', () => {
    expect(STATUS_LABELS.OPEN).toBe('Abierto');
    expect(STATUS_LABELS.IN_REVIEW).toBe('En revisión');
    expect(STATUS_LABELS.CLOSED).toBe('Cerrado');
    expect(statusLabel('CLOSED')).toBe('Cerrado');
  });
});