import { describe, it, expect, vi } from 'vitest';
import { listTasks, type Task } from './tasks';

describe('listTasks', () => {
	it('returns tasks from the API', async () => {
		const expected: Task[] = [
			{ id: '1', title: 'Write tests', status: 'Todo' },
			{ id: '2', title: 'Ship feature', status: 'InProgress' }
		];

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected)
		});

		const result = await listTasks(
			mockFetch as unknown as typeof fetch,
			'http://localhost:5001'
		);

		expect(mockFetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/tasks'));
		expect(result).toEqual(expected);
	});

	it('throws on non-ok response', async () => {
		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 500,
			statusText: 'Internal Server Error'
		});

		await expect(
			listTasks(mockFetch as unknown as typeof fetch, 'http://localhost:5001')
		).rejects.toThrow('Failed to fetch tasks: 500 Internal Server Error');
	});
});
