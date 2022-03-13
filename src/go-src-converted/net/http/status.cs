// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2022 March 13 05:37:35 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\status.go
namespace go.net;

public static partial class http_package {

// HTTP status codes as registered with IANA.
// See: https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
public static readonly nint StatusContinue = 100; // RFC 7231, 6.2.1
public static readonly nint StatusSwitchingProtocols = 101; // RFC 7231, 6.2.2
public static readonly nint StatusProcessing = 102; // RFC 2518, 10.1
public static readonly nint StatusEarlyHints = 103; // RFC 8297

public static readonly nint StatusOK = 200; // RFC 7231, 6.3.1
public static readonly nint StatusCreated = 201; // RFC 7231, 6.3.2
public static readonly nint StatusAccepted = 202; // RFC 7231, 6.3.3
public static readonly nint StatusNonAuthoritativeInfo = 203; // RFC 7231, 6.3.4
public static readonly nint StatusNoContent = 204; // RFC 7231, 6.3.5
public static readonly nint StatusResetContent = 205; // RFC 7231, 6.3.6
public static readonly nint StatusPartialContent = 206; // RFC 7233, 4.1
public static readonly nint StatusMultiStatus = 207; // RFC 4918, 11.1
public static readonly nint StatusAlreadyReported = 208; // RFC 5842, 7.1
public static readonly nint StatusIMUsed = 226; // RFC 3229, 10.4.1

public static readonly nint StatusMultipleChoices = 300; // RFC 7231, 6.4.1
public static readonly nint StatusMovedPermanently = 301; // RFC 7231, 6.4.2
public static readonly nint StatusFound = 302; // RFC 7231, 6.4.3
public static readonly nint StatusSeeOther = 303; // RFC 7231, 6.4.4
public static readonly nint StatusNotModified = 304; // RFC 7232, 4.1
public static readonly nint StatusUseProxy = 305; // RFC 7231, 6.4.5
private static readonly nint _ = 306; // RFC 7231, 6.4.6 (Unused)
public static readonly nint StatusTemporaryRedirect = 307; // RFC 7231, 6.4.7
public static readonly nint StatusPermanentRedirect = 308; // RFC 7538, 3

public static readonly nint StatusBadRequest = 400; // RFC 7231, 6.5.1
public static readonly nint StatusUnauthorized = 401; // RFC 7235, 3.1
public static readonly nint StatusPaymentRequired = 402; // RFC 7231, 6.5.2
public static readonly nint StatusForbidden = 403; // RFC 7231, 6.5.3
public static readonly nint StatusNotFound = 404; // RFC 7231, 6.5.4
public static readonly nint StatusMethodNotAllowed = 405; // RFC 7231, 6.5.5
public static readonly nint StatusNotAcceptable = 406; // RFC 7231, 6.5.6
public static readonly nint StatusProxyAuthRequired = 407; // RFC 7235, 3.2
public static readonly nint StatusRequestTimeout = 408; // RFC 7231, 6.5.7
public static readonly nint StatusConflict = 409; // RFC 7231, 6.5.8
public static readonly nint StatusGone = 410; // RFC 7231, 6.5.9
public static readonly nint StatusLengthRequired = 411; // RFC 7231, 6.5.10
public static readonly nint StatusPreconditionFailed = 412; // RFC 7232, 4.2
public static readonly nint StatusRequestEntityTooLarge = 413; // RFC 7231, 6.5.11
public static readonly nint StatusRequestURITooLong = 414; // RFC 7231, 6.5.12
public static readonly nint StatusUnsupportedMediaType = 415; // RFC 7231, 6.5.13
public static readonly nint StatusRequestedRangeNotSatisfiable = 416; // RFC 7233, 4.4
public static readonly nint StatusExpectationFailed = 417; // RFC 7231, 6.5.14
public static readonly nint StatusTeapot = 418; // RFC 7168, 2.3.3
public static readonly nint StatusMisdirectedRequest = 421; // RFC 7540, 9.1.2
public static readonly nint StatusUnprocessableEntity = 422; // RFC 4918, 11.2
public static readonly nint StatusLocked = 423; // RFC 4918, 11.3
public static readonly nint StatusFailedDependency = 424; // RFC 4918, 11.4
public static readonly nint StatusTooEarly = 425; // RFC 8470, 5.2.
public static readonly nint StatusUpgradeRequired = 426; // RFC 7231, 6.5.15
public static readonly nint StatusPreconditionRequired = 428; // RFC 6585, 3
public static readonly nint StatusTooManyRequests = 429; // RFC 6585, 4
public static readonly nint StatusRequestHeaderFieldsTooLarge = 431; // RFC 6585, 5
public static readonly nint StatusUnavailableForLegalReasons = 451; // RFC 7725, 3

public static readonly nint StatusInternalServerError = 500; // RFC 7231, 6.6.1
public static readonly nint StatusNotImplemented = 501; // RFC 7231, 6.6.2
public static readonly nint StatusBadGateway = 502; // RFC 7231, 6.6.3
public static readonly nint StatusServiceUnavailable = 503; // RFC 7231, 6.6.4
public static readonly nint StatusGatewayTimeout = 504; // RFC 7231, 6.6.5
public static readonly nint StatusHTTPVersionNotSupported = 505; // RFC 7231, 6.6.6
public static readonly nint StatusVariantAlsoNegotiates = 506; // RFC 2295, 8.1
public static readonly nint StatusInsufficientStorage = 507; // RFC 4918, 11.5
public static readonly nint StatusLoopDetected = 508; // RFC 5842, 7.2
public static readonly nint StatusNotExtended = 510; // RFC 2774, 7
public static readonly nint StatusNetworkAuthenticationRequired = 511; // RFC 6585, 6

private static map statusText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, @string>{StatusContinue:"Continue",StatusSwitchingProtocols:"Switching Protocols",StatusProcessing:"Processing",StatusEarlyHints:"Early Hints",StatusOK:"OK",StatusCreated:"Created",StatusAccepted:"Accepted",StatusNonAuthoritativeInfo:"Non-Authoritative Information",StatusNoContent:"No Content",StatusResetContent:"Reset Content",StatusPartialContent:"Partial Content",StatusMultiStatus:"Multi-Status",StatusAlreadyReported:"Already Reported",StatusIMUsed:"IM Used",StatusMultipleChoices:"Multiple Choices",StatusMovedPermanently:"Moved Permanently",StatusFound:"Found",StatusSeeOther:"See Other",StatusNotModified:"Not Modified",StatusUseProxy:"Use Proxy",StatusTemporaryRedirect:"Temporary Redirect",StatusPermanentRedirect:"Permanent Redirect",StatusBadRequest:"Bad Request",StatusUnauthorized:"Unauthorized",StatusPaymentRequired:"Payment Required",StatusForbidden:"Forbidden",StatusNotFound:"Not Found",StatusMethodNotAllowed:"Method Not Allowed",StatusNotAcceptable:"Not Acceptable",StatusProxyAuthRequired:"Proxy Authentication Required",StatusRequestTimeout:"Request Timeout",StatusConflict:"Conflict",StatusGone:"Gone",StatusLengthRequired:"Length Required",StatusPreconditionFailed:"Precondition Failed",StatusRequestEntityTooLarge:"Request Entity Too Large",StatusRequestURITooLong:"Request URI Too Long",StatusUnsupportedMediaType:"Unsupported Media Type",StatusRequestedRangeNotSatisfiable:"Requested Range Not Satisfiable",StatusExpectationFailed:"Expectation Failed",StatusTeapot:"I'm a teapot",StatusMisdirectedRequest:"Misdirected Request",StatusUnprocessableEntity:"Unprocessable Entity",StatusLocked:"Locked",StatusFailedDependency:"Failed Dependency",StatusTooEarly:"Too Early",StatusUpgradeRequired:"Upgrade Required",StatusPreconditionRequired:"Precondition Required",StatusTooManyRequests:"Too Many Requests",StatusRequestHeaderFieldsTooLarge:"Request Header Fields Too Large",StatusUnavailableForLegalReasons:"Unavailable For Legal Reasons",StatusInternalServerError:"Internal Server Error",StatusNotImplemented:"Not Implemented",StatusBadGateway:"Bad Gateway",StatusServiceUnavailable:"Service Unavailable",StatusGatewayTimeout:"Gateway Timeout",StatusHTTPVersionNotSupported:"HTTP Version Not Supported",StatusVariantAlsoNegotiates:"Variant Also Negotiates",StatusInsufficientStorage:"Insufficient Storage",StatusLoopDetected:"Loop Detected",StatusNotExtended:"Not Extended",StatusNetworkAuthenticationRequired:"Network Authentication Required",};

// StatusText returns a text for the HTTP status code. It returns the empty
// string if the code is unknown.
public static @string StatusText(nint code) {
    return statusText[code];
}

} // end http_package
