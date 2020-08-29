// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tar -- go2cs converted at 2020 August 29 08:45:32 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\writer.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using path = go.path_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        // Writer provides sequential writing of a tar archive.
        // Write.WriteHeader begins a new file with the provided Header,
        // and then Writer can be treated as an io.Writer to supply that file's data.
        public partial struct Writer
        {
            public io.Writer w;
            public long pad; // Amount of padding to write after current file entry
            public fileWriter curr; // Writer for current file entry
            public Header hdr; // Shallow copy of Header that is safe for mutations
            public block blk; // Buffer to use as temporary local storage

// err is a persistent error.
// It is only the responsibility of every exported method of Writer to
// ensure that this error is sticky.
            public error err;
        }

        // NewWriter creates a new Writer writing to w.
        public static ref Writer NewWriter(io.Writer w)
        {
            return ref new Writer(w:w,curr:&regFileWriter{w,0});
        }

        private partial interface fileWriter : io.Writer, fileState
        {
            (long, error) ReadFrom(io.Reader _p0);
        }

        // Flush finishes writing the current file's block padding.
        // The current file must be fully written before Flush can be called.
        //
        // This is unnecessary as the next call to WriteHeader or Close
        // will implicitly flush out the file's padding.
        private static error Flush(this ref Writer tw)
        {
            if (tw.err != null)
            {
                return error.As(tw.err);
            }
            {
                var nb = tw.curr.LogicalRemaining();

                if (nb > 0L)
                {
                    return error.As(fmt.Errorf("archive/tar: missed writing %d bytes", nb));
                }

            }
            _, tw.err = tw.w.Write(zeroBlock[..tw.pad]);

            if (tw.err != null)
            {
                return error.As(tw.err);
            }
            tw.pad = 0L;
            return error.As(null);
        }

        // WriteHeader writes hdr and prepares to accept the file's contents.
        // The Header.Size determines how many bytes can be written for the next file.
        // If the current file is not fully written, then this returns an error.
        // This implicitly flushes any padding necessary before writing the header.
        private static error WriteHeader(this ref Writer tw, ref Header hdr)
        {
            {
                var err = tw.Flush();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            tw.hdr = hdr.Value; // Shallow copy of Header

            // Round ModTime and ignore AccessTime and ChangeTime unless
            // the format is explicitly chosen.
            // This ensures nominal usage of WriteHeader (without specifying the format)
            // does not always result in the PAX format being chosen, which
            // causes a 1KiB increase to every header.
            if (tw.hdr.Format == FormatUnknown)
            {
                tw.hdr.ModTime = tw.hdr.ModTime.Round(time.Second);
                tw.hdr.AccessTime = new time.Time();
                tw.hdr.ChangeTime = new time.Time();
            }
            var (allowedFormats, paxHdrs, err) = tw.hdr.allowedFormats();

            if (allowedFormats.has(FormatUSTAR)) 
                tw.err = tw.writeUSTARHeader(ref tw.hdr);
                return error.As(tw.err);
            else if (allowedFormats.has(FormatPAX)) 
                tw.err = tw.writePAXHeader(ref tw.hdr, paxHdrs);
                return error.As(tw.err);
            else if (allowedFormats.has(FormatGNU)) 
                tw.err = tw.writeGNUHeader(ref tw.hdr);
                return error.As(tw.err);
            else 
                return error.As(err); // Non-fatal error
                    }

        private static error writeUSTARHeader(this ref Writer tw, ref Header hdr)
        { 
            // Check if we can use USTAR prefix/suffix splitting.
            @string namePrefix = default;
            {
                var (prefix, suffix, ok) = splitUSTARPath(hdr.Name);

                if (ok)
                {
                    namePrefix = prefix;
                    hdr.Name = suffix;
                } 

                // Pack the main header.

            } 

            // Pack the main header.
            formatter f = default;
            var blk = tw.templateV7Plus(hdr, f.formatString, f.formatOctal);
            f.formatString(blk.USTAR().Prefix(), namePrefix);
            blk.SetFormat(FormatUSTAR);
            if (f.err != null)
            {
                return error.As(f.err); // Should never happen since header is validated
            }
            return error.As(tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag));
        }

        private static error writePAXHeader(this ref Writer tw, ref Header hdr, map<@string, @string> paxHdrs)
        {
            var realName = hdr.Name;
            var realSize = hdr.Size; 

            // TODO(dsnet): Re-enable this when adding sparse support.
            // See https://golang.org/issue/22735
            /*
                    // Handle sparse files.
                    var spd sparseDatas
                    var spb []byte
                    if len(hdr.SparseHoles) > 0 {
                        sph := append([]sparseEntry{}, hdr.SparseHoles...) // Copy sparse map
                        sph = alignSparseEntries(sph, hdr.Size)
                        spd = invertSparseEntries(sph, hdr.Size)

                        // Format the sparse map.
                        hdr.Size = 0 // Replace with encoded size
                        spb = append(strconv.AppendInt(spb, int64(len(spd)), 10), '\n')
                        for _, s := range spd {
                            hdr.Size += s.Length
                            spb = append(strconv.AppendInt(spb, s.Offset, 10), '\n')
                            spb = append(strconv.AppendInt(spb, s.Length, 10), '\n')
                        }
                        pad := blockPadding(int64(len(spb)))
                        spb = append(spb, zeroBlock[:pad]...)
                        hdr.Size += int64(len(spb)) // Accounts for encoded sparse map

                        // Add and modify appropriate PAX records.
                        dir, file := path.Split(realName)
                        hdr.Name = path.Join(dir, "GNUSparseFile.0", file)
                        paxHdrs[paxGNUSparseMajor] = "1"
                        paxHdrs[paxGNUSparseMinor] = "0"
                        paxHdrs[paxGNUSparseName] = realName
                        paxHdrs[paxGNUSparseRealSize] = strconv.FormatInt(realSize, 10)
                        paxHdrs[paxSize] = strconv.FormatInt(hdr.Size, 10)
                        delete(paxHdrs, paxPath) // Recorded by paxGNUSparseName
                    }
                */
            _ = realSize; 

            // Write PAX records to the output.
            var isGlobal = hdr.Typeflag == TypeXGlobalHeader;
            if (len(paxHdrs) > 0L || isGlobal)
            { 
                // Sort keys for deterministic ordering.
                slice<@string> keys = default;
                {
                    var k__prev1 = k;

                    foreach (var (__k) in paxHdrs)
                    {
                        k = __k;
                        keys = append(keys, k);
                    }

                    k = k__prev1;
                }

                sort.Strings(keys); 

                // Write each record to a buffer.
                bytes.Buffer buf = default;
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in keys)
                    {
                        k = __k;
                        var (rec, err) = formatPAXRecord(k, paxHdrs[k]);
                        if (err != null)
                        {
                            return error.As(err);
                        }
                        buf.WriteString(rec);
                    } 

                    // Write the extended header file.

                    k = k__prev1;
                }

                @string name = default;
                byte flag = default;
                if (isGlobal)
                {
                    name = realName;
                    if (name == "")
                    {
                        name = "GlobalHead.0.0";
                    }
                    flag = TypeXGlobalHeader;
                }
                else
                {
                    var (dir, file) = path.Split(realName);
                    name = path.Join(dir, "PaxHeaders.0", file);
                    flag = TypeXHeader;
                }
                var data = buf.String();
                {
                    var err__prev2 = err;

                    var err = tw.writeRawFile(name, data, flag, FormatPAX);

                    if (err != null || isGlobal)
                    {
                        return error.As(err); // Global headers return here
                    }

                    err = err__prev2;

                }
            } 

            // Pack the main header.
            formatter f = default; // Ignore errors since they are expected
            Action<slice<byte>, @string> fmtStr = (b, s) =>
            {
                f.formatString(b, toASCII(s));

            }
;
            var blk = tw.templateV7Plus(hdr, fmtStr, f.formatOctal);
            blk.SetFormat(FormatPAX);
            {
                var err__prev1 = err;

                err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag);

                if (err != null)
                {
                    return error.As(err);
                } 

                // TODO(dsnet): Re-enable this when adding sparse support.
                // See https://golang.org/issue/22735
                /*
                        // Write the sparse map and setup the sparse writer if necessary.
                        if len(spd) > 0 {
                            // Use tw.curr since the sparse map is accounted for in hdr.Size.
                            if _, err := tw.curr.Write(spb); err != nil {
                                return err
                            }
                            tw.curr = &sparseFileWriter{tw.curr, spd, 0}
                        }
                    */

                err = err__prev1;

            } 

            // TODO(dsnet): Re-enable this when adding sparse support.
            // See https://golang.org/issue/22735
            /*
                    // Write the sparse map and setup the sparse writer if necessary.
                    if len(spd) > 0 {
                        // Use tw.curr since the sparse map is accounted for in hdr.Size.
                        if _, err := tw.curr.Write(spb); err != nil {
                            return err
                        }
                        tw.curr = &sparseFileWriter{tw.curr, spd, 0}
                    }
                */
            return error.As(null);
        }

        private static error writeGNUHeader(this ref Writer tw, ref Header hdr)
        { 
            // Use long-link files if Name or Linkname exceeds the field size.
            const @string longName = "././@LongLink";

            if (len(hdr.Name) > nameSize)
            {
                var data = hdr.Name + "\x00";
                {
                    var err__prev2 = err;

                    var err = tw.writeRawFile(longName, data, TypeGNULongName, FormatGNU);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            if (len(hdr.Linkname) > nameSize)
            {
                data = hdr.Linkname + "\x00";
                {
                    var err__prev2 = err;

                    err = tw.writeRawFile(longName, data, TypeGNULongLink, FormatGNU);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            } 

            // Pack the main header.
            formatter f = default; // Ignore errors since they are expected
            sparseDatas spd = default;
            slice<byte> spb = default;
            var blk = tw.templateV7Plus(hdr, f.formatString, f.formatNumeric);
            if (!hdr.AccessTime.IsZero())
            {
                f.formatNumeric(blk.GNU().AccessTime(), hdr.AccessTime.Unix());
            }
            if (!hdr.ChangeTime.IsZero())
            {
                f.formatNumeric(blk.GNU().ChangeTime(), hdr.ChangeTime.Unix());
            } 
            // TODO(dsnet): Re-enable this when adding sparse support.
            // See https://golang.org/issue/22735
            /*
                    if hdr.Typeflag == TypeGNUSparse {
                        sph := append([]sparseEntry{}, hdr.SparseHoles...) // Copy sparse map
                        sph = alignSparseEntries(sph, hdr.Size)
                        spd = invertSparseEntries(sph, hdr.Size)

                        // Format the sparse map.
                        formatSPD := func(sp sparseDatas, sa sparseArray) sparseDatas {
                            for i := 0; len(sp) > 0 && i < sa.MaxEntries(); i++ {
                                f.formatNumeric(sa.Entry(i).Offset(), sp[0].Offset)
                                f.formatNumeric(sa.Entry(i).Length(), sp[0].Length)
                                sp = sp[1:]
                            }
                            if len(sp) > 0 {
                                sa.IsExtended()[0] = 1
                            }
                            return sp
                        }
                        sp2 := formatSPD(spd, blk.GNU().Sparse())
                        for len(sp2) > 0 {
                            var spHdr block
                            sp2 = formatSPD(sp2, spHdr.Sparse())
                            spb = append(spb, spHdr[:]...)
                        }

                        // Update size fields in the header block.
                        realSize := hdr.Size
                        hdr.Size = 0 // Encoded size; does not account for encoded sparse map
                        for _, s := range spd {
                            hdr.Size += s.Length
                        }
                        copy(blk.V7().Size(), zeroBlock[:]) // Reset field
                        f.formatNumeric(blk.V7().Size(), hdr.Size)
                        f.formatNumeric(blk.GNU().RealSize(), realSize)
                    }
                */
            blk.SetFormat(FormatGNU);
            {
                var err__prev1 = err;

                err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag);

                if (err != null)
                {
                    return error.As(err);
                } 

                // Write the extended sparse map and setup the sparse writer if necessary.

                err = err__prev1;

            } 

            // Write the extended sparse map and setup the sparse writer if necessary.
            if (len(spd) > 0L)
            { 
                // Use tw.w since the sparse map is not accounted for in hdr.Size.
                {
                    var err__prev2 = err;

                    var (_, err) = tw.w.Write(spb);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                tw.curr = ref new sparseFileWriter(tw.curr,spd,0);
            }
            return error.As(null);
        }

        public delegate void stringFormatter(slice<byte>, @string);
        public delegate void numberFormatter(slice<byte>, long);        private static ref block templateV7Plus(this ref Writer tw, ref Header hdr, stringFormatter fmtStr, numberFormatter fmtNum)
        {
            tw.blk.Reset();

            var modTime = hdr.ModTime;
            if (modTime.IsZero())
            {
                modTime = time.Unix(0L, 0L);
            }
            var v7 = tw.blk.V7();
            v7.TypeFlag()[0L] = hdr.Typeflag;
            fmtStr(v7.Name(), hdr.Name);
            fmtStr(v7.LinkName(), hdr.Linkname);
            fmtNum(v7.Mode(), hdr.Mode);
            fmtNum(v7.UID(), int64(hdr.Uid));
            fmtNum(v7.GID(), int64(hdr.Gid));
            fmtNum(v7.Size(), hdr.Size);
            fmtNum(v7.ModTime(), modTime.Unix());

            var ustar = tw.blk.USTAR();
            fmtStr(ustar.UserName(), hdr.Uname);
            fmtStr(ustar.GroupName(), hdr.Gname);
            fmtNum(ustar.DevMajor(), hdr.Devmajor);
            fmtNum(ustar.DevMinor(), hdr.Devminor);

            return ref tw.blk;
        }

        // writeRawFile writes a minimal file with the given name and flag type.
        // It uses format to encode the header format and will write data as the body.
        // It uses default values for all of the other fields (as BSD and GNU tar does).
        private static error writeRawFile(this ref Writer tw, @string name, @string data, byte flag, Format format)
        {
            tw.blk.Reset(); 

            // Best effort for the filename.
            name = toASCII(name);
            if (len(name) > nameSize)
            {
                name = name[..nameSize];
            }
            name = strings.TrimRight(name, "/");

            formatter f = default;
            var v7 = tw.blk.V7();
            v7.TypeFlag()[0L] = flag;
            f.formatString(v7.Name(), name);
            f.formatOctal(v7.Mode(), 0L);
            f.formatOctal(v7.UID(), 0L);
            f.formatOctal(v7.GID(), 0L);
            f.formatOctal(v7.Size(), int64(len(data))); // Must be < 8GiB
            f.formatOctal(v7.ModTime(), 0L);
            tw.blk.SetFormat(format);
            if (f.err != null)
            {
                return error.As(f.err); // Only occurs if size condition is violated
            } 

            // Write the header and data.
            {
                var err = tw.writeRawHeader(ref tw.blk, int64(len(data)), flag);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, err) = io.WriteString(tw, data);
            return error.As(err);
        }

        // writeRawHeader writes the value of blk, regardless of its value.
        // It sets up the Writer such that it can accept a file of the given size.
        // If the flag is a special header-only flag, then the size is treated as zero.
        private static error writeRawHeader(this ref Writer tw, ref block blk, long size, byte flag)
        {
            {
                var err = tw.Flush();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var (_, err) = tw.w.Write(blk[..]);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            if (isHeaderOnlyType(flag))
            {
                size = 0L;
            }
            tw.curr = ref new regFileWriter(tw.w,size);
            tw.pad = blockPadding(size);
            return error.As(null);
        }

        // splitUSTARPath splits a path according to USTAR prefix and suffix rules.
        // If the path is not splittable, then it will return ("", "", false).
        private static (@string, @string, bool) splitUSTARPath(@string name)
        {
            var length = len(name);
            if (length <= nameSize || !isASCII(name))
            {
                return ("", "", false);
            }
            else if (length > prefixSize + 1L)
            {
                length = prefixSize + 1L;
            }
            else if (name[length - 1L] == '/')
            {
                length--;
            }
            var i = strings.LastIndex(name[..length], "/");
            var nlen = len(name) - i - 1L; // nlen is length of suffix
            var plen = i; // plen is length of prefix
            if (i <= 0L || nlen > nameSize || nlen == 0L || plen > prefixSize)
            {
                return ("", "", false);
            }
            return (name[..i], name[i + 1L..], true);
        }

        // Write writes to the current file in the tar archive.
        // Write returns the error ErrWriteTooLong if more than
        // Header.Size bytes are written after WriteHeader.
        //
        // Calling Write on special types like TypeLink, TypeSymlink, TypeChar,
        // TypeBlock, TypeDir, and TypeFifo returns (0, ErrWriteTooLong) regardless
        // of what the Header.Size claims.
        private static (long, error) Write(this ref Writer tw, slice<byte> b)
        {
            if (tw.err != null)
            {
                return (0L, tw.err);
            }
            var (n, err) = tw.curr.Write(b);
            if (err != null && err != ErrWriteTooLong)
            {
                tw.err = err;
            }
            return (n, err);
        }

        // readFrom populates the content of the current file by reading from r.
        // The bytes read must match the number of remaining bytes in the current file.
        //
        // If the current file is sparse and r is an io.ReadSeeker,
        // then readFrom uses Seek to skip past holes defined in Header.SparseHoles,
        // assuming that skipped regions are all NULs.
        // This always reads the last byte to ensure r is the right size.
        //
        // TODO(dsnet): Re-export this when adding sparse file support.
        // See https://golang.org/issue/22735
        private static (long, error) readFrom(this ref Writer tw, io.Reader r)
        {
            if (tw.err != null)
            {
                return (0L, tw.err);
            }
            var (n, err) = tw.curr.ReadFrom(r);
            if (err != null && err != ErrWriteTooLong)
            {
                tw.err = err;
            }
            return (n, err);
        }

        // Close closes the tar archive by flushing the padding, and writing the footer.
        // If the current file (from a prior call to WriteHeader) is not fully written,
        // then this returns an error.
        private static error Close(this ref Writer tw)
        {
            if (tw.err == ErrWriteAfterClose)
            {
                return error.As(null);
            }
            if (tw.err != null)
            {
                return error.As(tw.err);
            } 

            // Trailer: two zero blocks.
            var err = tw.Flush();
            for (long i = 0L; i < 2L && err == null; i++)
            {
                _, err = tw.w.Write(zeroBlock[..]);
            } 

            // Ensure all future actions are invalid.
 

            // Ensure all future actions are invalid.
            tw.err = ErrWriteAfterClose;
            return error.As(err); // Report IO errors
        }

        // regFileWriter is a fileWriter for writing data to a regular file entry.
        private partial struct regFileWriter
        {
            public io.Writer w; // Underlying Writer
            public long nb; // Number of remaining bytes to write
        }

        private static (long, error) Write(this ref regFileWriter fw, slice<byte> b)
        {
            var overwrite = int64(len(b)) > fw.nb;
            if (overwrite)
            {
                b = b[..fw.nb];
            }
            if (len(b) > 0L)
            {
                n, err = fw.w.Write(b);
                fw.nb -= int64(n);
            }

            if (err != null) 
                return (n, err);
            else if (overwrite) 
                return (n, ErrWriteTooLong);
            else 
                return (n, null);
                    }

        private static (long, error) ReadFrom(this ref regFileWriter fw, io.Reader r)
        {
            return io.Copy(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Writer}{fw}, r);
        }

        private static long LogicalRemaining(this regFileWriter fw)
        {
            return fw.nb;
        }
        private static long PhysicalRemaining(this regFileWriter fw)
        {
            return fw.nb;
        }

        // sparseFileWriter is a fileWriter for writing data to a sparse file entry.
        private partial struct sparseFileWriter
        {
            public fileWriter fw; // Underlying fileWriter
            public sparseDatas sp; // Normalized list of data fragments
            public long pos; // Current position in sparse file
        }

        private static (long, error) Write(this ref sparseFileWriter sw, slice<byte> b)
        {
            var overwrite = int64(len(b)) > sw.LogicalRemaining();
            if (overwrite)
            {
                b = b[..sw.LogicalRemaining()];
            }
            var b0 = b;
            var endPos = sw.pos + int64(len(b));
            while (endPos > sw.pos && err == null)
            {
                long nf = default; // Bytes written in fragment
                var dataStart = sw.sp[0L].Offset;
                var dataEnd = sw.sp[0L].endOffset();
                if (sw.pos < dataStart)
                { // In a hole fragment
                    var bf = b[..min(int64(len(b)), dataStart - sw.pos)];
                    nf, err = new zeroWriter().Write(bf);
                }
                else
                { // In a data fragment
                    bf = b[..min(int64(len(b)), dataEnd - sw.pos)];
                    nf, err = sw.fw.Write(bf);
                }
                b = b[nf..];
                sw.pos += int64(nf);
                if (sw.pos >= dataEnd && len(sw.sp) > 1L)
                {
                    sw.sp = sw.sp[1L..]; // Ensure last fragment always remains
                }
            }


            n = len(b0) - len(b);

            if (err == ErrWriteTooLong) 
                return (n, errMissData); // Not possible; implies bug in validation logic
            else if (err != null) 
                return (n, err);
            else if (sw.LogicalRemaining() == 0L && sw.PhysicalRemaining() > 0L) 
                return (n, errUnrefData); // Not possible; implies bug in validation logic
            else if (overwrite) 
                return (n, ErrWriteTooLong);
            else 
                return (n, null);
                    }

        private static (long, error) ReadFrom(this ref sparseFileWriter sw, io.Reader r)
        {
            io.ReadSeeker (rs, ok) = r._<io.ReadSeeker>();
            if (ok)
            {
                {
                    var (_, err) = rs.Seek(0L, io.SeekCurrent);

                    if (err != null)
                    {
                        ok = false; // Not all io.Seeker can really seek
                    }

                }
            }
            if (!ok)
            {
                return io.Copy(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Writer}{sw}, r);
            }
            bool readLastByte = default;
            var pos0 = sw.pos;
            while (sw.LogicalRemaining() > 0L && !readLastByte && err == null)
            {
                long nf = default; // Size of fragment
                var dataStart = sw.sp[0L].Offset;
                var dataEnd = sw.sp[0L].endOffset();
                if (sw.pos < dataStart)
                { // In a hole fragment
                    nf = dataStart - sw.pos;
                    if (sw.PhysicalRemaining() == 0L)
                    {
                        readLastByte = true;
                        nf--;
                    }
                    _, err = rs.Seek(nf, io.SeekCurrent);
                }
                else
                { // In a data fragment
                    nf = dataEnd - sw.pos;
                    nf, err = io.CopyN(sw.fw, rs, nf);
                }
                sw.pos += nf;
                if (sw.pos >= dataEnd && len(sw.sp) > 1L)
                {
                    sw.sp = sw.sp[1L..]; // Ensure last fragment always remains
                }
            } 

            // If the last fragment is a hole, then seek to 1-byte before EOF, and
            // read a single byte to ensure the file is the right size.
 

            // If the last fragment is a hole, then seek to 1-byte before EOF, and
            // read a single byte to ensure the file is the right size.
            if (readLastByte && err == null)
            {
                _, err = mustReadFull(rs, new slice<byte>(new byte[] { 0 }));
                sw.pos++;
            }
            n = sw.pos - pos0;

            if (err == io.EOF) 
                return (n, io.ErrUnexpectedEOF);
            else if (err == ErrWriteTooLong) 
                return (n, errMissData); // Not possible; implies bug in validation logic
            else if (err != null) 
                return (n, err);
            else if (sw.LogicalRemaining() == 0L && sw.PhysicalRemaining() > 0L) 
                return (n, errUnrefData); // Not possible; implies bug in validation logic
            else 
                return (n, ensureEOF(rs));
                    }

        private static long LogicalRemaining(this sparseFileWriter sw)
        {
            return sw.sp[len(sw.sp) - 1L].endOffset() - sw.pos;
        }
        private static long PhysicalRemaining(this sparseFileWriter sw)
        {
            return sw.fw.PhysicalRemaining();
        }

        // zeroWriter may only be written with NULs, otherwise it returns errWriteHole.
        private partial struct zeroWriter
        {
        }

        private static (long, error) Write(this zeroWriter _p0, slice<byte> b)
        {
            foreach (var (i, c) in b)
            {
                if (c != 0L)
                {
                    return (i, errWriteHole);
                }
            }
            return (len(b), null);
        }

        // ensureEOF checks whether r is at EOF, reporting ErrWriteTooLong if not so.
        private static error ensureEOF(io.Reader r)
        {
            var (n, err) = tryReadFull(r, new slice<byte>(new byte[] { 0 }));

            if (n > 0L) 
                return error.As(ErrWriteTooLong);
            else if (err == io.EOF) 
                return error.As(null);
            else 
                return error.As(err);
                    }
    }
}}
