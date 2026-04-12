import { error, fail, redirect } from '@sveltejs/kit';
import { getQuest, deleteQuest, addTaskToQuest, removeTaskFromQuest } from '$lib/api/quests';
import { listTasks, createTask, ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
}

export const load: PageServerLoad = async ({ fetch, params }) => {
	let quest;
	try {
		quest = await getQuest(fetch, getBaseUrl(), params.id);
	} catch {
		throw error(404, 'Quest not found');
	}

	let availableTasks: { id: string; title: string; status: string }[];
	try {
		const allTasks = await listTasks(fetch, getBaseUrl());
		const questTaskIds = new Set(quest.tasks.map((t) => t.id));
		availableTasks = allTasks.filter((t) => !questTaskIds.has(t.id));
	} catch {
		availableTasks = [];
	}

	return { quest, availableTasks };
};

export const actions: Actions = {
	addTask: async ({ fetch, params, request }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'addTask', error: 'Task ID is required.' });
		}

		try {
			await addTaskToQuest(fetch, getBaseUrl(), params.id, taskId);
			return { action: 'addTask', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to add task to quest', 'addTask');
		}
	},

	createAndAddTask: async ({ fetch, params, request }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';

		if (!title) {
			return fail(400, { action: 'createAndAddTask', error: 'Title is required.' });
		}

		try {
			const task = await createTask(fetch, getBaseUrl(), title);
			await addTaskToQuest(fetch, getBaseUrl(), params.id, task.id);
			return { action: 'createAndAddTask', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to create and add task', 'createAndAddTask');
		}
	},

	removeTask: async ({ fetch, params, request }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'removeTask', error: 'Task ID is required.' });
		}

		try {
			await removeTaskFromQuest(fetch, getBaseUrl(), params.id, taskId);
			return { action: 'removeTask', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to remove task from quest', 'removeTask');
		}
	},

	delete: async ({ fetch, params }) => {
		try {
			await deleteQuest(fetch, getBaseUrl(), params.id);
		} catch (e) {
			const message = e instanceof ApiError ? e.problem.detail : 'Failed to delete quest';
			return fail(500, { action: 'delete', error: message });
		}
		throw redirect(303, '/quests');
	}
};
