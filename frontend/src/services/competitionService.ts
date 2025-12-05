/**
 * Competition API Service
 */
import type {
  CompetitionDetailResponse,
  CompetitionResponse,
  CreateCompetitionRequest,
  CreateRegistrationFieldRequest,
  PagedResponse,
  RegistrationFieldResponse,
  UpdateCompetitionRequest,
  UpdateRegistrationFieldRequest,
} from '../types';
import { apiClient } from './api/apiClient';

interface CompetitionQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  status?: string;
}

const BASE_PATH = '/api/competitions';
const PUBLIC_PATH = '/api/public/competitions';

export const competitionService = {
  /** Get paged list of competitions */
  getCompetitions(
    params: CompetitionQueryParams = {}
  ): Promise<PagedResponse<CompetitionResponse>> {
    const searchParams = new URLSearchParams();
    if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    if (params.sortBy) searchParams.append('sortBy', params.sortBy);
    if (params.sortDescending !== undefined)
      searchParams.append('sortDescending', params.sortDescending.toString());
    if (params.status) searchParams.append('status', params.status);

    const query = searchParams.toString();
    return apiClient.get<PagedResponse<CompetitionResponse>>(
      `${BASE_PATH}${query ? `?${query}` : ''}`
    );
  },

  /** Get competition by ID */
  getCompetition(id: number): Promise<CompetitionDetailResponse> {
    return apiClient.get<CompetitionDetailResponse>(`${BASE_PATH}/${String(id)}`);
  },

  /** Get active competitions (public) */
  getActiveCompetitions(): Promise<CompetitionResponse[]> {
    return apiClient.get<CompetitionResponse[]>(PUBLIC_PATH, true);
  },

  /** Create a new competition */
  createCompetition(request: CreateCompetitionRequest): Promise<CompetitionResponse> {
    return apiClient.post<CompetitionResponse>(BASE_PATH, request);
  },

  /** Update a competition */
  updateCompetition(id: number, request: UpdateCompetitionRequest): Promise<CompetitionResponse> {
    return apiClient.put<CompetitionResponse>(`${BASE_PATH}/${String(id)}`, request);
  },

  /** Update competition status */
  updateStatus(id: number, status: string): Promise<undefined> {
    return apiClient.put<undefined>(`${BASE_PATH}/${String(id)}/status`, { status });
  },

  /** Add registration field to competition */
  addRegistrationField(
    competitionId: number,
    request: CreateRegistrationFieldRequest
  ): Promise<RegistrationFieldResponse> {
    return apiClient.post<RegistrationFieldResponse>(
      `${BASE_PATH}/${String(competitionId)}/fields`,
      request
    );
  },

  /** Update registration field */
  updateRegistrationField(
    fieldId: number,
    request: UpdateRegistrationFieldRequest
  ): Promise<RegistrationFieldResponse> {
    return apiClient.put<RegistrationFieldResponse>(
      `${BASE_PATH}/fields/${String(fieldId)}`,
      request
    );
  },

  /** Delete registration field */
  deleteRegistrationField(fieldId: number): Promise<undefined> {
    return apiClient.delete<undefined>(`${BASE_PATH}/fields/${String(fieldId)}`);
  },
};
