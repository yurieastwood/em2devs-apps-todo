import { getMe } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async ({ fetch, cookies }) => {
	const hasCookie = cookies.get('demo-user');
	if (!hasCookie) {
		return { user: null };
	}

	const baseUrl = getBaseUrl();

	const apiFetch: typeof fetch = (input, init) => {
		const headers = new Headers(init?.headers);
		headers.set('X-Demo-User', 'true');
		return fetch(input, { ...init, headers });
	};

	try {
		const user = await getMe(apiFetch, baseUrl);
		return { user };
	} catch (error) {
		console.error('[layout] getMe failed for URL:', baseUrl, error);
		return { user: null };
	}
};
