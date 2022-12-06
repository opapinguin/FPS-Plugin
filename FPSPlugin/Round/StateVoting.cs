using System;
using System.Collections.Generic;
using FPS.Entities;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;

namespace FPS;

internal sealed class StateVoting : GameState
{
    private List<string> _maps;
    private int[] _votes;
    private int _timeRemainingSeconds;
    private readonly DateTime _voteEnd;

    internal StateVoting(FPSGame game, int voteDurationSeconds) : base(game)
    {
        _maps = _game.LevelPicker.PickVotingMaps();
        _votes = new int[_maps.Count];
        _timeRemainingSeconds = voteDurationSeconds;
        _voteEnd = DateTime.Now + TimeSpan.FromSeconds(voteDurationSeconds);
    }

    internal override void Enter()
    {
        if (_maps.Count == 0)
        {
            _game.Stop();
            return;
        }
        else if (_maps.Count == 1)
        {
            _game.SetState(new StateLoading(_game, _maps[0]));
            return;
        }

        OnPlayerChatEvent.Register(HandleVoting, Priority.High);
        _game.OnVoteStarted(_maps);
    }

    internal override void Loop()
    {
        DateTime now = DateTime.Now;

        int previousTimeRemainingSeconds = _timeRemainingSeconds;
        _timeRemainingSeconds = (int)(_voteEnd - now).TotalSeconds;
        bool timeChanged = previousTimeRemainingSeconds != _timeRemainingSeconds;

        if (timeChanged)
        {
            _game.OnVoteTicked(_timeRemainingSeconds);
        }

        if (now >= _voteEnd)
        {
            string mapName = GetNextMap();
            _game.SetState(new StateLoading(_game, mapName));
        }
    }

    internal override void Exit()
    {
        OnPlayerChatEvent.Unregister(HandleVoting);
        _game.OnVoteEnded(_maps.ToArray(), _votes);
    }

    internal override void EndGame() {}

    private string GetNextMap()
    {
        List<int> indexes;

        indexes = Utils.ArgMaxAllIndexes(_votes);
        var bestMaps = new List<string>();

        foreach (int index in indexes)
        {
            bestMaps.Add(_maps[index]);
        }

        var rand = new Random();
        int chosenMapIndex = rand.Next(indexes.Count);
        return _maps[indexes[chosenMapIndex]];
    }

    private void Vote(Player player, int mapNumber)
    {
        PlayerData playerData = PlayerDataHandler.Instance[player.truename];

        if (playerData.HasVoted)
        {
            int previousVote = (int)playerData.Vote;

            switch (previousVote)
            {
                case 1:
                    _votes[0]--;
                    break;
                case 2:
                    _votes[1]--;
                    break;
                case 3:
                    _votes[2]--;
                    break;
            }
        }

        playerData.Vote = (ushort)mapNumber;

        switch (mapNumber)
        {
            case 1:
                _votes[0]++;
                break;
            case 2:
                _votes[1]++;
                break;
            case 3:
                _votes[2]++;
                break;
        }

        playerData.HasVoted = true;
        player.Message($"&SYour vote: &T{mapNumber}&S.");
    }

    private bool GetVotingMessage(string message, ref int mapNumber)
    {
        message = message.TrimEnd();

        if (message == "1")
        {
            mapNumber = 1;
            return true;
        }
        else if (message == "2")
        {
            mapNumber = 2;
            return true;
        }
        else if (message == "3" && _maps[2] != null)
        {
            mapNumber = 3;
            return true;
        }

        return false;
    }

    private void HandleVoting(Player player, string message)
    {
        int mapNumber = 0;

        if (GetVotingMessage(message, ref mapNumber))
        {
            Vote(player, mapNumber);
            player.cancelchat = true;
        }
    }
}