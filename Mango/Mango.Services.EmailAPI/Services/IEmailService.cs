using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailAndLogCartAsync(CartDto cartDto);
        Task EmailAndLogRegisteredUserAsync(string email);
        Task EmailAndLogCreatedOrderAsync(RewardDto rewardDto);
    }
}
