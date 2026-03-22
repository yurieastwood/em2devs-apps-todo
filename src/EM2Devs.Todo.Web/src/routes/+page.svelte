<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let tasks = $derived(data.tasks);
	let loadError = $derived(data.error);

	let newTitle = $state('');
	let creating = $state(false);
	let actionInFlight = $state<string | null>(null);
	let notification = $state<string | null>(null);

	let createError = $derived(
		form?.action === 'create' && form?.error ? String(form.error) : null
	);
	let actionError = $derived(
		(form?.action === 'updateStatus' || form?.action === 'delete') && form?.error
			? String(form.error)
			: null
	);

	$effect(() => {
		if (actionError) {
			notification = actionError;
			const timer = setTimeout(() => (notification = null), 4000);
			return () => clearTimeout(timer);
		}
	});

	function nextStatus(current: string): 'InProgress' | 'Done' | null {
		if (current === 'Todo') return 'InProgress';
		if (current === 'InProgress') return 'Done';
		return null;
	}

	function nextStatusLabel(current: string): string {
		if (current === 'Todo') return 'Start';
		if (current === 'InProgress') return 'Complete';
		return '';
	}
</script>

<svelte:head>
	<title>Tasks | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Tasks</h1>

	{#if notification}
		<div class="notification" role="alert">
			{notification}
			<button type="button" onclick={() => (notification = null)}>Dismiss</button>
		</div>
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
					if (result.type === 'success') newTitle = '';
				}
			};
		}}
		class="create-form"
	>
		<input
			type="text"
			name="title"
			bind:value={newTitle}
			placeholder="What needs to be done?"
			disabled={creating}
			maxlength={200}
		/>
		<button type="submit" disabled={creating || !newTitle.trim()}>
			{creating ? 'Adding...' : 'Add Task'}
		</button>
		{#if createError}
			<p class="form-error" role="alert">{createError}</p>
		{/if}
	</form>

	{#if loadError}
		<p class="error" role="alert">{loadError}</p>
	{:else if tasks.length === 0}
		<p class="empty">No tasks yet. Create your first task to get started!</p>
	{:else}
		<ul class="task-list">
			{#each tasks as task (task.id)}
				<li class="task-item" data-status={task.status}>
					<div class="task-info">
						<span class="task-title" class:done={task.status === 'Done'}
							>{task.title}</span
						>
						<span class="task-status" data-status={task.status}>{task.status}</span>
					</div>
					<div class="task-actions">
						{#if nextStatus(task.status)}
							<form
								method="POST"
								action="?/updateStatus"
								use:enhance={() => {
									actionInFlight = task.id;
									return async ({ update }) => {
										try {
											await update();
										} finally {
											actionInFlight = null;
										}
									};
								}}
							>
								<input type="hidden" name="taskId" value={task.id} />
								<input
									type="hidden"
									name="status"
									value={nextStatus(task.status)}
								/>
								<button
									type="submit"
									class="btn-action"
									disabled={actionInFlight === task.id}
								>
									{actionInFlight === task.id
										? '...'
										: nextStatusLabel(task.status)}
								</button>
							</form>
						{/if}
						<form
							method="POST"
							action="?/delete"
							use:enhance={() => {
								actionInFlight = task.id;
								return async ({ update }) => {
									try {
										await update();
									} finally {
										actionInFlight = null;
									}
								};
							}}
						>
							<input type="hidden" name="taskId" value={task.id} />
							<button
								type="submit"
								class="btn-delete"
								disabled={actionInFlight === task.id}
							>
								{actionInFlight === task.id ? '...' : 'Delete'}
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

	.notification {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		margin-bottom: 1rem;
		background: #fef2f2;
		border: 1px solid #fca5a5;
		border-radius: 0.25rem;
		color: #991b1b;
	}

	.notification button {
		background: none;
		border: none;
		color: #991b1b;
		cursor: pointer;
		font-weight: 600;
	}

	.create-form {
		display: flex;
		flex-wrap: wrap;
		gap: 0.5rem;
		margin-bottom: 1.5rem;
	}

	.create-form input[type='text'] {
		flex: 1;
		min-width: 200px;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
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

	.form-error {
		width: 100%;
		color: #dc2626;
		font-size: 0.875rem;
		margin: 0;
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

	.task-status[data-status='Todo'] {
		background: #fef3c7;
		color: #92400e;
	}

	.task-actions {
		display: flex;
		gap: 0.25rem;
		flex-shrink: 0;
		margin-left: 0.75rem;
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
