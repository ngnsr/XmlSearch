using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Network;
using System.Net;
using System.Text;
using System.Web;


namespace XmlSearch.MAUI.GDrive
{
    public class CodeReceiver : ICodeReceiver
    {
        public CodeReceiver()
        {
        }

        private readonly int port = 5002;
        public string RedirectUri => string.Format("http://localhost:{0}/oauth2/redirect", port);

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            // start HttpListener
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri + "/");
            listener.Start();
            IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);

            // start Browser with auth
            await Launcher.Default.OpenAsync(url.Build().AbsoluteUri);

            result.AsyncWaitHandle.WaitOne();
            listener.Close();

            //Construct the AuthorizationCodeResponseUrl object

            var uri = await FooNotifier.WaitForResponse();

            var query = HttpUtility.ParseQueryString(uri.Query);

            return new AuthorizationCodeResponseUrl
            {
                Code = query.Get("code"),
                State = query.Get("state"),
                Error = query.Get("error"),
                ErrorDescription = query.Get("error_description"),
                ErrorUri = query.Get("error_uri")
            };
        }

        public static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            var context = listener.EndGetContext(result);
            var response = context.Response;

            //var rawUrl = context.Request.;

            // Send an HTML page instructing the user to close the browser
            var html = "<html><head><title>Authorization Complete</title></head><body>You can now close this browser window and return to your application.</body></html>";
            byte[] data = Encoding.UTF8.GetBytes(html);

            response.StatusCode = 200;
            response.ContentType = "text/html";
            response.ContentLength64 = data.Length;

            using (var output = response.OutputStream)
            {
                output.Write(data, 0, data.Length);
            }

            response.Close();
            FooNotifier.Notify(context.Request.Url);
        }

    }
}

