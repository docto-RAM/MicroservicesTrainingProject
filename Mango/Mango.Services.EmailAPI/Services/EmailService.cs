using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private const string _innerEmail = "testinner@email.com";

        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task EmailAndLogCartAsync(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested");
            message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
            message.AppendLine("<br/>");
            message.AppendLine("<ul>");
            foreach (var item in cartDto.CartDetails)
            {
                message.AppendLine("<li>");
                message.AppendLine(item.Product.Name + " x " + item.Count);
                message.AppendLine("</li>");
            }
            message.AppendLine("</ul>");

            await EmailAndLog(message.ToString(), cartDto.CartHeader.Email);
        }

        public async Task EmailAndLogRegisterUserAsync(string email)
        {
            string message = "User Registration Successful.<br/>Email: " + email;

            await EmailAndLog(message, _innerEmail);
        }

        public async Task EmailAndLogPlacedOrderAsync(RewardDto rewardDto)
        {
            string message = "New Order Placed.<br/>Order Id : " + rewardDto.OrderId;

            await EmailAndLog(message, _innerEmail);
        }

        private async Task EmailAndLog(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new()
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message
                };

                await using var _db = new AppDbContext(_dbOptions);
                await _db.EmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();
            }
            catch
            {
            }
        }
    }
}
