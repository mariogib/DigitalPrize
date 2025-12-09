/**
 * Theme Context Definition
 */

import { createContext } from 'react';
import type { TenantTheme } from '../services/themeService';

export interface ThemeContextValue {
  theme: TenantTheme | null;
  isLoading: boolean;
  logoUrl: string | null;
  tenantName: string | null;
}

export const ThemeContext = createContext<ThemeContextValue>({
  theme: null,
  isLoading: true,
  logoUrl: null,
  tenantName: null,
});
