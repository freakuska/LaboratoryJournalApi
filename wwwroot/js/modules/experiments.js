// Experiments Module
import { API } from './api.js';
import { UI } from './ui.js';

export class Experiments {
    static experiments = [];

    static init() {
        this.setupEventListeners();
    }

    static setupEventListeners() {
        const form = document.getElementById('exp-form');
        if (form && !form.hasAttribute('onsubmit')) {
            form.addEventListener('submit', (e) => this.handleCreate(e));
        }
    }

    static openModal() {
        this.loadExpForSelect();
        UI.openModal('expModal');
    }

    static async loadExpForSelect(selectId = 'entry-exp') {
        try {
            const data = await API.getExperiments(1, 1000);
            const select = document.getElementById(selectId);
            if (select) {
                select.innerHTML = (data.items || []).map(exp => 
                    `<option value="${exp.id}">${exp.title}</option>`
                ).join('');
            }
        } catch (e) {
            console.error('Load experiments for select error', e);
        }
    }

    static async load() {
        if (!window.currentUser) return;
        const grid = document.getElementById('experiments-grid');
        grid.innerHTML = '<div class="col-12 text-center text-muted"><div class="spinner-border spinner-border-sm"></div> Загрузка...</div>';
        
        try {
            const data = await API.getExperiments(1, 100);
            this.experiments = data.items || [];
            this.render();
        } catch (e) {
            console.error('Load experiments error', e);
            grid.innerHTML = '<div class="col-12 text-center text-danger">Ошибка загрузки</div>';
        }
    }

    static render() {
        const grid = document.getElementById('experiments-grid');
        if (this.experiments.length === 0) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4"><i class="bi bi-inbox" style="font-size: 2rem;"></i><p class="mt-2">Нет экспериментов<br><small>Создайте первый эксперимент</small></p></div>';
            return;
        }

        const statuses = ['Планируется', 'В процессе', 'Завершен'];
        grid.innerHTML = this.experiments.map(exp => `
            <div class="col">
                <div class="exp-card">
                    <h5><i class="bi bi-flask-fill" style="color: #0dcaf0;"></i> ${exp.title}</h5>
                    <p>${exp.description.substring(0, 100)}</p>
                    <div class="exp-meta">
                        <div><strong>Цель:</strong> ${exp.objective.substring(0, 60)}...</div>
                        <div class="mt-2">
                            <span class="badge bg-info">${statuses[exp.status] || 'Неизвестно'}</span>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    }

    static async handleCreate(e) {
        e.preventDefault();
        if (!window.currentUser) return;

        const exp = {
            title: document.getElementById('exp-title').value,
            description: document.getElementById('exp-desc').value,
            objective: document.getElementById('exp-objective').value,
            methodology: document.getElementById('exp-methodology').value,
            startDate: document.getElementById('exp-date').value
        };

        try {
            const result = await API.createExperiment(exp);
            if (result.id) {
                UI.closeModal('expModal');
                UI.clearForm('exp-form');
                await this.load();
                window.Dashboard.loadStats();
                UI.showAlert('✓ Эксперимент создан!', 'success');
            } else {
                UI.showAlert('✗ ' + (result.message || 'Ошибка создания'), 'danger');
            }
        } catch (e) {
            UI.showAlert('✗ Ошибка сети', 'danger');
        }
    }
}
