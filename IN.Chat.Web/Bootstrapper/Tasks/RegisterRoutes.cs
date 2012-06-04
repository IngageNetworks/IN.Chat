using System.Web.Mvc;
using System.Web.Routing;
using IN.Chat.Web.Bootstrapper.Tasks.Core;

namespace IN.Chat.Web.Bootstrapper.Tasks
{
    public class RegisterRoutes : IBootstrapperPerApplicationTask
    {
        public void Execute()
        {
            var routes = RouteTable.Routes;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("content/{*pathInfo}");
            routes.IgnoreRoute("scripts/{*pathInfo}");

            routes.MapRoute(
                name: "Account - Create",
                url: "account/create",
                defaults: new { controller = "Account", action = "Create" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "Account - Create - Post",
                url: "account/create",
                defaults: new { controller = "Account", action = "Create" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Login", action = "Login" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "Login - Post",
                url: "login",
                defaults: new { controller = "Login", action = "Login" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Login", action = "Logout" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "Chat",
                url: "chat",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                "Default",
                "{*url}",
                new { controller = "Error", action = "NotFound" }
            );
        }
    }
}