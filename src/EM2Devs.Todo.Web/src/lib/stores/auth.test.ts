import { describe, it, expect } from 'vitest';
import { get } from 'svelte/store';
import { currentUser } from './auth';

describe('auth store', () => {
	it('Should_BeNull_When_NoUserLoggedIn', () => {
		expect(get(currentUser)).toBeNull();
	});

	it('Should_HoldUser_When_UserSetsStore', () => {
		currentUser.set({
			userId: '00000000-0000-0000-0000-000000000001',
			displayName: 'Demo User'
		});

		expect(get(currentUser)).toEqual({
			userId: '00000000-0000-0000-0000-000000000001',
			displayName: 'Demo User'
		});
	});

	it('Should_ClearUser_When_LogoutSetsNull', () => {
		currentUser.set({
			userId: '00000000-0000-0000-0000-000000000001',
			displayName: 'Demo User'
		});
		currentUser.set(null);

		expect(get(currentUser)).toBeNull();
	});
});
