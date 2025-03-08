using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;
using Bulky.UI.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Bulky.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartViewModel shoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartViewModel = new()
            {
                ShoppingCartList = await _unitOfWork.ShoppingCarts.GetAllAsync(x => x.ApplicationUserId == userId,
                includeProperties: "Product"),
                Order = new ()
            };

            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartViewModel.Order.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartViewModel);
        }

        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartViewModel = new()
            {
                ShoppingCartList = await _unitOfWork.ShoppingCarts.GetAllAsync(x => x.ApplicationUserId == userId,
                includeProperties: "Product"),
                Order = new()
            };

            shoppingCartViewModel.Order.ApplicationUser = await _unitOfWork.ApplicationUsers.GetAsync(x => x.Id == userId);

            shoppingCartViewModel.Order.Name = shoppingCartViewModel.Order.ApplicationUser.Name;
            shoppingCartViewModel.Order.PhoneNumber = shoppingCartViewModel.Order.ApplicationUser.PhoneNumber;
            shoppingCartViewModel.Order.StreetAddress = shoppingCartViewModel.Order.ApplicationUser.StreetAddress;
            shoppingCartViewModel.Order.City = shoppingCartViewModel.Order.ApplicationUser.City;
            shoppingCartViewModel.Order.State = shoppingCartViewModel.Order.ApplicationUser.State;
            shoppingCartViewModel.Order.PostalCode = shoppingCartViewModel.Order.ApplicationUser.PostalCode;

            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartViewModel.Order.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartViewModel.ShoppingCartList = await _unitOfWork.ShoppingCarts.GetAllAsync(x => x.ApplicationUserId == userId,
                includeProperties: "Product");

            shoppingCartViewModel.Order.OrderDate = System.DateTime.Now;
            shoppingCartViewModel.Order.ApplicationUserId = userId;

            ApplicationUser applicationUser = await _unitOfWork.ApplicationUsers.GetAsync(x => x.Id == userId);

            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartViewModel.Order.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0) 
            {
                //it is a regular customer 
                shoppingCartViewModel.Order.PaymentStatus = PaymentStatus.PaymentStatusPending;
                shoppingCartViewModel.Order.OrderStatus = OrderStatus.StatusPending;
            }
            else
            {
                //it is a company user
                shoppingCartViewModel.Order.PaymentStatus = PaymentStatus.PaymentStatusDelayedPayment;
                shoppingCartViewModel.Order.OrderStatus = OrderStatus.StatusApproved;
            }

            await _unitOfWork.Orders.AddAsync(shoppingCartViewModel.Order);
            await _unitOfWork.CommitAsync();

            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartViewModel.Order.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            }
            await _unitOfWork.CommitAsync();

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic
                var domain = "https://localhost:44391/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartViewModel.Order.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in shoppingCartViewModel.ShoppingCartList)
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
                _unitOfWork.Orders.UpdateStripePaymentID(shoppingCartViewModel.Order.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.CommitAsync();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartViewModel.Order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            Order order = await _unitOfWork.Orders.GetAsync(x => x.Id == id, includeProperties: "ApplicationUser");
            if (order.PaymentStatus != PaymentStatus.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(order.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Orders.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Orders.UpdateStatus(id, OrderStatus.StatusApproved, PaymentStatus.PaymentStatusApproved);
                    await _unitOfWork.CommitAsync();
                }
                HttpContext.Session.Clear();
            }

            List<ShoppingCart> shoppingCarts = (List<ShoppingCart>)await _unitOfWork.ShoppingCarts
                .GetAllAsync(x => x.ApplicationUserId == order.ApplicationUserId);

            _unitOfWork.ShoppingCarts.RemoveRange(shoppingCarts);
            _unitOfWork.CommitAsync();
            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCarts.GetAsync(x => x.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCarts.Update(cartFromDb);
            _unitOfWork.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCarts.GetAsync(x => x.Id == cartId, tracked: true);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                HttpContext.Session.SetInt32(ApplicationRole.SessionCart, (await _unitOfWork.ShoppingCarts
                    .GetAllAsync(x => x.ApplicationUserId == cartFromDb.ApplicationUserId)).Count() - 1);
                _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCarts.Update(cartFromDb);
            }

            _unitOfWork.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCarts.GetAsync(x => x.Id == cartId, tracked: true);
            HttpContext.Session.SetInt32(ApplicationRole.SessionCart, (await _unitOfWork.ShoppingCarts
                    .GetAllAsync(x => x.ApplicationUserId == cartFromDb.ApplicationUserId)).Count() - 1);
            _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            _unitOfWork.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}