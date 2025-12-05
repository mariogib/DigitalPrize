/**
 * Competition-related API types
 */

import type { PrizePoolSummaryResponse } from './prizes';

/** Competition summary response */
export interface CompetitionResponse {
  competitionId: number;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  status: string;
  requiresRegistration?: boolean;
  createdAt: string;
  registrationCount: number;
  prizePoolCount: number;
  awardedPrizesCount: number;
}

/** Competition detail response with related data */
export interface CompetitionDetailResponse extends CompetitionResponse {
  registrationFields: RegistrationFieldResponse[];
  prizePools?: PrizePoolSummaryResponse[];
}

/** Registration field response */
export interface RegistrationFieldResponse {
  registrationFieldId: number;
  competitionId: number;
  fieldName: string;
  fieldType: string;
  isRequired: boolean;
  displayOrder: number;
  options?: string;
  validationRules?: string;
}

/** Create competition request */
export interface CreateCompetitionRequest {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  requiresRegistration: boolean;
  registrationFields?: CreateRegistrationFieldRequest[];
}

/** Update competition request */
export interface UpdateCompetitionRequest {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  status: string;
}

/** Create registration field request */
export interface CreateRegistrationFieldRequest {
  fieldName: string;
  fieldType: string;
  isRequired: boolean;
  displayOrder: number;
  validationRules?: string;
  options?: string;
}

/** Update registration field request */
export type UpdateRegistrationFieldRequest = CreateRegistrationFieldRequest;

/** Competition status constants */
export const CompetitionStatus = {
  Draft: 'Draft',
  Active: 'Active',
  Paused: 'Paused',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
} as const;

export type CompetitionStatusType = (typeof CompetitionStatus)[keyof typeof CompetitionStatus];
