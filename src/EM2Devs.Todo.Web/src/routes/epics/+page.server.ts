import { fail } from '@sveltejs/kit';
import { listEpics, createEpic } from '$lib/api/epics';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
}

export const load: PageServerLoad = async ({ fetch }) => {
	try {
		const epics = await listEpics(fetch, getBaseUrl());
		return { epics, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { epics: [], error: message };
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
			await createEpic(fetch, getBaseUrl(), title, description);
			return { action: 'create', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to create epic', 'create');
		}
	}
};
