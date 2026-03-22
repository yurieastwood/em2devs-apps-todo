import { env } from '$env/dynamic/private';
import { fail } from '@sveltejs/kit';
import { listTasks, createTask, updateTaskStatus, deleteTask, ApiError } from '$lib/api/tasks';
import type { Actions, PageServerLoad } from './$types';

const VALID_STATUSES = ['Todo', 'InProgress', 'Done'];

function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
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
			return failFromError(e, 'Failed to create task', 'create');
		}
	},

	updateStatus: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';
		const status = formData.get('status')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'updateStatus', error: 'Task ID is required.' });
		}
		if (!VALID_STATUSES.includes(status)) {
			return fail(400, { action: 'updateStatus', error: `Invalid status: ${status}` });
		}

		try {
			await updateTaskStatus(
				fetch,
				getBaseUrl(),
				taskId,
				status as 'Todo' | 'InProgress' | 'Done'
			);
			return { action: 'updateStatus', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to update status', 'updateStatus');
		}
	},

	delete: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'delete', error: 'Task ID is required.' });
		}

		try {
			await deleteTask(fetch, getBaseUrl(), taskId);
			return { action: 'delete', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to delete task', 'delete');
		}
	}
};
