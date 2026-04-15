import { fail } from '@sveltejs/kit';
import { freezeStreak, getProfile } from '$lib/api/profile';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	try {
		const profile = await getProfile(fetch, getBaseUrl());
		return { profile, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { profile: null, error: message };
	}
};

export const actions: Actions = {
	freezeStreak: async ({ request, fetch }) => {
		const form = await request.formData();
		const rawDays = form.get('days');
		const parsed = Number(rawDays ?? 7);
		const days = Number.isFinite(parsed) && parsed >= 1 && parsed <= 7 ? Math.trunc(parsed) : 7;

		try {
			const profile = await freezeStreak(fetch, getBaseUrl(), days);
			return { profile };
		} catch (e) {
			const message = e instanceof Error ? e.message : 'Could not freeze streak';
			return fail(409, { freezeError: message });
		}
	}
};
