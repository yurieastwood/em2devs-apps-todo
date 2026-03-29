<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();
	let quest = $derived(data.quest);
	let availableTasks = $derived(data.availableTasks);

	let selectedTaskId = $state('');
	let newTaskTitle = $state('');
	let actionInFlight = $state(false);

	let actionError = $derived(form?.error ? String(form.error) : null);
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

	{#if actionError}
		<p class="error" role="alert">{actionError}</p>
	{/if}

	<section>
		<h2>Tasks ({quest.tasks.length})</h2>

		<form
			method="POST"
			action="?/createAndAddTask"
			use:enhance={() => {
				actionInFlight = true;
				return async ({ update, result }) => {
					try {
						await update();
					} finally {
						actionInFlight = false;
						if (result.type === 'success') newTaskTitle = '';
					}
				};
			}}
			class="add-task-form"
		>
			<input
				type="text"
				name="title"
				bind:value={newTaskTitle}
				placeholder="Create new task..."
				disabled={actionInFlight}
				maxlength={200}
			/>
			<button type="submit" disabled={actionInFlight || !newTaskTitle.trim()}>
				{actionInFlight ? 'Creating...' : 'Create & Add'}
			</button>
		</form>

		{#if availableTasks.length > 0}
			<form
				method="POST"
				action="?/addTask"
				use:enhance={() => {
					actionInFlight = true;
					return async ({ update, result }) => {
						try {
							await update();
						} finally {
							actionInFlight = false;
							if (result.type === 'success') selectedTaskId = '';
						}
					};
				}}
				class="add-task-form"
			>
				<select name="taskId" bind:value={selectedTaskId} disabled={actionInFlight}>
					<option value="">Select a task to add...</option>
					{#each availableTasks as task (task.id)}
						<option value={task.id}>{task.title} ({task.status})</option>
					{/each}
				</select>
				<button type="submit" disabled={actionInFlight || !selectedTaskId}>
					{actionInFlight ? 'Adding...' : 'Add'}
				</button>
			</form>
		{/if}

		{#if quest.tasks.length === 0}
			<p class="empty">No tasks assigned to this quest yet.</p>
		{:else}
			<ul class="task-list">
				{#each quest.tasks as task (task.id)}
					<li class="task-item" data-status={task.status}>
						<div class="task-info">
							<span class="task-title" class:done={task.status === 'Done'}
								>{task.title}</span
							>
							<span class="task-status" data-status={task.status}>{task.status}</span>
						</div>
						<form
							method="POST"
							action="?/removeTask"
							use:enhance={() => {
								actionInFlight = true;
								return async ({ update }) => {
									try {
										await update();
									} finally {
										actionInFlight = false;
									}
								};
							}}
						>
							<input type="hidden" name="taskId" value={task.id} />
							<button type="submit" class="btn-remove" disabled={actionInFlight}>
								Remove
							</button>
						</form>
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

	.error {
		color: #dc2626;
		padding: 0.75rem 1rem;
		border: 1px solid #fca5a5;
		border-radius: 0.25rem;
		background: #fef2f2;
		margin-bottom: 1rem;
	}

	h2 {
		margin-bottom: 1rem;
	}

	.add-task-form {
		display: flex;
		gap: 0.5rem;
		margin-bottom: 1rem;
	}

	.add-task-form select {
		flex: 1;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 0.875rem;
	}

	.add-task-form button {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.add-task-form button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
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

	.task-info {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		flex: 1;
		min-width: 0;
	}

	.task-title {
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
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
		flex-shrink: 0;
	}

	.task-status[data-status='Done'] {
		background: #d1fae5;
		color: #065f46;
	}

	.task-status[data-status='InProgress'] {
		background: #dbeafe;
		color: #1e40af;
	}

	.btn-remove {
		padding: 0.25rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		background: white;
		cursor: pointer;
		font-size: 0.75rem;
		flex-shrink: 0;
	}

	.btn-remove:hover {
		background: #fef2f2;
		border-color: #dc2626;
		color: #dc2626;
	}

	.btn-remove:disabled {
		opacity: 0.5;
		cursor: not-allowed;
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
