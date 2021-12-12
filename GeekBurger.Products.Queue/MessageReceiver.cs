using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeekBurger.Products.Queue
{
    internal class MessageReceiver
    {
        private readonly string _queueConnectionString;
        private readonly string _queuePath;
        private IQueueClient _queueClient;
        private IList<Task> PendingCompleteTasks;
        private short count = 0;

        public MessageReceiver(string conectionString, string path)
        {
            _queueConnectionString = conectionString;
            _queuePath = path;
        }

        public async Task ReceiveMessagesAsync()
        {
            _queueClient = new QueueClient(_queueConnectionString, _queuePath, ReceiveMode.PeekLock);
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ExceptionHandler) { AutoComplete = false });
            Console.ReadLine();
            Console.WriteLine($" Request to close async. Pending tasks: {PendingCompleteTasks.Count}");
            
            await Task.WhenAll(PendingCompleteTasks);
            Console.WriteLine($"All pending tasks were completed");
            var closeTask = _queueClient.CloseAsync();
            
            await closeTask;
            CheckCommunicationExceptions(closeTask);
        }


        private Task ExceptionHandler(ExceptionReceivedEventArgs exceptionArgs)
        {
            Console.WriteLine($"Message handler encountered an exception: {exceptionArgs.Exception}.");
            var context = exceptionArgs.ExceptionReceivedContext;

            Console.WriteLine($"Endpoint: {context.Endpoint}, Path: {context.EntityPath}, Action: {context.Action}");
            return Task.CompletedTask;
        }

        private async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            PendingCompleteTasks = new List<Task>();
            
            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(message.Body)}");

            if (cancellationToken.IsCancellationRequested || _queueClient.IsClosedOrClosing)
                return;

            Console.WriteLine($"task {count++}");
            Task PendingTask;
            lock (PendingCompleteTasks)
            {
                PendingCompleteTasks.Add(_queueClient.CompleteAsync(
                     message.SystemProperties.LockToken));
                PendingTask = PendingCompleteTasks.LastOrDefault();
            }

            Console.WriteLine($"calling complete for task {count}");
            await PendingTask;
            Console.WriteLine($"remove task {count} from task queue");
            PendingCompleteTasks.Remove(PendingTask);
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
