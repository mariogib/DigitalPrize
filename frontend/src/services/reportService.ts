/**
 * Reports API Service
 * Handles all reporting and analytics API calls
 */

import type {
  AwardsReportParameters,
  AwardStats,
  CompetitionSummary,
  DailyStatistic,
  DashboardSummary,
  ExportReportRequest,
  RedemptionStats,
  RegistrationStats,
} from '../types';
import { apiClient } from './api/apiClient';

const BASE_PATH = '/api/reports';

export const reportService = {
  /** Get dashboard summary */
  getDashboardSummary(): Promise<DashboardSummary> {
    return apiClient.get<DashboardSummary>(`${BASE_PATH}/dashboard`);
  },

  /** Get registration statistics */
  getRegistrationStats(params?: {
    competitionId?: number;
    fromDate?: string;
    toDate?: string;
  }): Promise<RegistrationStats> {
    const queryParams = new URLSearchParams();
    if (params?.competitionId) queryParams.append('competitionId', String(params.competitionId));
    if (params?.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params?.toDate) queryParams.append('toDate', params.toDate);

    const queryString = queryParams.toString();
    const url = `${BASE_PATH}/registrations${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<RegistrationStats>(url);
  },

  /** Get award statistics */
  getAwardStats(params?: AwardsReportParameters): Promise<AwardStats> {
    const queryParams = new URLSearchParams();
    if (params?.competitionId) queryParams.append('competitionId', String(params.competitionId));
    if (params?.prizePoolId) queryParams.append('prizePoolId', String(params.prizePoolId));
    if (params?.awardedFrom) queryParams.append('fromDate', params.awardedFrom);
    if (params?.awardedTo) queryParams.append('toDate', params.awardedTo);

    const queryString = queryParams.toString();
    const url = `${BASE_PATH}/awards${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<AwardStats>(url);
  },

  /** Get redemption statistics */
  getRedemptionStats(params?: {
    competitionId?: number;
    prizePoolId?: number;
    fromDate?: string;
    toDate?: string;
  }): Promise<RedemptionStats> {
    const queryParams = new URLSearchParams();
    if (params?.competitionId) queryParams.append('competitionId', String(params.competitionId));
    if (params?.prizePoolId) queryParams.append('prizePoolId', String(params.prizePoolId));
    if (params?.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params?.toDate) queryParams.append('toDate', params.toDate);

    const queryString = queryParams.toString();
    const url = `${BASE_PATH}/redemptions${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<RedemptionStats>(url);
  },

  /** Get daily statistics for charts */
  getDailyStats(params: {
    fromDate: string;
    toDate: string;
    competitionId?: number;
    metric: 'registrations' | 'awards' | 'redemptions';
  }): Promise<DailyStatistic[]> {
    const queryParams = new URLSearchParams();
    queryParams.append('fromDate', params.fromDate);
    queryParams.append('toDate', params.toDate);
    queryParams.append('metric', params.metric);
    if (params.competitionId) queryParams.append('competitionId', String(params.competitionId));

    const url = `${BASE_PATH}/daily?${queryParams.toString()}`;
    return apiClient.get<DailyStatistic[]>(url);
  },

  /** Get competition summaries */
  getCompetitionSummaries(params?: {
    status?: string;
    includeArchived?: boolean;
  }): Promise<CompetitionSummary[]> {
    const queryParams = new URLSearchParams();
    if (params?.status) queryParams.append('status', params.status);
    if (params?.includeArchived !== undefined) {
      queryParams.append('includeArchived', String(params.includeArchived));
    }

    const queryString = queryParams.toString();
    const url = `${BASE_PATH}/competitions${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<CompetitionSummary[]>(url);
  },

  /** Export report data */
  async exportReport(request: ExportReportRequest): Promise<Blob> {
    const response = await fetch(`${BASE_PATH}/export`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error('Failed to export report');
    }

    return response.blob();
  },

  /** Get prize pool performance report */
  getPrizePoolPerformance(prizePoolId: number): Promise<{
    totalPrizes: number;
    awardedCount: number;
    redeemedCount: number;
    expiredCount: number;
    awardRate: number;
    redemptionRate: number;
    averageRedemptionTime: number;
    dailyAwards: DailyStatistic[];
  }> {
    return apiClient.get<{
      totalPrizes: number;
      awardedCount: number;
      redeemedCount: number;
      expiredCount: number;
      awardRate: number;
      redemptionRate: number;
      averageRedemptionTime: number;
      dailyAwards: DailyStatistic[];
    }>(`${BASE_PATH}/prize-pools/${String(prizePoolId)}/performance`);
  },
};
