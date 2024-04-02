using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RoundEndSound.Utils;

public class PlayerUtils
{
    public static CCSPlayerController? GetPlayerFromSteamId(string? steamId)
    {
        return Utilities.GetPlayers().FirstOrDefault(u =>
            u.AuthorizedSteamID != null &&
            (u.AuthorizedSteamID.SteamId2.ToString().Equals(steamId) ||
             u.AuthorizedSteamID.SteamId64.ToString().Equals(steamId) ||
             u.AuthorizedSteamID.AccountId.ToString().Equals(steamId)));
    }
    
    public static List<CCSPlayerController> GetOnlinePlayers(bool getBots = false)
    {
        var players = Utilities.GetPlayers();

        List<CCSPlayerController> validPlayers = new List<CCSPlayerController>();

        foreach (var p in players)
        {
            if (!p.IsValid) continue;
            if (p.AuthorizedSteamID == null && !getBots) continue;
            if (p.IsBot && !getBots) continue;
            if (p.Connected != PlayerConnectedState.PlayerConnected) continue;
            validPlayers.Add(p);
        }

        return validPlayers;
    }
    
    public bool IsInvalidPlayer(CCSPlayerController player)
    {
        return player is { IsBot: true, IsHLTV: true, IsValid: false };
    }
}