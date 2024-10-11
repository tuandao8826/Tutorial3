using System;
using System.Collections.Generic;
using Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces;

namespace Tutorial.Infrastructure.Facades.Common.HttpClients
{
    public class HttpHeadersManager : IHttpHeadersManager
    {
        private Dictionary<string, string> _headers;

        public HttpHeadersManager()
        {
            _headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get { return _headers; } }

        // Phương thức thêm header chung
        public HttpHeadersManager AddHeader(string key, string value)
        {
            if (!_headers.ContainsKey(key))
            {
                _headers.Add(key, value);
            }
            else
            {
                _headers[key] = value;  // Cập nhật nếu đã tồn tại
            }

            return this;
        }

        // Thêm Authorization
        public HttpHeadersManager AddAuthorization(string token)
        {
            return AddHeader("Authorization", $"Bearer {token}");
        }

        // Thêm User-Agent
        public HttpHeadersManager AddUserAgent(string name)
        {
            return AddHeader("User-Agent", name);
        }

        // Thêm Accept
        public HttpHeadersManager AddAccept(string mediatype)
        {
            return AddHeader("Accept", mediatype);
        }

        // Thêm Accept-Encoding
        public HttpHeadersManager AddAcceptEncoding(string encoding)
        {
            return AddHeader("Accept-Encoding", encoding);
        }

        // Thêm Accept-Language
        public HttpHeadersManager AddAcceptLanguage(string language)
        {
            return AddHeader("Accept-Language", language);
        }

        // Thêm Cookie
        public HttpHeadersManager AddCookie(string cookie)
        {
            return AddHeader("Cookie", cookie);
        }

        // Thêm Referer
        public HttpHeadersManager AddReferer(string referer)
        {
            return AddHeader("Referer", referer);
        }

        // Thêm các Sec-* headers
        public HttpHeadersManager AddSecChUa(string secChUa)
        {
            return AddHeader("Sec-Ch-Ua", secChUa);
        }

        public HttpHeadersManager AddSecChUaMobile(string secChUaMobile)
        {
            return AddHeader("Sec-Ch-Ua-Mobile", secChUaMobile);
        }

        public HttpHeadersManager AddSecChUaPlatform(string secChUaPlatform)
        {
            return AddHeader("Sec-Ch-Ua-Platform", secChUaPlatform);
        }

        public HttpHeadersManager AddSecFetchDest(string secFetchDest)
        {
            return AddHeader("Sec-Fetch-Dest", secFetchDest);
        }

        public HttpHeadersManager AddSecFetchMode(string secFetchMode)
        {
            return AddHeader("Sec-Fetch-Mode", secFetchMode);
        }

        public HttpHeadersManager AddSecFetchSite(string secFetchSite)
        {
            return AddHeader("Sec-Fetch-Site", secFetchSite);
        }

        // Nếu bạn muốn thêm If-Modified-Since và If-None-Match
        public HttpHeadersManager AddIfModifiedSince(string date)
        {
            return AddHeader("If-Modified-Since", date);
        }

        public HttpHeadersManager AddIfNoneMatch(string etag)
        {
            return AddHeader("If-None-Match", etag);
        }

        // Hàm tiện ích để lấy các header đã thêm
        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }
    }
}
