using System;
using System.Collections.Generic;
using System.Linq;
using FPS.Entities;
using FPS.Weapons;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;

namespace FPS;

internal class GUI
{
    private FPSGame _game;
    private List<Player> Players => _game.Players.Values.ToList();
    private Dictionary<CpeMessageType, string> _persistentMessages;
    private object _persistentMessagesLock = new();

    internal GUI(FPSGame game)
    {
        _game = game;
        _persistentMessages = new Dictionary<CpeMessageType, string>();
    }

    internal void Observe(FPSGame game)
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
        game.CountdownFailed += HandleCountdownFailed;
    }

    internal void Unobserve(FPSGame game)
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
        game.CountdownFailed -= HandleCountdownFailed;
    }

    internal void Observe(AchievementsManager manager)
    {
        manager.AchievementUnlocked += HandleAchievementUnlocked;
    }

    internal void Unobserve(AchievementsManager manager)
    {
        manager.AchievementUnlocked -= HandleAchievementUnlocked;
    }

    internal void Observe(FPSMOPlugin plugin)
    {
        plugin.PluginLoaded += HandlePluginLoaded;
        plugin.PluginUnloading += HandlePluginUnloading;
    }

    internal void Unobserve(FPSMOPlugin plugin)
    {
        plugin.PluginLoaded -= HandlePluginLoaded;
        plugin.PluginUnloading += HandlePluginUnloading;
    }

    internal void ObserveJoinedLevel()
    {
        OnJoinedLevelEvent.Register(HandleJoinedLevel, Priority.Normal);
    }

    internal void UnobserveJoinedLevel()
    {
        OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
    }

    private void MessageAll(string message, CpeMessageType type = CpeMessageType.Normal, bool persist = false)
    {
        bool isBottomRight = (11 <= (int)type && (int)type <= 13);
        bool isStatus = (1 <= (int)type && (int)type <= 3);

        if (persist)
        {
            if (!isBottomRight && !isStatus)
                throw new ArgumentException("Only BottomRight and Status messages can persist.", nameof(persist));

            lock (_persistentMessagesLock)
            {
                _persistentMessages[type] = message;
            }
        }
        else if (isBottomRight || isStatus)
        {
            lock (_persistentMessagesLock)
            {
                _persistentMessages.Remove(type);
            }
        }

        foreach (Player player in Players)
        {
            player.SendCpeMessage(type, message);
        }
    }

    private void SendPersistentMessages(Player player)
    {
        foreach (CpeMessageType type in _persistentMessages.Keys)
        {
            player.SendCpeMessage(type, _persistentMessages[type]);
        }
    }

    internal void HandleCountdownStarted(Object sender, CountdownStartedEventArgs args)
    {
        FPSGame game = (FPSGame)sender;
        MessageAll("&SStarting new round...");
        ShowMapInfo(game.Map, args.MapAverageRating);
        ShowTeamStatistics();

        foreach (Player player in Players)
        {
            ShowInventory(player);
        }
    }

    internal void HandlePlayerShotWeapon(Object sender, PlayerShotWeaponEventArgs args)
    {
        Player player = args.Player;

        Weapon gun = PlayerDataHandler.Instance[player.truename].gun;
        PlayerDataHandler.Instance[player.truename].currentWeapon = gun;

        // Can't shoot if the status below 10
        if (gun.GetStatus(WeaponHandler.Tick) < 10)
        {
            return;
        }

        ShowWeaponReload(player, 0);
        PlayerDataHandler.Instance[player.truename].gun.Use(player.Rot, player.Pos.ToVec3F32());   // This takes care of the status too
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
        if (args.TimeRemaining <= 10 || args.TimeRemaining % 5 == 0)
        {
            ShowCountdown(args.TimeRemaining);
        }
    }

    internal void HandleCountdownEnded(Object sender, EventArgs args)
    {
        MessageAll("", CpeMessageType.Announcement);
    }

    internal void HandleRoundStarted(Object sender, EventArgs args)
    {
        ShowTeamStatistics();
        MessageAll("Round has started!");
    }

    internal void HandleRoundTicked(Object sender, RoundTickedEventArgs args)
    {
        MessageAll($"&STime remaining: &T{args.TimeRemaining}", CpeMessageType.Status3, persist: true);
    }

    internal void HandleRoundEnded(Object sender, EventArgs args)
    {
        MessageAll("", CpeMessageType.Status2, persist: true);
        MessageAll("", CpeMessageType.Status3, persist: true);
        MessageAll("", CpeMessageType.BottomRight1, persist: true);
        MessageAll("", CpeMessageType.BottomRight2, persist: true);
        MessageAll("", CpeMessageType.BottomRight3, persist: true);
        MessageAll("&SThe &T<winning team> &Swins!");
    }

    internal void HandleVoteStarted(Object sender, VoteStartedEventArgs args)
    {
        string map1 = args.Map1;
        string map2 = args.Map2;
        string map3 = args.Map3;

        int count = (map3 is null) ? 2 : 3;

        if (count == 2)
        {
            MessageAll("&SLevel vote - type &a1 &Sor &b2&S.", CpeMessageType.BottomRight3, persist: true);
            MessageAll($"&a{map1}&S, &b{map2}", CpeMessageType.BottomRight2, persist: true);
        }
        else if (count == 3)
        {
            MessageAll("&SLevel vote - type &a1&S, &b2&S &Sor &c3&S.", CpeMessageType.BottomRight3, persist: true);
            MessageAll($"&a{map1}&S, &b{map2}&S, &c{map3}", CpeMessageType.BottomRight2, persist: true);
        }
    }

    internal void HandleVoteTicked(Object sender, VoteTickedEventArgs args)
    {
        string message = $"&T{args.TimeRemaining}s &Sleft to vote.";
        MessageAll(message, CpeMessageType.BottomRight1, persist: true);
    }

    internal void HandleVoteEnded(Object sender, VoteEndedEventArgs args)
    {
        FPSGame game = (FPSGame)sender;
        List<Player> players = game.Players.Values.ToList();

        MessageAll("", CpeMessageType.BottomRight1);
        MessageAll("", CpeMessageType.BottomRight2);
        MessageAll("", CpeMessageType.BottomRight3);
        MessageAll("&SVotes are in!");

        string[] maps = args.Maps;
        int[] votes = args.Votes;
        string results;

        if (maps.Length == 3)
            results = $"&a{maps[0]}: {votes[0]}&S, &b{maps[1]}: {votes[1]}&S, &c{maps[2]}: {votes[2]}&S";
        else
            results = $"&a{maps[0]}: {votes[0]}&S, &b{maps[1]}: {votes[1]}&S";

        MessageAll(results);
    }

    internal void HandlePlayerJoined(Object sender, PlayerJoinedEventArgs args)
    {
        Player player = args.Player;
        ShowInventory(player);
        SendPersistentMessages(player);
    }

    internal void HandlePlayerLeft(Object sender, PlayerLeftEventArgs args)
    {
        Player player = args.Player;

        player.SendCpeMessage(CpeMessageType.BottomRight1, "");
        player.SendCpeMessage(CpeMessageType.BottomRight2, "");
        player.SendCpeMessage(CpeMessageType.BottomRight3, "");
        player.SendCpeMessage(CpeMessageType.Status2, "");
        player.SendCpeMessage(CpeMessageType.Status3, "");
    }

    internal void HandleGameStopped(Object sender, EventArgs args)
    {
        MessageAll("", CpeMessageType.Status2);
        MessageAll("", CpeMessageType.Status3);
        MessageAll("", CpeMessageType.BottomRight1);
        MessageAll("", CpeMessageType.BottomRight2);
        MessageAll("", CpeMessageType.BottomRight3);
        MessageAll("", CpeMessageType.Announcement);

        Chat.MessageAll("&SThe FPS game has been stopped.");
    }

    internal void HandleWeaponSpeedChanged(Object sender, WeaponSpeedChangedEventArgs args)
    {
        Player player = args.Player;
        int amount = args.Amount;

        amount = Utils.Clamp(amount, 0, 10);
        player.SendCpeMessage(CpeMessageType.SmallAnnouncement, ColoredBlocks(amount));
    }

    internal void HandlePlayerJoinedTeam(Object sender, PlayerJoinedTeamEventArgs args)
    {
        string color = (args.TeamName.ToUpper() == "RED") ? "&c" : "&9";
        MessageAll($"{args.Player.name} &Sjoined team {color}{args.TeamName.ToUpper()}&S!");
    }

    internal void HandlePlayerHit(Object sender, PlayerHitEventArgs args)
    {
        Player victim = args.Victim;
        Player shooter = args.Shooter;

        victim.Message($"&SYou got hit by {shooter.ColoredName}");
        shooter.Message($"&SYou've hit {shooter.ColoredName}!");
    }

    internal void HandlePlayerKilled(Object sender, PlayerKilledEventArgs args)
    {
        FPSGame game = (FPSGame)sender;

        foreach (Player player in game.Players.Values)
        {
            player.SendCpeMessage(CpeMessageType.Normal,
                $"{args.Killer.ColoredName} &Skilled {args.Victim.ColoredName}");
        }
    }

    internal void HandleAchievementUnlocked(Object sender, AchievementUnlockedEventArgs args)
    {
        Player who = args.Player;
        MessageAll($"{who.ColoredName} &Sunlocked &f{args.Achievement.Name}");
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

    internal void HandleCountdownFailed(Object sender, EventArgs args)
    {
		MessageAll("&WNeed 2 or more non-ref players to start a round.");
    }

    private void ShowMapInfo(Level level, float? rating)
    {
        string authors = string.Join(", ", level.Config.Authors.Split(','));

        if (authors.Length != 0)
        {
            MessageAll($"&SThis map was made by &T{authors}&S.");
        }

        if (rating != null)
        {
            string formattedRating = $"{RatingColor(rating.Value)}{rating.Value:0.00}";
            MessageAll($"&SThis map has an average rating of &T{formattedRating}/5&S.");
        }
        else
        {
            MessageAll("&SThis map has not yet been rated.");
        }
    }

    private string RatingColor(float rating)
    {
        if (0f <= rating && rating < 1f) return Colors.gray;
        if (1f <= rating && rating < 2f) return Colors.maroon;
        if (2f <= rating && rating < 3f) return Colors.gold;
        if (3f <= rating && rating < 4f) return Colors.green;
        else return Colors.lime;
    }

    private void ShowTeamStatistics()
    {
        MessageAll("<Team statistics>", CpeMessageType.Status2, persist: true);
    }

    private void ShowCountdown(int timeRemaining)
    {
        bool plural = (timeRemaining != 1);
        string message;

        if (plural) message = $"&4Starting in &f{timeRemaining} &4seconds";
        else message = $"&4Starting in &f{timeRemaining} &4second";

        MessageAll(message, CpeMessageType.Announcement);
    }

    private void ShowInventory(Player player)
    {
        ShowHealth(player, 20);
        ShowWeaponSelection(player);
        ShowWeaponReload(player, 10);
    }

    private void ShowWeaponSelection(Player player)
    {
        player.SendCpeMessage(CpeMessageType.BottomRight2, "<Weapon selection>");
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
        return StatusBar(Symbol.HEART_FULL, Symbol.HEART_HALF, Symbol.HEART_EMPTY, amount, size: 10);
    }

    private void ShowHealth(Player player, int health)
    {
        player.SendCpeMessage(CpeMessageType.BottomRight3, HeartBar(health));
    }

    private void ShowWeaponReload(Player player, int amount)
    {
        player.SendCpeMessage(CpeMessageType.BottomRight1, ColoredBlocks(amount));
    }

    private void ShowMap(Player player, string mapName)
    {
        player.SendCpeMessage(CpeMessageType.Status1, $"&e{Symbol.Bold("Map")}&f: {mapName}");
    }
}
