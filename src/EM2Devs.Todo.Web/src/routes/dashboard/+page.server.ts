import { getProfile } from '$lib/api/profile';
import { getBaseUrl } from '$lib/server/config';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	try {
		const profile = await getProfile(fetch, getBaseUrl());
		return { profile, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { profile: null, error: message };
	}
};
