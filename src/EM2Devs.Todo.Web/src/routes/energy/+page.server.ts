import { getEnergyProfile, checkInEnergy } from '$lib/api/energy';
import { getBaseUrl } from '$lib/server/config';
import { fail } from '@sveltejs/kit';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	try {
		const profile = await getEnergyProfile(fetch, baseUrl);
		return { profile, error: null };
	} catch (e: unknown) {
		return {
			profile: null,
			error: e instanceof Error ? e.message : 'Failed to load energy profile'
		};
	}
};

export const actions: Actions = {
	checkin: async ({ request, fetch }) => {
		const formData = await request.formData();
		const level = formData.get('level') as string;
		if (!level) return fail(400, { error: 'Level is required' });

		const baseUrl = getBaseUrl();
		try {
			const result = await checkInEnergy(fetch, baseUrl, level);
			return { success: true, result };
		} catch (e: unknown) {
			return fail(500, { error: e instanceof Error ? e.message : 'Check-in failed' });
		}
	}
};
