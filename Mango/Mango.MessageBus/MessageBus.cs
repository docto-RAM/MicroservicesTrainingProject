using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private string connectionString = "ServiceBusConnectionString";

        public async Task PublishMessage(object message, string queueOrTopicName)
        {
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueOrTopicName);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            { 
                CorrelationId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(serviceBusMessage);

            await client.DisposeAsync();
        }
    }
}
