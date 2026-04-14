import { error, fail, redirect } from '@sveltejs/kit';
import {
	getTask,
	updateTask,
	deleteTask,
	recordActualTime,
	ApiError,
	type TaskDifficulty,
	type TaskPriority
} from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

const VALID_DIFFICULTIES: TaskDifficulty[] = ['Trivial', 'Easy', 'Normal', 'Hard', 'Epic'];
const VALID_PRIORITIES: TaskPriority[] = ['Low', 'Medium', 'High', 'Critical'];

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
}

export const load: PageServerLoad = async ({ fetch, params }) => {
	try {
		const task = await getTask(fetch, getBaseUrl(), params.id);
		return { task };
	} catch {
		throw error(404, 'Task not found');
	}
};

export const actions: Actions = {
	save: async ({ request, fetch, params }) => {
		const formData = await request.formData();

		const title = formData.get('title')?.toString()?.trim() ?? '';
		const description = formData.get('description')?.toString() ?? '';
		const difficulty = formData.get('difficulty')?.toString() ?? '';
		const priority = formData.get('priority')?.toString() ?? '';
		const estimatedMinutesRaw = formData.get('estimatedMinutes')?.toString() ?? '';
		const dueDateRaw = formData.get('dueDate')?.toString() ?? '';

		if (!title) {
			return fail(400, { action: 'save', error: 'Title is required.' });
		}
		if (!VALID_DIFFICULTIES.includes(difficulty as TaskDifficulty)) {
			return fail(400, { action: 'save', error: `Invalid difficulty: ${difficulty}` });
		}
		if (!VALID_PRIORITIES.includes(priority as TaskPriority)) {
			return fail(400, { action: 'save', error: `Invalid priority: ${priority}` });
		}

		const estimatedMinutes = estimatedMinutesRaw === '' ? null : Number(estimatedMinutesRaw);
		if (estimatedMinutes !== null && Number.isNaN(estimatedMinutes)) {
			return fail(400, {
				action: 'save',
				error: 'Estimated minutes must be a valid number.'
			});
		}
		if (
			estimatedMinutes !== null &&
			(!Number.isInteger(estimatedMinutes) ||
				estimatedMinutes < 1 ||
				estimatedMinutes > 525600)
		) {
			return fail(400, {
				action: 'save',
				error: 'Estimated minutes must be an integer between 1 and 525600.'
			});
		}

		let dueDate: string | null = null;
		if (dueDateRaw !== '') {
			const parsedDueDate = new Date(dueDateRaw);
			if (Number.isNaN(parsedDueDate.getTime())) {
				return fail(400, {
					action: 'save',
					error: 'Due date must be a valid date/time.'
				});
			}
			dueDate = parsedDueDate.toISOString();
		}

		try {
			await updateTask(fetch, getBaseUrl(), params.id, {
				title,
				description: description === '' ? null : description,
				difficulty: difficulty as TaskDifficulty,
				priority: priority as TaskPriority,
				estimatedMinutes,
				clearEstimatedTime: estimatedMinutes === null,
				dueDate,
				clearDueDate: dueDate === null
			});
		} catch (e) {
			return failFromError(e, 'Failed to save task', 'save');
		}

		throw redirect(303, '/');
	},

	delete: async ({ fetch, params }) => {
		try {
			await deleteTask(fetch, getBaseUrl(), params.id);
		} catch (e) {
			return failFromError(e, 'Failed to delete task', 'delete');
		}
		throw redirect(303, '/');
	},

	recordActualTime: async ({ request, fetch, params }) => {
		const formData = await request.formData();
		const raw = formData.get('actualMinutes')?.toString() ?? '';
		const actualMinutes = Number(raw);

		if (raw === '' || Number.isNaN(actualMinutes)) {
			return fail(400, {
				action: 'recordActualTime',
				error: 'Actual minutes must be a number.'
			});
		}
		if (!Number.isInteger(actualMinutes) || actualMinutes < 1 || actualMinutes > 1440) {
			return fail(400, {
				action: 'recordActualTime',
				error: 'Actual minutes must be between 1 and 1440.'
			});
		}

		try {
			const updated = await recordActualTime(fetch, getBaseUrl(), params.id, actualMinutes);
			return { action: 'recordActualTime', task: updated };
		} catch (e) {
			return failFromError(e, 'Failed to record actual time', 'recordActualTime');
		}
	}
};
