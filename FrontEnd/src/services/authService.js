import { Post, baseUrl, postFormData,PostBearer, apiFetch } from "../api/apiClient.js";
import * as session from '../sessions/session.js';


const loginEndpoint = `${baseUrl}api/Authentication/login`;
export async function login(data) {
    return await Post(loginEndpoint,data)
}

const RegisterEndpoint = `${baseUrl}api/Authentication/signup`;
export async function register(data) {
    return await postFormData(RegisterEndpoint,data)
}

const changePasswordEndpoint = `${baseUrl}api/Authentication/change-password`;
export async function changePassword(data) {
    return await apiFetch('api/Authentication/change-password', {
        method: 'POST',
        body: JSON.stringify(data)
    });
}

const forgetPasswordEndpoint = `${baseUrl}api/Authentication/forget-password`;
export async function forgetPassword(data) {
    return await Post(forgetPasswordEndpoint,data)
}

const resetPasswordEndpoint = `${baseUrl}api/Authentication/reset-password`;
export async function resetPassword(data) {
    return await Post(resetPasswordEndpoint,data)
}

const checkOtpEndpoint = `${baseUrl}api/Authentication/check-otp`;

export async function checkOtp(data) {
    return await Post(checkOtpEndpoint, data);
}


export async function logout() {
    try {
        const refreshToken = session.getRefreshToken();
        
        await apiFetch('api/Authentication/logout', {
            method: 'POST',
            body: JSON.stringify({ refreshToken: refreshToken }) // Matches RefreshRequestDto
        });
    } catch (error) {
        console.error('Server logout failed, clearing local session anyway:', error);
    } finally {
        session.clearSession();
        window.location.href = 'login.html';
    }
}



