import { me } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../hooks.server';
import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async ({ fetch, cookies }) => {
	const token = cookies.get(TOKEN_COOKIE);
	if (!token) {
		return { user: null };
	}

	try {
		const user = await me(fetch, getBaseUrl());
		return { user };
	} catch (error) {
		console.error('[layout] me() failed:', error);
		return { user: null };
	}
};
