import { describe, expect, it, vi } from 'vitest';
import {
	connectNotifications,
	disconnectNotifications,
	type RealtimeNotification
} from './realtime';

function buildFakeBuilder() {
	const handlers = new Map<string, (payload: RealtimeNotification) => void>();
	const connection = {
		on: vi.fn((event: string, cb: (payload: RealtimeNotification) => void) => {
			handlers.set(event, cb);
		}),
		start: vi.fn(() => Promise.resolve()),
		stop: vi.fn(() => Promise.resolve()),
		invoke: vi.fn(),
		off: vi.fn()
	};

	const builder = {
		withUrl: vi.fn(function (this: unknown) {
			return this;
		}),
		withAutomaticReconnect: vi.fn(function (this: unknown) {
			return this;
		}),
		configureLogging: vi.fn(function (this: unknown) {
			return this;
		}),
		build: vi.fn(() => connection)
	};

	return { builder, connection, handlers };
}

describe('connectNotifications', () => {
	it('starts the connection and wires the notificationCreated handler', async () => {
		const { builder, connection, handlers } = buildFakeBuilder();
		const onNotification = vi.fn();

		const result = await connectNotifications('jwt-token', onNotification, {
			baseUrl: 'http://api.example',
			builder: () => builder as unknown as import('@microsoft/signalr').HubConnectionBuilder
		});

		expect(builder.withUrl).toHaveBeenCalledWith(
			'http://api.example/hubs/notifications',
			expect.objectContaining({ accessTokenFactory: expect.any(Function) })
		);
		const call = builder.withUrl.mock.calls[0] as unknown as [
			string,
			{ accessTokenFactory: () => string }
		];
		expect(call[1].accessTokenFactory()).toBe('jwt-token');
		expect(connection.start).toHaveBeenCalledOnce();

		const pushed: RealtimeNotification = {
			id: '11111111-1111-1111-1111-111111111111',
			type: 'AchievementAlert',
			message: 'Level up!',
			createdAt: new Date().toISOString(),
			status: 'Unread',
			readAt: null
		};
		handlers.get('notificationCreated')?.(pushed);
		expect(onNotification).toHaveBeenCalledWith(pushed);

		expect(result).toBe(connection);
	});

	it('disconnect is a no-op on null', async () => {
		await expect(disconnectNotifications(null)).resolves.toBeUndefined();
	});

	it('disconnect swallows stop errors', async () => {
		const connection = { stop: vi.fn(() => Promise.reject(new Error('boom'))) };
		await expect(
			disconnectNotifications(
				connection as unknown as import('@microsoft/signalr').HubConnection
			)
		).resolves.toBeUndefined();
		expect(connection.stop).toHaveBeenCalledOnce();
	});
});
