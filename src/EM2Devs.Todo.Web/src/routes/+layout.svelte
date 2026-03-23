<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<nav>
	<a href={resolve('/')} class:active={page.url.pathname === '/'}>Tasks</a>
	<a href={resolve('/dashboard')} class:active={page.url.pathname === '/dashboard'}>Dashboard</a>
</nav>

{@render children()}

<style>
	nav {
		max-width: 640px;
		margin: 0 auto;
		padding: 1rem;
		display: flex;
		gap: 1.5rem;
		font-family: system-ui, sans-serif;
		border-bottom: 1px solid #e5e7eb;
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
</style>
