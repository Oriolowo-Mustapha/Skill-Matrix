document.addEventListener('DOMContentLoaded', function () {
    const passwordInput = document.getElementById('password-input');
    const togglePassword = document.querySelector('.toggle-password');

    if (passwordInput && togglePassword) {
        togglePassword.addEventListener('click', function () {
            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                togglePassword.textContent = '🙈';
            } else {
                passwordInput.type = 'password';
                togglePassword.textContent = '👁️';
            }
        });
    }
});
