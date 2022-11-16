using System;
using FPSMO.Configuration;
using MCGalaxy;
using System.Collections.Generic;
using System.Linq;
using FPSMO.Entities;
using static FPSMO.FPSMOGame;
using FPSMO.Weapons;

namespace FPSMO
{
	internal static class GUI
	{
        internal static void SubscribeTo(FPSMOGame game)
        {
            game.CountdownStarted += HandleCountdownStarted;
            game.PlayerShotWeapon += HandlePlayerShotWeapon;
            game.WeaponStatusChanged += HandleWeaponStatusChanged;
            game.CountdownTicked += HandleCountdownTicked;
            game.CountdownEnded += HandleCountdownEnded;
            game.RoundStarted += HandleRoundStarted;
            game.RoundTicked += HandleRoundTicked;
            game.RoundEnded += HandleRoundEnded;
            game.VoteStarted += HandleVoteStarted;
            game.VoteTicked += HandleVoteTicked;
            game.VoteEnded += HandleVoteEnded;
            game.PlayerJoined += HandlePlayerJoined;
            game.GameStopped += HandleGameStopped;
            game.WeaponSpeedChanged += HandleWeaponSpeedChanged;
            game.PlayerJoinedTeam += HandlePlayerJoinedTeam;
            game.PlayerKilled += HandlePlayerKilled;
            game.PlayerHit += HandlePlayerHit;
        }

        internal static void SubscribeTo(AchievementsManager manager)
        {
            manager.AchievementUnlocked += HandleAchievementUnlocked;
        }

        internal static void HandleCountdownStarted(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ShowLevel(player, game.map.name);
                ShowTeamStatistics(player);
                ShowMapInfo(player, game.map, game.mapConfig);
            }
        }

        internal static void HandlePlayerShotWeapon(Object sender, PlayerShotWeaponEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player p = args.p;

            if (!(game.stage == Stage.Round && game.subStage == SubStage.Middle))
            {
                return;
            }

            Weapon gun = PlayerDataHandler.Instance[p.truename].gun;
            PlayerDataHandler.Instance[p.truename].currentWeapon = gun;

            // Can't shoot if the status below 10
            if (gun.GetStatus(WeaponHandler.Tick) < 10)
            {
                return;
            }

            SetWeaponStatusBar(p, 0);

            PlayerDataHandler.Instance[p.truename].gun.Use(p.Rot, p.Pos.ToVec3F32());   // This takes care of the status too
        }

        internal static void HandleWeaponStatusChanged(Object sender, WeaponStatusChangedEventArgs args)
        {
            int status = args.status;
            Player p = args.p;

            status = Utils.Clamp(status, 0, 10);

            SetWeaponStatusBar(p, status);
        }

        internal static void HandleCountdownTicked(Object sender, CountdownTickedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            if (args.TimeRemaining <= 10 || args.TimeRemaining % 5 == 0)
            {
                foreach (Player player in game.players.Values)
                    ShowCountdown(player, args.TimeRemaining);
            }

            if (!args.HasEnoughPlayers && args.TimeRemaining == 1)
            {
                foreach (Player player in game.players.Values)
                    ShowNeedMorePlayers(player);
            }
        }

        internal static void HandleCountdownEnded(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal, "");
                player.SendCpeMessage(CpeMessageType.Announcement, "");
            }
        }

        internal static void HandleRoundStarted(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowRoundStarted(player);
        }

        internal static void HandleRoundTicked(Object sender, RoundTickedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowRoundTimeRemaining(player, args.TimeRemaining);
        }

        internal static void HandleRoundEnded(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ClearStatus(player);
                ClearBottomRight(player);
                player.Message("&SAnd the winners are: &7not implemented yet.");
            }
        }

        internal static void HandleVoteStarted(Object sender, VoteStartedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowVoteOptions(player, args.Map1, args.Map2, args.Map3);
        }

        internal static void HandleVoteTicked(Object sender, VoteTickedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowVoteTimeRemaining(player, args.TimeRemaining);
        }

        internal static void HandleVoteEnded(Object sender, VoteEndedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ClearBottomRight(player);
                ShowVoteResults(player, args.Map1, args.Map2, args.Map3,
                                args.Votes1, args.Votes2, args.Votes3);
            }
        }

        internal static void HandlePlayerJoined(Object sender, PlayerJoinedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            ShowTeamStatistics(args.Player);
            ShowLevel(args.Player, game.map.name);

            if (game.stage == FPSMOGame.Stage.Voting)
            {
                ShowVoteOptions(args.Player, game.map1, game.map2, game.map3);
            }
            else
            {
                InitStatusBars(args.Player);
            }

            if (game.stage == FPSMOGame.Stage.Round)
            {
                // TODO Find a way to access round time remaining from there
                ShowRoundTimeRemaining(args.Player, -1);
            }
        }

        internal static void HandleGameStopped(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ClearBottomRight(player);
                ClearStatus(player);
            }

            Chat.MessageAll("Parkour Game Stopped");
        }

        internal static void HandleWeaponSpeedChanged(Object sender, WeaponSpeedChangedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player player = args.Player;
            int amount = args.Amount;

            amount = Utils.Clamp(amount, 0, 10);

            player.SendCpeMessage(CpeMessageType.SmallAnnouncement, ColoredBlocks(amount));
        }

        internal static void HandlePlayerJoinedTeam(Object sender, PlayerJoinedTeamEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{args.Player.ColoredName} joined team {args.TeamName.ToUpper()}");
            }
        }

        internal static void HandlePlayerHit(Object sender, PlayerHitEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player victim = args.victim;
            Player shooter = args.shooter;

            victim.Message(String.Format("{0} hit you!"), shooter.DisplayName);
            shooter.Message(String.Format("Hit {0}!"), victim.DisplayName);
        }

        internal static void HandlePlayerKilled(Object sender, PlayerKilledEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{args.killer.ColoredName} killed {args.victim.ColoredName}");
            }
        }

        internal static void HandleAchievementUnlocked(Object sender, AchievementUnlockedEventArgs args)
        {
            Player who = args.Player;

            foreach (Player player in FPSMOGame.Instance.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{who.ColoredName} &Sunlocked &f{args.Achievement.Name}");
            }
        }

        private static void ShowMapInfo(Player p, Level level, FPSMOMapConfig mapConfig)
        {
            string authors = string.Join(", ", level.Config.Authors.Split(',').Select(
                x => PlayerInfo.FindExact(x) == null ? x : PlayerInfo.FindExact(x).ColoredName + "%e"));

            p.Message(String.Format("Starting new round"));
            p.Message(String.Format("This map was made by {0}", authors));

            if (mapConfig.TOTAL_RATINGS == 0)
            {
                p.Message("This map has not yet been rated");
            }
            else
            {
                p.Message(String.Format("This map has a rating of {0}", mapConfig.Rating.ToString("0.00")));
            }
        }

        private static void ShowTeamStatistics(Player p)
        {
            p.SendCpeMessage(CpeMessageType.Status2, "<Team statistics>");
        }

        private static void ShowCountdown(Player player, int timeRemaining)
        {
            bool plural = (timeRemaining != 1);
            string message;

            if (plural) message = $"&4Starting in &f{timeRemaining} &4seconds";
            else message = $"&4Starting in &f{timeRemaining} &4second";

            player.SendCpeMessage(CpeMessageType.Announcement, message);
        }

        private static void ShowNeedMorePlayers(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Normal, "&WNeed 2 or more non-ref players to start a round.");
        }

        private static void ShowVoteOptions(Player player, string map1, string map2, string map3)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight2, "Pick a level");
            player.SendCpeMessage(CpeMessageType.BottomRight1, $"{map1}, {map2}, {map3}");
        }

        private static void ShowVoteTimeRemaining(Player player, int timeRemaining)
        {
            string message = String.Format($"Vote time remaining: {timeRemaining}");
            player.SendCpeMessage(CpeMessageType.BottomRight3, message);
        }

        private static void ShowVoteResults(Player player, string map1, string map2, string map3,
                                     int votes1, int votes2, int votes3)
        {
            player.SendCpeMessage(CpeMessageType.Normal, $"Votes are in! map 1: {votes1} map 2: {votes2} map 3: {votes3}");
        }

        private static void ShowRoundTimeRemaining(Player player, int timeRemaining)
        {
            player.SendCpeMessage(CpeMessageType.Status3, $"Time remaining: {timeRemaining}");
        }

        private static void ClearBottomRight(Player player)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight1, "");
            player.SendCpeMessage(CpeMessageType.BottomRight2, "");
            player.SendCpeMessage(CpeMessageType.BottomRight3, "");
        }

        private static void ClearStatus(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Status1, "");
            player.SendCpeMessage(CpeMessageType.Status2, "");
            player.SendCpeMessage(CpeMessageType.Status3, "");
        }

        private static void ShowRoundStarted(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Normal, "Round has started! You are no longer invincible.");
        }

        private static void InitStatusBars(Player player)
        {
            SetHealthBar(player, 10);
            SetStaminaBar(player, 10);
            SetWeaponStatusBar(player, 10);
        }

        private static string ColoredBlocks(int amount)
        {
            string blocks = "▌▌▌▌▌▌▌▌▌▌";
            return $"%4{blocks.Substring(amount)}%2{blocks.Substring(10 - amount)}";
        }

        private static void SetHealthBar(Player player, int amount)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight1, ColoredBlocks(amount));
        }

        private static void SetStaminaBar(Player player, int amount)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight2, ColoredBlocks(amount));
        }

        private static void SetWeaponStatusBar(Player player, int amount)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight3, ColoredBlocks(amount));
        }

        private static void ShowLevel(Player p, string mapName)
        {
            p.SendCpeMessage(CpeMessageType.Status1, String.Format("Map: {0}", mapName));
        }
    }
}