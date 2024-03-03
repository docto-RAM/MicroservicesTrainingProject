using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        protected ResponseDto _response;

        private readonly AppDbContext _db;
        private IProductService _productService;
        private IMapper _mapper;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;

        public OrderAPIController(
            AppDbContext db,
            IProductService productService,
            IMapper mapper,
            IMessageBus messageBus,
            IConfiguration configuration)
        {
            _response = new ResponseDto();

            _db = db;
            _productService = productService;
            _mapper = mapper;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDto? GetOrders(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> objList;

                if (User.IsInRole(SD.Role.Admin))
                {
                    objList = _db.OrderHeaders
                        .Include(x => x.OrderDetails)
                        .OrderByDescending(x => x.OrderHeaderId)
                        .ToList();
                }
                else
                {
                    objList = _db.OrderHeaders
                        .Include(x => x.OrderDetails)
                        .Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.OrderHeaderId)
                        .ToList();
                }

                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDto? GetOrder(int id)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders
                    .Include(x => x.OrderDetails)
                    .First(x => x.OrderHeaderId == id);

                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.OrderStatus.Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

                OrderHeader createdOrder = (await _db.OrderHeaders.AddAsync(_mapper.Map<OrderHeader>(orderHeaderDto))).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = createdOrder.OrderHeaderId;
                _response.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderId);

                if (orderHeader != null)
                {
                    if (newStatus == SD.OrderStatus.Cancelled)
                    {
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId
                        };

                        var refundService = new RefundService();
                        Refund refund = refundService.Create(options);
                    }

                    orderHeader.Status = newStatus;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = SD.Stripe.CheckoutSessionMode.Payment
                };

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.99 -> 2099
                            Currency = SD.Stripe.Currency.USD,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    var discountsObj = new List<SessionDiscountOptions>()
                    {
                        new SessionDiscountOptions
                        {
                            Coupon = stripeRequestDto.OrderHeader.CouponCode
                        }
                    };

                    options.Discounts = discountsObj;
                }

                var service = new SessionService();
                Session session = service.Create(options);

                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();

                stripeRequestDto.StripeSessionUrl = session.Url;
                _response.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

                var sessionService = new SessionService();
                Session session = sessionService.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent.Status == SD.Stripe.PaymentIntentStatus.Succeeded)
                {
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = SD.OrderStatus.Approved;
                    await _db.SaveChangesAsync();

                    RewardDto rewardDto = new()
                    {
                        OrderId = orderHeader.OrderHeaderId,
                        RewardActivity = Convert.ToInt32(orderHeader.OrderTotal),
                        UserId = orderHeader.UserId
                    };
                    await _messageBus.PublishMessage(rewardDto, _configuration.GetValue<string>("QueueAndTopicNames:CreatedOrderTopic"));

                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }
    }
}
