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
      await userManager.signinRedirect();
    } catch (error) {
      console.error('Sign in error:', error);
      throw error;
    }
  }, [userManager]);

  // Sign out handler - uses direct URL to avoid CORS issues with OIDC discovery
  const signOut = useCallback(async () => {
    try {
      const currentUser = await userManager.getUser();
      const idToken = currentUser?.id_token;

      // Clear local auth state
      await userManager.removeUser();

      // Build direct logout URL
      const authority = 'https://worldplayauth.ngrok.app';
      const postLogoutUri = encodeURIComponent(window.location.origin + '/DigitalPrize/');
      let logoutUrl = `${authority}/connect/logout?post_logout_redirect_uri=${postLogoutUri}`;

      if (idToken) {
        logoutUrl += `&id_token_hint=${idToken}`;
      }

      // Redirect to logout endpoint
      window.location.href = logoutUrl;
    } catch (error) {
      console.error('Sign out error:', error);
      throw error;
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
