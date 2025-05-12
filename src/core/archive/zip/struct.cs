// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package zip provides support for reading and writing ZIP archives.

See the [ZIP specification] for details.

This package does not support disk spanning.

A note about ZIP64:

To be backwards compatible the FileHeader has both 32 and 64 bit Size
fields. The 64 bit fields will always contain the correct value and
for normal archives both fields will be the same. For files requiring
the ZIP64 format the 32 bit fields will be 0xffffffff and the 64 bit
fields must be used instead.

[ZIP specification]: https://support.pkware.com/pkzip/appnote
*/
namespace go.archive;

using fs = io.fs_package;
using path = path_package;
using time = time_package;
using io;

partial class zip_package {

// Compression methods.
public const uint16 Store = 0; // no compression

public const uint16 Deflate = 8; // DEFLATE compressed

internal static readonly UntypedInt fileHeaderSignature = /* 0x04034b50 */ 67324752;
internal static readonly UntypedInt directoryHeaderSignature = /* 0x02014b50 */ 33639248;
internal static readonly UntypedInt directoryEndSignature = /* 0x06054b50 */ 101010256;
internal static readonly UntypedInt directory64LocSignature = /* 0x07064b50 */ 117853008;
internal static readonly UntypedInt directory64EndSignature = /* 0x06064b50 */ 101075792;
internal static readonly UntypedInt dataDescriptorSignature = /* 0x08074b50 */ 134695760; // de-facto standard; required by OS X Finder
internal static readonly UntypedInt fileHeaderLen = 30; // + filename + extra
internal static readonly UntypedInt directoryHeaderLen = 46; // + filename + extra + comment
internal static readonly UntypedInt directoryEndLen = 22; // + comment
internal static readonly UntypedInt dataDescriptorLen = 16; // four uint32: descriptor signature, crc32, compressed size, size
internal static readonly UntypedInt dataDescriptor64Len = 24; // two uint32: signature, crc32 | two uint64: compressed size, size
internal static readonly UntypedInt directory64LocLen = 20; //
internal static readonly UntypedInt directory64EndLen = 56; // + extra
internal static readonly UntypedInt creatorFAT = 0;
internal static readonly UntypedInt creatorUnix = 3;
internal static readonly UntypedInt creatorNTFS = 11;
internal static readonly UntypedInt creatorVFAT = 14;
internal static readonly UntypedInt creatorMacOSX = 19;
internal static readonly UntypedInt zipVersion20 = 20; // 2.0
internal static readonly UntypedInt zipVersion45 = 45; // 4.5 (reads and writes zip64 archives)
internal static readonly UntypedInt uint16max = /* (1 << 16) - 1 */ 65535;
internal static readonly UntypedInt uint32max = /* (1 << 32) - 1 */ 4294967295;
internal static readonly UntypedInt zip64ExtraID = /* 0x0001 */ 1; // Zip64 extended information
internal static readonly UntypedInt ntfsExtraID = /* 0x000a */ 10; // NTFS
internal static readonly UntypedInt unixExtraID = /* 0x000d */ 13; // UNIX
internal static readonly UntypedInt extTimeExtraID = /* 0x5455 */ 21589; // Extended timestamp
internal static readonly UntypedInt infoZipUnixExtraID = /* 0x5855 */ 22613; // Info-ZIP Unix extension

// FileHeader describes a file within a ZIP file.
// See the [ZIP specification] for details.
//
// [ZIP specification]: https://support.pkware.com/pkzip/appnote
[GoType] partial struct FileHeader {
    // Name is the name of the file.
    //
    // It must be a relative path, not start with a drive letter (such as "C:"),
    // and must use forward slashes instead of back slashes. A trailing slash
    // indicates that this file is a directory and should have no data.
    public @string Name;
    // Comment is any arbitrary user-defined string shorter than 64KiB.
    public @string Comment;
    // NonUTF8 indicates that Name and Comment are not encoded in UTF-8.
    //
    // By specification, the only other encoding permitted should be CP-437,
    // but historically many ZIP readers interpret Name and Comment as whatever
    // the system's local character encoding happens to be.
    //
    // This flag should only be set if the user intends to encode a non-portable
    // ZIP file for a specific localized region. Otherwise, the Writer
    // automatically sets the ZIP format's UTF-8 flag for valid UTF-8 strings.
    public bool NonUTF8;
    public uint16 CreatorVersion;
    public uint16 ReaderVersion;
    public uint16 Flags;
    // Method is the compression method. If zero, Store is used.
    public uint16 Method;
    // Modified is the modified time of the file.
    //
    // When reading, an extended timestamp is preferred over the legacy MS-DOS
    // date field, and the offset between the times is used as the timezone.
    // If only the MS-DOS date is present, the timezone is assumed to be UTC.
    //
    // When writing, an extended timestamp (which is timezone-agnostic) is
    // always emitted. The legacy MS-DOS date field is encoded according to the
    // location of the Modified time.
    public time_package.Time Modified;
    // ModifiedTime is an MS-DOS-encoded time.
    //
    // Deprecated: Use Modified instead.
    public uint16 ModifiedTime;
    // ModifiedDate is an MS-DOS-encoded date.
    //
    // Deprecated: Use Modified instead.
    public uint16 ModifiedDate;
    // CRC32 is the CRC32 checksum of the file content.
    public uint32 CRC32;
    // CompressedSize is the compressed size of the file in bytes.
    // If either the uncompressed or compressed size of the file
    // does not fit in 32 bits, CompressedSize is set to ^uint32(0).
    //
    // Deprecated: Use CompressedSize64 instead.
    public uint32 CompressedSize;
    // UncompressedSize is the uncompressed size of the file in bytes.
    // If either the uncompressed or compressed size of the file
    // does not fit in 32 bits, UncompressedSize is set to ^uint32(0).
    //
    // Deprecated: Use UncompressedSize64 instead.
    public uint32 UncompressedSize;
    // CompressedSize64 is the compressed size of the file in bytes.
    public uint64 CompressedSize64;
    // UncompressedSize64 is the uncompressed size of the file in bytes.
    public uint64 UncompressedSize64;
    public slice<byte> Extra;
    public uint32 ExternalAttrs; // Meaning depends on CreatorVersion
}

// FileInfo returns an fs.FileInfo for the [FileHeader].
[GoRecv] public static fs.FileInfo FileInfo(this ref FileHeader h) {
    return new headerFileInfo(h);
}

// headerFileInfo implements [fs.FileInfo].
[GoType] partial struct headerFileInfo {
    internal ж<FileHeader> fh;
}

internal static @string Name(this headerFileInfo fi) {
    return path.Base(fi.fh.Name);
}

internal static int64 Size(this headerFileInfo fi) {
    if (fi.fh.UncompressedSize64 > 0) {
        return ((int64)fi.fh.UncompressedSize64);
    }
    return ((int64)fi.fh.UncompressedSize);
}

internal static bool IsDir(this headerFileInfo fi) {
    return fi.Mode().IsDir();
}

internal static time.Time ModTime(this headerFileInfo fi) {
    if (fi.fh.Modified.IsZero()) {
        return fi.fh.ModTime();
    }
    return fi.fh.Modified.UTC();
}

internal static fs.FileMode Mode(this headerFileInfo fi) {
    return fi.fh.Mode();
}

internal static fs.FileMode Type(this headerFileInfo fi) {
    return fi.fh.Mode().Type();
}

internal static any Sys(this headerFileInfo fi) {
    return fi.fh;
}

internal static (fs.FileInfo, error) Info(this headerFileInfo fi) {
    return (fi, default!);
}

internal static @string String(this headerFileInfo fi) {
    return fs.FormatFileInfo(fi);
}

// FileInfoHeader creates a partially-populated [FileHeader] from an
// fs.FileInfo.
// Because fs.FileInfo's Name method returns only the base name of
// the file it describes, it may be necessary to modify the Name field
// of the returned header to provide the full path name of the file.
// If compression is desired, callers should set the FileHeader.Method
// field; it is unset by default.
public static (ж<FileHeader>, error) FileInfoHeader(fs.FileInfo fi) {
    var size = fi.Size();
    var fh = Ꮡ(new FileHeader(
        Name: fi.Name(),
        UncompressedSize64: ((uint64)size)
    ));
    fh.SetModTime(fi.ModTime());
    fh.SetMode(fi.Mode());
    if ((~fh).UncompressedSize64 > uint32max){
        fh.val.UncompressedSize = uint32max;
    } else {
        fh.val.UncompressedSize = ((uint32)(~fh).UncompressedSize64);
    }
    return (fh, default!);
}

[GoType] partial struct directoryEnd {
    internal uint32 diskNbr; // unused
    internal uint32 dirDiskNbr; // unused
    internal uint64 dirRecordsThisDisk; // unused
    internal uint64 directoryRecords;
    internal uint64 directorySize;
    internal uint64 directoryOffset; // relative to file
    internal uint16 commentLen;
    internal @string comment;
}

// timeZone returns a *time.Location based on the provided offset.
// If the offset is non-sensible, then this uses an offset of zero.
internal static ж<timeꓸLocation> timeZone(time.Duration offset) {
    GoUntyped minOffset = /* -12 * time.Hour */ // E.g., Baker island at -12:00
            GoUntyped.Parse("-43200000000000");
    static readonly time.Duration maxOffset = /* +14 * time.Hour */ 50400000000000; // E.g., Line island at +14:00
    static readonly time.Duration offsetAlias = /* 15 * time.Minute */ 900000000000; // E.g., Nepal at +5:45
    offset = offset.Round(offsetAlias);
    if (offset < minOffset || maxOffset < offset) {
        offset = 0;
    }
    return time.FixedZone(""u8, ((nint)(offset / time.ΔSecond)));
}

// msDosTimeToTime converts an MS-DOS date and time into a time.Time.
// The resolution is 2s.
// See: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-dosdatetimetofiletime
internal static time.Time msDosTimeToTime(uint16 dosDate, uint16 dosTime) {
    return time.Date(
        ((nint)(dosDate >> (int)(9) + 1980)), // date bits 0-4: day of month; 5-8: month; 9-15: years since 1980

        ((timeꓸMonth)((uint16)(dosDate >> (int)(5) & 15))),
        ((nint)((uint16)(dosDate & 31))), // time bits 0-4: second/2; 5-10: minute; 11-15: hour

        ((nint)(dosTime >> (int)(11))),
        ((nint)((uint16)(dosTime >> (int)(5) & 63))),
        ((nint)((uint16)(dosTime & 31) * 2)),
        0, // nanoseconds

        time.ΔUTC);
}

// timeToMsDosTime converts a time.Time to an MS-DOS date and time.
// The resolution is 2s.
// See: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-filetimetodosdatetime
internal static (uint16 fDate, uint16 fTime) timeToMsDosTime(time.Time t) {
    uint16 fDate = default!;
    uint16 fTime = default!;

    fDate = ((uint16)(t.Day() + ((nint)t.Month()) << (int)(5) + (t.Year() - 1980) << (int)(9)));
    fTime = ((uint16)(t.Second() / 2 + t.Minute() << (int)(5) + t.Hour() << (int)(11)));
    return (fDate, fTime);
}

// ModTime returns the modification time in UTC using the legacy
// [ModifiedDate] and [ModifiedTime] fields.
//
// Deprecated: Use [Modified] instead.
[GoRecv] public static time.Time ModTime(this ref FileHeader h) {
    return msDosTimeToTime(h.ModifiedDate, h.ModifiedTime);
}

// SetModTime sets the [Modified], [ModifiedTime], and [ModifiedDate] fields
// to the given time in UTC.
//
// Deprecated: Use [Modified] instead.
[GoRecv] public static void SetModTime(this ref FileHeader h, time.Time t) {
    t = t.UTC();
    // Convert to UTC for compatibility
    h.Modified = t;
    (h.ModifiedDate, h.ModifiedTime) = timeToMsDosTime(t);
}

internal static readonly UntypedInt s_IFMT = /* 0xf000 */ 61440;
internal static readonly UntypedInt s_IFSOCK = /* 0xc000 */ 49152;
internal static readonly UntypedInt s_IFLNK = /* 0xa000 */ 40960;
internal static readonly UntypedInt s_IFREG = /* 0x8000 */ 32768;
internal static readonly UntypedInt s_IFBLK = /* 0x6000 */ 24576;
internal static readonly UntypedInt s_IFDIR = /* 0x4000 */ 16384;
internal static readonly UntypedInt s_IFCHR = /* 0x2000 */ 8192;
internal static readonly UntypedInt s_IFIFO = /* 0x1000 */ 4096;
internal static readonly UntypedInt s_ISUID = /* 0x800 */ 2048;
internal static readonly UntypedInt s_ISGID = /* 0x400 */ 1024;
internal static readonly UntypedInt s_ISVTX = /* 0x200 */ 512;
internal static readonly UntypedInt msdosDir = /* 0x10 */ 16;
internal static readonly UntypedInt msdosReadOnly = /* 0x01 */ 1;

// Mode returns the permission and mode bits for the [FileHeader].
[GoRecv] public static fs.FileMode /*mode*/ Mode(this ref FileHeader h) {
    fs.FileMode mode = default!;

    var exprᴛ1 = h.CreatorVersion >> (int)(8);
    if (exprᴛ1 == creatorUnix || exprᴛ1 == creatorMacOSX) {
        mode = unixModeToFileMode(h.ExternalAttrs >> (int)(16));
    }
    else if (exprᴛ1 == creatorNTFS || exprᴛ1 == creatorVFAT || exprᴛ1 == creatorFAT) {
        mode = msdosModeToFileMode(h.ExternalAttrs);
    }

    if (len(h.Name) > 0 && h.Name[len(h.Name) - 1] == (rune)'/') {
        mode |= (fs.FileMode)(fs.ModeDir);
    }
    return mode;
}

// SetMode changes the permission and mode bits for the [FileHeader].
[GoRecv] public static void SetMode(this ref FileHeader h, fs.FileMode mode) {
    h.CreatorVersion = (uint16)((uint16)(h.CreatorVersion & 255) | creatorUnix << (int)(8));
    h.ExternalAttrs = fileModeToUnixMode(mode) << (int)(16);
    // set MSDOS attributes too, as the original zip does.
    if ((fs.FileMode)(mode & fs.ModeDir) != 0) {
        h.ExternalAttrs |= (uint32)(msdosDir);
    }
    if ((fs.FileMode)(mode & 128) == 0) {
        h.ExternalAttrs |= (uint32)(msdosReadOnly);
    }
}

// isZip64 reports whether the file size exceeds the 32 bit limit
[GoRecv] internal static bool isZip64(this ref FileHeader h) {
    return h.CompressedSize64 >= uint32max || h.UncompressedSize64 >= uint32max;
}

[GoRecv] internal static bool hasDataDescriptor(this ref FileHeader h) {
    return (uint16)(h.Flags & 8) != 0;
}

internal static fs.FileMode /*mode*/ msdosModeToFileMode(uint32 m) {
    fs.FileMode mode = default!;

    if ((uint32)(m & msdosDir) != 0){
        mode = (fs.FileMode)(fs.ModeDir | 511);
    } else {
        mode = 438;
    }
    if ((uint32)(m & msdosReadOnly) != 0) {
        mode &= ~(fs.FileMode)(146);
    }
    return mode;
}

internal static uint32 fileModeToUnixMode(fs.FileMode mode) {
    uint32 m = default!;
    var exprᴛ1 = (fs.FileMode)(mode & fs.ModeType);
    { /* default: */
        m = s_IFREG;
    }
    else if (exprᴛ1 == fs.ModeDir) {
        m = s_IFDIR;
    }
    else if (exprᴛ1 == fs.ModeSymlink) {
        m = s_IFLNK;
    }
    else if (exprᴛ1 == fs.ModeNamedPipe) {
        m = s_IFIFO;
    }
    else if (exprᴛ1 == fs.ModeSocket) {
        m = s_IFSOCK;
    }
    else if (exprᴛ1 == fs.ModeDevice) {
        m = s_IFBLK;
    }
    else if (exprᴛ1 == (fs.FileMode)(fs.ModeDevice | fs.ModeCharDevice)) {
        m = s_IFCHR;
    }

    if ((fs.FileMode)(mode & fs.ModeSetuid) != 0) {
        m |= (uint32)(s_ISUID);
    }
    if ((fs.FileMode)(mode & fs.ModeSetgid) != 0) {
        m |= (uint32)(s_ISGID);
    }
    if ((fs.FileMode)(mode & fs.ModeSticky) != 0) {
        m |= (uint32)(s_ISVTX);
    }
    return (uint32)(m | ((uint32)((fs.FileMode)(mode & 511))));
}

internal static fs.FileMode unixModeToFileMode(uint32 m) {
    var mode = ((fs.FileMode)((uint32)(m & 511)));
    var exprᴛ1 = (uint32)(m & s_IFMT);
    if (exprᴛ1 == s_IFBLK) {
        mode |= (fs.FileMode)(fs.ModeDevice);
    }
    else if (exprᴛ1 == s_IFCHR) {
        mode |= (fs.FileMode)((fs.FileMode)(fs.ModeDevice | fs.ModeCharDevice));
    }
    else if (exprᴛ1 == s_IFDIR) {
        mode |= (fs.FileMode)(fs.ModeDir);
    }
    else if (exprᴛ1 == s_IFIFO) {
        mode |= (fs.FileMode)(fs.ModeNamedPipe);
    }
    else if (exprᴛ1 == s_IFLNK) {
        mode |= (fs.FileMode)(fs.ModeSymlink);
    }
    else if (exprᴛ1 == s_IFREG) {
    }
    else if (exprᴛ1 == s_IFSOCK) {
        mode |= (fs.FileMode)(fs.ModeSocket);
    }

    // nothing to do
    if ((uint32)(m & s_ISGID) != 0) {
        mode |= (fs.FileMode)(fs.ModeSetgid);
    }
    if ((uint32)(m & s_ISUID) != 0) {
        mode |= (fs.FileMode)(fs.ModeSetuid);
    }
    if ((uint32)(m & s_ISVTX) != 0) {
        mode |= (fs.FileMode)(fs.ModeSticky);
    }
    return mode;
}

} // end zip_package
