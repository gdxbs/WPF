using System;
using System.Media;
using System.Threading.Tasks;

namespace ChatClient.Services
{
    public class SoundService
    {
        private bool _soundEnabled = true;
        private int _volume = 70;
        private SoundPlayer _currentPlayer;

        public void SetSoundEnabled(bool enabled)
        {
            _soundEnabled = enabled;
        }

        public void SetVolume(int volume)
        {
            _volume = Math.Clamp(volume, 0, 100);
        }

        public async Task PlayNotificationAsync(string eventType)
        {
            if (!_soundEnabled)
                return;

            await Task.Run(() => PlaySound(eventType));
        }

        private void PlaySound(string eventType)
        {
            try
            {
                string soundFile = GetSystemSoundPath(eventType);

                if (string.IsNullOrEmpty(soundFile))
                    return;

                _currentPlayer?.Dispose();
                _currentPlayer = new SoundPlayer(soundFile);
                _currentPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sound playback error: {ex.Message}");
            }
        }

        private string GetSystemSoundPath(string eventType)
        {
            return eventType switch
            {
                "message" => @"C:\Windows\Media\Notification.Default.wav",
                "join_leave" => @"C:\Windows\Media\tada.wav",
                "error" => @"C:\Windows\Media\Windows Error.wav",
                _ => null
            };
        }

        public void Dispose()
        {
            _currentPlayer?.Dispose();
        }
    }
}
