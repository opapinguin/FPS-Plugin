using System;
using FPS.Entities;
using System.Collections.Generic;
using FPS.Weapons;
using MCGalaxy;

namespace FPS;

internal sealed class StateRound : GameState
{
    private readonly int _roundDurationSeconds;
    private int _timeRemainingSeconds;
    private DateTime _roundEnd;
    private DateTime _lastUpdateWeaponStatus;

    private const int RoundTickMilliseconds = 50;
    private readonly TimeSpan _spanUpdateRoundStatus = TimeSpan.FromMilliseconds(RoundTickMilliseconds);

    internal StateRound(FPSGame game, int roundDurationSeconds) : base(game)
    {
        _roundDurationSeconds = roundDurationSeconds;
    }
 
    internal override void Enter()
    {
        WeaponHandler.Activate();
        _roundEnd = DateTime.Now + TimeSpan.FromSeconds(_roundDurationSeconds);
        _timeRemainingSeconds = _roundDurationSeconds;
        _game.IsShootingEnabled = true;
        _game.IsTeamSwappingAllowed = true;
        UpdateWeaponStatus();

        _game.OnRoundStarted();
    }

    internal override void Loop()
    {
        DateTime now = DateTime.Now;

        int previousTimeRemainingSeconds = _timeRemainingSeconds;
        _timeRemainingSeconds = (int)(_roundEnd - now).TotalSeconds;
        bool timeChanged = previousTimeRemainingSeconds != _timeRemainingSeconds;

        if (timeChanged)
        {
            _game.OnRoundTicked(_timeRemainingSeconds);
        }

        if (now - _lastUpdateWeaponStatus > _spanUpdateRoundStatus)
        {
            UpdateWeaponStatus();
        }

        if (now >= _roundEnd)
        {
            _game.SetState(new StateVoting(_game, _game.VoteDurationSeconds));
            return;
        }
    }

    internal override void Exit()
    {
        WeaponHandler.Deactivate();
        _game.IsShootingEnabled = false;
        _game.IsTeamSwappingAllowed = false;
        _game.OnRoundEnded();
    }

    private void UpdateWeaponStatus()
    {
        _lastUpdateWeaponStatus = DateTime.Now;

        var playersCopy = new Dictionary<String, Player>(_game.Players);

        PlayerData playerData;
        Weapon weapon;

        foreach (KeyValuePair<String, Player> keyValuePair in playersCopy)
        {
            playerData = PlayerDataHandler.Instance[keyValuePair.Key];
            weapon = playerData.currentWeapon;
            uint delta = RoundTickMilliseconds / Constants.MS_UPDATE_WEAPON_ANIMATIONS;

            if (weapon.GetStatus(WeaponHandler.Tick - delta) < 10)
            {
                _game.OnWeaponStatusChanged(keyValuePair.Value);
            }
        }
    }
}