import { describe, it, expect, vi } from 'vitest';
import { getDailyBrief, type DailyBrief } from './dailyBrief';
import { ApiError } from './tasks';

const BASE = 'http://localhost:5001';

describe('getDailyBrief', () => {
	it('returns the daily brief from the API', async () => {
		const expected: DailyBrief = {
			date: '2026-04-12',
			greeting: 'Good morning, Demo User',
			currentStreakDays: 3,
			corePlanCount: 2,
			ifTimeAllowsCount: 0,
			overdueCount: 1,
			corePlan: [
				{
					id: '11111111-1111-1111-1111-111111111111',
					title: 'Review quarterly plan',
					difficulty: 'Normal',
					priority: 'Medium',
					estimatedMinutes: 30,
					calibratedMinutes: 42,
					scheduledDate: '2026-04-12'
				},
				{
					id: '22222222-2222-2222-2222-222222222222',
					title: 'Respond to overdue email',
					difficulty: 'Easy',
					priority: 'High',
					estimatedMinutes: 10,
					calibratedMinutes: 14,
					scheduledDate: '2026-04-11'
				}
			],
			ifTimeAllows: [],
			overdue: [
				{
					id: '22222222-2222-2222-2222-222222222222',
					title: 'Respond to overdue email',
					difficulty: 'Easy',
					priority: 'High',
					estimatedMinutes: 10,
					calibratedMinutes: 14,
					scheduledDate: '2026-04-11'
				}
			],
			status: 'Available'
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected),
			headers: new Headers({ 'content-type': 'application/json' })
		}) as unknown as typeof fetch;

		const result = await getDailyBrief(mockFetch, BASE);

		expect(mockFetch).toHaveBeenCalledWith(new URL(`${BASE}/api/daily-brief`));
		expect(result).toEqual(expected);
	});

	it('throws ApiError on error response', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Unauthorized',
			status: 401,
			detail: 'Missing bearer token'
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 401,
			statusText: 'Unauthorized',
			json: () => Promise.resolve(problem),
			headers: new Headers({ 'content-type': 'application/problem+json' })
		}) as unknown as typeof fetch;

		await expect(getDailyBrief(mockFetch, BASE)).rejects.toThrow(ApiError);
	});
});
