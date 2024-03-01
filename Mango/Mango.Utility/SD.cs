namespace Mango.Utility
{
    public class SD
    {
        public class OrderStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string ReadyForPickup = "ReadyForPickup";
            public const string Completed = "Completed";
            public const string Refunded = "Refunded";
            public const string Cancelled = "Cancelled";
        }

        public class Role
        {
            public const string Admin = "ADMIN";
            public const string Customer = "CUSTOMER";
        }

        public class Stripe
        {
            public class Currency
            {
                public const string USD = "usd";
            }

            public class CheckoutSessionMode
            {
                public const string Payment = "payment";
                public const string Setup = "setup";
                public const string Subscription = "subscription";
            }

            public class PaymentIntentStatus
            {
                public const string RequiresPaymentMethod = "requires_payment_method";
                public const string RequiresConfirmation = "requires_confirmation";
                public const string RequiresAction = "requires_action";
                public const string Processing = "processing";
                public const string RequiresCapture = "requires_capture";
                public const string Canceled = "canceled";
                public const string Succeeded = "succeeded";
            }
        }
    }
}
