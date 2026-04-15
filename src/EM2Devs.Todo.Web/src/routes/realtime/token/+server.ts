import { error, json } from '@sveltejs/kit';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../../hooks.server';
import type { RequestHandler } from './$types';

/**
 * Returns the current user's JWT so the browser can open a SignalR connection
 * to the API's notifications hub. SignalR attaches the token as
 * `?access_token=` on the WebSocket/SSE handshake because browsers cannot set
 * custom Authorization headers on upgrade requests.
 *
 * Safe-by-construction: the hooks layer has already redirected unauthenticated
 * users, and the cookie itself is httpOnly — this endpoint is the single
 * controlled exit point for the token into JavaScript.
 */
export const GET: RequestHandler = ({ cookies }) => {
	const token = cookies.get(TOKEN_COOKIE);
	if (!token) {
		throw error(401, 'Not authenticated');
	}
	return json({ token, baseUrl: getBaseUrl() });
};
