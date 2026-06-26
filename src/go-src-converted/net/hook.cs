// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;

partial class net_package {

internal static Func<context.Context, @string, ж<TCPAddr>, ж<net.TCPAddr>, (*net.TCPConn, error)> testHookDialTCP;
internal static Func<context.Context, Action, @string, (<>IPAddr, error), @string, @string, (<>net.IPAddr, error)> testHookLookupIP = (context.Context ctx, Func<context.Context, @string, @string, (<>IPAddr, error)> fn, @string network, @string host) => fn(ctx, network, host);
internal static Action<ж<netFD>> testPreHookSetKeepAlive = (ж<netFD> _) => {
};
internal static Action<KeepAliveConfig> testHookSetKeepAlive = (KeepAliveConfig _) => {
};
internal static Action testHookStepTime = () => {
};

} // end net_package
