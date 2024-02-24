using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailAndLogCartAsync(CartDto cartDto);
        Task EmailAndLogRegisterUserAsync(string email);
    }
}
