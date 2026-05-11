import { describe, it, expect, vi } from 'vitest';
import { deleteAccount, recoverAccount } from './account';
import { ApiError } from './tasks';
import type { AuthResponse } from './auth';

const BASE = 'http://localhost:5001';

function mockOk<T>(data: T, status = 200) {
	return vi.fn().mockResolvedValue({
		ok: true,
		status,
		headers: { get: () => 'application/json' },
		json: () => Promise.resolve(data)
	}) as unknown as typeof fetch;
}

function mockNoContent() {
	return vi.fn().mockResolvedValue({
		ok: true,
		status: 204,
		headers: { get: () => null },
		json: () => Promise.resolve(null)
	}) as unknown as typeof fetch;
}

function mockProblem(status: number, detail: string) {
	return vi.fn().mockResolvedValue({
		ok: false,
		status,
		statusText: 'error',
		headers: { get: () => 'application/problem+json' },
		json: () =>
			Promise.resolve({
				type: 'https://tools.ietf.org/html/rfc9457',
				title: 'error',
				status,
				detail
			})
	}) as unknown as typeof fetch;
}

const sampleAuth: AuthResponse = {
	token: 'jwt-recover',
	userId: '00000000-0000-0000-0000-000000000001',
	displayName: 'Demo User',
	expiresAt: '2026-04-13T00:00:00Z'
};

describe('deleteAccount', () => {
	it('Should_Resolve_When_DeleteSucceeds', async () => {
		await deleteAccount(mockNoContent(), BASE, 'DELETE MY ACCOUNT');
	});

	it('Should_PostConfirmation_When_DeleteCalled', async () => {
		const fetchMock = mockNoContent();
		await deleteAccount(fetchMock, BASE, 'DELETE MY ACCOUNT');
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/account/delete`), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ confirmation: 'DELETE MY ACCOUNT' })
		});
	});

	it('Should_ThrowApiError_When_DeleteFails', async () => {
		await expect(
			deleteAccount(mockProblem(400, 'wrong confirmation'), BASE, 'wrong')
		).rejects.toBeInstanceOf(ApiError);
	});
});

describe('recoverAccount', () => {
	it('Should_ReturnAuthResponse_When_RecoverSucceeds', async () => {
		const result = await recoverAccount(
			mockOk(sampleAuth),
			BASE,
			'demo@waypoint.dev',
			'demo1234'
		);
		expect(result).toEqual(sampleAuth);
	});

	it('Should_ThrowApiError_When_RecoverFailsWithProblemBody', async () => {
		await expect(
			recoverAccount(mockProblem(401, 'Invalid email or password.'), BASE, 'a@b.dev', 'x')
		).rejects.toBeInstanceOf(ApiError);
	});

	it('Should_ThrowApiError_When_RecoverFailsWith409', async () => {
		await expect(
			recoverAccount(mockProblem(409, 'Account is not deactivated.'), BASE, 'a@b.dev', 'x')
		).rejects.toBeInstanceOf(ApiError);
	});
});
