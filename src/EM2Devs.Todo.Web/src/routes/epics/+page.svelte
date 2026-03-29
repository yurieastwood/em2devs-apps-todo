<script lang="ts">
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let epics = $derived(data.epics);
	let loadError = $derived(data.error);

	let newTitle = $state('');
	let newDescription = $state('');
	let creating = $state(false);

	let createError = $derived(
		form?.action === 'create' && form?.error ? String(form.error) : null
	);
</script>

<svelte:head>
	<title>Epics | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Epics</h1>

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
						newDescription = '';
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
			placeholder="Epic title"
			disabled={creating}
			maxlength={200}
			data-testid="epic-title-input"
		/>
		<input
			type="text"
			name="description"
			bind:value={newDescription}
			placeholder="Description (optional)"
			disabled={creating}
			data-testid="epic-description-input"
		/>
		<button
			type="submit"
			disabled={creating || !newTitle.trim()}
			data-testid="create-epic-button"
		>
			{creating ? 'Creating...' : 'Create Epic'}
		</button>
		{#if createError}
			<p class="form-error" role="alert">{createError}</p>
		{/if}
	</form>

	{#if loadError}
		<p class="error" role="alert">{loadError}</p>
	{:else if epics.length === 0}
		<p class="empty">
			No epics yet. Create your first epic to organise quests into milestones.
		</p>
	{:else}
		<ul class="epic-list" data-testid="epic-list">
			{#each epics as epic (epic.id)}
				<li data-testid="epic-item">
					<a href={resolve(`/epics/${epic.id}`)} class="epic-link">
						<div class="epic-info">
							<span class="epic-title" class:completed={epic.isCompleted}
								>{epic.title}</span
							>
							{#if epic.description}
								<span class="epic-description">{epic.description}</span>
							{/if}
						</div>
						<div class="epic-progress">
							<span class="progress-text">{Math.round(epic.progress)}%</span>
							<div class="progress-bar">
								<div
									class="progress-fill"
									style:width="{epic.progress}%"
									class:complete={epic.isCompleted}
								></div>
							</div>
						</div>
					</a>
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

	.create-form {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		margin-bottom: 1.5rem;
	}

	.create-form input {
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
		align-self: flex-start;
	}

	.create-form button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.form-error {
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

	.epic-list {
		list-style: none;
		padding: 0;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.epic-link {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
		text-decoration: none;
		color: inherit;
		transition: border-color 0.15s;
	}

	.epic-link:hover {
		border-color: #2563eb;
	}

	.epic-info {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
		flex: 1;
		min-width: 0;
	}

	.epic-title {
		font-weight: 500;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.epic-title.completed {
		text-decoration: line-through;
		color: #9ca3af;
	}

	.epic-description {
		font-size: 0.875rem;
		color: #6b7280;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.epic-progress {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		flex-shrink: 0;
		margin-left: 1rem;
		min-width: 100px;
	}

	.progress-text {
		font-size: 0.75rem;
		font-weight: 600;
		color: #6b7280;
		min-width: 2.5rem;
		text-align: right;
	}

	.progress-bar {
		flex: 1;
		height: 0.5rem;
		background: #e5e7eb;
		border-radius: 0.25rem;
		overflow: hidden;
	}

	.progress-fill {
		height: 100%;
		background: #2563eb;
		border-radius: 0.25rem;
		transition: width 0.3s ease;
	}

	.progress-fill.complete {
		background: #059669;
	}
</style>
