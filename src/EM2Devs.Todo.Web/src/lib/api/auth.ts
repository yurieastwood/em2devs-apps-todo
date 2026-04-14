import { ApiError, type ProblemDetails } from './tasks';

export interface AuthResponse {
	token: string;
	userId: string;
	displayName: string;
	expiresAt: string;
}

export interface MeResponse {
	userId: string;
	displayName: string;
	email: string;
}

// Legacy alias retained so existing consumers keep compiling.
export type AuthUser = MeResponse;

async function throwIfError(response: Response): Promise<never> {
	const contentType = response.headers.get('content-type') ?? '';
	if (
		contentType.includes('application/problem+json') ||
		contentType.includes('application/json')
	) {
		const problem: ProblemDetails = await response.json();
		throw new ApiError(problem);
	}

	throw new ApiError({
		type: 'https://tools.ietf.org/html/rfc9457',
		title: response.statusText,
		status: response.status,
		detail: `Request failed with status ${response.status}`
	});
}

async function readJson<T>(response: Response): Promise<T> {
	if (response.ok) {
		return response.json();
	}
	return throwIfError(response);
}

export async function login(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	email: string,
	password: string
): Promise<AuthResponse> {
	const url = new URL('/api/auth/login', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ email, password })
	});
	return readJson<AuthResponse>(response);
}

export async function register(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	email: string,
	password: string,
	displayName: string
): Promise<AuthResponse> {
	const url = new URL('/api/auth/register', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ email, password, displayName })
	});
	return readJson<AuthResponse>(response);
}

export async function logout(fetch: typeof globalThis.fetch, baseUrl: string): Promise<void> {
	const url = new URL('/api/auth/logout', baseUrl);
	const response = await fetch(url, { method: 'POST' });
	if (response.ok) return;
	await throwIfError(response);
}

export async function me(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<MeResponse | null> {
	const url = new URL('/api/auth/me', baseUrl);
	const response = await fetch(url);
	if (response.status === 401) {
		return null;
	}
	return readJson<MeResponse>(response);
}

// Back-compat alias used by hooks.server.ts and +layout.server.ts.
export const getMe = me;
