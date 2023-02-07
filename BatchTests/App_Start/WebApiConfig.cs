using System.Web.Http;
using System.Web.Http.Batch;

namespace BatchTests {
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpBatchRoute(
                routeName: "batch",
                routeTemplate: "api/batch",
                batchHandler: new DefaultHttpBatchHandler(GlobalConfiguration.DefaultServer));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
