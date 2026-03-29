import { describe, it, expect, vi } from 'vitest';
import { ApiError } from './tasks';
import {
	listEpics,
	createEpic,
	getEpic,
	assignQuestToEpic,
	removeQuestFromEpic,
	completeEpic,
	deleteEpic,
	type Epic
} from './epics';

const BASE = 'http://localhost:5001';

function mockFetch(data: unknown, status = 200) {
	return vi.fn().mockResolvedValue({
		ok: status >= 200 && status < 300,
		status,
		json: () => Promise.resolve(data)
	});
}

const sampleEpic: Epic = {
	id: '1',
	title: 'Launch MVP',
	description: 'Complete all MVP milestones',
	targetDate: null,
	progress: 50,
	isCompleted: false,
	quests: [{ id: 'q1', title: 'Onboarding', progress: 100 }]
};

describe('listEpics', () => {
	it('returns epics from the API', async () => {
		const fetch = mockFetch([sampleEpic]);
		const result = await listEpics(fetch as unknown as typeof globalThis.fetch, BASE);
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics'));
		expect(result).toEqual([sampleEpic]);
	});
});

describe('createEpic', () => {
	it('sends title and description', async () => {
		const fetch = mockFetch(sampleEpic);
		const result = await createEpic(
			fetch as unknown as typeof globalThis.fetch,
			BASE,
			'Launch MVP',
			'Complete all MVP milestones'
		);
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics'), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({
				title: 'Launch MVP',
				description: 'Complete all MVP milestones'
			})
		});
		expect(result).toEqual(sampleEpic);
	});
});

describe('getEpic', () => {
	it('fetches epic by ID', async () => {
		const fetch = mockFetch(sampleEpic);
		const result = await getEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1');
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics/1'));
		expect(result).toEqual(sampleEpic);
	});
});

describe('assignQuestToEpic', () => {
	it('posts quest ID to epic', async () => {
		const fetch = mockFetch(sampleEpic);
		await assignQuestToEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1', 'q2');
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics/1/quests'), {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ questId: 'q2' })
		});
	});
});

describe('removeQuestFromEpic', () => {
	it('deletes quest from epic', async () => {
		const fetch = mockFetch(sampleEpic);
		await removeQuestFromEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1', 'q1');
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics/1/quests/q1'), {
			method: 'DELETE'
		});
	});
});

describe('completeEpic', () => {
	it('posts to complete endpoint', async () => {
		const completed = { ...sampleEpic, isCompleted: true, progress: 100 };
		const fetch = mockFetch(completed);
		const result = await completeEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1');
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics/1/complete'), {
			method: 'POST'
		});
		expect(result.isCompleted).toBe(true);
	});
});

describe('deleteEpic', () => {
	it('deletes epic', async () => {
		const fetch = mockFetch(null, 204);
		await deleteEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1');
		expect(fetch).toHaveBeenCalledWith(new URL('http://localhost:5001/api/epics/1'), {
			method: 'DELETE'
		});
	});
});

describe('error handling', () => {
	const problemDetails = {
		type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
		title: 'Not Found',
		status: 404,
		detail: 'Epic not found'
	};

	function mockErrorFetch() {
		return vi.fn().mockResolvedValue({
			ok: false,
			status: 404,
			headers: new Headers({ 'content-type': 'application/problem+json' }),
			json: () => Promise.resolve(problemDetails)
		});
	}

	it('throws ApiError on failed listEpics', async () => {
		const fetch = mockErrorFetch();
		await expect(listEpics(fetch as unknown as typeof globalThis.fetch, BASE)).rejects.toThrow(
			ApiError
		);
	});

	it('throws ApiError on failed getEpic', async () => {
		const fetch = mockErrorFetch();
		await expect(
			getEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1')
		).rejects.toThrow(ApiError);
	});

	it('throws ApiError on failed createEpic', async () => {
		const fetch = mockErrorFetch();
		await expect(
			createEpic(fetch as unknown as typeof globalThis.fetch, BASE, 'title', 'desc')
		).rejects.toThrow(ApiError);
	});

	it('throws ApiError on failed deleteEpic', async () => {
		const fetch = mockErrorFetch();
		await expect(
			deleteEpic(fetch as unknown as typeof globalThis.fetch, BASE, '1')
		).rejects.toThrow(ApiError);
	});
});
