<script lang="ts">
	import { enhance } from '$app/forms';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let quest = $derived(data.quest);
</script>

<svelte:head>
	<title>{quest.title} | EM2Devs Todo</title>
</svelte:head>

<main>
	<header>
		<h1>{quest.title}</h1>
		<p class="description">{quest.description}</p>
		<div class="meta">
			<span class="progress">{quest.progress}% complete</span>
			{#if quest.dueDate}
				<span class="due-date">Due: {quest.dueDate}</span>
			{/if}
		</div>
	</header>

	<section>
		<h2>Tasks ({quest.tasks.length})</h2>
		{#if quest.tasks.length === 0}
			<p class="empty">No tasks assigned to this quest yet.</p>
		{:else}
			<ul class="task-list">
				{#each quest.tasks as task (task.id)}
					<li class="task-item" data-status={task.status}>
						<span class="task-title" class:done={task.status === 'Done'}
							>{task.title}</span
						>
						<span class="task-status" data-status={task.status}>{task.status}</span>
					</li>
				{/each}
			</ul>
		{/if}
	</section>

	<footer>
		<form method="POST" action="?/delete" use:enhance>
			<button type="submit" class="btn-delete">Delete Quest</button>
		</form>
	</footer>
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	header {
		margin-bottom: 2rem;
	}

	h1 {
		margin-bottom: 0.5rem;
	}

	.description {
		color: #6b7280;
		margin-bottom: 0.5rem;
	}

	.meta {
		display: flex;
		gap: 1rem;
		font-size: 0.875rem;
		color: #374151;
	}

	.progress {
		font-weight: 600;
	}

	h2 {
		margin-bottom: 1rem;
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
		padding: 0.5rem 0.75rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
	}

	.task-title.done {
		text-decoration: line-through;
		color: #9ca3af;
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

	footer {
		margin-top: 2rem;
		padding-top: 1rem;
		border-top: 1px solid #e5e7eb;
	}

	.btn-delete {
		padding: 0.5rem 1rem;
		border: 1px solid #dc2626;
		border-radius: 0.25rem;
		background: white;
		color: #dc2626;
		cursor: pointer;
	}

	.btn-delete:hover {
		background: #fef2f2;
	}
</style>
