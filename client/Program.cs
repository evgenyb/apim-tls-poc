using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cert2 = new X509Certificate2("C:\\Users\\evgen\\Projects\\pocs\\apim-agw-mtls-poc\\cert\\mycert.pfx", "foobar"))
            {
                var _clientHandler = new HttpClientHandler();
                _clientHandler.ClientCertificates.Add(cert2);
                _clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                using (var _client = new HttpClient(_clientHandler))
                //using (var response = _client.GetAsync("https://iac-ws4-evg-fd.azurefd.net/echo/resource").Result)
                using (var response = _client.GetAsync("https://iac-lab-api.iac-labs.com/echo/resource").Result)
                //using (var response = _client.GetAsync("https://iac-dev-ext-apim.azure-api.net/echo/resource").Result)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonString = response.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}