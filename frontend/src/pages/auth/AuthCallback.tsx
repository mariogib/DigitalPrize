/**
 * Auth Callback Page
 * Handles the redirect callback from the OAuth2 server after login
 */

import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserManager } from '../../auth';
import './AuthCallback.css';

export const AuthCallback: React.FC = () => {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const processedRef = useRef(false);

  useEffect(() => {
    // Prevent duplicate processing in React StrictMode
    if (processedRef.current) {
      return;
    }
    processedRef.current = true;

    const handleCallback = async () => {
      try {
        const userManager = getUserManager();
        await userManager.signinRedirectCallback();

        // Redirect to the admin page after successful login
        navigate('/admin', { replace: true });
      } catch (err) {
        console.error('Auth callback error:', err);
        setError(err instanceof Error ? err.message : 'Authentication failed');
      }
    };

    void handleCallback();
  }, [navigate]);

  if (error) {
    return (
      <div className="auth-callback-container">
        <div className="auth-callback-error">
          <h2>Authentication Error</h2>
          <p>{error}</p>
          <button onClick={() => navigate('/', { replace: true })}>Return to Home</button>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-callback-container">
      <div className="auth-callback-loading">
        <div className="auth-callback-spinner" />
        <p className="auth-callback-text">Completing sign in...</p>
      </div>
    </div>
  );
};
