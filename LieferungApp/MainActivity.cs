using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using LieferungApp.Views;
using LieferungApp.Models;

namespace LieferungApp
{
    [Activity(Label = "LieferungApp",  MainLauncher = false)]
    public class MainActivity : Activity
    {
        public WebView WebView { get; set; }
        private ProgressBar bar;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ActionBar.Hide();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            bar = (ProgressBar)FindViewById(Resource.Id.marker_progress);
            WebView = FindViewById<WebView>(Resource.Id.webView);
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.CacheMode = CacheModes.CacheElseNetwork;

            // Use subclassed WebViewClient to intercept hybrid native calls
            WebView.SetWebViewClient(new HybridWebViewClient(bar));

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1() { Text = "Text goes here" };
            var template = new RazorView() { Model = model };
            var page = template.GenerateString();
            WebView.LoadUrl("http://www.lieferung45.at/");
            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Assets folder
            //webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);

        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                switch (keyCode)
                {
                    case Keycode.Back:
                        if (WebView.CanGoBack())
                        {
                            WebView.GoBack();
                        }
                        else
                        {
                            Finish();
                        }
                        return true;
                }

            }
            return base.OnKeyDown(keyCode, e);
        }

        private class HybridWebViewClient : WebViewClient
        {
            private ProgressBar _bar;
            public HybridWebViewClient(ProgressBar bar)
            {
                _bar = bar;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                _bar.Visibility = ViewStates.Visible;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                _bar.Visibility = ViewStates.Gone;
            }

            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {

                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                var scheme = "hybrid:";

                if (!url.StartsWith(scheme))
                    return false;

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

                if (method == "UpdateLabel")
                {
                    var textbox = parameters["textbox"];

                    // Add some text to our string here so that we know something
                    // happened on the native part of the round trip.
                    var prepended = string.Format("C# says \"{0}\"", textbox);

                    // Build some javascript using the C#-modified result
                    var js = string.Format("SetLabelText('{0}');", prepended);

                    webView.LoadUrl("javascript:" + js);
                }

                return true;
            }
        }
    }
}

