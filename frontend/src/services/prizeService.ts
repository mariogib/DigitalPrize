import {
  type CreatePrizeRequest,
  type PagedResponse,
  type PrizeDetailResponse,
  type PrizeSummaryResponse,
  type UpdatePrizeRequest,
} from '../types';
import { apiClient } from './api/apiClient';

export const prizeService = {
  // Public method - skip auth for public prizes page
  async getAll(): Promise<PrizeSummaryResponse[]> {
    const response = await apiClient.get<PagedResponse<PrizeSummaryResponse>>('/api/prizes', true);
    return response.items;
  },

  getById(id: string): Promise<PrizeDetailResponse> {
    return apiClient.get<PrizeDetailResponse>(`/api/prizes/${id}`);
  },

  create(dto: CreatePrizeRequest): Promise<PrizeDetailResponse> {
    return apiClient.post<PrizeDetailResponse>('/api/prizes', dto);
  },

  update(id: string, dto: UpdatePrizeRequest): Promise<PrizeDetailResponse> {
    return apiClient.put<PrizeDetailResponse>(`/api/prizes/${id}`, dto);
  },

  delete(id: string): Promise<undefined> {
    return apiClient.delete<undefined>(`/api/prizes/${id}`);
  },
};
