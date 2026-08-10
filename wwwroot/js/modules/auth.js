// Authentication Module
import { API } from './api.js';
import { UI } from './ui.js';

export class Auth {
    static init() {
        this.setupEventListeners();
        this.checkAuth();
    }

    static setupEventListeners() {
        document.querySelectorAll('[id="login-btn"]').forEach((btn) => {
            if (!btn.hasAttribute('onclick')) {
                btn.addEventListener('click', () => {
                    window.goToPage('auth');
                });
            }
        });

        document.querySelectorAll('[id="logout-btn"]').forEach((btn) => {
            if (!btn.hasAttribute('onclick')) {
                btn.addEventListener('click', () => {
                    this.logout();
                });
            }
        });

        const loginForm = document.querySelector('#login-tab form');
        if (loginForm && !loginForm.hasAttribute('onsubmit')) {
            loginForm.addEventListener('submit', (e) => this.handleLogin(e));
        }

        const registerForm = document.querySelector('#register-tab form');
        if (registerForm && !registerForm.hasAttribute('onsubmit')) {
            registerForm.addEventListener('submit', (e) => this.handleRegister(e));
        }
    }

    static async checkAuth() {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const user = await API.getCurrentUser();
                if (user && user.id) {
                    window.currentUser = user;
                    UI.updateAuthUI(user);
                    window.goToPage('dashboard');
                    return;
                }
            } catch (e) {
                localStorage.removeItem('token');
            }
        }

        UI.updateAuthUI(null);
        window.goToPage('auth');
    }

    static async handleLogin(e) {
        e.preventDefault();

        const email = document.getElementById('login-email').value;
        const password = document.getElementById('login-password').value;

        try {
            const data = await API.login(email, password);
            localStorage.setItem('token', data.token);
            window.currentUser = data.user;
            UI.updateAuthUI(data.user);

            document.getElementById('login-email').value = '';
            document.getElementById('login-password').value = '';

            UI.showAlert('Successful login', 'success');
            setTimeout(() => {
                window.goToPage('dashboard');
            }, 300);
        } catch (e) {
            UI.showAlert(e.message || 'Login error', 'danger');
        }
    }

    static async handleRegister(e) {
        e.preventDefault();

        const fullName = document.getElementById('register-fullname').value;
        const email = document.getElementById('register-email').value;
        const position = document.getElementById('register-position').value;
        const password = document.getElementById('register-password').value;

        try {
            await API.register(fullName, email, position, password);

            UI.showAlert('Registration successful. Please login.', 'success');
            document.getElementById('register-fullname').value = '';
            document.getElementById('register-email').value = '';
            document.getElementById('register-position').value = '';
            document.getElementById('register-password').value = '';

            setTimeout(() => {
                new bootstrap.Tab(document.querySelector('[data-bs-target="#login-tab"]')).show();
            }, 300);
        } catch (e) {
            UI.showAlert(e.message || 'Registration error', 'danger');
        }
    }

    static async logout() {
        try {
            await API.logout();
        } catch (e) {
            console.error('Logout error', e);
        }

        localStorage.removeItem('token');
        window.currentUser = null;
        UI.updateAuthUI(null);
        window.goToPage('auth');
        UI.showAlert('Logged out', 'success');
    }
}
