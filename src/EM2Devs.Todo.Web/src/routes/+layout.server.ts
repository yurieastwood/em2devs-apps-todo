import { env } from '$env/dynamic/private';
import { getMe } from '$lib/api/auth';
import type { LayoutServerLoad } from './$types';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

export const load: LayoutServerLoad = async ({ fetch, cookies }) => {
	const hasCookie = cookies.get('demo-user');
	if (!hasCookie) {
		return { user: null };
	}

	const user = await getMe(fetch, getBaseUrl());
	return { user };
};
