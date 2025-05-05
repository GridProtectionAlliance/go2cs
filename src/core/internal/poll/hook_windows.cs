// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using syscall = syscall_package;

partial class poll_package {

// CloseFunc is used to hook the close call.
public static Func<syscall.Handle, error> CloseFunc = syscall.Closesocket;

// AcceptFunc is used to hook the accept call.
public static Func<syscall.Handle, syscall.Handle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<syscall.Overlapped>, error> AcceptFunc = syscall.AcceptEx;

// ConnectExFunc is used to hook the ConnectEx call.
public static Func<syscall.Handle, syscall.Sockaddr, ж<byte>, uint32, ж<uint32>, ж<syscall.Overlapped>, error> ConnectExFunc = syscall.ConnectEx;

} // end poll_package
