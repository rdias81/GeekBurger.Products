using System;

namespace GeekBurger.Products.Queue
{
    internal class Program
    {
        const string QueueConnectionString = "Endpoint";
        const string QueuePath = "ProductChanged";

        static void Main(string[] args)
        {
            if (args.Length <= 0 || args[0] == "sender")
            {
                var queueSender = new MessageSender(QueueConnectionString, QueuePath);
                queueSender.SendMessagesAsync().GetAwaiter().GetResult();
                Console.WriteLine("messages were sent");
            }
            else if (args[0] == "receiver")
            {
                var queueReceiver = new MessageReceiver(QueueConnectionString, QueuePath);
                queueReceiver.ReceiveMessagesAsync().GetAwaiter().GetResult();
                Console.WriteLine("messages were received");
            }
            else
                Console.WriteLine("nothing to do");

            Console.ReadLine();
        }

    }
}
