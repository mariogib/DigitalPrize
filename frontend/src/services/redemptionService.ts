/**
 * Redemption API Service
 * Handles all prize redemption-related API calls
 */

import type {
  CompleteRedemptionRequest,
  CompleteRedemptionResponse,
  FilterParameters,
  InitiateRedemptionRequest,
  InitiateRedemptionResponse,
  PagedResponse,
  PrizeRedemptionResponse,
  RedeemablePrizeResponse,
} from '../types';
import { apiClient } from './api/apiClient';

const PUBLIC_PATH = '/api/public/redemption';
const ADMIN_PATH = '/api/redemptions';

export interface RedemptionFilterParameters extends FilterParameters {
  competitionId?: number;
  prizePoolId?: number;
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export const redemptionService = {
  // ===== Public Functions =====

  /** Get redeemable prizes for a confirmation code */
  getRedeemablePrizes(confirmationCode: string): Promise<RedeemablePrizeResponse[]> {
    return apiClient.get<RedeemablePrizeResponse[]>(
      `${PUBLIC_PATH}/prizes/${encodeURIComponent(confirmationCode)}`,
      true
    );
  },

  /** Initiate prize redemption */
  initiateRedemption(request: InitiateRedemptionRequest): Promise<InitiateRedemptionResponse> {
    return apiClient.post<InitiateRedemptionResponse>(`${PUBLIC_PATH}/initiate`, request, true);
  },

  /** Complete prize redemption */
  completeRedemption(request: CompleteRedemptionRequest): Promise<CompleteRedemptionResponse> {
    return apiClient.post<CompleteRedemptionResponse>(`${PUBLIC_PATH}/complete`, request, true);
  },

  /** Get redemption status */
  getRedemptionStatus(redemptionCode: string): Promise<{
    status: string;
    message: string;
    completedAt?: string;
  }> {
    return apiClient.get<{
      status: string;
      message: string;
      completedAt?: string;
    }>(`${PUBLIC_PATH}/status/${encodeURIComponent(redemptionCode)}`, true);
  },

  // ===== Admin Functions =====

  /** Get all redemptions with filtering */
  getRedemptions(
    params?: RedemptionFilterParameters
  ): Promise<PagedResponse<PrizeRedemptionResponse>> {
    const queryParams = new URLSearchParams();
    if (params?.pageNumber) queryParams.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) queryParams.append('pageSize', String(params.pageSize));
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append('sortDescending', String(params.sortDescending));
    }
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    if (params?.competitionId) queryParams.append('competitionId', String(params.competitionId));
    if (params?.prizePoolId) queryParams.append('prizePoolId', String(params.prizePoolId));
    if (params?.status) queryParams.append('status', params.status);
    if (params?.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params?.toDate) queryParams.append('toDate', params.toDate);

    const queryString = queryParams.toString();
    const url = `${ADMIN_PATH}${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<PrizeRedemptionResponse>>(url);
  },

  /** Get redemption by ID */
  getRedemption(id: number): Promise<PrizeRedemptionResponse> {
    return apiClient.get<PrizeRedemptionResponse>(`${ADMIN_PATH}/${String(id)}`);
  },

  /** Approve a pending redemption */
  approveRedemption(id: number, notes?: string): Promise<undefined> {
    return apiClient.put<undefined>(`${ADMIN_PATH}/${String(id)}/approve`, { notes });
  },

  /** Reject a pending redemption */
  rejectRedemption(id: number, reason: string): Promise<undefined> {
    return apiClient.put<undefined>(`${ADMIN_PATH}/${String(id)}/reject`, { reason });
  },

  /** Manually complete a redemption */
  manuallyComplete(id: number, notes: string): Promise<undefined> {
    return apiClient.put<undefined>(`${ADMIN_PATH}/${String(id)}/complete`, { notes });
  },
};
