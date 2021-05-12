using System;
using System.IO;
using System.Net;

using XRMFramework.Text;

namespace XRMFramework.Net
{
    public interface ICrmWebClient
    {
        string Delete<TInputModel>(TInputModel data);
        string Get(string uri);

        TResponseType Get<TResponseType>(string uri);

        /// <summary>
        /// Makes a post request with <paramref name="data"/> as is
        /// </summary>
        /// <param name="uri">The endpoint to call</param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        //string Post(string uri, string data);
        string Post<TInputModel>(string uri, TInputModel data);
        string Post<TInputModel>(TInputModel data);
        string Put<TInputModel>(string uri, TInputModel data);
        string Put<TInputModel>(TInputModel data);
    }

    public class CrmWebClient : WebClient, ICrmWebClient
    {
        private Uri baseUri;

        public Uri BaseUri
        {
            get { return baseUri ?? throw new NullReferenceException("Cannot access baseuri before setting it"); }
            set { baseUri = value; }
        }

        public TResponseType Get<TResponseType>(string uri)
            => Json.Deserialize<TResponseType>(Get(uri));

        public string Get(string uri)
            => MakeCall(() => DownloadString(MakeUri(uri)));

        public string Post<TInputModel>(string uri, TInputModel data)
            => MakeCall(() => UploadString(MakeUri(uri), Json.Serialize(data)));

        public string Post<TInputModel>(TInputModel data)
            => Post("", data);

        public string Put<TInputModel>(string uri, TInputModel data)
            => MakeCall(() => UploadString(MakeUri(uri), "PUT", Json.Serialize(data)));

        /// <summary>
        /// Issues a put request with the supplied data
        /// </summary>
        /// <typeparam name="TInputModel"></typeparam>
        /// <param name="data"></param>
        /// <returns>The response string</returns>
        public string Put<TInputModel>(TInputModel data)
            => Put("", data);

        public string Delete<TInputModel>(string uri, TInputModel data)
            => MakeCall(() => UploadString(MakeUri(uri), "DELETE", Json.Serialize(data)));

        public string Delete<TInputModel>(TInputModel data)
            => Delete("", data);

        /// <summary>
        /// Handle web exception
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="call"></param>
        /// <returns></returns>
        private TReturn MakeCall<TReturn>(Func<TReturn> call)
        {            
            try
            {
                var result = call();
                return result;
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException(
                        "The timeout elapsed while attempting to issue the request.", wex);
                }

                string str = string.Empty;

                if (wex.Response != null)
                {
                    using (StreamReader reader =
                        new StreamReader(wex.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }
                    wex.Response.Close();

                    throw new WebException(str, wex);
                }

                throw;
            }
        }

        private Uri MakeUri(string uri)
                            => BaseAddress == null ? new Uri(uri) : new Uri(new Uri(BaseAddress), uri);

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = 15000;
                request.KeepAlive = false;
            }

            return request;
        }
    }
}