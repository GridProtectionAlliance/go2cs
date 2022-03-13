// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// Read system port mappings from /etc/services

// package net -- go2cs converted at 2022 March 13 05:30:04 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\port_unix.go
namespace go;

using bytealg = @internal.bytealg_package;
using sync = sync_package;

public static partial class net_package {

private static sync.Once onceReadServices = default;

private static void readServices() => func((defer, _, _) => {
    var (file, err) = open("/etc/services");
    if (err != null) {
        return ;
    }
    defer(file.close());

    {
        var (line, ok) = file.readLine();

        while (ok) { 
            // "http 80/tcp www www-http # World Wide Web HTTP"
            {
                var i__prev1 = i;

                var i = bytealg.IndexByteString(line, '#');

                if (i >= 0) {
                    line = line[..(int)i];
            line, ok = file.readLine();
                }

                i = i__prev1;

            }
            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }
            var portnet = f[1]; // "80/tcp"
            var (port, j, ok) = dtoi(portnet);
            if (!ok || port <= 0 || j >= len(portnet) || portnet[j] != '/') {
                continue;
            }
            var netw = portnet[(int)j + 1..]; // "tcp"
            var (m, ok1) = services[netw];
            if (!ok1) {
                m = make_map<@string, nint>();
                services[netw] = m;
            }
            {
                var i__prev2 = i;

                for (i = 0; i < len(f); i++) {
                    if (i != 1) { // f[1] was port/net
                        m[f[i]] = port;
                    }
                }


                i = i__prev2;
            }
        }
    }
});

// goLookupPort is the native Go implementation of LookupPort.
private static (nint, error) goLookupPort(@string network, @string service) {
    nint port = default;
    error err = default!;

    onceReadServices.Do(readServices);
    return lookupPortMap(network, service);
}

} // end net_package
