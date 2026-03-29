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
	let onboardingDismissed = $state(false);
	let editingTaskId = $state<string | null>(null);
	let editTitle = $state('');
	let editDescription = $state('');
	let confirmDeleteId = $state<string | null>(null);

	let createError = $derived(
		form?.action === 'create' && form?.error ? String(form.error) : null
	);
	let actionError = $derived(
		(form?.action === 'updateStatus' ||
			form?.action === 'delete' ||
			form?.action === 'edit' ||
			form?.action === 'reopen') &&
			form?.error
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

	function startEditing(task: { id: string; title: string; description: string | null }) {
		editingTaskId = task.id;
		editTitle = task.title;
		editDescription = task.description ?? '';
	}

	function cancelEditing() {
		editingTaskId = null;
		editTitle = '';
		editDescription = '';
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
			data-testid="task-title-input"
		/>
		<button type="submit" disabled={creating || !newTitle.trim()} data-testid="add-task-button">
			{creating ? 'Adding...' : 'Add Task'}
		</button>
		{#if createError}
			<p class="form-error" role="alert" data-testid="create-error">{createError}</p>
		{/if}
	</form>

	{#if loadError}
		<p class="error" role="alert">{loadError}</p>
	{:else if tasks.length === 0 && !onboardingDismissed}
		<div class="onboarding">
			<h2>Create your first task</h2>
			<p>Get started by adding something you need to do today.</p>
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
				class="onboarding-form"
			>
				<input
					type="text"
					name="title"
					bind:value={newTitle}
					placeholder="e.g. Buy groceries"
					disabled={creating}
					maxlength={200}
				/>
				<button type="submit" disabled={creating || !newTitle.trim()}>
					{creating ? 'Creating...' : 'Create Task'}
				</button>
			</form>
			<button type="button" class="btn-skip" onclick={() => (onboardingDismissed = true)}>
				Skip for now
			</button>
		</div>
	{:else if tasks.length === 0}
		<p class="empty">No tasks yet. Create your first task to get started!</p>
	{:else}
		<ul class="task-list" data-testid="task-list">
			{#each tasks as task (task.id)}
				<li class="task-item" data-status={task.status} data-testid="task-item">
					{#if editingTaskId === task.id}
						<form
							method="POST"
							action="?/edit"
							class="edit-form"
							use:enhance={() => {
								actionInFlight = task.id;
								return async ({ update, result }) => {
									try {
										await update();
									} finally {
										actionInFlight = null;
										if (result.type === 'success') cancelEditing();
									}
								};
							}}
						>
							<input type="hidden" name="taskId" value={task.id} />
							<input
								type="text"
								name="title"
								bind:value={editTitle}
								maxlength={200}
								class="edit-input"
								data-testid="edit-title-input"
							/>
							<textarea
								name="description"
								bind:value={editDescription}
								placeholder="Add a description..."
								class="edit-textarea"
								data-testid="edit-description-input"
							></textarea>
							<div class="edit-actions">
								<button
									type="submit"
									class="btn-save"
									disabled={actionInFlight === task.id || !editTitle.trim()}
									data-testid="edit-save-button"
								>
									{actionInFlight === task.id ? 'Saving...' : 'Save'}
								</button>
								<button
									type="button"
									class="btn-cancel"
									onclick={cancelEditing}
									data-testid="edit-cancel-button">Cancel</button
								>
							</div>
						</form>
					{:else}
						<div class="task-info">
							<button
								type="button"
								class="task-title-btn"
								class:done={task.status === 'Done'}
								onclick={() => startEditing(task)}
								data-testid="task-title"
							>
								{task.title}
							</button>
							<span
								class="task-status"
								data-status={task.status}
								data-testid="task-status">{task.status}</span
							>
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
										data-testid="task-advance-button"
									>
										{actionInFlight === task.id
											? '...'
											: nextStatusLabel(task.status)}
									</button>
								</form>
							{/if}
							{#if task.status === 'Done'}
								<form
									method="POST"
									action="?/reopen"
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
										class="btn-action"
										disabled={actionInFlight === task.id}
										data-testid="task-reopen-button"
									>
										{actionInFlight === task.id ? '...' : 'Reopen'}
									</button>
								</form>
							{/if}
							{#if confirmDeleteId === task.id}
								<div class="confirm-delete">
									<span>Delete?</span>
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
													confirmDeleteId = null;
												}
											};
										}}
									>
										<input type="hidden" name="taskId" value={task.id} />
										<button
											type="submit"
											class="btn-confirm-yes"
											disabled={actionInFlight === task.id}
											data-testid="task-confirm-delete"
										>
											{actionInFlight === task.id ? '...' : 'Yes'}
										</button>
									</form>
									<button
										type="button"
										class="btn-confirm-no"
										onclick={() => (confirmDeleteId = null)}
										data-testid="task-cancel-delete">No</button
									>
								</div>
							{:else}
								<button
									type="button"
									class="btn-delete"
									onclick={() => (confirmDeleteId = task.id)}
									data-testid="task-delete-button">Delete</button
								>
							{/if}
						</div>
					{/if}
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
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
	}

	.task-item:not(:has(.edit-form)) {
		display: flex;
		justify-content: space-between;
		align-items: center;
	}

	.task-info {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		flex: 1;
		min-width: 0;
	}

	.task-title-btn {
		background: none;
		border: none;
		padding: 0;
		cursor: pointer;
		text-align: left;
		font-size: inherit;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.task-title-btn:hover {
		color: #2563eb;
	}

	.task-title-btn.done {
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
		align-items: center;
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

	.edit-form {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		width: 100%;
	}

	.edit-input {
		padding: 0.5rem 0.75rem;
		border: 1px solid #2563eb;
		border-radius: 0.25rem;
		font-size: 1rem;
	}

	.edit-textarea {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 0.875rem;
		min-height: 3rem;
		resize: vertical;
		font-family: inherit;
	}

	.edit-actions {
		display: flex;
		gap: 0.5rem;
	}

	.btn-save {
		padding: 0.25rem 0.75rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
	}

	.btn-save:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-cancel {
		padding: 0.25rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
		background: white;
	}

	.confirm-delete {
		display: flex;
		gap: 0.25rem;
		align-items: center;
		font-size: 0.75rem;
		color: #dc2626;
	}

	.btn-confirm-yes {
		padding: 0.125rem 0.5rem;
		background: #dc2626;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
	}

	.btn-confirm-no {
		padding: 0.125rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
		background: white;
	}

	.onboarding {
		text-align: center;
		padding: 2rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		margin-top: 1rem;
	}

	.onboarding h2 {
		margin-bottom: 0.5rem;
	}

	.onboarding p {
		color: #6b7280;
		margin-bottom: 1.5rem;
	}

	.onboarding-form {
		display: flex;
		gap: 0.5rem;
		margin-bottom: 1rem;
	}

	.onboarding-form input {
		flex: 1;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
	}

	.onboarding-form button {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.onboarding-form button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-skip {
		background: none;
		border: none;
		color: #6b7280;
		cursor: pointer;
		font-size: 0.875rem;
		text-decoration: underline;
	}

	.btn-skip:hover {
		color: #374151;
	}
</style>
