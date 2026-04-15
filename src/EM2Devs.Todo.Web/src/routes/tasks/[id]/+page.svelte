<script lang="ts">
	import { untrack } from 'svelte';
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();
	let task = $derived(form && 'task' in form && form.task ? form.task : data.task);

	function toLocalDateTimeString(iso: string | null): string {
		if (!iso) return '';
		const date = new Date(iso);
		if (Number.isNaN(date.getTime())) return '';
		const yyyy = date.getFullYear();
		const mm = String(date.getMonth() + 1).padStart(2, '0');
		const dd = String(date.getDate()).padStart(2, '0');
		const hh = String(date.getHours()).padStart(2, '0');
		const mi = String(date.getMinutes()).padStart(2, '0');
		return `${yyyy}-${mm}-${dd}T${hh}:${mi}`;
	}

	// Seed local edit state from the initial loaded task (server load or form action result).
	// We intentionally untrack the derived `task` here so these `$state` values become
	// independently editable inputs and Svelte doesn't warn about capturing only the initial value.
	let title = $state(untrack(() => task.title));
	let description = $state(untrack(() => task.description ?? ''));
	let difficulty = $state(untrack(() => task.difficulty));
	let priority = $state(untrack(() => task.priority));
	let estimatedMinutes = $state(untrack(() => task.estimatedMinutes?.toString() ?? ''));
	let dueDateLocal = $state(untrack(() => toLocalDateTimeString(task.dueDate)));

	let saving = $state(false);
	let deleting = $state(false);
	let recording = $state(false);
	let actualMinutesInput = $state('');

	let actionError = $derived(form?.error ? String(form.error) : null);
	let showRecordActualTimeForm = $derived(
		task.status === 'Done' &&
			task.estimatedMinutes != null &&
			(task.actualMinutes == null || task.actualMinutes === undefined)
	);
	let hasActualTime = $derived(task.actualMinutes != null && task.actualMinutes !== undefined);
</script>

<svelte:head>
	<title>{task.title} | EM2Devs Todo</title>
</svelte:head>

<main>
	<header>
		<a href={resolve('/')} class="back-link">← Back to tasks</a>
		<h1>Edit task</h1>
	</header>

	{#if actionError}
		<p class="error" role="alert" data-testid="task-edit-error">{actionError}</p>
	{/if}

	<form
		method="POST"
		action="?/save"
		use:enhance={() => {
			saving = true;
			return async ({ update }) => {
				try {
					await update();
				} finally {
					saving = false;
				}
			};
		}}
	>
		<label>
			Title
			<input
				type="text"
				name="title"
				bind:value={title}
				maxlength={200}
				required
				data-testid="task-edit-title"
			/>
		</label>

		<label>
			Description
			<textarea
				name="description"
				bind:value={description}
				rows={4}
				data-testid="task-edit-description"
			></textarea>
		</label>

		<label>
			Difficulty
			<select name="difficulty" bind:value={difficulty} data-testid="task-edit-difficulty">
				<option value="Trivial">Trivial</option>
				<option value="Easy">Easy</option>
				<option value="Normal">Normal</option>
				<option value="Hard">Hard</option>
				<option value="Epic">Epic</option>
			</select>
		</label>

		<label>
			Priority
			<select name="priority" bind:value={priority} data-testid="task-edit-priority">
				<option value="Low">Low</option>
				<option value="Medium">Medium</option>
				<option value="High">High</option>
				<option value="Critical">Critical</option>
			</select>
		</label>

		<label>
			Estimated minutes
			<input
				type="number"
				name="estimatedMinutes"
				bind:value={estimatedMinutes}
				min={1}
				max={525600}
				placeholder="(none)"
				data-testid="task-edit-estimated-minutes"
			/>
		</label>

		<label>
			Due date
			<input
				type="datetime-local"
				name="dueDate"
				bind:value={dueDateLocal}
				data-testid="task-edit-due-date"
			/>
		</label>

		<div class="actions">
			<button type="submit" class="btn-save" disabled={saving} data-testid="task-edit-save">
				{saving ? 'Saving...' : 'Save'}
			</button>
			<a href={resolve('/')} class="btn-cancel">Cancel</a>
		</div>
	</form>

	{#if hasActualTime}
		<section class="variance-block" data-testid="task-variance">
			<h2>Time spent</h2>
			<p>
				Estimated {task.estimatedMinutes} min, actual {task.actualMinutes} min ({#if (task.variancePercent ?? 0) >= 0}+{/if}{task.variancePercent}%
				variance)
			</p>
		</section>
	{:else if showRecordActualTimeForm}
		<section class="record-actual-time" data-testid="record-actual-time-form">
			<h2>Record actual time</h2>
			<p class="hint">This task is done. How long did it actually take?</p>
			<form
				method="POST"
				action="?/recordActualTime"
				use:enhance={() => {
					recording = true;
					return async ({ update }) => {
						try {
							await update({ reset: false });
						} finally {
							recording = false;
						}
					};
				}}
			>
				<label>
					Actual minutes
					<input
						type="number"
						name="actualMinutes"
						bind:value={actualMinutesInput}
						min={1}
						max={1440}
						required
						data-testid="actual-minutes-input"
					/>
				</label>
				<button
					type="submit"
					class="btn-save"
					disabled={recording}
					data-testid="record-actual-time-submit"
				>
					{recording ? 'Saving...' : 'Record actual time'}
				</button>
			</form>
		</section>
	{/if}

	<form
		method="POST"
		action="?/delete"
		use:enhance={() => {
			deleting = true;
			return async ({ update }) => {
				try {
					await update();
				} finally {
					deleting = false;
				}
			};
		}}
		class="delete-form"
	>
		<button type="submit" class="btn-delete" disabled={deleting} data-testid="task-edit-delete">
			{deleting ? 'Deleting...' : 'Delete task'}
		</button>
	</form>
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	header {
		margin-bottom: 1.5rem;
	}

	.back-link {
		color: #6b7280;
		text-decoration: none;
		font-size: 0.875rem;
	}

	.back-link:hover {
		color: #2563eb;
	}

	h1 {
		margin: 0.5rem 0 0;
	}

	.error {
		color: #991b1b;
		padding: 0.75rem 1rem;
		border: 1px solid #fca5a5;
		border-radius: 0.25rem;
		background: #fef2f2;
		margin-bottom: 1rem;
	}

	form {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
		font-size: 0.875rem;
		color: #374151;
	}

	input[type='text'],
	input[type='number'],
	input[type='datetime-local'],
	textarea,
	select {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
		font-family: inherit;
	}

	textarea {
		resize: vertical;
	}

	.actions {
		display: flex;
		gap: 0.5rem;
		margin-top: 0.5rem;
	}

	.btn-save {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.btn-save:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-cancel {
		padding: 0.5rem 1rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		background: white;
		text-decoration: none;
		color: #374151;
		font-weight: 500;
	}

	.delete-form {
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

	.record-actual-time,
	.variance-block {
		margin-top: 2rem;
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
		background: #f9fafb;
	}

	.record-actual-time h2,
	.variance-block h2 {
		margin: 0 0 0.5rem;
		font-size: 1rem;
	}

	.hint {
		color: #6b7280;
		font-size: 0.875rem;
		margin: 0 0 0.75rem;
	}
</style>
