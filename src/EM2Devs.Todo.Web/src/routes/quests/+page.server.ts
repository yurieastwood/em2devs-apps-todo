import { env } from '$env/dynamic/private';
import { fail } from '@sveltejs/kit';
import { listQuests, createQuest } from '$lib/api/quests';
import type { Actions, PageServerLoad } from './$types';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

export const load: PageServerLoad = async ({ fetch }) => {
	try {
		const quests = await listQuests(fetch, getBaseUrl());
		return { quests, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { quests: [], error: message };
	}
};

export const actions: Actions = {
	create: async ({ request, fetch }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';
		const description = formData.get('description')?.toString()?.trim() ?? '';

		if (!title) {
			return fail(400, { action: 'create', error: 'Title is required.' });
		}

		try {
			await createQuest(fetch, getBaseUrl(), title, description);
			return { action: 'create', success: true };
		} catch (e) {
			const message = e instanceof Error ? e.message : 'Failed to create quest';
			return fail(500, { action: 'create', error: message });
		}
	}
};
