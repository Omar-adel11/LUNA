export function ValidateLoginForm(formData)
{
    const email = formData.get('email');
    const password = formData.get('password');

    const errors = {};

    const fields = [
        ['email', email, 200],
        ['password', password, 100]
    ];

    fields.forEach(([field,value,maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if(!text)
        {
            errors[field] = `${field} is required`;
        }else if(text.length > maxLength){
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

  
    if (
        !errors.email &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
        errors.email = 'Please enter a valid email address';
    }

    

    return errors;
    

}



export function ValidateRegisterForm(formData)
{
    const username = formData.get('username');
    const email = formData.get('email');
    const password = formData.get('password');
    const confirmPassword = formData.get('confirmPassword');
    const file = formData.get('image');

    const errors = {};

    const fields = [
        ['username',username,100],
        ['email', email, 200],
        ['password', password, 100],
        ['confirmPassword',confirmPassword,100]
    ];

    fields.forEach(([field,value,maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if(!text)
        {
            errors[field] = `${field} is required`;
        }else if(text.length > maxLength){
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });
    if (
        !errors.email &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
        errors.email = 'Please enter a valid email address';
    }

    ValidatePassword(errors,password);

    if (
        !errors.confirmPassword && !errors.password &&
        password !== confirmPassword
    ) {
        errors.confirmPassword = 'Passwords do not match';
    }

    if (!(file instanceof File) || !file.name) {
        errors.file = 'Image is required';
    } else if (!file.type.startsWith('image/')) {
        errors.file = 'File must be an image';
    } else if (file.size > 10 * 1024 * 1024) {
        errors.file = 'Image must be 10 MB or smaller';
    }

    
    return errors;
    

}

export function ValidateForgetPasswordForm(formData)
{
    const email = formData.get('email');
    

    const errors = {};

    const fields = [
        ['email', email, 200],
        
    ];

    fields.forEach(([field,value,maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if(!text)
        {
            errors[field] = `${field} is required`;
        }else if(text.length > maxLength){
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

  
    if (
        !errors.email &&
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
    ) {
        errors.email = 'Please enter a valid email address';
    }

    

    return errors;
    

}



export function ValidateChangePasswordForm(formData)
{
    const password = formData.get('password');
    const newPassword = formData.get('newPassword');
    

    const errors = {};
    const fields = [
        ['password', password, 100],
        ['newPassword', newPassword, 100]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    ValidatePassword(errors,newPassword);
    

    return errors;
}

export function ValidateOtpForm(formData)
{
    const errors = {};
    const fields = ['otp1', 'otp2', 'otp3', 'otp4', 'otp5', 'otp6'];

    fields.forEach((field) => {
        const value = formData.get(field);

        if (typeof value !== 'string' || !value.trim()) {
            errors[field] = `${field} is required`;
        } else if (!/^\d$/.test(value.trim())) {
            errors[field] = `${field} must be a single number from 0 to 9`;
        }
    });

    return errors;
}

export function ValidateResetPasswordForm(formData)
{
    const Password = formData.get('Password');
    const ConfirmPassword = formData.get('ConfirmPassword');

    const errors = {};
    const fields = [
        ['Password', Password, 100],
        ['ConfirmPassword', ConfirmPassword, 100]
    ];

    fields.forEach(([field, value, maxLength]) => {
        const text = typeof value === 'string' ? value.trim() : '';
        if (!text) {
            errors[field] = `${field} is required`;
        } else if (text.length > maxLength) {
            errors[field] = `${field} must be ${maxLength} characters or fewer`;
        }
    });

    ValidatePassword(errors,Password);
   

    return errors;
}

function ValidatePassword(errors, password) {
    if (!errors.password && password.trim().length < 8) {
        errors.password = 'Password must be at least 8 characters';
    }
    if (!errors.password && !/[A-Z]/.test(password)) {
        errors.password = 'Password must contain at least one capital letter';
    }
    if (!errors.password && !/\d/.test(password)) {
        errors.password = 'Password must contain at least one number';
    }
    if (!errors.password && !/[^A-Za-z0-9]/.test(password)) {
        errors.password = 'Password must contain at least one special character';
    }
}