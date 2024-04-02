namespace RoundEndSound.Repository;

public class QueryConstants
{
    public const string SelectPlayer = @"select steam_id as SteamId, sound_enabled as SoundEnabled,
                    chat_enabled as ChatEnabled
                    from `res_players_settings` where steam_id = @SteamId";
    
    public const string SelectPlayers = @"select steam_id as SteamId, sound_enabled as SoundEnabled,
                    chat_enabled as ChatEnabled
                    from `res_players_settings` where steam_id in @Players";
    
    public const string InsertPlayer = @"INSERT INTO `res_players_settings` (steam_id, sound_enabled, chat_enabled, last_connect)
                    VALUES (@SteamID, @SoundEnabled, @ChatEnabled, @LastConnect) ON DUPLICATE KEY UPDATE last_connect = @LastConnect;";
    
    public const string UpdatePlayer = @"UPDATE `res_players_settings` SET sound_enabled = @SoundEnabled, chat_enabled = @ChatEnabled
                    WHERE steam_id = @SteamId";
}