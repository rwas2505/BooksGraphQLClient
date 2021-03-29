using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BooksGraphQLClient
{
  internal class OktaToken
  {
      [JsonProperty(PropertyName = "access_token")]
      public string AccessToken { get; set; }

      [JsonProperty(PropertyName = "expires_in")]
      public int ExpiresIn { get; set; }

      public DateTime ExpiresAt { get; set; }

      public string Scope { get; set; }

      [JsonProperty(PropertyName = "token_type")]
      public string TokenType { get; set; }

      public bool IsValidAndNotExpiring
      {
          get
          {
              return !String.IsNullOrEmpty(this.AccessToken) &&
        this.ExpiresAt > DateTime.UtcNow.AddSeconds(30);
          }
      }
  }

  public class TokenService : ITokenService
  {
    private OktaToken _token = new OktaToken();
    private readonly OktaConfig _oktaSettings;

    public TokenService(OktaConfig oktaSettings) => _oktaSettings = oktaSettings;

    public async Task<string> GetToken()
    {
      if (_token.IsValidAndNotExpiring)
      {
        return _token.AccessToken;
      }

      _token = await GetNewAccessToken();

      return _token.AccessToken;
    }

    private async Task<OktaToken> GetNewAccessToken()
    {
      var client = new HttpClient();
      var clientId = _oktaSettings.ClientId;
      var clientSecret = _oktaSettings.ClientSecret;
      var clientCreds = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(clientCreds));

      var postMessage = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"scope", "access_token"}
            };

      var request = new HttpRequestMessage(HttpMethod.Post, _oktaSettings.TokenUrl)
      {
        Content = new FormUrlEncodedContent(postMessage)
      };

      var response = await client.SendAsync(request);

      if (response.IsSuccessStatusCode)
      {
        var json = await response.Content.ReadAsStringAsync();
        var newToken = JsonConvert.DeserializeObject<OktaToken>(json);
        newToken.ExpiresAt = DateTime.UtcNow.AddSeconds(_token.ExpiresIn);

        return newToken;
      }

      throw new ApplicationException("Unable to retrieve access token from Okta");
    }
  }
}