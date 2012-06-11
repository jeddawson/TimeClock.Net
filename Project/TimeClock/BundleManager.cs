using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using TimeClock.Resources;

namespace TimeClock
{
    /**
     *  Returns Bundle information for ASP.NET MVC to create a single file
     *  consisting of multiple JS or CSS files. Doing so cuts down on the 
     *  open sockets required to handle a page load. Also, we cache these
     *  files, so things happen fast!
     *  
     *  In development, we DO NOT minify. Debugging with minification is no fun.
     *  For production, minification will be enabled. Enable the PRODUCTION 
     *  constant to accomplish this.
     * 
    **/
    public class BundleManager
    {

        public static Bundle JsBundle()
        {
            IBundleTransform jsTrans;
            
            if (Constants.PRODUCTION)
                jsTrans = new JsMinify();
            else
                jsTrans = new NoTransform("text/javascript");
           
            var jsBundle = new Bundle("~/Scripts/js", jsTrans);

            jsBundle.AddFile("~/Scripts/app.js");
            jsBundle.AddFile("~/Scripts/foundation.js");
            jsBundle.AddFile("~/Scripts/modernizr.foundation.js");
            jsBundle.AddFile("~/Scripts/jquery-1.7.2.min.js");
            jsBundle.AddFile("~/Scripts/sammy.min.js");
            jsBundle.AddFile("~/Scripts/jquery.unobtrusive-ajax.min.js");
            jsBundle.AddFile("~/Scripts/knockout-2.1.0.js");

            return jsBundle;
        }

        public static Bundle CssBundle()
        {
            IBundleTransform cssTrans;
            
            if (Constants.PRODUCTION)
                cssTrans = new CssMinify();
            else
                cssTrans = new NoTransform("text/css");

            var cssBundle = new Bundle("~/Content/css", cssTrans);

            cssBundle.AddFile("~/Content/themes/foundation/app.css");
            cssBundle.AddFile("~/Content/themes/foundation/foundation.css");
            cssBundle.AddFile("~/Content/themes/foundation/foundation.top-bar.css");
            cssBundle.AddFile("~/Content/themes/foundation/presentation.css");

            return cssBundle;

        }
    }
}