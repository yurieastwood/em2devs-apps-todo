import { fail } from '@sveltejs/kit';
import { freezeStreak, getProfile } from '$lib/api/profile';
import { dismiss, listNotifications, markRead, type Notification } from '$lib/api/notifications';
import { getDailyBrief } from '$lib/api/dailyBrief';
import { getWeeklyReview, type WeeklyReview } from '$lib/api/weeklyReview';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	const [profileResult, briefResult, notificationsResult, weeklyReviewResult] = await Promise.all(
		[
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
			),
			listNotifications(fetch, baseUrl, { includeRead: true }).then(
				(notifications) => ({ ok: true as const, notifications }),
				// Non-fatal: dashboard still renders if the notifications endpoint fails.
				() => ({ ok: false as const, notifications: [] as Notification[] })
			),
			getWeeklyReview(fetch, baseUrl).then(
				(review) => ({ ok: true as const, review }),
				// Non-fatal: dashboard still renders if the weekly review endpoint fails.
				() => ({ ok: false as const })
			)
		]
	);

	// Surface the weekly review nudge on Sundays when no reflection has been saved yet.
	const todayIsSunday = new Date().getDay() === 0;
	const review: WeeklyReview | null = weeklyReviewResult.ok ? weeklyReviewResult.review : null;
	const showWeeklyReviewNudge = todayIsSunday && review !== null && review.reflection === null;

	return {
		profile: profileResult.ok ? profileResult.profile : null,
		error: profileResult.ok ? null : profileResult.error,
		dailyBrief: briefResult.ok ? briefResult.brief : null,
		notifications: notificationsResult.notifications,
		showWeeklyReviewNudge
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
	},
	markNotificationRead: async ({ request, fetch }) => {
		const form = await request.formData();
		const id = form.get('id');
		if (typeof id !== 'string' || id.length === 0) {
			return fail(400, { notificationError: 'Missing notification id' });
		}
		try {
			await markRead(fetch, getBaseUrl(), id);
			return { ok: true };
		} catch (e) {
			const message = e instanceof Error ? e.message : 'Could not mark as read';
			return fail(500, { notificationError: message });
		}
	},
	dismissNotification: async ({ request, fetch }) => {
		const form = await request.formData();
		const id = form.get('id');
		if (typeof id !== 'string' || id.length === 0) {
			return fail(400, { notificationError: 'Missing notification id' });
		}
		try {
			await dismiss(fetch, getBaseUrl(), id);
			return { ok: true };
		} catch (e) {
			const message = e instanceof Error ? e.message : 'Could not dismiss';
			return fail(500, { notificationError: message });
		}
	}
};
