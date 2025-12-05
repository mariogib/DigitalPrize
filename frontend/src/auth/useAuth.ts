/**
 * useAuth Hook
 * Hook to access authentication context
 */

import { useContext } from 'react';
import { AuthContext, type AuthContextValue } from './authContextDef';

/**
 * Hook to access authentication context
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
