// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:30 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\port.go


namespace go;

public static partial class net_package {

    // parsePort parses service as a decimal integer and returns the
    // corresponding value as port. It is the caller's responsibility to
    // parse service as a non-decimal integer when needsLookup is true.
    //
    // Some system resolvers will return a valid port number when given a number
    // over 65536 (see https://golang.org/issues/11715). Alas, the parser
    // can't bail early on numbers > 65536. Therefore reasonably large/small
    // numbers are parsed in full and rejected if invalid.
private static (nint, bool) parsePort(@string service) {
    nint port = default;
    bool needsLookup = default;

    if (service == "") { 
        // Lock in the legacy behavior that an empty string
        // means port 0. See golang.org/issue/13610.
        return (0, false);

    }
    const var max = uint32(1 << 32 - 1);
    const var cutoff = uint32(1 << 30);

    var neg = false;
    if (service[0] == '+') {
        service = service[(int)1..];
    }
    else if (service[0] == '-') {
        neg = true;
        service = service[(int)1..];
    }
    uint n = default;
    foreach (var (_, d) in service) {
        if ('0' <= d && d <= '9') {
            d -= '0';
        }
        else
 {
            return (0, true);
        }
        if (n >= cutoff) {
            n = max;
            break;
        }
        n *= 10;
        var nn = n + uint32(d);
        if (nn < n || nn > max) {
            n = max;
            break;
        }
        n = nn;

    }    if (!neg && n >= cutoff) {
        port = int(cutoff - 1);
    }
    else if (neg && n > cutoff) {
        port = int(cutoff);
    }
    else
 {
        port = int(n);
    }
    if (neg) {
        port = -port;
    }
    return (port, false);

}

} // end net_package
