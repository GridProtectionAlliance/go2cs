// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package zip -- go2cs converted at 2022 March 13 05:42:40 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Program Files\Go\src\archive\zip\writer.go
namespace go.archive;

using bufio = bufio_package;
using binary = encoding.binary_package;
using errors = errors_package;
using hash = hash_package;
using crc32 = hash.crc32_package;
using io = io_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class zip_package {

private static var errLongName = errors.New("zip: FileHeader.Name too long");private static var errLongExtra = errors.New("zip: FileHeader.Extra too long");

// Writer implements a zip file writer.
public partial struct Writer {
    public ptr<countWriter> cw;
    public slice<ptr<header>> dir;
    public ptr<fileWriter> last;
    public bool closed;
    public map<ushort, Compressor> compressors;
    public @string comment; // testHookCloseSizeOffset if non-nil is called with the size
// of offset of the central directory at Close.
    public Action<ulong, ulong> testHookCloseSizeOffset;
}

private partial struct header {
    public ref ptr<FileHeader> ptr<FileHeader> => ref ptr<FileHeader>_ptr;
    public ulong offset;
    public bool raw;
}

// NewWriter returns a new Writer writing a zip file to w.
public static ptr<Writer> NewWriter(io.Writer w) {
    return addr(new Writer(cw:&countWriter{w:bufio.NewWriter(w)}));
}

// SetOffset sets the offset of the beginning of the zip data within the
// underlying writer. It should be used when the zip data is appended to an
// existing file, such as a binary executable.
// It must be called before any data is written.
private static void SetOffset(this ptr<Writer> _addr_w, long n) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    if (w.cw.count != 0) {
        panic("zip: SetOffset called after data was written");
    }
    w.cw.count = n;
});

// Flush flushes any buffered data to the underlying writer.
// Calling Flush is not normally necessary; calling Close is sufficient.
private static error Flush(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    return error.As(w.cw.w._<ptr<bufio.Writer>>().Flush()!)!;
}

// SetComment sets the end-of-central-directory comment field.
// It can only be called before Close.
private static error SetComment(this ptr<Writer> _addr_w, @string comment) {
    ref Writer w = ref _addr_w.val;

    if (len(comment) > uint16max) {
        return error.As(errors.New("zip: Writer.Comment too long"))!;
    }
    w.comment = comment;
    return error.As(null!)!;
}

// Close finishes writing the zip file by writing the central directory.
// It does not close the underlying writer.
private static error Close(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    if (w.last != null && !w.last.closed) {
        {
            var err = w.last.close();

            if (err != null) {
                return error.As(err)!;
            }

        }
        w.last = null;
    }
    if (w.closed) {
        return error.As(errors.New("zip: writer closed twice"))!;
    }
    w.closed = true; 

    // write central directory
    var start = w.cw.count;
    foreach (var (_, h) in w.dir) {
        array<byte> buf = new array<byte>(directoryHeaderLen);
        var b = writeBuf(buf[..]);
        b.uint32(uint32(directoryHeaderSignature));
        b.uint16(h.CreatorVersion);
        b.uint16(h.ReaderVersion);
        b.uint16(h.Flags);
        b.uint16(h.Method);
        b.uint16(h.ModifiedTime);
        b.uint16(h.ModifiedDate);
        b.uint32(h.CRC32);
        if (h.isZip64() || h.offset >= uint32max) { 
            // the file needs a zip64 header. store maxint in both
            // 32 bit size fields (and offset later) to signal that the
            // zip64 extra header should be used.
            b.uint32(uint32max); // compressed size
            b.uint32(uint32max); // uncompressed size

            // append a zip64 extra block to Extra
            buf = new array<byte>(28); // 2x uint16 + 3x uint64
            var eb = writeBuf(buf[..]);
            eb.uint16(zip64ExtraID);
            eb.uint16(24); // size = 3x uint64
            eb.uint64(h.UncompressedSize64);
            eb.uint64(h.CompressedSize64);
            eb.uint64(h.offset);
            h.Extra = append(h.Extra, buf[..]);
        }
        else
 {
            b.uint32(h.CompressedSize);
            b.uint32(h.UncompressedSize);
        }
        b.uint16(uint16(len(h.Name)));
        b.uint16(uint16(len(h.Extra)));
        b.uint16(uint16(len(h.Comment)));
        b = b[(int)4..]; // skip disk number start and internal file attr (2x uint16)
        b.uint32(h.ExternalAttrs);
        if (h.offset > uint32max) {
            b.uint32(uint32max);
        }
        else
 {
            b.uint32(uint32(h.offset));
        }
        {
            var (_, err) = w.cw.Write(buf[..]);

            if (err != null) {
                return error.As(err)!;
            }

        }
        {
            (_, err) = io.WriteString(w.cw, h.Name);

            if (err != null) {
                return error.As(err)!;
            }

        }
        {
            (_, err) = w.cw.Write(h.Extra);

            if (err != null) {
                return error.As(err)!;
            }

        }
        {
            (_, err) = io.WriteString(w.cw, h.Comment);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }    var end = w.cw.count;

    var records = uint64(len(w.dir));
    var size = uint64(end - start);
    var offset = uint64(start);

    {
        var f = w.testHookCloseSizeOffset;

        if (f != null) {
            f(size, offset);
        }
    }

    if (records >= uint16max || size >= uint32max || offset >= uint32max) {
        buf = new array<byte>(directory64EndLen + directory64LocLen);
        b = writeBuf(buf[..]); 

        // zip64 end of central directory record
        b.uint32(directory64EndSignature);
        b.uint64(directory64EndLen - 12); // length minus signature (uint32) and length fields (uint64)
        b.uint16(zipVersion45); // version made by
        b.uint16(zipVersion45); // version needed to extract
        b.uint32(0); // number of this disk
        b.uint32(0); // number of the disk with the start of the central directory
        b.uint64(records); // total number of entries in the central directory on this disk
        b.uint64(records); // total number of entries in the central directory
        b.uint64(size); // size of the central directory
        b.uint64(offset); // offset of start of central directory with respect to the starting disk number

        // zip64 end of central directory locator
        b.uint32(directory64LocSignature);
        b.uint32(0); // number of the disk with the start of the zip64 end of central directory
        b.uint64(uint64(end)); // relative offset of the zip64 end of central directory record
        b.uint32(1); // total number of disks

        {
            (_, err) = w.cw.Write(buf[..]);

            if (err != null) {
                return error.As(err)!;
            } 

            // store max values in the regular end record to signal
            // that the zip64 values should be used instead

        } 

        // store max values in the regular end record to signal
        // that the zip64 values should be used instead
        records = uint16max;
        size = uint32max;
        offset = uint32max;
    }
    buf = new array<byte>(directoryEndLen);
    b = writeBuf(buf[..]);
    b.uint32(uint32(directoryEndSignature));
    b = b[(int)4..]; // skip over disk number and first disk number (2x uint16)
    b.uint16(uint16(records)); // number of entries this disk
    b.uint16(uint16(records)); // number of entries total
    b.uint32(uint32(size)); // size of directory
    b.uint32(uint32(offset)); // start of directory
    b.uint16(uint16(len(w.comment))); // byte size of EOCD comment
    {
        (_, err) = w.cw.Write(buf[..]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        (_, err) = io.WriteString(w.cw, w.comment);

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(w.cw.w._<ptr<bufio.Writer>>().Flush()!)!;
}

// Create adds a file to the zip file using the provided name.
// It returns a Writer to which the file contents should be written.
// The file contents will be compressed using the Deflate method.
// The name must be a relative path: it must not start with a drive
// letter (e.g. C:) or leading slash, and only forward slashes are
// allowed. To create a directory instead of a file, add a trailing
// slash to the name.
// The file's contents must be written to the io.Writer before the next
// call to Create, CreateHeader, or Close.
private static (io.Writer, error) Create(this ptr<Writer> _addr_w, @string name) {
    io.Writer _p0 = default;
    error _p0 = default!;
    ref Writer w = ref _addr_w.val;

    ptr<FileHeader> header = addr(new FileHeader(Name:name,Method:Deflate,));
    return w.CreateHeader(header);
}

// detectUTF8 reports whether s is a valid UTF-8 string, and whether the string
// must be considered UTF-8 encoding (i.e., not compatible with CP-437, ASCII,
// or any other common encoding).
private static (bool, bool) detectUTF8(@string s) {
    bool valid = default;
    bool require = default;

    {
        nint i = 0;

        while (i < len(s)) {
            var (r, size) = utf8.DecodeRuneInString(s[(int)i..]);
            i += size; 
            // Officially, ZIP uses CP-437, but many readers use the system's
            // local character encoding. Most encoding are compatible with a large
            // subset of CP-437, which itself is ASCII-like.
            //
            // Forbid 0x7e and 0x5c since EUC-KR and Shift-JIS replace those
            // characters with localized currency and overline characters.
            if (r < 0x20 || r > 0x7d || r == 0x5c) {
                if (!utf8.ValidRune(r) || (r == utf8.RuneError && size == 1)) {
                    return (false, false);
                }
                require = true;
            }
        }
    }
    return (true, require);
}

// prepare performs the bookkeeping operations required at the start of
// CreateHeader and CreateRaw.
private static error prepare(this ptr<Writer> _addr_w, ptr<FileHeader> _addr_fh) {
    ref Writer w = ref _addr_w.val;
    ref FileHeader fh = ref _addr_fh.val;

    if (w.last != null && !w.last.closed) {
        {
            var err = w.last.close();

            if (err != null) {
                return error.As(err)!;
            }

        }
    }
    if (len(w.dir) > 0 && w.dir[len(w.dir) - 1].FileHeader == fh) { 
        // See https://golang.org/issue/11144 confusion.
        return error.As(errors.New("archive/zip: invalid duplicate FileHeader"))!;
    }
    return error.As(null!)!;
}

// CreateHeader adds a file to the zip archive using the provided FileHeader
// for the file metadata. Writer takes ownership of fh and may mutate
// its fields. The caller must not modify fh after calling CreateHeader.
//
// This returns a Writer to which the file contents should be written.
// The file's contents must be written to the io.Writer before the next
// call to Create, CreateHeader, CreateRaw, or Close.
private static (io.Writer, error) CreateHeader(this ptr<Writer> _addr_w, ptr<FileHeader> _addr_fh) {
    io.Writer _p0 = default;
    error _p0 = default!;
    ref Writer w = ref _addr_w.val;
    ref FileHeader fh = ref _addr_fh.val;

    {
        var err__prev1 = err;

        var err = w.prepare(fh);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 

    // The ZIP format has a sad state of affairs regarding character encoding.
    // Officially, the name and comment fields are supposed to be encoded
    // in CP-437 (which is mostly compatible with ASCII), unless the UTF-8
    // flag bit is set. However, there are several problems:
    //
    //    * Many ZIP readers still do not support UTF-8.
    //    * If the UTF-8 flag is cleared, several readers simply interpret the
    //    name and comment fields as whatever the local system encoding is.
    //
    // In order to avoid breaking readers without UTF-8 support,
    // we avoid setting the UTF-8 flag if the strings are CP-437 compatible.
    // However, if the strings require multibyte UTF-8 encoding and is a
    // valid UTF-8 string, then we set the UTF-8 bit.
    //
    // For the case, where the user explicitly wants to specify the encoding
    // as UTF-8, they will need to set the flag bit themselves.
    var (utf8Valid1, utf8Require1) = detectUTF8(fh.Name);
    var (utf8Valid2, utf8Require2) = detectUTF8(fh.Comment);

    if (fh.NonUTF8) 
        fh.Flags &= 0x800;
    else if ((utf8Require1 || utf8Require2) && (utf8Valid1 && utf8Valid2)) 
        fh.Flags |= 0x800;
        fh.CreatorVersion = fh.CreatorVersion & 0xff00 | zipVersion20; // preserve compatibility byte
    fh.ReaderVersion = zipVersion20; 

    // If Modified is set, this takes precedence over MS-DOS timestamp fields.
    if (!fh.Modified.IsZero()) { 
        // Contrary to the FileHeader.SetModTime method, we intentionally
        // do not convert to UTC, because we assume the user intends to encode
        // the date using the specified timezone. A user may want this control
        // because many legacy ZIP readers interpret the timestamp according
        // to the local timezone.
        //
        // The timezone is only non-UTC if a user directly sets the Modified
        // field directly themselves. All other approaches sets UTC.
        fh.ModifiedDate, fh.ModifiedTime = timeToMsDosTime(fh.Modified); 

        // Use "extended timestamp" format since this is what Info-ZIP uses.
        // Nearly every major ZIP implementation uses a different format,
        // but at least most seem to be able to understand the other formats.
        //
        // This format happens to be identical for both local and central header
        // if modification time is the only timestamp being encoded.
        array<byte> mbuf = new array<byte>(9); // 2*SizeOf(uint16) + SizeOf(uint8) + SizeOf(uint32)
        var mt = uint32(fh.Modified.Unix());
        var eb = writeBuf(mbuf[..]);
        eb.uint16(extTimeExtraID);
        eb.uint16(5); // Size: SizeOf(uint8) + SizeOf(uint32)
        eb.uint8(1); // Flags: ModTime
        eb.uint32(mt); // ModTime
        fh.Extra = append(fh.Extra, mbuf[..]);
    }
    io.Writer ow = default;    ptr<fileWriter> fw;
    ptr<header> h = addr(new header(FileHeader:fh,offset:uint64(w.cw.count),));

    if (strings.HasSuffix(fh.Name, "/")) { 
        // Set the compression method to Store to ensure data length is truly zero,
        // which the writeHeader method always encodes for the size fields.
        // This is necessary as most compression formats have non-zero lengths
        // even when compressing an empty string.
        fh.Method = Store;
        fh.Flags &= 0x8; // we will not write a data descriptor

        // Explicitly clear sizes as they have no meaning for directories.
        fh.CompressedSize = 0;
        fh.CompressedSize64 = 0;
        fh.UncompressedSize = 0;
        fh.UncompressedSize64 = 0;

        ow = new dirWriter();
    }
    else
 {
        fh.Flags |= 0x8; // we will write a data descriptor

        fw = addr(new fileWriter(zipw:w.cw,compCount:&countWriter{w:w.cw},crc32:crc32.NewIEEE(),));
        var comp = w.compressor(fh.Method);
        if (comp == null) {
            return (null, error.As(ErrAlgorithm)!);
        }
        err = default!;
        fw.comp, err = comp(fw.compCount);
        if (err != null) {
            return (null, error.As(err)!);
        }
        fw.rawCount = addr(new countWriter(w:fw.comp));
        fw.header = h;
        ow = fw;
    }
    w.dir = append(w.dir, h);
    {
        var err__prev1 = err;

        err = writeHeader(w.cw, h);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 
    // If we're creating a directory, fw is nil.
    w.last = fw;
    return (ow, error.As(null!)!);
}

private static error writeHeader(io.Writer w, ptr<header> _addr_h) {
    ref header h = ref _addr_h.val;

    const nint maxUint16 = 1 << 16 - 1;

    if (len(h.Name) > maxUint16) {
        return error.As(errLongName)!;
    }
    if (len(h.Extra) > maxUint16) {
        return error.As(errLongExtra)!;
    }
    array<byte> buf = new array<byte>(fileHeaderLen);
    var b = writeBuf(buf[..]);
    b.uint32(uint32(fileHeaderSignature));
    b.uint16(h.ReaderVersion);
    b.uint16(h.Flags);
    b.uint16(h.Method);
    b.uint16(h.ModifiedTime);
    b.uint16(h.ModifiedDate); 
    // In raw mode (caller does the compression), the values are either
    // written here or in the trailing data descriptor based on the header
    // flags.
    if (h.raw && !h.hasDataDescriptor()) {
        b.uint32(h.CRC32);
        b.uint32(uint32(min64(h.CompressedSize64, uint32max)));
        b.uint32(uint32(min64(h.UncompressedSize64, uint32max)));
    }
    else
 { 
        // When this package handle the compression, these values are
        // always written to the trailing data descriptor.
        b.uint32(0); // crc32
        b.uint32(0); // compressed size
        b.uint32(0); // uncompressed size
    }
    b.uint16(uint16(len(h.Name)));
    b.uint16(uint16(len(h.Extra)));
    {
        var (_, err) = w.Write(buf[..]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        (_, err) = io.WriteString(w, h.Name);

        if (err != null) {
            return error.As(err)!;
        }
    }
    (_, err) = w.Write(h.Extra);
    return error.As(err)!;
}

private static ulong min64(ulong x, ulong y) {
    if (x < y) {
        return x;
    }
    return y;
}

// CreateRaw adds a file to the zip archive using the provided FileHeader and
// returns a Writer to which the file contents should be written. The file's
// contents must be written to the io.Writer before the next call to Create,
// CreateHeader, CreateRaw, or Close.
//
// In contrast to CreateHeader, the bytes passed to Writer are not compressed.
private static (io.Writer, error) CreateRaw(this ptr<Writer> _addr_w, ptr<FileHeader> _addr_fh) {
    io.Writer _p0 = default;
    error _p0 = default!;
    ref Writer w = ref _addr_w.val;
    ref FileHeader fh = ref _addr_fh.val;

    {
        var err__prev1 = err;

        var err = w.prepare(fh);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    fh.CompressedSize = uint32(min64(fh.CompressedSize64, uint32max));
    fh.UncompressedSize = uint32(min64(fh.UncompressedSize64, uint32max));

    ptr<header> h = addr(new header(FileHeader:fh,offset:uint64(w.cw.count),raw:true,));
    w.dir = append(w.dir, h);
    {
        var err__prev1 = err;

        err = writeHeader(w.cw, h);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    if (strings.HasSuffix(fh.Name, "/")) {
        w.last = null;
        return (new dirWriter(), error.As(null!)!);
    }
    ptr<fileWriter> fw = addr(new fileWriter(header:h,zipw:w.cw,));
    w.last = fw;
    return (fw, error.As(null!)!);
}

// Copy copies the file f (obtained from a Reader) into w. It copies the raw
// form directly bypassing decompression, compression, and validation.
private static error Copy(this ptr<Writer> _addr_w, ptr<File> _addr_f) {
    ref Writer w = ref _addr_w.val;
    ref File f = ref _addr_f.val;

    var (r, err) = f.OpenRaw();
    if (err != null) {
        return error.As(err)!;
    }
    var (fw, err) = w.CreateRaw(_addr_f.FileHeader);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = io.Copy(fw, r);
    return error.As(err)!;
}

// RegisterCompressor registers or overrides a custom compressor for a specific
// method ID. If a compressor for a given method is not found, Writer will
// default to looking up the compressor at the package level.
private static void RegisterCompressor(this ptr<Writer> _addr_w, ushort method, Compressor comp) {
    ref Writer w = ref _addr_w.val;

    if (w.compressors == null) {
        w.compressors = make_map<ushort, Compressor>();
    }
    w.compressors[method] = comp;
}

private static Compressor compressor(this ptr<Writer> _addr_w, ushort method) {
    ref Writer w = ref _addr_w.val;

    var comp = w.compressors[method];
    if (comp == null) {
        comp = compressor(method);
    }
    return comp;
}

private partial struct dirWriter {
}

private static (nint, error) Write(this dirWriter _p0, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;

    if (len(b) == 0) {
        return (0, error.As(null!)!);
    }
    return (0, error.As(errors.New("zip: write to directory"))!);
}

private partial struct fileWriter {
    public ref ptr<header> ptr<header> => ref ptr<header>_ptr;
    public io.Writer zipw;
    public ptr<countWriter> rawCount;
    public io.WriteCloser comp;
    public ptr<countWriter> compCount;
    public hash.Hash32 crc32;
    public bool closed;
}

private static (nint, error) Write(this ptr<fileWriter> _addr_w, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref fileWriter w = ref _addr_w.val;

    if (w.closed) {
        return (0, error.As(errors.New("zip: write to closed file"))!);
    }
    if (w.raw) {
        return w.zipw.Write(p);
    }
    w.crc32.Write(p);
    return w.rawCount.Write(p);
}

private static error close(this ptr<fileWriter> _addr_w) {
    ref fileWriter w = ref _addr_w.val;

    if (w.closed) {
        return error.As(errors.New("zip: file closed twice"))!;
    }
    w.closed = true;
    if (w.raw) {
        return error.As(w.writeDataDescriptor())!;
    }
    {
        var err = w.comp.Close();

        if (err != null) {
            return error.As(err)!;
        }
    } 

    // update FileHeader
    var fh = w.header.FileHeader;
    fh.CRC32 = w.crc32.Sum32();
    fh.CompressedSize64 = uint64(w.compCount.count);
    fh.UncompressedSize64 = uint64(w.rawCount.count);

    if (fh.isZip64()) {
        fh.CompressedSize = uint32max;
        fh.UncompressedSize = uint32max;
        fh.ReaderVersion = zipVersion45; // requires 4.5 - File uses ZIP64 format extensions
    }
    else
 {
        fh.CompressedSize = uint32(fh.CompressedSize64);
        fh.UncompressedSize = uint32(fh.UncompressedSize64);
    }
    return error.As(w.writeDataDescriptor())!;
}

private static error writeDataDescriptor(this ptr<fileWriter> _addr_w) {
    ref fileWriter w = ref _addr_w.val;

    if (!w.hasDataDescriptor()) {
        return error.As(null!)!;
    }
    slice<byte> buf = default;
    if (w.isZip64()) {
        buf = make_slice<byte>(dataDescriptor64Len);
    }
    else
 {
        buf = make_slice<byte>(dataDescriptorLen);
    }
    var b = writeBuf(buf);
    b.uint32(dataDescriptorSignature); // de-facto standard, required by OS X
    b.uint32(w.CRC32);
    if (w.isZip64()) {
        b.uint64(w.CompressedSize64);
        b.uint64(w.UncompressedSize64);
    }
    else
 {
        b.uint32(w.CompressedSize);
        b.uint32(w.UncompressedSize);
    }
    var (_, err) = w.zipw.Write(buf);
    return error.As(err)!;
}

private partial struct countWriter {
    public io.Writer w;
    public long count;
}

private static (nint, error) Write(this ptr<countWriter> _addr_w, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref countWriter w = ref _addr_w.val;

    var (n, err) = w.w.Write(p);
    w.count += int64(n);
    return (n, error.As(err)!);
}

private partial struct nopCloser : io.Writer {
    public ref io.Writer Writer => ref Writer_val;
}

private static error Close(this nopCloser w) {
    return error.As(null!)!;
}

private partial struct writeBuf { // : slice<byte>
}

private static void uint8(this ptr<writeBuf> _addr_b, byte v) {
    ref writeBuf b = ref _addr_b.val;

    (b.val)[0] = v;
    b.val = (b.val)[(int)1..];
}

private static void uint16(this ptr<writeBuf> _addr_b, ushort v) {
    ref writeBuf b = ref _addr_b.val;

    binary.LittleEndian.PutUint16(b.val, v);
    b.val = (b.val)[(int)2..];
}

private static void uint32(this ptr<writeBuf> _addr_b, uint v) {
    ref writeBuf b = ref _addr_b.val;

    binary.LittleEndian.PutUint32(b.val, v);
    b.val = (b.val)[(int)4..];
}

private static void uint64(this ptr<writeBuf> _addr_b, ulong v) {
    ref writeBuf b = ref _addr_b.val;

    binary.LittleEndian.PutUint64(b.val, v);
    b.val = (b.val)[(int)8..];
}

} // end zip_package
