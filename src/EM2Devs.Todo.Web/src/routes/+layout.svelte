<script lang="ts">
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

{#if user}
	<nav>
		<span class="user-name">{user.displayName}</span>
		<form method="POST" action="/logout" use:enhance>
			<button type="submit" class="btn-logout">Logout</button>
		</form>
	</nav>
{/if}

{@render children()}

<style>
	nav {
		display: flex;
		justify-content: flex-end;
		align-items: center;
		gap: 1rem;
		padding: 0.75rem 1rem;
		border-bottom: 1px solid #e5e7eb;
		font-family: system-ui, sans-serif;
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
