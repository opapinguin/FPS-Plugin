using System.Collections.Generic;
using System.Linq;
using FPS.Entities;
using FPS.Teams;
using MCGalaxy;

namespace FPS;

internal sealed class StateLoading : GameState
{
    private readonly string _mapName;

    public StateLoading(FPSGame game, string mapName) : base(game)
    {
        _mapName = mapName;
    }

    internal override void Enter()
    {
        _game.Map = Level.Load(_mapName);
        _game.MapData = _game.GetMapData();
        _game.LevelPicker.Register(_mapName);

        TeamHandler.Activate();
        PlayerDataHandler.Instance.ResetPlayerData();

        _game.AppendMapToServerName();
        MovePlayersToNextMap(_mapName);
        CountStandingPlayersAsJoining(_mapName);
        _game.SetState(new StateCountdown(_game, _game.GetCountdownDurationSeconds()));
    }

    private void CountStandingPlayersAsJoining(string levelName)
    {
        foreach (Player player in PlayerInfo.Online.Items)
        {
            if (player.level.name == levelName)
                _game.PlayerJoinedGame(player);
        }
    }

    private void MovePlayersToNextMap(string map)
    {
        List<Player> playersList = _game.Players.Values.ToList();

        foreach (Player p in playersList)
        {
            PlayerActions.ChangeMap(p, map);
        }
    }
}