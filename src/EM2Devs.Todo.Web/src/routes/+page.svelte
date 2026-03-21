<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let tasks = $derived(data.tasks);
	let error = $derived(data.error);
</script>

<svelte:head>
	<title>Tasks | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Tasks</h1>

	{#if error}
		<p class="error" role="alert">{error}</p>
	{:else if tasks.length === 0}
		<p class="empty">No tasks yet. Create your first task to get started!</p>
	{:else}
		<ul class="task-list">
			{#each tasks as task (task.id)}
				<li class="task-item">
					<span class="task-title">{task.title}</span>
					<span class="task-status" data-status={task.status}>{task.status}</span>
				</li>
			{/each}
		</ul>
	{/if}
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	h1 {
		margin-bottom: 1.5rem;
	}

	.error {
		color: #dc2626;
		padding: 1rem;
		border: 1px solid #dc2626;
		border-radius: 0.25rem;
	}

	.empty {
		color: #6b7280;
		font-style: italic;
	}

	.task-list {
		list-style: none;
		padding: 0;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.task-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
	}

	.task-status {
		font-size: 0.75rem;
		font-weight: 600;
		text-transform: uppercase;
		padding: 0.25rem 0.5rem;
		border-radius: 0.25rem;
		background: #e5e7eb;
	}

	.task-status[data-status='Done'] {
		background: #d1fae5;
		color: #065f46;
	}

	.task-status[data-status='InProgress'] {
		background: #dbeafe;
		color: #1e40af;
	}

	.task-status[data-status='Todo'] {
		background: #fef3c7;
		color: #92400e;
	}
</style>
