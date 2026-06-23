using System;
using Communicator;


namespace ConsoleConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Aplicatia consumator a pornit si asteapta date ...");

            while(true)
            {
                var data = HttpHelper.GetDataFromWebAPI();
                Console.Clear();
                Console.WriteLine($"Date primite: { data.Count }");
                System.Threading.Thread.Sleep(2500);
            }
        }
    }
}
