using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Atlas.Common.ServiceBus
{
    public interface IMessageBatchPublisher<in T>
    {
        Task BatchPublish(IEnumerable<T> contentToPublish);
    }

    public class MessageBatchPublisher<T> : IMessageBatchPublisher<T>
    {
        private readonly string connectionString;
        private readonly string topicName;

        public MessageBatchPublisher(string connectionString, string topicName)
        {
            this.connectionString = connectionString;
            this.topicName = topicName;
        }

        public async Task BatchPublish(IEnumerable<T> contentToPublish)
        {
            await using var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(topicName);

            var localQueue = new Queue<ServiceBusMessage>();
            foreach (var content in contentToPublish)
            {
                var message = new ServiceBusMessage(JsonConvert.SerializeObject(content));
                localQueue.Enqueue(message);
            }

            while (localQueue.Count > 0)
            {
                using var messageBatch = await sender.CreateMessageBatchAsync();
                while (localQueue.Count > 0 && messageBatch.TryAddMessage(localQueue.Peek()))
                {
                    localQueue.Dequeue();
                }

                await sender.SendMessagesAsync(messageBatch);
            }
        }
    }
}