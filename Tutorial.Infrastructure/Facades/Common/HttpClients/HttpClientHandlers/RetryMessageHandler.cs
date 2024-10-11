using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.Domain.Model.Options;
using Tutorial.Infrastructure.Services;
using Tutorial.Infrastructure.Services.Interfaces;

namespace Tutorial.Infrastructure.Facades.Common.HttpClients.HttpClientHandlers
{
    public class RetryMessageHandler : DelegatingHandler
    {
        private readonly PollyOptions _pollyOptions;

        public RetryMessageHandler(HttpMessageHandler innerHandler, IOptions<PollyOptions> options) : base(innerHandler)
        {
            this._pollyOptions = options.Value;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Policy
                .Handle<HttpRequestException>()  // Xử lý HttpRequestException
                .Or<TaskCanceledException>()     // Xử lý TaskCanceledException do timeout
                .OrResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode) // Retry nếu mã phản hồi không thành công
                .WaitAndRetryAsync(
                        _pollyOptions.RetryCount, // Số lần retry
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(_pollyOptions.TimeWaitAfterFailed, retryAttempt)) // thời gian chờ giữa các lần retry theo luỹ thừa số lần retry
                )
                .ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}
