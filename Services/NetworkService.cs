using System;
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
        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<Exception> ErrorOccurred;

        public NetworkService()
        {
            _receiveBuffer = new StringBuilder();
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _networkStream = _tcpClient.GetStream();

                _cancellationTokenSource = new CancellationTokenSource();

                OnConnectionStatusChanged("Connected");
                _ = ReceiveMessagesAsync(_cancellationTokenSource.Token);

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
                return false;
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
                    throw new InvalidOperationException("Not connected to server");
                }

                string formattedMessage = isCommand ? $"`{message}" : message;
                string nullTerminatedMessage = formattedMessage + '\0';

                byte[] data = Encoding.UTF8.GetBytes(nullTerminatedMessage);
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
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
            string buffer = _receiveBuffer.ToString();
            int delimiterIndex;

            while ((delimiterIndex = buffer.IndexOf('\0')) >= 0)
            {
                string message = buffer.Substring(0, delimiterIndex);
                buffer = buffer.Substring(delimiterIndex + 1);

                if (!string.IsNullOrEmpty(message))
                {
                    OnMessageReceived(message);
                }
            }

            _receiveBuffer.Clear();
            _receiveBuffer.Append(buffer);
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
