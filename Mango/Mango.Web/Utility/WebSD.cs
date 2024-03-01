namespace Mango.Web.Utility
{
    public class WebSD
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

        public enum ApiType
        {
            GET,
            POST, 
            PUT,
            DELETE
        }
    }
}
