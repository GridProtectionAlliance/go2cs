// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package socktest provides utilities for socket testing.
namespace go.net.@internal;

using fmt = fmt_package;
using sync = sync_package;

partial class socktest_package {

// A Switch represents a callpath point switch for socket system
// calls.
[GoType] partial struct Switch {
    internal sync_package.Once once;
    internal sync_package.RWMutex fmu;
    internal socktest.Filter fltab;
    internal sync_package.RWMutex smu;
    internal ΔSockets sotab;
    internal stats stats;
}

[GoRecv] internal static void init(this ref Switch sw) {
    sw.fltab = new socktest.Filter();
    sw.sotab = new ΔSockets();
    sw.stats = new stats();
}

// Stats returns a list of per-cookie socket statistics.
[GoRecv] public static slice<Stat> Stats(this ref Switch sw) {
    slice<Stat> st = default!;
    sw.smu.RLock();
    foreach (var (_, s) in sw.stats) {
        var ns = s.val;
        st = append(st, ns);
    }
    sw.smu.RUnlock();
    return st;
}

// Sockets returns mappings of socket descriptor to socket status.
[GoRecv] public static ΔSockets Sockets(this ref Switch sw) {
    sw.smu.RLock();
    var tab = new ΔSockets(len(sw.sotab));
    foreach (var (i, s) in sw.sotab) {
        tab[i] = s;
    }
    sw.smu.RUnlock();
    return tab;
}

[GoType("num:uint64")] partial struct Cookie;

// Family returns an address family.
public static nint Family(this Cookie c) {
    return ((nint)(c >> (int)(48)));
}

// Type returns a socket type.
public static nint Type(this Cookie c) {
    return ((nint)(c << (int)(16) >> (int)(32)));
}

// Protocol returns a protocol number.
public static nint Protocol(this Cookie c) {
    return ((nint)((Cookie)(c & 255)));
}

internal static Cookie cookie(nint family, nint sotype, nint proto) {
    return (Cookie)((Cookie)(((Cookie)family) << (int)(48) | (Cookie)(((Cookie)sotype) & (nint)4294967295L) << (int)(16)) | (Cookie)(((Cookie)proto) & 255));
}

// A Status represents the status of a socket.
[GoType] partial struct Status {
    public Cookie Cookie;
    public error Err; // error status of socket system call
    public error SocketErr; // error status of socket by SO_ERROR
}

public static @string String(this Status so) {
    return fmt.Sprintf("(%s, %s, %s): syscallerr=%v socketerr=%v"u8, familyString(so.Cookie.Family()), typeString(so.Cookie.Type()), protocolString(so.Cookie.Protocol()), so.Err, so.SocketErr);
}

// A Stat represents a per-cookie socket statistics.
[GoType] partial struct Stat {
    public nint Family; // address family
    public nint Type; // socket type
    public nint Protocol; // protocol number
    public uint64 Opened; // number of sockets opened
    public uint64 Connected; // number of sockets connected
    public uint64 Listened; // number of sockets listened
    public uint64 Accepted; // number of sockets accepted
    public uint64 Closed; // number of sockets closed
    public uint64 OpenFailed; // number of sockets open failed
    public uint64 ConnectFailed; // number of sockets connect failed
    public uint64 ListenFailed; // number of sockets listen failed
    public uint64 AcceptFailed; // number of sockets accept failed
    public uint64 CloseFailed; // number of sockets close failed
}

public static @string String(this Stat st) {
    return fmt.Sprintf("(%s, %s, %s): opened=%d connected=%d listened=%d accepted=%d closed=%d openfailed=%d connectfailed=%d listenfailed=%d acceptfailed=%d closefailed=%d"u8, familyString(st.Family), typeString(st.Type), protocolString(st.Protocol), st.Opened, st.Connected, st.Listened, st.Accepted, st.Closed, st.OpenFailed, st.ConnectFailed, st.ListenFailed, st.AcceptFailed, st.CloseFailed);
}
/* visitMapType: map[Cookie]*Stat */

internal static ж<Stat> getLocked(this stats st, Cookie c) {
    var s = st[c];
    var ok = st[c];
    if (!ok) {
        s = Ꮡ(new Stat(Family: c.Family(), Type: c.Type(), Protocol: c.Protocol()));
        st[c] = s;
    }
    return s;
}

[GoType("num:nint")] partial struct FilterType;

public static readonly FilterType FilterSocket = /* iota */ 0;         // for Socket
public static readonly FilterType FilterConnect = 1;        // for Connect or ConnectEx
public static readonly FilterType FilterListen = 2;         // for Listen
public static readonly FilterType FilterAccept = 3;         // for Accept, Accept4 or AcceptEx
public static readonly FilterType FilterGetsockoptInt = 4;  // for GetsockoptInt
public static readonly FilterType FilterClose = 5;          // for Close or Closesocket

public delegate (AfterFilter, error) Filter(ж<Status> _);

public static (AfterFilter, error) apply(this Filter f, ж<Status> Ꮡst) {
    ref var st = ref Ꮡst.val;

    if (f == default!) {
        return (default!, default!);
    }
    return f(Ꮡst);
}

public delegate error AfterFilter(ж<Status> _);

public static error apply(this AfterFilter f, ж<Status> Ꮡst) {
    ref var st = ref Ꮡst.val;

    if (f == default!) {
        return default!;
    }
    return f(Ꮡst);
}

// Set deploys the socket system call filter f for the filter type t.
[GoRecv] public static void Set(this ref Switch sw, FilterType t, Filter f) {
    sw.once.Do(sw.init);
    sw.fmu.Lock();
    sw.fltab[t] = f;
    sw.fmu.Unlock();
}

} // end socktest_package
