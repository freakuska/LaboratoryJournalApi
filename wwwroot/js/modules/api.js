// API Core Module
export class API {
    static BASE_URL = 'http://localhost:5239/api';

    static getHeaders() {
        const token = localStorage.getItem('token');
        return {
            'Content-Type': 'application/json',
            ...(token && { Authorization: `Bearer ${token}` })
        };
    }

    static async parseResponse(res) {
        let data = {};
        try {
            data = await res.json();
        } catch (_) {
            data = {};
        }

        if (!res.ok) {
            throw new Error(data.message || `Request failed (${res.status})`);
        }

        return data;
    }

    // AUTH
    static async login(email, password) {
        const res = await fetch(`${this.BASE_URL}/auth/login`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify({ email, password })
        });

        return this.parseResponse(res);
    }

    static async register(fullName, email, position, password) {
        const res = await fetch(`${this.BASE_URL}/auth/register`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify({ fullName, email, position, password })
        });

        return this.parseResponse(res);
    }

    static async getCurrentUser() {
        const res = await fetch(`${this.BASE_URL}/auth/me`, {
            headers: this.getHeaders()
        });

        return this.parseResponse(res);
    }

    static async logout() {
        const res = await fetch(`${this.BASE_URL}/auth/logout`, {
            method: 'POST',
            headers: this.getHeaders()
        });

        await this.parseResponse(res);
    }

    // EXPERIMENTS
    static async getExperiments(page = 1, pageSize = 100) {
        const res = await fetch(`${this.BASE_URL}/experiments?pageNumber=${page}&pageSize=${pageSize}`, {
            headers: this.getHeaders()
        });

        const data = await this.parseResponse(res);
        return {
            totalCount: data.total || 0,
            items: data.data || []
        };
    }

    static async createExperiment(data) {
        const res = await fetch(`${this.BASE_URL}/experiments`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify(data)
        });

        return this.parseResponse(res);
    }

    // ENTRIES
    static async getEntries(page = 1, pageSize = 100) {
        const res = await fetch(`${this.BASE_URL}/journal-entries?pageNumber=${page}&pageSize=${pageSize}`, {
            headers: this.getHeaders()
        });

        const data = await this.parseResponse(res);
        return {
            totalCount: data.total || 0,
            items: data.data || []
        };
    }

    static async createEntry(data) {
        const res = await fetch(`${this.BASE_URL}/journal-entries`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify(data)
        });

        return this.parseResponse(res);
    }

    // RESULTS
    static async getResults(page = 1, pageSize = 100) {
        const res = await fetch(`${this.BASE_URL}/experiment-results?pageNumber=${page}&pageSize=${pageSize}`, {
            headers: this.getHeaders()
        });

        const data = await this.parseResponse(res);
        return {
            totalCount: data.total || 0,
            items: data.data || []
        };
    }

    static async createResult(data) {
        const res = await fetch(`${this.BASE_URL}/experiment-results`, {
            method: 'POST',
            headers: this.getHeaders(),
            body: JSON.stringify(data)
        });

        return this.parseResponse(res);
    }
}
