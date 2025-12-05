/**
 * Prize-related API types
 */

/** Prize type response */
export interface PrizeTypeResponse {
  prizeTypeId: number;
  name: string;
  description?: string;
  iconUrl?: string;
  isActive: boolean;
}

/** Prize summary response */
export interface PrizeSummaryResponse {
  prizeId: number;
  prizePoolId: number;
  prizePoolName: string;
  prizeTypeId: number;
  prizeTypeName: string;
  name: string;
  monetaryValue?: number;
  totalQuantity: number;
  remainingQuantity: number;
  awardedQuantity: number;
  redeemedQuantity: number;
  isActive: boolean;
  expiryDate?: string;
}

/** Prize detail response */
export interface PrizeDetailResponse extends PrizeSummaryResponse {
  description?: string;
  imageUrl?: string;
  metadataJson?: string;
  createdAt: string;
}

/** Prize pool summary response */
export interface PrizePoolSummaryResponse {
  prizePoolId: number;
  name: string;
  description?: string;
  competitionId?: number;
  competitionName?: string;
  isActive: boolean;
  totalPrizes: number;
  availablePrizes: number;
  awardedPrizes: number;
  redeemedPrizes: number;
}

/** Prize pool detail response */
export interface PrizePoolDetailResponse extends PrizePoolSummaryResponse {
  createdAt: string;
  prizes: PrizeSummaryResponse[];
}

/** Create prize request */
export interface CreatePrizeRequest {
  prizePoolId: number;
  prizeTypeId: number;
  name: string;
  description?: string;
  monetaryValue?: number;
  totalQuantity: number;
  imageUrl?: string;
  expiryDate?: string;
  metadataJson?: string;
}

/** Update prize request */
export interface UpdatePrizeRequest {
  prizeTypeId: number;
  name: string;
  description?: string;
  monetaryValue?: number;
  totalQuantity: number;
  imageUrl?: string;
  expiryDate?: string;
  metadataJson?: string;
  isActive: boolean;
}

/** Bulk create prizes request */
export interface BulkCreatePrizesRequest {
  prizePoolId: number;
  prizeTypeId: number;
  namePrefix: string;
  description?: string;
  monetaryValue?: number;
  quantity: number;
  expiryDate?: string;
}

/** Create prize pool request */
export interface CreatePrizePoolRequest {
  competitionId?: number;
  name: string;
  description?: string;
}

/** Update prize pool request */
export interface UpdatePrizePoolRequest {
  name: string;
  description?: string;
  isActive: boolean;
}
