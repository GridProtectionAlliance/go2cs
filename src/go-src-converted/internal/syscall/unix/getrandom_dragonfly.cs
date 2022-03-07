// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getrandom_dragonfly.go


namespace go.@internal.syscall;

public static partial class unix_package {

    // DragonFlyBSD getrandom system call number.
private static readonly System.UIntPtr getrandomTrap = 550;



 
// GRND_RANDOM is only set for portability purpose, no-op on DragonFlyBSD.
public static readonly GetRandomFlag GRND_RANDOM = 0x0001; 

// GRND_NONBLOCK means return EAGAIN rather than blocking.
public static readonly GetRandomFlag GRND_NONBLOCK = 0x0002;


} // end unix_package
