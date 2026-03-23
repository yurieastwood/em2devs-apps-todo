import { writable } from 'svelte/store';
import type { AuthUser } from '$lib/api/auth';

export const currentUser = writable<AuthUser | null>(null);
