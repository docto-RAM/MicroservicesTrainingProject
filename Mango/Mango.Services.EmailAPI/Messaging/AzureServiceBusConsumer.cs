using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;

        private readonly string emailShoppingCartQueue;
        private readonly string registeredUserQueue;

        private readonly string createdOrderTopic;
        private readonly string createdOrderEmailSubscription;

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private ServiceBusProcessor _emailShoppingCartProcessor;
        private ServiceBusProcessor _registeredUserProcessor;
        private ServiceBusProcessor _createdOrderEmailProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {            
            _configuration = configuration;
            _emailService = emailService;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emailShoppingCartQueue = _configuration.GetValue<string>("QueueAndTopicNames:EmailShoppingCartQueue");
            registeredUserQueue = _configuration.GetValue<string>("QueueAndTopicNames:RegisteredUserQueue");

            createdOrderTopic = _configuration.GetValue<string>("QueueAndTopicNames:CreatedOrderTopic");
            createdOrderEmailSubscription = _configuration.GetValue<string>("QueueAndTopicNames:CreatedOrderEmailSubscription");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailShoppingCartProcessor = client.CreateProcessor(emailShoppingCartQueue);
            _registeredUserProcessor = client.CreateProcessor(registeredUserQueue);
            _createdOrderEmailProcessor = client.CreateProcessor(createdOrderTopic, createdOrderEmailSubscription);
        }

        public async Task Start()
        {
            _emailShoppingCartProcessor.ProcessMessageAsync += OnEmailShoppingCartRequestReceived;
            _emailShoppingCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailShoppingCartProcessor.StartProcessingAsync();

            _registeredUserProcessor.ProcessMessageAsync += OnRegisteredUserRequestReceived;
            _registeredUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _registeredUserProcessor.StartProcessingAsync();

            _createdOrderEmailProcessor.ProcessMessageAsync += OnCreatedOrderEmailRequestReceived;
            _createdOrderEmailProcessor.ProcessErrorAsync += ErrorHandler;
            await _createdOrderEmailProcessor.StartProcessingAsync();
        }



        public async Task Stop()
        {
            await _emailShoppingCartProcessor.StopProcessingAsync();
            await _emailShoppingCartProcessor.DisposeAsync();

            await _registeredUserProcessor.StopProcessingAsync();
            await _registeredUserProcessor.DisposeAsync();

            await _createdOrderEmailProcessor.StopProcessingAsync();
            await _createdOrderEmailProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());

            return Task.CompletedTask;
        }

        private async Task OnEmailShoppingCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);

            try
            {
                await _emailService.EmailAndLogCartAsync(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task OnRegisteredUserRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            string email = JsonConvert.DeserializeObject<string>(body);

            try
            {
                await _emailService.EmailAndLogRegisteredUserAsync(email);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task OnCreatedOrderEmailRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            RewardDto objMessage = JsonConvert.DeserializeObject<RewardDto>(body);

            try
            {
                await _emailService.EmailAndLogCreatedOrderAsync(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
