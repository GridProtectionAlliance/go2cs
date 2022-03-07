// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package zip provides support for reading and writing ZIP archives.

See: https://www.pkware.com/appnote

This package does not support disk spanning.

A note about ZIP64:

To be backwards compatible the FileHeader has both 32 and 64 bit Size
fields. The 64 bit fields will always contain the correct value and
for normal archives both fields will be the same. For files requiring
the ZIP64 format the 32 bit fields will be 0xffffffff and the 64 bit
fields must be used instead.
*/
// package zip -- go2cs converted at 2022 March 06 22:31:42 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Program Files\Go\src\archive\zip\struct.go
using fs = go.io.fs_package;
using path = go.path_package;
using time = go.time_package;

namespace go.archive;

public static partial class zip_package {

    // Compression methods.
public static readonly ushort Store = 0; // no compression
public static readonly ushort Deflate = 8; // DEFLATE compressed

private static readonly nuint fileHeaderSignature = 0x04034b50;
private static readonly nuint directoryHeaderSignature = 0x02014b50;
private static readonly nuint directoryEndSignature = 0x06054b50;
private static readonly nuint directory64LocSignature = 0x07064b50;
private static readonly nuint directory64EndSignature = 0x06064b50;
private static readonly nuint dataDescriptorSignature = 0x08074b50; // de-facto standard; required by OS X Finder
private static readonly nint fileHeaderLen = 30; // + filename + extra
private static readonly nint directoryHeaderLen = 46; // + filename + extra + comment
private static readonly nint directoryEndLen = 22; // + comment
private static readonly nint dataDescriptorLen = 16; // four uint32: descriptor signature, crc32, compressed size, size
private static readonly nint dataDescriptor64Len = 24; // two uint32: signature, crc32 | two uint64: compressed size, size
private static readonly nint directory64LocLen = 20; //
private static readonly nint directory64EndLen = 56; // + extra

// Constants for the first byte in CreatorVersion.
private static readonly nint creatorFAT = 0;
private static readonly nint creatorUnix = 3;
private static readonly nint creatorNTFS = 11;
private static readonly nint creatorVFAT = 14;
private static readonly nint creatorMacOSX = 19; 

// Version numbers.
private static readonly nint zipVersion20 = 20; // 2.0
private static readonly nint zipVersion45 = 45; // 4.5 (reads and writes zip64 archives)

// Limits for non zip64 files.
private static readonly nint uint16max = (1 << 16) - 1;
private static readonly nint uint32max = (1 << 32) - 1; 

// Extra header IDs.
//
// IDs 0..31 are reserved for official use by PKWARE.
// IDs above that range are defined by third-party vendors.
// Since ZIP lacked high precision timestamps (nor a official specification
// of the timezone used for the date fields), many competing extra fields
// have been invented. Pervasive use effectively makes them "official".
//
// See http://mdfs.net/Docs/Comp/Archiving/Zip/ExtraField
private static readonly nuint zip64ExtraID = 0x0001; // Zip64 extended information
private static readonly nuint ntfsExtraID = 0x000a; // NTFS
private static readonly nuint unixExtraID = 0x000d; // UNIX
private static readonly nuint extTimeExtraID = 0x5455; // Extended timestamp
private static readonly nuint infoZipUnixExtraID = 0x5855; // Info-ZIP Unix extension

// FileHeader describes a file within a zip file.
// See the zip spec for details.
public partial struct FileHeader {
    public @string Name; // Comment is any arbitrary user-defined string shorter than 64KiB.
    public @string Comment; // NonUTF8 indicates that Name and Comment are not encoded in UTF-8.
//
// By specification, the only other encoding permitted should be CP-437,
// but historically many ZIP readers interpret Name and Comment as whatever
// the system's local character encoding happens to be.
//
// This flag should only be set if the user intends to encode a non-portable
// ZIP file for a specific localized region. Otherwise, the Writer
// automatically sets the ZIP format's UTF-8 flag for valid UTF-8 strings.
    public bool NonUTF8;
    public ushort CreatorVersion;
    public ushort ReaderVersion;
    public ushort Flags; // Method is the compression method. If zero, Store is used.
    public ushort Method; // Modified is the modified time of the file.
//
// When reading, an extended timestamp is preferred over the legacy MS-DOS
// date field, and the offset between the times is used as the timezone.
// If only the MS-DOS date is present, the timezone is assumed to be UTC.
//
// When writing, an extended timestamp (which is timezone-agnostic) is
// always emitted. The legacy MS-DOS date field is encoded according to the
// location of the Modified time.
    public time.Time Modified;
    public ushort ModifiedTime; // Deprecated: Legacy MS-DOS date; use Modified instead.
    public ushort ModifiedDate; // Deprecated: Legacy MS-DOS time; use Modified instead.

    public uint CRC32;
    public uint CompressedSize; // Deprecated: Use CompressedSize64 instead.
    public uint UncompressedSize; // Deprecated: Use UncompressedSize64 instead.
    public ulong CompressedSize64;
    public ulong UncompressedSize64;
    public slice<byte> Extra;
    public uint ExternalAttrs; // Meaning depends on CreatorVersion
}

// FileInfo returns an fs.FileInfo for the FileHeader.
private static fs.FileInfo FileInfo(this ptr<FileHeader> _addr_h) {
    ref FileHeader h = ref _addr_h.val;

    return new headerFileInfo(h);
}

// headerFileInfo implements fs.FileInfo.
private partial struct headerFileInfo {
    public ptr<FileHeader> fh;
}

private static @string Name(this headerFileInfo fi) {
    return path.Base(fi.fh.Name);
}
private static long Size(this headerFileInfo fi) {
    if (fi.fh.UncompressedSize64 > 0) {
        return int64(fi.fh.UncompressedSize64);
    }
    return int64(fi.fh.UncompressedSize);

}
private static bool IsDir(this headerFileInfo fi) {
    return fi.Mode().IsDir();
}
private static time.Time ModTime(this headerFileInfo fi) {
    if (fi.fh.Modified.IsZero()) {
        return fi.fh.ModTime();
    }
    return fi.fh.Modified.UTC();

}
private static fs.FileMode Mode(this headerFileInfo fi) {
    return fi.fh.Mode();
}
private static fs.FileMode Type(this headerFileInfo fi) {
    return fi.fh.Mode().Type();
}
private static void Sys(this headerFileInfo fi) {
    return fi.fh;
}

private static (fs.FileInfo, error) Info(this headerFileInfo fi) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    return (fi, error.As(null!)!);
}

// FileInfoHeader creates a partially-populated FileHeader from an
// fs.FileInfo.
// Because fs.FileInfo's Name method returns only the base name of
// the file it describes, it may be necessary to modify the Name field
// of the returned header to provide the full path name of the file.
// If compression is desired, callers should set the FileHeader.Method
// field; it is unset by default.
public static (ptr<FileHeader>, error) FileInfoHeader(fs.FileInfo fi) {
    ptr<FileHeader> _p0 = default!;
    error _p0 = default!;

    var size = fi.Size();
    ptr<FileHeader> fh = addr(new FileHeader(Name:fi.Name(),UncompressedSize64:uint64(size),));
    fh.SetModTime(fi.ModTime());
    fh.SetMode(fi.Mode());
    if (fh.UncompressedSize64 > uint32max) {
        fh.UncompressedSize = uint32max;
    }
    else
 {
        fh.UncompressedSize = uint32(fh.UncompressedSize64);
    }
    return (_addr_fh!, error.As(null!)!);

}

private partial struct directoryEnd {
    public uint diskNbr; // unused
    public uint dirDiskNbr; // unused
    public ulong dirRecordsThisDisk; // unused
    public ulong directoryRecords;
    public ulong directorySize;
    public ulong directoryOffset; // relative to file
    public ushort commentLen;
    public @string comment;
}

// timeZone returns a *time.Location based on the provided offset.
// If the offset is non-sensible, then this uses an offset of zero.
private static ptr<time.Location> timeZone(time.Duration offset) {
    const nint minOffset = -12 * time.Hour; // E.g., Baker island at -12:00
    const nint maxOffset = +14 * time.Hour; // E.g., Line island at +14:00
    const nint offsetAlias = 15 * time.Minute; // E.g., Nepal at +5:45
    offset = offset.Round(offsetAlias);
    if (offset < minOffset || maxOffset < offset) {
        offset = 0;
    }
    return _addr_time.FixedZone("", int(offset / time.Second))!;

}

// msDosTimeToTime converts an MS-DOS date and time into a time.Time.
// The resolution is 2s.
// See: https://msdn.microsoft.com/en-us/library/ms724247(v=VS.85).aspx
private static time.Time msDosTimeToTime(ushort dosDate, ushort dosTime) {
    return time.Date(int(dosDate >> 9 + 1980), time.Month(dosDate >> 5 & 0xf), int(dosDate & 0x1f), int(dosTime >> 11), int(dosTime >> 5 & 0x3f), int(dosTime & 0x1f * 2), 0, time.UTC);
}

// timeToMsDosTime converts a time.Time to an MS-DOS date and time.
// The resolution is 2s.
// See: https://msdn.microsoft.com/en-us/library/ms724274(v=VS.85).aspx
private static (ushort, ushort) timeToMsDosTime(time.Time t) {
    ushort fDate = default;
    ushort fTime = default;

    fDate = uint16(t.Day() + int(t.Month()) << 5 + (t.Year() - 1980) << 9);
    fTime = uint16(t.Second() / 2 + t.Minute() << 5 + t.Hour() << 11);
    return ;
}

// ModTime returns the modification time in UTC using the legacy
// ModifiedDate and ModifiedTime fields.
//
// Deprecated: Use Modified instead.
private static time.Time ModTime(this ptr<FileHeader> _addr_h) {
    ref FileHeader h = ref _addr_h.val;

    return msDosTimeToTime(h.ModifiedDate, h.ModifiedTime);
}

// SetModTime sets the Modified, ModifiedTime, and ModifiedDate fields
// to the given time in UTC.
//
// Deprecated: Use Modified instead.
private static void SetModTime(this ptr<FileHeader> _addr_h, time.Time t) {
    ref FileHeader h = ref _addr_h.val;

    t = t.UTC(); // Convert to UTC for compatibility
    h.Modified = t;
    h.ModifiedDate, h.ModifiedTime = timeToMsDosTime(t);

}

 
// Unix constants. The specification doesn't mention them,
// but these seem to be the values agreed on by tools.
private static readonly nuint s_IFMT = 0xf000;
private static readonly nuint s_IFSOCK = 0xc000;
private static readonly nuint s_IFLNK = 0xa000;
private static readonly nuint s_IFREG = 0x8000;
private static readonly nuint s_IFBLK = 0x6000;
private static readonly nuint s_IFDIR = 0x4000;
private static readonly nuint s_IFCHR = 0x2000;
private static readonly nuint s_IFIFO = 0x1000;
private static readonly nuint s_ISUID = 0x800;
private static readonly nuint s_ISGID = 0x400;
private static readonly nuint s_ISVTX = 0x200;

private static readonly nuint msdosDir = 0x10;
private static readonly nuint msdosReadOnly = 0x01;


// Mode returns the permission and mode bits for the FileHeader.
private static fs.FileMode Mode(this ptr<FileHeader> _addr_h) {
    fs.FileMode mode = default;
    ref FileHeader h = ref _addr_h.val;


    if (h.CreatorVersion >> 8 == creatorUnix || h.CreatorVersion >> 8 == creatorMacOSX) 
        mode = unixModeToFileMode(h.ExternalAttrs >> 16);
    else if (h.CreatorVersion >> 8 == creatorNTFS || h.CreatorVersion >> 8 == creatorVFAT || h.CreatorVersion >> 8 == creatorFAT) 
        mode = msdosModeToFileMode(h.ExternalAttrs);
        if (len(h.Name) > 0 && h.Name[len(h.Name) - 1] == '/') {
        mode |= fs.ModeDir;
    }
    return mode;

}

// SetMode changes the permission and mode bits for the FileHeader.
private static void SetMode(this ptr<FileHeader> _addr_h, fs.FileMode mode) {
    ref FileHeader h = ref _addr_h.val;

    h.CreatorVersion = h.CreatorVersion & 0xff | creatorUnix << 8;
    h.ExternalAttrs = fileModeToUnixMode(mode) << 16; 

    // set MSDOS attributes too, as the original zip does.
    if (mode & fs.ModeDir != 0) {
        h.ExternalAttrs |= msdosDir;
    }
    if (mode & 0200 == 0) {
        h.ExternalAttrs |= msdosReadOnly;
    }
}

// isZip64 reports whether the file size exceeds the 32 bit limit
private static bool isZip64(this ptr<FileHeader> _addr_h) {
    ref FileHeader h = ref _addr_h.val;

    return h.CompressedSize64 >= uint32max || h.UncompressedSize64 >= uint32max;
}

private static bool hasDataDescriptor(this ptr<FileHeader> _addr_f) {
    ref FileHeader f = ref _addr_f.val;

    return f.Flags & 0x8 != 0;
}

private static fs.FileMode msdosModeToFileMode(uint m) {
    fs.FileMode mode = default;

    if (m & msdosDir != 0) {
        mode = fs.ModeDir | 0777;
    }
    else
 {
        mode = 0666;
    }
    if (m & msdosReadOnly != 0) {
        mode &= 0222;
    }
    return mode;

}

private static uint fileModeToUnixMode(fs.FileMode mode) {
    uint m = default;

    if (mode & fs.ModeType == fs.ModeDir) 
        m = s_IFDIR;
    else if (mode & fs.ModeType == fs.ModeSymlink) 
        m = s_IFLNK;
    else if (mode & fs.ModeType == fs.ModeNamedPipe) 
        m = s_IFIFO;
    else if (mode & fs.ModeType == fs.ModeSocket) 
        m = s_IFSOCK;
    else if (mode & fs.ModeType == fs.ModeDevice) 
        m = s_IFBLK;
    else if (mode & fs.ModeType == fs.ModeDevice | fs.ModeCharDevice) 
        m = s_IFCHR;
    else 
        m = s_IFREG;
        if (mode & fs.ModeSetuid != 0) {
        m |= s_ISUID;
    }
    if (mode & fs.ModeSetgid != 0) {
        m |= s_ISGID;
    }
    if (mode & fs.ModeSticky != 0) {
        m |= s_ISVTX;
    }
    return m | uint32(mode & 0777);

}

private static fs.FileMode unixModeToFileMode(uint m) {
    var mode = fs.FileMode(m & 0777);

    if (m & s_IFMT == s_IFBLK) 
        mode |= fs.ModeDevice;
    else if (m & s_IFMT == s_IFCHR) 
        mode |= fs.ModeDevice | fs.ModeCharDevice;
    else if (m & s_IFMT == s_IFDIR) 
        mode |= fs.ModeDir;
    else if (m & s_IFMT == s_IFIFO) 
        mode |= fs.ModeNamedPipe;
    else if (m & s_IFMT == s_IFLNK) 
        mode |= fs.ModeSymlink;
    else if (m & s_IFMT == s_IFREG)     else if (m & s_IFMT == s_IFSOCK) 
        mode |= fs.ModeSocket;
        if (m & s_ISGID != 0) {
        mode |= fs.ModeSetgid;
    }
    if (m & s_ISUID != 0) {
        mode |= fs.ModeSetuid;
    }
    if (m & s_ISVTX != 0) {
        mode |= fs.ModeSticky;
    }
    return mode;

}

// dataDescriptor holds the data descriptor that optionally follows the file
// contents in the zip file.
private partial struct dataDescriptor {
    public uint crc32;
    public ulong compressedSize;
    public ulong uncompressedSize;
}

} // end zip_package
