/**
 * Theme Hook
 * Provides access to the tenant theme context
 */

import { useContext } from 'react';
import { ThemeContext, type ThemeContextValue } from './themeContextDef';

export const useTheme = (): ThemeContextValue => {
  return useContext(ThemeContext);
};
