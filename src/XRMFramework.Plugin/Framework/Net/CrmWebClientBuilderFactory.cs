namespace XRMFramework.Net
{
    public interface ICrmWebClientBuilderFactory
    {
        ICrmWebClientBuilder CreateBuilder();
    }

    public class CrmWebClientBuilderFactory : ICrmWebClientBuilderFactory
    {
        private readonly ICrmWebClientBuilder webClientBuilder;

        public CrmWebClientBuilderFactory(ICrmWebClientBuilder webClientBuilder)
        {
            this.webClientBuilder = webClientBuilder;
        }

        public ICrmWebClientBuilder CreateBuilder()
            => webClientBuilder.ResetBuilder();
    }
}