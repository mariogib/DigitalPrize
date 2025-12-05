/**
 * Authentication Module Exports
 */

export { getUserRoles, hasAnyRole, hasRole, oidcConfig, parseDatabaseInfo } from './authConfig';
export type { DatabaseInfo, UserClaims } from './authConfig';
export { AuthProvider } from './AuthContext';
export type { AuthContextValue } from './authContextDef';
export { useAuth } from './useAuth';
export { getUserManager } from './userManager';
