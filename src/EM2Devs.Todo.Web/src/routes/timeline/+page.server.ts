import { getTimeline } from '$lib/api/timeline';
import { getBaseUrl } from '$lib/server/config';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch, url }) => {
	const baseUrl = getBaseUrl();
	const eventType = url.searchParams.get('eventType') ?? undefined;

	try {
		const timeline = await getTimeline(fetch, baseUrl, { eventType, pageSize: 20 });
		return { timeline, eventType: eventType ?? null, error: null };
	} catch (e: unknown) {
		return {
			timeline: { events: [], hasMore: false, nextCursor: null },
			eventType: eventType ?? null,
			error: e instanceof Error ? e.message : 'Failed to load timeline'
		};
	}
};
