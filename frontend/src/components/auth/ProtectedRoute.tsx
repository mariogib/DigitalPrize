/**
 * Protected Route Component
 * Wraps routes that require authentication
 */

import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth';
import './ProtectedRoute.css';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

/**
 * Protected Route Component
 * Redirects unauthenticated users to login
 * Optionally checks for required roles
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRoles }) => {
  const { isLoading, isSigningOut, isAuthenticated, hasAnyRole, signIn } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking authentication or signing out
  if (isLoading || isSigningOut) {
    return (
      <div className="protected-route-loading">
        <div className="protected-route-spinner" />
        <p className="protected-route-text">
          {isSigningOut ? 'Signing out...' : 'Checking authentication...'}
        </p>
      </div>
    );
  }

  // If not authenticated, redirect to sign in
  if (!isAuthenticated) {
    // Store the attempted URL in session storage for redirect after login
    sessionStorage.setItem('auth_redirect_url', location.pathname + location.search);

    // Trigger the sign in flow
    void signIn();

    return (
      <div className="protected-route-loading">
        <div className="protected-route-spinner" />
        <p className="protected-route-text">Redirecting to sign in...</p>
      </div>
    );
  }

  // Check for required roles if specified
  if (requiredRoles && requiredRoles.length > 0) {
    if (!hasAnyRole(requiredRoles)) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <>{children}</>;
};
