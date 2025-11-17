using System;
using System.Threading.Tasks;
using ChatClient.Models;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest.Extensions;

namespace ChatClient.Services
{
    public class PreferencesService
    {
        private Supabase.Client _supabaseClient;
        private UserPreferences _currentPreferences;

        public PreferencesService()
        {
        }

        public async Task InitializeAsync(string supabaseUrl, string supabaseAnonKey)
        {
            try
            {
                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false
                };

                _supabaseClient = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
                await _supabaseClient.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Supabase", ex);
            }
        }

        public async Task<UserPreferences> LoadPreferencesAsync(Guid userId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<UserPreferences>()
                    .Select("*")
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                    .Single();

                _currentPreferences = response ?? CreateDefaultPreferences(userId);
                return _currentPreferences;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading preferences: {ex.Message}. Creating default preferences.");
                _currentPreferences = CreateDefaultPreferences(userId);
                return _currentPreferences;
            }
        }

        public async Task<bool> SavePreferencesAsync(UserPreferences preferences)
        {
            try
            {
                preferences.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(preferences.Id))
                {
                    preferences.Id = Guid.NewGuid().ToString();
                    preferences.CreatedAt = DateTime.UtcNow;

                    await _supabaseClient
                        .From<UserPreferences>()
                        .Insert(preferences);
                }
                else
                {
                    await _supabaseClient
                        .From<UserPreferences>()
                        .Update(preferences);
                }

                _currentPreferences = preferences;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving preferences: {ex.Message}");
                return false;
            }
        }

        public UserPreferences GetCurrentPreferences(Guid userId)
        {
            return _currentPreferences ?? CreateDefaultPreferences(userId);
        }

        private UserPreferences CreateDefaultPreferences(Guid userId)
        {
            return new UserPreferences
            {
                UserId = userId,
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
    }
}
