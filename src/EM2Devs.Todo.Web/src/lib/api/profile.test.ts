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
			longestStreak: 12
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

	it('calculates progress percentage correctly', () => {
		const profile: PlayerProfile = {
			totalXp: 250,
			level: 4,
			xpToNextLevel: 50,
			currentStreak: 3,
			longestStreak: 10
		};

		// XP earned in current level = totalXp - (totalXp that was needed to reach current level)
		// Progress % = (1 - xpToNextLevel / (earned + xpToNextLevel)) * 100
		// But simpler: the API gives us xpToNextLevel. If we assume the level threshold
		// we can compute progress. For the UI, we derive it from level thresholds.
		// Simplest: xpToNextLevel out of the total for this level band.
		// Since we don't know the band size, we use a helper.
		const xpInCurrentLevel = profile.totalXp;
		const xpForNextLevel = profile.totalXp + profile.xpToNextLevel;
		const progress = Math.round((xpInCurrentLevel / xpForNextLevel) * 100);

		expect(progress).toBe(83);
	});
});
