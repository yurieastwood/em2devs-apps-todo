import { getCurrentSeason } from '$lib/api/seasons';
import { getBaseUrl } from '$lib/server/config';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	try {
		const season = await getCurrentSeason(fetch, baseUrl);
		return { season, error: null };
	} catch (e: unknown) {
		return { season: null, error: e instanceof Error ? e.message : 'Failed to load season' };
	}
};
