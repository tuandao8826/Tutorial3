using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces
{
    public interface IHttpHeadersManager
    {
        HttpHeadersManager AddHeader(string key, string value);
        HttpHeadersManager AddAuthorization(string token);
        HttpHeadersManager AddUserAgent(string name);
        HttpHeadersManager AddAccept(string mediatype);
    }
}
