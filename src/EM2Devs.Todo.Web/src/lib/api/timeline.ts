import { handleResponse } from './tasks';

export interface TimelineEvent {
	id: string;
	eventType: string;
	occurredAt: string;
	details: string;
	note: string | null;
}

export interface TimelinePage {
	events: TimelineEvent[];
	hasMore: boolean;
	nextCursor: string | null;
}

export async function getTimeline(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	options?: { eventType?: string; cursor?: string; pageSize?: number }
): Promise<TimelinePage> {
	const url = new URL('/api/timeline', baseUrl);
	if (options?.eventType) url.searchParams.set('eventType', options.eventType);
	if (options?.cursor) url.searchParams.set('cursor', options.cursor);
	if (options?.pageSize) url.searchParams.set('pageSize', String(options.pageSize));
	const response = await fetch(url);
	return handleResponse<TimelinePage>(response);
}
