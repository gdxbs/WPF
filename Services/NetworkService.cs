using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient.Services
{
    public class NetworkService
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private StringBuilder _receiveBuffer;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> MessageSent;
        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<Exception> ErrorOccurred;

        public NetworkService()
        {
            _receiveBuffer = new StringBuilder();
        }

        public async Task<(bool, string)> ConnectAsync(string host, int port)
        {
            Console.WriteLine($"Attempting to connect to {host}:{port}...");
            try
            {
                _tcpClient = new TcpClient();
                var connectTask = _tcpClient.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(5000); // 5-second timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("Connection timed out.");
                    throw new TimeoutException("Connection timed out.");
                }

                await connectTask; // Propagate any exceptions from the connection task

                _networkStream = _tcpClient.GetStream();
                _cancellationTokenSource = new CancellationTokenSource();

                _ = ReceiveMessagesAsync(_cancellationTokenSource.Token);
                OnConnectionStatusChanged("Connected");
                Console.WriteLine("Connection successful.");
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                OnErrorOccurred(ex);
                OnConnectionStatusChanged("Disconnected");
                return (false, ex.Message);
            }
        }

        public bool IsConnected()
        {
            return _tcpClient?.Connected ?? false;
        }

        public async Task<bool> SendCommandAsync(string command)
        {
            return await SendMessageAsync(command, isCommand: true);
        }

        public async Task<bool> SendMessageAsync(string message, bool isCommand = false)
        {
            try
            {
                if (!IsConnected())
                {
                    Console.WriteLine("SendMessageAsync failed: Not connected to server.");
                    throw new InvalidOperationException("Not connected to server");
                }

                string formattedMessage = isCommand ? $"`{message}" : message;
                string nullTerminatedMessage = formattedMessage + '\0';

                Console.WriteLine($"Sending message: {nullTerminatedMessage}");
                byte[] data = Encoding.UTF8.GetBytes(nullTerminatedMessage);
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();

                OnMessageSent(formattedMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessageAsync failed: {ex.Message}");
                OnErrorOccurred(ex);
                return false;
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[4096];

                while (!cancellationToken.IsCancellationRequested && IsConnected())
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead == 0)
                    {
                        OnConnectionStatusChanged("Disconnected");
                        break;
                    }

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received data: {receivedData}");
                    _receiveBuffer.Append(receivedData);

                    ProcessReceivedMessages();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
                OnConnectionStatusChanged("Disconnected");
            }
        }

        private void ProcessReceivedMessages()
        {
            string bufferContent = _receiveBuffer.ToString();
            int nullIndex;

            while ((nullIndex = bufferContent.IndexOf('\0')) != -1)
            {
                string message = bufferContent.Substring(0, nullIndex);
                bufferContent = bufferContent.Substring(nullIndex + 1);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    OnMessageReceived(message);
                }
            }

            _receiveBuffer = new StringBuilder(bufferContent);
        }

        public void Disconnect()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _networkStream?.Close();
                _tcpClient?.Close();
                OnConnectionStatusChanged("Disconnected");
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnMessageSent(string message)
        {
            MessageSent?.Invoke(this, message);
        }

        protected virtual void OnConnectionStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }

        protected virtual void OnErrorOccurred(Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}
