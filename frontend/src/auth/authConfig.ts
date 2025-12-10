/**
 * OIDC Authentication Configuration
 * Configures the oidc-client-ts UserManager for WorldPlay authentication
 */

import type { UserManagerSettings } from 'oidc-client-ts';
import { WebStorageStateStore } from 'oidc-client-ts';

/**
 * Custom storage wrapper that prefixes all keys with 'dp_' to isolate
 * DigitalPrize's OIDC state from other apps on the same domain
 */
class PrefixedStorage implements Storage {
  private prefix = 'dp_';
  private storage: Storage;

  constructor(storage: Storage) {
    this.storage = storage;
  }

  get length(): number {
    return Object.keys(this.storage).filter((key) => key.startsWith(this.prefix)).length;
  }

  key(index: number): string | null {
    const keys = Object.keys(this.storage).filter((key) => key.startsWith(this.prefix));
    return keys[index]?.substring(this.prefix.length) ?? null;
  }

  getItem(key: string): string | null {
    return this.storage.getItem(this.prefix + key);
  }

  setItem(key: string, value: string): void {
    this.storage.setItem(this.prefix + key, value);
  }

  removeItem(key: string): void {
    this.storage.removeItem(this.prefix + key);
  }

  clear(): void {
    Object.keys(this.storage)
      .filter((key) => key.startsWith(this.prefix))
      .forEach((key) => this.storage.removeItem(key));
  }
}

/**
 * OAuth2/OIDC Configuration for WorldPlay Auth Server
 */

// Get the base URL - use Vite's BASE_URL which is set from vite.config.ts
const getBaseUrl = (): string => {
  if (typeof window !== 'undefined') {
    // Use Vite's BASE_URL environment variable (e.g., '/DigitalPrize/')
    // This is set in vite.config.ts and ensures consistency
    const viteBase = import.meta.env.BASE_URL || '/';
    // Remove trailing slash if present for URL building
    const basePath = viteBase.endsWith('/') ? viteBase.slice(0, -1) : viteBase;
    const fullUrl = window.location.origin + basePath;
    console.log('[authConfig] getBaseUrl:', {
      viteBase,
      basePath,
      origin: window.location.origin,
      fullUrl,
    });
    return fullUrl;
  }
  return 'http://localhost:3000';
};

const baseUrl = getBaseUrl();
console.log('[authConfig] Configured redirect_uri:', `${baseUrl}/auth/callback`);

// Create prefixed storage instances to isolate from other apps on same domain
const prefixedLocalStorage = new PrefixedStorage(window.localStorage);

export const oidcConfig: UserManagerSettings = {
  authority: 'https://worldplayauth.ngrok.app/',
  client_id: 'DigitalPrizeApplication',
  redirect_uri: `${baseUrl}/auth/callback`,
  post_logout_redirect_uri: `${baseUrl}/`,
  silent_redirect_uri: `${baseUrl}/auth/silent-callback`,

  // Response type for authorization code flow with PKCE
  response_type: 'code',

  // Scopes to request (must match scopes registered for this client on the OpenIddict server)
  scope: 'openid profile email offline_access api.read api.write roles',

  // Token storage - use prefixed localStorage to isolate from other apps
  userStore: new WebStorageStateStore({ store: prefixedLocalStorage }),
  stateStore: new WebStorageStateStore({ store: prefixedLocalStorage }),

  // Don't load user info - the OpenIddict server doesn't expose a userinfo endpoint
  // Claims are included in the ID token instead
  loadUserInfo: false,

  // Enable automatic silent token refresh
  automaticSilentRenew: true,

  // Monitor session state
  monitorSession: true,

  // Include ID token claims in the profile
  filterProtocolClaims: false,

  // Provide explicit metadata to avoid CORS issues with discovery endpoint
  // This bypasses the need to fetch /.well-known/openid-configuration
  metadata: {
    issuer: 'https://worldplayauth.ngrok.app/',
    authorization_endpoint: 'https://worldplayauth.ngrok.app/connect/authorize',
    token_endpoint: 'https://worldplayauth.ngrok.app/connect/token',
    end_session_endpoint: 'https://worldplayauth.ngrok.app/connect/logout',
    jwks_uri: 'https://worldplayauth.ngrok.app/.well-known/jwks',
    userinfo_endpoint: 'https://worldplayauth.ngrok.app/connect/userinfo',
  },
};

/**
 * Database info from JWT token
 */
export interface DatabaseInfo {
  Provider: string;
  Server: string;
  Database: string;
  Port?: number;
  AdditionalOptions?: string;
}

/**
 * User claims from the JWT token
 */
export interface UserClaims {
  sub: string;
  tid?: string;
  org?: string;
  name?: string;
  email?: string;
  email_verified?: boolean;
  is_cross_tenant_admin?: boolean;
  original_tid?: string;
  role?: string | string[];
  db_info?: string;
  client_id?: string;
}

/**
 * Parse db_info claim from JWT
 */
export function parseDatabaseInfo(dbInfoClaim: string | undefined): DatabaseInfo | null {
  if (!dbInfoClaim) {
    return null;
  }

  try {
    return JSON.parse(dbInfoClaim) as DatabaseInfo;
  } catch {
    console.error('Failed to parse db_info claim:', dbInfoClaim);
    return null;
  }
}

/**
 * Get user roles as an array
 */
export function getUserRoles(claims: UserClaims): string[] {
  if (!claims.role) {
    return [];
  }

  if (Array.isArray(claims.role)) {
    return claims.role;
  }

  return [claims.role];
}

/**
 * Check if user has a specific role
 */
export function hasRole(claims: UserClaims, role: string): boolean {
  return getUserRoles(claims).includes(role);
}

/**
 * Check if user has any of the specified roles
 */
export function hasAnyRole(claims: UserClaims, roles: string[]): boolean {
  const userRoles = getUserRoles(claims);
  return roles.some((role) => userRoles.includes(role));
}
