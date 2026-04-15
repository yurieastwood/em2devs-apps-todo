import { fail } from '@sveltejs/kit';
import { freezeStreak, getProfile } from '$lib/api/profile';
import { getDailyBrief } from '$lib/api/dailyBrief';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	const [profileResult, briefResult] = await Promise.all([
		getProfile(fetch, baseUrl).then(
			(profile) => ({ ok: true as const, profile }),
			(e: unknown) => ({
				ok: false as const,
				error: e instanceof Error ? e.message : 'An unexpected error occurred'
			})
		),
		getDailyBrief(fetch, baseUrl).then(
			(brief) => ({ ok: true as const, brief }),
			// Daily brief failure is non-fatal — we still render the rest of the dashboard.
			() => ({ ok: false as const })
		)
	]);

	return {
		profile: profileResult.ok ? profileResult.profile : null,
		error: profileResult.ok ? null : profileResult.error,
		dailyBrief: briefResult.ok ? briefResult.brief : null
	};
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
