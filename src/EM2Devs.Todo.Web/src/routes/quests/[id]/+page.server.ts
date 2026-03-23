import { error, fail, redirect } from '@sveltejs/kit';
import { getQuest, deleteQuest } from '$lib/api/quests';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch, params }) => {
	try {
		const quest = await getQuest(fetch, getBaseUrl(), params.id);
		return { quest };
	} catch {
		throw error(404, 'Quest not found');
	}
};

export const actions: Actions = {
	delete: async ({ fetch, params }) => {
		try {
			await deleteQuest(fetch, getBaseUrl(), params.id);
		} catch (e) {
			const message = e instanceof ApiError ? e.problem.detail : 'Failed to delete quest';
			return fail(500, { action: 'delete', error: message });
		}
		throw redirect(303, '/quests');
	}
};
