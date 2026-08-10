// Results Module
import { API } from './api.js';
import { UI } from './ui.js';

export class Results {
    static results = [];

    static init() {
        this.setupEventListeners();
    }

    static setupEventListeners() {
        const form = document.getElementById('result-form');
        if (form && !form.hasAttribute('onsubmit')) {
            form.addEventListener('submit', (e) => this.handleCreate(e));
        }
    }

    static openModal() {
        this.loadExpForSelect('result-exp');
        UI.openModal('resultModal');
    }

    static async loadExpForSelect(selectId = 'result-exp') {
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
        const grid = document.getElementById('results-grid');
        grid.innerHTML = '<div class="col-12 text-center text-muted"><div class="spinner-border spinner-border-sm"></div> Загрузка...</div>';
        
        try {
            const data = await API.getResults(1, 100);
            this.results = data.items || [];
            this.render();
        } catch (e) {
            console.error('Load results error', e);
        }
    }

    static render() {
        const grid = document.getElementById('results-grid');
        if (this.results.length === 0) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4"><i class="bi bi-graph-up" style="font-size: 2rem;"></i><p class="mt-2">Нет результатов<br><small>Добавьте первый результат</small></p></div>';
            return;
        }

        const datatypes = ['Число', 'Текст', 'Дата', 'Логический', 'URL', 'JSON'];
        grid.innerHTML = this.results.map(result => `
            <div class="col">
                <div class="result-card">
                    <h5><i class="bi bi-graph-up" style="color: #0d6efd;"></i> ${result.name}</h5>
                    <p><strong>${result.value}</strong> ${result.unit}</p>
                    <p>${result.description}</p>
                    <div class="result-meta">
                        <div><strong>Тип:</strong> ${datatypes[result.dataType] || 'Неизвестно'}</div>
                    </div>
                </div>
            </div>
        `).join('');
    }

    static async handleCreate(e) {
        e.preventDefault();
        if (!window.currentUser) return;

        const result = {
            experimentId: parseInt(document.getElementById('result-exp').value),
            name: document.getElementById('result-name').value,
            description: document.getElementById('result-desc').value,
            value: document.getElementById('result-value').value,
            unit: document.getElementById('result-unit').value,
            dataType: parseInt(document.getElementById('result-datatype').value),
            status: parseInt(document.getElementById('result-status').value),
            notes: '',
            recordedAt: new Date().toISOString()
        };

        try {
            const res = await API.createResult(result);
            if (res.id) {
                UI.closeModal('resultModal');
                UI.clearForm('result-form');
                await this.load();
                window.Dashboard.loadStats();
                UI.showAlert('✓ Результат создан!', 'success');
            } else {
                UI.showAlert('✗ ' + (res.message || 'Ошибка создания'), 'danger');
            }
        } catch (e) {
            UI.showAlert('✗ Ошибка сети', 'danger');
        }
    }
}
