// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package zip -- go2cs converted at 2022 March 06 22:31:41 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Program Files\Go\src\archive\zip\reader.go
using bufio = go.bufio_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;
using crc32 = go.hash.crc32_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using path = go.path_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using System;


namespace go.archive;

public static partial class zip_package {

public static var ErrFormat = errors.New("zip: not a valid zip file");public static var ErrAlgorithm = errors.New("zip: unsupported compression algorithm");public static var ErrChecksum = errors.New("zip: checksum error");

// A Reader serves content from a ZIP archive.
public partial struct Reader {
    public io.ReaderAt r;
    public slice<ptr<File>> File;
    public @string Comment;
    public map<ushort, Decompressor> decompressors; // fileList is a list of files sorted by ename,
// for use by the Open method.
    public sync.Once fileListOnce;
    public slice<fileListEntry> fileList;
}

// A ReadCloser is a Reader that must be closed when no longer needed.
public partial struct ReadCloser : Reader {
    public ptr<os.File> f;
    public Reader Reader;
}

// A File is a single file in a ZIP archive.
// The file information is in the embedded FileHeader.
// The file content can be accessed by calling Open.
public partial struct File {
    public ref FileHeader FileHeader => ref FileHeader_val;
    public ptr<Reader> zip;
    public io.ReaderAt zipr;
    public long headerOffset;
    public bool zip64; // zip64 extended information extra field presence
    public error descErr; // error reading the data descriptor during init
}

// OpenReader will open the Zip file specified by name and return a ReadCloser.
public static (ptr<ReadCloser>, error) OpenReader(@string name) {
    ptr<ReadCloser> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (fi, err) = f.Stat();
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    ptr<ReadCloser> r = @new<ReadCloser>();
    {
        var err = r.init(f, fi.Size());

        if (err != null) {
            f.Close();
            return (_addr_null!, error.As(err)!);
        }
    }

    r.f = f;
    return (_addr_r!, error.As(null!)!);

}

// NewReader returns a new Reader reading from r, which is assumed to
// have the given size in bytes.
public static (ptr<Reader>, error) NewReader(io.ReaderAt r, long size) {
    ptr<Reader> _p0 = default!;
    error _p0 = default!;

    if (size < 0) {
        return (_addr_null!, error.As(errors.New("zip: size cannot be negative"))!);
    }
    ptr<Reader> zr = @new<Reader>();
    {
        var err = zr.init(r, size);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    return (_addr_zr!, error.As(null!)!);

}

private static error init(this ptr<Reader> _addr_z, io.ReaderAt r, long size) {
    ref Reader z = ref _addr_z.val;

    var (end, err) = readDirectoryEnd(r, size);
    if (err != null) {
        return error.As(err)!;
    }
    z.r = r; 
    // Since the number of directory records is not validated, it is not
    // safe to preallocate z.File without first checking that the specified
    // number of files is reasonable, since a malformed archive may
    // indicate it contains up to 1 << 128 - 1 files. Since each file has a
    // header which will be _at least_ 30 bytes we can safely preallocate
    // if (data size / 30) >= end.directoryRecords.
    if (end.directorySize < uint64(size) && (uint64(size) - end.directorySize) / 30 >= end.directoryRecords) {
        z.File = make_slice<ptr<File>>(0, end.directoryRecords);
    }
    z.Comment = end.comment;
    var rs = io.NewSectionReader(r, 0, size);
    _, err = rs.Seek(int64(end.directoryOffset), io.SeekStart);

    if (err != null) {
        return error.As(err)!;
    }
    var buf = bufio.NewReader(rs); 

    // The count of files inside a zip is truncated to fit in a uint16.
    // Gloss over this by reading headers until we encounter
    // a bad one, and then only report an ErrFormat or UnexpectedEOF if
    // the file count modulo 65536 is incorrect.
    while (true) {
        ptr<File> f = addr(new File(zip:z,zipr:r));
        err = readDirectoryHeader(f, buf);
        if (err == ErrFormat || err == io.ErrUnexpectedEOF) {
            break;
        }
        if (err != null) {
            return error.As(err)!;
        }
        f.readDataDescriptor();
        z.File = append(z.File, f);

    }
    if (uint16(len(z.File)) != uint16(end.directoryRecords)) { // only compare 16 bits here
        // Return the readDirectoryHeader error if we read
        // the wrong number of directory entries.
        return error.As(err)!;

    }
    return error.As(null!)!;

}

// RegisterDecompressor registers or overrides a custom decompressor for a
// specific method ID. If a decompressor for a given method is not found,
// Reader will default to looking up the decompressor at the package level.
private static void RegisterDecompressor(this ptr<Reader> _addr_z, ushort method, Decompressor dcomp) {
    ref Reader z = ref _addr_z.val;

    if (z.decompressors == null) {
        z.decompressors = make_map<ushort, Decompressor>();
    }
    z.decompressors[method] = dcomp;

}

private static Decompressor decompressor(this ptr<Reader> _addr_z, ushort method) {
    ref Reader z = ref _addr_z.val;

    var dcomp = z.decompressors[method];
    if (dcomp == null) {
        dcomp = decompressor(method);
    }
    return dcomp;

}

// Close closes the Zip file, rendering it unusable for I/O.
private static error Close(this ptr<ReadCloser> _addr_rc) {
    ref ReadCloser rc = ref _addr_rc.val;

    return error.As(rc.f.Close())!;
}

// DataOffset returns the offset of the file's possibly-compressed
// data, relative to the beginning of the zip file.
//
// Most callers should instead use Open, which transparently
// decompresses data and verifies checksums.
private static (long, error) DataOffset(this ptr<File> _addr_f) {
    long offset = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    var (bodyOffset, err) = f.findBodyOffset();
    if (err != null) {
        return ;
    }
    return (f.headerOffset + bodyOffset, error.As(null!)!);

}

// Open returns a ReadCloser that provides access to the File's contents.
// Multiple files may be read concurrently.
private static (io.ReadCloser, error) Open(this ptr<File> _addr_f) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    var (bodyOffset, err) = f.findBodyOffset();
    if (err != null) {
        return (null, error.As(err)!);
    }
    var size = int64(f.CompressedSize64);
    var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset, size);
    var dcomp = f.zip.decompressor(f.Method);
    if (dcomp == null) {
        return (null, error.As(ErrAlgorithm)!);
    }
    io.ReadCloser rc = dcomp(r);
    rc = addr(new checksumReader(rc:rc,hash:crc32.NewIEEE(),f:f,));
    return (rc, error.As(null!)!);

}

// OpenRaw returns a Reader that provides access to the File's contents without
// decompression.
private static (io.Reader, error) OpenRaw(this ptr<File> _addr_f) {
    io.Reader _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    var (bodyOffset, err) = f.findBodyOffset();
    if (err != null) {
        return (null, error.As(err)!);
    }
    var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset, int64(f.CompressedSize64));
    return (r, error.As(null!)!);

}

private static void readDataDescriptor(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    if (!f.hasDataDescriptor()) {
        return ;
    }
    var (bodyOffset, err) = f.findBodyOffset();
    if (err != null) {
        f.descErr = err;
        return ;
    }
    var zip64 = f.zip64 || f.isZip64();
    var n = int64(dataDescriptorLen);
    if (zip64) {
        n = dataDescriptor64Len;
    }
    var size = int64(f.CompressedSize64);
    var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset + size, n);
    var (dd, err) = readDataDescriptor(r, zip64);
    if (err != null) {
        f.descErr = err;
        return ;
    }
    f.CRC32 = dd.crc32;

}

private partial struct checksumReader {
    public io.ReadCloser rc;
    public hash.Hash32 hash;
    public ulong nread; // number of bytes read so far
    public ptr<File> f;
    public error err; // sticky error
}

private static (fs.FileInfo, error) Stat(this ptr<checksumReader> _addr_r) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref checksumReader r = ref _addr_r.val;

    return (new headerFileInfo(&r.f.FileHeader), error.As(null!)!);
}

private static (nint, error) Read(this ptr<checksumReader> _addr_r, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref checksumReader r = ref _addr_r.val;

    if (r.err != null) {
        return (0, error.As(r.err)!);
    }
    n, err = r.rc.Read(b);
    r.hash.Write(b[..(int)n]);
    r.nread += uint64(n);
    if (err == null) {
        return ;
    }
    if (err == io.EOF) {
        if (r.nread != r.f.UncompressedSize64) {
            return (0, error.As(io.ErrUnexpectedEOF)!);
        }
        if (r.f.hasDataDescriptor()) {
            if (r.f.descErr != null) {
                if (r.f.descErr == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                else
 {
                    err = r.f.descErr;
                }

            }
            else if (r.hash.Sum32() != r.f.CRC32) {
                err = ErrChecksum;
            }

        }
        else
 { 
            // If there's not a data descriptor, we still compare
            // the CRC32 of what we've read against the file header
            // or TOC's CRC32, if it seems like it was set.
            if (r.f.CRC32 != 0 && r.hash.Sum32() != r.f.CRC32) {
                err = ErrChecksum;
            }

        }
    }
    r.err = err;
    return ;

}

private static error Close(this ptr<checksumReader> _addr_r) {
    ref checksumReader r = ref _addr_r.val;

    return error.As(r.rc.Close())!;
}

// findBodyOffset does the minimum work to verify the file has a header
// and returns the file body offset.
private static (long, error) findBodyOffset(this ptr<File> _addr_f) {
    long _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    array<byte> buf = new array<byte>(fileHeaderLen);
    {
        var (_, err) = f.zipr.ReadAt(buf[..], f.headerOffset);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    var b = readBuf(buf[..]);
    {
        var sig = b.uint32();

        if (sig != fileHeaderSignature) {
            return (0, error.As(ErrFormat)!);
        }
    }

    b = b[(int)22..]; // skip over most of the header
    var filenameLen = int(b.uint16());
    var extraLen = int(b.uint16());
    return (int64(fileHeaderLen + filenameLen + extraLen), error.As(null!)!);

}

// readDirectoryHeader attempts to read a directory header from r.
// It returns io.ErrUnexpectedEOF if it cannot read a complete header,
// and ErrFormat if it doesn't find a valid header signature.
private static error readDirectoryHeader(ptr<File> _addr_f, io.Reader r) {
    ref File f = ref _addr_f.val;

    array<byte> buf = new array<byte>(directoryHeaderLen);
    {
        var (_, err) = io.ReadFull(r, buf[..]);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var b = readBuf(buf[..]);
    {
        var sig = b.uint32();

        if (sig != directoryHeaderSignature) {
            return error.As(ErrFormat)!;
        }
    }

    f.CreatorVersion = b.uint16();
    f.ReaderVersion = b.uint16();
    f.Flags = b.uint16();
    f.Method = b.uint16();
    f.ModifiedTime = b.uint16();
    f.ModifiedDate = b.uint16();
    f.CRC32 = b.uint32();
    f.CompressedSize = b.uint32();
    f.UncompressedSize = b.uint32();
    f.CompressedSize64 = uint64(f.CompressedSize);
    f.UncompressedSize64 = uint64(f.UncompressedSize);
    var filenameLen = int(b.uint16());
    var extraLen = int(b.uint16());
    var commentLen = int(b.uint16());
    b = b[(int)4..]; // skipped start disk number and internal attributes (2x uint16)
    f.ExternalAttrs = b.uint32();
    f.headerOffset = int64(b.uint32());
    var d = make_slice<byte>(filenameLen + extraLen + commentLen);
    {
        (_, err) = io.ReadFull(r, d);

        if (err != null) {
            return error.As(err)!;
        }
    }

    f.Name = string(d[..(int)filenameLen]);
    f.Extra = d[(int)filenameLen..(int)filenameLen + extraLen];
    f.Comment = string(d[(int)filenameLen + extraLen..]); 

    // Determine the character encoding.
    var (utf8Valid1, utf8Require1) = detectUTF8(f.Name);
    var (utf8Valid2, utf8Require2) = detectUTF8(f.Comment);

    if (!utf8Valid1 || !utf8Valid2) 
        // Name and Comment definitely not UTF-8.
        f.NonUTF8 = true;
    else if (!utf8Require1 && !utf8Require2) 
        // Name and Comment use only single-byte runes that overlap with UTF-8.
        f.NonUTF8 = false;
    else 
        // Might be UTF-8, might be some other encoding; preserve existing flag.
        // Some ZIP writers use UTF-8 encoding without setting the UTF-8 flag.
        // Since it is impossible to always distinguish valid UTF-8 from some
        // other encoding (e.g., GBK or Shift-JIS), we trust the flag.
        f.NonUTF8 = f.Flags & 0x800 == 0;
        var needUSize = f.UncompressedSize == ~uint32(0);
    var needCSize = f.CompressedSize == ~uint32(0);
    var needHeaderOffset = f.headerOffset == int64(~uint32(0)); 

    // Best effort to find what we need.
    // Other zip authors might not even follow the basic format,
    // and we'll just ignore the Extra content in that case.
    time.Time modified = default;
parseExtras:

    {
        var extra = readBuf(f.Extra);

        while (len(extra) >= 4) { // need at least tag and size
            var fieldTag = extra.uint16();
            var fieldSize = int(extra.uint16());
            if (len(extra) < fieldSize) {
                break;
            }

            var fieldBuf = extra.sub(fieldSize);


            if (fieldTag == zip64ExtraID) 
                f.zip64 = true; 

                // update directory values from the zip64 extra block.
                // They should only be consulted if the sizes read earlier
                // are maxed out.
                // See golang.org/issue/13367.
                if (needUSize) {
                    needUSize = false;
                    if (len(fieldBuf) < 8) {
                        return error.As(ErrFormat)!;
                    }
                    f.UncompressedSize64 = fieldBuf.uint64();
                }

                if (needCSize) {
                    needCSize = false;
                    if (len(fieldBuf) < 8) {
                        return error.As(ErrFormat)!;
                    }
                    f.CompressedSize64 = fieldBuf.uint64();
                }

                if (needHeaderOffset) {
                    needHeaderOffset = false;
                    if (len(fieldBuf) < 8) {
                        return error.As(ErrFormat)!;
                    }
                    f.headerOffset = int64(fieldBuf.uint64());
                }

            else if (fieldTag == ntfsExtraID) 
                if (len(fieldBuf) < 4) {
                    _continueparseExtras = true;
                    break;
                }

                fieldBuf.uint32(); // reserved (ignored)
                while (len(fieldBuf) >= 4) { // need at least tag and size
                    var attrTag = fieldBuf.uint16();
                    var attrSize = int(fieldBuf.uint16());
                    if (len(fieldBuf) < attrSize) {
                        _continueparseExtras = true;
                        break;
                    }

                    var attrBuf = fieldBuf.sub(attrSize);
                    if (attrTag != 1 || attrSize != 24) {
                        continue; // Ignore irrelevant attributes
                    }

                    const float ticksPerSecond = 1e7F; // Windows timestamp resolution
 // Windows timestamp resolution
                    var ts = int64(attrBuf.uint64()); // ModTime since Windows epoch
                    var secs = int64(ts / ticksPerSecond);
                    float nsecs = (1e9F / ticksPerSecond) * int64(ts % ticksPerSecond);
                    var epoch = time.Date(1601, time.January, 1, 0, 0, 0, 0, time.UTC);
                    modified = time.Unix(epoch.Unix() + secs, nsecs);

                }
            else if (fieldTag == unixExtraID || fieldTag == infoZipUnixExtraID) 
                if (len(fieldBuf) < 8) {
                    _continueparseExtras = true;
                    break;
                }

                fieldBuf.uint32(); // AcTime (ignored)
                ts = int64(fieldBuf.uint32()); // ModTime since Unix epoch
                modified = time.Unix(ts, 0);
            else if (fieldTag == extTimeExtraID) 
                if (len(fieldBuf) < 5 || fieldBuf.uint8() & 1 == 0) {
                    _continueparseExtras = true;
                    break;
                }

                ts = int64(fieldBuf.uint32()); // ModTime since Unix epoch
                modified = time.Unix(ts, 0);
            
        }
    }
    var msdosModified = msDosTimeToTime(f.ModifiedDate, f.ModifiedTime);
    f.Modified = msdosModified;
    if (!modified.IsZero()) {
        f.Modified = modified.UTC(); 

        // If legacy MS-DOS timestamps are set, we can use the delta between
        // the legacy and extended versions to estimate timezone offset.
        //
        // A non-UTC timezone is always used (even if offset is zero).
        // Thus, FileHeader.Modified.Location() == time.UTC is useful for
        // determining whether extended timestamps are present.
        // This is necessary for users that need to do additional time
        // calculations when dealing with legacy ZIP formats.
        if (f.ModifiedTime != 0 || f.ModifiedDate != 0) {
            f.Modified = modified.In(timeZone(msdosModified.Sub(modified)));
        }
    }
    _ = needUSize;

    if (needCSize || needHeaderOffset) {
        return error.As(ErrFormat)!;
    }
    return error.As(null!)!;

}

private static (ptr<dataDescriptor>, error) readDataDescriptor(io.Reader r, bool zip64) {
    ptr<dataDescriptor> _p0 = default!;
    error _p0 = default!;
 
    // Create enough space for the largest possible size
    array<byte> buf = new array<byte>(dataDescriptor64Len); 

    // The spec says: "Although not originally assigned a
    // signature, the value 0x08074b50 has commonly been adopted
    // as a signature value for the data descriptor record.
    // Implementers should be aware that ZIP files may be
    // encountered with or without this signature marking data
    // descriptors and should account for either case when reading
    // ZIP files to ensure compatibility."
    //
    // First read just those 4 bytes to see if the signature exists.
    {
        var (_, err) = io.ReadFull(r, buf[..(int)4]);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    nint off = 0;
    var maybeSig = readBuf(buf[..(int)4]);
    if (maybeSig.uint32() != dataDescriptorSignature) { 
        // No data descriptor signature. Keep these four
        // bytes.
        off += 4;

    }
    var end = dataDescriptorLen - 4;
    if (zip64) {
        end = dataDescriptor64Len - 4;
    }
    {
        (_, err) = io.ReadFull(r, buf[(int)off..(int)end]);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    var b = readBuf(buf[..(int)end]);

    ptr<dataDescriptor> @out = addr(new dataDescriptor(crc32:b.uint32(),));

    if (zip64) {
        @out.compressedSize = b.uint64();
        @out.uncompressedSize = b.uint64();
    }
    else
 {
        @out.compressedSize = uint64(b.uint32());
        @out.uncompressedSize = uint64(b.uint32());
    }
    return (_addr_out!, error.As(null!)!);

}

private static (ptr<directoryEnd>, error) readDirectoryEnd(io.ReaderAt r, long size) {
    ptr<directoryEnd> dir = default!;
    error err = default!;
 
    // look for directoryEndSignature in the last 1k, then in the last 65k
    slice<byte> buf = default;
    long directoryEndOffset = default;
    foreach (var (i, bLen) in new slice<long>(new long[] { 1024, 65*1024 })) {
        if (bLen > size) {
            bLen = size;
        }
        buf = make_slice<byte>(int(bLen));
        {
            var (_, err) = r.ReadAt(buf, size - bLen);

            if (err != null && err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }

        }

        {
            var p__prev1 = p;

            var p = findSignatureInBlock(buf);

            if (p >= 0) {
                buf = buf[(int)p..];
                directoryEndOffset = size - bLen + int64(p);
                break;
            }

            p = p__prev1;

        }

        if (i == 1 || bLen == size) {
            return (_addr_null!, error.As(ErrFormat)!);
        }
    }    var b = readBuf(buf[(int)4..]); // skip signature
    ptr<directoryEnd> d = addr(new directoryEnd(diskNbr:uint32(b.uint16()),dirDiskNbr:uint32(b.uint16()),dirRecordsThisDisk:uint64(b.uint16()),directoryRecords:uint64(b.uint16()),directorySize:uint64(b.uint32()),directoryOffset:uint64(b.uint32()),commentLen:b.uint16(),));
    var l = int(d.commentLen);
    if (l > len(b)) {
        return (_addr_null!, error.As(errors.New("zip: invalid comment length"))!);
    }
    d.comment = string(b[..(int)l]); 

    // These values mean that the file can be a zip64 file
    if (d.directoryRecords == 0xffff || d.directorySize == 0xffff || d.directoryOffset == 0xffffffff) {
        var (p, err) = findDirectory64End(r, directoryEndOffset);
        if (err == null && p >= 0) {
            err = readDirectory64End(r, p, d);
        }
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    {
        var o = int64(d.directoryOffset);

        if (o < 0 || o >= size) {
            return (_addr_null!, error.As(ErrFormat)!);
        }
    }

    return (_addr_d!, error.As(null!)!);

}

// findDirectory64End tries to read the zip64 locator just before the
// directory end and returns the offset of the zip64 directory end if
// found.
private static (long, error) findDirectory64End(io.ReaderAt r, long directoryEndOffset) {
    long _p0 = default;
    error _p0 = default!;

    var locOffset = directoryEndOffset - directory64LocLen;
    if (locOffset < 0) {
        return (-1, error.As(null!)!); // no need to look for a header outside the file
    }
    var buf = make_slice<byte>(directory64LocLen);
    {
        var (_, err) = r.ReadAt(buf, locOffset);

        if (err != null) {
            return (-1, error.As(err)!);
        }
    }

    var b = readBuf(buf);
    {
        var sig = b.uint32();

        if (sig != directory64LocSignature) {
            return (-1, error.As(null!)!);
        }
    }

    if (b.uint32() != 0) { // number of the disk with the start of the zip64 end of central directory
        return (-1, error.As(null!)!); // the file is not a valid zip64-file
    }
    var p = b.uint64(); // relative offset of the zip64 end of central directory record
    if (b.uint32() != 1) { // total number of disks
        return (-1, error.As(null!)!); // the file is not a valid zip64-file
    }
    return (int64(p), error.As(null!)!);

}

// readDirectory64End reads the zip64 directory end and updates the
// directory end with the zip64 directory end values.
private static error readDirectory64End(io.ReaderAt r, long offset, ptr<directoryEnd> _addr_d) {
    error err = default!;
    ref directoryEnd d = ref _addr_d.val;

    var buf = make_slice<byte>(directory64EndLen);
    {
        var (_, err) = r.ReadAt(buf, offset);

        if (err != null) {
            return error.As(err)!;
        }
    }


    var b = readBuf(buf);
    {
        var sig = b.uint32();

        if (sig != directory64EndSignature) {
            return error.As(ErrFormat)!;
        }
    }


    b = b[(int)12..]; // skip dir size, version and version needed (uint64 + 2x uint16)
    d.diskNbr = b.uint32(); // number of this disk
    d.dirDiskNbr = b.uint32(); // number of the disk with the start of the central directory
    d.dirRecordsThisDisk = b.uint64(); // total number of entries in the central directory on this disk
    d.directoryRecords = b.uint64(); // total number of entries in the central directory
    d.directorySize = b.uint64(); // size of the central directory
    d.directoryOffset = b.uint64(); // offset of start of central directory with respect to the starting disk number

    return error.As(null!)!;

}

private static nint findSignatureInBlock(slice<byte> b) {
    for (var i = len(b) - directoryEndLen; i >= 0; i--) { 
        // defined from directoryEndSignature in struct.go
        if (b[i] == 'P' && b[i + 1] == 'K' && b[i + 2] == 0x05 && b[i + 3] == 0x06) { 
            // n is length of comment
            var n = int(b[i + directoryEndLen - 2]) | int(b[i + directoryEndLen - 1]) << 8;
            if (n + directoryEndLen + i <= len(b)) {
                return i;
            }

        }
    }
    return -1;

}

private partial struct readBuf { // : slice<byte>
}

private static byte uint8(this ptr<readBuf> _addr_b) {
    ref readBuf b = ref _addr_b.val;

    var v = (b.val)[0];
    b.val = (b.val)[(int)1..];
    return v;
}

private static ushort uint16(this ptr<readBuf> _addr_b) {
    ref readBuf b = ref _addr_b.val;

    var v = binary.LittleEndian.Uint16(b.val);
    b.val = (b.val)[(int)2..];
    return v;
}

private static uint uint32(this ptr<readBuf> _addr_b) {
    ref readBuf b = ref _addr_b.val;

    var v = binary.LittleEndian.Uint32(b.val);
    b.val = (b.val)[(int)4..];
    return v;
}

private static ulong uint64(this ptr<readBuf> _addr_b) {
    ref readBuf b = ref _addr_b.val;

    var v = binary.LittleEndian.Uint64(b.val);
    b.val = (b.val)[(int)8..];
    return v;
}

private static readBuf sub(this ptr<readBuf> _addr_b, nint n) {
    ref readBuf b = ref _addr_b.val;

    var b2 = (b.val)[..(int)n];
    b.val = (b.val)[(int)n..];
    return b2;
}

// A fileListEntry is a File and its ename.
// If file == nil, the fileListEntry describes a directory without metadata.
private partial struct fileListEntry {
    public @string name;
    public ptr<File> file;
    public bool isDir;
}

private partial interface fileInfoDirEntry {
}

private static fileInfoDirEntry stat(this ptr<fileListEntry> _addr_e) {
    ref fileListEntry e = ref _addr_e.val;

    if (!e.isDir) {
        return new headerFileInfo(&e.file.FileHeader);
    }
    return e;

}

// Only used for directories.
private static @string Name(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    var (_, elem, _) = split(f.name);

    return elem;
}
private static long Size(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    return 0;
}
private static fs.FileMode Mode(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    return fs.ModeDir | 0555;
}
private static fs.FileMode Type(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    return fs.ModeDir;
}
private static bool IsDir(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    return true;
}
private static void Sys(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    return null;
}

private static time.Time ModTime(this ptr<fileListEntry> _addr_f) {
    ref fileListEntry f = ref _addr_f.val;

    if (f.file == null) {
        return new time.Time();
    }
    return f.file.FileHeader.Modified.UTC();

}

private static (fs.FileInfo, error) Info(this ptr<fileListEntry> _addr_f) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref fileListEntry f = ref _addr_f.val;

    return (f, error.As(null!)!);
}

// toValidName coerces name to be a valid name for fs.FS.Open.
private static @string toValidName(@string name) {
    name = strings.ReplaceAll(name, "\\", "/");
    var p = path.Clean(name);
    if (strings.HasPrefix(p, "/")) {
        p = p[(int)len("/")..];
    }
    while (strings.HasPrefix(p, "../")) {
        p = p[(int)len("../")..];
    }
    return p;

}

private static void initFileList(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    r.fileListOnce.Do(() => {
        var dirs = make_map<@string, bool>();
        var knownDirs = make_map<@string, bool>();
        foreach (var (_, file) in r.File) {
            var isDir = len(file.Name) > 0 && file.Name[len(file.Name) - 1] == '/';
            var name = toValidName(file.Name);
            if (name == "") {
                continue;
            }
            {
                var dir__prev2 = dir;

                var dir = path.Dir(name);

                while (dir != ".") {
                    dirs[dir] = true;
                    dir = path.Dir(dir);
                }


                dir = dir__prev2;
            }
            fileListEntry entry = new fileListEntry(name:name,file:file,isDir:isDir,);
            r.fileList = append(r.fileList, entry);
            if (isDir) {
                knownDirs[name] = true;
            }

        }        {
            var dir__prev1 = dir;

            foreach (var (__dir) in dirs) {
                dir = __dir;
                if (!knownDirs[dir]) {
                    entry = new fileListEntry(name:dir,file:nil,isDir:true,);
                    r.fileList = append(r.fileList, entry);
                }
            }

            dir = dir__prev1;
        }

        sort.Slice(r.fileList, (i, j) => fileEntryLess(r.fileList[i].name, r.fileList[j].name));

    });

}

private static bool fileEntryLess(@string x, @string y) {
    var (xdir, xelem, _) = split(x);
    var (ydir, yelem, _) = split(y);
    return xdir < ydir || xdir == ydir && xelem < yelem;
}

// Open opens the named file in the ZIP archive,
// using the semantics of fs.FS.Open:
// paths are always slash separated, with no
// leading / or ../ elements.
private static (fs.File, error) Open(this ptr<Reader> _addr_r, @string name) {
    fs.File _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    r.initFileList();

    if (!fs.ValidPath(name)) {
        return (null, error.As(addr(new fs.PathError(Op:"open",Path:name,Err:fs.ErrInvalid))!)!);
    }
    var e = r.openLookup(name);
    if (e == null) {
        return (null, error.As(addr(new fs.PathError(Op:"open",Path:name,Err:fs.ErrNotExist))!)!);
    }
    if (e.isDir) {
        return (addr(new openDir(e,r.openReadDir(name),0)), error.As(null!)!);
    }
    var (rc, err) = e.file.Open();
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (rc._<fs.File>(), error.As(null!)!);

}

private static (@string, @string, bool) split(@string name) {
    @string dir = default;
    @string elem = default;
    bool isDir = default;

    if (len(name) > 0 && name[len(name) - 1] == '/') {
        isDir = true;
        name = name[..(int)len(name) - 1];
    }
    var i = len(name) - 1;
    while (i >= 0 && name[i] != '/') {
        i--;
    }
    if (i < 0) {
        return (".", name, isDir);
    }
    return (name[..(int)i], name[(int)i + 1..], isDir);

}

private static ptr<fileListEntry> dotFile = addr(new fileListEntry(name:"./",isDir:true));

private static ptr<fileListEntry> openLookup(this ptr<Reader> _addr_r, @string name) {
    ref Reader r = ref _addr_r.val;

    if (name == ".") {
        return _addr_dotFile!;
    }
    var (dir, elem, _) = split(name);
    var files = r.fileList;
    var i = sort.Search(len(files), i => {
        var (idir, ielem, _) = split(files[i].name);
        return _addr_idir > dir || idir == dir && ielem >= elem!;
    });
    if (i < len(files)) {
        var fname = files[i].name;
        if (fname == name || len(fname) == len(name) + 1 && fname[len(name)] == '/' && fname[..(int)len(name)] == name) {
            return _addr__addr_files[i]!;
        }
    }
    return _addr_null!;

}

private static slice<fileListEntry> openReadDir(this ptr<Reader> _addr_r, @string dir) {
    ref Reader r = ref _addr_r.val;

    var files = r.fileList;
    var i = sort.Search(len(files), i => {
        var (idir, _, _) = split(files[i].name);
        return idir >= dir;
    });
    var j = sort.Search(len(files), j => {
        var (jdir, _, _) = split(files[j].name);
        return jdir > dir;
    });
    return files[(int)i..(int)j];
}

private partial struct openDir {
    public ptr<fileListEntry> e;
    public slice<fileListEntry> files;
    public nint offset;
}

private static error Close(this ptr<openDir> _addr_d) {
    ref openDir d = ref _addr_d.val;

    return error.As(null!)!;
}
private static (fs.FileInfo, error) Stat(this ptr<openDir> _addr_d) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    return (d.e.stat(), error.As(null!)!);
}

private static (nint, error) Read(this ptr<openDir> _addr_d, slice<byte> _p0) {
    nint _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    return (0, error.As(addr(new fs.PathError(Op:"read",Path:d.e.name,Err:errors.New("is a directory")))!)!);
}

private static (slice<fs.DirEntry>, error) ReadDir(this ptr<openDir> _addr_d, nint count) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    var n = len(d.files) - d.offset;
    if (count > 0 && n > count) {
        n = count;
    }
    if (n == 0) {
        if (count <= 0) {
            return (null, error.As(null!)!);
        }
        return (null, error.As(io.EOF)!);

    }
    var list = make_slice<fs.DirEntry>(n);
    foreach (var (i) in list) {
        list[i] = d.files[d.offset + i].stat();
    }    d.offset += n;
    return (list, error.As(null!)!);

}

} // end zip_package
