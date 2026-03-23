export interface AuthUser {
	userId: string;
	displayName: string;
}

async function handleAuthResponse(response: Response): Promise<AuthUser> {
	if (response.ok) {
		return response.json();
	}
	throw new Error(`Auth request failed with status ${response.status}`);
}

export async function demoLogin(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<AuthUser> {
	const url = new URL('/api/auth/demo-login', baseUrl);
	const response = await fetch(url, { method: 'POST' });
	return handleAuthResponse(response);
}

export async function logout(fetch: typeof globalThis.fetch, baseUrl: string): Promise<void> {
	const url = new URL('/api/auth/logout', baseUrl);
	const response = await fetch(url, { method: 'POST' });
	if (!response.ok) {
		throw new Error(`Logout failed with status ${response.status}`);
	}
}

export async function getMe(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<AuthUser | null> {
	const url = new URL('/api/auth/me', baseUrl);
	const response = await fetch(url);
	if (response.status === 401) {
		return null;
	}
	return handleAuthResponse(response);
}
