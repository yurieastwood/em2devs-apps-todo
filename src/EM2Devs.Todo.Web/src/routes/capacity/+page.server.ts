import { getCapacityOverview } from '$lib/api/capacity';
import { getBaseUrl } from '$lib/server/config';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	try {
		const capacity = await getCapacityOverview(fetch, baseUrl);
		return { capacity, error: null };
	} catch (e: unknown) {
		return {
			capacity: null,
			error: e instanceof Error ? e.message : 'Failed to load capacity'
		};
	}
};
