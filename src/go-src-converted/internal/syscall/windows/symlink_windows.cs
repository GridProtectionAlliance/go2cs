// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 22:13:10 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Program Files\Go\src\internal\syscall\windows\symlink_windows.go
using syscall = go.syscall_package;

namespace go.@internal.syscall;

public static partial class windows_package {

public static readonly syscall.Errno ERROR_INVALID_PARAMETER = 87; 

// symlink support for CreateSymbolicLink() starting with Windows 10 (1703, v10.0.14972)
public static readonly nuint SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 0x2; 

// FileInformationClass values
public static readonly nint FileBasicInfo = 0; // FILE_BASIC_INFO
public static readonly nint FileStandardInfo = 1; // FILE_STANDARD_INFO
public static readonly nint FileNameInfo = 2; // FILE_NAME_INFO
public static readonly nint FileStreamInfo = 7; // FILE_STREAM_INFO
public static readonly nint FileCompressionInfo = 8; // FILE_COMPRESSION_INFO
public static readonly nint FileAttributeTagInfo = 9; // FILE_ATTRIBUTE_TAG_INFO
public static readonly nuint FileIdBothDirectoryInfo = 0xa; // FILE_ID_BOTH_DIR_INFO
public static readonly nuint FileIdBothDirectoryRestartInfo = 0xb; // FILE_ID_BOTH_DIR_INFO
public static readonly nuint FileRemoteProtocolInfo = 0xd; // FILE_REMOTE_PROTOCOL_INFO
public static readonly nuint FileFullDirectoryInfo = 0xe; // FILE_FULL_DIR_INFO
public static readonly nuint FileFullDirectoryRestartInfo = 0xf; // FILE_FULL_DIR_INFO
public static readonly nuint FileStorageInfo = 0x10; // FILE_STORAGE_INFO
public static readonly nuint FileAlignmentInfo = 0x11; // FILE_ALIGNMENT_INFO
public static readonly nuint FileIdInfo = 0x12; // FILE_ID_INFO
public static readonly nuint FileIdExtdDirectoryInfo = 0x13; // FILE_ID_EXTD_DIR_INFO
public static readonly nuint FileIdExtdDirectoryRestartInfo = 0x14; // FILE_ID_EXTD_DIR_INFO

public partial struct FILE_ATTRIBUTE_TAG_INFO {
    public uint FileAttributes;
    public uint ReparseTag;
}

//sys    GetFileInformationByHandleEx(handle syscall.Handle, class uint32, info *byte, bufsize uint32) (err error)

} // end windows_package
