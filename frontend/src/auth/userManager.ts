/**
 * UserManager utilities
 * Provides direct access to the UserManager instance
 */

import { UserManager } from 'oidc-client-ts';
import { oidcConfig } from './authConfig';

/**
 * Get the UserManager instance for direct access
 * Used by callback pages
 */
export function getUserManager(): UserManager {
  return new UserManager(oidcConfig);
}
