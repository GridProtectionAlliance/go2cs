// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package server -- go2cs converted at 2022 March 06 23:32:49 UTC
// import "go/internal/gccgoimporter.server" ==> using server = go.go.@internal.gccgoimporter.server_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\issue29198.go
using context = go.context_package;
using errors = go.errors_package;

namespace go.go.@internal;

public static partial class server_package {

public partial struct A {
    public nint x;
}

private static ptr<Server> AMethod(this ptr<A> _addr_a, nint y) {
    ref A a = ref _addr_a.val;

    return _addr_null!;
}

// FooServer is a server that provides Foo services
public partial struct FooServer { // : Server
}

private static error WriteEvents(this ptr<FooServer> _addr_f, context.Context ctx, nint x) {
    ref FooServer f = ref _addr_f.val;

    return error.As(errors.New("hey!"))!;
}

public partial struct Server {
    public ptr<FooServer> FooServer;
    public @string user;
    public context.Context ctx;
}

public static (ptr<Server>, error) New(context.Context sctx, @string u) {
    ptr<Server> _p0 = default!;
    error _p0 = default!;

    ptr<Server> s = addr(new Server(user:u,ctx:sctx));
    s.FooServer = (FooServer.val)(s);
    return (_addr_s!, error.As(null!)!);
}

} // end server_package
