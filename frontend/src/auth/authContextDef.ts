/**
 * Authentication Context Definition
 * Separate file for React context to support fast refresh
 */

import type { User } from 'oidc-client-ts';
import { createContext } from 'react';
import type { DatabaseInfo, UserClaims } from './authConfig';

/**
 * Authentication state and methods
 */
export interface AuthContextValue {
  /** Whether authentication is still being initialized */
  isLoading: boolean;
  /** Whether the user is currently signing out */
  isSigningOut: boolean;
  /** Whether the user is authenticated */
  isAuthenticated: boolean;
  /** The authenticated user, if any */
  user: User | null;
  /** User claims from the token */
  claims: UserClaims | null;
  /** Database info parsed from the db_info claim */
  databaseInfo: DatabaseInfo | null;
  /** User roles as an array */
  roles: string[];
  /** Access token for API calls */
  accessToken: string | null;
  /** Sign in the user */
  signIn: () => Promise<void>;
  /** Sign out the user */
  signOut: () => Promise<void>;
  /** Check if user has a specific role */
  hasRole: (role: string) => boolean;
  /** Check if user has any of the specified roles */
  hasAnyRole: (roles: string[]) => boolean;
  /** Refresh the access token silently */
  refreshToken: () => Promise<User | null>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
