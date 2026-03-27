import { getMe } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async ({ fetch, cookies }) => {
	const hasCookie = cookies.get('demo-user');
	if (!hasCookie) {
		return { user: null };
	}

	const apiFetch: typeof fetch = (input, init) => {
		const headers = new Headers(init?.headers);
		headers.set('Cookie', `demo-user=${hasCookie}`);
		return fetch(input, { ...init, headers });
	};

	try {
		const user = await getMe(apiFetch, getBaseUrl());
		return { user };
	} catch (error) {
		console.error('Failed to load authenticated user:', error);
		return { user: null };
	}
};
