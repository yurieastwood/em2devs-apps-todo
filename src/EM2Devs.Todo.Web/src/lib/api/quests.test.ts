import { describe, it, expect, vi } from 'vitest';
import {
	listQuests,
	createQuest,
	getQuest,
	deleteQuest,
	addTaskToQuest,
	removeTaskFromQuest,
	type Quest
} from './quests';
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

describe('listQuests', () => {
	it('Should_ReturnQuests_When_ApiReturnsData', async () => {
		const expected: Quest[] = [
			{
				id: '1',
				title: 'Test Quest',
				description: 'Desc',
				dueDate: null,
				progress: 50,
				tasks: []
			}
		];

		const result = await listQuests(mockOk(expected), BASE);
		expect(result).toEqual(expected);
	});

	it('Should_ThrowApiError_When_ListFails', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Server error',
			status: 500,
			detail: 'Something went wrong'
		};

		await expect(listQuests(mockError(500, problem), BASE)).rejects.toThrow(ApiError);
	});
});

describe('createQuest', () => {
	it('Should_ReturnCreatedQuest_When_ValidDataProvided', async () => {
		const created: Quest = {
			id: '1',
			title: 'New Quest',
			description: 'New desc',
			dueDate: null,
			progress: 0,
			tasks: []
		};
		const fetchMock = mockOk(created);

		const result = await createQuest(fetchMock, BASE, 'New Quest', 'New desc');

		expect(result).toEqual(created);
		expect(fetchMock).toHaveBeenCalledWith(new URL(`${BASE}/api/quests`), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ title: 'New Quest', description: 'New desc' })
		});
	});

	it('Should_ThrowApiError_When_ValidationFails', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Validation failed',
			status: 400,
			detail: 'Title cannot be empty'
		};

		await expect(createQuest(mockError(400, problem), BASE, '', '')).rejects.toThrow(ApiError);
	});
});

describe('getQuest', () => {
	it('Should_ReturnQuest_When_QuestExists', async () => {
		const quest: Quest = {
			id: '1',
			title: 'Quest',
			description: 'Desc',
			dueDate: null,
			progress: 0,
			tasks: []
		};

		const result = await getQuest(mockOk(quest), BASE, '1');
		expect(result).toEqual(quest);
	});

	it('Should_ThrowApiError_When_QuestNotFound', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Not found',
			status: 404,
			detail: 'Quest not found'
		};

		await expect(getQuest(mockError(404, problem), BASE, 'xyz')).rejects.toThrow(ApiError);
	});
});

describe('deleteQuest', () => {
	it('Should_SucceedSilently_When_QuestDeleted', async () => {
		const fetchMock = vi.fn().mockResolvedValue({
			ok: true,
			headers: new Headers()
		}) as unknown as typeof fetch;

		await expect(deleteQuest(fetchMock, BASE, '1')).resolves.toBeUndefined();
	});

	it('Should_ThrowApiError_When_QuestNotFound', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Not found',
			status: 404,
			detail: 'Quest not found'
		};

		await expect(deleteQuest(mockError(404, problem), BASE, 'xyz')).rejects.toThrow(ApiError);
	});
});

describe('addTaskToQuest', () => {
	it('Should_ReturnUpdatedQuest_When_TaskAdded', async () => {
		const quest: Quest = {
			id: '1',
			title: 'Quest',
			description: 'Desc',
			dueDate: null,
			progress: 0,
			tasks: [{ id: 't1', title: 'Added task', status: 'Todo' }]
		};

		const result = await addTaskToQuest(mockOk(quest), BASE, '1', 't1');
		expect(result.tasks).toHaveLength(1);
	});

	it('Should_ThrowApiError_When_TaskAlreadyAssigned', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Conflict',
			status: 409,
			detail: 'Task is already assigned'
		};

		await expect(addTaskToQuest(mockError(409, problem), BASE, '1', 't1')).rejects.toThrow(
			ApiError
		);
	});
});

describe('removeTaskFromQuest', () => {
	it('Should_ReturnUpdatedQuest_When_TaskRemoved', async () => {
		const quest: Quest = {
			id: '1',
			title: 'Quest',
			description: 'Desc',
			dueDate: null,
			progress: 0,
			tasks: []
		};

		const result = await removeTaskFromQuest(mockOk(quest), BASE, '1', 't1');
		expect(result.tasks).toHaveLength(0);
	});

	it('Should_ThrowApiError_When_TaskNotAssigned', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Not found',
			status: 404,
			detail: 'Task not assigned to quest'
		};

		await expect(
			removeTaskFromQuest(mockError(404, problem), BASE, '1', 'xyz')
		).rejects.toThrow(ApiError);
	});
});
