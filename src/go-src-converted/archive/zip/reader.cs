// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package zip -- go2cs converted at 2020 October 09 05:08:09 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Go\src\archive\zip\reader.go
using bufio = go.bufio_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;
using crc32 = go.hash.crc32_package;
using io = go.io_package;
using os = go.os_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class zip_package
    {
        public static var ErrFormat = errors.New("zip: not a valid zip file");        public static var ErrAlgorithm = errors.New("zip: unsupported compression algorithm");        public static var ErrChecksum = errors.New("zip: checksum error");

        public partial struct Reader
        {
            public io.ReaderAt r;
            public slice<ptr<File>> File;
            public @string Comment;
            public map<ushort, Decompressor> decompressors;
        }

        public partial struct ReadCloser : Reader
        {
            public ptr<os.File> f;
            public Reader Reader;
        }

        public partial struct File
        {
            public ref FileHeader FileHeader => ref FileHeader_val;
            public ptr<Reader> zip;
            public io.ReaderAt zipr;
            public long zipsize;
            public long headerOffset;
        }

        private static bool hasDataDescriptor(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            return f.Flags & 0x8UL != 0L;
        }

        // OpenReader will open the Zip file specified by name and return a ReadCloser.
        public static (ptr<ReadCloser>, error) OpenReader(@string name)
        {
            ptr<ReadCloser> _p0 = default!;
            error _p0 = default!;

            var (f, err) = os.Open(name);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (fi, err) = f.Stat();
            if (err != null)
            {
                f.Close();
                return (_addr_null!, error.As(err)!);
            }

            ptr<ReadCloser> r = @new<ReadCloser>();
            {
                var err = r.init(f, fi.Size());

                if (err != null)
                {
                    f.Close();
                    return (_addr_null!, error.As(err)!);
                }

            }

            r.f = f;
            return (_addr_r!, error.As(null!)!);

        }

        // NewReader returns a new Reader reading from r, which is assumed to
        // have the given size in bytes.
        public static (ptr<Reader>, error) NewReader(io.ReaderAt r, long size)
        {
            ptr<Reader> _p0 = default!;
            error _p0 = default!;

            if (size < 0L)
            {
                return (_addr_null!, error.As(errors.New("zip: size cannot be negative"))!);
            }

            ptr<Reader> zr = @new<Reader>();
            {
                var err = zr.init(r, size);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            return (_addr_zr!, error.As(null!)!);

        }

        private static error init(this ptr<Reader> _addr_z, io.ReaderAt r, long size)
        {
            ref Reader z = ref _addr_z.val;

            var (end, err) = readDirectoryEnd(r, size);
            if (err != null)
            {
                return error.As(err)!;
            }

            z.r = r;
            z.File = make_slice<ptr<File>>(0L, end.directoryRecords);
            z.Comment = end.comment;
            var rs = io.NewSectionReader(r, 0L, size);
            _, err = rs.Seek(int64(end.directoryOffset), io.SeekStart);

            if (err != null)
            {
                return error.As(err)!;
            }

            var buf = bufio.NewReader(rs); 

            // The count of files inside a zip is truncated to fit in a uint16.
            // Gloss over this by reading headers until we encounter
            // a bad one, and then only report an ErrFormat or UnexpectedEOF if
            // the file count modulo 65536 is incorrect.
            while (true)
            {
                ptr<File> f = addr(new File(zip:z,zipr:r,zipsize:size));
                err = readDirectoryHeader(f, buf);
                if (err == ErrFormat || err == io.ErrUnexpectedEOF)
                {
                    break;
                }

                if (err != null)
                {
                    return error.As(err)!;
                }

                z.File = append(z.File, f);

            }

            if (uint16(len(z.File)) != uint16(end.directoryRecords))
            { // only compare 16 bits here
                // Return the readDirectoryHeader error if we read
                // the wrong number of directory entries.
                return error.As(err)!;

            }

            return error.As(null!)!;

        }

        // RegisterDecompressor registers or overrides a custom decompressor for a
        // specific method ID. If a decompressor for a given method is not found,
        // Reader will default to looking up the decompressor at the package level.
        private static void RegisterDecompressor(this ptr<Reader> _addr_z, ushort method, Decompressor dcomp)
        {
            ref Reader z = ref _addr_z.val;

            if (z.decompressors == null)
            {
                z.decompressors = make_map<ushort, Decompressor>();
            }

            z.decompressors[method] = dcomp;

        }

        private static Decompressor decompressor(this ptr<Reader> _addr_z, ushort method)
        {
            ref Reader z = ref _addr_z.val;

            var dcomp = z.decompressors[method];
            if (dcomp == null)
            {
                dcomp = decompressor(method);
            }

            return dcomp;

        }

        // Close closes the Zip file, rendering it unusable for I/O.
        private static error Close(this ptr<ReadCloser> _addr_rc)
        {
            ref ReadCloser rc = ref _addr_rc.val;

            return error.As(rc.f.Close())!;
        }

        // DataOffset returns the offset of the file's possibly-compressed
        // data, relative to the beginning of the zip file.
        //
        // Most callers should instead use Open, which transparently
        // decompresses data and verifies checksums.
        private static (long, error) DataOffset(this ptr<File> _addr_f)
        {
            long offset = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            var (bodyOffset, err) = f.findBodyOffset();
            if (err != null)
            {
                return ;
            }

            return (f.headerOffset + bodyOffset, error.As(null!)!);

        }

        // Open returns a ReadCloser that provides access to the File's contents.
        // Multiple files may be read concurrently.
        private static (io.ReadCloser, error) Open(this ptr<File> _addr_f)
        {
            io.ReadCloser _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            var (bodyOffset, err) = f.findBodyOffset();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var size = int64(f.CompressedSize64);
            var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset, size);
            var dcomp = f.zip.decompressor(f.Method);
            if (dcomp == null)
            {
                return (null, error.As(ErrAlgorithm)!);
            }

            io.ReadCloser rc = dcomp(r);
            io.Reader desr = default;
            if (f.hasDataDescriptor())
            {
                desr = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset + size, dataDescriptorLen);
            }

            rc = addr(new checksumReader(rc:rc,hash:crc32.NewIEEE(),f:f,desr:desr,));
            return (rc, error.As(null!)!);

        }

        private partial struct checksumReader
        {
            public io.ReadCloser rc;
            public hash.Hash32 hash;
            public ulong nread; // number of bytes read so far
            public ptr<File> f;
            public io.Reader desr; // if non-nil, where to read the data descriptor
            public error err; // sticky error
        }

        private static (long, error) Read(this ptr<checksumReader> _addr_r, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref checksumReader r = ref _addr_r.val;

            if (r.err != null)
            {
                return (0L, error.As(r.err)!);
            }

            n, err = r.rc.Read(b);
            r.hash.Write(b[..n]);
            r.nread += uint64(n);
            if (err == null)
            {
                return ;
            }

            if (err == io.EOF)
            {
                if (r.nread != r.f.UncompressedSize64)
                {
                    return (0L, error.As(io.ErrUnexpectedEOF)!);
                }

                if (r.desr != null)
                {
                    {
                        var err1 = readDataDescriptor(r.desr, _addr_r.f);

                        if (err1 != null)
                        {
                            if (err1 == io.EOF)
                            {
                                err = io.ErrUnexpectedEOF;
                            }
                            else
                            {
                                err = err1;
                            }

                        }
                        else if (r.hash.Sum32() != r.f.CRC32)
                        {
                            err = ErrChecksum;
                        }


                    }

                }
                else
                { 
                    // If there's not a data descriptor, we still compare
                    // the CRC32 of what we've read against the file header
                    // or TOC's CRC32, if it seems like it was set.
                    if (r.f.CRC32 != 0L && r.hash.Sum32() != r.f.CRC32)
                    {
                        err = ErrChecksum;
                    }

                }

            }

            r.err = err;
            return ;

        }

        private static error Close(this ptr<checksumReader> _addr_r)
        {
            ref checksumReader r = ref _addr_r.val;

            return error.As(r.rc.Close())!;
        }

        // findBodyOffset does the minimum work to verify the file has a header
        // and returns the file body offset.
        private static (long, error) findBodyOffset(this ptr<File> _addr_f)
        {
            long _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            array<byte> buf = new array<byte>(fileHeaderLen);
            {
                var (_, err) = f.zipr.ReadAt(buf[..], f.headerOffset);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            var b = readBuf(buf[..]);
            {
                var sig = b.uint32();

                if (sig != fileHeaderSignature)
                {
                    return (0L, error.As(ErrFormat)!);
                }

            }

            b = b[22L..]; // skip over most of the header
            var filenameLen = int(b.uint16());
            var extraLen = int(b.uint16());
            return (int64(fileHeaderLen + filenameLen + extraLen), error.As(null!)!);

        }

        // readDirectoryHeader attempts to read a directory header from r.
        // It returns io.ErrUnexpectedEOF if it cannot read a complete header,
        // and ErrFormat if it doesn't find a valid header signature.
        private static error readDirectoryHeader(ptr<File> _addr_f, io.Reader r)
        {
            ref File f = ref _addr_f.val;

            array<byte> buf = new array<byte>(directoryHeaderLen);
            {
                var (_, err) = io.ReadFull(r, buf[..]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var b = readBuf(buf[..]);
            {
                var sig = b.uint32();

                if (sig != directoryHeaderSignature)
                {
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
            b = b[4L..]; // skipped start disk number and internal attributes (2x uint16)
            f.ExternalAttrs = b.uint32();
            f.headerOffset = int64(b.uint32());
            var d = make_slice<byte>(filenameLen + extraLen + commentLen);
            {
                (_, err) = io.ReadFull(r, d);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            f.Name = string(d[..filenameLen]);
            f.Extra = d[filenameLen..filenameLen + extraLen];
            f.Comment = string(d[filenameLen + extraLen..]); 

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
                f.NonUTF8 = f.Flags & 0x800UL == 0L;
                        var needUSize = f.UncompressedSize == ~uint32(0L);
            var needCSize = f.CompressedSize == ~uint32(0L);
            var needHeaderOffset = f.headerOffset == int64(~uint32(0L)); 

            // Best effort to find what we need.
            // Other zip authors might not even follow the basic format,
            // and we'll just ignore the Extra content in that case.
            time.Time modified = default;
parseExtras:

            {
                var extra = readBuf(f.Extra);

                while (len(extra) >= 4L)
                { // need at least tag and size
                    var fieldTag = extra.uint16();
                    var fieldSize = int(extra.uint16());
                    if (len(extra) < fieldSize)
                    {
                        break;
                    }

                    var fieldBuf = extra.sub(fieldSize);


                    if (fieldTag == zip64ExtraID) 
                        // update directory values from the zip64 extra block.
                        // They should only be consulted if the sizes read earlier
                        // are maxed out.
                        // See golang.org/issue/13367.
                        if (needUSize)
                        {
                            needUSize = false;
                            if (len(fieldBuf) < 8L)
                            {
                                return error.As(ErrFormat)!;
                            }

                            f.UncompressedSize64 = fieldBuf.uint64();

                        }

                        if (needCSize)
                        {
                            needCSize = false;
                            if (len(fieldBuf) < 8L)
                            {
                                return error.As(ErrFormat)!;
                            }

                            f.CompressedSize64 = fieldBuf.uint64();

                        }

                        if (needHeaderOffset)
                        {
                            needHeaderOffset = false;
                            if (len(fieldBuf) < 8L)
                            {
                                return error.As(ErrFormat)!;
                            }

                            f.headerOffset = int64(fieldBuf.uint64());

                        }

                    else if (fieldTag == ntfsExtraID) 
                        if (len(fieldBuf) < 4L)
                        {
                            _continueparseExtras = true;
                            break;
                        }

                        fieldBuf.uint32(); // reserved (ignored)
                        while (len(fieldBuf) >= 4L)
                        { // need at least tag and size
                            var attrTag = fieldBuf.uint16();
                            var attrSize = int(fieldBuf.uint16());
                            if (len(fieldBuf) < attrSize)
                            {
                                _continueparseExtras = true;
                                break;
                            }

                            var attrBuf = fieldBuf.sub(attrSize);
                            if (attrTag != 1L || attrSize != 24L)
                            {
                                continue; // Ignore irrelevant attributes
                            }

                            const float ticksPerSecond = (float)1e7F; // Windows timestamp resolution
 // Windows timestamp resolution
                            var ts = int64(attrBuf.uint64()); // ModTime since Windows epoch
                            var secs = int64(ts / ticksPerSecond);
                            float nsecs = (1e9F / ticksPerSecond) * int64(ts % ticksPerSecond);
                            var epoch = time.Date(1601L, time.January, 1L, 0L, 0L, 0L, 0L, time.UTC);
                            modified = time.Unix(epoch.Unix() + secs, nsecs);

                        }
                    else if (fieldTag == unixExtraID || fieldTag == infoZipUnixExtraID) 
                        if (len(fieldBuf) < 8L)
                        {
                            _continueparseExtras = true;
                            break;
                        }

                        fieldBuf.uint32(); // AcTime (ignored)
                        ts = int64(fieldBuf.uint32()); // ModTime since Unix epoch
                        modified = time.Unix(ts, 0L);
                    else if (fieldTag == extTimeExtraID) 
                        if (len(fieldBuf) < 5L || fieldBuf.uint8() & 1L == 0L)
                        {
                            _continueparseExtras = true;
                            break;
                        }

                        ts = int64(fieldBuf.uint32()); // ModTime since Unix epoch
                        modified = time.Unix(ts, 0L);
                    
                }

            }
            var msdosModified = msDosTimeToTime(f.ModifiedDate, f.ModifiedTime);
            f.Modified = msdosModified;
            if (!modified.IsZero())
            {
                f.Modified = modified.UTC(); 

                // If legacy MS-DOS timestamps are set, we can use the delta between
                // the legacy and extended versions to estimate timezone offset.
                //
                // A non-UTC timezone is always used (even if offset is zero).
                // Thus, FileHeader.Modified.Location() == time.UTC is useful for
                // determining whether extended timestamps are present.
                // This is necessary for users that need to do additional time
                // calculations when dealing with legacy ZIP formats.
                if (f.ModifiedTime != 0L || f.ModifiedDate != 0L)
                {
                    f.Modified = modified.In(timeZone(msdosModified.Sub(modified)));
                }

            } 

            // Assume that uncompressed size 2³²-1 could plausibly happen in
            // an old zip32 file that was sharding inputs into the largest chunks
            // possible (or is just malicious; search the web for 42.zip).
            // If needUSize is true still, it means we didn't see a zip64 extension.
            // As long as the compressed size is not also 2³²-1 (implausible)
            // and the header is not also 2³²-1 (equally implausible),
            // accept the uncompressed size 2³²-1 as valid.
            // If nothing else, this keeps archive/zip working with 42.zip.
            _ = needUSize;

            if (needCSize || needHeaderOffset)
            {
                return error.As(ErrFormat)!;
            }

            return error.As(null!)!;

        }

        private static error readDataDescriptor(io.Reader r, ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            array<byte> buf = new array<byte>(dataDescriptorLen); 

            // The spec says: "Although not originally assigned a
            // signature, the value 0x08074b50 has commonly been adopted
            // as a signature value for the data descriptor record.
            // Implementers should be aware that ZIP files may be
            // encountered with or without this signature marking data
            // descriptors and should account for either case when reading
            // ZIP files to ensure compatibility."
            //
            // dataDescriptorLen includes the size of the signature but
            // first read just those 4 bytes to see if it exists.
            {
                var (_, err) = io.ReadFull(r, buf[..4L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            long off = 0L;
            var maybeSig = readBuf(buf[..4L]);
            if (maybeSig.uint32() != dataDescriptorSignature)
            { 
                // No data descriptor signature. Keep these four
                // bytes.
                off += 4L;

            }

            {
                (_, err) = io.ReadFull(r, buf[off..12L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var b = readBuf(buf[..12L]);
            if (b.uint32() != f.CRC32)
            {
                return error.As(ErrChecksum)!;
            } 

            // The two sizes that follow here can be either 32 bits or 64 bits
            // but the spec is not very clear on this and different
            // interpretations has been made causing incompatibilities. We
            // already have the sizes from the central directory so we can
            // just ignore these.
            return error.As(null!)!;

        }

        private static (ptr<directoryEnd>, error) readDirectoryEnd(io.ReaderAt r, long size)
        {
            ptr<directoryEnd> dir = default!;
            error err = default!;
 
            // look for directoryEndSignature in the last 1k, then in the last 65k
            slice<byte> buf = default;
            long directoryEndOffset = default;
            foreach (var (i, bLen) in new slice<long>(new long[] { 1024, 65*1024 }))
            {
                if (bLen > size)
                {
                    bLen = size;
                }

                buf = make_slice<byte>(int(bLen));
                {
                    var (_, err) = r.ReadAt(buf, size - bLen);

                    if (err != null && err != io.EOF)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                }

                {
                    var p__prev1 = p;

                    var p = findSignatureInBlock(buf);

                    if (p >= 0L)
                    {
                        buf = buf[p..];
                        directoryEndOffset = size - bLen + int64(p);
                        break;
                    }

                    p = p__prev1;

                }

                if (i == 1L || bLen == size)
                {
                    return (_addr_null!, error.As(ErrFormat)!);
                }

            } 

            // read header into struct
            var b = readBuf(buf[4L..]); // skip signature
            ptr<directoryEnd> d = addr(new directoryEnd(diskNbr:uint32(b.uint16()),dirDiskNbr:uint32(b.uint16()),dirRecordsThisDisk:uint64(b.uint16()),directoryRecords:uint64(b.uint16()),directorySize:uint64(b.uint32()),directoryOffset:uint64(b.uint32()),commentLen:b.uint16(),));
            var l = int(d.commentLen);
            if (l > len(b))
            {
                return (_addr_null!, error.As(errors.New("zip: invalid comment length"))!);
            }

            d.comment = string(b[..l]); 

            // These values mean that the file can be a zip64 file
            if (d.directoryRecords == 0xffffUL || d.directorySize == 0xffffUL || d.directoryOffset == 0xffffffffUL)
            {
                var (p, err) = findDirectory64End(r, directoryEndOffset);
                if (err == null && p >= 0L)
                {
                    err = readDirectory64End(r, p, d);
                }

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            } 
            // Make sure directoryOffset points to somewhere in our file.
            {
                var o = int64(d.directoryOffset);

                if (o < 0L || o >= size)
                {
                    return (_addr_null!, error.As(ErrFormat)!);
                }

            }

            return (_addr_d!, error.As(null!)!);

        }

        // findDirectory64End tries to read the zip64 locator just before the
        // directory end and returns the offset of the zip64 directory end if
        // found.
        private static (long, error) findDirectory64End(io.ReaderAt r, long directoryEndOffset)
        {
            long _p0 = default;
            error _p0 = default!;

            var locOffset = directoryEndOffset - directory64LocLen;
            if (locOffset < 0L)
            {
                return (-1L, error.As(null!)!); // no need to look for a header outside the file
            }

            var buf = make_slice<byte>(directory64LocLen);
            {
                var (_, err) = r.ReadAt(buf, locOffset);

                if (err != null)
                {
                    return (-1L, error.As(err)!);
                }

            }

            var b = readBuf(buf);
            {
                var sig = b.uint32();

                if (sig != directory64LocSignature)
                {
                    return (-1L, error.As(null!)!);
                }

            }

            if (b.uint32() != 0L)
            { // number of the disk with the start of the zip64 end of central directory
                return (-1L, error.As(null!)!); // the file is not a valid zip64-file
            }

            var p = b.uint64(); // relative offset of the zip64 end of central directory record
            if (b.uint32() != 1L)
            { // total number of disks
                return (-1L, error.As(null!)!); // the file is not a valid zip64-file
            }

            return (int64(p), error.As(null!)!);

        }

        // readDirectory64End reads the zip64 directory end and updates the
        // directory end with the zip64 directory end values.
        private static error readDirectory64End(io.ReaderAt r, long offset, ptr<directoryEnd> _addr_d)
        {
            error err = default!;
            ref directoryEnd d = ref _addr_d.val;

            var buf = make_slice<byte>(directory64EndLen);
            {
                var (_, err) = r.ReadAt(buf, offset);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var b = readBuf(buf);
            {
                var sig = b.uint32();

                if (sig != directory64EndSignature)
                {
                    return error.As(ErrFormat)!;
                }

            }


            b = b[12L..]; // skip dir size, version and version needed (uint64 + 2x uint16)
            d.diskNbr = b.uint32(); // number of this disk
            d.dirDiskNbr = b.uint32(); // number of the disk with the start of the central directory
            d.dirRecordsThisDisk = b.uint64(); // total number of entries in the central directory on this disk
            d.directoryRecords = b.uint64(); // total number of entries in the central directory
            d.directorySize = b.uint64(); // size of the central directory
            d.directoryOffset = b.uint64(); // offset of start of central directory with respect to the starting disk number

            return error.As(null!)!;

        }

        private static long findSignatureInBlock(slice<byte> b)
        {
            for (var i = len(b) - directoryEndLen; i >= 0L; i--)
            { 
                // defined from directoryEndSignature in struct.go
                if (b[i] == 'P' && b[i + 1L] == 'K' && b[i + 2L] == 0x05UL && b[i + 3L] == 0x06UL)
                { 
                    // n is length of comment
                    var n = int(b[i + directoryEndLen - 2L]) | int(b[i + directoryEndLen - 1L]) << (int)(8L);
                    if (n + directoryEndLen + i <= len(b))
                    {
                        return i;
                    }

                }

            }

            return -1L;

        }

        private partial struct readBuf // : slice<byte>
        {
        }

        private static byte uint8(this ptr<readBuf> _addr_b)
        {
            ref readBuf b = ref _addr_b.val;

            var v = (b.val)[0L];
            b.val = (b.val)[1L..];
            return v;
        }

        private static ushort uint16(this ptr<readBuf> _addr_b)
        {
            ref readBuf b = ref _addr_b.val;

            var v = binary.LittleEndian.Uint16(b.val);
            b.val = (b.val)[2L..];
            return v;
        }

        private static uint uint32(this ptr<readBuf> _addr_b)
        {
            ref readBuf b = ref _addr_b.val;

            var v = binary.LittleEndian.Uint32(b.val);
            b.val = (b.val)[4L..];
            return v;
        }

        private static ulong uint64(this ptr<readBuf> _addr_b)
        {
            ref readBuf b = ref _addr_b.val;

            var v = binary.LittleEndian.Uint64(b.val);
            b.val = (b.val)[8L..];
            return v;
        }

        private static readBuf sub(this ptr<readBuf> _addr_b, long n)
        {
            ref readBuf b = ref _addr_b.val;

            var b2 = (b.val)[..n];
            b.val = (b.val)[n..];
            return b2;
        }
    }
}}
