/**
 * Silent Callback Page
 * Handles silent token refresh in an iframe
 */

import { useEffect } from 'react';
import { getUserManager } from '../../auth';

export const SilentCallback: React.FC = () => {
  useEffect(() => {
    const handleSilentCallback = async () => {
      try {
        const userManager = getUserManager();
        await userManager.signinSilentCallback();
      } catch (err) {
        console.error('Silent callback error:', err);
      }
    };

    void handleSilentCallback();
  }, []);

  return null;
};
