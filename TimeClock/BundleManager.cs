using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace TimeClock
{
    public class BundleManager
    {

        public static Bundle JsBundle()
        {
            IBundleTransform jsTrans = new NoTransform("text/javascript");
           
            var jsBundle = new Bundle("~/Scripts/js", jsTrans);

            jsBundle.AddFile("~/Scripts/app.js", true);
            jsBundle.AddFile("~/Scripts/foundation.js", true);
            jsBundle.AddFile("~/Scripts/modernizr.foundation.js", true);
            jsBundle.AddFile("~/Scripts/jquery-1.7.2.min.js", true);
            jsBundle.AddFile("~/Scripts/jquery.unobtrusive-ajax.min.js", true);
            jsBundle.AddFile("~/Scripts/knockout.js", true);

            return jsBundle;
        }

        public static Bundle CssBundle()
        {
            IBundleTransform cssTrans = new NoTransform("text/css");

            var cssBundle = new Bundle("~/Content/css", cssTrans);

            cssBundle.AddFile("~/Content/themes/foundation/app.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/foundation.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/foundation.top-bar.css", true);
            cssBundle.AddFile("~/Content/themes/foundation/presentation.css", true);

            return cssBundle;

        }

        public static Bundle ModernizrBundle()
        {
            IBundleTransform jsTrans = new NoTransform("text/javascript");

            var jsBundle = new Bundle("~/Scripts/modernizr", jsTrans);

            jsBundle.AddFile("~/Scripts/modernizr.foundation.js", true);

            return jsBundle;
        }

    }
}