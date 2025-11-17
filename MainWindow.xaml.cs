using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private Dictionary<string, string> _roomUsernames;
        private string _currentRoomId;
        private Process _serverProcess;
        private User _user;

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;

            _joinedRooms = new Dictionary<string, ChatRoom>();
            _roomUsernames = new Dictionary<string, string>();
            _messageRouter = new MessageRouterService();
            _soundService = new SoundService();
            _networkService = new NetworkService();
            _preferencesService = new PreferencesService();

            StartServer();
            InitializeServices();
            SetPlaceholderText();
            AutoConnect();
        }

        private async void AutoConnect()
        {
            SubscribeToNetworkEvents();

            var (connected, errorMessage) = await Task.Run(() => _networkService.ConnectAsync("localhost", 9999));

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (connected)
                {
                    StatusIndicator.Text = "Connected";
                    StatusIndicator.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
                    _ = _networkService.SendCommandAsync("list");
                }
                else
                {
                    MessageBox.Show($"Failed to connect to server: {errorMessage}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void StartServer()
        {
            string pythonScriptPath = "is5.py";
            if (!System.IO.File.Exists(pythonScriptPath))
            {
                MessageBox.Show("Server script 'is5.py' not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = pythonScriptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };

            _serverProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Console.WriteLine($"Server ERROR: {args.Data}");
                    System.IO.File.AppendAllText("server_log.txt", $"ERROR: {args.Data}\n");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Server Error: {args.Data}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            };

            _serverProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Console.WriteLine($"Server OUT: {args.Data}");
                    System.IO.File.AppendAllText("server_log.txt", $"OUT: {args.Data}\n");
                }
            };

            try
            {
                _serverProcess.Start();
                _serverProcess.BeginErrorReadLine();
                _serverProcess.BeginOutputReadLine();
                Console.WriteLine("Server process started.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void InitializeServices()
        {
            try
            {
                await _preferencesService.InitializeAsync(
                    "https://drttmgajjlyhpwurbgsm.supabase.co",
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRydHRtZ2Fqamx5aHB3dXJiZ3NtIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc2MzM0NTcyNSwiZXhwIjoyMDc4OTIxNzI1fQ.tNQKdokibFLz_7ri0CjyiS3Ej5lB4hGRtlQVu2BIjF0");

                var preferences = await _preferencesService.LoadPreferencesAsync(_user.Id);

                ThemeManager.ApplyTheme(preferences.ThemeMode);
                ThemeManager.ApplyAccentColor(preferences.AccentColor);

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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void DisconnectFromServer()
        {
            _networkService.Disconnect();
            UnsubscribeFromNetworkEvents();

            _joinedRooms.Clear();
            ChatTabControl.Items.Clear();
            UsersListBox.Items.Clear();
            RoomListBox.Items.Clear();
        }

        private async void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string roomId = CreateRoomIdTextBox.Text.Trim();
            string roomName = CreateRoomNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(roomName) || roomId == "Enter Room ID" || roomName == "Enter Room Name")
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
            await _networkService.SendCommandAsync("list");

            CreateRoomIdTextBox.Clear();
            CreateRoomNameTextBox.Clear();
            SetPlaceholderText();
            CreateRoomStatusTextBlock.Text = "Room created successfully!";
            await Task.Delay(3000);
            CreateRoomStatusTextBlock.Text = "";
        }

        private async void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more rooms to join.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (string selectedRoom in RoomListBox.SelectedItems)
            {
                var match = Regex.Match(selectedRoom, @"@(\w+):");
                if (match.Success)
                {
                    string roomId = match.Groups[1].Value;

                    if (!_networkService.IsConnected())
                    {
                        MessageBox.Show("Not connected to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!_joinedRooms.ContainsKey(roomId))
                    {
                        _roomUsernames[roomId] = _user.Username;
                        await _networkService.SendCommandAsync($"join {roomId} {_user.Username}");
                        HandleRoomJoined(roomId);
                    }
                }
            }

            JoinRoomStatusTextBlock.Text = "Joined selected rooms!";
            await Task.Delay(3000);
            JoinRoomStatusTextBlock.Text = "";
        }

        private async void RefreshRoomsButton_Click(object sender, RoutedEventArgs e)
        {
            await _networkService.SendCommandAsync("list");
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
            var match = Regex.Match(message, @"^@(\w+):(.*)");
            if (match.Success)
            {
                string roomId = match.Groups[1].Value;
                string content = match.Groups[2].Value.Trim();
                HandleBroadcastMessage(content, roomId);
            }
            else if (message.StartsWith("No room exists") || Regex.IsMatch(message, @"\w+@\w+:"))
            {
                HandleListRoomsResponse(message);
            }
            else if (message.Contains("You were kicked out from chatroom"))
            {
                HandleKickedOut(message);
            }
            else if (message.Contains("You quitted from chatroom"))
            {
                // This is a confirmation message from the server.
                // The client already handled the cleanup when the tab was closed,
                // so we can safely ignore this message to prevent the error.
                return;
            }
            else
            {
                // If we receive a generic message, and we have an active room, display it there.
                if (!string.IsNullOrEmpty(_currentRoomId))
                {
                    HandleBroadcastMessage(message, _currentRoomId);
                }
            }
        }

        private void HandleBroadcastMessage(string content, string roomId)
        {
            if (content.Contains("You join @chatroom"))
            {
                HandleRoomJoined(roomId);
            }
            else if (content.Contains("joined us @chatroom"))
            {
                HandleUserJoined(content, roomId);
            }
            else if (content.Contains("quitted from chatroom"))
            {
                HandleRoomLeft(content, roomId);
            }
            else if (content.Contains("Server:"))
            {
                HandleServerMessage(content, roomId);
            }
            else
            {
                var userMessageMatch = Regex.Match(content, @"^([^:]+): (.*)");
                if (userMessageMatch.Success)
                {
                    string username = userMessageMatch.Groups[1].Value;
                    string message = userMessageMatch.Groups[2].Value.Trim();
                    HandleChatMessage($"{username}: {message}", roomId);
                }
                else
                {
                    HandleChatMessage(content, roomId);
                }
            }
        }

        private void HandleRoomJoined(string roomId)
        {
            if (!_joinedRooms.ContainsKey(roomId))
            {
                var room = new ChatRoom { RoomId = roomId, RoomName = roomId };
                _joinedRooms.Add(roomId, room);
                _messageRouter.RegisterRoom(roomId, room);

                AddRoomTab(roomId);
                _currentRoomId = roomId;
            }

            if (_currentRoomId == roomId && _joinedRooms.TryGetValue(roomId, out var chatRoom))
            {
                chatRoom.AddMessage($"[System] You joined {roomId}.");
            }

            _ = _networkService.SendCommandAsync("list");
            _ = _soundService.PlayNotificationAsync("join_leave");
        }

        private void HandleUserJoined(string message, string roomId)
        {
            var match = Regex.Match(message, @"^(\w+):");
            if (match.Success && _joinedRooms.TryGetValue(roomId, out var room))
            {
                string username = match.Groups[1].Value;
                if (!room.Users.Contains(username))
                {
                    room.Users.Add(username);
                }
                room.AddMessage($"[System] {message}");
                RefreshUsersList();
            }

            _ = _soundService.PlayNotificationAsync("join_leave");
        }

        private void HandleRoomLeft(string message, string roomId)
        {
            var match = Regex.Match(message, @"Server: (\w+) quitted.");
            if (match.Success && _joinedRooms.TryGetValue(roomId, out var room))
            {
                string username = match.Groups[1].Value;
                room.Users.Remove(username);
                room.AddMessage($"[System] {message}");
                RefreshUsersList();
            }
            else
            {
                if (_joinedRooms.TryGetValue(roomId, out var roomToLeave))
                {
                    RemoveRoomTab(roomId);
                    _joinedRooms.Remove(roomId);
                    _messageRouter.UnregisterRoom(roomId);

                    if (_currentRoomId == roomId)
                    {
                        _currentRoomId = _joinedRooms.Keys.FirstOrDefault();
                    }
                }
            }
        }

        private void HandleKickedOut(string message)
        {
            var match = Regex.Match(message, @"@(\w+) by the Administrator!");
            if (match.Success)
            {
                string roomId = match.Groups[1].Value;
                if (_joinedRooms.ContainsKey(roomId))
                {
                    RemoveRoomTab(roomId);
                    _joinedRooms.Remove(roomId);
                    _messageRouter.UnregisterRoom(roomId);

                    if (_currentRoomId == roomId)
                    {
                        _currentRoomId = _joinedRooms.Keys.FirstOrDefault();
                    }
                    
                    MessageBox.Show($"You have been kicked from room {roomId}.", "Kicked", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    var match = Regex.Match(line, @"^(.+)@(\w+): (.*)");
                    if (match.Success)
                    {
                        string roomName = match.Groups[1].Value;
                        string roomId = match.Groups[2].Value;
                        string users = match.Groups[3].Value;

                        if (_joinedRooms.TryGetValue(roomId, out var room))
                        {
                            room.Users.Clear();
                            var userMatches = Regex.Matches(users, @"(\w+)@\('[\d\.]+', \d+\)");
                            foreach (Match userMatch in userMatches)
                            {
                                var username = userMatch.Groups[1].Value;
                                if (!room.Users.Contains(username))
                                {
                                    room.Users.Add(username);
                                }
                            }
                        }
                    }
                }
            }
            RefreshUsersList();
        }

        private void HandleServerMessage(string message, string roomId)
        {
            if (_joinedRooms.TryGetValue(roomId, out var room))
            {
                room.AddMessage($"[Server] {message}");
            }

            _ = _soundService.PlayNotificationAsync("message");
        }

        private void HandleChatMessage(string message, string roomId)
        {
            if (_joinedRooms.TryGetValue(roomId, out var room))
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
                Name = $"Tab_{roomId}"
            };

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock { Text = roomId });
            var closeButton = new Button
            {
                Content = "x",
                Tag = roomId,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(3),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            closeButton.Click += CloseButton_Click;
            headerPanel.Children.Add(closeButton);

            tab.Header = headerPanel;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var chatListBox = new ListBox
            {
                Name = $"ChatBox_{roomId}",
                ItemsSource = _joinedRooms[roomId].Messages,
                Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush")
            };

            Grid.SetRow(chatListBox, 0);
            grid.Children.Add(chatListBox);

            var messageGrid = new Grid { Margin = new Thickness(10) };
            messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var messageInput = new TextBox
            {
                Name = $"MessageInput_{roomId}",
                Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush"),
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(5)
            };
            messageInput.KeyDown += MessageInput_KeyDown;
            Grid.SetColumn(messageInput, 0);
            messageGrid.Children.Add(messageInput);

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
            Grid.SetColumn(sendButton, 1);
            messageGrid.Children.Add(sendButton);

            Grid.SetRow(messageGrid, 1);
            grid.Children.Add(messageGrid);

            tab.Content = grid;
            ChatTabControl.Items.Add(tab);
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string roomId)
            {
                if (_joinedRooms.ContainsKey(roomId))
                {
                    await _networkService.SendCommandAsync($"quit {roomId}");
                    RemoveRoomTab(roomId);
                    _joinedRooms.Remove(roomId);
                    _messageRouter.UnregisterRoom(roomId);

                    if (_currentRoomId == roomId)
                    {
                        _currentRoomId = _joinedRooms.Keys.FirstOrDefault();
                    }
                }
            }
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
                    .OfType<Grid>()
                    .SelectMany(g => g.Children.OfType<TextBox>())
                    .FirstOrDefault(tb => tb.Name == $"MessageInput_{roomId}");

                if (messageInput != null && !string.IsNullOrWhiteSpace(messageInput.Text))
                {
                    string message = messageInput.Text;

                    if (_joinedRooms.TryGetValue(roomId, out var room))
                    {
                        room.AddMessage($"{_user.Username}: {message}");
                    }

                    await _networkService.SendMessageAsync(message);
                    messageInput.Clear();
                }
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                var messageGrid = textBox?.Parent as Grid;
                var sendButton = messageGrid?.Children.OfType<Button>().FirstOrDefault();

                if (sendButton != null)
                {
                    sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        private void ChatTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatTabControl.SelectedItem is TabItem selectedTab)
            {
                _currentRoomId = selectedTab.Name.Substring(4); // Remove "Tab_"
                RefreshUsersList();
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

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            var adminLoginWindow = new AdminLoginWindow();
            adminLoginWindow.Owner = this;
            adminLoginWindow.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_user, _preferencesService);
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
                    var preferences = _preferencesService.GetCurrentPreferences(_user.Id);
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is string placeholder)
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = (System.Windows.Media.Brush)FindResource("ForegroundBrush");
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is string placeholder)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.Foreground = (System.Windows.Media.Brush)FindResource("PlaceholderTextBrush");
                }
            }
        }

        private void SetPlaceholderText()
        {
            SetPlaceholder(CreateRoomIdTextBox, "Enter Room ID");
            SetPlaceholder(CreateRoomNameTextBox, "Enter Room Name");
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Tag = placeholder;
            textBox.Text = placeholder;
            textBox.Foreground = (System.Windows.Media.Brush)FindResource("PlaceholderTextBrush");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_networkService.IsConnected())
            {
                _ = _networkService.SendCommandAsync("exit");
                _networkService.Disconnect();
            }

            _soundService.Dispose();

            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
            }
        }
    }
}
