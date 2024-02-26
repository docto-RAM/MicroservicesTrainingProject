﻿namespace Mango.Services.OrderAPI.Utility
{
    public class SD
    {
        public class Status
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
    }
}
