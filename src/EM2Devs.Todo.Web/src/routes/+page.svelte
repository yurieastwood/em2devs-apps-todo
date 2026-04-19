<script lang="ts">
	import { enhance } from '$app/forms';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import type { ActionData, PageData } from './$types';
	import XpEarnedToast from '$lib/components/XpEarnedToast.svelte';
	import type { Task, TaskView } from '$lib/api/tasks';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let tasks = $derived(data.tasks);
	let recurringTasks = $derived(data.recurringTasks ?? []);
	let loadError = $derived(data.error);
	let profile = $derived(data.profile);
	let currentView = $derived<TaskView>(data.view);

	const VIEW_TABS: { view: TaskView; label: string }[] = [
		{ view: 'inbox', label: 'Inbox' },
		{ view: 'today', label: 'Today' },
		{ view: 'upcoming', label: 'Upcoming' },
		{ view: 'completed', label: 'Completed' }
	];

	function selectView(view: TaskView) {
		// resolve('/') returns the base path; append our query string for the view switch.
		// eslint-disable-next-line svelte/no-navigation-without-resolve
		goto(`${resolve('/')}?view=${view}`, { keepFocus: true, noScroll: true });
	}

	function startOfTodayMs(): number {
		const now = new Date();
		return new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
	}

	const WEEKDAY_FORMATTER = new Intl.DateTimeFormat(undefined, { weekday: 'long' });
	const DATE_FORMATTER = new Intl.DateTimeFormat(undefined, {
		month: 'short',
		day: 'numeric'
	});

	function parseDateOnly(iso: string): Date {
		// "YYYY-MM-DD" → local midnight Date for stable weekday formatting.
		const [y, m, d] = iso.split('-').map((s) => Number.parseInt(s, 10));
		return new Date(y, m - 1, d);
	}

	function formatUpcomingHeader(iso: string, todayMs: number): string {
		const target = parseDateOnly(iso);
		const diffDays = Math.round((target.getTime() - todayMs) / (24 * 60 * 60 * 1000));
		if (diffDays === 1) return 'Tomorrow';
		if (diffDays >= 2 && diffDays <= 7) return WEEKDAY_FORMATTER.format(target);
		return `${WEEKDAY_FORMATTER.format(target)}, ${DATE_FORMATTER.format(target)}`;
	}

	function formatCompletedHeader(iso: string, todayMs: number): string {
		const target = parseDateOnly(iso);
		const diffDays = Math.round((todayMs - target.getTime()) / (24 * 60 * 60 * 1000));
		if (diffDays === 0) return 'Today';
		if (diffDays === 1) return 'Yesterday';
		if (diffDays <= 7) return WEEKDAY_FORMATTER.format(target);
		return `${WEEKDAY_FORMATTER.format(target)}, ${DATE_FORMATTER.format(target)}`;
	}

	type TaskGroup = { key: string; label: string; tasks: Task[] };

	function bucketBy<T>(items: T[], keyOf: (item: T) => string | null): Record<string, T[]> {
		const buckets: Record<string, T[]> = {};
		for (const item of items) {
			const key = keyOf(item);
			if (key === null) continue;
			(buckets[key] ??= []).push(item);
		}
		return buckets;
	}

	let groupedTasks = $derived.by<TaskGroup[]>(() => {
		const todayMs = startOfTodayMs();
		if (currentView === 'upcoming') {
			const buckets = bucketBy(tasks, (t) => t.scheduledDate);
			return Object.entries(buckets)
				.sort(([a], [b]) => a.localeCompare(b))
				.map(([key, list]) => ({
					key,
					label: formatUpcomingHeader(key, todayMs),
					tasks: list
				}));
		}
		if (currentView === 'completed') {
			const cutoffMs = todayMs - 30 * 24 * 60 * 60 * 1000;
			const buckets = bucketBy(tasks, (t) => {
				if (!t.completedAt) return null;
				const ms = Date.parse(t.completedAt);
				if (Number.isNaN(ms) || ms < cutoffMs) return null;
				return t.completedAt.slice(0, 10);
			});
			return Object.entries(buckets)
				.sort(([a], [b]) => b.localeCompare(a))
				.map(([key, list]) => ({
					key,
					label: formatCompletedHeader(key, todayMs),
					tasks: list
				}));
		}
		return [];
	});
	let previousXp = $state<number | null>(null);
	let xpDelta = $state(0);

	$effect(() => {
		if (profile === null) return;
		if (previousXp === null) {
			previousXp = profile.totalXp;
			return;
		}
		if (profile.totalXp > previousXp) {
			const earnedXp = profile.totalXp - previousXp;
			xpDelta = 0;
			queueMicrotask(() => {
				xpDelta = earnedXp;
			});
		}
		previousXp = profile.totalXp;
	});

	let newTitle = $state('');
	let newScheduledDate = $state('');
	let newTags = $state('');
	let newRepeatPattern = $state('');
	let quickAddInput = $state('');
	let quickAddMode = $state(false);
	let creating = $state(false);
	let actionInFlight = $state<string | null>(null);
	let notification = $state<string | null>(null);
	let onboardingDismissed = $state(false);
	let confirmDeleteId = $state<string | null>(null);

	type StatusFilter = 'all' | 'Todo' | 'InProgress' | 'Done' | 'Skipped';
	type SortKey = 'created' | 'dueDate' | 'priority';

	let statusFilter = $state<StatusFilter>('all');
	let sortKey = $state<SortKey>('created');

	const PRIORITY_ORDER: Record<string, number> = {
		Critical: 0,
		High: 1,
		Medium: 2,
		Low: 3
	};

	let visibleTasks = $derived.by(() => {
		const filtered =
			statusFilter === 'all' ? tasks : tasks.filter((t) => t.status === statusFilter);
		const sorted = [...filtered];
		if (sortKey === 'dueDate') {
			sorted.sort((a, b) => {
				if (a.dueDate === null && b.dueDate === null) return 0;
				if (a.dueDate === null) return 1;
				if (b.dueDate === null) return -1;
				return a.dueDate.localeCompare(b.dueDate);
			});
		} else if (sortKey === 'priority') {
			sorted.sort(
				(a, b) => (PRIORITY_ORDER[a.priority] ?? 99) - (PRIORITY_ORDER[b.priority] ?? 99)
			);
		}
		return sorted;
	});

	let createError = $derived(
		(form?.action === 'create' || form?.action === 'quickAdd') && form?.error
			? String(form.error)
			: null
	);

	function formatScheduledLabel(iso: string): string {
		const d = parseDateOnly(iso);
		return `${WEEKDAY_FORMATTER.format(d)}, ${DATE_FORMATTER.format(d)}`;
	}
	let actionError = $derived(
		(form?.action === 'updateStatus' ||
			form?.action === 'delete' ||
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
</script>

<svelte:head>
	<title>Tasks | EM2Devs Todo</title>
</svelte:head>

<main>
	<XpEarnedToast delta={xpDelta} />
	<h1>Tasks</h1>

	<nav class="view-tabs" aria-label="Task views" data-testid="view-tabs">
		{#each VIEW_TABS as tab (tab.view)}
			<button
				type="button"
				class="view-tab"
				class:active={currentView === tab.view}
				aria-current={currentView === tab.view ? 'page' : undefined}
				onclick={() => selectView(tab.view)}
				data-testid={`view-tab-${tab.view}`}
			>
				{tab.label}
			</button>
		{/each}
	</nav>

	{#if notification}
		<div class="notification" role="alert">
			{notification}
			<button type="button" onclick={() => (notification = null)}>Dismiss</button>
		</div>
	{/if}

	<div class="create-mode-toggle" data-testid="create-mode-toggle">
		<button
			type="button"
			class="toggle-btn"
			class:active={!quickAddMode}
			onclick={() => (quickAddMode = false)}
			data-testid="toggle-structured"
		>
			Structured
		</button>
		<button
			type="button"
			class="toggle-btn"
			class:active={quickAddMode}
			onclick={() => (quickAddMode = true)}
			data-testid="toggle-quick-add"
		>
			Quick-add
		</button>
	</div>

	{#if !quickAddMode}
		<form
			method="POST"
			action={newRepeatPattern ? '?/createRecurring' : '?/create'}
			use:enhance={() => {
				creating = true;
				return async ({ update, result }) => {
					try {
						await update();
					} finally {
						creating = false;
						if (result.type === 'success') {
							newTitle = '';
							newScheduledDate = '';
							newTags = '';
							newRepeatPattern = '';
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
				placeholder="What needs to be done?"
				disabled={creating}
				maxlength={200}
				data-testid="task-title-input"
			/>
			<input
				type="date"
				name="scheduledDate"
				bind:value={newScheduledDate}
				disabled={creating || !!newRepeatPattern}
				data-testid="task-scheduled-date-input"
				aria-label="Scheduled date"
			/>
			<select
				name="pattern"
				bind:value={newRepeatPattern}
				disabled={creating}
				data-testid="task-repeat-select"
				aria-label="Repeat"
			>
				<option value="">No repeat</option>
				<option value="Daily">Daily</option>
				<option value="Weekly">Weekly</option>
				<option value="Monthly">Monthly</option>
			</select>
			<input
				type="text"
				name="tags"
				bind:value={newTags}
				placeholder="tags (comma-separated)"
				disabled={creating || !!newRepeatPattern}
				maxlength={500}
				data-testid="task-tags-input"
			/>
			<button
				type="submit"
				disabled={creating || !newTitle.trim()}
				data-testid="add-task-button"
			>
				{creating ? 'Adding...' : newRepeatPattern ? 'Add Recurring' : 'Add Task'}
			</button>
			{#if createError}
				<p class="form-error" role="alert" data-testid="create-error">{createError}</p>
			{/if}
		</form>
	{:else}
		<form
			method="POST"
			action="?/quickAdd"
			use:enhance={() => {
				creating = true;
				return async ({ update, result }) => {
					try {
						await update();
					} finally {
						creating = false;
						if (result.type === 'success') quickAddInput = '';
					}
				};
			}}
			class="create-form quick-add-form"
		>
			<input
				type="text"
				name="input"
				bind:value={quickAddInput}
				placeholder="try: standup notes ~daily #work !High ^tomorrow"
				disabled={creating}
				maxlength={500}
				data-testid="quick-add-input"
			/>
			<button
				type="submit"
				disabled={creating || !quickAddInput.trim()}
				data-testid="quick-add-button"
			>
				{creating ? 'Adding...' : 'Add'}
			</button>
			<p class="quick-add-hint">
				Use <code>#tag</code>, <code>!priority</code> (Low/Medium/High/Critical),
				<code>^date</code> (tomorrow, Monday, April 15), and
				<code>~repeat</code> (daily, weekly, monthly).
			</p>
			{#if createError}
				<p class="form-error" role="alert" data-testid="create-error">{createError}</p>
			{/if}
		</form>
	{/if}

	{#if tasks.length > 0}
		<div class="list-controls">
			<label class="control">
				Filter:
				<select bind:value={statusFilter} data-testid="filter-status">
					<option value="all">All</option>
					<option value="Todo">Todo</option>
					<option value="InProgress">In Progress</option>
					<option value="Done">Done</option>
					<option value="Skipped">Skipped</option>
				</select>
			</label>
			<label class="control">
				Sort by:
				<select bind:value={sortKey} data-testid="sort-key">
					<option value="created">Created</option>
					<option value="dueDate">Due date</option>
					<option value="priority">Priority</option>
				</select>
			</label>
		</div>
	{/if}

	{#snippet taskItem(task: Task)}
		<li class="task-item" data-status={task.status} data-testid="task-item">
			<div class="task-info">
				<div class="task-main">
					<a
						class="task-title-link"
						class:done={task.status === 'Done'}
						href={resolve(`/tasks/${task.id}`)}
						data-testid="task-title"
					>
						{task.title}
					</a>
					{#if task.tags && task.tags.length > 0}
						<span class="task-tags" data-testid="task-tags">
							{#each task.tags as tag (tag)}
								<span class="tag-chip" data-testid="task-tag-chip">#{tag}</span>
							{/each}
						</span>
					{/if}
					{#if task.scheduledDate}
						<span class="task-scheduled" data-testid="task-scheduled">
							scheduled for {formatScheduledLabel(task.scheduledDate)}
						</span>
					{/if}
				</div>
				<span class="task-status" data-status={task.status} data-testid="task-status"
					>{task.status}</span
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
						<input type="hidden" name="status" value={nextStatus(task.status)} />
						<button
							type="submit"
							class="btn-action"
							disabled={actionInFlight === task.id}
							data-testid="task-advance-button"
						>
							{actionInFlight === task.id ? '...' : nextStatusLabel(task.status)}
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
		</li>
	{/snippet}

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
		<p class="empty" data-testid="view-empty">
			{#if currentView === 'inbox'}
				No unassigned tasks. Everything's in a quest!
			{:else if currentView === 'today'}
				Nothing scheduled for today. Enjoy the calm.
			{:else if currentView === 'upcoming'}
				Nothing scheduled in the next 14 days.
			{:else if currentView === 'completed'}
				No completed tasks yet — finish something to see it here.
			{:else}
				No tasks yet. Create your first task to get started!
			{/if}
		</p>
	{:else if currentView === 'upcoming' || currentView === 'completed'}
		{#if groupedTasks.length === 0}
			<p class="empty" data-testid="view-empty">
				{#if currentView === 'upcoming'}
					Nothing scheduled in the next 14 days.
				{:else}
					No completed tasks in the last 30 days.
				{/if}
			</p>
		{:else}
			<div class="task-groups" data-testid="task-groups">
				{#each groupedTasks as group (group.key)}
					<section class="task-group" data-testid="task-group" data-group-key={group.key}>
						<h2 class="task-group-header" data-testid="task-group-header">
							{group.label}
						</h2>
						<ul class="task-list" data-testid="task-list">
							{#each group.tasks as task (task.id)}
								{@render taskItem(task)}
							{/each}
						</ul>
					</section>
				{/each}
			</div>
		{/if}
	{:else if visibleTasks.length === 0}
		<p class="empty" data-testid="filter-empty">No tasks match the current filter.</p>
	{:else}
		<ul class="task-list" data-testid="task-list">
			{#each visibleTasks as task (task.id)}
				{@render taskItem(task)}
			{/each}
		</ul>
	{/if}

	{#if recurringTasks.length > 0}
		<section class="recurring-section" data-testid="recurring-section">
			<h2>Recurring Tasks</h2>
			<ul class="task-list">
				{#each recurringTasks as rt (rt.id)}
					<li class="task-item" class:paused={!rt.isActive} data-testid="recurring-item">
						<div class="task-main">
							<span class="recurring-icon" title="Recurring">🔁</span>
							<span class="task-title-text" data-testid="recurring-title"
								>{rt.title}</span
							>
							<span class="recurring-pattern">{rt.pattern}</span>
							{#if !rt.isActive}
								<span class="recurring-paused">Paused</span>
							{/if}
						</div>
						<div class="task-actions-row">
							{#if rt.isActive}
								<form method="POST" action="?/pauseRecurring" use:enhance>
									<input type="hidden" name="id" value={rt.id} />
									<button type="submit" class="btn-sm">Pause</button>
								</form>
							{:else}
								<form method="POST" action="?/resumeRecurring" use:enhance>
									<input type="hidden" name="id" value={rt.id} />
									<button type="submit" class="btn-sm">Resume</button>
								</form>
							{/if}
							<form method="POST" action="?/deleteRecurring" use:enhance>
								<input type="hidden" name="id" value={rt.id} />
								<button type="submit" class="btn-sm btn-danger">Delete</button>
							</form>
						</div>
					</li>
				{/each}
			</ul>
		</section>
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

	.view-tabs {
		display: flex;
		gap: 0.25rem;
		margin-bottom: 1.25rem;
		border-bottom: 1px solid #e5e7eb;
	}

	.view-tab {
		padding: 0.5rem 0.9rem;
		background: none;
		border: none;
		border-bottom: 2px solid transparent;
		cursor: pointer;
		font-size: 0.9rem;
		color: #6b7280;
		font-weight: 500;
	}

	.view-tab:hover {
		color: #111827;
	}

	.view-tab.active {
		color: #2563eb;
		border-bottom-color: #2563eb;
	}

	.task-groups {
		display: flex;
		flex-direction: column;
		gap: 1.25rem;
	}

	.task-group-header {
		font-size: 0.85rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		color: #6b7280;
		margin: 0 0 0.5rem 0;
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

	.list-controls {
		display: flex;
		gap: 1rem;
		margin-bottom: 1rem;
	}

	.control {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		font-size: 0.875rem;
		color: #374151;
	}

	.control select {
		padding: 0.25rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 0.875rem;
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

	.task-title-link {
		color: inherit;
		text-decoration: none;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.task-title-link:hover {
		color: #2563eb;
	}

	.task-title-link.done {
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

	.create-mode-toggle {
		display: flex;
		gap: 0.25rem;
		margin-bottom: 0.75rem;
	}

	.toggle-btn {
		padding: 0.25rem 0.75rem;
		background: none;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.8rem;
		color: #6b7280;
	}

	.toggle-btn.active {
		background: #2563eb;
		color: white;
		border-color: #2563eb;
	}

	.create-form input[type='date'] {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
	}

	.quick-add-hint {
		width: 100%;
		margin: 0.25rem 0 0;
		color: #6b7280;
		font-size: 0.8rem;
	}

	.quick-add-hint code {
		background: #f3f4f6;
		padding: 0 0.25rem;
		border-radius: 0.2rem;
		font-size: 0.75rem;
	}

	.task-main {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		gap: 0.35rem 0.5rem;
		min-width: 0;
		flex: 1;
	}

	.task-tags {
		display: inline-flex;
		flex-wrap: wrap;
		gap: 0.25rem;
	}

	.tag-chip {
		display: inline-block;
		padding: 0.1rem 0.45rem;
		background: #eef2ff;
		color: #4338ca;
		border-radius: 999px;
		font-size: 0.7rem;
		font-weight: 500;
	}

	.task-scheduled {
		font-size: 0.75rem;
		color: #6b7280;
		font-style: italic;
	}

	.create-form select {
		padding: 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 0.9rem;
		background: white;
	}

	.recurring-section {
		margin-top: 2rem;
		padding-top: 1.5rem;
		border-top: 1px solid #e5e7eb;
	}

	.recurring-section h2 {
		font-size: 1rem;
		color: #6b7280;
		margin-bottom: 0.75rem;
	}

	.recurring-icon {
		font-size: 0.85rem;
	}

	.recurring-pattern {
		font-size: 0.75rem;
		color: #9ca3af;
		padding: 0.1rem 0.4rem;
		background: #f3f4f6;
		border-radius: 4px;
	}

	.recurring-paused {
		font-size: 0.7rem;
		color: #f59e0b;
		font-weight: 600;
	}

	.task-item.paused {
		opacity: 0.6;
	}

	.task-actions-row {
		display: flex;
		gap: 0.25rem;
	}

	.btn-sm {
		padding: 0.2rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 4px;
		background: white;
		cursor: pointer;
		font-size: 0.75rem;
	}

	.btn-sm:hover {
		background: #f3f4f6;
	}

	.btn-danger {
		color: #dc2626;
		border-color: #fca5a5;
	}
</style>
