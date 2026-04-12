import { redirect } from '@sveltejs/kit';
import { login } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import type { Actions } from './$types';

export const actions: Actions = {
	default: async ({ fetch, cookies }) => {
		await login(fetch, getBaseUrl());
		cookies.set('demo-user', 'true', { path: '/', httpOnly: true, sameSite: 'lax' });
		throw redirect(303, '/');
	}
};
