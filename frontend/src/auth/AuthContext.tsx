/**
 * Authentication Provider Component
 * Provides authentication state and methods to React components
 */

import type { User } from 'oidc-client-ts';
import { UserManager } from 'oidc-client-ts';
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  getUserRoles,
  hasAnyRole,
  hasRole,
  oidcConfig,
  parseDatabaseInfo,
  type UserClaims,
} from './authConfig';
import { AuthContext, type AuthContextValue } from './authContextDef';

export type { AuthContextValue };

/**
 * Authentication Provider Props
 */
interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication Provider
 * Wraps the application to provide authentication context
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [userManager] = useState(() => new UserManager(oidcConfig));
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSigningOut, setIsSigningOut] = useState(false);

  // Load user on mount
  useEffect(() => {
    const loadUser = async () => {
      try {
        const currentUser = await userManager.getUser();
        if (currentUser && !currentUser.expired) {
          setUser(currentUser);
        }
      } catch (error) {
        console.error('Failed to load user:', error);
      } finally {
        setIsLoading(false);
      }
    };

    void loadUser();
  }, [userManager]);

  // Set up event handlers
  useEffect(() => {
    const handleUserLoaded = (loadedUser: User) => {
      setUser(loadedUser);
    };

    const handleUserUnloaded = () => {
      setUser(null);
    };

    const handleSilentRenewError = (error: Error) => {
      console.error('Silent renew error:', error);
    };

    const handleAccessTokenExpired = () => {
      console.warn('Access token expired');
    };

    userManager.events.addUserLoaded(handleUserLoaded);
    userManager.events.addUserUnloaded(handleUserUnloaded);
    userManager.events.addSilentRenewError(handleSilentRenewError);
    userManager.events.addAccessTokenExpired(handleAccessTokenExpired);

    return () => {
      userManager.events.removeUserLoaded(handleUserLoaded);
      userManager.events.removeUserUnloaded(handleUserUnloaded);
      userManager.events.removeSilentRenewError(handleSilentRenewError);
      userManager.events.removeAccessTokenExpired(handleAccessTokenExpired);
    };
  }, [userManager]);

  // Sign in handler
  const signIn = useCallback(async () => {
    try {
      // Log the redirect URI for debugging
      console.log('Initiating sign in with config:', {
        redirect_uri: oidcConfig.redirect_uri,
        origin: window.location.origin,
        href: window.location.href,
      });
      // Use prompt: 'login' to force the auth server to show the login page
      // This prevents silent re-authentication after logout
      await userManager.signinRedirect({ prompt: 'login' });
    } catch (error) {
      console.error('Sign in error:', error);
      throw error;
    }
  }, [userManager]);

  // Sign out handler - clears local state and redirects to auth server logout
  const signOut = useCallback(async () => {
    console.log('signOut called - starting logout process');
    
    // Set signing out flag to prevent ProtectedRoute from triggering sign-in
    setIsSigningOut(true);
    
    try {
      // Get the id_token before clearing state (needed for logout hint)
      const currentUser = await userManager.getUser();
      const idToken = currentUser?.id_token;
      console.log('Got id_token for logout:', !!idToken);

      // Clear React state
      setUser(null);
      console.log('React state cleared');

      // Clear local auth state via userManager
      await userManager.removeUser();
      console.log('userManager.removeUser completed');

      // Explicitly clear the OIDC user key (authority from config)
      const authority = oidcConfig.authority?.replace(/\/$/, '') ?? '';
      const userKey = `oidc.user:${oidcConfig.authority}${oidcConfig.client_id}`;
      localStorage.removeItem(userKey);
      console.log('Removed userKey:', userKey);

      // Clear any remaining OIDC state from storage
      const keysToRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key && (key.startsWith('oidc.') || key.includes('DigitalPrize'))) {
          keysToRemove.push(key);
        }
      }
      console.log('localStorage keys to remove:', keysToRemove);
      keysToRemove.forEach(key => localStorage.removeItem(key));

      // Clear session storage as well - including auth_redirect_url
      const sessionKeysToRemove: string[] = [];
      for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        if (key && (key.startsWith('oidc.') || key.includes('DigitalPrize') || key === 'auth_redirect_url')) {
          sessionKeysToRemove.push(key);
        }
      }
      console.log('sessionStorage keys to remove:', sessionKeysToRemove);
      sessionKeysToRemove.forEach(key => sessionStorage.removeItem(key));

      // Build the auth server logout URL with id_token_hint first, then post_logout_redirect_uri
      const postLogoutUri = encodeURIComponent(window.location.origin + '/DigitalPrize');
      let logoutUrl = `${authority}/connect/logout?`;
      
      if (idToken) {
        logoutUrl += `id_token_hint=${idToken}&`;
      }
      logoutUrl += `post_logout_redirect_uri=${postLogoutUri}`;

      console.log('Redirecting to auth server logout:', logoutUrl);
      // Redirect to auth server to clear server-side session
      window.location.href = logoutUrl;
    } catch (error) {
      console.error('Sign out error:', error);
      // Even on error, try to logout via auth server
      const authority = oidcConfig.authority?.replace(/\/$/, '') ?? '';
      const postLogoutUri = encodeURIComponent(window.location.origin + '/DigitalPrize');
      window.location.href = `${authority}/connect/logout?post_logout_redirect_uri=${postLogoutUri}`;
    }
  }, [userManager]);

  // Refresh token handler
  const refreshToken = useCallback(async () => {
    try {
      const renewedUser = await userManager.signinSilent();
      return renewedUser;
    } catch (error) {
      console.error('Token refresh error:', error);
      return null;
    }
  }, [userManager]);

  // Computed values
  const claims = useMemo<UserClaims | null>(() => {
    if (!user?.profile) {
      return null;
    }
    return user.profile as unknown as UserClaims;
  }, [user]);

  const databaseInfo = useMemo(() => {
    return claims ? parseDatabaseInfo(claims.db_info) : null;
  }, [claims]);

  const roles = useMemo(() => {
    return claims ? getUserRoles(claims) : [];
  }, [claims]);

  const checkHasRole = useCallback(
    (role: string) => {
      return claims ? hasRole(claims, role) : false;
    },
    [claims]
  );

  const checkHasAnyRole = useCallback(
    (checkRoles: string[]) => {
      return claims ? hasAnyRole(claims, checkRoles) : false;
    },
    [claims]
  );

  const contextValue = useMemo<AuthContextValue>(
    () => ({
      isLoading,
      isSigningOut,
      isAuthenticated: !!user && !user.expired,
      user,
      claims,
      databaseInfo,
      roles,
      accessToken: user?.access_token ?? null,
      signIn,
      signOut,
      hasRole: checkHasRole,
      hasAnyRole: checkHasAnyRole,
      refreshToken,
    }),
    [
      isLoading,
      isSigningOut,
      user,
      claims,
      databaseInfo,
      roles,
      signIn,
      signOut,
      checkHasRole,
      checkHasAnyRole,
      refreshToken,
    ]
  );

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
};
