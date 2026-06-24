using System;
using System.Threading;
using Communicator;

namespace ConsoleConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Console Consumer — citește WebAPI";
            Console.WriteLine("ConsoleConsumer pornit. Polling " + HttpHelper.BaseUrl + "/api/simulator la 2s");

            while (true)
            {
                try
                {
                    var data = HttpHelper.GetDataFromWebAPI();
                    Console.Clear();
                    Console.WriteLine("Evenimente stocate: " + data.Count);
                    if (data.Count > 0)
                    {
                        var last = data[data.Count - 1];
                        Console.WriteLine("Ultimul:");
                        Console.WriteLine("  Time:     " + last.StateChangedDate.ToString("HH:mm:ss.fff"));
                        Console.WriteLine("  Pressure: " + last.Pressure.ToString("F2") + " bar (set " + last.Setpoint.ToString("F1") + ")");
                        Console.WriteLine("  Pumps:    " + Bits(last.Pumps));
                        Console.WriteLine("  Lamps:    " + Bits(last.Lamps));
                        Console.WriteLine("  Mode:     " + last.Mode);
                        Console.WriteLine("  Alarm:    " + (last.Alarm ? "DA" : "nu"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Consumer] " + ex.Message);
                }
                Thread.Sleep(2000);
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
