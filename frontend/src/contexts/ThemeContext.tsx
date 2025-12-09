/**
 * Theme Context Provider
 * Provides tenant theme to the application
 */

import { useEffect, useState, type ReactNode } from 'react';
import { initializeTheme, type TenantTheme } from '../services/themeService';
import { ThemeContext, type ThemeContextValue } from './themeContextDef';

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [theme, setTheme] = useState<TenantTheme | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadTheme = async () => {
      try {
        const fetchedTheme = await initializeTheme();
        setTheme(fetchedTheme);
      } catch (error) {
        console.error('Failed to load theme:', error);
      } finally {
        setIsLoading(false);
      }
    };

    void loadTheme();
  }, []);

  const value: ThemeContextValue = {
    theme,
    isLoading,
    logoUrl: theme?.logoUrl ?? null,
    tenantName: theme?.tenantName ?? null,
  };

  return (
    <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
  );
};
