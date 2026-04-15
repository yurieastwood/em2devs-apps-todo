import { describe, it, expect, vi } from 'vitest';
import { listNotifications, markRead, dismiss, type Notification } from './notifications';

const BASE = 'http://localhost:5001';

const sample: Notification = {
	id: '11111111-1111-1111-1111-111111111111',
	type: 'AchievementAlert',
	message: 'Level up!',
	createdAt: '2026-04-12T00:00:00Z',
	status: 'Unread',
	readAt: null
};

function mockJson<T>(body: T) {
	return vi.fn().mockResolvedValue({
		ok: true,
		json: () => Promise.resolve(body),
		headers: new Headers({ 'content-type': 'application/json' })
	}) as unknown as typeof fetch;
}

describe('listNotifications', () => {
	it('calls /api/notifications without includeRead by default', async () => {
		const mockFetch = mockJson<Notification[]>([sample]);
		const result = await listNotifications(mockFetch, BASE);
		expect(mockFetch).toHaveBeenCalledWith(new URL(`${BASE}/api/notifications`));
		expect(result).toEqual([sample]);
	});

	it('adds includeRead=true when requested', async () => {
		const mockFetch = mockJson<Notification[]>([]);
		await listNotifications(mockFetch, BASE, { includeRead: true });
		expect(mockFetch).toHaveBeenCalledWith(
			new URL(`${BASE}/api/notifications?includeRead=true`)
		);
	});
});

describe('markRead', () => {
	it('POSTs to /api/notifications/{id}/read', async () => {
		const mockFetch = mockJson<Notification>({
			...sample,
			status: 'Read',
			readAt: '2026-04-12T00:01:00Z'
		});
		const result = await markRead(mockFetch, BASE, sample.id);
		expect(mockFetch).toHaveBeenCalledWith(
			new URL(`${BASE}/api/notifications/${sample.id}/read`),
			{ method: 'POST' }
		);
		expect(result.status).toBe('Read');
	});
});

describe('dismiss', () => {
	it('POSTs to /api/notifications/{id}/dismiss', async () => {
		const mockFetch = mockJson<Notification>({ ...sample, status: 'Dismissed' });
		await dismiss(mockFetch, BASE, sample.id);
		expect(mockFetch).toHaveBeenCalledWith(
			new URL(`${BASE}/api/notifications/${sample.id}/dismiss`),
			{ method: 'POST' }
		);
	});
});
