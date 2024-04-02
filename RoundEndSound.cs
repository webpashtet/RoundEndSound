using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Menu;
using Dapper;
using MySqlConnector;
using RoundEndSound.Models;
using RoundEndSound.Repository;
using RoundEndSound.Services;
using static RoundEndSound.Repository.QueryConstants;

namespace RoundEndSound
{
    [MinimumApiVersion(199)]
    public class RoundEndSound : BasePlugin, IPluginConfig<Config.Config>
    {
        public override string ModuleName => "Round End Sound";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "gleb_khlebov";
        public override string ModuleDescription => "Plays a sound at the end of the round";
        
        public Config.Config Config { get; set; } = new();
        
        private static string? _connectionString;
        private static RepositoryService? _databaseService;
        
        private readonly Random _random = new();
        private readonly Utils.LogUtils _logUtils = new();
        private readonly Utils.PlayerUtils _playerUtils = new();
        
        private int _trackCount;
        private static Sound? _lastPlayedTrack;
        private static List<Sound> _tracks = [];
        
        private readonly HashSet<string> _playersHotLoaded = [];
        private readonly Dictionary<string, ResPlayer?> _players = [];
        private readonly Dictionary<string, ResPlayer?> _playersForSave = [];

        private const char NewLine = '\u2029';

        public override void Load(bool hotReload)
        {
            base.Load(hotReload);

            Console.WriteLine(" ");
            Console.WriteLine("  _____                       _   ______           _    _____                       _ ");
            Console.WriteLine(" |  __ \\                     | | |  ____|         | |  / ____|                     | |");
            Console.WriteLine(" | |__) |___  _   _ _ __   __| | | |__   _ __   __| | | (___   ___  _   _ _ __   __| |");
            Console.WriteLine(" |  _  // _ \\| | | | '_ \\ / _` | |  __| | '_ \\ / _` |  \\___ \\ / _ \\| | | | '_ \\ / _` |");
            Console.WriteLine(" | | \\ \\ (_) | |_| | | | | (_| | | |____| | | | (_| |  ____) | (_) | |_| | | | | (_| |");
            Console.WriteLine(" |_|  \\_\\___/ \\__,_|_| |_|\\__,_| |______|_| |_|\\__,_| |_____/ \\___/ \\__,_|_| |_|\\__,_|");
            Console.WriteLine("                                                                                      ");
            Console.WriteLine("                                                                                      ");
            Console.WriteLine("			    >> Version: " + ModuleVersion);
            Console.WriteLine("			    >> Author: " + ModuleAuthor);
            Console.WriteLine(" ");

            if (hotReload)
            {
                UpdatePlayersAfterReload();
            }
            
            RegisterEventHandler<EventRoundMvp>((@event, info) =>
            {
                if (!Config.DisableMvpSound) return HookResult.Continue;
            
                info.DontBroadcast = true;
            
                return HookResult.Continue;
            }, HookMode.Pre);
            
            AddCommand("css_res", "Command that opens the Round End Sound menu",
                (player, _) => CreateMenu(player));
        }
        
        public override void Unload(bool hotReload)
        {
            base.Unload(hotReload);
            
            _tracks.Clear();
            _players.Clear();
            _playersForSave.Clear();
            _playersHotLoaded.Clear();
            
            _lastPlayedTrack = null;
        }
        
        public void OnConfigParsed(Config.Config config)
        {
            Database dbConfig = config.DbConfig;
            
            if (dbConfig.Host.Length < 1 || dbConfig.DbName.Length < 1 || dbConfig.User.Length < 1)
            {
                _logUtils.Log("You need to setup Database credentials in config!");
                throw new Exception("[Round End Sound] You need to setup Database credentials in config!");
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = dbConfig.Host,
                UserID = dbConfig.User,
                Password = dbConfig.Password,
                Database = dbConfig.DbName,
                Port = (uint)dbConfig.Port,
                Pooling = true
            };

            _connectionString = builder.ConnectionString;

            _databaseService = new RepositoryService(_connectionString);
            Task.Run(() => _databaseService.CreateTable());
            
            _tracks = config.MusicList;
            _trackCount = _tracks.Count;
            Config = config;
        }
        
        [GameEventHandler]
        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            CCSPlayerController? player = @event.Userid;
            
            if (_playerUtils.IsInvalidPlayer(player))
                return HookResult.Continue;

            ResPlayer resPlayer = new ResPlayer
            {
                SteamId = player.SteamID.ToString(),
                SoundEnabled = Config.DefaultEnableMusic,
                ChatEnabled = Config.DefaultEnableNotify,
            };
            
            Task.Run(() => GetPlayerAsync(resPlayer, currentTime));
            
            return HookResult.Continue;
        }
        
        [GameEventHandler]
        public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            string steamId = player.SteamID.ToString();
            
            if (_playerUtils.IsInvalidPlayer(player))
                return HookResult.Continue;

            if (!_players.ContainsKey(steamId))
                return HookResult.Continue;

            if (_playersForSave.TryGetValue(steamId, out ResPlayer? value))
            {
                Task.Run(() => SavePlayersAsync([value]));
                _playersForSave.Remove(steamId);
            }
            
            _players.Remove(steamId);

            return HookResult.Continue;
        }
        
        [GameEventHandler]
        public HookResult OnServerShutdown(EventServerShutdown @event, GameEventInfo info)
        {
            if (_playersForSave.Count < 1)
                return HookResult.Continue;
            
            Task.Run(() => SavePlayersAsync(_playersForSave.Values.ToList()));
            
            return HookResult.Continue;
        }
        
        [GameEventHandler]
        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if (_trackCount < 1)
                return HookResult.Continue;
            
            int trackIndex = _random.Next(_trackCount);
            Sound currentSound = _tracks[trackIndex];
            
            foreach (var player in _players.Select(resPlayer => resPlayer.Value))
            {
                PlaySound(player, currentSound);
            }

            _lastPlayedTrack = currentSound;

            return HookResult.Continue;
        }

        private void PlaySound(ResPlayer? resPlayer, Sound sound)
        {
            CCSPlayerController? player = Utils.PlayerUtils.GetPlayerFromSteamId(resPlayer!.SteamId);
            
            Server.NextFrame(() =>
            {   
                if (resPlayer.SoundEnabled)
                    player?.ExecuteClientCommand($"play {sound.Path}");
                if (resPlayer.ChatEnabled)
                    player?.PrintToChat($"{Localizer["chat.Prefix"]}{Localizer["chat.PlayedSong", sound.Name]}");
            });
        }
        
        private void PlayLastSound(CCSPlayerController player)
        {
            Server.NextFrame(() =>
            {   
                player.ExecuteClientCommand($"play {_lastPlayedTrack!.Path}");
                player.PrintToChat($"{Localizer["chat.Prefix"]}{Localizer["chat.PlayedSong", _lastPlayedTrack.Name]}");
            });
        }
        
        private async Task GetPlayerAsync(ResPlayer? resPlayer, long currentTime)
        {
            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                await connection.ExecuteAsync(InsertPlayer,
                    new { SteamID = resPlayer?.SteamId, resPlayer?.SoundEnabled, 
                        resPlayer?.ChatEnabled, LastConnect = currentTime });
                
                var playerData = await connection.QuerySingleOrDefaultAsync<ResPlayer>(
                    SelectPlayer, new {resPlayer?.SteamId});
                
                if (playerData == null)
                    return;
                
                InsertPlayerData(playerData); 
            }
            catch (Exception ex)
            {
                _logUtils.Log($"Error in GetPlayerAsync: {ex.Message}");
            }
        }
        
        private async Task GetPlayersAsync(HashSet<string> players)
        {
            if (players.Count < 1) return;
            
            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var playersData = connection.QueryAsync<ResPlayer>(SelectPlayers,
                    new {players}).Result.ToList();

                foreach (var resPlayer in playersData)
                {
                    InsertPlayerData(resPlayer);
                } 
            }
            catch (Exception ex)
            {
                _logUtils.Log($"Error in GetPlayersAsync: {ex.Message}");
            }
        }
        
        private async Task SavePlayersAsync(List<ResPlayer?> players)
        {
            if (players.Count < 1) return;
            
            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                await connection.ExecuteAsync(UpdatePlayer, players);
            }
            catch (Exception ex)
            {
                _logUtils.Log($"Error in SavePlayersAsync: {ex.Message}");
            }
        }
        
        private void InsertPlayerData(ResPlayer resPlayer)
        {
            Server.NextFrame(() =>
            {
                _players[resPlayer.SteamId!] = resPlayer;
            });    
        }

        private void UpdatePlayersAfterReload()
        {
            foreach (var player in Utils.PlayerUtils.GetOnlinePlayers())
            {
                if (_playerUtils.IsInvalidPlayer(player))
                    return;

                _playersHotLoaded.Add(player.SteamID.ToString());
            }

            Task.Run(() => GetPlayersAsync(_playersHotLoaded));
        }
        
        private void CreateMenu(CCSPlayerController? player)
        {
            if (player == null) return;

            string steamId = player.SteamID.ToString();

            if (!_players.TryGetValue(steamId, out var user)) return;

            bool lastPlayedTrackIsNull = _lastPlayedTrack == null;
            string menuTitle = Localizer["menu.Title"];
            var title = lastPlayedTrackIsNull
                ? menuTitle
                : menuTitle + $"{NewLine}" + Localizer["menu.LastPlayedSong", _lastPlayedTrack!.Name];
            
            user!.Menu = new ChatMenu(title);

            if (user.Menu == null)
            {
                _logUtils.Log("user.Menu is nullable");
                return;
            }
            
            foreach (var (feature, state) in user.Settings)
            {
                var featureState = BoolStateToString(state);
                
                user.Menu.AddMenuOption(
                    Localizer[feature] + $" {featureState}",
                    (controller, _) =>
                    {
                        bool changedState = !state;
                        user.SetBoolProperty(feature, changedState);

                        string returnState = BoolStateToString(changedState);
                        
                        _players[steamId] = user;
                        _playersForSave[steamId] = user;

                        if (Config.ReOpenMenuAfterItemClick)
                        {
                            CreateMenu(controller);
                        }
                        else
                        {
                            player.PrintToChat($"{Localizer["chat.Prefix"]}{Localizer[feature]}: {returnState}");
                        }
                    });
            }
            
            if (!lastPlayedTrackIsNull)
                user.Menu.AddMenuOption(
                    Localizer["menu.ListenLastPlayedSong"],
                    (controller, _) =>
                    {
                        PlayLastSound(controller);
                    });
            
            MenuManager.OpenChatMenu(player, (ChatMenu)user.Menu);
        }
        
        private string BoolStateToString(bool state)
        {
            return state switch
            {
                true => $"{Localizer["chat.Enabled"]}",
                false => $"{Localizer["chat.Disabled"]}"
            };
        }
    }
}