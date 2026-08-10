// Main App Module
import { Auth } from './modules/auth.js';
import { UI } from './modules/ui.js';
import { Dashboard } from './modules/dashboard.js';
import { Experiments } from './modules/experiments.js';
import { Entries } from './modules/entries.js';
import { Results } from './modules/results.js';

console.log('Laboratory Journal Pro v1.0');

// Global state
window.currentUser = null;
window.Auth = Auth;
window.UI = UI;
window.Dashboard = Dashboard;
window.Experiments = Experiments;
window.Entries = Entries;
window.Results = Results;

function loadViewData(viewName) {
    if (!window.currentUser) return;

    if (viewName === 'dashboard') {
        Experiments.load();
        Entries.load();
        Results.load();
        Dashboard.loadStats();
        return;
    }

    if (viewName === 'experiments') {
        Experiments.load();
        return;
    }

    if (viewName === 'entries') {
        Entries.load();
        return;
    }

    if (viewName === 'results') {
        Results.load();
    }
}

window.goToPage = (viewName) => {
    if (!viewName) return;

    // Protect private modules when user is not authenticated.
    if (viewName !== 'auth' && !window.currentUser) {
        UI.showView('auth');
        return;
    }

    UI.showView(viewName);
    loadViewData(viewName);
};

window.showView = (viewName) => window.goToPage(viewName);
window.logout = () => Auth.logout();
window.handleLogin = (e) => Auth.handleLogin(e);
window.handleRegister = (e) => Auth.handleRegister(e);
window.createExperiment = (e) => Experiments.handleCreate(e);
window.createEntry = (e) => Entries.handleCreate(e);
window.createResult = (e) => Results.handleCreate(e);

// App initialization
document.addEventListener('DOMContentLoaded', () => {
    console.log('App is initializing...');

    Dashboard.init();
    Experiments.init();
    Entries.init();
    Results.init();
    Auth.init();

    console.log('All modules loaded');
});

// HTML onclick handlers
window.openExpModal = () => Experiments.openModal();
window.openEntryModal = () => Entries.openModal();
window.openResultModal = () => Results.openModal();
