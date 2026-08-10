// Dashboard Module
import { API } from './api.js';

export class Dashboard {
    static statusChart = null;
    static pieChart = null;

    static init() {
        this.renderCharts();
    }

    static async loadStats() {
        if (!window.currentUser) return;
        try {
            const [expRes, entriesRes, resultsRes] = await Promise.all([
                API.getExperiments(1, 1),
                API.getEntries(1, 1),
                API.getResults(1, 1)
            ]);

            document.getElementById('stat-exps').textContent = expRes.totalCount || 0;
            document.getElementById('stat-entries').textContent = entriesRes.totalCount || 0;
            document.getElementById('stat-results').textContent = resultsRes.totalCount || 0;
            document.getElementById('stat-completed').textContent = Math.floor((expRes.totalCount || 0) * 0.6);
        } catch (e) {
            console.error('Load stats error', e);
        }
    }

    static renderCharts() {
        const ctx1 = document.getElementById('chart-stats');
        const ctx2 = document.getElementById('chart-pie');

        if (ctx1 && !this.statusChart) {
            this.statusChart = new Chart(ctx1, {
                type: 'bar',
                data: {
                    labels: ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'],
                    datasets: [{
                        label: 'Экспериментов',
                        data: [5, 8, 12, 9, 15, 7, 4],
                        backgroundColor: 'rgba(13, 110, 253, 0.8)',
                        borderRadius: 8,
                        borderSkipped: false
                    }, {
                        label: 'Записей',
                        data: [10, 14, 18, 16, 22, 12, 8],
                        backgroundColor: 'rgba(32, 201, 151, 0.8)',
                        borderRadius: 8,
                        borderSkipped: false
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top',
                            labels: { font: { size: 12 } }
                        }
                    },
                    scales: {
                        y: { beginAtZero: true }
                    }
                }
            });
        }

        if (ctx2 && !this.pieChart) {
            this.pieChart = new Chart(ctx2, {
                type: 'doughnut',
                data: {
                    labels: ['Планируется', 'В процессе', 'Завершено'],
                    datasets: [{
                        data: [30, 45, 25],
                        backgroundColor: ['#0d6efd', '#0dcaf0', '#20c997'],
                        borderColor: 'white',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: { font: { size: 12 } }
                        }
                    }
                }
            });
        }
    }
}
