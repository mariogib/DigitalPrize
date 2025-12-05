import { getUserManager } from '../../auth';

const API_BASE_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? '';

interface RequestOptions {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  headers?: Record<string, string>;
  skipAuth?: boolean;
}

class ApiError extends Error {
  constructor(
    message: string,
    public status: number
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/**
 * Get the current access token from the user manager
 */
async function getAccessToken(): Promise<string | null> {
  try {
    const userManager = getUserManager();
    const user = await userManager.getUser();
    if (user && !user.expired) {
      return user.access_token;
    }
    return null;
  } catch {
    return null;
  }
}

export const apiClient = {
  async request<T>(endpoint: string, options: RequestOptions): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    // Add authorization header if not skipping auth
    if (!options.skipAuth) {
      const token = await getAccessToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const fetchOptions: RequestInit = {
      method: options.method,
      headers,
    };

    if (options.body) {
      fetchOptions.body = JSON.stringify(options.body);
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, fetchOptions);

    // Handle 401 Unauthorized - token might be expired
    if (response.status === 401 && !options.skipAuth) {
      // Try to refresh the token
      try {
        const userManager = getUserManager();
        const user = await userManager.signinSilent();
        if (user?.access_token) {
          // Retry the request with the new token
          headers['Authorization'] = `Bearer ${user.access_token}`;
          const retryResponse = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...fetchOptions,
            headers,
          });

          if (!retryResponse.ok) {
            throw new ApiError(`API Error: ${retryResponse.statusText}`, retryResponse.status);
          }

          if (retryResponse.status === 204) {
            return undefined as T;
          }

          return retryResponse.json() as Promise<T>;
        }
      } catch {
        // Token refresh failed, throw the original error
      }
      throw new ApiError('Unauthorized - please sign in again', 401);
    }

    if (!response.ok) {
      throw new ApiError(`API Error: ${response.statusText}`, response.status);
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return undefined as T;
    }

    return response.json() as Promise<T>;
  },

  get<T>(endpoint: string, skipAuth = false): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', skipAuth });
  },

  post<T>(endpoint: string, body: unknown, skipAuth = false): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, skipAuth });
  },

  put<T>(endpoint: string, body: unknown, skipAuth = false): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, skipAuth });
  },

  delete<T>(endpoint: string, skipAuth = false): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', skipAuth });
  },
};

export { ApiError };
