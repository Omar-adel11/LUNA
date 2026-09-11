import * as authService from '../services/authService.js';
import * as authValidation from '../validation/authValidation.js';


const form = document.getElementById('register-form');
const submitBtn = document.querySelector('.submit-btn');
const formStatus = document.getElementById('form-status');

form.addEventListener('submit', async(event) => {
    event.preventDefault();
    const formData = new FormData(form);
    const errors = authValidation.ValidateRegisterForm(formData);

    if(Object.keys(errors).length > 0)
    {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

   

    submitBtn.disabled = true;
    formStatus.textContent = 'registering ...';

    try{
        const result = await authService.register(formData);
        console.log(result);
        sessionStorage.setItem('name',result.name);
        sessionStorage.setItem('email',result.email);
        sessionStorage.setItem('imgUrl',result.imgUrl);
        sessionStorage.setItem('token', result.token);
        window.location.href = 'home.html';
    }catch(error){
        console.error(error);
        formStatus.textContent = error.message;
        submitBtn.disabled = false;
    }
    
});