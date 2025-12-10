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
    // Check for clear parameter - allows manual clearing via URL
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('clear') === 'true') {
      // Clear both old oidc. keys and new dp_ prefixed keys
      Object.keys(localStorage).forEach((key) => {
        if (key.startsWith('oidc.') || key.startsWith('dp_')) {
          localStorage.removeItem(key);
        }
      });
      window.location.href = window.location.origin + '/DigitalPrize/';
      return;
    }

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
        console.log('AuthCallback: Processing callback...');
        console.log('AuthCallback: Debug info:', debug);

        await userManager.signinRedirectCallback(currentUrl);

        // Redirect to the admin page after successful login
        navigate('/admin', { replace: true });
      } catch (err) {
        console.error('Auth callback error:', err);
        const errorMsg = err instanceof Error ? err.message : 'Authentication failed';
        console.log('AuthCallback: Error message:', errorMsg);

        // If authority mismatch, clear stale state and retry
        if (errorMsg.toLowerCase().includes('authority mismatch')) {
          console.log('AuthCallback: Authority mismatch detected, clearing stale auth state...');
          // Clear both old oidc. keys and new dp_ prefixed keys
          Object.keys(localStorage).forEach((key) => {
            if (key.startsWith('oidc.') || key.startsWith('dp_')) {
              console.log('AuthCallback: Removing localStorage key:', key);
              localStorage.removeItem(key);
            }
          });
          // Use window.location for hard redirect to ensure it works
          console.log('AuthCallback: Redirecting to home...');
          window.location.href = window.location.origin + '/DigitalPrize/';
          return;
        }

        const currentUrl = window.location.href;
        const localKeys = Object.keys(localStorage);
        const localData = localKeys
          .filter((k) => k.startsWith('oidc.') || k.startsWith('dp_'))
          .map((k) => `${k}: ${localStorage.getItem(k)?.substring(0, 100)}...`)
          .join('\n');
        setDebugInfo(`URL: ${currentUrl}\n\nLocalStorage OIDC Keys:\n${localData || 'none'}`);
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
          <pre
            style={{
              textAlign: 'left',
              fontSize: '12px',
              background: '#1a1a2e',
              padding: '15px',
              borderRadius: '8px',
              maxWidth: '600px',
              overflow: 'auto',
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-all',
            }}
          >
            {debugInfo}
          </pre>
          <button
            onClick={() => {
              // Clear ALL auth-related state from localStorage and sessionStorage
              // oidc-client-ts uses keys like "oidc.user:authority:clientid"
              const keysToRemove: string[] = [];
              for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && (key.startsWith('oidc.') || key.includes('DigitalPrize'))) {
                  keysToRemove.push(key);
                }
              }
              keysToRemove.forEach((key) => localStorage.removeItem(key));

              // Also clear sessionStorage
              const sessionKeysToRemove: string[] = [];
              for (let i = 0; i < sessionStorage.length; i++) {
                const key = sessionStorage.key(i);
                if (key && (key.startsWith('oidc.') || key.includes('DigitalPrize'))) {
                  sessionKeysToRemove.push(key);
                }
              }
              sessionKeysToRemove.forEach((key) => sessionStorage.removeItem(key));

              // Force full page reload to reset React state
              window.location.replace(window.location.origin + '/DigitalPrize/');
            }}
          >
            Return to Home
          </button>
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
