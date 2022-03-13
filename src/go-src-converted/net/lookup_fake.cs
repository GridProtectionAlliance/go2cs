// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package net -- go2cs converted at 2022 March 13 05:29:56 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\lookup_fake.go
namespace go;

using context = context_package;
using syscall = syscall_package;

public static partial class net_package {

private static (nint, error) lookupProtocol(context.Context ctx, @string name) {
    nint proto = default;
    error err = default!;

    return lookupProtocolMap(name);
}

private static (slice<@string>, error) lookupHost(this ptr<Resolver> _addr__p0, context.Context ctx, @string host) {
    slice<@string> addrs = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

private static (slice<IPAddr>, error) lookupIP(this ptr<Resolver> _addr__p0, context.Context ctx, @string network, @string host) {
    slice<IPAddr> addrs = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

private static (nint, error) lookupPort(this ptr<Resolver> _addr__p0, context.Context ctx, @string network, @string service) {
    nint port = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return goLookupPort(network, service);
}

private static (@string, error) lookupCNAME(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) {
    @string cname = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return ("", error.As(syscall.ENOPROTOOPT)!);
}

private static (@string, slice<ptr<SRV>>, error) lookupSRV(this ptr<Resolver> _addr__p0, context.Context ctx, @string service, @string proto, @string name) {
    @string cname = default;
    slice<ptr<SRV>> srvs = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return ("", null, error.As(syscall.ENOPROTOOPT)!);
}

private static (slice<ptr<MX>>, error) lookupMX(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) {
    slice<ptr<MX>> mxs = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

private static (slice<ptr<NS>>, error) lookupNS(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) {
    slice<ptr<NS>> nss = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

private static (slice<@string>, error) lookupTXT(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) {
    slice<@string> txts = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

private static (slice<@string>, error) lookupAddr(this ptr<Resolver> _addr__p0, context.Context ctx, @string addr) {
    slice<@string> ptrs = default;
    error err = default!;
    ref Resolver _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.ENOPROTOOPT)!);
}

// concurrentThreadsLimit returns the number of threads we permit to
// run concurrently doing DNS lookups.
private static nint concurrentThreadsLimit() {
    return 500;
}

} // end net_package
