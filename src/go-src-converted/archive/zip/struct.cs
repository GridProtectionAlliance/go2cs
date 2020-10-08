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
// package zip -- go2cs converted at 2020 October 08 03:49:28 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Go\src\archive\zip\struct.go
using os = go.os_package;
using path = go.path_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class zip_package
    {
        // Compression methods.
        public static readonly ushort Store = (ushort)0L; // no compression
        public static readonly ushort Deflate = (ushort)8L; // DEFLATE compressed

        private static readonly ulong fileHeaderSignature = (ulong)0x04034b50UL;
        private static readonly ulong directoryHeaderSignature = (ulong)0x02014b50UL;
        private static readonly ulong directoryEndSignature = (ulong)0x06054b50UL;
        private static readonly ulong directory64LocSignature = (ulong)0x07064b50UL;
        private static readonly ulong directory64EndSignature = (ulong)0x06064b50UL;
        private static readonly ulong dataDescriptorSignature = (ulong)0x08074b50UL; // de-facto standard; required by OS X Finder
        private static readonly long fileHeaderLen = (long)30L; // + filename + extra
        private static readonly long directoryHeaderLen = (long)46L; // + filename + extra + comment
        private static readonly long directoryEndLen = (long)22L; // + comment
        private static readonly long dataDescriptorLen = (long)16L; // four uint32: descriptor signature, crc32, compressed size, size
        private static readonly long dataDescriptor64Len = (long)24L; // descriptor with 8 byte sizes
        private static readonly long directory64LocLen = (long)20L; //
        private static readonly long directory64EndLen = (long)56L; // + extra

        // Constants for the first byte in CreatorVersion.
        private static readonly long creatorFAT = (long)0L;
        private static readonly long creatorUnix = (long)3L;
        private static readonly long creatorNTFS = (long)11L;
        private static readonly long creatorVFAT = (long)14L;
        private static readonly long creatorMacOSX = (long)19L; 

        // Version numbers.
        private static readonly long zipVersion20 = (long)20L; // 2.0
        private static readonly long zipVersion45 = (long)45L; // 4.5 (reads and writes zip64 archives)

        // Limits for non zip64 files.
        private static readonly long uint16max = (long)(1L << (int)(16L)) - 1L;
        private static readonly long uint32max = (long)(1L << (int)(32L)) - 1L; 

        // Extra header IDs.
        //
        // IDs 0..31 are reserved for official use by PKWARE.
        // IDs above that range are defined by third-party vendors.
        // Since ZIP lacked high precision timestamps (nor a official specification
        // of the timezone used for the date fields), many competing extra fields
        // have been invented. Pervasive use effectively makes them "official".
        //
        // See http://mdfs.net/Docs/Comp/Archiving/Zip/ExtraField
        private static readonly ulong zip64ExtraID = (ulong)0x0001UL; // Zip64 extended information
        private static readonly ulong ntfsExtraID = (ulong)0x000aUL; // NTFS
        private static readonly ulong unixExtraID = (ulong)0x000dUL; // UNIX
        private static readonly ulong extTimeExtraID = (ulong)0x5455UL; // Extended timestamp
        private static readonly ulong infoZipUnixExtraID = (ulong)0x5855UL; // Info-ZIP Unix extension

        // FileHeader describes a file within a zip file.
        // See the zip spec for details.
        public partial struct FileHeader
        {
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

        // FileInfo returns an os.FileInfo for the FileHeader.
        private static os.FileInfo FileInfo(this ptr<FileHeader> _addr_h)
        {
            ref FileHeader h = ref _addr_h.val;

            return new headerFileInfo(h);
        }

        // headerFileInfo implements os.FileInfo.
        private partial struct headerFileInfo
        {
            public ptr<FileHeader> fh;
        }

        private static @string Name(this headerFileInfo fi)
        {
            return path.Base(fi.fh.Name);
        }
        private static long Size(this headerFileInfo fi)
        {
            if (fi.fh.UncompressedSize64 > 0L)
            {
                return int64(fi.fh.UncompressedSize64);
            }

            return int64(fi.fh.UncompressedSize);

        }
        private static bool IsDir(this headerFileInfo fi)
        {
            return fi.Mode().IsDir();
        }
        private static time.Time ModTime(this headerFileInfo fi)
        {
            if (fi.fh.Modified.IsZero())
            {
                return fi.fh.ModTime();
            }

            return fi.fh.Modified.UTC();

        }
        private static os.FileMode Mode(this headerFileInfo fi)
        {
            return fi.fh.Mode();
        }
        private static void Sys(this headerFileInfo fi)
        {
            return fi.fh;
        }

        // FileInfoHeader creates a partially-populated FileHeader from an
        // os.FileInfo.
        // Because os.FileInfo's Name method returns only the base name of
        // the file it describes, it may be necessary to modify the Name field
        // of the returned header to provide the full path name of the file.
        // If compression is desired, callers should set the FileHeader.Method
        // field; it is unset by default.
        public static (ptr<FileHeader>, error) FileInfoHeader(os.FileInfo fi)
        {
            ptr<FileHeader> _p0 = default!;
            error _p0 = default!;

            var size = fi.Size();
            ptr<FileHeader> fh = addr(new FileHeader(Name:fi.Name(),UncompressedSize64:uint64(size),));
            fh.SetModTime(fi.ModTime());
            fh.SetMode(fi.Mode());
            if (fh.UncompressedSize64 > uint32max)
            {
                fh.UncompressedSize = uint32max;
            }
            else
            {
                fh.UncompressedSize = uint32(fh.UncompressedSize64);
            }

            return (_addr_fh!, error.As(null!)!);

        }

        private partial struct directoryEnd
        {
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
        private static ptr<time.Location> timeZone(time.Duration offset)
        {
            const long minOffset = (long)-12L * time.Hour; // E.g., Baker island at -12:00
            const long maxOffset = (long)+14L * time.Hour; // E.g., Line island at +14:00
            const long offsetAlias = (long)15L * time.Minute; // E.g., Nepal at +5:45
            offset = offset.Round(offsetAlias);
            if (offset < minOffset || maxOffset < offset)
            {
                offset = 0L;
            }

            return _addr_time.FixedZone("", int(offset / time.Second))!;

        }

        // msDosTimeToTime converts an MS-DOS date and time into a time.Time.
        // The resolution is 2s.
        // See: https://msdn.microsoft.com/en-us/library/ms724247(v=VS.85).aspx
        private static time.Time msDosTimeToTime(ushort dosDate, ushort dosTime)
        {
            return time.Date(int(dosDate >> (int)(9L) + 1980L), time.Month(dosDate >> (int)(5L) & 0xfUL), int(dosDate & 0x1fUL), int(dosTime >> (int)(11L)), int(dosTime >> (int)(5L) & 0x3fUL), int(dosTime & 0x1fUL * 2L), 0L, time.UTC);
        }

        // timeToMsDosTime converts a time.Time to an MS-DOS date and time.
        // The resolution is 2s.
        // See: https://msdn.microsoft.com/en-us/library/ms724274(v=VS.85).aspx
        private static (ushort, ushort) timeToMsDosTime(time.Time t)
        {
            ushort fDate = default;
            ushort fTime = default;

            fDate = uint16(t.Day() + int(t.Month()) << (int)(5L) + (t.Year() - 1980L) << (int)(9L));
            fTime = uint16(t.Second() / 2L + t.Minute() << (int)(5L) + t.Hour() << (int)(11L));
            return ;
        }

        // ModTime returns the modification time in UTC using the legacy
        // ModifiedDate and ModifiedTime fields.
        //
        // Deprecated: Use Modified instead.
        private static time.Time ModTime(this ptr<FileHeader> _addr_h)
        {
            ref FileHeader h = ref _addr_h.val;

            return msDosTimeToTime(h.ModifiedDate, h.ModifiedTime);
        }

        // SetModTime sets the Modified, ModifiedTime, and ModifiedDate fields
        // to the given time in UTC.
        //
        // Deprecated: Use Modified instead.
        private static void SetModTime(this ptr<FileHeader> _addr_h, time.Time t)
        {
            ref FileHeader h = ref _addr_h.val;

            t = t.UTC(); // Convert to UTC for compatibility
            h.Modified = t;
            h.ModifiedDate, h.ModifiedTime = timeToMsDosTime(t);

        }

 
        // Unix constants. The specification doesn't mention them,
        // but these seem to be the values agreed on by tools.
        private static readonly ulong s_IFMT = (ulong)0xf000UL;
        private static readonly ulong s_IFSOCK = (ulong)0xc000UL;
        private static readonly ulong s_IFLNK = (ulong)0xa000UL;
        private static readonly ulong s_IFREG = (ulong)0x8000UL;
        private static readonly ulong s_IFBLK = (ulong)0x6000UL;
        private static readonly ulong s_IFDIR = (ulong)0x4000UL;
        private static readonly ulong s_IFCHR = (ulong)0x2000UL;
        private static readonly ulong s_IFIFO = (ulong)0x1000UL;
        private static readonly ulong s_ISUID = (ulong)0x800UL;
        private static readonly ulong s_ISGID = (ulong)0x400UL;
        private static readonly ulong s_ISVTX = (ulong)0x200UL;

        private static readonly ulong msdosDir = (ulong)0x10UL;
        private static readonly ulong msdosReadOnly = (ulong)0x01UL;


        // Mode returns the permission and mode bits for the FileHeader.
        private static os.FileMode Mode(this ptr<FileHeader> _addr_h)
        {
            os.FileMode mode = default;
            ref FileHeader h = ref _addr_h.val;


            if (h.CreatorVersion >> (int)(8L) == creatorUnix || h.CreatorVersion >> (int)(8L) == creatorMacOSX) 
                mode = unixModeToFileMode(h.ExternalAttrs >> (int)(16L));
            else if (h.CreatorVersion >> (int)(8L) == creatorNTFS || h.CreatorVersion >> (int)(8L) == creatorVFAT || h.CreatorVersion >> (int)(8L) == creatorFAT) 
                mode = msdosModeToFileMode(h.ExternalAttrs);
                        if (len(h.Name) > 0L && h.Name[len(h.Name) - 1L] == '/')
            {
                mode |= os.ModeDir;
            }

            return mode;

        }

        // SetMode changes the permission and mode bits for the FileHeader.
        private static void SetMode(this ptr<FileHeader> _addr_h, os.FileMode mode)
        {
            ref FileHeader h = ref _addr_h.val;

            h.CreatorVersion = h.CreatorVersion & 0xffUL | creatorUnix << (int)(8L);
            h.ExternalAttrs = fileModeToUnixMode(mode) << (int)(16L); 

            // set MSDOS attributes too, as the original zip does.
            if (mode & os.ModeDir != 0L)
            {
                h.ExternalAttrs |= msdosDir;
            }

            if (mode & 0200L == 0L)
            {
                h.ExternalAttrs |= msdosReadOnly;
            }

        }

        // isZip64 reports whether the file size exceeds the 32 bit limit
        private static bool isZip64(this ptr<FileHeader> _addr_h)
        {
            ref FileHeader h = ref _addr_h.val;

            return h.CompressedSize64 >= uint32max || h.UncompressedSize64 >= uint32max;
        }

        private static os.FileMode msdosModeToFileMode(uint m)
        {
            os.FileMode mode = default;

            if (m & msdosDir != 0L)
            {
                mode = os.ModeDir | 0777L;
            }
            else
            {
                mode = 0666L;
            }

            if (m & msdosReadOnly != 0L)
            {
                mode &= 0222L;
            }

            return mode;

        }

        private static uint fileModeToUnixMode(os.FileMode mode)
        {
            uint m = default;

            if (mode & os.ModeType == os.ModeDir) 
                m = s_IFDIR;
            else if (mode & os.ModeType == os.ModeSymlink) 
                m = s_IFLNK;
            else if (mode & os.ModeType == os.ModeNamedPipe) 
                m = s_IFIFO;
            else if (mode & os.ModeType == os.ModeSocket) 
                m = s_IFSOCK;
            else if (mode & os.ModeType == os.ModeDevice) 
                if (mode & os.ModeCharDevice != 0L)
                {
                    m = s_IFCHR;
                }
                else
                {
                    m = s_IFBLK;
                }

            else 
                m = s_IFREG;
                        if (mode & os.ModeSetuid != 0L)
            {
                m |= s_ISUID;
            }

            if (mode & os.ModeSetgid != 0L)
            {
                m |= s_ISGID;
            }

            if (mode & os.ModeSticky != 0L)
            {
                m |= s_ISVTX;
            }

            return m | uint32(mode & 0777L);

        }

        private static os.FileMode unixModeToFileMode(uint m)
        {
            var mode = os.FileMode(m & 0777L);

            if (m & s_IFMT == s_IFBLK) 
                mode |= os.ModeDevice;
            else if (m & s_IFMT == s_IFCHR) 
                mode |= os.ModeDevice | os.ModeCharDevice;
            else if (m & s_IFMT == s_IFDIR) 
                mode |= os.ModeDir;
            else if (m & s_IFMT == s_IFIFO) 
                mode |= os.ModeNamedPipe;
            else if (m & s_IFMT == s_IFLNK) 
                mode |= os.ModeSymlink;
            else if (m & s_IFMT == s_IFREG)             else if (m & s_IFMT == s_IFSOCK) 
                mode |= os.ModeSocket;
                        if (m & s_ISGID != 0L)
            {
                mode |= os.ModeSetgid;
            }

            if (m & s_ISUID != 0L)
            {
                mode |= os.ModeSetuid;
            }

            if (m & s_ISVTX != 0L)
            {
                mode |= os.ModeSticky;
            }

            return mode;

        }
    }
}}
