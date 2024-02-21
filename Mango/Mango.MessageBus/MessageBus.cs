namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        public Task PublishMessage(object message, string topicOrQueueName)
        {
            throw new NotImplementedException();
        }
    }
}
