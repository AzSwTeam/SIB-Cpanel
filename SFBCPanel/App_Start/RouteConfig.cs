using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SFBCPanel
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                namespaces: new[] { "SFBCPanel.Controllers" },
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NotDefault",
                url: "cpanel/{controller}/{action}/{id}",
                namespaces: new[] { "SFBCPanel.Controllers" },
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "NotDefault",
            //    url: "cpanel/{controller}/{action}/{id}",
            //    namespaces: new[] { "SFBCPanel.Controllers" },
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "AlsoNotDefault",
            //    url: "",
            //    namespaces: new[] { "SFBCPanel.Controllers" },
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

        }
    }
}
