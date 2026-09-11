import { checkOtp } from '../services/authService.js';
import { ValidateOtpForm } from '../validation/authValidation.js';

const otpForm = document.getElementById('otp-form');
const verifyBtn = document.querySelector('.submit-btn');
const formStatus = document.getElementById('form-status');

otpForm.addEventListener('submit', async (event) => {

    event.preventDefault();

    const formData = new FormData(otpForm);

    const errors = ValidateOtpForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const email = sessionStorage.getItem('email');

    if (!email) {
        formStatus.textContent = 'Reset session expired. Please request a new OTP.';
        return;
    }

    const otp = [
        formData.get('otp1'),
        formData.get('otp2'),
        formData.get('otp3'),
        formData.get('otp4'),
        formData.get('otp5'),
        formData.get('otp6')
    ].join('');

    const data = {
        email: email,
        otp: otp
    };

    verifyBtn.disabled = true;
    formStatus.textContent = 'Verifying OTP...';

    try {

        const resetToken = await checkOtp(data);

        console.log('RESET TOKEN:', resetToken);

        sessionStorage.setItem('resetToken', resetToken);

        window.location.href = 'resetpassword.html';

    } catch (error) {

        console.error('OTP ERROR:', error);

        formStatus.textContent = error.message;

        verifyBtn.disabled = false;
    }
});