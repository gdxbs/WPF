using System;
using System.Threading.Tasks;
using ChatClient.Models;
using Supabase;
using Supabase.Gotrue;

namespace ChatClient.Services
{
    public class PreferencesService
    {
        private Client _supabaseClient;
        private string _machineId;
        private UserPreferences _currentPreferences;

        public PreferencesService()
        {
            _machineId = GetOrCreateMachineId();
        }

        public async Task InitializeAsync(string supabaseUrl, string supabaseAnonKey)
        {
            try
            {
                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false
                };

                _supabaseClient = new Client(supabaseUrl, supabaseAnonKey, options);
                await _supabaseClient.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Supabase", ex);
            }
        }

        public async Task<UserPreferences> LoadPreferencesAsync()
        {
            try
            {
                var response = await _supabaseClient
                    .From<UserPreferences>("user_preferences")
                    .Select("*")
                    .Eq("machine_id", _machineId)
                    .Single();

                _currentPreferences = response ?? CreateDefaultPreferences();
                return _currentPreferences;
            }
            catch (Exception ex)
            {
                _currentPreferences = CreateDefaultPreferences();
                return _currentPreferences;
            }
        }

        public async Task<bool> SavePreferencesAsync(UserPreferences preferences)
        {
            try
            {
                preferences.MachineId = _machineId;
                preferences.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(preferences.Id))
                {
                    preferences.Id = Guid.NewGuid().ToString();
                    preferences.CreatedAt = DateTime.UtcNow;

                    await _supabaseClient
                        .From<UserPreferences>("user_preferences")
                        .Insert(preferences);
                }
                else
                {
                    await _supabaseClient
                        .From<UserPreferences>("user_preferences")
                        .Update(preferences);
                }

                _currentPreferences = preferences;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public UserPreferences GetCurrentPreferences()
        {
            return _currentPreferences ?? CreateDefaultPreferences();
        }

        private UserPreferences CreateDefaultPreferences()
        {
            return new UserPreferences
            {
                MachineId = _machineId,
                ThemeMode = "light",
                AccentColor = "#0078D4",
                FontSize = 12,
                WindowTransparency = 100,
                SoundEnabled = true,
                NotificationVolume = 70,
                MessageNotificationEnabled = true,
                JoinLeaveNotificationEnabled = true,
                ErrorNotificationEnabled = true,
                AutoConnectEnabled = false,
                LastConnectionHost = "localhost",
                LastConnectionPort = 9999,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private string GetOrCreateMachineId()
        {
            string registryPath = @"Software\ChatClient\AppSettings";
            string valueName = "MachineId";

            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath);

                if (key?.GetValue(valueName) is string existingId)
                {
                    return existingId;
                }

                string newMachineId = Guid.NewGuid().ToString();
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath);
                key?.SetValue(valueName, newMachineId);

                return newMachineId;
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }
    }
}
