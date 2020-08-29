// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:13 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_nacl_arm.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public partial struct Timespec
        {
            public long Sec;
            public int Nsec;
        }

        public partial struct Timeval
        {
            public long Sec;
            public int Usec;
        }

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:int32(nsec));
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:int32(usec));
        }
    }
}
