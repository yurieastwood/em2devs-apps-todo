import { fail } from '@sveltejs/kit';
import {
	listRecurringTasks,
	createRecurringTask,
	pauseRecurringTask,
	resumeRecurringTask,
	deleteRecurringTask
} from '$lib/api/recurring';
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
		const recurringTasks = await listRecurringTasks(fetch, getBaseUrl());
		return { recurringTasks, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { recurringTasks: [], error: message };
	}
};

export const actions: Actions = {
	create: async ({ request, fetch }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';
		const pattern = formData.get('pattern')?.toString() ?? '';

		if (!title) {
			return fail(400, { action: 'create', error: 'Title is required.' });
		}
		if (!pattern) {
			return fail(400, { action: 'create', error: 'Pattern is required.' });
		}

		try {
			await createRecurringTask(fetch, getBaseUrl(), title, pattern);
			return { action: 'create', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to create recurring task', 'create');
		}
	},

	pause: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';

		try {
			await pauseRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'pause', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to pause recurring task', 'pause');
		}
	},

	resume: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';

		try {
			await resumeRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'resume', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to resume recurring task', 'resume');
		}
	},

	delete: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';

		try {
			await deleteRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'delete', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to delete recurring task', 'delete');
		}
	}
};
