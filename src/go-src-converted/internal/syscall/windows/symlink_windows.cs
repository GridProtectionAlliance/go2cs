// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 08 03:32:28 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Go\src\internal\syscall\windows\symlink_windows.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class windows_package
    {
        public static readonly syscall.Errno ERROR_INVALID_PARAMETER = (syscall.Errno)87L; 

        // symlink support for CreateSymbolicLink() starting with Windows 10 (1703, v10.0.14972)
        public static readonly ulong SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = (ulong)0x2UL; 

        // FileInformationClass values
        public static readonly long FileBasicInfo = (long)0L; // FILE_BASIC_INFO
        public static readonly long FileStandardInfo = (long)1L; // FILE_STANDARD_INFO
        public static readonly long FileNameInfo = (long)2L; // FILE_NAME_INFO
        public static readonly long FileStreamInfo = (long)7L; // FILE_STREAM_INFO
        public static readonly long FileCompressionInfo = (long)8L; // FILE_COMPRESSION_INFO
        public static readonly long FileAttributeTagInfo = (long)9L; // FILE_ATTRIBUTE_TAG_INFO
        public static readonly ulong FileIdBothDirectoryInfo = (ulong)0xaUL; // FILE_ID_BOTH_DIR_INFO
        public static readonly ulong FileIdBothDirectoryRestartInfo = (ulong)0xbUL; // FILE_ID_BOTH_DIR_INFO
        public static readonly ulong FileRemoteProtocolInfo = (ulong)0xdUL; // FILE_REMOTE_PROTOCOL_INFO
        public static readonly ulong FileFullDirectoryInfo = (ulong)0xeUL; // FILE_FULL_DIR_INFO
        public static readonly ulong FileFullDirectoryRestartInfo = (ulong)0xfUL; // FILE_FULL_DIR_INFO
        public static readonly ulong FileStorageInfo = (ulong)0x10UL; // FILE_STORAGE_INFO
        public static readonly ulong FileAlignmentInfo = (ulong)0x11UL; // FILE_ALIGNMENT_INFO
        public static readonly ulong FileIdInfo = (ulong)0x12UL; // FILE_ID_INFO
        public static readonly ulong FileIdExtdDirectoryInfo = (ulong)0x13UL; // FILE_ID_EXTD_DIR_INFO
        public static readonly ulong FileIdExtdDirectoryRestartInfo = (ulong)0x14UL; // FILE_ID_EXTD_DIR_INFO

        public partial struct FILE_ATTRIBUTE_TAG_INFO
        {
            public uint FileAttributes;
            public uint ReparseTag;
        }

        //sys    GetFileInformationByHandleEx(handle syscall.Handle, class uint32, info *byte, bufsize uint32) (err error)
    }
}}}
