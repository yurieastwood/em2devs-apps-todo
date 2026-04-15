<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();

	let profile = $derived(data.profile);
	let error = $derived(data.error);
	let freezeError = $derived(
		form && 'freezeError' in form ? (form as { freezeError?: string }).freezeError : null
	);
	let freezeDays = $state(7);

	let isMaxLevel = $derived(profile !== null && profile.xpToNextLevel === 0);

	let progressPercent = $derived(
		profile && profile.totalXp + profile.xpToNextLevel > 0
			? Math.round((profile.totalXp / (profile.totalXp + profile.xpToNextLevel)) * 100)
			: 0
	);

	let breakdown = $derived(profile?.lastXpBreakdown ?? null);

	function formatModifier(value: number): string {
		if (value === 1.0) return 'none';
		if (value > 1.0) return `+${Math.round((value - 1) * 100)}%`;
		return `${Math.round((value - 1) * 100)}%`;
	}
</script>

<svelte:head>
	<title>Dashboard | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Progression Dashboard</h1>

	{#if error}
		<p class="error" role="alert">{error}</p>
	{:else if !profile}
		<div class="loading" aria-label="Loading profile">
			<div class="skeleton skeleton-level"></div>
			<div class="skeleton skeleton-bar"></div>
			<div class="skeleton skeleton-stats"></div>
		</div>
	{:else}
		<section class="level-section">
			<div
				class="level-badge"
				aria-label={`Level ${profile.level}`}
				data-testid="level-badge"
			>
				<span class="level-number" data-testid="level-number">{profile.level}</span>
				<span class="level-label">Level</span>
			</div>
		</section>

		<section class="xp-section">
			<div class="progress-header">
				<span class="xp-total" data-testid="xp-total"
					>{profile.totalXp.toLocaleString()} XP</span
				>
				{#if isMaxLevel}
					<span class="xp-next">Max Level</span>
				{:else}
					<span class="xp-next"
						>{profile.xpToNextLevel.toLocaleString()} XP to next level</span
					>
				{/if}
			</div>
			{#if !isMaxLevel}
				<div
					class="progress-bar"
					role="progressbar"
					aria-valuenow={progressPercent}
					aria-valuemin={0}
					aria-valuemax={100}
				>
					<div class="progress-fill" style:width="{progressPercent}%"></div>
				</div>
				<p class="progress-label">{progressPercent}% to Level {profile.level + 1}</p>
			{:else}
				<p class="progress-label">You've reached the highest level!</p>
			{/if}
		</section>

		{#if breakdown}
			<section class="breakdown-section">
				<h2>Last XP Earned</h2>
				<div class="breakdown-card">
					<div class="breakdown-row">
						<span class="breakdown-label">Base XP</span>
						<span class="breakdown-value">{breakdown.baseXp.toLocaleString()}</span>
					</div>
					<div class="breakdown-row">
						<span class="breakdown-label">Deadline</span>
						<span
							class="breakdown-value"
							data-modifier={breakdown.deadlineModifier > 1
								? 'bonus'
								: breakdown.deadlineModifier < 1
									? 'penalty'
									: 'none'}
						>
							{formatModifier(breakdown.deadlineModifier)}
						</span>
					</div>
					<div class="breakdown-row">
						<span class="breakdown-label">Streak</span>
						<span
							class="breakdown-value"
							data-modifier={breakdown.streakMultiplier > 1 ? 'bonus' : 'none'}
						>
							{formatModifier(breakdown.streakMultiplier)}
						</span>
					</div>
					<div class="breakdown-row breakdown-total">
						<span class="breakdown-label">Total</span>
						<span class="breakdown-value">+{breakdown.finalXp.toLocaleString()} XP</span
						>
					</div>
				</div>
			</section>
		{/if}

		<section class="streaks-section">
			<div class="streak-card" data-testid="current-streak">
				<span class="streak-value">{profile.currentStreak}</span>
				<span class="streak-label">Current Streak</span>
				<span class="streak-unit">days</span>
			</div>
			<div class="streak-card" data-testid="longest-streak">
				<span class="streak-value">{profile.longestStreak}</span>
				<span class="streak-label">Longest Streak</span>
				<span class="streak-unit">days</span>
			</div>
		</section>

		<section class="streak-freeze-section" data-testid="streak-freeze-section">
			{#if profile.streakFreeze}
				<div class="freeze-banner" data-testid="freeze-banner" role="status">
					<span class="freeze-icon" aria-hidden="true">❄</span>
					<div class="freeze-text">
						<strong>Streak frozen</strong>
						<span
							>Active until {profile.streakFreeze.expiresAt} ({profile.streakFreeze
								.days} day{profile.streakFreeze.days === 1 ? '' : 's'})</span
						>
					</div>
				</div>
			{:else}
				<form
					method="POST"
					action="?/freezeStreak"
					use:enhance
					class="freeze-form"
					data-testid="freeze-form"
				>
					<label for="freeze-days">Freeze duration</label>
					<select id="freeze-days" name="days" bind:value={freezeDays}>
						{#each [1, 3, 7] as option (option)}
							<option value={option}>{option} day{option === 1 ? '' : 's'}</option>
						{/each}
					</select>
					<button type="submit" data-testid="freeze-button">Freeze streak</button>
				</form>
				{#if freezeError}
					<p class="error" role="alert" data-testid="freeze-error">{freezeError}</p>
				{/if}
			{/if}
		</section>

		<section class="xp-history-section" data-testid="xp-history-section">
			<h2>XP History</h2>
			{#if profile.xpHistory.length === 0}
				<p class="empty-state">No XP earned yet — complete a task to start your history.</p>
			{:else}
				<ul class="xp-history-list">
					{#each [...profile.xpHistory].reverse() as entry (entry.date + '-' + entry.source + '-' + entry.cumulativeTotal)}
						<li class="xp-history-row" data-testid="xp-history-row">
							<span class="xp-history-date">{entry.date}</span>
							<span class="xp-history-amount">+{entry.xpEarned} XP</span>
							<span class="xp-history-source">{entry.source}</span>
							<span class="xp-history-total"
								>{entry.cumulativeTotal.toLocaleString()} total</span
							>
						</li>
					{/each}
				</ul>
			{/if}
		</section>

		<section class="titles-section" data-testid="titles-section">
			<h2>Titles</h2>
			{#if profile.titles.earned.length === 0 && profile.titles.progress.length === 0}
				<p class="empty-state">
					No titles earned yet — keep completing tasks to earn recognition.
				</p>
			{:else}
				{#if profile.titles.earned.length > 0}
					<div class="titles-grid">
						{#each profile.titles.earned as title (title.type)}
							<div
								class="title-card"
								class:title-active={profile.titles.active === title.type}
								data-testid="title-card"
							>
								<span class="title-name">{title.displayName}</span>
								<span class="title-meta">Earned {title.earnedOn}</span>
								{#if profile.titles.active === title.type}
									<span class="title-badge">Active</span>
								{/if}
							</div>
						{/each}
					</div>
				{/if}
				{#if profile.titles.progress.length > 0}
					<ul class="titles-progress">
						{#each profile.titles.progress as p (p.type)}
							<li class="title-progress-row">
								<span class="title-progress-name">{p.type}</span>
								<div
									class="progress-bar small"
									role="progressbar"
									aria-valuenow={p.progressPercentage}
									aria-valuemin={0}
									aria-valuemax={100}
								>
									<div
										class="progress-fill"
										style:width="{p.progressPercentage}%"
									></div>
								</div>
								<span class="title-progress-hint">{p.remainingDescription}</span>
							</li>
						{/each}
					</ul>
				{/if}
			{/if}
		</section>

		<section class="skill-trees-section" data-testid="skill-trees-section">
			<h2>Skill Trees</h2>
			<div class="skill-trees-grid">
				{#each profile.skillTrees as tree (tree.type)}
					<div
						class="skill-tree-card"
						class:skill-tree-locked={tree.tier === null}
						data-testid="skill-tree-card"
					>
						<h3 class="skill-tree-name">{tree.type}</h3>
						{#if tree.tier !== null}
							<p class="skill-tree-tier">Tier {tree.tier}</p>
							{#if tree.tasksToNextTier !== null && tree.tasksToNextTier > 0}
								<p class="skill-tree-progress-text">
									{tree.tasksCompletedInTier} of {(tree.tasksCompletedInTier ??
										0) + tree.tasksToNextTier} tasks toward Tier {tree.tier + 1}
								</p>
							{:else}
								<p class="skill-tree-progress-text">Maxed out</p>
							{/if}
							{#if tree.perks.length > 0}
								<ul class="skill-tree-perks">
									{#each tree.perks as perk (perk.tier + '-' + perk.perkType)}
										<li class="skill-tree-perk">
											<span class="perk-tier">T{perk.tier}</span>
											<span class="perk-desc">{perk.description}</span>
										</li>
									{/each}
								</ul>
							{/if}
						{:else}
							<p class="skill-tree-locked-text">Locked</p>
							<p class="skill-tree-hint">{tree.unlockHint}</p>
						{/if}
					</div>
				{/each}
			</div>
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

	h2 {
		font-size: 1rem;
		margin-bottom: 0.75rem;
		color: #374151;
	}

	.error {
		color: #dc2626;
		padding: 1rem;
		border: 1px solid #dc2626;
		border-radius: 0.25rem;
	}

	.loading {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.skeleton {
		background: linear-gradient(90deg, #e5e7eb 25%, #f3f4f6 50%, #e5e7eb 75%);
		background-size: 200% 100%;
		animation: shimmer 1.5s infinite;
		border-radius: 0.5rem;
	}

	.skeleton-level {
		width: 100px;
		height: 100px;
		border-radius: 50%;
		margin: 0 auto;
	}

	.skeleton-bar {
		height: 1.5rem;
	}

	.skeleton-stats {
		height: 5rem;
	}

	@keyframes shimmer {
		0% {
			background-position: -200% 0;
		}
		100% {
			background-position: 200% 0;
		}
	}

	.level-section {
		display: flex;
		justify-content: center;
		margin-bottom: 2rem;
	}

	.level-badge {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		width: 100px;
		height: 100px;
		border-radius: 50%;
		background: linear-gradient(135deg, #2563eb, #7c3aed);
		color: white;
	}

	.level-number {
		font-size: 2.5rem;
		font-weight: 700;
		line-height: 1;
	}

	.level-label {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		opacity: 0.9;
	}

	.xp-section {
		margin-bottom: 2rem;
	}

	.progress-header {
		display: flex;
		justify-content: space-between;
		margin-bottom: 0.5rem;
	}

	.xp-total {
		font-weight: 600;
		font-size: 1.125rem;
	}

	.xp-next {
		color: #6b7280;
		font-size: 0.875rem;
	}

	.progress-bar {
		height: 1.5rem;
		background: #e5e7eb;
		border-radius: 0.75rem;
		overflow: hidden;
	}

	.progress-fill {
		height: 100%;
		background: linear-gradient(90deg, #2563eb, #7c3aed);
		border-radius: 0.75rem;
		transition: width 0.3s ease;
		min-width: 0;
	}

	.progress-label {
		text-align: center;
		color: #6b7280;
		font-size: 0.875rem;
		margin-top: 0.5rem;
	}

	.breakdown-section {
		margin-bottom: 2rem;
	}

	.breakdown-card {
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		overflow: hidden;
	}

	.breakdown-row {
		display: flex;
		justify-content: space-between;
		padding: 0.5rem 1rem;
		border-bottom: 1px solid #f3f4f6;
	}

	.breakdown-row:last-child {
		border-bottom: none;
	}

	.breakdown-total {
		background: #f9fafb;
		font-weight: 600;
	}

	.breakdown-label {
		color: #6b7280;
	}

	.breakdown-value[data-modifier='bonus'] {
		color: #059669;
	}

	.breakdown-value[data-modifier='penalty'] {
		color: #dc2626;
	}

	.breakdown-value[data-modifier='none'] {
		color: #9ca3af;
	}

	.streaks-section {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 1rem;
	}

	.streak-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding: 1.5rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
	}

	.streak-value {
		font-size: 2rem;
		font-weight: 700;
		color: #2563eb;
	}

	.streak-label {
		font-size: 0.875rem;
		font-weight: 500;
		margin-top: 0.25rem;
	}

	.streak-unit {
		font-size: 0.75rem;
		color: #6b7280;
	}

	.xp-history-section,
	.titles-section,
	.skill-trees-section {
		margin-top: 2rem;
	}

	.streak-freeze-section {
		margin-top: 1rem;
	}

	.freeze-banner {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		padding: 0.75rem 1rem;
		border: 1px solid #bae6fd;
		background: #e0f2fe;
		border-radius: 0.5rem;
		color: #075985;
	}

	.freeze-icon {
		font-size: 1.5rem;
	}

	.freeze-text {
		display: flex;
		flex-direction: column;
		font-size: 0.875rem;
	}

	.freeze-form {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		padding: 0.5rem 0.75rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
	}

	.freeze-form label {
		font-size: 0.875rem;
		color: #374151;
	}

	.freeze-form select {
		padding: 0.25rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		background: white;
	}

	.freeze-form button {
		margin-left: auto;
		padding: 0.375rem 0.75rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
	}

	.freeze-form button:hover {
		background: #1d4ed8;
	}

	.empty-state {
		color: #6b7280;
		font-size: 0.875rem;
		padding: 1rem;
		border: 1px dashed #d1d5db;
		border-radius: 0.5rem;
		text-align: center;
	}

	.xp-history-list {
		list-style: none;
		padding: 0;
		margin: 0;
		max-height: 18rem;
		overflow-y: auto;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
	}

	.xp-history-row {
		display: grid;
		grid-template-columns: auto 1fr auto auto;
		gap: 0.75rem;
		align-items: center;
		padding: 0.5rem 0.75rem;
		border-bottom: 1px solid #f3f4f6;
		font-size: 0.875rem;
	}

	.xp-history-row:last-child {
		border-bottom: none;
	}

	.xp-history-date {
		font-variant-numeric: tabular-nums;
		color: #6b7280;
	}

	.xp-history-amount {
		color: #059669;
		font-weight: 600;
	}

	.xp-history-source {
		color: #374151;
	}

	.xp-history-total {
		color: #9ca3af;
		font-variant-numeric: tabular-nums;
	}

	.titles-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
		gap: 0.75rem;
	}

	.title-card {
		position: relative;
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		display: flex;
		flex-direction: column;
	}

	.title-active {
		border-color: #2563eb;
		background: #eff6ff;
	}

	.title-name {
		font-weight: 600;
	}

	.title-meta {
		font-size: 0.75rem;
		color: #6b7280;
		margin-top: 0.25rem;
	}

	.title-badge {
		position: absolute;
		top: 0.5rem;
		right: 0.5rem;
		font-size: 0.625rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		background: #2563eb;
		color: white;
		padding: 0.125rem 0.375rem;
		border-radius: 0.25rem;
	}

	.titles-progress {
		list-style: none;
		padding: 0;
		margin-top: 1rem;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.title-progress-row {
		display: grid;
		grid-template-columns: auto 1fr auto;
		gap: 0.75rem;
		align-items: center;
	}

	.title-progress-name {
		font-weight: 500;
	}

	.progress-bar.small {
		height: 0.5rem;
	}

	.title-progress-hint {
		font-size: 0.75rem;
		color: #6b7280;
	}

	.skill-trees-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
		gap: 0.75rem;
	}

	.skill-tree-card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
	}

	.skill-tree-locked {
		background: #f9fafb;
		color: #6b7280;
	}

	.skill-tree-name {
		font-size: 1rem;
		margin: 0 0 0.25rem 0;
	}

	.skill-tree-tier {
		font-weight: 600;
		color: #2563eb;
		margin: 0.25rem 0;
	}

	.skill-tree-progress-text {
		font-size: 0.75rem;
		color: #6b7280;
		margin: 0.25rem 0;
	}

	.skill-tree-locked-text {
		font-size: 0.875rem;
		font-weight: 500;
		margin: 0.25rem 0;
	}

	.skill-tree-hint {
		font-size: 0.75rem;
		color: #6b7280;
		margin: 0.25rem 0 0 0;
	}

	.skill-tree-perks {
		list-style: none;
		padding: 0;
		margin: 0.5rem 0 0 0;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.skill-tree-perk {
		display: flex;
		gap: 0.5rem;
		font-size: 0.75rem;
	}

	.perk-tier {
		font-weight: 600;
		color: #7c3aed;
		min-width: 1.75rem;
	}

	.perk-desc {
		color: #374151;
	}
</style>
