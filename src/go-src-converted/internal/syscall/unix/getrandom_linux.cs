// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\getrandom_linux.go


namespace go.@internal.syscall;

public static partial class unix_package {

 
// GRND_NONBLOCK means return EAGAIN rather than blocking.
public static readonly GetRandomFlag GRND_NONBLOCK = 0x0001; 

// GRND_RANDOM means use the /dev/random pool instead of /dev/urandom.
public static readonly GetRandomFlag GRND_RANDOM = 0x0002;


} // end unix_package
