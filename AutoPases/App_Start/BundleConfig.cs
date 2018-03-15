using System.Web;
using System.Web.Optimization;

namespace AutoPases
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);

            BundleTable.EnableOptimizations = false;

            var jqueryBundle = new ScriptBundle("~/scripts/jquery")
                        .Include("~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/kendo.all.min.js",
                        "~/Scripts/console.js",
                        "~/Scripts/kendo.aspnetmvc.min.js",
                        "~/Scripts/monibyte.utiles.comun.js",
                        "~/Scripts/monibyte.utiles.jquery.ui.js");

            jqueryBundle.Transforms.Add(new JsMinify());
            bundles.Add(jqueryBundle);

            var validateBundle = new ScriptBundle("~/scripts/validate")
                   .Include("~/Scripts/jquery.validate*",
                   "~/Scripts/monibyte.utiles.globalize.js");

            validateBundle.Transforms.Add(new JsMinify());
            bundles.Add(validateBundle);

            var kendouiCssBundle = new StyleBundle("~/content/kendoui/css").Include(
               "~/content/kendoui/kendo.common.core.min.css",
               "~/content/kendoui/kendo.common.min.css",
               "~/content/kendoui/kendo.default.min.css",
               "~/content/kendoui/kendo.mobile.all.min.css",
               "~/content/kendoui/kendo.rtl.min.css",
               "~/content/kendoui/kendo.default.mobile.min.css",
               "~/content/kendoui/console.css");
            kendouiCssBundle.Transforms.Add(new CssMinify());
            bundles.Add(kendouiCssBundle);

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));

            var monibyteCssBundle = new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css");

            monibyteCssBundle.Transforms.Add(new CssMinify());
            bundles.Add(monibyteCssBundle);

        }
    }
}
