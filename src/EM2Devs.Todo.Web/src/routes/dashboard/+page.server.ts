import { env } from '$env/dynamic/private';
import { getProfile } from '$lib/api/profile';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = env.API_BASE_URL ?? 'http://localhost:5001';

	try {
		const profile = await getProfile(fetch, baseUrl);
		return { profile, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { profile: null, error: message };
	}
};
