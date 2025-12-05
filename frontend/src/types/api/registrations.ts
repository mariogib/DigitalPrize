/**
 * Registration related API types
 */

/** Registration response */
export interface RegistrationResponse {
  registrationId: number;
  competitionId: number;
  competitionName: string;
  cellNumber: string;
  registeredAt: string;
  isVerified: boolean;
  externalUserId?: number;
  externalUserName?: string;
  answers: RegistrationAnswerResponse[];
}

/** Registration answer response */
export interface RegistrationAnswerResponse {
  registrationFieldId: number;
  fieldName: string;
  value?: string;
}

/** Public registration request */
export interface PublicRegistrationRequest {
  competitionId: number;
  cellNumber: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  answers?: RegistrationAnswerRequest[];
}

/** Registration answer request */
export interface RegistrationAnswerRequest {
  registrationFieldId: number;
  value: string;
}

/** Public registration response */
export interface PublicRegistrationResponse {
  success: boolean;
  requiresVerification: boolean;
  message?: string;
  registrationId?: number;
}

/** Verify OTP request */
export interface VerifyOtpRequest {
  cellNumber: string;
  code: string;
  competitionId: number;
}

/** Admin registration request */
export interface AdminRegistrationRequest {
  cellNumber: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  answers?: RegistrationAnswerRequest[];
}

/** Bulk registration result */
export interface BulkRegistrationResult {
  totalRequested: number;
  successfulRegistrations: number;
  failedRegistrations: number;
  duplicateRegistrations: number;
  results: BulkRegistrationItemResult[];
}

/** Bulk registration item result */
export interface BulkRegistrationItemResult {
  cellNumber: string;
  success: boolean;
  registrationId?: number;
  errorMessage?: string;
}
