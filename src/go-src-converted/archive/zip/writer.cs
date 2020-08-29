// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package zip -- go2cs converted at 2020 August 29 08:45:40 UTC
// import "archive/zip" ==> using zip = go.archive.zip_package
// Original source: C:\Go\src\archive\zip\writer.go
using bufio = go.bufio_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;
using crc32 = go.hash.crc32_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace archive
{
    public static partial class zip_package
    {
        private static var errLongName = errors.New("zip: FileHeader.Name too long");        private static var errLongExtra = errors.New("zip: FileHeader.Extra too long");

        // Writer implements a zip file writer.
        public partial struct Writer
        {
            public ptr<countWriter> cw;
            public slice<ref header> dir;
            public ptr<fileWriter> last;
            public bool closed;
            public map<ushort, Compressor> compressors;
            public @string comment; // testHookCloseSizeOffset if non-nil is called with the size
// of offset of the central directory at Close.
            public Action<ulong, ulong> testHookCloseSizeOffset;
        }

        private partial struct header
        {
            public ref FileHeader FileHeader => ref FileHeader_ptr;
            public ulong offset;
        }

        // NewWriter returns a new Writer writing a zip file to w.
        public static ref Writer NewWriter(io.Writer w)
        {
            return ref new Writer(cw:&countWriter{w:bufio.NewWriter(w)});
        }

        // SetOffset sets the offset of the beginning of the zip data within the
        // underlying writer. It should be used when the zip data is appended to an
        // existing file, such as a binary executable.
        // It must be called before any data is written.
        private static void SetOffset(this ref Writer _w, long n) => func(_w, (ref Writer w, Defer _, Panic panic, Recover __) =>
        {
            if (w.cw.count != 0L)
            {
                panic("zip: SetOffset called after data was written");
            }
            w.cw.count = n;
        });

        // Flush flushes any buffered data to the underlying writer.
        // Calling Flush is not normally necessary; calling Close is sufficient.
        private static error Flush(this ref Writer w)
        {
            return error.As(w.cw.w._<ref bufio.Writer>().Flush());
        }

        // SetComment sets the end-of-central-directory comment field.
        // It can only be called before Close.
        private static error SetComment(this ref Writer w, @string comment)
        {
            if (len(comment) > uint16max)
            {
                return error.As(errors.New("zip: Writer.Comment too long"));
            }
            w.comment = comment;
            return error.As(null);
        }

        // Close finishes writing the zip file by writing the central directory.
        // It does not (and cannot) close the underlying writer.
        private static error Close(this ref Writer w)
        {
            if (w.last != null && !w.last.closed)
            {
                {
                    var err = w.last.close();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                w.last = null;
            }
            if (w.closed)
            {
                return error.As(errors.New("zip: writer closed twice"));
            }
            w.closed = true; 

            // write central directory
            var start = w.cw.count;
            foreach (var (_, h) in w.dir)
            {
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
                if (h.isZip64() || h.offset >= uint32max)
                { 
                    // the file needs a zip64 header. store maxint in both
                    // 32 bit size fields (and offset later) to signal that the
                    // zip64 extra header should be used.
                    b.uint32(uint32max); // compressed size
                    b.uint32(uint32max); // uncompressed size

                    // append a zip64 extra block to Extra
                    buf = new array<byte>(28L); // 2x uint16 + 3x uint64
                    var eb = writeBuf(buf[..]);
                    eb.uint16(zip64ExtraID);
                    eb.uint16(24L); // size = 3x uint64
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
                b = b[4L..]; // skip disk number start and internal file attr (2x uint16)
                b.uint32(h.ExternalAttrs);
                if (h.offset > uint32max)
                {
                    b.uint32(uint32max);
                }
                else
                {
                    b.uint32(uint32(h.offset));
                }
                {
                    var (_, err) = w.cw.Write(buf[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                {
                    (_, err) = io.WriteString(w.cw, h.Name);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                {
                    (_, err) = w.cw.Write(h.Extra);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                {
                    (_, err) = io.WriteString(w.cw, h.Comment);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            var end = w.cw.count;

            var records = uint64(len(w.dir));
            var size = uint64(end - start);
            var offset = uint64(start);

            {
                var f = w.testHookCloseSizeOffset;

                if (f != null)
                {
                    f(size, offset);
                }

            }

            if (records >= uint16max || size >= uint32max || offset >= uint32max)
            {
                buf = new array<byte>(directory64EndLen + directory64LocLen);
                b = writeBuf(buf[..]); 

                // zip64 end of central directory record
                b.uint32(directory64EndSignature);
                b.uint64(directory64EndLen - 12L); // length minus signature (uint32) and length fields (uint64)
                b.uint16(zipVersion45); // version made by
                b.uint16(zipVersion45); // version needed to extract
                b.uint32(0L); // number of this disk
                b.uint32(0L); // number of the disk with the start of the central directory
                b.uint64(records); // total number of entries in the central directory on this disk
                b.uint64(records); // total number of entries in the central directory
                b.uint64(size); // size of the central directory
                b.uint64(offset); // offset of start of central directory with respect to the starting disk number

                // zip64 end of central directory locator
                b.uint32(directory64LocSignature);
                b.uint32(0L); // number of the disk with the start of the zip64 end of central directory
                b.uint64(uint64(end)); // relative offset of the zip64 end of central directory record
                b.uint32(1L); // total number of disks

                {
                    (_, err) = w.cw.Write(buf[..]);

                    if (err != null)
                    {
                        return error.As(err);
                    } 

                    // store max values in the regular end record to signal that
                    // that the zip64 values should be used instead

                } 

                // store max values in the regular end record to signal that
                // that the zip64 values should be used instead
                records = uint16max;
                size = uint32max;
                offset = uint32max;
            } 

            // write end record
            buf = new array<byte>(directoryEndLen);
            b = writeBuf(buf[..]);
            b.uint32(uint32(directoryEndSignature));
            b = b[4L..]; // skip over disk number and first disk number (2x uint16)
            b.uint16(uint16(records)); // number of entries this disk
            b.uint16(uint16(records)); // number of entries total
            b.uint32(uint32(size)); // size of directory
            b.uint32(uint32(offset)); // start of directory
            b.uint16(uint16(len(w.comment))); // byte size of EOCD comment
            {
                (_, err) = w.cw.Write(buf[..]);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                (_, err) = io.WriteString(w.cw, w.comment);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            return error.As(w.cw.w._<ref bufio.Writer>().Flush());
        }

        // Create adds a file to the zip file using the provided name.
        // It returns a Writer to which the file contents should be written.
        // The file contents will be compressed using the Deflate method.
        // The name must be a relative path: it must not start with a drive
        // letter (e.g. C:) or leading slash, and only forward slashes are
        // allowed.
        // The file's contents must be written to the io.Writer before the next
        // call to Create, CreateHeader, or Close.
        private static (io.Writer, error) Create(this ref Writer w, @string name)
        {
            FileHeader header = ref new FileHeader(Name:name,Method:Deflate,);
            return w.CreateHeader(header);
        }

        // detectUTF8 reports whether s is a valid UTF-8 string, and whether the string
        // must be considered UTF-8 encoding (i.e., not compatible with CP-437, ASCII,
        // or any other common encoding).
        private static (bool, bool) detectUTF8(@string s)
        {
            {
                long i = 0L;

                while (i < len(s))
                {
                    var (r, size) = utf8.DecodeRuneInString(s[i..]);
                    i += size; 
                    // Officially, ZIP uses CP-437, but many readers use the system's
                    // local character encoding. Most encoding are compatible with a large
                    // subset of CP-437, which itself is ASCII-like.
                    //
                    // Forbid 0x7e and 0x5c since EUC-KR and Shift-JIS replace those
                    // characters with localized currency and overline characters.
                    if (r < 0x20UL || r > 0x7dUL || r == 0x5cUL)
                    {
                        if (!utf8.ValidRune(r) || (r == utf8.RuneError && size == 1L))
                        {
                            return (false, false);
                        }
                        require = true;
                    }
                }

            }
            return (true, require);
        }

        // CreateHeader adds a file to the zip archive using the provided FileHeader
        // for the file metadata. Writer takes ownership of fh and may mutate
        // its fields. The caller must not modify fh after calling CreateHeader.
        //
        // This returns a Writer to which the file contents should be written.
        // The file's contents must be written to the io.Writer before the next
        // call to Create, CreateHeader, or Close.
        private static (io.Writer, error) CreateHeader(this ref Writer w, ref FileHeader fh)
        {
            if (w.last != null && !w.last.closed)
            {
                {
                    var err__prev2 = err;

                    var err = w.last.close();

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev2;

                }
            }
            if (len(w.dir) > 0L && w.dir[len(w.dir) - 1L].FileHeader == fh)
            { 
                // See https://golang.org/issue/11144 confusion.
                return (null, errors.New("archive/zip: invalid duplicate FileHeader"));
            }
            fh.Flags |= 0x8UL; // we will write a data descriptor

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
                fh.Flags &= 0x800UL;
            else if ((utf8Require1 || utf8Require2) && (utf8Valid1 && utf8Valid2)) 
                fh.Flags |= 0x800UL;
                        fh.CreatorVersion = fh.CreatorVersion & 0xff00UL | zipVersion20; // preserve compatibility byte
            fh.ReaderVersion = zipVersion20; 

            // If Modified is set, this takes precedence over MS-DOS timestamp fields.
            if (!fh.Modified.IsZero())
            { 
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
                array<byte> mbuf = new array<byte>(9L); // 2*SizeOf(uint16) + SizeOf(uint8) + SizeOf(uint32)
                var mt = uint32(fh.Modified.Unix());
                var eb = writeBuf(mbuf[..]);
                eb.uint16(extTimeExtraID);
                eb.uint16(5L); // Size: SizeOf(uint8) + SizeOf(uint32)
                eb.uint8(1L); // Flags: ModTime
                eb.uint32(mt); // ModTime
                fh.Extra = append(fh.Extra, mbuf[..]);
            }
            fileWriter fw = ref new fileWriter(zipw:w.cw,compCount:&countWriter{w:w.cw},crc32:crc32.NewIEEE(),);
            var comp = w.compressor(fh.Method);
            if (comp == null)
            {
                return (null, ErrAlgorithm);
            }
            err = default;
            fw.comp, err = comp(fw.compCount);
            if (err != null)
            {
                return (null, err);
            }
            fw.rawCount = ref new countWriter(w:fw.comp);

            header h = ref new header(FileHeader:fh,offset:uint64(w.cw.count),);
            w.dir = append(w.dir, h);
            fw.header = h;

            {
                var err__prev1 = err;

                err = writeHeader(w.cw, fh);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }

            w.last = fw;
            return (fw, null);
        }

        private static error writeHeader(io.Writer w, ref FileHeader h)
        {
            const long maxUint16 = 1L << (int)(16L) - 1L;

            if (len(h.Name) > maxUint16)
            {
                return error.As(errLongName);
            }
            if (len(h.Extra) > maxUint16)
            {
                return error.As(errLongExtra);
            }
            array<byte> buf = new array<byte>(fileHeaderLen);
            var b = writeBuf(buf[..]);
            b.uint32(uint32(fileHeaderSignature));
            b.uint16(h.ReaderVersion);
            b.uint16(h.Flags);
            b.uint16(h.Method);
            b.uint16(h.ModifiedTime);
            b.uint16(h.ModifiedDate);
            b.uint32(0L); // since we are writing a data descriptor crc32,
            b.uint32(0L); // compressed size,
            b.uint32(0L); // and uncompressed size should be zero
            b.uint16(uint16(len(h.Name)));
            b.uint16(uint16(len(h.Extra)));
            {
                var (_, err) = w.Write(buf[..]);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                (_, err) = io.WriteString(w, h.Name);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            (_, err) = w.Write(h.Extra);
            return error.As(err);
        }

        // RegisterCompressor registers or overrides a custom compressor for a specific
        // method ID. If a compressor for a given method is not found, Writer will
        // default to looking up the compressor at the package level.
        private static void RegisterCompressor(this ref Writer w, ushort method, Compressor comp)
        {
            if (w.compressors == null)
            {
                w.compressors = make_map<ushort, Compressor>();
            }
            w.compressors[method] = comp;
        }

        private static Compressor compressor(this ref Writer w, ushort method)
        {
            var comp = w.compressors[method];
            if (comp == null)
            {
                comp = compressor(method);
            }
            return comp;
        }

        private partial struct fileWriter
        {
            public ref header header => ref header_ptr;
            public io.Writer zipw;
            public ptr<countWriter> rawCount;
            public io.WriteCloser comp;
            public ptr<countWriter> compCount;
            public hash.Hash32 crc32;
            public bool closed;
        }

        private static (long, error) Write(this ref fileWriter w, slice<byte> p)
        {
            if (w.closed)
            {
                return (0L, errors.New("zip: write to closed file"));
            }
            w.crc32.Write(p);
            return w.rawCount.Write(p);
        }

        private static error close(this ref fileWriter w)
        {
            if (w.closed)
            {
                return error.As(errors.New("zip: file closed twice"));
            }
            w.closed = true;
            {
                var err = w.comp.Close();

                if (err != null)
                {
                    return error.As(err);
                } 

                // update FileHeader

            } 

            // update FileHeader
            var fh = w.header.FileHeader;
            fh.CRC32 = w.crc32.Sum32();
            fh.CompressedSize64 = uint64(w.compCount.count);
            fh.UncompressedSize64 = uint64(w.rawCount.count);

            if (fh.isZip64())
            {
                fh.CompressedSize = uint32max;
                fh.UncompressedSize = uint32max;
                fh.ReaderVersion = zipVersion45; // requires 4.5 - File uses ZIP64 format extensions
            }
            else
            {
                fh.CompressedSize = uint32(fh.CompressedSize64);
                fh.UncompressedSize = uint32(fh.UncompressedSize64);
            } 

            // Write data descriptor. This is more complicated than one would
            // think, see e.g. comments in zipfile.c:putextended() and
            // http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=7073588.
            // The approach here is to write 8 byte sizes if needed without
            // adding a zip64 extra in the local header (too late anyway).
            slice<byte> buf = default;
            if (fh.isZip64())
            {
                buf = make_slice<byte>(dataDescriptor64Len);
            }
            else
            {
                buf = make_slice<byte>(dataDescriptorLen);
            }
            var b = writeBuf(buf);
            b.uint32(dataDescriptorSignature); // de-facto standard, required by OS X
            b.uint32(fh.CRC32);
            if (fh.isZip64())
            {
                b.uint64(fh.CompressedSize64);
                b.uint64(fh.UncompressedSize64);
            }
            else
            {
                b.uint32(fh.CompressedSize);
                b.uint32(fh.UncompressedSize);
            }
            var (_, err) = w.zipw.Write(buf);
            return error.As(err);
        }

        private partial struct countWriter
        {
            public io.Writer w;
            public long count;
        }

        private static (long, error) Write(this ref countWriter w, slice<byte> p)
        {
            var (n, err) = w.w.Write(p);
            w.count += int64(n);
            return (n, err);
        }

        private partial struct nopCloser : io.Writer
        {
            public ref io.Writer Writer => ref Writer_val;
        }

        private static error Close(this nopCloser w)
        {
            return error.As(null);
        }

        private partial struct writeBuf // : slice<byte>
        {
        }

        private static void uint8(this ref writeBuf b, byte v)
        {
            (b.Value)[0L] = v;
            b.Value = (b.Value)[1L..];
        }

        private static void uint16(this ref writeBuf b, ushort v)
        {
            binary.LittleEndian.PutUint16(b.Value, v);
            b.Value = (b.Value)[2L..];
        }

        private static void uint32(this ref writeBuf b, uint v)
        {
            binary.LittleEndian.PutUint32(b.Value, v);
            b.Value = (b.Value)[4L..];
        }

        private static void uint64(this ref writeBuf b, ulong v)
        {
            binary.LittleEndian.PutUint64(b.Value, v);
            b.Value = (b.Value)[8L..];
        }
    }
}}
