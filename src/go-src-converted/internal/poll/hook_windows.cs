// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// CloseFunc is used to hook the close call.
public static Func<syscallꓸHandle, error> CloseFunc = Δsyscall.Closesocket;

// AcceptFunc is used to hook the accept call.
public static Func<syscallꓸHandle, syscallꓸHandle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error> AcceptFunc = Δsyscall.AcceptEx;

// ConnectExFunc is used to hook the ConnectEx call.
public static Func<syscallꓸHandle, syscallꓸSockaddr, ж<byte>, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error> ConnectExFunc = Δsyscall.ConnectEx;

} // end poll_package
