using AutoMapper;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using Tutorial.Domain.Model.Options;
using Tutorial.Infrastructure.Facades.Common.Helpers;
using Tutorial.Infrastructure.Facades.Common.HttpClients.HttpClientHandlers;
using Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces;
using Tutorial.Infrastructure.Services.Interfaces;

namespace Tutorial.Infrastructure.Facades.Common.HttpClients;

public class HttpClientSender : IHttpClientSender
{
    private readonly IMapper _mapper;
    private HttpRequestMessage _request = new();
    private bool _isDefaultLog = true;
    private HttpClient? _httpClient;

    private static readonly SocketsHttpHandler _socketHandler = new()
    {
        // Tái sử dụng kết nối trong pool trong 2 phút, sau đó tạo lại kết nối mới --> linh hoạt khi IP máy chủ thay đổi
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        // Tăng số lượng kết nối tối đa cho mỗi server để giảm thời gian chờ
        MaxConnectionsPerServer = 2000,
        // Giữ kết nối hoạt động
        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
        // Sau 1p nếu kết nối vẫn còn hoạt động thì gửi một yêu cầu để đảm bảo server đích không đóng lại
        KeepAlivePingTimeout = TimeSpan.FromMinutes(10),
    };

    /// <summary>
    /// Retry request khi bị huỷ hoặc hết timeout
    /// </summary>
    private static RetryMessageHandler _retryMessageHandler = new RetryMessageHandler(_socketHandler);

    private static readonly HttpClient _defaultHttpClient = new(_retryMessageHandler)
    {
        Timeout = TimeSpan.FromSeconds(100),
    };

    public HttpClientSender(IMapper mapper)
    {
        this._mapper = mapper;
    }

    public IHttpClientSender UseClient(HttpClient httpClient, bool enableLogging = false)
    {
        _request = new();
        _isDefaultLog = enableLogging;
        this._httpClient = httpClient;
        return this;
    }

    public IHttpClientSender UseMethod(HttpMethod method)
    {
        _request.Method = method;
        return this;
    }

    public IHttpClientSender WithUri(string uri)
    {
        _request.RequestUri = new Uri(uri);
        return this;
    }

    public IHttpClientSender WithUri(Uri uri)
    {
        _request.RequestUri = uri;
        return this;
    }

    public IHttpClientSender WithContent(HttpContent content)
    {
        _request.Content = content;
        return this;
    }

    public IHttpClientSender WithHeaders(object headers, bool replaceUnderscoreWithHyphen = true)
    {
        if (headers == null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        // underscore replacement only applies when object properties are parsed to kv pairs
        replaceUnderscoreWithHyphen = replaceUnderscoreWithHyphen && headers is not string && headers is not IEnumerable;

        foreach (var res in KeyValueHelper.ParseKeyValuePairs(headers))
        {
            string replaceKey = replaceUnderscoreWithHyphen ? res.Key.Replace("_", "-", StringComparison.Ordinal) : res.Key;
            _request.Headers.Add(replaceKey, res.Value?.ToString());
        }

        return this;
    }

    public IHttpClientSender WithHeaders(HttpHeadersManager httpHeadersManager)
    {
        foreach (var item in httpHeadersManager.Headers)
        {
            _request.Headers.Add(item.Key, item.Value);
        }

        return this;
    }

    public async Task<HttpResult> SendAsync(CancellationToken cancellationToken = default)
    {
        TimeSpan duration = TimeSpan.Zero;

        try
        {
            HttpClient client = _httpClient ?? _defaultHttpClient;

            if (_isDefaultLog)
            {
                Log.Information($"--->_request Info: \n{_request}\n---> End", _request.ToString());
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            HttpResult httpResult = _mapper.Map<HttpResult>(response);
            httpResult.Duration = stopwatch.Elapsed;

            if (_isDefaultLog)
            {
                Log.Information($"---> Response Info: \n{httpResult}\n---> End", httpResult.ToString());
            }

            return httpResult;
        }
        catch (Exception ex)
        {
            return new(duration, ex, _request);
        }
        // Đảm bảo _request thành một đối tượng HttpRequestMessage để chuẩn bị cho yêu cầu tiếp theo
        finally
        {
            _request = new();
        }
    }
}