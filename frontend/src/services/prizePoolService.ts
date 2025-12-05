/**
 * Prize Pool API Service
 * Handles all prize pool-related API calls
 */

import type {
  CreatePrizePoolRequest,
  FilterParameters,
  PagedResponse,
  PrizePoolDetailResponse,
  PrizePoolSummaryResponse,
  UpdatePrizePoolRequest,
} from '../types';
import { apiClient } from './api/apiClient';

const BASE_PATH = '/api/prizepools';

export const prizePoolService = {
  /** Get all prize pools with pagination */
  getPrizePools(params?: FilterParameters): Promise<PagedResponse<PrizePoolSummaryResponse>> {
    const queryParams = new URLSearchParams();
    if (params?.pageNumber) queryParams.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) queryParams.append('pageSize', String(params.pageSize));
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append('sortDescending', String(params.sortDescending));
    }
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);

    const queryString = queryParams.toString();
    const url = `${BASE_PATH}${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<PrizePoolSummaryResponse>>(url);
  },

  /** Get prize pools for a competition */
  getPrizePoolsForCompetition(
    competitionId: number,
    params?: FilterParameters
  ): Promise<PagedResponse<PrizePoolSummaryResponse>> {
    const queryParams = new URLSearchParams();
    queryParams.append('competitionId', String(competitionId));
    if (params?.pageNumber) queryParams.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) queryParams.append('pageSize', String(params.pageSize));
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append('sortDescending', String(params.sortDescending));
    }

    const url = `${BASE_PATH}?${queryParams.toString()}`;
    return apiClient.get<PagedResponse<PrizePoolSummaryResponse>>(url);
  },

  /** Get prize pool by ID */
  getPrizePool(id: number): Promise<PrizePoolDetailResponse> {
    return apiClient.get<PrizePoolDetailResponse>(`${BASE_PATH}/${String(id)}`);
  },

  /** Create a new prize pool */
  createPrizePool(request: CreatePrizePoolRequest): Promise<PrizePoolSummaryResponse> {
    return apiClient.post<PrizePoolSummaryResponse>(BASE_PATH, request);
  },

  /** Update a prize pool */
  updatePrizePool(id: number, request: UpdatePrizePoolRequest): Promise<PrizePoolSummaryResponse> {
    return apiClient.put<PrizePoolSummaryResponse>(`${BASE_PATH}/${String(id)}`, request);
  },

  /** Update prize pool status */
  updateStatus(id: number, status: string): Promise<undefined> {
    return apiClient.put<undefined>(`${BASE_PATH}/${String(id)}/status`, { status });
  },

  /** Delete a prize pool */
  deletePrizePool(id: number): Promise<undefined> {
    return apiClient.delete<undefined>(`${BASE_PATH}/${String(id)}`);
  },

  /** Get prize pool statistics */
  getStatistics(id: number): Promise<{
    totalPrizes: number;
    availablePrizes: number;
    awardedPrizes: number;
    redeemedPrizes: number;
    expiredPrizes: number;
  }> {
    return apiClient.get<{
      totalPrizes: number;
      availablePrizes: number;
      awardedPrizes: number;
      redeemedPrizes: number;
      expiredPrizes: number;
    }>(`${BASE_PATH}/${String(id)}/statistics`);
  },
};
