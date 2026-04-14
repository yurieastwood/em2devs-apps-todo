import { redirect } from '@sveltejs/kit';
import { logout } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions } from './$types';

export const actions: Actions = {
	default: async ({ fetch, cookies }) => {
		// Best-effort call to the API; stateless JWT logout is a client-side concern.
		try {
			await logout(fetch, getBaseUrl());
		} catch {
			// Ignore — we're clearing our cookie regardless.
		}
		cookies.delete(TOKEN_COOKIE, { path: '/' });
		throw redirect(303, '/login');
	}
};
