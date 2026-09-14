export const baseUrl = 'https://localhost:7054/';
import * as session from '../sessions/session.js';
export async function Post(url, data) {

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    if (!response.ok) {

        const error = await response.json();

        console.log('Server error body:', error);

        throw new Error(
            error.ErrorMessage || 'An error occurred'
        );
    }

    const contentType = response.headers.get('content-type');

    if (contentType && contentType.includes('application/json')) {
        return response.json();
    }

    return response.text();
}

export async function PostBearer(url, data,token) {
    const response = await fetch(url,{
        method : 'POST',
        headers:{
            'Content-Type' : 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
    });

    if(!response.ok)
    {
        const error = await response.json();
        console.log('Server error body:', error); 
        throw new Error(error.ErrorMessage || 'An error occured');
    }

    return response.json();
}

export async function postFormData(url, formData) {

    const response = await fetch(url, {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        const error = await response.json();
        console.log('Server error body:', error); 
        throw new Error(
            error.ErrorMessage || 'An error occurred'
        );
    }

    return response.json();
}


let refreshPromise = null;

async function refreshAccessToken() {
    if (refreshPromise) {
        return refreshPromise;
    }

    refreshPromise = (async () => {
        const refreshToken = session.getRefreshToken();
        if (!refreshToken) {
            throw new Error('No refresh token available');
        }

        const response = await fetch(`${baseUrl}api/Authentication/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
        });

        if (!response.ok) {
            throw new Error('Refresh failed');
        }

        const result = await response.json(); // Expected: { email, name, token, imgUrl, refreshToken }
        session.setSession(result); // Rotates/updates both tokens in session storage
        return result.token;
    })();

    try {
        return await refreshPromise;
    } finally {
        refreshPromise = null; // Reset so future expirations can trigger a new refresh
    }
}

/**
 * Use this wrapper for any call to a protected endpoint.
 * - Automatically attaches the current access token.
 * - Handles 401 Unauthorized by trying a silent token refresh, then retrying the request.
 * - Redirects to login if the refresh fails.
 */
export async function apiFetch(path, options = {}) {
    const buildRequest = (accessToken) => ({
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
            ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {})
        }
    });

    // 1. Try initial request with current access token
    let response = await fetch(`${baseUrl}${path}`, buildRequest(session.getAccessToken()));

    // 2. If unauthorized and we have a refresh token, attempt silent refresh
    if (response.status === 401 && session.getRefreshToken()) {
        try {
            const newToken = await refreshAccessToken();
            // Retry original request with the new token
            response = await fetch(`${baseUrl}${path}`, buildRequest(newToken));
        } catch (err) {
            session.clearSession();
            window.location.href = 'login.html';
            throw err;
        }
    }

    if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw new Error(errorBody.ErrorMessage || errorBody.message || `Request failed (${response.status})`);
    }

    if (response.status === 204) {
        return null; // Handle empty responses (e.g., successful logout or updates)
    }

    return response.json();
}
