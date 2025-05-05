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

internal static readonly UntypedInt _ = 306; // RFC 9110, 15.4.7 (Unused)

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
    switch (code) {
    case StatusContinue: {
        return "Continue"u8;
    }
    case StatusSwitchingProtocols: {
        return "Switching Protocols"u8;
    }
    case StatusProcessing: {
        return "Processing"u8;
    }
    case StatusEarlyHints: {
        return "Early Hints"u8;
    }
    case StatusOK: {
        return "OK"u8;
    }
    case StatusCreated: {
        return "Created"u8;
    }
    case StatusAccepted: {
        return "Accepted"u8;
    }
    case StatusNonAuthoritativeInfo: {
        return "Non-Authoritative Information"u8;
    }
    case StatusNoContent: {
        return "No Content"u8;
    }
    case StatusResetContent: {
        return "Reset Content"u8;
    }
    case StatusPartialContent: {
        return "Partial Content"u8;
    }
    case StatusMultiStatus: {
        return "Multi-Status"u8;
    }
    case StatusAlreadyReported: {
        return "Already Reported"u8;
    }
    case StatusIMUsed: {
        return "IM Used"u8;
    }
    case StatusMultipleChoices: {
        return "Multiple Choices"u8;
    }
    case StatusMovedPermanently: {
        return "Moved Permanently"u8;
    }
    case StatusFound: {
        return "Found"u8;
    }
    case StatusSeeOther: {
        return "See Other"u8;
    }
    case StatusNotModified: {
        return "Not Modified"u8;
    }
    case StatusUseProxy: {
        return "Use Proxy"u8;
    }
    case StatusTemporaryRedirect: {
        return "Temporary Redirect"u8;
    }
    case StatusPermanentRedirect: {
        return "Permanent Redirect"u8;
    }
    case StatusBadRequest: {
        return "Bad Request"u8;
    }
    case StatusUnauthorized: {
        return "Unauthorized"u8;
    }
    case StatusPaymentRequired: {
        return "Payment Required"u8;
    }
    case StatusForbidden: {
        return "Forbidden"u8;
    }
    case StatusNotFound: {
        return "Not Found"u8;
    }
    case StatusMethodNotAllowed: {
        return "Method Not Allowed"u8;
    }
    case StatusNotAcceptable: {
        return "Not Acceptable"u8;
    }
    case StatusProxyAuthRequired: {
        return "Proxy Authentication Required"u8;
    }
    case StatusRequestTimeout: {
        return "Request Timeout"u8;
    }
    case StatusConflict: {
        return "Conflict"u8;
    }
    case StatusGone: {
        return "Gone"u8;
    }
    case StatusLengthRequired: {
        return "Length Required"u8;
    }
    case StatusPreconditionFailed: {
        return "Precondition Failed"u8;
    }
    case StatusRequestEntityTooLarge: {
        return "Request Entity Too Large"u8;
    }
    case StatusRequestURITooLong: {
        return "Request URI Too Long"u8;
    }
    case StatusUnsupportedMediaType: {
        return "Unsupported Media Type"u8;
    }
    case StatusRequestedRangeNotSatisfiable: {
        return "Requested Range Not Satisfiable"u8;
    }
    case StatusExpectationFailed: {
        return "Expectation Failed"u8;
    }
    case StatusTeapot: {
        return "I'm a teapot"u8;
    }
    case StatusMisdirectedRequest: {
        return "Misdirected Request"u8;
    }
    case StatusUnprocessableEntity: {
        return "Unprocessable Entity"u8;
    }
    case StatusLocked: {
        return "Locked"u8;
    }
    case StatusFailedDependency: {
        return "Failed Dependency"u8;
    }
    case StatusTooEarly: {
        return "Too Early"u8;
    }
    case StatusUpgradeRequired: {
        return "Upgrade Required"u8;
    }
    case StatusPreconditionRequired: {
        return "Precondition Required"u8;
    }
    case StatusTooManyRequests: {
        return "Too Many Requests"u8;
    }
    case StatusRequestHeaderFieldsTooLarge: {
        return "Request Header Fields Too Large"u8;
    }
    case StatusUnavailableForLegalReasons: {
        return "Unavailable For Legal Reasons"u8;
    }
    case StatusInternalServerError: {
        return "Internal Server Error"u8;
    }
    case StatusNotImplemented: {
        return "Not Implemented"u8;
    }
    case StatusBadGateway: {
        return "Bad Gateway"u8;
    }
    case StatusServiceUnavailable: {
        return "Service Unavailable"u8;
    }
    case StatusGatewayTimeout: {
        return "Gateway Timeout"u8;
    }
    case StatusHTTPVersionNotSupported: {
        return "HTTP Version Not Supported"u8;
    }
    case StatusVariantAlsoNegotiates: {
        return "Variant Also Negotiates"u8;
    }
    case StatusInsufficientStorage: {
        return "Insufficient Storage"u8;
    }
    case StatusLoopDetected: {
        return "Loop Detected"u8;
    }
    case StatusNotExtended: {
        return "Not Extended"u8;
    }
    case StatusNetworkAuthenticationRequired: {
        return "Network Authentication Required"u8;
    }
    default: {
        return ""u8;
    }}

}

} // end http_package
