<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import favicon from '$lib/assets/favicon.svg';
	import { enhance } from '$app/forms';
	import type { Snippet } from 'svelte';
	import type { LayoutData } from './$types';

	let { data, children }: { data: LayoutData; children: Snippet } = $props();
	let user = $derived(data.user);
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<nav>
	<div class="nav-links">
		<a href={resolve('/')} class:active={page.url.pathname === '/'}>Tasks</a>
		<a href={resolve('/dashboard')} class:active={page.url.pathname === '/dashboard'}
			>Dashboard</a
		>
	</div>
	{#if user}
		<div class="nav-user">
			<span class="user-name">{user.displayName}</span>
			<form method="POST" action="/logout" use:enhance>
				<button type="submit" class="btn-logout">Logout</button>
			</form>
		</div>
	{/if}
</nav>

{@render children()}

<style>
	nav {
		max-width: 640px;
		margin: 0 auto;
		padding: 0.75rem 1rem;
		display: flex;
		justify-content: space-between;
		align-items: center;
		font-family: system-ui, sans-serif;
		border-bottom: 1px solid #e5e7eb;
	}

	.nav-links {
		display: flex;
		gap: 1.5rem;
	}

	nav a {
		text-decoration: none;
		color: #6b7280;
		font-weight: 500;
		padding-bottom: 0.25rem;
	}

	nav a:hover {
		color: #111827;
	}

	nav a.active {
		color: #2563eb;
		border-bottom: 2px solid #2563eb;
	}

	.nav-user {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.user-name {
		font-weight: 500;
		color: #374151;
	}

	.btn-logout {
		padding: 0.25rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		background: white;
		cursor: pointer;
		font-size: 0.875rem;
	}

	.btn-logout:hover {
		background: #f3f4f6;
	}
</style>
