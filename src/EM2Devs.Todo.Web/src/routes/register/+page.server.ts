import { fail, redirect } from '@sveltejs/kit';
import { register } from '$lib/api/auth';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions } from './$types';

export const actions: Actions = {
	default: async ({ request, fetch, cookies }) => {
		const formData = await request.formData();
		const email = formData.get('email')?.toString()?.trim() ?? '';
		const password = formData.get('password')?.toString() ?? '';
		const displayName = formData.get('displayName')?.toString()?.trim() ?? '';

		if (!email || !password || !displayName) {
			return fail(400, {
				email,
				displayName,
				error: 'Email, password, and display name are required.'
			});
		}

		let authResponse;
		try {
			authResponse = await register(fetch, getBaseUrl(), email, password, displayName);
		} catch (e) {
			const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Registration failed.')
					: 'Registration failed. Please try again.';
			return fail(status, { email, displayName, error: message });
		}

		const expiresAt = new Date(authResponse.expiresAt);
		cookies.set(TOKEN_COOKIE, authResponse.token, {
			path: '/',
			httpOnly: true,
			sameSite: 'lax',
			expires: expiresAt
		});

		throw redirect(303, '/');
	}
};
