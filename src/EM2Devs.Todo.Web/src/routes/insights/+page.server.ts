import { listInsights, markInsightRead, saveInsight, dismissInsight } from '$lib/api/insights';
import { getBaseUrl } from '$lib/server/config';
import { fail } from '@sveltejs/kit';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	try {
		const insights = await listInsights(fetch, baseUrl, { includeRead: true });
		return { insights, error: null };
	} catch (e: unknown) {
		return { insights: [], error: e instanceof Error ? e.message : 'Failed to load insights' };
	}
};

export const actions: Actions = {
	read: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id') as string;
		if (!id) return fail(400);
		try {
			await markInsightRead(fetch, getBaseUrl(), id);
		} catch {
			return fail(500, { error: 'Failed to mark as read' });
		}
	},
	save: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id') as string;
		if (!id) return fail(400);
		try {
			await saveInsight(fetch, getBaseUrl(), id);
		} catch {
			return fail(500, { error: 'Failed to save insight' });
		}
	},
	dismiss: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id') as string;
		if (!id) return fail(400);
		try {
			await dismissInsight(fetch, getBaseUrl(), id);
		} catch {
			return fail(500, { error: 'Failed to dismiss insight' });
		}
	}
};
