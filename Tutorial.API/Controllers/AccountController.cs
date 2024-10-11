using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Tutorial.Infrastructure.Facades.Common.HttpClients;
using Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces;

namespace Tutorial.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController(IHttpClientSender httpClientSender)
        {
            this._httpClientSender = httpClientSender;
        }

        [HttpGet("SignInWithGoogle")]
        public async Task<IActionResult> SignInWithGoogle()
        {
            string authorizationUrl = $"{authorizationEndpoint}?response_type=code&scope=openid%20profile%20email&redirect_uri={Uri.EscapeDataString(redirectUri)}&client_id={clientId}";

            return Redirect(authorizationUrl);
        }

        [HttpGet("OAuth2Callback")]
        public async Task<IActionResult> OAuth2Callback([FromQuery] string code)
        {
            var authorizationCode = Uri.UnescapeDataString(code);

            using (HttpClient client = new HttpClient())
            {
                HttpHeadersManager headers = new HttpHeadersManager()
                    .AddUserAgent("Tutorial")
                    .AddAccept(MediaTypeNames.Application.Json);

                var content = new
                {
                    code = authorizationCode,
                    client_id = clientId,
                    client_secret = clientSecret,
                    redirect_uri = redirectUri,
                    grant_type = "authorization_code",
                };

                HttpResult response = await _httpClientSender
                    .WithUri(tokenEndpoint)
                    .UseMethod(HttpMethod.Post)
                    .WithHeaders(headers)
                    .WithContent(HttpClientExtensions.ToStringContent(content))
                    .SendAsync();

                if (response.IsSuccessStatusCode)
                    return Ok(await response.ReadAsStreamAsync());
                
                return Ok($"Lỗi khi trao đổi mã: {response.StatusCode}");
            }
        }
    }
}