import { env } from '$env/dynamic/private';
import { redirect } from '@sveltejs/kit';
import { demoLogin } from '$lib/api/auth';
import type { Actions } from './$types';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

export const actions: Actions = {
	default: async ({ fetch }) => {
		await demoLogin(fetch, getBaseUrl());
		throw redirect(303, '/');
	}
};
