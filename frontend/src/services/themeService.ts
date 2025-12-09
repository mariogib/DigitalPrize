/**
 * Theme Service
 * Fetches and applies tenant-specific styles from the auth server
 */

/**
 * TenantTheme interface matching the auth server API response
 */
export interface TenantTheme {
  tenantId?: string | null;
  tenantName?: string | null;
  primaryColor?: string;
  secondaryColor?: string;
  successColor?: string;
  dangerColor?: string;
  warningColor?: string;
  bgColor?: string;
  cardBgColor?: string;
  textColor?: string;
  textMutedColor?: string;
  borderColor?: string;
  shadowColor?: string;
  logoUrl?: string | null;
  faviconUrl?: string | null;
  companyName?: string | null;
  isDefault?: boolean;
}

const AUTH_SERVER_URL = 'https://worldplayauth.ngrok.app';

/**
 * Fetch theme configuration from the auth server based on the current URL
 */
export const fetchTenantTheme = async (): Promise<TenantTheme | null> => {
  try {
    const currentUrl = window.location.href;
    const themeUrl = import.meta.env.DEV
      ? `/auth-api/Theme/by-url?url=${encodeURIComponent(currentUrl)}`
      : `${AUTH_SERVER_URL}/api/Theme/by-url?url=${encodeURIComponent(currentUrl)}`;

    const response = await fetch(themeUrl);
    
    if (!response.ok) {
      console.warn('Failed to fetch tenant theme:', response.status);
      return null;
    }

    const theme = (await response.json()) as TenantTheme;
    return theme;
  } catch (error) {
    console.error('Error fetching tenant theme:', error);
    return null;
  }
};

/**
 * Apply theme CSS variables to the document root
 * Maps auth server theme properties to CSS variables
 */
export const applyTheme = (theme: TenantTheme): void => {
  const root = document.documentElement;

  // Primary color - used for buttons, links, accents
  if (theme.primaryColor) {
    root.style.setProperty('--color-primary', theme.primaryColor);
    root.style.setProperty('--color-primary-rgb', hexToRgb(theme.primaryColor));
    // Also set as button background
    root.style.setProperty('--button-background', theme.primaryColor);
  }

  // Secondary color
  if (theme.secondaryColor) {
    root.style.setProperty('--color-secondary', theme.secondaryColor);
    root.style.setProperty('--color-primary-dark', theme.secondaryColor);
    root.style.setProperty('--button-hover-background', theme.secondaryColor);
  }

  // Background color
  if (theme.bgColor) {
    root.style.setProperty('--color-background', theme.bgColor);
    root.style.setProperty('--color-dark', theme.bgColor);
    root.style.setProperty('--header-background', theme.bgColor);
    root.style.setProperty('--footer-background', theme.bgColor);
  }

  // Card background color
  if (theme.cardBgColor) {
    root.style.setProperty('--color-dark-card', theme.cardBgColor);
    root.style.setProperty('--color-dark-lighter', theme.cardBgColor);
  }

  // Text color
  if (theme.textColor) {
    root.style.setProperty('--color-text', theme.textColor);
    root.style.setProperty('--text-primary', theme.textColor);
    root.style.setProperty('--header-text-color', theme.textColor);
    root.style.setProperty('--footer-text-color', theme.textColor);
    root.style.setProperty('--button-text-color', theme.textColor);
  }

  // Muted text color
  if (theme.textMutedColor) {
    root.style.setProperty('--text-secondary', theme.textMutedColor);
    root.style.setProperty('--text-muted', theme.textMutedColor);
  }

  // Border color
  if (theme.borderColor) {
    root.style.setProperty('--color-dark-border', theme.borderColor);
    root.style.setProperty('--border', theme.borderColor);
  }

  // Shadow color
  if (theme.shadowColor) {
    root.style.setProperty('--shadow-color', theme.shadowColor);
  }

  // Status colors
  if (theme.successColor) {
    root.style.setProperty('--color-success', theme.successColor);
  }
  if (theme.dangerColor) {
    root.style.setProperty('--color-error', theme.dangerColor);
  }
  if (theme.warningColor) {
    root.style.setProperty('--color-warning', theme.warningColor);
  }

  // Update favicon if provided
  if (theme.faviconUrl) {
    updateFavicon(theme.faviconUrl);
  }

  // Update document title if company name or tenant name provided
  const displayName = theme.companyName || theme.tenantName;
  if (displayName) {
    document.title = `${displayName} - Digital Prizes`;
  }
};

/**
 * Convert hex color to RGB values
 */
const hexToRgb = (hex: string): string => {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  if (result && result[1] && result[2] && result[3]) {
    const r = parseInt(result[1], 16);
    const g = parseInt(result[2], 16);
    const b = parseInt(result[3], 16);
    return `${String(r)}, ${String(g)}, ${String(b)}`;
  }
  return '0, 180, 216'; // Default teal
};

/**
 * Update the favicon dynamically
 */
const updateFavicon = (faviconUrl: string): void => {
  let link = document.querySelector<HTMLLinkElement>("link[rel~='icon']");
  if (!link) {
    link = document.createElement('link');
    link.rel = 'icon';
    document.head.appendChild(link);
  }
  link.href = faviconUrl;
};

/**
 * Initialize theme - fetch and apply
 */
export const initializeTheme = async (): Promise<TenantTheme | null> => {
  const theme = await fetchTenantTheme();
  if (theme) {
    applyTheme(theme);
  }
  return theme;
};

/**
 * Theme service object for convenient imports
 */
export const themeService = {
  fetchTenantTheme,
  applyTheme,
  initializeTheme,
};
