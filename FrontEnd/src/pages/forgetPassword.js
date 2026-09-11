import { forgetPassword } from '../services/authService.js'
import { ValidateForgetPasswordForm } from '../validation/authValidation.js'

const forgePasswordForm = document.getElementById('forget-password-form');
const sendOtpBtn = document.querySelector('.submit-btn');
const formStatus = document.querySelector('.form-status');


forgePasswordForm.addEventListener('submit', async (event)=>{
    event.preventDefault();

    const formData = new FormData(forgePasswordForm);

    var errors = ValidateForgetPasswordForm(formData);

    if(Object.keys(errors).length > 0)
    {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const data =  formData.get('email');
    

     sendOtpBtn.disabled = true;

     try{
            sessionStorage.setItem('email',data);
            const result = await forgetPassword(data);
            formStatus.textContent = result;
            window.location.href = 'otp.html';
         }catch(error){
             console.error(error);
             formStatus.textContent = error.message;
             sendOtpBtn.disabled = false;
         }
   


});