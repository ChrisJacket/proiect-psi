using System;
using Communicator;
using DataModel;
using Newtonsoft.Json;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Monitor — TCP relay (3000) → WebAPI (5000)";
            Console.WriteLine("======================================================");
            Console.WriteLine(" Monitor pornit. Ascult pe TCP 127.0.0.1:3000        ");
            Console.WriteLine(" Forward HTTP la " + HttpHelper.BaseUrl + "/api/simulator");
            Console.WriteLine("======================================================");

            var receiver = new Receiver("127.0.0.1", 3000);
            receiver.JsonReceived += OnJsonReceived;
            receiver.StartListen();
        }

        private static void OnJsonReceived(object sender, string payload)
        {
            try
            {
                var ev = JsonConvert.DeserializeObject<PumpSystemStatusEvent>(payload);
                if (ev == null) return;
                Console.WriteLine(
                    "[" + ev.StateChangedDate.ToString("HH:mm:ss.fff") + "] " +
                    "P=" + ev.Pressure.ToString("F2") + "/" + ev.Setpoint.ToString("F1") + " bar | " +
                    "M=[" + Bits(ev.Pumps) + "] L=[" + Bits(ev.Lamps) + "] | " +
                    "Mod=" + ev.Mode + (ev.Alarm ? " | ALARMĂ" : ""));

                HttpHelper.PostJsonToWebAPI(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Monitor] eroare: " + ex.Message);
            }
        }

        private static string Bits(bool[] arr)
        {
            if (arr == null) return "----";
            char[] c = new char[arr.Length];
            for (int i = 0; i < arr.Length; i++) c[i] = arr[i] ? '1' : '0';
            return new string(c);
        }
    }
}
