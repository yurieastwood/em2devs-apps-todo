import { fail, redirect } from '@sveltejs/kit';
import { login } from '$lib/api/auth';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions } from './$types';

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
			const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Invalid email or password.')
					: 'Login failed. Please try again.';
			return fail(status === 401 ? 401 : status, { email, error: message });
		}

		const expiresAt = new Date(authResponse.expiresAt);
		cookies.set(TOKEN_COOKIE, authResponse.token, {
			path: '/',
			httpOnly: true,
			sameSite: 'lax',
			secure: url.protocol === 'https:',
			expires: expiresAt
		});

		throw redirect(303, '/');
	}
};
