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
        private readonly string registerUserQueue;

        private readonly string orderCreatedTopic;
        private readonly string orderCreatedEmailSubscription;

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private ServiceBusProcessor _emailShoppingCartProcessor;
        private ServiceBusProcessor _registerUserProcessor;
        private ServiceBusProcessor _orderCreatedProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {            
            _configuration = configuration;
            _emailService = emailService;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emailShoppingCartQueue = _configuration.GetValue<string>("QueueAndTopicNames:EmailShoppingCartQueue");
            registerUserQueue = _configuration.GetValue<string>("QueueAndTopicNames:RegisterUserQueue");

            orderCreatedTopic = _configuration.GetValue<string>("QueueAndTopicNames:OrderCreatedTopic");
            orderCreatedEmailSubscription = _configuration.GetValue<string>("QueueAndTopicNames:OrderCreatedEmailSubscription");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailShoppingCartProcessor = client.CreateProcessor(emailShoppingCartQueue);
            _registerUserProcessor = client.CreateProcessor(registerUserQueue);
            _orderCreatedProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedEmailSubscription);
        }

        public async Task Start()
        {
            _emailShoppingCartProcessor.ProcessMessageAsync += OnEmailShoppingCartRequestReceived;
            _emailShoppingCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailShoppingCartProcessor.StartProcessingAsync();

            _registerUserProcessor.ProcessMessageAsync += OnRegisterUserRequestReceived;
            _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _registerUserProcessor.StartProcessingAsync();

            _orderCreatedProcessor.ProcessMessageAsync += OnCreatedOrderRequestReceived;
            _orderCreatedProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderCreatedProcessor.StartProcessingAsync();
        }



        public async Task Stop()
        {
            await _emailShoppingCartProcessor.StopProcessingAsync();
            await _emailShoppingCartProcessor.DisposeAsync();

            await _registerUserProcessor.StopProcessingAsync();
            await _registerUserProcessor.DisposeAsync();

            await _orderCreatedProcessor.StopProcessingAsync();
            await _orderCreatedProcessor.DisposeAsync();
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

        private async Task OnRegisterUserRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            string email = JsonConvert.DeserializeObject<string>(body);

            try
            {
                await _emailService.EmailAndLogRegisterUserAsync(email);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task OnCreatedOrderRequestReceived(ProcessMessageEventArgs args)
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
