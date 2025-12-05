/**
 * Reports and dashboard API types
 */

/** Dashboard summary */
export interface DashboardSummary {
  activeCompetitions: number;
  totalRegistrations: number;
  totalPrizesAwarded: number;
  totalPrizesRedeemed: number;
  pendingRedemptions: number;
  totalPrizeValueAwarded: number;
  totalPrizeValueRedeemed: number;
  externalUsersCount: number;
  recentCompetitions: CompetitionSummary[];
  last7DaysStats: DailyStatistic[];
}

/** Competition summary for dashboard */
export interface CompetitionSummary {
  competitionId: number;
  name: string;
  status: string;
  registrationCount: number;
  awardedCount: number;
  startDate: string;
  endDate: string;
}

/** Daily statistic */
export interface DailyStatistic {
  date: string;
  registrations: number;
  awards: number;
  redemptions: number;
}

/** Registration statistics */
export interface RegistrationStats {
  totalRegistrations: number;
  todayRegistrations: number;
  thisWeekRegistrations: number;
  thisMonthRegistrations: number;
  byCompetition: Record<string, number>;
  dailyBreakdown: DailyStatistic[];
}

/** Award statistics */
export interface AwardStats {
  totalAwarded: number;
  totalRedeemed: number;
  totalExpired: number;
  pendingRedemption: number;
  totalValueAwarded: number;
  totalValueRedeemed: number;
  byPrizeType: Record<string, number>;
  dailyBreakdown: DailyStatistic[];
}

/** Redemption statistics */
export interface RedemptionStats {
  totalRedemptions: number;
  todayRedemptions: number;
  averageRedemptionTime: number;
  byChannel: Record<string, number>;
  dailyBreakdown: DailyStatistic[];
}

/** Export report request */
export interface ExportReportRequest {
  competitionId?: number;
  prizePoolId?: number;
  fromDate?: string;
  toDate?: string;
  format: 'csv' | 'excel';
  columns?: string[];
}

/** Audit log parameters */
export interface AuditLogParameters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  entityType?: string;
  action?: string;
  subjectId?: string;
  from?: string;
  to?: string;
}

/** Audit log entry */
export interface AuditLogEntry {
  auditLogId: number;
  entityType: string;
  entityId: string;
  action: string;
  subjectId?: string;
  subjectName?: string;
  timestamp: string;
  oldValues?: string;
  newValues?: string;
}
