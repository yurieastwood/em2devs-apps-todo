import { fail } from '@sveltejs/kit';
import { freezeStreak, getProfile } from '$lib/api/profile';
import { dismiss, listNotifications, markRead, type Notification } from '$lib/api/notifications';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = getBaseUrl();
	let profile = null;
	let error: string | null = null;
	try {
		profile = await getProfile(fetch, baseUrl);
	} catch (e) {
		error = e instanceof Error ? e.message : 'An unexpected error occurred';
	}

	let notifications: Notification[];
	try {
		notifications = await listNotifications(fetch, baseUrl, { includeRead: true });
	} catch {
		// Non-fatal: dashboard still renders if the notifications endpoint fails.
		notifications = [];
	}

	return { profile, error, notifications };
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
