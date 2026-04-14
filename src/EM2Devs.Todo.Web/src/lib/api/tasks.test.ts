import { describe, it, expect, vi } from 'vitest';
import {
	listTasks,
	createTask,
	getTask,
	updateTaskStatus,
	updateTask,
	reopenTask,
	deleteTask,
	ApiError,
	type Task
} from './tasks';

function mockOk<T>(data: T, contentType = 'application/json') {
	return vi.fn().mockResolvedValue({
		ok: true,
		json: () => Promise.resolve(data),
		headers: new Headers({ 'content-type': contentType })
	}) as unknown as typeof fetch;
}

function mockError(status: number, problem: object) {
	return vi.fn().mockResolvedValue({
		ok: false,
		status,
		statusText: 'Error',
		json: () => Promise.resolve(problem),
		headers: new Headers({ 'content-type': 'application/problem+json' })
	}) as unknown as typeof fetch;
}

const BASE = 'http://localhost:5001';

describe('listTasks', () => {
	it('returns tasks from the API', async () => {
		const expected: Task[] = [
			{
				id: '1',
				title: 'Write tests',
				description: null,
				status: 'Todo',
				difficulty: 'Normal',
				priority: 'Medium',
				estimatedMinutes: null,
				dueDate: null,
				completedAt: null,
				scheduledDate: null
			},
			{
				id: '2',
				title: 'Ship feature',
				description: null,
				status: 'InProgress',
				difficulty: 'Normal',
				priority: 'Medium',
				estimatedMinutes: null,
				dueDate: null,
				completedAt: null,
				scheduledDate: null
			}
		];

		const result = await listTasks(mockOk(expected), BASE);
		expect(result).toEqual(expected);
	});

	it('throws ApiError with ProblemDetails on error response', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Validation failed',
			status: 400,
			detail: 'Invalid status filter'
		};

		await expect(listTasks(mockError(400, problem), BASE)).rejects.toThrow(ApiError);
	});
});

describe('createTask', () => {
	it('creates a task and returns it', async () => {
		const created: Task = {
			id: '1',
			title: 'New task',
			description: null,
			status: 'Todo',
			difficulty: 'Normal',
			priority: 'Medium',
			estimatedMinutes: null,
			dueDate: null,
			completedAt: null,
			scheduledDate: null
		};
		const fetchMock = mockOk(created);

		const result = await createTask(fetchMock, BASE, 'New task');

		expect(result).toEqual(created);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks`), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ title: 'New task' })
		});
	});

	it('throws ApiError with validation errors on empty title', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Validation failed',
			status: 400,
			detail: 'Validation failed.',
			errors: { Title: ['Title is required.'] }
		};

		try {
			await createTask(mockError(400, problem), BASE, '');
			expect.fail('Should have thrown');
		} catch (e) {
			expect(e).toBeInstanceOf(ApiError);
			expect((e as ApiError).problem.errors?.Title).toContain('Title is required.');
		}
	});
});

describe('updateTaskStatus', () => {
	it('updates task status and returns updated task', async () => {
		const updated: Task = {
			id: '1',
			title: 'My task',
			description: null,
			status: 'InProgress',
			difficulty: 'Normal',
			priority: 'Medium',
			estimatedMinutes: null,
			dueDate: null,
			completedAt: null,
			scheduledDate: null
		};
		const fetchMock = mockOk(updated);

		const result = await updateTaskStatus(fetchMock, BASE, '1', 'InProgress');

		expect(result).toEqual(updated);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks/1/status`), {
			method: 'PATCH',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ status: 'InProgress' })
		});
	});

	it('throws ApiError on conflict', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Conflict',
			status: 409,
			detail: "Task is already in status 'Todo'."
		};

		await expect(updateTaskStatus(mockError(409, problem), BASE, '1', 'Todo')).rejects.toThrow(
			ApiError
		);
	});
});

describe('deleteTask', () => {
	it('deletes a task successfully', async () => {
		const fetchMock = vi.fn().mockResolvedValue({
			ok: true,
			headers: new Headers()
		}) as unknown as typeof fetch;

		await expect(deleteTask(fetchMock, BASE, '1')).resolves.toBeUndefined();
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks/1`), {
			method: 'DELETE'
		});
	});

	it('throws ApiError on not found', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Resource not found',
			status: 404,
			detail: 'Task not found'
		};

		await expect(deleteTask(mockError(404, problem), BASE, 'xyz')).rejects.toThrow(ApiError);
	});
});

describe('updateTask', () => {
	it('updates task fields and returns updated task', async () => {
		const updated: Task = {
			id: '1',
			title: 'Updated',
			description: 'New desc',
			status: 'Todo',
			difficulty: 'Hard',
			priority: 'Medium',
			estimatedMinutes: null,
			dueDate: null,
			completedAt: null,
			scheduledDate: null
		};
		const fetchMock = mockOk(updated);

		const result = await updateTask(fetchMock, BASE, '1', {
			title: 'Updated',
			description: 'New desc'
		});

		expect(result).toEqual(updated);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks/1`), {
			method: 'PATCH',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ title: 'Updated', description: 'New desc' })
		});
	});

	it('throws ApiError on not found', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Resource not found',
			status: 404,
			detail: 'Task not found'
		};

		await expect(
			updateTask(mockError(404, problem), BASE, 'xyz', { title: 'X' })
		).rejects.toThrow(ApiError);
	});
});

describe('reopenTask', () => {
	it('reopens a completed task', async () => {
		const reopened: Task = {
			id: '1',
			title: 'Reopened',
			description: null,
			status: 'Todo',
			difficulty: 'Normal',
			priority: 'Medium',
			estimatedMinutes: null,
			dueDate: null,
			completedAt: null,
			scheduledDate: null
		};
		const fetchMock = mockOk(reopened);

		const result = await reopenTask(fetchMock, BASE, '1');

		expect(result).toEqual(reopened);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks/1/reopen`), {
			method: 'PATCH'
		});
	});

	it('throws ApiError on conflict', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Conflict',
			status: 409,
			detail: 'Task is not completed'
		};

		await expect(reopenTask(mockError(409, problem), BASE, '1')).rejects.toThrow(ApiError);
	});
});

describe('getTask', () => {
	it('fetches a single task by id', async () => {
		const expected: Task = {
			id: 'abc',
			title: 'Single task',
			description: null,
			status: 'Todo',
			difficulty: 'Normal',
			priority: 'Medium',
			estimatedMinutes: null,
			dueDate: null,
			completedAt: null,
			scheduledDate: null
		};
		const fetchMock = mockOk(expected);

		const result = await getTask(fetchMock, BASE, 'abc');

		expect(result).toEqual(expected);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/tasks/abc`));
	});

	it('throws ApiError on not found', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Resource not found',
			status: 404,
			detail: 'Task not found'
		};

		await expect(getTask(mockError(404, problem), BASE, 'xyz')).rejects.toThrow(ApiError);
	});
});
