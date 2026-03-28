import { redirect } from '@sveltejs/kit';
import { logout } from '$lib/api/auth';
import { getBaseUrl } from '$lib/server/config';
import type { Actions } from './$types';

export const actions: Actions = {
	default: async ({ fetch, cookies }) => {
		await logout(fetch, getBaseUrl());
		cookies.delete('demo-user', { path: '/' });
		throw redirect(303, '/login');
	}
};
