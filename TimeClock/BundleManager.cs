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

            jsBundle.AddFile("~/Scripts/app.js", true);
            jsBundle.AddFile("~/Scripts/foundation.js", true);
            jsBundle.AddFile("~/Scripts/modernizr.foundation.js", true);
            jsBundle.AddFile("~/Scripts/jquery-1.7.2.min.js", true);
            jsBundle.AddFile("~/Scripts/jquery.unobtrusive-ajax.min.js", true);
            jsBundle.AddFile("~/Scripts/knockout-2.1.0.js", true);

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

            cssBundle.AddFile("~/Content/themes/foundation/app.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/foundation.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/foundation.top-bar.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/presentation.css", true);

            return cssBundle;

        }

        public static Bundle ModernizrBundle()
        {
            IBundleTransform jsTrans;

            if (Constants.PRODUCTION)
                jsTrans = new JsMinify();
            else
                jsTrans = new NoTransform("text/javascript");

            var jsBundle = new Bundle("~/Scripts/modernizr", jsTrans);

            jsBundle.AddFile("~/Scripts/modernizr.foundation.js", true);

            return jsBundle;
        }

    }
}