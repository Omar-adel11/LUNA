import { Post, baseUrl, postFormData,PostBearer } from "../api/apiClient.js";

const loginEndpoint = `${baseUrl}api/Authentication/login`;
export async function login(data) {
    return await Post(loginEndpoint,data)
}

const RegisterEndpoint = `${baseUrl}api/Authentication/signup`;
export async function register(data) {
    return await postFormData(RegisterEndpoint,data)
}

const changePasswordEndpoint = `${baseUrl}api/Authentication/change-password`;
export async function changePassword(data,token) {
    return await PostBearer(changePasswordEndpoint,data,token)
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



