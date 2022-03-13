// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fake network poller for wasm/js.
// Should never be used, because wasm/js network connections do not honor "SetNonblock".

//go:build js && wasm
// +build js,wasm

// package runtime -- go2cs converted at 2022 March 13 05:26:04 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll_fake.go
namespace go;

public static partial class runtime_package {

private static void netpollinit() {
}

private static bool netpollIsPollDescriptor(System.UIntPtr fd) {
    return false;
}

private static int netpollopen(System.UIntPtr fd, ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    return 0;
}

private static int netpollclose(System.UIntPtr fd) {
    return 0;
}

private static void netpollarm(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

}

private static void netpollBreak() {
}

private static gList netpoll(long delay) {
    return new gList();
}

} // end runtime_package
