// UI Core Module
export class UI {
    static showAlert(message, type = 'info') {
        const container = document.getElementById('alerts-container');
        if (!container) return;

        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-notification alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        container.appendChild(alert);
        setTimeout(() => alert.remove(), 4000);
    }

    static showView(viewName) {
        const pageTitle = document.getElementById('page-title');
        const titles = {
            dashboard: 'Dashboard',
            experiments: 'Experiments',
            entries: 'Journal Entries',
            results: 'Results',
            auth: 'Laboratory Journal Pro'
        };

        if (pageTitle) {
            pageTitle.textContent = titles[viewName] || 'Laboratory Journal Pro';
        }

        document.querySelectorAll('.view-container').forEach((v) => {
            v.style.display = 'none';
        });

        const view = document.getElementById(`${viewName}-view`);
        if (view) {
            view.style.display = 'block';
        }
    }

    static updateAuthUI(user) {
        const authNav = document.getElementById('auth-nav');
        const userPanel = document.getElementById('user-panel');
        const userName = document.getElementById('user-name');
        const userEmail = document.getElementById('user-email');
        const userDisplay = document.getElementById('user-display');

        const loginButtons = document.querySelectorAll('#login-btn, #login-btn-top');
        const logoutButtons = document.querySelectorAll('#logout-btn, #logout-btn-top');

        const expNavTab = document.getElementById('exp-nav-tab');
        const entriesNavTab = document.getElementById('entries-nav-tab');
        const resultsNavTab = document.getElementById('results-nav-tab');

        if (user) {
            if (authNav) authNav.style.display = 'block';
            if (userPanel) userPanel.style.display = 'block';

            loginButtons.forEach((btn) => {
                btn.style.display = 'none';
            });

            logoutButtons.forEach((btn) => {
                btn.style.display = 'block';
            });

            if (userName) userName.textContent = user.fullName || '-';
            if (userEmail) userEmail.textContent = user.email || '-';

            if (userDisplay) {
                userDisplay.style.display = 'inline';
                userDisplay.textContent = user.fullName || user.email || '';
            }

            if (expNavTab) expNavTab.style.display = 'block';
            if (entriesNavTab) entriesNavTab.style.display = 'block';
            if (resultsNavTab) resultsNavTab.style.display = 'block';
        } else {
            if (authNav) authNav.style.display = 'none';
            if (userPanel) userPanel.style.display = 'none';

            loginButtons.forEach((btn) => {
                btn.style.display = 'block';
            });

            logoutButtons.forEach((btn) => {
                btn.style.display = 'none';
            });

            if (userDisplay) {
                userDisplay.style.display = 'none';
                userDisplay.textContent = '';
            }

            if (expNavTab) expNavTab.style.display = 'none';
            if (entriesNavTab) entriesNavTab.style.display = 'none';
            if (resultsNavTab) resultsNavTab.style.display = 'none';
        }
    }

    static closeModal(modalId) {
        const modal = bootstrap.Modal.getInstance(document.getElementById(modalId));
        if (modal) modal.hide();
    }

    static openModal(modalId) {
        new bootstrap.Modal(document.getElementById(modalId)).show();
    }

    static clearForm(formId) {
        const form = document.getElementById(formId);
        if (form) form.reset();
    }
}
