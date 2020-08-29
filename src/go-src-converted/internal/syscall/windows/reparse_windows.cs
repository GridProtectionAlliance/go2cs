// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 August 29 08:22:31 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Go\src\internal\syscall\windows\reparse_windows.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class windows_package
    {
        public static readonly ulong FSCTL_SET_REPARSE_POINT = 0x000900A4UL;
        public static readonly ulong IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003UL;

        public static readonly long SYMLINK_FLAG_RELATIVE = 1L;

        // These structures are described
        // in https://msdn.microsoft.com/en-us/library/cc232007.aspx
        // and https://msdn.microsoft.com/en-us/library/cc232006.aspx.

        // REPARSE_DATA_BUFFER_HEADER is a common part of REPARSE_DATA_BUFFER structure.
        public partial struct REPARSE_DATA_BUFFER_HEADER
        {
            public uint ReparseTag; // The size, in bytes, of the reparse data that follows
// the common portion of the REPARSE_DATA_BUFFER element.
// This value is the length of the data starting at the
// SubstituteNameOffset field.
            public ushort ReparseDataLength;
            public ushort Reserved;
        }

        public partial struct SymbolicLinkReparseBuffer
        {
            public ushort SubstituteNameOffset; // The integer that contains the length, in bytes, of the
// substitute name string. If this string is null-terminated,
// SubstituteNameLength does not include the Unicode null character.
            public ushort SubstituteNameLength; // PrintNameOffset is similar to SubstituteNameOffset.
            public ushort PrintNameOffset; // PrintNameLength is similar to SubstituteNameLength.
            public ushort PrintNameLength; // Flags specifies whether the substitute name is a full path name or
// a path name relative to the directory containing the symbolic link.
            public uint Flags;
            public array<ushort> PathBuffer;
        }

        public partial struct MountPointReparseBuffer
        {
            public ushort SubstituteNameOffset; // The integer that contains the length, in bytes, of the
// substitute name string. If this string is null-terminated,
// SubstituteNameLength does not include the Unicode null character.
            public ushort SubstituteNameLength; // PrintNameOffset is similar to SubstituteNameOffset.
            public ushort PrintNameOffset; // PrintNameLength is similar to SubstituteNameLength.
            public ushort PrintNameLength;
            public array<ushort> PathBuffer;
        }
    }
}}}
