using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> GetOrders(string? userId = "");
        Task<ResponseDto?> GetOrder(int orderId);
        Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
        Task<ResponseDto?> UpdateOrderStatus(int orderId, string newStatus);
        Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto);
        Task<ResponseDto?> ValidateStripeSession(int orderHeaderId);
    }
}
