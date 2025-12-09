/**
 * Theme Context Provider
 * Provides tenant theme to the application with loading overlay
 */

import { useEffect, useState, type ReactNode } from 'react';
import { initializeTheme, type TenantTheme } from '../services/themeService';
import { ThemeContext, type ThemeContextValue } from './themeContextDef';
import styles from './ThemeContext.module.css';

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
        // Small delay to ensure CSS variables are applied
        setTimeout(() => setIsLoading(false), 100);
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
    <ThemeContext.Provider value={value}>
      {isLoading && (
        <div className={styles.loadingOverlay}>
          <div className={styles.loadingContent}>
            <div className={styles.spinner} />
            <p className={styles.loadingText}>Loading...</p>
          </div>
        </div>
      )}
      <div className={isLoading ? styles.hidden : styles.visible}>
        {children}
      </div>
    </ThemeContext.Provider>
  );
};
