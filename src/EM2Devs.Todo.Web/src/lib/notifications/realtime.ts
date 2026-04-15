import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';
import type { Notification } from '$lib/api/notifications';

/**
 * Shape of the payload pushed by the API over SignalR. Matches the
 * {@link Notification} REST response so the dashboard can merge without mapping.
 */
export type RealtimeNotification = Notification;

const HUB_PATH = '/hubs/notifications';
const EVENT_NAME = 'notificationCreated';

export interface ConnectOptions {
	/** Absolute or relative base URL of the API. Defaults to the current origin. */
	baseUrl?: string;
	/** Optional builder override for tests. */
	builder?: () => HubConnectionBuilder;
}

/**
 * Opens a SignalR connection to the notifications hub and subscribes to push
 * events. The bearer token is supplied via the access-token factory — SignalR
 * appends it as `?access_token=` because browsers cannot set an Authorization
 * header on WebSocket upgrades.
 */
export async function connectNotifications(
	token: string,
	onNotification: (notification: RealtimeNotification) => void,
	options: ConnectOptions = {}
): Promise<HubConnection> {
	const url = options.baseUrl ? new URL(HUB_PATH, options.baseUrl).toString() : HUB_PATH;
	const builder = options.builder ? options.builder() : new HubConnectionBuilder();
	const connection = builder
		.withUrl(url, { accessTokenFactory: () => token })
		.withAutomaticReconnect()
		.configureLogging(LogLevel.Warning)
		.build();

	connection.on(EVENT_NAME, onNotification);
	await connection.start();
	return connection;
}

/** Stops the connection, swallowing errors so disconnect is idempotent. */
export async function disconnectNotifications(connection: HubConnection | null): Promise<void> {
	if (!connection) return;
	try {
		await connection.stop();
	} catch {
		// Best-effort teardown — the underlying transport may already be torn down.
	}
}
