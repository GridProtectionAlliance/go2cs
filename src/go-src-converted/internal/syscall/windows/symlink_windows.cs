// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class windows_package {

public static readonly syscall.Errno ERROR_INVALID_PARAMETER = 87;
public static readonly UntypedInt FILE_SUPPORTS_OBJECT_IDS = /* 0x00010000 */ 65536;
public static readonly UntypedInt FILE_SUPPORTS_OPEN_BY_FILE_ID = /* 0x01000000 */ 16777216;
public static readonly UntypedInt SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = /* 0x2 */ 2;
public static readonly UntypedInt FileBasicInfo = 0; // FILE_BASIC_INFO
public static readonly UntypedInt FileStandardInfo = 1; // FILE_STANDARD_INFO
public static readonly UntypedInt FileNameInfo = 2; // FILE_NAME_INFO
public static readonly UntypedInt FileStreamInfo = 7; // FILE_STREAM_INFO
public static readonly UntypedInt FileCompressionInfo = 8; // FILE_COMPRESSION_INFO
public static readonly UntypedInt FileAttributeTagInfo = 9; // FILE_ATTRIBUTE_TAG_INFO
public static readonly UntypedInt FileIdBothDirectoryInfo = /* 0xa */ 10; // FILE_ID_BOTH_DIR_INFO
public static readonly UntypedInt FileIdBothDirectoryRestartInfo = /* 0xb */ 11; // FILE_ID_BOTH_DIR_INFO
public static readonly UntypedInt FileRemoteProtocolInfo = /* 0xd */ 13; // FILE_REMOTE_PROTOCOL_INFO
public static readonly UntypedInt FileFullDirectoryInfo = /* 0xe */ 14; // FILE_FULL_DIR_INFO
public static readonly UntypedInt FileFullDirectoryRestartInfo = /* 0xf */ 15; // FILE_FULL_DIR_INFO
public static readonly UntypedInt FileStorageInfo = /* 0x10 */ 16; // FILE_STORAGE_INFO
public static readonly UntypedInt FileAlignmentInfo = /* 0x11 */ 17; // FILE_ALIGNMENT_INFO
public static readonly UntypedInt FileIdInfo = /* 0x12 */ 18; // FILE_ID_INFO
public static readonly UntypedInt FileIdExtdDirectoryInfo = /* 0x13 */ 19; // FILE_ID_EXTD_DIR_INFO
public static readonly UntypedInt FileIdExtdDirectoryRestartInfo = /* 0x14 */ 20; // FILE_ID_EXTD_DIR_INFO

[GoType] partial struct FILE_ATTRIBUTE_TAG_INFO {
    public uint32 FileAttributes;
    public uint32 ReparseTag;
}

//sys	GetFileInformationByHandleEx(handle syscall.Handle, class uint32, info *byte, bufsize uint32) (err error)

} // end windows_package
