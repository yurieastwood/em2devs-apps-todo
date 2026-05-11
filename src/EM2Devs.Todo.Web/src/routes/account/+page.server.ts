import { fail, redirect } from '@sveltejs/kit';
import { deleteAccount } from '$lib/api/account';
import { me } from '$lib/api/auth';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const profile = await me(fetch, getBaseUrl());
	if (!profile) {
		throw redirect(303, '/login');
	}
	return { user: profile };
};

export const actions: Actions = {
	delete: async ({ request, fetch, cookies }) => {
		const formData = await request.formData();
		const confirmation = formData.get('confirmation')?.toString() ?? '';

		if (confirmation !== 'DELETE MY ACCOUNT') {
			return fail(400, { error: 'Confirmation phrase must be "DELETE MY ACCOUNT".' });
		}

		try {
			await deleteAccount(fetch, getBaseUrl(), confirmation);
		} catch (e) {
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Failed to delete account.')
					: 'Failed to delete account.';
			return fail(500, { error: message });
		}

		cookies.delete(TOKEN_COOKIE, { path: '/' });
		throw redirect(303, '/login?deactivated=1');
	}
};
