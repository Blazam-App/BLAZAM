namespace BLAZAM.Tests.Mocks
{
    internal class Mock_HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            HttpClient client = new HttpClient();
            return client;
        }
    }
}
