using Dapper;
using MySqlConnector;
using RoundEndSound.Utils;

namespace RoundEndSound.Services;

    public class RepositoryService(string dbConnectionString)
    {
        internal LogUtils LogUtils = new();

        public async Task CreateTable()
        {
            try
            {
                await using var connection = new MySqlConnection(dbConnectionString);
                await connection.OpenAsync();
                
                string createResUsersTable = @"
                CREATE TABLE IF NOT EXISTS `res_players_settings` (
                    `steam_id` VARCHAR(17) NOT NULL,
                    `sound_enabled` BOOLEAN NOT NULL,
                    `chat_enabled` BOOLEAN NOT NULL,
                    `last_connect` BIGINT NOT NULL,
                PRIMARY KEY (`steam_id`));";
                
                await connection.ExecuteAsync(createResUsersTable);
            }
            catch (Exception ex)
            {
                LogUtils.Log(ex.Message);
            }
        }
    }