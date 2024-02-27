namespace Mango.Web.Utility
{
    public class SD
    {
        public const string TokenCookie = "JWTToken";

        public class APIBase
        {
            public static string ProductAPI { get; set; }
            public static string CouponAPI { get; set; }
            public static string AuthAPI { get; set; }
            public static string ShoppingCartAPI { get; set; }
            public static string OrderAPI { get; set; }
        }

        public class Role
        {
            public const string Admin = "ADMIN";
            public const string Customer = "CUSTOMER";
        }

        public enum ApiType
        {
            GET,
            POST, 
            PUT,
            DELETE
        }
    }
}
