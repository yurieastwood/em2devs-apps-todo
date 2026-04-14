import { describe, it, expect, vi } from 'vitest';
import { login, register, logout, me, type AuthResponse, type MeResponse } from './auth';

const BASE = 'http://localhost:5001';

function mockOk<T>(data: T) {
	return vi.fn().mockResolvedValue({
		ok: true,
		status: 200,
		headers: { get: () => 'application/json' },
		json: () => Promise.resolve(data)
	}) as unknown as typeof fetch;
}

function mockStatus(status: number) {
	return vi.fn().mockResolvedValue({
		ok: status >= 200 && status < 300,
		status,
		headers: { get: () => 'application/json' },
		json: () => Promise.resolve(null)
	}) as unknown as typeof fetch;
}

const sampleAuth: AuthResponse = {
	token: 'eyJhbGciOiJIUzI1NiJ9.payload.sig',
	userId: '00000000-0000-0000-0000-000000000001',
	displayName: 'Demo User',
	expiresAt: '2026-04-13T00:00:00Z'
};

const sampleMe: MeResponse = {
	userId: '00000000-0000-0000-0000-000000000001',
	displayName: 'Demo User',
	email: 'demo@waypoint.dev'
};

describe('login', () => {
	it('Should_ReturnAuthResponse_When_LoginSucceeds', async () => {
		const result = await login(mockOk(sampleAuth), BASE, 'demo@waypoint.dev', 'demo1234');
		expect(result).toEqual(sampleAuth);
	});

	it('Should_PostCredentials_When_LoginCalled', async () => {
		const fetchMock = mockOk(sampleAuth);
		await login(fetchMock, BASE, 'demo@waypoint.dev', 'demo1234');
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/auth/login`), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ email: 'demo@waypoint.dev', password: 'demo1234' })
		});
	});
});

describe('register', () => {
	it('Should_ReturnAuthResponse_When_RegisterSucceeds', async () => {
		const result = await register(
			mockOk(sampleAuth),
			BASE,
			'new@waypoint.dev',
			'hunter2hunter2',
			'New User'
		);
		expect(result).toEqual(sampleAuth);
	});
});

describe('logout', () => {
	it('Should_CallPostEndpoint_When_LogoutCalled', async () => {
		const fetchMock = mockStatus(204);
		await logout(fetchMock, BASE);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/auth/logout`), {
			method: 'POST'
		});
	});
});

describe('me', () => {
	it('Should_ReturnUser_When_Authenticated', async () => {
		const result = await me(mockOk(sampleMe), BASE);
		expect(result).toEqual(sampleMe);
	});

	it('Should_ReturnNull_When_NotAuthenticated', async () => {
		const result = await me(mockStatus(401), BASE);
		expect(result).toBeNull();
	});
});
