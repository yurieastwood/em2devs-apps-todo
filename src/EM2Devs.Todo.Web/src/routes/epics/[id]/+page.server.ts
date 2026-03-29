import { error, fail, redirect } from '@sveltejs/kit';
import {
	getEpic,
	assignQuestToEpic,
	removeQuestFromEpic,
	completeEpic,
	deleteEpic
} from '$lib/api/epics';
import { listQuests } from '$lib/api/quests';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
}

export const load: PageServerLoad = async ({ fetch, params }) => {
	let epic;
	try {
		epic = await getEpic(fetch, getBaseUrl(), params.id);
	} catch {
		throw error(404, 'Epic not found');
	}

	let availableQuests: { id: string; title: string; progress: number }[];
	try {
		const allQuests = await listQuests(fetch, getBaseUrl());
		const epicQuestIds = new Set(epic.quests.map((q) => q.id));
		availableQuests = allQuests
			.filter((q) => !epicQuestIds.has(q.id))
			.map((q) => ({ id: q.id, title: q.title, progress: q.progress }));
	} catch {
		availableQuests = [];
	}

	return { epic, availableQuests };
};

export const actions: Actions = {
	assignQuest: async ({ fetch, params, request }) => {
		const formData = await request.formData();
		const questId = formData.get('questId')?.toString() ?? '';

		if (!questId) {
			return fail(400, { action: 'assignQuest', error: 'Quest is required.' });
		}

		try {
			await assignQuestToEpic(fetch, getBaseUrl(), params.id, questId);
			return { action: 'assignQuest', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to assign quest', 'assignQuest');
		}
	},

	removeQuest: async ({ fetch, params, request }) => {
		const formData = await request.formData();
		const questId = formData.get('questId')?.toString() ?? '';

		if (!questId) {
			return fail(400, { action: 'removeQuest', error: 'Quest ID is required.' });
		}

		try {
			await removeQuestFromEpic(fetch, getBaseUrl(), params.id, questId);
			return { action: 'removeQuest', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to remove quest', 'removeQuest');
		}
	},

	complete: async ({ fetch, params }) => {
		try {
			await completeEpic(fetch, getBaseUrl(), params.id);
			return { action: 'complete', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to complete epic', 'complete');
		}
	},

	delete: async ({ fetch, params }) => {
		try {
			await deleteEpic(fetch, getBaseUrl(), params.id);
		} catch (e) {
			return failFromError(e, 'Failed to delete epic', 'delete');
		}
		throw redirect(303, '/epics');
	}
};
