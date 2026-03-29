import { describe, it, expect, vi } from 'vitest';
import {
	listRecurringTasks,
	createRecurringTask,
	pauseRecurringTask,
	resumeRecurringTask,
	deleteRecurringTask,
	type RecurringTask
} from './recurring';
import { ApiError } from './tasks';

const BASE = 'http://localhost:5001';

function mockOk<T>(data: T) {
	return vi.fn().mockResolvedValue({
		ok: true,
		json: () => Promise.resolve(data),
		headers: new Headers({ 'content-type': 'application/json' })
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

describe('listRecurringTasks', () => {
	it('Should_ReturnTasks_When_ApiReturnsData', async () => {
		const expected: RecurringTask[] = [
			{ id: '1', title: 'Daily standup', pattern: 'Daily', isActive: true }
		];

		const result = await listRecurringTasks(mockOk(expected), BASE);
		expect(result).toEqual(expected);
	});
});

describe('createRecurringTask', () => {
	it('Should_ReturnCreatedTask_When_ValidDataProvided', async () => {
		const created: RecurringTask = {
			id: '1',
			title: 'Weekly review',
			pattern: 'Weekly',
			isActive: true
		};
		const fetchMock = mockOk(created);

		const result = await createRecurringTask(fetchMock, BASE, 'Weekly review', 'Weekly');

		expect(result).toEqual(created);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/recurring-tasks`), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ title: 'Weekly review', pattern: 'Weekly' })
		});
	});

	it('Should_ThrowApiError_When_PatternInvalid', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Validation failed',
			status: 400,
			detail: 'Invalid pattern'
		};

		await expect(
			createRecurringTask(mockError(400, problem), BASE, 'Bad', 'Hourly')
		).rejects.toThrow(ApiError);
	});
});

describe('pauseRecurringTask', () => {
	it('Should_ReturnPausedTask_When_Paused', async () => {
		const paused: RecurringTask = {
			id: '1',
			title: 'Task',
			pattern: 'Daily',
			isActive: false
		};
		const fetchMock = mockOk(paused);

		const result = await pauseRecurringTask(fetchMock, BASE, '1');

		expect(result.isActive).toBe(false);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/recurring-tasks/1/pause`), {
			method: 'PATCH'
		});
	});
});

describe('resumeRecurringTask', () => {
	it('Should_ReturnActiveTask_When_Resumed', async () => {
		const resumed: RecurringTask = {
			id: '1',
			title: 'Task',
			pattern: 'Daily',
			isActive: true
		};
		const fetchMock = mockOk(resumed);

		const result = await resumeRecurringTask(fetchMock, BASE, '1');

		expect(result.isActive).toBe(true);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/recurring-tasks/1/resume`), {
			method: 'PATCH'
		});
	});
});

describe('deleteRecurringTask', () => {
	it('Should_SucceedSilently_When_Deleted', async () => {
		const fetchMock = vi.fn().mockResolvedValue({
			ok: true,
			headers: new Headers()
		}) as unknown as typeof fetch;

		await expect(deleteRecurringTask(fetchMock, BASE, '1')).resolves.toBeUndefined();
	});

	it('Should_ThrowApiError_When_NotFound', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Not found',
			status: 404,
			detail: 'Not found'
		};

		await expect(deleteRecurringTask(mockError(404, problem), BASE, 'xyz')).rejects.toThrow(
			ApiError
		);
	});
});
