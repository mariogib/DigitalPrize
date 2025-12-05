/**
 * Prize award related API types
 */

/** Prize award response */
export interface PrizeAwardResponse {
  prizeAwardId: number;
  prizeId: number;
  prizeName: string;
  prizeTypeName: string;
  competitionId: number;
  competitionName: string;
  cellNumber: string;
  status: string;
  awardedAt: string;
  expiryDate?: string;
  isRedeemable: boolean;
  monetaryValue?: number;
}

/** Prize award detail response */
export interface PrizeAwardDetailResponse extends PrizeAwardResponse {
  externalUserId?: number;
  externalUserName?: string;
  redemption?: PrizeRedemptionResponse;
  prizeImageUrl?: string;
  prizeDescription?: string;
}

/** Prize redemption response */
export interface PrizeRedemptionResponse {
  prizeRedemptionId: number;
  prizeAwardId: number;
  redeemedAt: string;
  redemptionChannel?: string;
  redemptionCode?: string;
  redeemedBy?: string;
}

/** Award prize request */
export interface AwardPrizeRequest {
  prizeId: number;
  cellNumber: string;
  competitionId: number;
  externalUserId?: number;
  sendNotification?: boolean;
}

/** Bulk award prizes request */
export interface BulkAwardPrizesRequest {
  prizeId: number;
  competitionId: number;
  cellNumbers: string[];
  sendNotifications?: boolean;
}

/** Bulk award response */
export interface BulkAwardResponse {
  totalRequested: number;
  successfulAwards: number;
  failedAwards: number;
  results: BulkAwardItemResult[];
}

/** Bulk award item result */
export interface BulkAwardItemResult {
  cellNumber: string;
  success: boolean;
  prizeAwardId?: number;
  errorMessage?: string;
}

/** Cancel award request */
export interface CancelAwardRequest {
  reason: string;
}

/** Awards report parameters */
export interface AwardsReportParameters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  competitionId?: number;
  prizePoolId?: number;
  prizeTypeId?: number;
  status?: string;
  awardedFrom?: string;
  awardedTo?: string;
}

/** Award status constants */
export const AwardStatus = {
  Awarded: 'Awarded',
  Redeemed: 'Redeemed',
  Expired: 'Expired',
  Cancelled: 'Cancelled',
} as const;

export type AwardStatusType = (typeof AwardStatus)[keyof typeof AwardStatus];
