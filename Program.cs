using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BooksGraphQLClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var oktaConfig = new OktaConfig
            {
                ClientId = "0oaeus467Rd076IQL5d6",
                ClientSecret = "wFSoK-Yz0F3H5-nxs8LR5imfUrL7BmCyDKNXKkoy",
                TokenUrl = "https://dev-84275405.okta.com/oauth2/default/v1/token"
            };

            var tokenService = new TokenService(oktaConfig);
            var token = await tokenService.GetToken();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query = @"
            {
                author(id: 1) {
                    name
                }
            }";

            var postData = new  { Query = query };
            var stringContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

            var postUri = "http://localhost:5000";
            var res = await httpClient.PostAsync(postUri, stringContent);
            if (res.IsSuccessStatusCode)
            {
            var content = await res.Content.ReadAsStringAsync();

            Console.WriteLine(content);
            }
            else
            {
            Console.WriteLine($"Error occurred... Status code:{res.StatusCode}");
            }
        }
    }
}
