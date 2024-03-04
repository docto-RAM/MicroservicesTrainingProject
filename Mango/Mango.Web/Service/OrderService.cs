using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = cartDto,
                Url = WebSD.APIBase.OrderAPI + "/api/order/CreateOrder"
            });
        }

        public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = stripeRequestDto,
                Url = WebSD.APIBase.OrderAPI + "/api/order/CreateStripeSession"
            });
        }

        public async Task<ResponseDto?> GetOrder(int orderId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.GET,
                Url = WebSD.APIBase.OrderAPI + "/api/order/GetOrder/" + orderId
            });
        }

        public async Task<ResponseDto?> GetOrders(string? userId = "")
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.GET,
                Url = WebSD.APIBase.OrderAPI + "/api/order/GetOrders/" + userId
            });
        }

        public async Task<ResponseDto?> UpdateOrderStatus(int orderId, string newStatus)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = newStatus,
                Url = WebSD.APIBase.OrderAPI + "/api/order/UpdateOrderStatus/" + orderId
            });
        }

        public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = WebSD.ApiType.POST,
                Data = orderHeaderId,
                Url = WebSD.APIBase.OrderAPI + "/api/order/ValidateStripeSession"
            });
        }
    }
}
