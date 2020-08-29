// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 August 29 08:33:45 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\status.go

using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // HTTP status codes as registered with IANA.
        // See: http://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
        public static readonly long StatusContinue = 100L; // RFC 7231, 6.2.1
        public static readonly long StatusSwitchingProtocols = 101L; // RFC 7231, 6.2.2
        public static readonly long StatusProcessing = 102L; // RFC 2518, 10.1

        public static readonly long StatusOK = 200L; // RFC 7231, 6.3.1
        public static readonly long StatusCreated = 201L; // RFC 7231, 6.3.2
        public static readonly long StatusAccepted = 202L; // RFC 7231, 6.3.3
        public static readonly long StatusNonAuthoritativeInfo = 203L; // RFC 7231, 6.3.4
        public static readonly long StatusNoContent = 204L; // RFC 7231, 6.3.5
        public static readonly long StatusResetContent = 205L; // RFC 7231, 6.3.6
        public static readonly long StatusPartialContent = 206L; // RFC 7233, 4.1
        public static readonly long StatusMultiStatus = 207L; // RFC 4918, 11.1
        public static readonly long StatusAlreadyReported = 208L; // RFC 5842, 7.1
        public static readonly long StatusIMUsed = 226L; // RFC 3229, 10.4.1

        public static readonly long StatusMultipleChoices = 300L; // RFC 7231, 6.4.1
        public static readonly long StatusMovedPermanently = 301L; // RFC 7231, 6.4.2
        public static readonly long StatusFound = 302L; // RFC 7231, 6.4.3
        public static readonly long StatusSeeOther = 303L; // RFC 7231, 6.4.4
        public static readonly long StatusNotModified = 304L; // RFC 7232, 4.1
        public static readonly long StatusUseProxy = 305L; // RFC 7231, 6.4.5
        private static readonly long _ = 306L; // RFC 7231, 6.4.6 (Unused)
        public static readonly long StatusTemporaryRedirect = 307L; // RFC 7231, 6.4.7
        public static readonly long StatusPermanentRedirect = 308L; // RFC 7538, 3

        public static readonly long StatusBadRequest = 400L; // RFC 7231, 6.5.1
        public static readonly long StatusUnauthorized = 401L; // RFC 7235, 3.1
        public static readonly long StatusPaymentRequired = 402L; // RFC 7231, 6.5.2
        public static readonly long StatusForbidden = 403L; // RFC 7231, 6.5.3
        public static readonly long StatusNotFound = 404L; // RFC 7231, 6.5.4
        public static readonly long StatusMethodNotAllowed = 405L; // RFC 7231, 6.5.5
        public static readonly long StatusNotAcceptable = 406L; // RFC 7231, 6.5.6
        public static readonly long StatusProxyAuthRequired = 407L; // RFC 7235, 3.2
        public static readonly long StatusRequestTimeout = 408L; // RFC 7231, 6.5.7
        public static readonly long StatusConflict = 409L; // RFC 7231, 6.5.8
        public static readonly long StatusGone = 410L; // RFC 7231, 6.5.9
        public static readonly long StatusLengthRequired = 411L; // RFC 7231, 6.5.10
        public static readonly long StatusPreconditionFailed = 412L; // RFC 7232, 4.2
        public static readonly long StatusRequestEntityTooLarge = 413L; // RFC 7231, 6.5.11
        public static readonly long StatusRequestURITooLong = 414L; // RFC 7231, 6.5.12
        public static readonly long StatusUnsupportedMediaType = 415L; // RFC 7231, 6.5.13
        public static readonly long StatusRequestedRangeNotSatisfiable = 416L; // RFC 7233, 4.4
        public static readonly long StatusExpectationFailed = 417L; // RFC 7231, 6.5.14
        public static readonly long StatusTeapot = 418L; // RFC 7168, 2.3.3
        public static readonly long StatusUnprocessableEntity = 422L; // RFC 4918, 11.2
        public static readonly long StatusLocked = 423L; // RFC 4918, 11.3
        public static readonly long StatusFailedDependency = 424L; // RFC 4918, 11.4
        public static readonly long StatusUpgradeRequired = 426L; // RFC 7231, 6.5.15
        public static readonly long StatusPreconditionRequired = 428L; // RFC 6585, 3
        public static readonly long StatusTooManyRequests = 429L; // RFC 6585, 4
        public static readonly long StatusRequestHeaderFieldsTooLarge = 431L; // RFC 6585, 5
        public static readonly long StatusUnavailableForLegalReasons = 451L; // RFC 7725, 3

        public static readonly long StatusInternalServerError = 500L; // RFC 7231, 6.6.1
        public static readonly long StatusNotImplemented = 501L; // RFC 7231, 6.6.2
        public static readonly long StatusBadGateway = 502L; // RFC 7231, 6.6.3
        public static readonly long StatusServiceUnavailable = 503L; // RFC 7231, 6.6.4
        public static readonly long StatusGatewayTimeout = 504L; // RFC 7231, 6.6.5
        public static readonly long StatusHTTPVersionNotSupported = 505L; // RFC 7231, 6.6.6
        public static readonly long StatusVariantAlsoNegotiates = 506L; // RFC 2295, 8.1
        public static readonly long StatusInsufficientStorage = 507L; // RFC 4918, 11.5
        public static readonly long StatusLoopDetected = 508L; // RFC 5842, 7.2
        public static readonly long StatusNotExtended = 510L; // RFC 2774, 7
        public static readonly long StatusNetworkAuthenticationRequired = 511L; // RFC 6585, 6

        private static map statusText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, @string>{StatusContinue:"Continue",StatusSwitchingProtocols:"Switching Protocols",StatusProcessing:"Processing",StatusOK:"OK",StatusCreated:"Created",StatusAccepted:"Accepted",StatusNonAuthoritativeInfo:"Non-Authoritative Information",StatusNoContent:"No Content",StatusResetContent:"Reset Content",StatusPartialContent:"Partial Content",StatusMultiStatus:"Multi-Status",StatusAlreadyReported:"Already Reported",StatusIMUsed:"IM Used",StatusMultipleChoices:"Multiple Choices",StatusMovedPermanently:"Moved Permanently",StatusFound:"Found",StatusSeeOther:"See Other",StatusNotModified:"Not Modified",StatusUseProxy:"Use Proxy",StatusTemporaryRedirect:"Temporary Redirect",StatusPermanentRedirect:"Permanent Redirect",StatusBadRequest:"Bad Request",StatusUnauthorized:"Unauthorized",StatusPaymentRequired:"Payment Required",StatusForbidden:"Forbidden",StatusNotFound:"Not Found",StatusMethodNotAllowed:"Method Not Allowed",StatusNotAcceptable:"Not Acceptable",StatusProxyAuthRequired:"Proxy Authentication Required",StatusRequestTimeout:"Request Timeout",StatusConflict:"Conflict",StatusGone:"Gone",StatusLengthRequired:"Length Required",StatusPreconditionFailed:"Precondition Failed",StatusRequestEntityTooLarge:"Request Entity Too Large",StatusRequestURITooLong:"Request URI Too Long",StatusUnsupportedMediaType:"Unsupported Media Type",StatusRequestedRangeNotSatisfiable:"Requested Range Not Satisfiable",StatusExpectationFailed:"Expectation Failed",StatusTeapot:"I'm a teapot",StatusUnprocessableEntity:"Unprocessable Entity",StatusLocked:"Locked",StatusFailedDependency:"Failed Dependency",StatusUpgradeRequired:"Upgrade Required",StatusPreconditionRequired:"Precondition Required",StatusTooManyRequests:"Too Many Requests",StatusRequestHeaderFieldsTooLarge:"Request Header Fields Too Large",StatusUnavailableForLegalReasons:"Unavailable For Legal Reasons",StatusInternalServerError:"Internal Server Error",StatusNotImplemented:"Not Implemented",StatusBadGateway:"Bad Gateway",StatusServiceUnavailable:"Service Unavailable",StatusGatewayTimeout:"Gateway Timeout",StatusHTTPVersionNotSupported:"HTTP Version Not Supported",StatusVariantAlsoNegotiates:"Variant Also Negotiates",StatusInsufficientStorage:"Insufficient Storage",StatusLoopDetected:"Loop Detected",StatusNotExtended:"Not Extended",StatusNetworkAuthenticationRequired:"Network Authentication Required",};

        // StatusText returns a text for the HTTP status code. It returns the empty
        // string if the code is unknown.
        public static @string StatusText(long code)
        {
            return statusText[code];
        }
    }
}}
