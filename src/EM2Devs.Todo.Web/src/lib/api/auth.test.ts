import { describe, it, expect, vi } from 'vitest';
import { demoLogin, logout, getMe, type AuthUser } from './auth';

const BASE = 'http://localhost:5001';

function mockOk<T>(data: T) {
	return vi.fn().mockResolvedValue({
		ok: true,
		status: 200,
		json: () => Promise.resolve(data)
	}) as unknown as typeof fetch;
}

function mockStatus(status: number) {
	return vi.fn().mockResolvedValue({
		ok: status >= 200 && status < 300,
		status,
		json: () => Promise.resolve(null)
	}) as unknown as typeof fetch;
}

describe('demoLogin', () => {
	it('Should_ReturnDemoUser_When_LoginSucceeds', async () => {
		const user: AuthUser = {
			userId: '00000000-0000-0000-0000-000000000001',
			displayName: 'Demo User'
		};
		const result = await demoLogin(mockOk(user), BASE);
		expect(result).toEqual(user);
	});

	it('Should_CallPostEndpoint_When_LoginCalled', async () => {
		const fetchMock = mockOk({ userId: '1', displayName: 'Demo User' });
		await demoLogin(fetchMock, BASE);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/auth/login`), {
			method: 'POST'
		});
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

describe('getMe', () => {
	it('Should_ReturnUser_When_Authenticated', async () => {
		const user: AuthUser = {
			userId: '00000000-0000-0000-0000-000000000001',
			displayName: 'Demo User'
		};
		const result = await getMe(mockOk(user), BASE);
		expect(result).toEqual(user);
	});

	it('Should_ReturnNull_When_NotAuthenticated', async () => {
		const result = await getMe(mockStatus(401), BASE);
		expect(result).toBeNull();
	});
});
