import { env } from '$env/dynamic/private';
import { redirect } from '@sveltejs/kit';
import { login } from '$lib/api/auth';
import type { Actions } from './$types';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

export const actions: Actions = {
	default: async ({ fetch, cookies }) => {
		await login(fetch, getBaseUrl());
		cookies.set('demo-user', 'true', { path: '/', httpOnly: true, sameSite: 'lax' });
		throw redirect(303, '/');
	}
};
