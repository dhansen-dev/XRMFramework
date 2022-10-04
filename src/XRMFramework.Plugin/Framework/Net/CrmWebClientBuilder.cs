using System;
using System.Net;
using System.Text;

namespace XRMFramework.Net
{
    public interface ICrmWebClientBuilder
    {
        ICrmWebClient CreateClient();

        ICrmWebClientBuilder ResetBuilder();

        ICrmWebClientBuilder WithBaseUrl(Uri baseUrl);
        ICrmWebClientBuilder WithBearerToken(string token);
        ICrmWebClientBuilder WithContentType(string contentType);

        ICrmWebClientBuilder WithContentTypeApplicationJson();

        ICrmWebClientBuilder WithEncoding(Encoding encodig);

        ICrmWebClientBuilder WithHeader(HttpRequestHeader header, string value);

        ICrmWebClientBuilder WithHeader(string header, string value);
    }

    public class CrmWebClientBuilder : ICrmWebClientBuilder
    {
        private CrmWebClient _client;

        public CrmWebClientBuilder()
        {
            _client = new CrmWebClient();
        }

        public ICrmWebClient CreateClient()
        {
            var internalClient = _client;

            _client = new CrmWebClient();

            return internalClient;
        }
            

        public ICrmWebClientBuilder ResetBuilder()
            => new CrmWebClientBuilder();

        public ICrmWebClientBuilder WithBaseUrl(Uri baseUrl)
        {
            _client.BaseAddress = baseUrl.ToString();

            return this;
        }

        public ICrmWebClientBuilder WithBearerToken(string token)
        {
            _client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");

            return this;
        }

        public ICrmWebClientBuilder WithContentType(string contentType)
        {
            _client.Headers.Add("Content-Type", contentType);

            return this;
        }

        public ICrmWebClientBuilder WithContentTypeApplicationJson()
        {
            _client.Headers.Add("Content-Type", "application/json");

            return this;
        }

        public ICrmWebClientBuilder WithEncoding(Encoding encodig)
        {
            _client.Encoding = encodig;
            return this;
        }

        public ICrmWebClientBuilder WithHeader(string header, string value)
        {
            _client.Headers.Add(header, value);

            return this;
        }

        public ICrmWebClientBuilder WithHeader(HttpRequestHeader header, string value)
        {
            _client.Headers.Add(header, value);

            return this;
        }
    }
}