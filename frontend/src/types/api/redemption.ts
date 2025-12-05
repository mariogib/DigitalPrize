/**
 * Redemption related API types
 */

/** Redeemable prize response */
export interface RedeemablePrizeResponse {
  prizeAwardId: number;
  prizeName: string;
  prizeTypeName: string;
  monetaryValue?: number;
  competitionName: string;
  awardedAt: string;
  expiryDate?: string;
  daysUntilExpiry?: number;
  imageUrl?: string;
}

/** Initiate redemption request */
export interface InitiateRedemptionRequest {
  cellNumber: string;
}

/** Initiate redemption response */
export interface InitiateRedemptionResponse {
  success: boolean;
  message?: string;
  requiresVerification: boolean;
  redeemablePrizes: RedeemablePrizeResponse[];
}

/** Complete redemption request */
export interface CompleteRedemptionRequest {
  cellNumber: string;
  otpCode: string;
  prizeAwardId: number;
  redemptionChannel?: string;
}

/** Complete redemption response */
export interface CompleteRedemptionResponse {
  success: boolean;
  message?: string;
  redemptionCode?: string;
  prizeName?: string;
  prizeTypeName?: string;
  redeemedAt?: string;
}

/** Available prize DTO */
export interface AvailablePrizeDto {
  prizeId: number;
  name: string;
  description?: string;
  prizeTypeName: string;
  prizeTypeIconUrl?: string;
  value?: number;
  currency?: string;
  imageUrl?: string;
  availableQuantity: number;
}

/** Available prizes response */
export interface AvailablePrizesResponse {
  competitionId: number;
  competitionName: string;
  prizes: AvailablePrizeDto[];
}
