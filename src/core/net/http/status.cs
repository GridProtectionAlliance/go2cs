// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class http_package {

// HTTP status codes as registered with IANA.
// See: https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
public static readonly UntypedInt StatusContinue = 100; // RFC 9110, 15.2.1

public static readonly UntypedInt StatusSwitchingProtocols = 101; // RFC 9110, 15.2.2

public static readonly UntypedInt StatusProcessing = 102; // RFC 2518, 10.1

public static readonly UntypedInt StatusEarlyHints = 103; // RFC 8297

public static readonly UntypedInt StatusOK = 200; // RFC 9110, 15.3.1

public static readonly UntypedInt StatusCreated = 201; // RFC 9110, 15.3.2

public static readonly UntypedInt StatusAccepted = 202; // RFC 9110, 15.3.3

public static readonly UntypedInt StatusNonAuthoritativeInfo = 203; // RFC 9110, 15.3.4

public static readonly UntypedInt StatusNoContent = 204; // RFC 9110, 15.3.5

public static readonly UntypedInt StatusResetContent = 205; // RFC 9110, 15.3.6

public static readonly UntypedInt StatusPartialContent = 206; // RFC 9110, 15.3.7

public static readonly UntypedInt StatusMultiStatus = 207; // RFC 4918, 11.1

public static readonly UntypedInt StatusAlreadyReported = 208; // RFC 5842, 7.1

public static readonly UntypedInt StatusIMUsed = 226; // RFC 3229, 10.4.1

public static readonly UntypedInt StatusMultipleChoices = 300; // RFC 9110, 15.4.1

public static readonly UntypedInt StatusMovedPermanently = 301; // RFC 9110, 15.4.2

public static readonly UntypedInt StatusFound = 302; // RFC 9110, 15.4.3

public static readonly UntypedInt StatusSeeOther = 303; // RFC 9110, 15.4.4

public static readonly UntypedInt StatusNotModified = 304; // RFC 9110, 15.4.5

public static readonly UntypedInt StatusUseProxy = 305; // RFC 9110, 15.4.6

internal static readonly UntypedInt _ᴛ1ʗ = 306; // RFC 9110, 15.4.7 (Unused)

public static readonly UntypedInt StatusTemporaryRedirect = 307; // RFC 9110, 15.4.8

public static readonly UntypedInt StatusPermanentRedirect = 308; // RFC 9110, 15.4.9

public static readonly UntypedInt StatusBadRequest = 400; // RFC 9110, 15.5.1

public static readonly UntypedInt StatusUnauthorized = 401; // RFC 9110, 15.5.2

public static readonly UntypedInt StatusPaymentRequired = 402; // RFC 9110, 15.5.3

public static readonly UntypedInt StatusForbidden = 403; // RFC 9110, 15.5.4

public static readonly UntypedInt StatusNotFound = 404; // RFC 9110, 15.5.5

public static readonly UntypedInt StatusMethodNotAllowed = 405; // RFC 9110, 15.5.6

public static readonly UntypedInt StatusNotAcceptable = 406; // RFC 9110, 15.5.7

public static readonly UntypedInt StatusProxyAuthRequired = 407; // RFC 9110, 15.5.8

public static readonly UntypedInt StatusRequestTimeout = 408; // RFC 9110, 15.5.9

public static readonly UntypedInt StatusConflict = 409; // RFC 9110, 15.5.10

public static readonly UntypedInt StatusGone = 410; // RFC 9110, 15.5.11

public static readonly UntypedInt StatusLengthRequired = 411; // RFC 9110, 15.5.12

public static readonly UntypedInt StatusPreconditionFailed = 412; // RFC 9110, 15.5.13

public static readonly UntypedInt StatusRequestEntityTooLarge = 413; // RFC 9110, 15.5.14

public static readonly UntypedInt StatusRequestURITooLong = 414; // RFC 9110, 15.5.15

public static readonly UntypedInt StatusUnsupportedMediaType = 415; // RFC 9110, 15.5.16

public static readonly UntypedInt StatusRequestedRangeNotSatisfiable = 416; // RFC 9110, 15.5.17

public static readonly UntypedInt StatusExpectationFailed = 417; // RFC 9110, 15.5.18

public static readonly UntypedInt StatusTeapot = 418; // RFC 9110, 15.5.19 (Unused)

public static readonly UntypedInt StatusMisdirectedRequest = 421; // RFC 9110, 15.5.20

public static readonly UntypedInt StatusUnprocessableEntity = 422; // RFC 9110, 15.5.21

public static readonly UntypedInt StatusLocked = 423; // RFC 4918, 11.3

public static readonly UntypedInt StatusFailedDependency = 424; // RFC 4918, 11.4

public static readonly UntypedInt StatusTooEarly = 425; // RFC 8470, 5.2.

public static readonly UntypedInt StatusUpgradeRequired = 426; // RFC 9110, 15.5.22

public static readonly UntypedInt StatusPreconditionRequired = 428; // RFC 6585, 3

public static readonly UntypedInt StatusTooManyRequests = 429; // RFC 6585, 4

public static readonly UntypedInt StatusRequestHeaderFieldsTooLarge = 431; // RFC 6585, 5

public static readonly UntypedInt StatusUnavailableForLegalReasons = 451; // RFC 7725, 3

public static readonly UntypedInt StatusInternalServerError = 500; // RFC 9110, 15.6.1

public static readonly UntypedInt StatusNotImplemented = 501; // RFC 9110, 15.6.2

public static readonly UntypedInt StatusBadGateway = 502; // RFC 9110, 15.6.3

public static readonly UntypedInt StatusServiceUnavailable = 503; // RFC 9110, 15.6.4

public static readonly UntypedInt StatusGatewayTimeout = 504; // RFC 9110, 15.6.5

public static readonly UntypedInt StatusHTTPVersionNotSupported = 505; // RFC 9110, 15.6.6

public static readonly UntypedInt StatusVariantAlsoNegotiates = 506; // RFC 2295, 8.1

public static readonly UntypedInt StatusInsufficientStorage = 507; // RFC 4918, 11.5

public static readonly UntypedInt StatusLoopDetected = 508; // RFC 5842, 7.2

public static readonly UntypedInt StatusNotExtended = 510; // RFC 2774, 7

public static readonly UntypedInt StatusNetworkAuthenticationRequired = 511; // RFC 6585, 6

// StatusText returns a text for the HTTP status code. It returns the empty
// string if the code is unknown.
public static @string StatusText(nint code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == StatusContinue) {
        return "Continue"u8;
    }
    if (exprᴛ1 == StatusSwitchingProtocols) {
        return "Switching Protocols"u8;
    }
    if (exprᴛ1 == StatusProcessing) {
        return "Processing"u8;
    }
    if (exprᴛ1 == StatusEarlyHints) {
        return "Early Hints"u8;
    }
    if (exprᴛ1 == StatusOK) {
        return "OK"u8;
    }
    if (exprᴛ1 == StatusCreated) {
        return "Created"u8;
    }
    if (exprᴛ1 == StatusAccepted) {
        return "Accepted"u8;
    }
    if (exprᴛ1 == StatusNonAuthoritativeInfo) {
        return "Non-Authoritative Information"u8;
    }
    if (exprᴛ1 == StatusNoContent) {
        return "No Content"u8;
    }
    if (exprᴛ1 == StatusResetContent) {
        return "Reset Content"u8;
    }
    if (exprᴛ1 == StatusPartialContent) {
        return "Partial Content"u8;
    }
    if (exprᴛ1 == StatusMultiStatus) {
        return "Multi-Status"u8;
    }
    if (exprᴛ1 == StatusAlreadyReported) {
        return "Already Reported"u8;
    }
    if (exprᴛ1 == StatusIMUsed) {
        return "IM Used"u8;
    }
    if (exprᴛ1 == StatusMultipleChoices) {
        return "Multiple Choices"u8;
    }
    if (exprᴛ1 == StatusMovedPermanently) {
        return "Moved Permanently"u8;
    }
    if (exprᴛ1 == StatusFound) {
        return "Found"u8;
    }
    if (exprᴛ1 == StatusSeeOther) {
        return "See Other"u8;
    }
    if (exprᴛ1 == StatusNotModified) {
        return "Not Modified"u8;
    }
    if (exprᴛ1 == StatusUseProxy) {
        return "Use Proxy"u8;
    }
    if (exprᴛ1 == StatusTemporaryRedirect) {
        return "Temporary Redirect"u8;
    }
    if (exprᴛ1 == StatusPermanentRedirect) {
        return "Permanent Redirect"u8;
    }
    if (exprᴛ1 == StatusBadRequest) {
        return "Bad Request"u8;
    }
    if (exprᴛ1 == StatusUnauthorized) {
        return "Unauthorized"u8;
    }
    if (exprᴛ1 == StatusPaymentRequired) {
        return "Payment Required"u8;
    }
    if (exprᴛ1 == StatusForbidden) {
        return "Forbidden"u8;
    }
    if (exprᴛ1 == StatusNotFound) {
        return "Not Found"u8;
    }
    if (exprᴛ1 == StatusMethodNotAllowed) {
        return "Method Not Allowed"u8;
    }
    if (exprᴛ1 == StatusNotAcceptable) {
        return "Not Acceptable"u8;
    }
    if (exprᴛ1 == StatusProxyAuthRequired) {
        return "Proxy Authentication Required"u8;
    }
    if (exprᴛ1 == StatusRequestTimeout) {
        return "Request Timeout"u8;
    }
    if (exprᴛ1 == StatusConflict) {
        return "Conflict"u8;
    }
    if (exprᴛ1 == StatusGone) {
        return "Gone"u8;
    }
    if (exprᴛ1 == StatusLengthRequired) {
        return "Length Required"u8;
    }
    if (exprᴛ1 == StatusPreconditionFailed) {
        return "Precondition Failed"u8;
    }
    if (exprᴛ1 == StatusRequestEntityTooLarge) {
        return "Request Entity Too Large"u8;
    }
    if (exprᴛ1 == StatusRequestURITooLong) {
        return "Request URI Too Long"u8;
    }
    if (exprᴛ1 == StatusUnsupportedMediaType) {
        return "Unsupported Media Type"u8;
    }
    if (exprᴛ1 == StatusRequestedRangeNotSatisfiable) {
        return "Requested Range Not Satisfiable"u8;
    }
    if (exprᴛ1 == StatusExpectationFailed) {
        return "Expectation Failed"u8;
    }
    if (exprᴛ1 == StatusTeapot) {
        return "I'm a teapot"u8;
    }
    if (exprᴛ1 == StatusMisdirectedRequest) {
        return "Misdirected Request"u8;
    }
    if (exprᴛ1 == StatusUnprocessableEntity) {
        return "Unprocessable Entity"u8;
    }
    if (exprᴛ1 == StatusLocked) {
        return "Locked"u8;
    }
    if (exprᴛ1 == StatusFailedDependency) {
        return "Failed Dependency"u8;
    }
    if (exprᴛ1 == StatusTooEarly) {
        return "Too Early"u8;
    }
    if (exprᴛ1 == StatusUpgradeRequired) {
        return "Upgrade Required"u8;
    }
    if (exprᴛ1 == StatusPreconditionRequired) {
        return "Precondition Required"u8;
    }
    if (exprᴛ1 == StatusTooManyRequests) {
        return "Too Many Requests"u8;
    }
    if (exprᴛ1 == StatusRequestHeaderFieldsTooLarge) {
        return "Request Header Fields Too Large"u8;
    }
    if (exprᴛ1 == StatusUnavailableForLegalReasons) {
        return "Unavailable For Legal Reasons"u8;
    }
    if (exprᴛ1 == StatusInternalServerError) {
        return "Internal Server Error"u8;
    }
    if (exprᴛ1 == StatusNotImplemented) {
        return "Not Implemented"u8;
    }
    if (exprᴛ1 == StatusBadGateway) {
        return "Bad Gateway"u8;
    }
    if (exprᴛ1 == StatusServiceUnavailable) {
        return "Service Unavailable"u8;
    }
    if (exprᴛ1 == StatusGatewayTimeout) {
        return "Gateway Timeout"u8;
    }
    if (exprᴛ1 == StatusHTTPVersionNotSupported) {
        return "HTTP Version Not Supported"u8;
    }
    if (exprᴛ1 == StatusVariantAlsoNegotiates) {
        return "Variant Also Negotiates"u8;
    }
    if (exprᴛ1 == StatusInsufficientStorage) {
        return "Insufficient Storage"u8;
    }
    if (exprᴛ1 == StatusLoopDetected) {
        return "Loop Detected"u8;
    }
    if (exprᴛ1 == StatusNotExtended) {
        return "Not Extended"u8;
    }
    if (exprᴛ1 == StatusNetworkAuthenticationRequired) {
        return "Network Authentication Required"u8;
    }
    { /* default: */
        return ""u8;
    }

}

} // end http_package
