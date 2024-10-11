using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Text.Json;
using Tutorial.Infrastructure.Facades.Common.HttpClients;
using Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces;

namespace Tutorial.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestHttpClientController : ControllerBase
    {
        private readonly IHttpClientSender _httpClientSender;
        private readonly HttpClient _httpClient;

        public TestHttpClientController(IHttpClientSender httpClientSender, HttpClient httpClient)
        {
            this._httpClientSender = httpClientSender;
            this._httpClient = httpClient;
        }

        [HttpGet("TestEmpty")]
        public async Task<IActionResult> TestEmpty()
        {
            return Ok();
        }

        [HttpGet("GetDataUseClientAsync")]
        public async Task<IActionResult> GetDataUseClientAsync()
        {
            var urlString = "https://jsonplaceholder.typicode.com/posts";

            var result = await _httpClientSender
                .WithUri(urlString)
                .UseMethod(HttpMethod.Get)
                .SendAsync();

            if (result.IsSuccessStatusCode)
                return Ok(await result.ReadAsStreamAsync());

            return Ok(result.Content);
        }

        [HttpGet("GetDataDefaultClientAsync")]
        public async Task<IActionResult> GetDataDefaultClientAsync()
        {
            var urlString = "http://localhost:5256/WeatherForecast";

            var header = new HttpHeadersManager()
                .AddAccept(MediaTypeNames.Application.Json)
                .AddUserAgent("ABCXYZ")
                .AddHeader("Host", "localhost:5237");

            var result = await _httpClientSender
                .WithUri(urlString)
                .UseMethod(HttpMethod.Get)
                .WithHeaders(header)
                .SendAsync();

            return Ok(await result.ReadAsStreamAsync());
        }

        [HttpGet("GetDataDefaultClientAsync2")]
        public async Task<IActionResult> GetDataDefaultClientAsync2()
        {
            var urlString = "https://raw.githubusercontent.com/xuanthulabnet/jekyll-example/master/images/jekyll-01.png";

            var header = new HttpHeadersManager()
                .AddAccept(MediaTypeNames.Application.Json);

            var result = await _httpClientSender
                .WithUri(urlString)
                .UseMethod(HttpMethod.Get)
                .WithHeaders(header)
                .SendAsync();

            return Ok(await result.ReadAsStreamAsync());
        }

        [HttpPost("PostDataAsync")]
        public async Task<IActionResult> PostDataAsync()
        {
            var urlString = "https://jsonplaceholder.typicode.com/posts";

            var post = new
            {
                UserId = 1,
                Title = new string('X', 1),
                Body = new string('X', 1),
            };

            var header = new HttpHeadersManager()
                .AddAccept(MediaTypeNames.Application.Json);

            var result = await _httpClientSender
                .WithUri(urlString)
                .UseMethod(HttpMethod.Post)
                .WithHeaders(header)
                .WithContent(HttpClientExtensions.ToStringContent(post))
                .SendAsync();

            return Ok(await result.ReadAsStreamAsync());
        }

        [HttpPost("PostDataAsyncUseClient")]
        public async Task<IActionResult> PostDataAsyncUseClient()
        {
            var urlString = "https://jsonplaceholder.typicode.com/posts";

            var post = new
            {
                UserId = 1,
                Title = "ABC",
                Body = "XYZ",
            };

            var result = await _httpClientSender
                .UseClient(_httpClient, false)
                .WithUri(urlString)
                .UseMethod(HttpMethod.Post)
                .WithContent(HttpClientExtensions.ToStringContent(post))
                .SendAsync();

            return Ok(result.ReadAsStringAsync().Result);
        }
    }
}
