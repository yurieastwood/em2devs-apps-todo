import { env } from '$env/dynamic/private';
import { fail } from '@sveltejs/kit';
import {
	listTasks,
	createTask,
	updateTaskStatus,
	deleteTask,
	ApiError
} from '$lib/api/tasks';
import type { Actions, PageServerLoad } from './$types';

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

export const load: PageServerLoad = async ({ fetch }) => {
	try {
		const tasks = await listTasks(fetch, getBaseUrl());
		return { tasks, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { tasks: [], error: message };
	}
};

export const actions: Actions = {
	create: async ({ request, fetch }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';

		if (!title) {
			return fail(400, { action: 'create', error: 'Title is required.' });
		}

		try {
			await createTask(fetch, getBaseUrl(), title);
			return { action: 'create', success: true };
		} catch (e) {
			const message =
				e instanceof ApiError ? e.problem.detail : 'Failed to create task';
			return fail(400, { action: 'create', error: message });
		}
	},

	updateStatus: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';
		const status = formData.get('status')?.toString() ?? '';

		try {
			await updateTaskStatus(
				fetch,
				getBaseUrl(),
				taskId,
				status as 'Todo' | 'InProgress' | 'Done'
			);
			return { action: 'updateStatus', success: true };
		} catch (e) {
			const message =
				e instanceof ApiError ? e.problem.detail : 'Failed to update status';
			return fail(409, { action: 'updateStatus', error: message });
		}
	},

	delete: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		try {
			await deleteTask(fetch, getBaseUrl(), taskId);
			return { action: 'delete', success: true };
		} catch (e) {
			const message =
				e instanceof ApiError ? e.problem.detail : 'Failed to delete task';
			return fail(400, { action: 'delete', error: message });
		}
	}
};
