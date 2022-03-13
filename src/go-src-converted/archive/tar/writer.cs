// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tar -- go2cs converted at 2022 March 13 05:42:30 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Program Files\Go\src\archive\tar\writer.go
namespace go.archive;

using fmt = fmt_package;
using io = io_package;
using path = path_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;


// Writer provides sequential writing of a tar archive.
// Write.WriteHeader begins a new file with the provided Header,
// and then Writer can be treated as an io.Writer to supply that file's data.

using System;
public static partial class tar_package {

public partial struct Writer {
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
public static ptr<Writer> NewWriter(io.Writer w) {
    return addr(new Writer(w:w,curr:&regFileWriter{w,0}));
}

private partial interface fileWriter {
    (long, error) ReadFrom(io.Reader _p0);
}

// Flush finishes writing the current file's block padding.
// The current file must be fully written before Flush can be called.
//
// This is unnecessary as the next call to WriteHeader or Close
// will implicitly flush out the file's padding.
private static error Flush(this ptr<Writer> _addr_tw) {
    ref Writer tw = ref _addr_tw.val;

    if (tw.err != null) {
        return error.As(tw.err)!;
    }
    {
        var nb = tw.curr.LogicalRemaining();

        if (nb > 0) {
            return error.As(fmt.Errorf("archive/tar: missed writing %d bytes", nb))!;
        }
    }
    _, tw.err = tw.w.Write(zeroBlock[..(int)tw.pad]);

    if (tw.err != null) {
        return error.As(tw.err)!;
    }
    tw.pad = 0;
    return error.As(null!)!;
}

// WriteHeader writes hdr and prepares to accept the file's contents.
// The Header.Size determines how many bytes can be written for the next file.
// If the current file is not fully written, then this returns an error.
// This implicitly flushes any padding necessary before writing the header.
private static error WriteHeader(this ptr<Writer> _addr_tw, ptr<Header> _addr_hdr) {
    ref Writer tw = ref _addr_tw.val;
    ref Header hdr = ref _addr_hdr.val;

    {
        var err = tw.Flush();

        if (err != null) {
            return error.As(err)!;
        }
    }
    tw.hdr = hdr; // Shallow copy of Header

    // Avoid usage of the legacy TypeRegA flag, and automatically promote
    // it to use TypeReg or TypeDir.
    if (tw.hdr.Typeflag == TypeRegA) {
        if (strings.HasSuffix(tw.hdr.Name, "/")) {
            tw.hdr.Typeflag = TypeDir;
        }
        else
 {
            tw.hdr.Typeflag = TypeReg;
        }
    }
    if (tw.hdr.Format == FormatUnknown) {
        tw.hdr.ModTime = tw.hdr.ModTime.Round(time.Second);
        tw.hdr.AccessTime = new time.Time();
        tw.hdr.ChangeTime = new time.Time();
    }
    var (allowedFormats, paxHdrs, err) = tw.hdr.allowedFormats();

    if (allowedFormats.has(FormatUSTAR)) 
        tw.err = tw.writeUSTARHeader(_addr_tw.hdr);
        return error.As(tw.err)!;
    else if (allowedFormats.has(FormatPAX)) 
        tw.err = tw.writePAXHeader(_addr_tw.hdr, paxHdrs);
        return error.As(tw.err)!;
    else if (allowedFormats.has(FormatGNU)) 
        tw.err = tw.writeGNUHeader(_addr_tw.hdr);
        return error.As(tw.err)!;
    else 
        return error.As(err)!; // Non-fatal error
    }

private static error writeUSTARHeader(this ptr<Writer> _addr_tw, ptr<Header> _addr_hdr) {
    ref Writer tw = ref _addr_tw.val;
    ref Header hdr = ref _addr_hdr.val;
 
    // Check if we can use USTAR prefix/suffix splitting.
    @string namePrefix = default;
    {
        var (prefix, suffix, ok) = splitUSTARPath(hdr.Name);

        if (ok) {
            (namePrefix, hdr.Name) = (prefix, suffix);
        }
    } 

    // Pack the main header.
    formatter f = default;
    var blk = tw.templateV7Plus(hdr, f.formatString, f.formatOctal);
    f.formatString(blk.USTAR().Prefix(), namePrefix);
    blk.SetFormat(FormatUSTAR);
    if (f.err != null) {
        return error.As(f.err)!; // Should never happen since header is validated
    }
    return error.As(tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag))!;
}

private static error writePAXHeader(this ptr<Writer> _addr_tw, ptr<Header> _addr_hdr, map<@string, @string> paxHdrs) {
    ref Writer tw = ref _addr_tw.val;
    ref Header hdr = ref _addr_hdr.val;

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
    if (len(paxHdrs) > 0 || isGlobal) { 
        // Sort keys for deterministic ordering.
        slice<@string> keys = default;
        {
            var k__prev1 = k;

            foreach (var (__k) in paxHdrs) {
                k = __k;
                keys = append(keys, k);
            }

            k = k__prev1;
        }

        sort.Strings(keys); 

        // Write each record to a buffer.
        strings.Builder buf = default;
        {
            var k__prev1 = k;

            foreach (var (_, __k) in keys) {
                k = __k;
                var (rec, err) = formatPAXRecord(k, paxHdrs[k]);
                if (err != null) {
                    return error.As(err)!;
                }
                buf.WriteString(rec);
            } 

            // Write the extended header file.

            k = k__prev1;
        }

        @string name = default;
        byte flag = default;
        if (isGlobal) {
            name = realName;
            if (name == "") {
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

            if (err != null || isGlobal) {
                return error.As(err)!; // Global headers return here
            }

            err = err__prev2;

        }
    }
    formatter f = default; // Ignore errors since they are expected
    Action<slice<byte>, @string> fmtStr = (b, s) => {
        f.formatString(b, toASCII(s));
    };
    var blk = tw.templateV7Plus(hdr, fmtStr, f.formatOctal);
    blk.SetFormat(FormatPAX);
    {
        var err__prev1 = err;

        err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag);

        if (err != null) {
            return error.As(err)!;
        }
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
    return error.As(null!)!;
}

private static error writeGNUHeader(this ptr<Writer> _addr_tw, ptr<Header> _addr_hdr) {
    ref Writer tw = ref _addr_tw.val;
    ref Header hdr = ref _addr_hdr.val;
 
    // Use long-link files if Name or Linkname exceeds the field size.
    const @string longName = "././@LongLink";

    if (len(hdr.Name) > nameSize) {
        var data = hdr.Name + "\x00";
        {
            var err__prev2 = err;

            var err = tw.writeRawFile(longName, data, TypeGNULongName, FormatGNU);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    if (len(hdr.Linkname) > nameSize) {
        data = hdr.Linkname + "\x00";
        {
            var err__prev2 = err;

            err = tw.writeRawFile(longName, data, TypeGNULongLink, FormatGNU);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    formatter f = default; // Ignore errors since they are expected
    sparseDatas spd = default;
    slice<byte> spb = default;
    var blk = tw.templateV7Plus(hdr, f.formatString, f.formatNumeric);
    if (!hdr.AccessTime.IsZero()) {
        f.formatNumeric(blk.GNU().AccessTime(), hdr.AccessTime.Unix());
    }
    if (!hdr.ChangeTime.IsZero()) {
        f.formatNumeric(blk.GNU().ChangeTime(), hdr.ChangeTime.Unix());
    }
    blk.SetFormat(FormatGNU);
    {
        var err__prev1 = err;

        err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Write the extended sparse map and setup the sparse writer if necessary.
    if (len(spd) > 0) { 
        // Use tw.w since the sparse map is not accounted for in hdr.Size.
        {
            var err__prev2 = err;

            var (_, err) = tw.w.Write(spb);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
        tw.curr = addr(new sparseFileWriter(tw.curr,spd,0));
    }
    return error.As(null!)!;
}

public delegate void stringFormatter(slice<byte>, @string);
public delegate void numberFormatter(slice<byte>, long);private static ptr<block> templateV7Plus(this ptr<Writer> _addr_tw, ptr<Header> _addr_hdr, stringFormatter fmtStr, numberFormatter fmtNum) {
    ref Writer tw = ref _addr_tw.val;
    ref Header hdr = ref _addr_hdr.val;

    tw.blk.Reset();

    var modTime = hdr.ModTime;
    if (modTime.IsZero()) {
        modTime = time.Unix(0, 0);
    }
    var v7 = tw.blk.V7();
    v7.TypeFlag()[0] = hdr.Typeflag;
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

    return _addr__addr_tw.blk!;
}

// writeRawFile writes a minimal file with the given name and flag type.
// It uses format to encode the header format and will write data as the body.
// It uses default values for all of the other fields (as BSD and GNU tar does).
private static error writeRawFile(this ptr<Writer> _addr_tw, @string name, @string data, byte flag, Format format) {
    ref Writer tw = ref _addr_tw.val;

    tw.blk.Reset(); 

    // Best effort for the filename.
    name = toASCII(name);
    if (len(name) > nameSize) {
        name = name[..(int)nameSize];
    }
    name = strings.TrimRight(name, "/");

    formatter f = default;
    var v7 = tw.blk.V7();
    v7.TypeFlag()[0] = flag;
    f.formatString(v7.Name(), name);
    f.formatOctal(v7.Mode(), 0);
    f.formatOctal(v7.UID(), 0);
    f.formatOctal(v7.GID(), 0);
    f.formatOctal(v7.Size(), int64(len(data))); // Must be < 8GiB
    f.formatOctal(v7.ModTime(), 0);
    tw.blk.SetFormat(format);
    if (f.err != null) {
        return error.As(f.err)!; // Only occurs if size condition is violated
    }
    {
        var err = tw.writeRawHeader(_addr_tw.blk, int64(len(data)), flag);

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, err) = io.WriteString(tw, data);
    return error.As(err)!;
}

// writeRawHeader writes the value of blk, regardless of its value.
// It sets up the Writer such that it can accept a file of the given size.
// If the flag is a special header-only flag, then the size is treated as zero.
private static error writeRawHeader(this ptr<Writer> _addr_tw, ptr<block> _addr_blk, long size, byte flag) {
    ref Writer tw = ref _addr_tw.val;
    ref block blk = ref _addr_blk.val;

    {
        var err = tw.Flush();

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        var (_, err) = tw.w.Write(blk[..]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    if (isHeaderOnlyType(flag)) {
        size = 0;
    }
    tw.curr = addr(new regFileWriter(tw.w,size));
    tw.pad = blockPadding(size);
    return error.As(null!)!;
}

// splitUSTARPath splits a path according to USTAR prefix and suffix rules.
// If the path is not splittable, then it will return ("", "", false).
private static (@string, @string, bool) splitUSTARPath(@string name) {
    @string prefix = default;
    @string suffix = default;
    bool ok = default;

    var length = len(name);
    if (length <= nameSize || !isASCII(name)) {
        return ("", "", false);
    }
    else if (length > prefixSize + 1) {
        length = prefixSize + 1;
    }
    else if (name[length - 1] == '/') {
        length--;
    }
    var i = strings.LastIndex(name[..(int)length], "/");
    var nlen = len(name) - i - 1; // nlen is length of suffix
    var plen = i; // plen is length of prefix
    if (i <= 0 || nlen > nameSize || nlen == 0 || plen > prefixSize) {
        return ("", "", false);
    }
    return (name[..(int)i], name[(int)i + 1..], true);
}

// Write writes to the current file in the tar archive.
// Write returns the error ErrWriteTooLong if more than
// Header.Size bytes are written after WriteHeader.
//
// Calling Write on special types like TypeLink, TypeSymlink, TypeChar,
// TypeBlock, TypeDir, and TypeFifo returns (0, ErrWriteTooLong) regardless
// of what the Header.Size claims.
private static (nint, error) Write(this ptr<Writer> _addr_tw, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Writer tw = ref _addr_tw.val;

    if (tw.err != null) {
        return (0, error.As(tw.err)!);
    }
    var (n, err) = tw.curr.Write(b);
    if (err != null && err != ErrWriteTooLong) {
        tw.err = err;
    }
    return (n, error.As(err)!);
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
private static (long, error) readFrom(this ptr<Writer> _addr_tw, io.Reader r) {
    long _p0 = default;
    error _p0 = default!;
    ref Writer tw = ref _addr_tw.val;

    if (tw.err != null) {
        return (0, error.As(tw.err)!);
    }
    var (n, err) = tw.curr.ReadFrom(r);
    if (err != null && err != ErrWriteTooLong) {
        tw.err = err;
    }
    return (n, error.As(err)!);
}

// Close closes the tar archive by flushing the padding, and writing the footer.
// If the current file (from a prior call to WriteHeader) is not fully written,
// then this returns an error.
private static error Close(this ptr<Writer> _addr_tw) {
    ref Writer tw = ref _addr_tw.val;

    if (tw.err == ErrWriteAfterClose) {
        return error.As(null!)!;
    }
    if (tw.err != null) {
        return error.As(tw.err)!;
    }
    var err = tw.Flush();
    for (nint i = 0; i < 2 && err == null; i++) {
        _, err = tw.w.Write(zeroBlock[..]);
    } 

    // Ensure all future actions are invalid.
    tw.err = ErrWriteAfterClose;
    return error.As(err)!; // Report IO errors
}

// regFileWriter is a fileWriter for writing data to a regular file entry.
private partial struct regFileWriter {
    public io.Writer w; // Underlying Writer
    public long nb; // Number of remaining bytes to write
}

private static (nint, error) Write(this ptr<regFileWriter> _addr_fw, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref regFileWriter fw = ref _addr_fw.val;

    var overwrite = int64(len(b)) > fw.nb;
    if (overwrite) {
        b = b[..(int)fw.nb];
    }
    if (len(b) > 0) {
        n, err = fw.w.Write(b);
        fw.nb -= int64(n);
    }

    if (err != null) 
        return (n, error.As(err)!);
    else if (overwrite) 
        return (n, error.As(ErrWriteTooLong)!);
    else 
        return (n, error.As(null!)!);
    }

private static (long, error) ReadFrom(this ptr<regFileWriter> _addr_fw, io.Reader r) {
    long _p0 = default;
    error _p0 = default!;
    ref regFileWriter fw = ref _addr_fw.val;

    return io.Copy(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Writer}{fw}, r);
}

private static long LogicalRemaining(this regFileWriter fw) {
    return fw.nb;
}
private static long PhysicalRemaining(this regFileWriter fw) {
    return fw.nb;
}

// sparseFileWriter is a fileWriter for writing data to a sparse file entry.
private partial struct sparseFileWriter {
    public fileWriter fw; // Underlying fileWriter
    public sparseDatas sp; // Normalized list of data fragments
    public long pos; // Current position in sparse file
}

private static (nint, error) Write(this ptr<sparseFileWriter> _addr_sw, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref sparseFileWriter sw = ref _addr_sw.val;

    var overwrite = int64(len(b)) > sw.LogicalRemaining();
    if (overwrite) {
        b = b[..(int)sw.LogicalRemaining()];
    }
    var b0 = b;
    var endPos = sw.pos + int64(len(b));
    while (endPos > sw.pos && err == null) {
        nint nf = default; // Bytes written in fragment
        var dataStart = sw.sp[0].Offset;
        var dataEnd = sw.sp[0].endOffset();
        if (sw.pos < dataStart) { // In a hole fragment
            var bf = b[..(int)min(int64(len(b)), dataStart - sw.pos)];
            nf, err = new zeroWriter().Write(bf);
        }
        else
 { // In a data fragment
            bf = b[..(int)min(int64(len(b)), dataEnd - sw.pos)];
            nf, err = sw.fw.Write(bf);
        }
        b = b[(int)nf..];
        sw.pos += int64(nf);
        if (sw.pos >= dataEnd && len(sw.sp) > 1) {
            sw.sp = sw.sp[(int)1..]; // Ensure last fragment always remains
        }
    }

    n = len(b0) - len(b);

    if (err == ErrWriteTooLong) 
        return (n, error.As(errMissData)!); // Not possible; implies bug in validation logic
    else if (err != null) 
        return (n, error.As(err)!);
    else if (sw.LogicalRemaining() == 0 && sw.PhysicalRemaining() > 0) 
        return (n, error.As(errUnrefData)!); // Not possible; implies bug in validation logic
    else if (overwrite) 
        return (n, error.As(ErrWriteTooLong)!);
    else 
        return (n, error.As(null!)!);
    }

private static (long, error) ReadFrom(this ptr<sparseFileWriter> _addr_sw, io.Reader r) {
    long n = default;
    error err = default!;
    ref sparseFileWriter sw = ref _addr_sw.val;

    io.ReadSeeker (rs, ok) = r._<io.ReadSeeker>();
    if (ok) {
        {
            var (_, err) = rs.Seek(0, io.SeekCurrent);

            if (err != null) {
                ok = false; // Not all io.Seeker can really seek
            }

        }
    }
    if (!ok) {
        return io.Copy(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Writer}{sw}, r);
    }
    bool readLastByte = default;
    var pos0 = sw.pos;
    while (sw.LogicalRemaining() > 0 && !readLastByte && err == null) {
        long nf = default; // Size of fragment
        var dataStart = sw.sp[0].Offset;
        var dataEnd = sw.sp[0].endOffset();
        if (sw.pos < dataStart) { // In a hole fragment
            nf = dataStart - sw.pos;
            if (sw.PhysicalRemaining() == 0) {
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
        if (sw.pos >= dataEnd && len(sw.sp) > 1) {
            sw.sp = sw.sp[(int)1..]; // Ensure last fragment always remains
        }
    } 

    // If the last fragment is a hole, then seek to 1-byte before EOF, and
    // read a single byte to ensure the file is the right size.
    if (readLastByte && err == null) {
        _, err = mustReadFull(rs, new slice<byte>(new byte[] { 0 }));
        sw.pos++;
    }
    n = sw.pos - pos0;

    if (err == io.EOF) 
        return (n, error.As(io.ErrUnexpectedEOF)!);
    else if (err == ErrWriteTooLong) 
        return (n, error.As(errMissData)!); // Not possible; implies bug in validation logic
    else if (err != null) 
        return (n, error.As(err)!);
    else if (sw.LogicalRemaining() == 0 && sw.PhysicalRemaining() > 0) 
        return (n, error.As(errUnrefData)!); // Not possible; implies bug in validation logic
    else 
        return (n, error.As(ensureEOF(rs))!);
    }

private static long LogicalRemaining(this sparseFileWriter sw) {
    return sw.sp[len(sw.sp) - 1].endOffset() - sw.pos;
}
private static long PhysicalRemaining(this sparseFileWriter sw) {
    return sw.fw.PhysicalRemaining();
}

// zeroWriter may only be written with NULs, otherwise it returns errWriteHole.
private partial struct zeroWriter {
}

private static (nint, error) Write(this zeroWriter _p0, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;

    foreach (var (i, c) in b) {
        if (c != 0) {
            return (i, error.As(errWriteHole)!);
        }
    }    return (len(b), error.As(null!)!);
}

// ensureEOF checks whether r is at EOF, reporting ErrWriteTooLong if not so.
private static error ensureEOF(io.Reader r) {
    var (n, err) = tryReadFull(r, new slice<byte>(new byte[] { 0 }));

    if (n > 0) 
        return error.As(ErrWriteTooLong)!;
    else if (err == io.EOF) 
        return error.As(null!)!;
    else 
        return error.As(err)!;
    }

} // end tar_package
