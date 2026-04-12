<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let recurringTasks = $derived(data.recurringTasks);
	let loadError = $derived(data.error);
	let newTitle = $state('');
	let newPattern = $state('Daily');
	let creating = $state(false);
	let actionId = $state<string | null>(null);

	let actionError = $derived(form?.error ? String(form.error) : null);
</script>

<svelte:head>
	<title>Recurring Tasks | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Recurring Tasks</h1>

	{#if actionError}
		<p class="error" role="alert">{actionError}</p>
	{/if}

	<form
		method="POST"
		action="?/create"
		use:enhance={() => {
			creating = true;
			return async ({ update, result }) => {
				try {
					await update();
				} finally {
					creating = false;
					if (result.type === 'success') {
						newTitle = '';
						newPattern = 'Daily';
					}
				}
			};
		}}
		class="create-form"
	>
		<input
			type="text"
			name="title"
			bind:value={newTitle}
			placeholder="Task title"
			disabled={creating}
			maxlength={200}
		/>
		<select name="pattern" bind:value={newPattern} disabled={creating}>
			<option value="Daily">Daily</option>
			<option value="Weekly">Weekly</option>
			<option value="Monthly">Monthly</option>
		</select>
		<button type="submit" disabled={creating || !newTitle.trim()}>
			{creating ? 'Creating...' : 'Create'}
		</button>
	</form>

	{#if loadError}
		<p class="error" role="alert">{loadError}</p>
	{:else if recurringTasks.length === 0}
		<p class="empty">No recurring tasks yet.</p>
	{:else}
		<ul class="task-list">
			{#each recurringTasks as task (task.id)}
				<li class="task-item" class:paused={!task.isActive}>
					<div class="task-info">
						<span class="task-title">{task.title}</span>
						<span class="task-pattern">{task.pattern}</span>
						<span class="task-status-badge" class:active={task.isActive}>
							{task.isActive ? 'Active' : 'Paused'}
						</span>
					</div>
					<div class="task-actions">
						{#if task.isActive}
							<form
								method="POST"
								action="?/pause"
								use:enhance={() => {
									actionId = task.id;
									return async ({ update }) => {
										try {
											await update();
										} finally {
											actionId = null;
										}
									};
								}}
							>
								<input type="hidden" name="id" value={task.id} />
								<button
									type="submit"
									class="btn-action"
									disabled={actionId === task.id}
								>
									{actionId === task.id ? '...' : 'Pause'}
								</button>
							</form>
						{:else}
							<form
								method="POST"
								action="?/resume"
								use:enhance={() => {
									actionId = task.id;
									return async ({ update }) => {
										try {
											await update();
										} finally {
											actionId = null;
										}
									};
								}}
							>
								<input type="hidden" name="id" value={task.id} />
								<button
									type="submit"
									class="btn-action"
									disabled={actionId === task.id}
								>
									{actionId === task.id ? '...' : 'Resume'}
								</button>
							</form>
						{/if}
						<form
							method="POST"
							action="?/delete"
							use:enhance={() => {
								actionId = task.id;
								return async ({ update }) => {
									try {
										await update();
									} finally {
										actionId = null;
									}
								};
							}}
						>
							<input type="hidden" name="id" value={task.id} />
							<button
								type="submit"
								class="btn-delete"
								disabled={actionId === task.id}
							>
								{actionId === task.id ? '...' : 'Delete'}
							</button>
						</form>
					</div>
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
		padding: 0.75rem 1rem;
		border: 1px solid #fca5a5;
		border-radius: 0.25rem;
		background: #fef2f2;
		margin-bottom: 1rem;
	}

	.create-form {
		display: flex;
		gap: 0.5rem;
		margin-bottom: 1.5rem;
	}

	.create-form input {
		flex: 1;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
	}

	.create-form select {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
	}

	.create-form button {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.create-form button:disabled {
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
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
	}

	.task-item.paused {
		opacity: 0.6;
	}

	.task-info {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.task-title {
		font-weight: 500;
	}

	.task-pattern {
		font-size: 0.75rem;
		color: #6b7280;
		text-transform: uppercase;
	}

	.task-status-badge {
		font-size: 0.75rem;
		font-weight: 600;
		padding: 0.125rem 0.5rem;
		border-radius: 0.25rem;
		background: #fef3c7;
		color: #92400e;
	}

	.task-status-badge.active {
		background: #d1fae5;
		color: #065f46;
	}

	.task-actions {
		display: flex;
		gap: 0.25rem;
	}

	.btn-action,
	.btn-delete {
		padding: 0.25rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
		background: white;
	}

	.btn-action:hover {
		background: #eff6ff;
		border-color: #2563eb;
		color: #2563eb;
	}

	.btn-delete:hover {
		background: #fef2f2;
		border-color: #dc2626;
		color: #dc2626;
	}

	.btn-action:disabled,
	.btn-delete:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
</style>
