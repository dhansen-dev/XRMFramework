namespace XRMFramework.Net
{
    public interface ICrmWebClientBuilderFactory
    {
        ICrmWebClientBuilder CreateBuilder();
    }

    public class CrmWebClientBuilderFactory : ICrmWebClientBuilderFactory
    {
        private readonly ICrmWebClientBuilder _webClientBuilder;

        public CrmWebClientBuilderFactory(ICrmWebClientBuilder webClientBuilder)
        {
            _webClientBuilder = webClientBuilder;
        }

        public ICrmWebClientBuilder CreateBuilder()
            => _webClientBuilder.ResetBuilder();
    }
}