import { getWrapped } from '$lib/api/wrapped';
import { getBaseUrl } from '$lib/server/config';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch, url }) => {
	const baseUrl = getBaseUrl();
	const yearParam = url.searchParams.get('year');
	const year = yearParam ? parseInt(yearParam, 10) : undefined;

	try {
		const wrapped = await getWrapped(fetch, baseUrl, year);
		return { wrapped, error: null };
	} catch (e: unknown) {
		return { wrapped: null, error: e instanceof Error ? e.message : 'Failed to load wrapped' };
	}
};
