using Microsoft.Azure.ServiceBus;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurger.Products.Queue
{
    internal class MessageSender
    {
        private readonly string _queueConnectionString;
        private readonly string _queuePath;
        private IQueueClient _queueClient;

        public MessageSender(string conectionString, string path)
        {
            _queueConnectionString = conectionString;
            _queuePath = path;
        }

        public async Task SendMessagesAsync()
        {
            _queueClient = new QueueClient(_queueConnectionString, _queuePath);
            _queueClient.OperationTimeout = TimeSpan.FromSeconds(10);
            var messages = " Hi,Hello,Hey,How are you,Be Welcome"
                .Split(',')
                .Select(msg =>
                {
                    Console.WriteLine($"Will send message: {msg}");
                    return new Message(Encoding.UTF8.GetBytes(msg));
                })
                .ToList();
            var sendTask = _queueClient.SendAsync(messages);
            await sendTask;
            CheckCommunicationExceptions(sendTask);
            var closeTask = _queueClient.CloseAsync();
            await closeTask;
            CheckCommunicationExceptions(closeTask);
        }

        private bool CheckCommunicationExceptions(Task task)
        {
            if (task.Exception == null || task.Exception.InnerExceptions.Count == 0) return true;

            task.Exception.InnerExceptions.ToList()
                .ForEach(innerException =>
                {
                    Console.WriteLine($"Error in SendAsync task: {innerException.Message}. Details: {innerException.StackTrace}");
                    if (innerException is ServiceBusCommunicationException)
                        Console.WriteLine("Connection Problem with Host");
                });

            return false;
        }
    }
}
