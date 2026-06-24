using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communicator
{
    public class Receiver
    {
        private readonly TcpListener _listener;
        public event EventHandler<string> JsonReceived;

        public Receiver(string ipAddress, int portNumber)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAddress), portNumber);
        }

        public void StartListen()
        {
            _listener.Start();
            while (true)
            {
                Console.WriteLine("[Receiver] Astept conexiune...");
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                    Console.WriteLine("[Receiver] Conectat.");
                    using (var stream = client.GetStream())
                    {
                        while (TryReadFrame(stream, out string payload))
                        {
                            var handler = JsonReceived;
                            handler?.Invoke(this, payload);
                        }
                    }
                    Console.WriteLine("[Receiver] Client deconectat.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Receiver] " + ex.Message);
                }
                finally
                {
                    try { client?.Close(); } catch { }
                }
            }
        }

        private static bool TryReadFrame(NetworkStream stream, out string payload)
        {
            payload = null;
            byte[] lenBuf = new byte[4];
            if (!ReadExactly(stream, lenBuf, 4)) return false;
            int len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
            if (len <= 0 || len > 1024 * 1024) return false;
            byte[] body = new byte[len];
            if (!ReadExactly(stream, body, len)) return false;
            payload = Encoding.UTF8.GetString(body);
            return true;
        }

        private static bool ReadExactly(NetworkStream stream, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset, count - offset);
                if (read <= 0) return false;
                offset += read;
            }
            return true;
        }
    }
}
