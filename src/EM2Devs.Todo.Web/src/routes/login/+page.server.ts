import { fail, redirect, type Cookies } from '@sveltejs/kit';
import { login } from '$lib/api/auth';
import { recoverAccount } from '$lib/api/account';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions } from './$types';

const DEACTIVATED_DETAIL = 'Account has been deactivated.';

function setTokenCookie(cookies: Cookies, url: URL, token: string, expiresAt: Date) {
	cookies.set(TOKEN_COOKIE, token, {
		path: '/',
		httpOnly: true,
		sameSite: 'lax',
		secure: url.protocol === 'https:',
		expires: expiresAt
	});
}

export const actions: Actions = {
	default: async ({ request, fetch, cookies, url }) => {
		const formData = await request.formData();
		const email = formData.get('email')?.toString()?.trim() ?? '';
		const password = formData.get('password')?.toString() ?? '';

		if (!email || !password) {
			return fail(400, { email, error: 'Email and password are required.' });
		}

		let authResponse;
		try {
			authResponse = await login(fetch, getBaseUrl(), email, password);
		} catch (e) {
			if (e instanceof ApiError && e.problem.detail === DEACTIVATED_DETAIL) {
				return fail(401, {
					email,
					error: DEACTIVATED_DETAIL,
					deactivated: true
				});
			}

			const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Invalid email or password.')
					: 'Login failed. Please try again.';
			return fail(status === 401 ? 401 : status, { email, error: message });
		}

		setTokenCookie(cookies, url, authResponse.token, new Date(authResponse.expiresAt));
		throw redirect(303, '/');
	},

	recover: async ({ request, fetch, cookies, url }) => {
		const formData = await request.formData();
		const email = formData.get('email')?.toString()?.trim() ?? '';
		const password = formData.get('password')?.toString() ?? '';

		if (!email || !password) {
			return fail(400, {
				email,
				error: 'Email and password are required.',
				deactivated: true
			});
		}

		let authResponse;
		try {
			authResponse = await recoverAccount(fetch, getBaseUrl(), email, password);
		} catch (e) {
			const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Recovery failed.')
					: 'Recovery failed. Please try again.';
			return fail(status === 401 || status === 409 ? status : 500, {
				email,
				error: message,
				// Stay in recovery mode if the API still considers the account recoverable
				// (401 = wrong creds for a deactivated account). Drop back to normal login
				// if the API says the account isn't deactivated or holding period elapsed (409).
				deactivated: status !== 409
			});
		}

		setTokenCookie(cookies, url, authResponse.token, new Date(authResponse.expiresAt));
		throw redirect(303, '/');
	}
};
