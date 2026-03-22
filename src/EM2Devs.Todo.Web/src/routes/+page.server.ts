import { env } from '$env/dynamic/private';
import { listTasks } from '$lib/api/tasks';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const baseUrl = env.API_BASE_URL ?? 'http://localhost:5001';

	try {
		const tasks = await listTasks(fetch, baseUrl);
		return { tasks, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { tasks: [], error: message };
	}
};
