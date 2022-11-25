using System;
using FPSMO.Configuration;
using MCGalaxy;
using System.Collections.Generic;
using System.Linq;
using FPSMO.Entities;
using static FPSMO.FPSMOGame;
using FPSMO.Weapons;
using MCGalaxy.Events.PlayerEvents;

namespace FPSMO
{
	internal class GUI
	{
        internal GUI(FPSMOPlugin plugin, FPSMOGame game, AchievementsManager manager)
        {
            SubscribeTo(game);
            SubscribeTo(manager);
            SubscribeTo(plugin);

            // MCGalaxy-related events
            OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.Normal);
        }

        internal void UnsubscribeFromAll(FPSMOPlugin plugin, FPSMOGame game, AchievementsManager manager)
        {
            UnsubscribeFrom(game);
            UnsubscribeFrom(manager);
            UnsubscribeFrom(plugin);

            // MCGalaxy-related events
            OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
        }

        private void SubscribeTo(FPSMOGame game)
        {
            game.CountdownStarted += HandleCountdownStarted;
            game.PlayerShotWeapon += HandlePlayerShotWeapon;
            game.WeaponChanged += HandleWeaponChanged;
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
            game.PlayerLeft += HandlePlayerLeft;
            game.GameStopped += HandleGameStopped;
            game.WeaponSpeedChanged += HandleWeaponSpeedChanged;
            game.PlayerJoinedTeam += HandlePlayerJoinedTeam;
            game.PlayerKilled += HandlePlayerKilled;
            game.PlayerHit += HandlePlayerHit;
        }

        private void UnsubscribeFrom(FPSMOGame game)
        {
            game.CountdownStarted -= HandleCountdownStarted;
            game.PlayerShotWeapon -= HandlePlayerShotWeapon;
            game.WeaponChanged -= HandleWeaponChanged;
            game.WeaponStatusChanged -= HandleWeaponStatusChanged;
            game.CountdownTicked -= HandleCountdownTicked;
            game.CountdownEnded -= HandleCountdownEnded;
            game.RoundStarted -= HandleRoundStarted;
            game.RoundTicked -= HandleRoundTicked;
            game.RoundEnded -= HandleRoundEnded;
            game.VoteStarted -= HandleVoteStarted;
            game.VoteTicked -= HandleVoteTicked;
            game.VoteEnded -= HandleVoteEnded;
            game.PlayerJoined -= HandlePlayerJoined;
            game.PlayerLeft -= HandlePlayerLeft;
            game.GameStopped -= HandleGameStopped;
            game.WeaponSpeedChanged -= HandleWeaponSpeedChanged;
            game.PlayerJoinedTeam -= HandlePlayerJoinedTeam;
            game.PlayerKilled -= HandlePlayerKilled;
            game.PlayerHit -= HandlePlayerHit;
        }

        private void SubscribeTo(AchievementsManager manager)
        {
            manager.AchievementUnlocked += HandleAchievementUnlocked;
        }

        private void UnsubscribeFrom(AchievementsManager manager)
        {
            manager.AchievementUnlocked -= HandleAchievementUnlocked;
        }

        private void SubscribeTo(FPSMOPlugin plugin)
        {
            plugin.PluginLoaded += HandlePluginLoaded;
            plugin.PluginUnloading += HandlePluginUnloading;
        }

        private void UnsubscribeFrom(FPSMOPlugin plugin)
        {
            plugin.PluginLoaded -= HandlePluginLoaded;
            plugin.PluginUnloading += HandlePluginUnloading;
        }

        internal void HandleCountdownStarted(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ShowTeamStatistics(player);
                ShowMapInfo(player, game.map, game.mapConfig);
                ShowInventory(player);
            }
        }

        internal void HandlePlayerShotWeapon(Object sender, PlayerShotWeaponEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player p = args.Player;

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

            ShowWeaponReload(p, 0);

            PlayerDataHandler.Instance[p.truename].gun.Use(p.Rot, p.Pos.ToVec3F32());   // This takes care of the status too
        }

        internal void HandleWeaponChanged(Object sender, WeaponChangedEventArgs args)
        {
            Player p = args.p;
            int status = PlayerDataHandler.Instance[p.truename].currentWeapon.GetStatus(WeaponHandler.Tick);

            status = Utils.Clamp(status, 0, 10);

            p.SendCpeMessage(CpeMessageType.SmallAnnouncement, ColoredBlocks(status));
        }

        internal void HandleWeaponStatusChanged(Object sender, WeaponStatusChangedEventArgs args)
        {
            int status = args.Status;
            Player p = args.Player;

            status = Utils.Clamp(status, 0, 10);

            ShowWeaponReload(p, status);
        }

        internal void HandleCountdownTicked(Object sender, CountdownTickedEventArgs args)
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

        internal void HandleCountdownEnded(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.SmallAnnouncement, "");
                player.SendCpeMessage(CpeMessageType.Announcement, "");
            }
        }

        internal void HandleRoundStarted(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ShowRoundStarted(player);
            }
        }

        internal void HandleRoundTicked(Object sender, RoundTickedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowRoundTimeRemaining(player, args.TimeRemaining);
        }

        internal void HandleRoundEnded(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Status2, "");
                player.SendCpeMessage(CpeMessageType.Status3, "");
                ClearBottomRight(player);
                player.Message("&7The &cRED &7team wins!");
            }
        }

        internal void HandleVoteStarted(Object sender, VoteStartedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowVoteOptions(player, args.Map1, args.Map2, args.Map3);
        }

        internal void HandleVoteTicked(Object sender, VoteTickedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
                ShowVoteTimeRemaining(player, args.TimeRemaining);
        }

        internal void HandleVoteEnded(Object sender, VoteEndedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ClearBottomRight(player);
                ShowVoteResults(player, args.Map1, args.Map2, args.Map3,
                                args.Votes1, args.Votes2, args.Votes3);
            }
        }

        internal void HandlePlayerJoined(Object sender, PlayerJoinedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            ShowTeamStatistics(args.Player);

            if (game.stage == FPSMOGame.Stage.Voting)
            {
                ShowVoteOptions(args.Player, game.map1, game.map2, game.map3);
            }
            else
            {
                ShowInventory(args.Player);
            }

            if (game.stage == FPSMOGame.Stage.Round)
            {
                // TODO Find a way to access round time remaining from there
                ShowRoundTimeRemaining(args.Player, -1);
            }
        }

        internal void HandlePlayerLeft(Object sender, PlayerLeftEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            ClearBottomRight(args.Player);
            args.Player.SendCpeMessage(CpeMessageType.Status2, "");
            args.Player.SendCpeMessage(CpeMessageType.Status3, "");
        }

        internal void HandleGameStopped(Object sender, EventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                ClearBottomRight(player);
                player.SendCpeMessage(CpeMessageType.Status2, "");
                player.SendCpeMessage(CpeMessageType.Status3, "");
            }

            Chat.MessageAll("&7The FPS game has been stopped.");
        }

        internal void HandleWeaponSpeedChanged(Object sender, WeaponSpeedChangedEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player player = args.Player;
            int amount = args.Amount;

            amount = Utils.Clamp(amount, 0, 10);

            player.SendCpeMessage(CpeMessageType.SmallAnnouncement, ColoredBlocks(amount));
        }

        internal void HandlePlayerJoinedTeam(Object sender, PlayerJoinedTeamEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            string color = (args.TeamName.ToUpper() == "RED") ? "&c" : "&9";

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{args.Player.ColoredName} &7joined team {color}{args.TeamName.ToUpper()}&7!");
            }
        }

        internal void HandlePlayerHit(Object sender, PlayerHitEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;
            Player victim = args.Victim;
            Player shooter = args.Shooter;

            victim.Message($"&7You got hit by {shooter.ColoredName}");
            shooter.Message("&7You've hit {shooter.ColoredName}!");
        }

        internal void HandlePlayerKilled(Object sender, PlayerKilledEventArgs args)
        {
            FPSMOGame game = (FPSMOGame)sender;

            foreach (Player player in game.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{args.Killer.ColoredName} &7killed {args.Victim.ColoredName}");
            }
        }

        internal void HandleAchievementUnlocked(Object sender, AchievementUnlockedEventArgs args)
        {
            Player who = args.Player;

            foreach (Player player in FPSMOGame.Instance.players.Values)
            {
                player.SendCpeMessage(CpeMessageType.Normal,
                    $"{who.ColoredName} &7unlocked &f{args.Achievement.Name}");
            }
        }

        internal void HandleJoinedLevel(Player player, Level from, Level to, ref bool announce)
        {
            ShowMap(player, to.name);
        }

        internal void HandlePluginLoaded(Object sender, EventArgs args)
        {
            Player[] players = PlayerInfo.Online.Items;

            foreach (Player player in players)
            {
                ShowMap(player, player.Level.name);
            }
        }

        internal void HandlePluginUnloading(Object sender, EventArgs args)
        {
            Player[] players = PlayerInfo.Online.Items;

            foreach (Player player in players)
            {
                player.SendCpeMessage(CpeMessageType.Status1, "");
            }
        }

        private void ShowMapInfo(Player player, Level level, FPSMOMapConfig mapConfig)
        {
            string authors = string.Join(", ", level.Config.Authors.Split(','));

            player.Message("&7Starting new round");
            player.Message($"&7This map was made by &f{authors}");

            if (mapConfig.TOTAL_RATINGS == 0)
            {
                player.Message("&7This map has not yet been rated");
            }
            else
            {
                float rating = mapConfig.Rating;
                string formattedRating = $"{RatingColor(rating)}{rating.ToString("0.00")}";
                player.Message($"&7This map has a rating of {formattedRating}");
            }
        }

        private string RatingColor(float rating)
        {
            if (0f <= rating && rating < 1f) return Colors.gray;
            if (1f <= rating && rating < 2f) return Colors.maroon;
            if (2f <= rating && rating < 3f) return Colors.gold;
            if (3f <= rating && rating < 4f) return Colors.green;
            else                             return Colors.lime;
        }

        private void ShowTeamStatistics(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Status2, "<Team statistics>");
        }

        private void ShowCountdown(Player player, int timeRemaining)
        {
            bool plural = (timeRemaining != 1);
            string message;

            if (plural) message = $"&2Starting in &f{timeRemaining} &2seconds";
            else message = $"&2Starting in &f{timeRemaining} &2second";

            player.SendCpeMessage(CpeMessageType.Announcement, message);
        }

        private void ShowNeedMorePlayers(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Normal, "&WNeed 2 or more non-ref players to start a round.");
        }

        private void ShowVoteOptions(Player player, string map1, string map2, string map3)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight2, "&ePick a level");
            player.SendCpeMessage(CpeMessageType.BottomRight1, $"&c{map1}, &a{map2}, &9{map3}");
        }

        private void ShowVoteTimeRemaining(Player player, int timeRemaining)
        {
            string message = String.Format($"&eVote time remaining: {timeRemaining}");
            player.SendCpeMessage(CpeMessageType.BottomRight3, message);
        }

        private void ShowVoteResults(Player player, string map1, string map2, string map3,
                                     int votes1, int votes2, int votes3)
        {
            player.SendCpeMessage(CpeMessageType.Normal, $"&7Votes are in!");
            player.SendCpeMessage(CpeMessageType.Normal, $"&c{map1}: {votes1} &ç{map2}: {votes2} {map3}: {votes3}");
        }

        private void ShowRoundTimeRemaining(Player player, int timeRemaining)
        {
            player.SendCpeMessage(CpeMessageType.Status3, $"&7Time remaining: &f{timeRemaining}");
        }

        private void ClearBottomRight(Player player)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight1, "");
            player.SendCpeMessage(CpeMessageType.BottomRight2, "");
            player.SendCpeMessage(CpeMessageType.BottomRight3, "");
        }

        private void ShowRoundStarted(Player player)
        {
            player.SendCpeMessage(CpeMessageType.Normal, "&7Round has started! You are no longer invincible.");
        }

        private void ShowInventory(Player player)
        {
            ShowHealthAndStamina(player, health: 10, stamina: 10);
            ShowWeaponReload(player, 10);
            ShowAmmunitions(player, 5);
        }

        private void ShowAmmunitions(Player player, int amount)
        {
            string message = StatusBar(Symbol.HAND_GUN, Symbol.HANDGUN_OUTLINE,
                                       amount, size: 10, insertSpaces: true);
            player.SendCpeMessage(CpeMessageType.BottomRight1, message);
        }

        private string ColoredBlocks(int amount, int total = 10)
        {
            char b = Symbol.SQUARE_FULL;
            string blocks = $"{b}{b}{b}{b}{b}{b}{b}{b}{b}{b}";
            return $"%4{blocks.Substring(amount)}%2{blocks.Substring(total - amount)}";
        }

        private string StatusBar(char fullChar, char halfChar, char emptyChar,
                                 int amount, int size, bool insertSpaces = false)
        {
            char[] bar = new char[size];

            int full = amount / 2;
            int half = amount % 2;

            for (int i = 0; i < full; i++)
            {
                bar[i] = fullChar;
            }

            for (int i = full; i < full + half; i++)
            {
                bar[i] = halfChar;
            }

            for (int i = full + half; i < size; i++)
            {
                bar[i] = emptyChar;
            }

            if (insertSpaces)
            {
                return Utils.InsertSpaceBetweenCharacters(new string(bar));
            }

            return new string(bar);
        }

        private string StatusBar(char fullChar, char emptyChar,
                                 int amount, int size, bool insertSpaces = false)
        {
            char[] bar = new char[size];
            int full = amount;

            for (int i = 0; i < full; i++)
            {
                bar[i] = fullChar;
            }

            for (int i = full; i < size; i++)
            {
                bar[i] = emptyChar;
            }

            if (insertSpaces)
            {
                return Utils.InsertSpaceBetweenCharacters(new string(bar));
            }

            return new string(bar);
        }

        private string HeartBar(int amount)
        {
            return StatusBar(Symbol.HEART_FULL, Symbol.HEART_HALF, Symbol.HEART_EMPTY, amount, size: 5);
        }

        private void ShowHealthAndStamina(Player player, int health, int stamina)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight3, $"{HeartBar(health)} &e{Symbol.Bold("Stamina")}&f: {stamina}");
        }

        private void ShowWeaponReload(Player player, int amount)
        {
            player.SendCpeMessage(CpeMessageType.BottomRight2, ColoredBlocks(amount));
        }

        private void ShowMap(Player player, string mapName)
        {
            player.SendCpeMessage(CpeMessageType.Status1, $"&e{Symbol.Bold("Map")}&f: {mapName}");
        }
    }
}