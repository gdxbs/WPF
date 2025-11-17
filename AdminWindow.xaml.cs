using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ChatClient.Services;
using System.Threading.Tasks;

namespace ChatClient
{
    public partial class AdminWindow : Window
    {
        private NetworkService _networkService;

        public AdminWindow()
        {
            InitializeComponent();
            _networkService = new NetworkService();
            ConnectAdmin();
        }

        private async void ConnectAdmin()
        {
            var (connected, errorMessage) = await _networkService.ConnectAsync("localhost", 9999);
            if (connected)
            {
                _networkService.MessageReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ServerOutputTextBox.AppendText(args + Environment.NewLine);
                            ServerOutputTextBox.ScrollToEnd();
                        });
                    }
                };

                _networkService.MessageSent += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args))
                    {
                        if (args.StartsWith("`help"))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ServerOutputTextBox.AppendText(
                                    "\n`help`: Show usage of all commands.\n" +
                                    "`list`: List all chat rooms and the chatters in each.\n" +
                                    "`end <chatroom_id>`: Withdraw all chatters from the specified chat room and then end the room.\n" +
                                    "`start <chatroom_id> <chatroom_name>`: Start a new chat room with the given ID and name.\n" +
                                    "`kick <chatter_name> <room_id>`: Kick the specified chatter out of the given chat room.\n" +
                                    "`quit`:  Clean all chat rooms and end all chatter connections. Quit the server.\n" +
                                    Environment.NewLine);
                                ServerOutputTextBox.ScrollToEnd();
                            });
                        }
                        else if (args.StartsWith("`list"))
                        {
                            // The server's response to `list` will be handled by the MessageReceived event
                        }
                    }
                };
            }
            else
            {
                MessageBox.Show($"Failed to connect to admin service: {errorMessage}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommandButton_Click(sender, e);
            }
        }

        private async void SendCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (_networkService != null && _networkService.IsConnected())
            {
                await _networkService.SendCommandAsync(CommandTextBox.Text);
                CommandTextBox.Clear();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_networkService != null && _networkService.IsConnected())
            {
                _networkService.SendCommandAsync("`quit").Wait();
                _networkService.Disconnect();
            }
        }
    }
}

