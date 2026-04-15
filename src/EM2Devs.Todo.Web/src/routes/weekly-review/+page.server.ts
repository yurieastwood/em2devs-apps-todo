import { fail } from '@sveltejs/kit';
import { getWeeklyReview, saveWeeklyReview } from '$lib/api/weeklyReview';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch, url }) => {
	const weekOf = url.searchParams.get('weekOf');
	try {
		const review = await getWeeklyReview(fetch, getBaseUrl(), weekOf);
		return { review, error: null as string | null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'Could not load weekly review';
		return { review: null, error: message };
	}
};

export const actions: Actions = {
	save: async ({ request, fetch }) => {
		const form = await request.formData();
		const whatWentWell = String(form.get('whatWentWell') ?? '').trim();
		const whatDragged = String(form.get('whatDragged') ?? '').trim();
		const adjustment = String(form.get('adjustment') ?? '').trim();
		const weekOf = (form.get('weekOf') as string) || null;

		if (!whatWentWell || !whatDragged || !adjustment) {
			return fail(400, {
				saveError: 'All three reflection fields are required.',
				whatWentWell,
				whatDragged,
				adjustment
			});
		}

		try {
			const reflection = await saveWeeklyReview(fetch, getBaseUrl(), {
				whatWentWell,
				whatDragged,
				adjustment,
				weekOf
			});
			return { reflection };
		} catch (e) {
			const message = e instanceof Error ? e.message : 'Could not save reflection';
			return fail(500, {
				saveError: message,
				whatWentWell,
				whatDragged,
				adjustment
			});
		}
	}
};
