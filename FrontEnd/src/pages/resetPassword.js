import { resetPassword } from '../services/authService.js';
import { ValidateResetPasswordForm } from '../validation/authValidation.js';

const resetPasswordForm = document.getElementById('reset-password-form');
const resetBtn = document.querySelector('.submit-btn');
const formStatus = document.getElementById('form-status');

resetPasswordForm.addEventListener('submit', async (event) => {

    event.preventDefault();

    const formData = new FormData(resetPasswordForm);

    const errors = ValidateResetPasswordForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const email = sessionStorage.getItem('email');
    const resetToken = sessionStorage.getItem('resetToken');

    if (!email || !resetToken) {
        formStatus.textContent =
            'Reset session expired. Please request a new OTP.';
        return;
    }

    const password = formData.get('Password');
    const ConfirmPassword = formData.get('ConfirmPassword');

    if (password !== ConfirmPassword) {
        formStatus.textContent = 'Passwords do not match.';
        return;
    }

    const data = {
        email: email,
        resetToken: resetToken,
        password: password,
        ConfirmPassword : ConfirmPassword
    };

    resetBtn.disabled = true;
    formStatus.textContent = 'Resetting password...';

    try {

        const result = await resetPassword(data);

        console.log('RESET PASSWORD RESULT:', result);

        formStatus.textContent = result;

        // Reset flow is finished
        sessionStorage.removeItem('email');
        sessionStorage.removeItem('resetToken');

        setTimeout(() => {
            window.location.href = 'login.html';
        }, 1500);

    } catch (error) {

        console.error('RESET PASSWORD ERROR:', error);

        formStatus.textContent = error.message;

        resetBtn.disabled = false;
    }
});