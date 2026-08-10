// Entries Module
import { API } from './api.js';
import { UI } from './ui.js';

export class Entries {
    static entries = [];

    static init() {
        this.setupEventListeners();
    }

    static setupEventListeners() {
        const form = document.getElementById('entry-form');
        if (form && !form.hasAttribute('onsubmit')) {
            form.addEventListener('submit', (e) => this.handleCreate(e));
        }
    }

    static openModal() {
        this.loadExpForSelect('entry-exp');
        UI.openModal('entryModal');
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
        const grid = document.getElementById('entries-grid');
        grid.innerHTML = '<div class="col-12 text-center text-muted"><div class="spinner-border spinner-border-sm"></div> Загрузка...</div>';
        
        try {
            const data = await API.getEntries(1, 100);
            this.entries = data.items || [];
            this.render();
        } catch (e) {
            console.error('Load entries error', e);
        }
    }

    static render() {
        const grid = document.getElementById('entries-grid');
        if (this.entries.length === 0) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4"><i class="bi bi-journal" style="font-size: 2rem;"></i><p class="mt-2">Нет записей<br><small>Создайте первую запись</small></p></div>';
            return;
        }

        const types = ['Наблюдение', 'Результат', 'Проблема', 'Заметка', 'Процедура', 'Заключение'];
        grid.innerHTML = this.entries.map(entry => `
            <div class="col">
                <div class="entry-card">
                    <h5><i class="bi bi-journal-text" style="color: #20c997;"></i> ${entry.title}</h5>
                    <p>${entry.content.substring(0, 100)}...</p>
                    <div class="entry-meta">
                        <div><strong>Тип:</strong> ${types[entry.type] || 'Неизвестно'}</div>
                        <div><strong>Теги:</strong> <em>${entry.tags || 'нет'}</em></div>
                    </div>
                </div>
            </div>
        `).join('');
    }

    static async handleCreate(e) {
        e.preventDefault();
        if (!window.currentUser) return;

        const entry = {
            experimentId: parseInt(document.getElementById('entry-exp').value),
            title: document.getElementById('entry-title').value,
            content: document.getElementById('entry-content').value,
            type: parseInt(document.getElementById('entry-type').value),
            priority: parseInt(document.getElementById('entry-priority').value),
            tags: document.getElementById('entry-tags').value,
            attachments: ''
        };

        try {
            const result = await API.createEntry(entry);
            if (result.id) {
                UI.closeModal('entryModal');
                UI.clearForm('entry-form');
                await this.load();
                window.Dashboard.loadStats();
                UI.showAlert('✓ Запись создана!', 'success');
            } else {
                UI.showAlert('✗ ' + (result.message || 'Ошибка создания'), 'danger');
            }
        } catch (e) {
            UI.showAlert('✗ Ошибка сети', 'danger');
        }
    }
}
