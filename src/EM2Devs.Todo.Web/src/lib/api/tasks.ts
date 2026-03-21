export interface Task {
	id: string;
	title: string;
	status: 'Todo' | 'InProgress' | 'Done';
}

export async function listTasks(fetch: typeof globalThis.fetch, baseUrl: string): Promise<Task[]> {
	const url = new URL('/api/tasks', baseUrl);
	const response = await fetch(url);
	if (!response.ok) {
		throw new Error(`Failed to fetch tasks: ${response.status} ${response.statusText}`);
	}
	return response.json();
}
