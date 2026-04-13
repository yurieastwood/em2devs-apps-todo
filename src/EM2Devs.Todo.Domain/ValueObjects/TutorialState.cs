namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks which tutorials have been seen, the current session, and enforces
/// bombardment prevention (max 1 tutorial per session).
/// Maps to: docs/features/onboarding/progressive-disclosure.feature — Tutorial rule
/// </summary>
public sealed record TutorialState
{
    public const int MaxTutorialsPerSession = 1;

    private readonly HashSet<TutorialTopic> _seenTutorials;
    private readonly List<TutorialTopic> _queuedTutorials;

    public SessionId CurrentSessionId { get; }
    public int TutorialsShownThisSession { get; }
    public IReadOnlySet<TutorialTopic> SeenTutorials => _seenTutorials;
    public IReadOnlyList<TutorialTopic> QueuedTutorials => _queuedTutorials.AsReadOnly();

    private TutorialState(
        SessionId currentSessionId,
        int tutorialsShownThisSession,
        HashSet<TutorialTopic> seenTutorials,
        List<TutorialTopic> queuedTutorials)
    {
        ArgumentNullException.ThrowIfNull(currentSessionId);
        CurrentSessionId = currentSessionId;
        TutorialsShownThisSession = tutorialsShownThisSession;
        _seenTutorials = seenTutorials;
        _queuedTutorials = queuedTutorials;
    }

    public static TutorialState NewSession(SessionId sessionId)
    {
        return new TutorialState(sessionId, 0, [], []);
    }

    /// <summary>
    /// Starts a new session, preserving seen tutorials and carrying over queued tutorials.
    /// A new session is triggered by login or app launch, not by resume from background.
    /// </summary>
    public TutorialState StartNewSession(SessionId newSessionId)
    {
        return new TutorialState(
            newSessionId,
            0,
            new HashSet<TutorialTopic>(_seenTutorials),
            new List<TutorialTopic>(_queuedTutorials));
    }

    /// <summary>
    /// Attempts to show a tutorial. Returns the updated state with the tutorial shown
    /// if allowed, or queued if the bombardment limit has been reached.
    /// </summary>
    public TutorialState RequestTutorial(TutorialTopic topic)
    {
        if (_seenTutorials.Contains(topic))
        {
            return this;
        }

        if (TutorialsShownThisSession >= MaxTutorialsPerSession)
        {
            if (!_queuedTutorials.Contains(topic))
            {
                var newQueued = new List<TutorialTopic>(_queuedTutorials) { topic };
                return new TutorialState(
                    CurrentSessionId,
                    TutorialsShownThisSession,
                    new HashSet<TutorialTopic>(_seenTutorials),
                    newQueued);
            }

            return this;
        }

        var newSeen = new HashSet<TutorialTopic>(_seenTutorials) { topic };
        var remainingQueued = new List<TutorialTopic>(_queuedTutorials);
        remainingQueued.Remove(topic);
        return new TutorialState(
            CurrentSessionId,
            TutorialsShownThisSession + 1,
            newSeen,
            remainingQueued);
    }

    /// <summary>
    /// Shows the next queued tutorial for a new session.
    /// Should be called after StartNewSession when there are pending tutorials.
    /// </summary>
    public TutorialState ShowNextQueued()
    {
        if (_queuedTutorials.Count == 0 || TutorialsShownThisSession >= MaxTutorialsPerSession)
        {
            return this;
        }

        var topic = _queuedTutorials[0];
        var newSeen = new HashSet<TutorialTopic>(_seenTutorials) { topic };
        var newQueued = new List<TutorialTopic>(_queuedTutorials);
        newQueued.RemoveAt(0);
        return new TutorialState(
            CurrentSessionId,
            TutorialsShownThisSession + 1,
            newSeen,
            newQueued);
    }

    /// <summary>
    /// Returns true if the given tutorial can be shown in the current session.
    /// </summary>
    public bool CanShowTutorial(TutorialTopic topic)
    {
        return !_seenTutorials.Contains(topic) && TutorialsShownThisSession < MaxTutorialsPerSession;
    }

    /// <summary>
    /// Returns true if the given tutorial topic has already been seen.
    /// </summary>
    public bool HasSeenTutorial(TutorialTopic topic) => _seenTutorials.Contains(topic);
}
