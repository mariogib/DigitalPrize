import { useCallback, useEffect, useState } from 'react';
import { prizeService } from '../services/prizeService';
import { type PrizeSummaryResponse } from '../types';

interface UsePrizesResult {
  prizes: PrizeSummaryResponse[];
  isLoading: boolean;
  error: Error | null;
  refetch: () => Promise<void>;
}

export const usePrizes = (): UsePrizesResult => {
  const [prizes, setPrizes] = useState<PrizeSummaryResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchPrizes = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await prizeService.getAll();
      setPrizes(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch prizes'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchPrizes();
  }, [fetchPrizes]);

  return { prizes, isLoading, error, refetch: fetchPrizes };
};
