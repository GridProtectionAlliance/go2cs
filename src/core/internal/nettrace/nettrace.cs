// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nettrace contains internal hooks for tracing activity in
// the net package. This package is purely internal for use by the
// net/http/httptrace package and has no stable API exposed to end
// users.
namespace go.@internal;

partial class nettrace_package {

// TraceKey is a context.Context Value key. Its associated value should
// be a *Trace struct.
[GoType] partial struct TraceKey {
}

// LookupIPAltResolverKey is a context.Context Value key used by tests to
// specify an alternate resolver func.
// It is not exposed to outsider users. (But see issue 12503)
// The value should be the same type as lookupIP:
//
//	func lookupIP(ctx context.Context, host string) ([]IPAddr, error)
[GoType] partial struct LookupIPAltResolverKey {
}

// Trace contains a set of hooks for tracing events within
// the net package. Any specific hook may be nil.
[GoType] partial struct Trace {
    // DNSStart is called with the hostname of a DNS lookup
    // before it begins.
    public Action<@string> DNSStart;
    // DNSDone is called after a DNS lookup completes (or fails).
    // The coalesced parameter is whether singleflight de-duped
    // the call. The addrs are of type net.IPAddr but can't
    // actually be for circular dependency reasons.
    public Action<slice<any>, bool, error> DNSDone;
    // ConnectStart is called before a Dial, excluding Dials made
    // during DNS lookups. In the case of DualStack (Happy Eyeballs)
    // dialing, this may be called multiple times, from multiple
    // goroutines.
    public Action<@string, @string> ConnectStart;
    // ConnectDone is called after a Dial with the results, excluding
    // Dials made during DNS lookups. It may also be called multiple
    // times, like ConnectStart.
    public Action<@string, @string, error> ConnectDone;
}

} // end nettrace_package
