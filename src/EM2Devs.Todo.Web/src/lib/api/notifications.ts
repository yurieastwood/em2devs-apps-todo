import { handleResponse } from './tasks';

export type NotificationStatus = 'Unread' | 'Read' | 'Dismissed';

export interface Notification {
	id: string;
	type: string;
	message: string;
	createdAt: string;
	status: NotificationStatus;
	readAt: string | null;
}

export interface ListNotificationsOptions {
	includeRead?: boolean;
}

export async function listNotifications(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	options: ListNotificationsOptions = {}
): Promise<Notification[]> {
	const url = new URL('/api/notifications', baseUrl);
	if (options.includeRead) {
		url.searchParams.set('includeRead', 'true');
	}
	const response = await fetch(url);
	return handleResponse<Notification[]>(response);
}

export async function markRead(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	id: string
): Promise<Notification> {
	const url = new URL(`/api/notifications/${id}/read`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	return handleResponse<Notification>(response);
}

export async function dismiss(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	id: string
): Promise<Notification> {
	const url = new URL(`/api/notifications/${id}/dismiss`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	return handleResponse<Notification>(response);
}
