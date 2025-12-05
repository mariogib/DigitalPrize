/**
 * Prize Award API Service
 * Handles all prize award-related API calls
 */

import type {
  AwardPrizeRequest,
  BulkAwardPrizesRequest,
  BulkAwardResponse,
  CancelAwardRequest,
  FilterParameters,
  PagedResponse,
  PrizeAwardDetailResponse,
  PrizeAwardResponse,
} from '../types';
import { apiClient } from './api/apiClient';

const BASE_PATH = '/api/prizeawards';

export interface AwardFilterParameters extends FilterParameters {
  competitionId?: number;
  prizePoolId?: number;
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export const awardService = {
  /** Get all awards with filtering */
  getAwards(params?: AwardFilterParameters): Promise<PagedResponse<PrizeAwardResponse>> {
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
    const url = `${BASE_PATH}${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<PrizeAwardResponse>>(url);
  },

  /** Get award by ID */
  getAward(id: number): Promise<PrizeAwardDetailResponse> {
    return apiClient.get<PrizeAwardDetailResponse>(`${BASE_PATH}/${String(id)}`);
  },

  /** Get awards for a registration */
  getAwardsForRegistration(registrationId: number): Promise<PrizeAwardResponse[]> {
    return apiClient.get<PrizeAwardResponse[]>(
      `${BASE_PATH}/registration/${String(registrationId)}`
    );
  },

  /** Award a prize */
  awardPrize(request: AwardPrizeRequest): Promise<PrizeAwardResponse> {
    return apiClient.post<PrizeAwardResponse>(BASE_PATH, request);
  },

  /** Bulk award prizes */
  bulkAwardPrizes(request: BulkAwardPrizesRequest): Promise<BulkAwardResponse> {
    return apiClient.post<BulkAwardResponse>(`${BASE_PATH}/bulk`, request);
  },

  /** Cancel an award */
  cancelAward(id: number, request: CancelAwardRequest): Promise<undefined> {
    return apiClient.put<undefined>(`${BASE_PATH}/${String(id)}/cancel`, request);
  },

  /** Resend award notification */
  resendNotification(id: number): Promise<undefined> {
    return apiClient.post<undefined>(`${BASE_PATH}/${String(id)}/resend-notification`, {});
  },

  /** Extend award expiry */
  extendExpiry(id: number, newExpiryDate: string): Promise<PrizeAwardResponse> {
    return apiClient.put<PrizeAwardResponse>(`${BASE_PATH}/${String(id)}/extend-expiry`, {
      newExpiryDate,
    });
  },
};
