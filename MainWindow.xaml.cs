using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using ChatClient.Models;
using ChatClient.Services;
using ChatClient.Themes;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private NetworkService _networkService;
        private PreferencesService _preferencesService;
        private MessageRouterService _messageRouter;
        private SoundService _soundService;
        private Dictionary<string, ChatRoom> _joinedRooms;
        private string _currentRoomId;

        public MainWindow()
        {
            InitializeComponent();

            _joinedRooms = new Dictionary<string, ChatRoom>();
            _messageRouter = new MessageRouterService();
            _soundService = new SoundService();
            _networkService = new NetworkService();
            _preferencesService = new PreferencesService();

            InitializeServices();
        }

        private async void InitializeServices()
        {
            try
            {
                await _preferencesService.InitializeAsync(
                    "https://ndunxomhastqvcbtfrhp.supabase.co",
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5kdW54b21oYXN0cXZjYnRmcmhwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMyMzYxNjgsImV4cCI6MjA3ODgxMjE2OH0.z1KObMN-GXbBX4KKZm90CT1D2I_Teecf6DkPKEaNn4s");

                var preferences = await _preferencesService.LoadPreferencesAsync();

                ThemeManager.ApplyTheme(preferences.ThemeMode);
                ThemeManager.ApplyAccentColor(preferences.AccentColor);

                HostTextBox.Text = preferences.LastConnectionHost;
                PortTextBox.Text = preferences.LastConnectionPort.ToString();

                _soundService.SetSoundEnabled(preferences.SoundEnabled);
                _soundService.SetVolume(preferences.NotificationVolume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubscribeToNetworkEvents()
        {
            _networkService.MessageReceived += NetworkService_MessageReceived;
            _networkService.ConnectionStatusChanged += NetworkService_ConnectionStatusChanged;
            _networkService.ErrorOccurred += NetworkService_ErrorOccurred;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            _networkService.MessageReceived -= NetworkService_MessageReceived;
            _networkService.ConnectionStatusChanged -= NetworkService_ConnectionStatusChanged;
            _networkService.ErrorOccurred -= NetworkService_ErrorOccurred;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string host = HostTextBox.Text.Trim();
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                MessageBox.Show("Invalid port number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ConnectButton.IsEnabled = false;

            bool connected = await _networkService.ConnectAsync(host, port);

            if (connected)
            {
                SubscribeToNetworkEvents();
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                HostTextBox.IsEnabled = false;
                PortTextBox.IsEnabled = false;

                SaveConnectionDetails(host, port);
            }
            else
            {
                ConnectButton.IsEnabled = true;
                MessageBox.Show("Failed to connect to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();
        }

        private void DisconnectFromServer()
        {
            _networkService.Disconnect();
            UnsubscribeFromNetworkEvents();

            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            HostTextBox.IsEnabled = true;
            PortTextBox.IsEnabled = true;
            LeaveRoomButton.IsEnabled = false;

            _joinedRooms.Clear();
            ChatTabControl.Items.Clear();
            UsersListBox.Items.Clear();
            RoomListBox.Items.Clear();
        }

        private async void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string roomId = CreateRoomIdTextBox.Text.Trim();
            string roomName = CreateRoomNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(roomName))
            {
                MessageBox.Show("Please enter both room ID and name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_networkService.IsConnected())
            {
                MessageBox.Show("Not connected to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await _networkService.SendCommandAsync($"start {roomId} {roomName}");

            CreateRoomIdTextBox.Clear();
            CreateRoomNameTextBox.Clear();
        }

        private async void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string roomId = JoinRoomIdTextBox.Text.Trim();
            string username = JoinUsernameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter both room ID and username", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_networkService.IsConnected())
            {
                MessageBox.Show("Not connected to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await _networkService.SendCommandAsync($"join {roomId} {username}");
        }

        private async void ListRoomsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_networkService.IsConnected())
            {
                MessageBox.Show("Not connected to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await _networkService.SendCommandAsync("list");
        }

        private async void LeaveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentRoomId))
            {
                MessageBox.Show("No room selected", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _networkService.SendCommandAsync("quit");
        }

        private void NetworkService_MessageReceived(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProcessReceivedMessage(message);
            });
        }

        private void ProcessReceivedMessage(string message)
        {
            if (message.Contains("You join @chatroom"))
            {
                HandleRoomJoined(message);
            }
            else if (message.Contains("joined us @chatroom"))
            {
                HandleUserJoined(message);
            }
            else if (message.Contains("quitted from chatroom"))
            {
                HandleRoomLeft(message);
            }
            else if (message.StartsWith("No room exists") || message.Contains("@chatroom:"))
            {
                HandleListRoomsResponse(message);
            }
            else if (message.Contains("Server:"))
            {
                HandleServerMessage(message);
            }
            else
            {
                HandleChatMessage(message);
            }
        }

        private void HandleRoomJoined(string message)
        {
            string roomId = ExtractRoomId(message);

            if (!_joinedRooms.ContainsKey(roomId))
            {
                var room = new ChatRoom { RoomId = roomId, RoomName = roomId };
                _joinedRooms.Add(roomId, room);
                _messageRouter.RegisterRoom(roomId, room);

                AddRoomTab(roomId);
                _currentRoomId = roomId;
                LeaveRoomButton.IsEnabled = true;
            }

            if (_currentRoomId == roomId && _joinedRooms.TryGetValue(roomId, out var chatRoom))
            {
                chatRoom.AddMessage($"[System] {message}");
            }

            _ = _soundService.PlayNotificationAsync("join_leave");
        }

        private void HandleUserJoined(string message)
        {
            string roomId = ExtractRoomId(message);

            if (_joinedRooms.TryGetValue(roomId, out var room))
            {
                room.AddMessage($"[System] {message}");
                RefreshUsersList();
            }

            _ = _soundService.PlayNotificationAsync("join_leave");
        }

        private void HandleRoomLeft(string message)
        {
            string roomId = ExtractRoomId(message);

            if (_joinedRooms.TryGetValue(roomId, out var room))
            {
                RemoveRoomTab(roomId);
                _joinedRooms.Remove(roomId);
                _messageRouter.UnregisterRoom(roomId);

                if (_currentRoomId == roomId)
                {
                    _currentRoomId = _joinedRooms.Keys.FirstOrDefault();
                    if (string.IsNullOrEmpty(_currentRoomId))
                    {
                        LeaveRoomButton.IsEnabled = false;
                    }
                }
            }
        }

        private void HandleListRoomsResponse(string message)
        {
            RoomListBox.Items.Clear();

            string[] lines = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line != "No room exists!")
                {
                    RoomListBox.Items.Add(line.Trim());
                }
            }
        }

        private void HandleServerMessage(string message)
        {
            if (!string.IsNullOrEmpty(_currentRoomId) && _joinedRooms.TryGetValue(_currentRoomId, out var room))
            {
                room.AddMessage($"[Server] {message}");
            }

            _ = _soundService.PlayNotificationAsync("message");
        }

        private void HandleChatMessage(string message)
        {
            if (!string.IsNullOrEmpty(_currentRoomId) && _joinedRooms.TryGetValue(_currentRoomId, out var room))
            {
                room.AddMessage(message);
                RefreshUsersList();
            }

            _ = _soundService.PlayNotificationAsync("message");
        }

        private void AddRoomTab(string roomId)
        {
            var tab = new TabItem
            {
                Header = roomId,
                Name = $"Tab_{roomId}",
                Background = (System.Windows.Media.Brush)FindResource("PanelBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush")
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var chatTextBox = new TextBox
            {
                Name = $"ChatBox_{roomId}",
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush"),
                Padding = new Thickness(10)
            };

            Grid.SetRow(chatTextBox, 0);
            grid.Children.Add(chatTextBox);

            var messagePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
            var messageInput = new TextBox
            {
                Name = $"MessageInput_{roomId}",
                Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush"),
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(5)
            };

            var sendButton = new Button
            {
                Content = "Send",
                Width = 70,
                Background = (System.Windows.Media.Brush)FindResource("AccentBrush"),
                Foreground = System.Windows.Media.Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = roomId
            };
            sendButton.Click += SendButton_Click;

            messagePanel.Children.Add(messageInput);
            messagePanel.Children.Add(sendButton);

            Grid.SetRow(messagePanel, 1);
            grid.Children.Add(messagePanel);

            tab.Content = grid;
            ChatTabControl.Items.Add(tab);
        }

        private void RemoveRoomTab(string roomId)
        {
            var tab = ChatTabControl.Items
                .OfType<TabItem>()
                .FirstOrDefault(t => t.Name == $"Tab_{roomId}");

            if (tab != null)
            {
                ChatTabControl.Items.Remove(tab);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string roomId = button?.Tag as string;

            if (string.IsNullOrEmpty(roomId))
                return;

            var tab = ChatTabControl.Items
                .OfType<TabItem>()
                .FirstOrDefault(t => t.Name == $"Tab_{roomId}");

            if (tab?.Content is Grid grid)
            {
                var messageInput = grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(sp => sp.Children.OfType<TextBox>())
                    .FirstOrDefault(tb => tb.Name == $"MessageInput_{roomId}");

                if (messageInput != null && !string.IsNullOrWhiteSpace(messageInput.Text))
                {
                    await _networkService.SendMessageAsync(messageInput.Text);
                    messageInput.Clear();
                }
            }
        }

        private void RefreshUsersList()
        {
            UsersListBox.Items.Clear();

            if (!string.IsNullOrEmpty(_currentRoomId) && _joinedRooms.TryGetValue(_currentRoomId, out var room))
            {
                foreach (string user in room.Users)
                {
                    UsersListBox.Items.Add(user);
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void NetworkService_ConnectionStatusChanged(object sender, string status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusIndicator.Text = status;
                StatusIndicator.Foreground = status == "Connected"
                    ? (System.Windows.Media.Brush)FindResource("SuccessBrush")
                    : (System.Windows.Media.Brush)FindResource("ErrorBrush");
            });
        }

        private void NetworkService_ErrorOccurred(object sender, Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void SaveConnectionDetails(string host, int port)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var preferences = _preferencesService.GetCurrentPreferences();
                    preferences.LastConnectionHost = host;
                    preferences.LastConnectionPort = port;
                    await _preferencesService.SavePreferencesAsync(preferences);
                }
                catch { }
            });
        }

        private string ExtractRoomId(string message)
        {
            var match = Regex.Match(message, @"@chatroom\s+(\w+)");
            if (match.Success)
                return match.Groups[1].Value;

            match = Regex.Match(message, @"chatroom\s+(\w+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_networkService.IsConnected())
            {
                _ = _networkService.SendCommandAsync("exit");
                _networkService.Disconnect();
            }

            _soundService.Dispose();
        }
    }
}
