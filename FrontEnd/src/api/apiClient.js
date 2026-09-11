export const baseUrl = 'https://localhost:7054/';

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
