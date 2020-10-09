// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 09 06:00:59 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\types_windows_386.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
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
}}}}}}
