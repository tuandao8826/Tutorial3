using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.Domain.Model.Options;
using Tutorial.Infrastructure.Services.Interfaces;

namespace Tutorial.Infrastructure.Services
{
    public class PollyOptionsService : IPollyOptionsService
    {
        private readonly PollyOptions _pollyOptions;

        public PollyOptionsService(IOptions<PollyOptions> options)
        {
            this._pollyOptions = options.Value;
        }

        public int GetRetryCount()
        {
            return _pollyOptions.RetryCount;
        }

        public int GetTimeWaitAfterFailed()
        {
            return _pollyOptions.TimeWaitAfterFailed;
        }
    }
}
