using System;
namespace FPS;

internal abstract class GameState
{
	protected FPSGame _game;

	internal GameState(FPSGame game)
	{
		_game = game;
	}

	internal virtual void Enter() {}
	internal virtual void Loop() {}
	internal virtual void Exit() {}

	internal virtual void EndGame()
	{
		_game.SetState(new StateVoting(_game, _game.VoteDurationSeconds));
    }
}