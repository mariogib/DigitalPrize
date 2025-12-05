/**
 * Registration API Service
 * Handles all registration-related API calls
 */

import type {
  FilterParameters,
  PagedResponse,
  PublicRegistrationRequest,
  PublicRegistrationResponse,
  RegistrationResponse,
} from '../types';
import { apiClient } from './api/apiClient';

const ADMIN_PATH = '/api/registrations';
const PUBLIC_PATH = '/api/public/registration';

export interface RegistrationFilterParameters extends FilterParameters {
  competitionId?: number;
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export const registrationService = {
  // ===== Admin Functions =====

  /** Get all registrations with filtering */
  getRegistrations(
    params?: RegistrationFilterParameters
  ): Promise<PagedResponse<RegistrationResponse>> {
    const queryParams = new URLSearchParams();
    if (params?.pageNumber) queryParams.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) queryParams.append('pageSize', String(params.pageSize));
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append('sortDescending', String(params.sortDescending));
    }
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    if (params?.competitionId) queryParams.append('competitionId', String(params.competitionId));
    if (params?.status) queryParams.append('status', params.status);
    if (params?.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params?.toDate) queryParams.append('toDate', params.toDate);

    const queryString = queryParams.toString();
    const url = `${ADMIN_PATH}${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<RegistrationResponse>>(url);
  },

  /** Get registration by ID */
  getRegistration(id: number): Promise<RegistrationResponse> {
    return apiClient.get<RegistrationResponse>(`${ADMIN_PATH}/${String(id)}`);
  },

  /** Get registrations for a competition */
  getRegistrationsForCompetition(
    competitionId: number,
    params?: FilterParameters
  ): Promise<PagedResponse<RegistrationResponse>> {
    const queryParams = new URLSearchParams();
    queryParams.append('competitionId', String(competitionId));
    if (params?.pageNumber) queryParams.append('pageNumber', String(params.pageNumber));
    if (params?.pageSize) queryParams.append('pageSize', String(params.pageSize));
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append('sortDescending', String(params.sortDescending));
    }
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);

    const queryString = queryParams.toString();
    const url = `${ADMIN_PATH}?${queryString}`;
    return apiClient.get<PagedResponse<RegistrationResponse>>(url);
  },

  /** Verify a registration */
  verifyRegistration(id: number): Promise<undefined> {
    return apiClient.put<undefined>(`${ADMIN_PATH}/${String(id)}/verify`, {});
  },

  /** Flag a registration */
  flagRegistration(id: number, reason: string): Promise<undefined> {
    return apiClient.put<undefined>(`${ADMIN_PATH}/${String(id)}/flag`, { reason });
  },

  /** Delete a registration */
  deleteRegistration(id: number): Promise<undefined> {
    return apiClient.delete<undefined>(`${ADMIN_PATH}/${String(id)}`);
  },

  // ===== Public Functions =====

  /** Submit a public registration */
  submitRegistration(request: PublicRegistrationRequest): Promise<PublicRegistrationResponse> {
    return apiClient.post<PublicRegistrationResponse>(PUBLIC_PATH, request, true);
  },

  /** Check registration status */
  checkStatus(confirmationCode: string): Promise<{
    status: string;
    message: string;
    awards?: Array<{
      prizeType: string;
      prizeName: string;
      status: string;
    }>;
  }> {
    return apiClient.get<{
      status: string;
      message: string;
      awards?: Array<{
        prizeType: string;
        prizeName: string;
        status: string;
      }>;
    }>(`${PUBLIC_PATH}/status/${encodeURIComponent(confirmationCode)}`, true);
  },

  /** Verify email with token */
  verifyEmail(token: string): Promise<{ success: boolean; message: string }> {
    return apiClient.post<{ success: boolean; message: string }>(
      `${PUBLIC_PATH}/verify-email`,
      {
        token,
      },
      true
    );
  },
};
