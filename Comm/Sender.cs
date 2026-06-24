using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communicator
{
    public class Sender
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;

        public Sender(string ipAddress, int portNumber)
        {
            _ip = ipAddress;
            _port = portNumber;
            EnsureConnected();
        }

        private void EnsureConnected()
        {
            if (_client != null && _client.Connected) return;
            try
            {
                _client?.Close();
            }
            catch { /* ignore */ }
            _client = new TcpClient(_ip, _port);
            _stream = _client.GetStream();
        }

        public void Send(string jsonPayload)
        {
            if (jsonPayload == null) throw new ArgumentNullException(nameof(jsonPayload));
            try
            {
                EnsureConnected();
                byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
                byte[] lenBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
                _stream.Write(lenBytes, 0, 4);
                _stream.Write(body, 0, body.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Sender] " + ex.Message);
                try { _client?.Close(); } catch { }
                _client = null;
                _stream = null;
            }
        }
    }
}
