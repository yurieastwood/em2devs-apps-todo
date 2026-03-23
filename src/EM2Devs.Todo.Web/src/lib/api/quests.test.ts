import { describe, it, expect, vi } from 'vitest';
import { listQuests, createQuest, getQuest, deleteQuest, type Quest } from './quests';

const BASE = 'http://localhost:5001';

function mockOk<T>(data: T) {
	return vi.fn().mockResolvedValue({
		ok: true,
		json: () => Promise.resolve(data),
		headers: new Headers({ 'content-type': 'application/json' })
	}) as unknown as typeof fetch;
}

function mockStatus(status: number) {
	return vi.fn().mockResolvedValue({
		ok: status >= 200 && status < 300,
		status,
		headers: new Headers()
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
});

describe('deleteQuest', () => {
	it('Should_SucceedSilently_When_QuestDeleted', async () => {
		await expect(deleteQuest(mockStatus(204), BASE, '1')).resolves.toBeUndefined();
	});

	it('Should_ThrowError_When_QuestNotFound', async () => {
		await expect(deleteQuest(mockStatus(404), BASE, 'xyz')).rejects.toThrow();
	});
});
