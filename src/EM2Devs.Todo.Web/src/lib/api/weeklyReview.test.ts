import { describe, it, expect, vi } from 'vitest';
import { getWeeklyReview, saveWeeklyReview, type WeeklyReview } from './weeklyReview';
import { ApiError } from './tasks';

const BASE = 'http://localhost:5001';

describe('getWeeklyReview', () => {
	it('returns the weekly review from the API', async () => {
		const expected: WeeklyReview = {
			weekOf: '2026-04-12',
			tasksCompleted: 7,
			xpEarned: 420,
			streakStart: 4,
			streakEnd: 11,
			notableEvents: ['Completed 7 task(s) this week'],
			reflection: null
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected),
			headers: new Headers({ 'content-type': 'application/json' })
		}) as unknown as typeof fetch;

		const result = await getWeeklyReview(mockFetch, BASE);

		expect(mockFetch).toHaveBeenCalledWith(new URL(`${BASE}/api/weekly-review`));
		expect(result).toEqual(expected);
	});

	it('passes the weekOf query parameter when provided', async () => {
		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () =>
				Promise.resolve({
					weekOf: '2026-04-05',
					tasksCompleted: 0,
					xpEarned: 0,
					streakStart: 0,
					streakEnd: 0,
					notableEvents: [],
					reflection: null
				} satisfies WeeklyReview),
			headers: new Headers({ 'content-type': 'application/json' })
		});

		await getWeeklyReview(mockFetch as unknown as typeof fetch, BASE, '2026-04-05');

		const called = mockFetch.mock.calls[0][0] as URL;
		expect(called.searchParams.get('weekOf')).toBe('2026-04-05');
	});

	it('throws ApiError on 401', async () => {
		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 401,
			statusText: 'Unauthorized',
			json: () => Promise.resolve({ title: 'Unauthorized', status: 401 }),
			headers: new Headers({ 'content-type': 'application/problem+json' })
		}) as unknown as typeof fetch;

		await expect(getWeeklyReview(mockFetch, BASE)).rejects.toThrow(ApiError);
	});
});

describe('saveWeeklyReview', () => {
	it('POSTs the reflection and returns the saved payload', async () => {
		const expected = {
			whatWentWell: 'went well',
			whatDragged: 'dragged',
			adjustment: 'adjust',
			savedAt: '2026-04-15T20:00:00Z'
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected),
			headers: new Headers({ 'content-type': 'application/json' })
		}) as unknown as typeof fetch;

		const result = await saveWeeklyReview(mockFetch, BASE, {
			whatWentWell: 'went well',
			whatDragged: 'dragged',
			adjustment: 'adjust'
		});

		expect(mockFetch).toHaveBeenCalledWith(
			new URL(`${BASE}/api/weekly-review`),
			expect.objectContaining({ method: 'POST' })
		);
		expect(result).toEqual(expected);
	});

	it('throws ApiError on 400', async () => {
		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 400,
			statusText: 'Bad Request',
			json: () => Promise.resolve({ title: 'Bad Request', status: 400 }),
			headers: new Headers({ 'content-type': 'application/problem+json' })
		}) as unknown as typeof fetch;

		await expect(
			saveWeeklyReview(mockFetch, BASE, { whatWentWell: '', whatDragged: '', adjustment: '' })
		).rejects.toThrow(ApiError);
	});
});
