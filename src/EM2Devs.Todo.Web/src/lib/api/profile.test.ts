import { describe, it, expect, vi } from 'vitest';
import { getProfile, type PlayerProfile } from './profile';
import { ApiError } from './tasks';

const BASE = 'http://localhost:5001';

describe('getProfile', () => {
	it('returns profile data from the API', async () => {
		const expected: PlayerProfile = {
			totalXp: 150,
			level: 3,
			xpToNextLevel: 50,
			currentStreak: 5,
			longestStreak: 12,
			lastXpBreakdown: null,
			xpHistory: [],
			titles: { earned: [], active: null, progress: [] },
			skillTrees: []
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected),
			headers: new Headers({ 'content-type': 'application/json' })
		}) as unknown as typeof fetch;

		const result = await getProfile(mockFetch, BASE);

		expect(mockFetch).toHaveBeenCalledWith(new URL(`${BASE}/api/profile`));
		expect(result).toEqual(expected);
	});

	it('throws ApiError on error response', async () => {
		const problem = {
			type: 'https://tools.ietf.org/html/rfc9457',
			title: 'Internal Server Error',
			status: 500,
			detail: 'Something went wrong'
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 500,
			statusText: 'Internal Server Error',
			json: () => Promise.resolve(problem),
			headers: new Headers({ 'content-type': 'application/problem+json' })
		}) as unknown as typeof fetch;

		await expect(getProfile(mockFetch, BASE)).rejects.toThrow(ApiError);
	});

	it('returns profile with XP breakdown when present', async () => {
		const expected: PlayerProfile = {
			totalXp: 180,
			level: 3,
			xpToNextLevel: 20,
			currentStreak: 7,
			longestStreak: 14,
			lastXpBreakdown: {
				baseXp: 30,
				deadlineModifier: 1.2,
				streakMultiplier: 1.14,
				finalXp: 41
			},
			xpHistory: [
				{ date: '2026-04-10', xpEarned: 40, source: 'task-completed', cumulativeTotal: 140 },
				{ date: '2026-04-12', xpEarned: 40, source: 'task-completed', cumulativeTotal: 180 }
			],
			titles: {
				earned: [{ type: 'EarlyBird', displayName: 'Early Bird', earnedOn: '2026-04-05' }],
				active: 'EarlyBird',
				progress: []
			},
			skillTrees: [
				{
					type: 'Scholar',
					tier: 1,
					tasksCompletedInTier: 5,
					tasksToNextTier: 25,
					unlockHint: null,
					perks: [{ tier: 1, perkType: 'Tips', description: 'Personalised Scholar tips' }]
				}
			]
		};

		const mockFetch = vi.fn().mockResolvedValue({
			ok: true,
			json: () => Promise.resolve(expected),
			headers: new Headers({ 'content-type': 'application/json' })
		}) as unknown as typeof fetch;

		const result = await getProfile(mockFetch, BASE);

		expect(result.lastXpBreakdown).not.toBeNull();
		expect(result.lastXpBreakdown!.baseXp).toBe(30);
		expect(result.lastXpBreakdown!.finalXp).toBe(41);
	});

	it('throws ApiError on non-JSON error response', async () => {
		const mockFetch = vi.fn().mockResolvedValue({
			ok: false,
			status: 502,
			statusText: 'Bad Gateway',
			headers: new Headers({ 'content-type': 'text/html' })
		}) as unknown as typeof fetch;

		await expect(getProfile(mockFetch, BASE)).rejects.toThrow(ApiError);
	});
});
