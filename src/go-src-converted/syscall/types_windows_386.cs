// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 08 03:27:53 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\types_windows_386.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public partial struct WSAData
        {
            public ushort Version;
            public ushort HighVersion;
            public array<byte> Description;
            public array<byte> SystemStatus;
            public ushort MaxSockets;
            public ushort MaxUdpDg;
            public ptr<byte> VendorInfo;
        }

        public partial struct Servent
        {
            public ptr<byte> Name;
            public ptr<ptr<byte>> Aliases;
            public ushort Port;
            public ptr<byte> Proto;
        }
    }
}
