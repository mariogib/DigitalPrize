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
  const [debugInfo, setDebugInfo] = useState<string>('');
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
        
        // Collect debug info
        const currentUrl = window.location.href;
        const sessionKeys = Object.keys(sessionStorage);
        const localKeys = Object.keys(localStorage);
        const debug = `URL: ${currentUrl}\nSession keys: ${sessionKeys.join(', ') || 'none'}\nLocal keys: ${localKeys.join(', ') || 'none'}`;
        setDebugInfo(debug);
        
        await userManager.signinRedirectCallback(currentUrl);

        // Redirect to the admin page after successful login
        navigate('/admin', { replace: true });
      } catch (err) {
        console.error('Auth callback error:', err);
        const errorMsg = err instanceof Error ? err.message : 'Authentication failed';
        const currentUrl = window.location.href;
        const sessionKeys = Object.keys(sessionStorage);
        const sessionData = sessionKeys.map(k => `${k}: ${sessionStorage.getItem(k)?.substring(0, 100)}...`).join('\n');
        setDebugInfo(`URL: ${currentUrl}\n\nSession Storage Keys: ${sessionKeys.join(', ') || 'none'}\n\nSession Data:\n${sessionData}`);
        setError(errorMsg);
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
          <pre style={{ 
            textAlign: 'left', 
            fontSize: '12px', 
            background: '#1a1a2e', 
            padding: '15px', 
            borderRadius: '8px',
            maxWidth: '600px',
            overflow: 'auto',
            whiteSpace: 'pre-wrap',
            wordBreak: 'break-all'
          }}>
            {debugInfo}
          </pre>
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
