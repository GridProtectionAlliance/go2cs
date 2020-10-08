// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:40:35 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\status.go

using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // HTTP status codes as registered with IANA.
        // See: https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
        public static readonly long StatusContinue = (long)100L; // RFC 7231, 6.2.1
        public static readonly long StatusSwitchingProtocols = (long)101L; // RFC 7231, 6.2.2
        public static readonly long StatusProcessing = (long)102L; // RFC 2518, 10.1
        public static readonly long StatusEarlyHints = (long)103L; // RFC 8297

        public static readonly long StatusOK = (long)200L; // RFC 7231, 6.3.1
        public static readonly long StatusCreated = (long)201L; // RFC 7231, 6.3.2
        public static readonly long StatusAccepted = (long)202L; // RFC 7231, 6.3.3
        public static readonly long StatusNonAuthoritativeInfo = (long)203L; // RFC 7231, 6.3.4
        public static readonly long StatusNoContent = (long)204L; // RFC 7231, 6.3.5
        public static readonly long StatusResetContent = (long)205L; // RFC 7231, 6.3.6
        public static readonly long StatusPartialContent = (long)206L; // RFC 7233, 4.1
        public static readonly long StatusMultiStatus = (long)207L; // RFC 4918, 11.1
        public static readonly long StatusAlreadyReported = (long)208L; // RFC 5842, 7.1
        public static readonly long StatusIMUsed = (long)226L; // RFC 3229, 10.4.1

        public static readonly long StatusMultipleChoices = (long)300L; // RFC 7231, 6.4.1
        public static readonly long StatusMovedPermanently = (long)301L; // RFC 7231, 6.4.2
        public static readonly long StatusFound = (long)302L; // RFC 7231, 6.4.3
        public static readonly long StatusSeeOther = (long)303L; // RFC 7231, 6.4.4
        public static readonly long StatusNotModified = (long)304L; // RFC 7232, 4.1
        public static readonly long StatusUseProxy = (long)305L; // RFC 7231, 6.4.5
        private static readonly long _ = (long)306L; // RFC 7231, 6.4.6 (Unused)
        public static readonly long StatusTemporaryRedirect = (long)307L; // RFC 7231, 6.4.7
        public static readonly long StatusPermanentRedirect = (long)308L; // RFC 7538, 3

        public static readonly long StatusBadRequest = (long)400L; // RFC 7231, 6.5.1
        public static readonly long StatusUnauthorized = (long)401L; // RFC 7235, 3.1
        public static readonly long StatusPaymentRequired = (long)402L; // RFC 7231, 6.5.2
        public static readonly long StatusForbidden = (long)403L; // RFC 7231, 6.5.3
        public static readonly long StatusNotFound = (long)404L; // RFC 7231, 6.5.4
        public static readonly long StatusMethodNotAllowed = (long)405L; // RFC 7231, 6.5.5
        public static readonly long StatusNotAcceptable = (long)406L; // RFC 7231, 6.5.6
        public static readonly long StatusProxyAuthRequired = (long)407L; // RFC 7235, 3.2
        public static readonly long StatusRequestTimeout = (long)408L; // RFC 7231, 6.5.7
        public static readonly long StatusConflict = (long)409L; // RFC 7231, 6.5.8
        public static readonly long StatusGone = (long)410L; // RFC 7231, 6.5.9
        public static readonly long StatusLengthRequired = (long)411L; // RFC 7231, 6.5.10
        public static readonly long StatusPreconditionFailed = (long)412L; // RFC 7232, 4.2
        public static readonly long StatusRequestEntityTooLarge = (long)413L; // RFC 7231, 6.5.11
        public static readonly long StatusRequestURITooLong = (long)414L; // RFC 7231, 6.5.12
        public static readonly long StatusUnsupportedMediaType = (long)415L; // RFC 7231, 6.5.13
        public static readonly long StatusRequestedRangeNotSatisfiable = (long)416L; // RFC 7233, 4.4
        public static readonly long StatusExpectationFailed = (long)417L; // RFC 7231, 6.5.14
        public static readonly long StatusTeapot = (long)418L; // RFC 7168, 2.3.3
        public static readonly long StatusMisdirectedRequest = (long)421L; // RFC 7540, 9.1.2
        public static readonly long StatusUnprocessableEntity = (long)422L; // RFC 4918, 11.2
        public static readonly long StatusLocked = (long)423L; // RFC 4918, 11.3
        public static readonly long StatusFailedDependency = (long)424L; // RFC 4918, 11.4
        public static readonly long StatusTooEarly = (long)425L; // RFC 8470, 5.2.
        public static readonly long StatusUpgradeRequired = (long)426L; // RFC 7231, 6.5.15
        public static readonly long StatusPreconditionRequired = (long)428L; // RFC 6585, 3
        public static readonly long StatusTooManyRequests = (long)429L; // RFC 6585, 4
        public static readonly long StatusRequestHeaderFieldsTooLarge = (long)431L; // RFC 6585, 5
        public static readonly long StatusUnavailableForLegalReasons = (long)451L; // RFC 7725, 3

        public static readonly long StatusInternalServerError = (long)500L; // RFC 7231, 6.6.1
        public static readonly long StatusNotImplemented = (long)501L; // RFC 7231, 6.6.2
        public static readonly long StatusBadGateway = (long)502L; // RFC 7231, 6.6.3
        public static readonly long StatusServiceUnavailable = (long)503L; // RFC 7231, 6.6.4
        public static readonly long StatusGatewayTimeout = (long)504L; // RFC 7231, 6.6.5
        public static readonly long StatusHTTPVersionNotSupported = (long)505L; // RFC 7231, 6.6.6
        public static readonly long StatusVariantAlsoNegotiates = (long)506L; // RFC 2295, 8.1
        public static readonly long StatusInsufficientStorage = (long)507L; // RFC 4918, 11.5
        public static readonly long StatusLoopDetected = (long)508L; // RFC 5842, 7.2
        public static readonly long StatusNotExtended = (long)510L; // RFC 2774, 7
        public static readonly long StatusNetworkAuthenticationRequired = (long)511L; // RFC 6585, 6

        private static map statusText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, @string>{StatusContinue:"Continue",StatusSwitchingProtocols:"Switching Protocols",StatusProcessing:"Processing",StatusEarlyHints:"Early Hints",StatusOK:"OK",StatusCreated:"Created",StatusAccepted:"Accepted",StatusNonAuthoritativeInfo:"Non-Authoritative Information",StatusNoContent:"No Content",StatusResetContent:"Reset Content",StatusPartialContent:"Partial Content",StatusMultiStatus:"Multi-Status",StatusAlreadyReported:"Already Reported",StatusIMUsed:"IM Used",StatusMultipleChoices:"Multiple Choices",StatusMovedPermanently:"Moved Permanently",StatusFound:"Found",StatusSeeOther:"See Other",StatusNotModified:"Not Modified",StatusUseProxy:"Use Proxy",StatusTemporaryRedirect:"Temporary Redirect",StatusPermanentRedirect:"Permanent Redirect",StatusBadRequest:"Bad Request",StatusUnauthorized:"Unauthorized",StatusPaymentRequired:"Payment Required",StatusForbidden:"Forbidden",StatusNotFound:"Not Found",StatusMethodNotAllowed:"Method Not Allowed",StatusNotAcceptable:"Not Acceptable",StatusProxyAuthRequired:"Proxy Authentication Required",StatusRequestTimeout:"Request Timeout",StatusConflict:"Conflict",StatusGone:"Gone",StatusLengthRequired:"Length Required",StatusPreconditionFailed:"Precondition Failed",StatusRequestEntityTooLarge:"Request Entity Too Large",StatusRequestURITooLong:"Request URI Too Long",StatusUnsupportedMediaType:"Unsupported Media Type",StatusRequestedRangeNotSatisfiable:"Requested Range Not Satisfiable",StatusExpectationFailed:"Expectation Failed",StatusTeapot:"I'm a teapot",StatusMisdirectedRequest:"Misdirected Request",StatusUnprocessableEntity:"Unprocessable Entity",StatusLocked:"Locked",StatusFailedDependency:"Failed Dependency",StatusTooEarly:"Too Early",StatusUpgradeRequired:"Upgrade Required",StatusPreconditionRequired:"Precondition Required",StatusTooManyRequests:"Too Many Requests",StatusRequestHeaderFieldsTooLarge:"Request Header Fields Too Large",StatusUnavailableForLegalReasons:"Unavailable For Legal Reasons",StatusInternalServerError:"Internal Server Error",StatusNotImplemented:"Not Implemented",StatusBadGateway:"Bad Gateway",StatusServiceUnavailable:"Service Unavailable",StatusGatewayTimeout:"Gateway Timeout",StatusHTTPVersionNotSupported:"HTTP Version Not Supported",StatusVariantAlsoNegotiates:"Variant Also Negotiates",StatusInsufficientStorage:"Insufficient Storage",StatusLoopDetected:"Loop Detected",StatusNotExtended:"Not Extended",StatusNetworkAuthenticationRequired:"Network Authentication Required",};

        // StatusText returns a text for the HTTP status code. It returns the empty
        // string if the code is unknown.
        public static @string StatusText(long code)
        {
            return statusText[code];
        }
    }
}}
