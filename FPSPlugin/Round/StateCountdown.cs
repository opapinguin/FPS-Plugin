using System;
using MCGalaxy;

namespace FPS;

internal sealed class StateCountdown : GameState
{
    private readonly int _durationSeconds;
    private readonly TimeSpan _spanMorePlayersAlert = TimeSpan.FromSeconds(30d);
    private int _timeRemainingSeconds;
    private DateTime _countdownEnd;
    private DateTime _lastMorePlayersAlert;
    private bool _isCountdownRunning;

    #if DEBUG
    private const int MinimumPlayersCount = 1;
    #else
    private const int MinimumPlayersCount = 2;
    #endif

    internal StateCountdown(FPSGame game, int duration) : base(game)
    {
        if (duration < 0)
            throw new ArgumentException("Countdown duration must be positive or 0.", nameof(duration));

        _durationSeconds = duration;
        _timeRemainingSeconds = duration;
        _isCountdownRunning = false;

        if (HasEnoughPlayers())
            BeginCountdown();
    }

    internal override void Enter()
    {
        _game.PlayerJoined += HandlePlayerCountChanged;
        _game.PlayerLeft += HandlePlayerCountChanged;
    }

    internal void HandlePlayerCountChanged(Object sender, EventArgs args)
    {
        if (HasEnoughPlayers() && !_isCountdownRunning)
            BeginCountdown();
        else if (!HasEnoughPlayers() && _isCountdownRunning)
            CancelCountdown();
    }

    internal override void Loop()
    {
        DateTime now = DateTime.Now;

        if (!_isCountdownRunning)
        {
            if (now >= _lastMorePlayersAlert + _spanMorePlayersAlert)
            {
                _game.OnCountdownFailed();
                _lastMorePlayersAlert = now;
            }

            return;
        }

        int previousTimeRemainingSeconds = _timeRemainingSeconds;
        _timeRemainingSeconds = (int)(_countdownEnd - now).TotalSeconds;
        bool timeChanged = previousTimeRemainingSeconds != _timeRemainingSeconds;

        if (timeChanged)
        {
            _game.OnCountdownTicked(_timeRemainingSeconds);
        }

        if (now >= _countdownEnd)
        {
            _game.SetState(new StateRound(_game, (int)_game.MapData.RoundDurationSeconds));
        }

        return;
    }

    internal override void Exit()
    {
        _game.PlayerJoined -= HandlePlayerCountChanged;
        _game.PlayerLeft -= HandlePlayerCountChanged;
        _game.OnCountdownEnded();
    }

    private bool HasEnoughPlayers()
    {
        return _game.Players.Count >= MinimumPlayersCount;
    }

    private void BeginCountdown()
    {
        _countdownEnd = DateTime.Now + TimeSpan.FromSeconds(_durationSeconds);
        _timeRemainingSeconds = _durationSeconds;
        _isCountdownRunning = true;
    }

    private void CancelCountdown()
    {
        _isCountdownRunning = false;
    }
}