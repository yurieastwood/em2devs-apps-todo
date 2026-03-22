import { describe, it, expect, vi } from 'vitest';
import {
	listTasks,
	createTask,
	updateTaskStatus,
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
			{ id: '1', title: 'Write tests', status: 'Todo' },
			{ id: '2', title: 'Ship feature', status: 'InProgress' }
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
		const created: Task = { id: '1', title: 'New task', status: 'Todo' };
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
		const updated: Task = { id: '1', title: 'My task', status: 'InProgress' };
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
