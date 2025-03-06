using Bulky.DataAccess.Repository.IRepositories;
using Bulky.UI.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Bulky.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderViewModel orderViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            orderViewModel = new()
            {
                Order = await _unitOfWork.Orders.GetAsync(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await _unitOfWork.OrderDetails.GetAllAsync(x => x.OrderId == orderId, includeProperties: "Product")
            };

            return View(orderViewModel);
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRole.Role_Admin + "," + ApplicationRole.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderFromDb = await _unitOfWork.Orders.GetAsync(x => x.Id == orderViewModel.Order.Id);
            orderFromDb.Name = orderViewModel.Order.Name;
            orderFromDb.PhoneNumber = orderViewModel.Order.PhoneNumber;
            orderFromDb.StreetAddress = orderViewModel.Order.StreetAddress;
            orderFromDb.City = orderViewModel.Order.City;
            orderFromDb.State = orderViewModel.Order.State;
            orderFromDb.PostalCode = orderViewModel.Order.PostalCode;
            if (!string.IsNullOrEmpty(orderViewModel.Order.Carrier))
            {
                orderFromDb.Carrier = orderViewModel.Order.Carrier;
            }
            if (!string.IsNullOrEmpty(orderViewModel.Order.TrackingNumber))
            {
                orderFromDb.Carrier = orderViewModel.Order.TrackingNumber; 
            }

            _unitOfWork.Orders.Update(orderFromDb);
            _unitOfWork.CommitAsync();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRole.Role_Admin + "," + ApplicationRole.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            _unitOfWork.Orders.UpdateStatus(orderViewModel.Order.Id, OrderStatus.StatusInProcess);
            _unitOfWork.CommitAsync();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRole.Role_Admin + "," + ApplicationRole.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var order = await _unitOfWork.Orders.GetAsync(x => x.Id == orderViewModel.Order.Id);
            order.TrackingNumber = orderViewModel.Order.TrackingNumber;
            order.Carrier = orderViewModel.Order.Carrier;
            order.OrderStatus = OrderStatus.StatusShipped;
            order.ShippingDate = DateTime.Now;
            if (order.PaymentStatus == PaymentStatus.PaymentStatusDelayedPayment)
            {
                order.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.Orders.Update(order);
            _unitOfWork.CommitAsync();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRole.Role_Admin + "," + ApplicationRole.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {
            var order = await _unitOfWork.Orders.GetAsync(x => x.Id == orderViewModel.Order.Id);

            if (order.PaymentStatus == PaymentStatus.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = order.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.Orders.UpdateStatus(order.Id, OrderStatus.StatusCancelled, OrderStatus.StatusRefunded);
            }
            else
            {
                _unitOfWork.Orders.UpdateStatus(order.Id, OrderStatus.StatusCancelled, OrderStatus.StatusCancelled);
            }
            _unitOfWork.CommitAsync();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.Order.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> Details_PAY_NOW()
        {
            orderViewModel.Order = await _unitOfWork.Orders
                .GetAsync(x => x.Id == orderViewModel.Order.Id, includeProperties: "ApplicationUser");
            orderViewModel.OrderDetail = await _unitOfWork.OrderDetails
                .GetAllAsync(x => x.OrderId == orderViewModel.Order.Id, includeProperties: "Product");

            //stripe logic
            var domain = "https://localhost:44391/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderViewModel.Order.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderViewModel.Order.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderViewModel.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.Orders.UpdateStripePaymentID(orderViewModel.Order.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.CommitAsync();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> PaymentConfirmation(int orderId)
        {
            Entity.Entities.Order order = await _unitOfWork.Orders.GetAsync(x => x.Id == orderId);
            if (order.PaymentStatus == PaymentStatus.PaymentStatusDelayedPayment)
            {
                //this is an order by company
                var service = new SessionService();
                Session session = await service.GetAsync(order.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Orders.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Orders.UpdateStatus(orderId, order.OrderStatus, PaymentStatus.PaymentStatusApproved);
                    _unitOfWork.CommitAsync();
                }
            }
            return View(orderId);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<Entity.Entities.Order> objOrders;

            if (User.IsInRole(ApplicationRole.Role_Admin) || User.IsInRole(ApplicationRole.Role_Employee))
            {
                objOrders = await _unitOfWork.Orders.GetAllAsync(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrders = await _unitOfWork.Orders.GetAllAsync(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrders = objOrders.Where(x => x.PaymentStatus == PaymentStatus.PaymentStatusDelayedPayment).ToList();
                    break;
                case "inprocess":
                    objOrders = objOrders.Where(x => x.OrderStatus == OrderStatus.StatusInProcess).ToList();
                    break;
                case "completed":
                    objOrders = objOrders.Where(x => x.OrderStatus == OrderStatus.StatusShipped).ToList();
                    break;
                case "approved":
                    objOrders = objOrders.Where(x => x.OrderStatus == OrderStatus.StatusApproved).ToList();
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrders });
        }
        #endregion
    }
}