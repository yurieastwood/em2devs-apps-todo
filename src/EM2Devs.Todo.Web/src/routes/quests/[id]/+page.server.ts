import { env } from '$env/dynamic/private';
import { error } from '@sveltejs/kit';
import { getQuest, deleteQuest } from '$lib/api/quests';
import type { Actions, PageServerLoad } from './$types';
import { redirect } from '@sveltejs/kit';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

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
		await deleteQuest(fetch, getBaseUrl(), params.id);
		throw redirect(303, '/quests');
	}
};
