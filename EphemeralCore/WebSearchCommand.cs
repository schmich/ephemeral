using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Net;
using System.IO;
using System.ComponentModel.Composition;

namespace Ephemeral.Commands
{
    [Export(typeof(ICommandFactory))]
    class WebSearchCommandFactory : ICommandFactory
    {
        [ImportingConstructor]
        public WebSearchCommandFactory(ICommandStore store)
        {
            store.AddCommand(new WebSearchCommand("msdn", "http://msdn.microsoft.com/", "http://social.msdn.microsoft.com/Search/en-US?query=%s"));
            store.AddCommand(new WebSearchCommand("StackOverflow", "http://stackoverflow.com/", "http://www.google.com/#hl=en&q=%s+site%3Astackoverflow.com"));
            store.AddCommand(new WebSearchCommand("Wikipedia", "http://en.wikipedia.org/", "http://en.wikipedia.org/wiki/Special:Search/%s"));
            store.AddCommand(new WebSearchCommand("?", "http://bing.com/", "http://www.bing.com/search?q=%s"));
            store.AddCommand(new WebSearchCommand("bing", "http://bing.com/", "http://www.bing.com/search?q=%s"));
            store.AddCommand(new WebSearchCommand("AcronymFinder", "http://www.acronymfinder.com/", "http://www.acronymfinder.com/%s.html"));
            store.AddCommand(new WebSearchCommand("Microsoft Mail", "https://mail.microsoft.com", "https://mail.microsoft.com"));
            store.AddCommand(new WebSearchCommand("gmail", "https://gmail.com", "https://gmail.com"));
        }
    }

    class WebSearchCommand : ICommand
    {
        public WebSearchCommand(string searchKeyword, string defaultUri, string searchUri)
        {
            _searchKeyword = searchKeyword;
            _defaultUri = defaultUri;
            _searchUri = searchUri;

            string rootUri = (new Uri(defaultUri)).GetLeftPart(UriPartial.Authority);
            UriBuilder faviconUri = new UriBuilder(rootUri);
            faviconUri.Path = "/apple-touch-icon.png";

            _icon = GetWebIcon(faviconUri.ToString());
            if (_icon == null)
            {
                faviconUri.Path = "/favicon.ico";
                _icon = GetWebIcon(faviconUri.ToString());
            }

            if (_icon != null)
            {
                // TODO: hack
                try
                {
                    _icon.MakeTransparent();
                }
                catch (InvalidOperationException)
                {
                    // Actual icons cannot be made transparent (they already are)
                }
            }
        }

        public string Name
        {
            get { return _searchKeyword; }
        }

        public Bitmap Icon
        {
            get { return _icon; }
        }

        public void Execute(string arguments)
        {
            string uri;
            if (string.IsNullOrEmpty(arguments))
            {
                uri = _defaultUri;
            }
            else
            {
                string query = Uri.EscapeDataString(arguments);
                uri = _searchUri.Replace("%s", query);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = uri;
            startInfo.UseShellExecute = true;

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
            }
        }

        Bitmap GetWebIcon(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            try // TODO: Hack for 404s, yuck
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // TODO: Hack to support multiple types.
                        try
                        {
                            return new Bitmap(response.GetResponseStream());
                        }
                        catch
                        {
                            try
                            {
                                return (new Icon(response.GetResponseStream())).ToBitmap();
                            }
                            catch
                            {
                                return null;
                            }
                        }
                        //return (new Icon(response.GetResponseStream())).ToBitmap();
                    }

                    return null;
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        string _searchKeyword;
        string _defaultUri;
        string _searchUri;

        Bitmap _icon;
    }
}
