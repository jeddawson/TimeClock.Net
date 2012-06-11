using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TimeClock.Formatters;
using TimeClock.Models;

namespace TimeClock
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // This is the Web API route:
            routes.MapHttpRoute(
                name: "RESTApi",
                routeTemplate: "REST/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //This is the MVC route:
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Timeclock", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Use LocalDB for Entity Framework by default
            Database.DefaultConnectionFactory = new SqlConnectionFactory("Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");
            
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //Putting all of the JS into a single file:
            BundleTable.Bundles.Add(BundleManager.JsBundle());
            
            //Putting all of the CSS into a single file:
            BundleTable.Bundles.Add(BundleManager.CssBundle());

            //Wipe out XML, who likes XML!?
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            //Enable a custom JSONP formatter, MS: this should be available by default!
            GlobalConfiguration.Configuration.Formatters.Insert(0, new JsonpMediaTypeFormatter());

        }
    }
}