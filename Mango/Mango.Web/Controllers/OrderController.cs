using Mango.Utility;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllOrders(string status)
        {
            IEnumerable<OrderHeaderDto> list;
            string userId = "";

            if (!User.IsInRole(SD.Role.Admin))
            {
                userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }

            ResponseDto response = _orderService.GetOrders(userId).GetAwaiter().GetResult();

            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));

                switch (status)
                {
                    case "approved":
                        list = list.Where(x => x.Status == SD.OrderStatus.Approved);
                        break;
                    case "readyforpickup":
                        list = list.Where(x => x.Status == SD.OrderStatus.ReadyForPickup);
                        break;
                    case "cancelled":
                        list = list.Where(x => x.Status == SD.OrderStatus.Cancelled || x.Status == SD.OrderStatus.Refunded);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                list = new List<OrderHeaderDto>();
            }

            return Json(new { data = list.OrderByDescending(x => x.OrderHeaderId) });
        }

        [Authorize]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            OrderHeaderDto orderHeaderDto;

            string userId = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            var response = await _orderService.GetOrder(orderId);

            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            }
            else
            {
                orderHeaderDto = new OrderHeaderDto();
            }

            if (!User.IsInRole(SD.Role.Admin) && userId != orderHeaderDto.UserId)
            {
                return NotFound();
            }

            return View(orderHeaderDto);
        }
    }
}
