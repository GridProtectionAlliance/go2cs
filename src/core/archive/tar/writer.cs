// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using path = path_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;
using io;

partial class tar_package {

// Writer provides sequential writing of a tar archive.
// [Writer.WriteHeader] begins a new file with the provided [Header],
// and then Writer can be treated as an io.Writer to supply that file's data.
[GoType] partial struct Writer {
    internal io_package.Writer w;
    internal int64 pad;      // Amount of padding to write after current file entry
    internal fileWriter curr; // Writer for current file entry
    internal Header hdr;     // Shallow copy of Header that is safe for mutations
    internal block blk;      // Buffer to use as temporary local storage
    // err is a persistent error.
    // It is only the responsibility of every exported method of Writer to
    // ensure that this error is sticky.
    internal error err;
}

// NewWriter creates a new Writer writing to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(w: w, curr: Ꮡ(new regFileWriter(w, 0))));
}

[GoType] partial interface fileWriter :
    io.Writer,
    fileState
{
    (int64, error) ReadFrom(io.Reader _);
}

// Flush finishes writing the current file's block padding.
// The current file must be fully written before Flush can be called.
//
// This is unnecessary as the next call to [Writer.WriteHeader] or [Writer.Close]
// will implicitly flush out the file's padding.
[GoRecv] public static error Flush(this ref Writer tw) {
    if (tw.err != default!) {
        return tw.err;
    }
    {
        var nb = tw.curr.logicalRemaining(); if (nb > 0) {
            return fmt.Errorf("archive/tar: missed writing %d bytes"u8, nb);
        }
    }
    {
        var (_, tw.err) = tw.w.Write(zeroBlock[..(int)(tw.pad)]); if (tw.err != default!) {
            return tw.err;
        }
    }
    tw.pad = 0;
    return default!;
}

// WriteHeader writes hdr and prepares to accept the file's contents.
// The Header.Size determines how many bytes can be written for the next file.
// If the current file is not fully written, then this returns an error.
// This implicitly flushes any padding necessary before writing the header.
[GoRecv] public static error WriteHeader(this ref Writer tw, ж<Header> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.val;

    {
        var errΔ1 = tw.Flush(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    tw.hdr = hdr;
    // Shallow copy of Header
    // Avoid usage of the legacy TypeRegA flag, and automatically promote
    // it to use TypeReg or TypeDir.
    if (tw.hdr.Typeflag == TypeRegA) {
        if (strings.HasSuffix(tw.hdr.Name, "/"u8)){
            tw.hdr.Typeflag = TypeDir;
        } else {
            tw.hdr.Typeflag = TypeReg;
        }
    }
    // Round ModTime and ignore AccessTime and ChangeTime unless
    // the format is explicitly chosen.
    // This ensures nominal usage of WriteHeader (without specifying the format)
    // does not always result in the PAX format being chosen, which
    // causes a 1KiB increase to every header.
    if (tw.hdr.Format == FormatUnknown) {
        tw.hdr.ModTime = tw.hdr.ModTime.Round(time.ΔSecond);
        tw.hdr.AccessTime = new time.Time(nil);
        tw.hdr.ChangeTime = new time.Time(nil);
    }
    var (allowedFormats, paxHdrs, err) = tw.hdr.allowedFormats();
    switch (ᐧ) {
    case {} when allowedFormats.has(FormatUSTAR): {
        tw.err = tw.writeUSTARHeader(Ꮡ(tw.hdr));
        return tw.err;
    }
    case {} when allowedFormats.has(FormatPAX): {
        tw.err = tw.writePAXHeader(Ꮡ(tw.hdr), paxHdrs);
        return tw.err;
    }
    case {} when allowedFormats.has(FormatGNU): {
        tw.err = tw.writeGNUHeader(Ꮡ(tw.hdr));
        return tw.err;
    }
    default: {
        return err;
    }}

}

// Non-fatal error
[GoRecv] public static error writeUSTARHeader(this ref Writer tw, ж<Header> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.val;

    // Check if we can use USTAR prefix/suffix splitting.
    @string namePrefix = default!;
    {
        var (prefix, suffix, ok) = splitUSTARPath(hdr.Name); if (ok) {
            (namePrefix, hdr.Name) = (prefix, suffix);
        }
    }
    // Pack the main header.
    formatter f = default!;
    var blk = tw.templateV7Plus(Ꮡhdr, f.formatString, f.formatOctal);
    f.formatString(blk.toUSTAR().prefix(), namePrefix);
    blk.setFormat(FormatUSTAR);
    if (f.err != default!) {
        return f.err;
    }
    // Should never happen since header is validated
    return tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag);
}

[GoRecv] public static error writePAXHeader(this ref Writer tw, ж<Header> Ꮡhdr, map<@string, @string> paxHdrs) {
    ref var hdr = ref Ꮡhdr.val;

    @string realName = hdr.Name;
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
        slice<@string> keys = default!;
        foreach (var (k, _) in paxHdrs) {
            keys = append(keys, k);
        }
        slices.Sort(keys);
        // Write each record to a buffer.
        strings.Builder buf = default!;
        foreach (var (_, k) in keys) {
            var (rec, err) = formatPAXRecord(k, paxHdrs[k]);
            if (err != default!) {
                return err;
            }
            buf.WriteString(rec);
        }
        // Write the extended header file.
        @string name = default!;
        byte flag = default!;
        if (isGlobal){
            name = realName;
            if (name == ""u8) {
                name = "GlobalHead.0.0"u8;
            }
            flag = TypeXGlobalHeader;
        } else {
            var (dir, file) = path.Split(realName);
            name = path.Join(dir, "PaxHeaders.0", file);
            flag = TypeXHeader;
        }
        @string data = buf.String();
        if (len(data) > maxSpecialFileSize) {
            return ErrFieldTooLong;
        }
        {
            var err = tw.writeRawFile(name, data, flag, FormatPAX); if (err != default! || isGlobal) {
                return err;
            }
        }
    }
    // Global headers return here
    // Pack the main header.
    ref var f = ref heap(new formatter(), out var Ꮡf);                   // Ignore errors since they are expected
    var fmtStr = 
    var fʗ1 = f;
    (slice<byte> b, @string s) => {
        fʗ1.formatString(b, toASCII(s));
    };
    var blk = tw.templateV7Plus(Ꮡhdr, fmtStr, f.formatOctal);
    blk.setFormat(FormatPAX);
    {
        var err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag); if (err != default!) {
            return err;
        }
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
    return default!;
}

[GoRecv] public static error writeGNUHeader(this ref Writer tw, ж<Header> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.val;

    // Use long-link files if Name or Linkname exceeds the field size.
    @string longName = "././@LongLink"u8;
    if (len(hdr.Name) > nameSize) {
        @string data = hdr.Name + "\x00"u8;
        {
            var err = tw.writeRawFile(longName, data, TypeGNULongName, FormatGNU); if (err != default!) {
                return err;
            }
        }
    }
    if (len(hdr.Linkname) > nameSize) {
        @string data = hdr.Linkname + "\x00"u8;
        {
            var err = tw.writeRawFile(longName, data, TypeGNULongLink, FormatGNU); if (err != default!) {
                return err;
            }
        }
    }
    // Pack the main header.
    formatter f = default!;                   // Ignore errors since they are expected
    sparseDatas spd = default!;
    slice<byte> spb = default!;
    var blk = tw.templateV7Plus(Ꮡhdr, f.formatString, f.formatNumeric);
    if (!hdr.AccessTime.IsZero()) {
        f.formatNumeric(blk.toGNU().accessTime(), hdr.AccessTime.Unix());
    }
    if (!hdr.ChangeTime.IsZero()) {
        f.formatNumeric(blk.toGNU().changeTime(), hdr.ChangeTime.Unix());
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
    blk.setFormat(FormatGNU);
    {
        var err = tw.writeRawHeader(blk, hdr.Size, hdr.Typeflag); if (err != default!) {
            return err;
        }
    }
    // Write the extended sparse map and setup the sparse writer if necessary.
    if (len(spd) > 0) {
        // Use tw.w since the sparse map is not accounted for in hdr.Size.
        {
            var (_, err) = tw.w.Write(spb); if (err != default!) {
                return err;
            }
        }
        tw.curr = Ꮡ(new sparseFileWriter(tw.curr, spd, 0));
    }
    return default!;
}

internal delegate void stringFormatter(slice<byte> _, @string _);

internal delegate void numberFormatter(slice<byte> _, int64 _);

// templateV7Plus fills out the V7 fields of a block using values from hdr.
// It also fills out fields (uname, gname, devmajor, devminor) that are
// shared in the USTAR, PAX, and GNU formats using the provided formatters.
//
// The block returned is only valid until the next call to
// templateV7Plus or writeRawFile.
[GoRecv] public static ж<block> templateV7Plus(this ref Writer tw, ж<Header> Ꮡhdr, stringFormatter fmtStr, numberFormatter fmtNum) {
    ref var hdr = ref Ꮡhdr.val;

    tw.blk.reset();
    var modTime = hdr.ModTime;
    if (modTime.IsZero()) {
        modTime = time.Unix(0, 0);
    }
    var v7 = tw.blk.toV7();
    v7.typeFlag()[0] = hdr.Typeflag;
    fmtStr(v7.name(), hdr.Name);
    fmtStr(v7.linkName(), hdr.Linkname);
    fmtNum(v7.mode(), hdr.Mode);
    fmtNum(v7.uid(), ((int64)hdr.Uid));
    fmtNum(v7.gid(), ((int64)hdr.Gid));
    fmtNum(v7.size(), hdr.Size);
    fmtNum(v7.modTime(), modTime.Unix());
    var ustar = tw.blk.toUSTAR();
    fmtStr(ustar.userName(), hdr.Uname);
    fmtStr(ustar.groupName(), hdr.Gname);
    fmtNum(ustar.devMajor(), hdr.Devmajor);
    fmtNum(ustar.devMinor(), hdr.Devminor);
    return Ꮡ(tw.blk);
}

// writeRawFile writes a minimal file with the given name and flag type.
// It uses format to encode the header format and will write data as the body.
// It uses default values for all of the other fields (as BSD and GNU tar does).
[GoRecv] internal static error writeRawFile(this ref Writer tw, @string name, @string data, byte flag, Format format) {
    tw.blk.reset();
    // Best effort for the filename.
    name = toASCII(name);
    if (len(name) > nameSize) {
        name = name[..(int)(nameSize)];
    }
    name = strings.TrimRight(name, "/"u8);
    formatter f = default!;
    var v7 = tw.blk.toV7();
    v7.typeFlag()[0] = flag;
    f.formatString(v7.name(), name);
    f.formatOctal(v7.mode(), 0);
    f.formatOctal(v7.uid(), 0);
    f.formatOctal(v7.gid(), 0);
    f.formatOctal(v7.size(), ((int64)len(data)));
    // Must be < 8GiB
    f.formatOctal(v7.modTime(), 0);
    tw.blk.setFormat(format);
    if (f.err != default!) {
        return f.err;
    }
    // Only occurs if size condition is violated
    // Write the header and data.
    {
        var errΔ1 = tw.writeRawHeader(Ꮡ(tw.blk), ((int64)len(data)), flag); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, err) = io.WriteString(~tw, data);
    return err;
}

// writeRawHeader writes the value of blk, regardless of its value.
// It sets up the Writer such that it can accept a file of the given size.
// If the flag is a special header-only flag, then the size is treated as zero.
[GoRecv] public static error writeRawHeader(this ref Writer tw, ж<block> Ꮡblk, int64 size, byte flag) {
    ref var blk = ref Ꮡblk.val;

    {
        var err = tw.Flush(); if (err != default!) {
            return err;
        }
    }
    {
        var (_, err) = tw.w.Write(blk[..]); if (err != default!) {
            return err;
        }
    }
    if (isHeaderOnlyType(flag)) {
        size = 0;
    }
    tw.curr = Ꮡ(new regFileWriter(tw.w, size));
    tw.pad = blockPadding(size);
    return default!;
}

// AddFS adds the files from fs.FS to the archive.
// It walks the directory tree starting at the root of the filesystem
// adding each file to the tar archive while maintaining the directory structure.
[GoRecv] public static error AddFS(this ref Writer tw, fs.FS fsys) => func((defer, _) => {
    return fs.WalkDir(fsys, "."u8, (@string name, fs.DirEntry d, error err) => {
        if (err != default!) {
            return err;
        }
        if (d.IsDir()) {
            return default!;
        }
        (info, err) = d.Info();
        if (err != default!) {
            return err;
        }
        // TODO(#49580): Handle symlinks when fs.ReadLinkFS is available.
        if (!info.Mode().IsRegular()) {
            return errors.New("tar: cannot add non-regular file"u8);
        }
        (h, err) = FileInfoHeader(info, ""u8);
        if (err != default!) {
            return err;
        }
        h.val.Name = name;
        {
            var errΔ1 = tw.WriteHeader(h); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        (f, err) = fsys.Open(name);
        if (err != default!) {
            return err;
        }
        var fʗ1 = f;
        defer(fʗ1.Close);
        (_, err) = io.Copy(~tw, f);
        return err;
    });
});

// splitUSTARPath splits a path according to USTAR prefix and suffix rules.
// If the path is not splittable, then it will return ("", "", false).
internal static (@string prefix, @string suffix, bool ok) splitUSTARPath(@string name) {
    @string prefix = default!;
    @string suffix = default!;
    bool ok = default!;

    nint length = len(name);
    if (length <= nameSize || !isASCII(name)){
        return ("", "", false);
    } else 
    if (length > prefixSize + 1){
        length = prefixSize + 1;
    } else 
    if (name[length - 1] == (rune)'/') {
        length--;
    }
    nint i = strings.LastIndex(name[..(int)(length)], "/"u8);
    nint nlen = len(name) - i - 1;
    // nlen is length of suffix
    nint plen = i;
    // plen is length of prefix
    if (i <= 0 || nlen > nameSize || nlen == 0 || plen > prefixSize) {
        return ("", "", false);
    }
    return (name[..(int)(i)], name[(int)(i + 1)..], true);
}

// Write writes to the current file in the tar archive.
// Write returns the error [ErrWriteTooLong] if more than
// Header.Size bytes are written after [Writer.WriteHeader].
//
// Calling Write on special types like [TypeLink], [TypeSymlink], [TypeChar],
// [TypeBlock], [TypeDir], and [TypeFifo] returns (0, [ErrWriteTooLong]) regardless
// of what the [Header.Size] claims.
[GoRecv] public static (nint, error) Write(this ref Writer tw, slice<byte> b) {
    if (tw.err != default!) {
        return (0, tw.err);
    }
    var (n, err) = tw.curr.Write(b);
    if (err != default! && !AreEqual(err, ErrWriteTooLong)) {
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
[GoRecv] internal static (int64, error) readFrom(this ref Writer tw, io.Reader r) {
    if (tw.err != default!) {
        return (0, tw.err);
    }
    var (n, err) = tw.curr.ReadFrom(r);
    if (err != default! && !AreEqual(err, ErrWriteTooLong)) {
        tw.err = err;
    }
    return (n, err);
}

// Close closes the tar archive by flushing the padding, and writing the footer.
// If the current file (from a prior call to [Writer.WriteHeader]) is not fully written,
// then this returns an error.
[GoRecv] public static error Close(this ref Writer tw) {
    if (AreEqual(tw.err, ErrWriteAfterClose)) {
        return default!;
    }
    if (tw.err != default!) {
        return tw.err;
    }
    // Trailer: two zero blocks.
    var err = tw.Flush();
    for (nint i = 0; i < 2 && err == default!; i++) {
        (_, err) = tw.w.Write(zeroBlock[..]);
    }
    // Ensure all future actions are invalid.
    tw.err = ErrWriteAfterClose;
    return err;
}

// Report IO errors

// regFileWriter is a fileWriter for writing data to a regular file entry.
[GoType] partial struct regFileWriter {
    internal io_package.Writer w; // Underlying Writer
    internal int64 nb;     // Number of remaining bytes to write
}

[GoRecv] internal static (nint n, error err) Write(this ref regFileWriter fw, slice<byte> b) {
    nint n = default!;
    error err = default!;

    var overwrite = ((int64)len(b)) > fw.nb;
    if (overwrite) {
        b = b[..(int)(fw.nb)];
    }
    if (len(b) > 0) {
        (n, err) = fw.w.Write(b);
        fw.nb -= ((int64)n);
    }
    switch (ᐧ) {
    case {} when err != default!: {
        return (n, err);
    }
    case {} when overwrite: {
        return (n, ErrWriteTooLong);
    }
    default: {
        return (n, default!);
    }}

}

[GoType("dyn")] partial struct ReadFrom_dst {
    public partial ref io_package.Writer Writer { get; }
}

[GoRecv] internal static (int64, error) ReadFrom(this ref regFileWriter fw, io.Reader r) {
    return io.Copy(new ReadFrom_dst(fw), r);
}

// logicalRemaining implements fileState.logicalRemaining.
internal static int64 logicalRemaining(this regFileWriter fw) {
    return fw.nb;
}

// physicalRemaining implements fileState.physicalRemaining.
internal static int64 physicalRemaining(this regFileWriter fw) {
    return fw.nb;
}

// sparseFileWriter is a fileWriter for writing data to a sparse file entry.
[GoType] partial struct sparseFileWriter {
    internal fileWriter fw;  // Underlying fileWriter
    internal sparseDatas sp; // Normalized list of data fragments
    internal int64 pos;       // Current position in sparse file
}

[GoRecv] internal static (nint n, error err) Write(this ref sparseFileWriter sw, slice<byte> b) {
    nint n = default!;
    error err = default!;

    var overwrite = ((int64)len(b)) > sw.logicalRemaining();
    if (overwrite) {
        b = b[..(int)(sw.logicalRemaining())];
    }
    var b0 = b;
    var endPos = sw.pos + ((int64)len(b));
    while (endPos > sw.pos && err == default!) {
        nint nf = default!;      // Bytes written in fragment
        var (dataStart, dataEnd) = (sw.sp[0].Offset, sw.sp[0].endOffset());
        if (sw.pos < dataStart){
            // In a hole fragment
            var bf = b[..(int)(min(((int64)len(b)), dataStart - sw.pos))];
            (nf, err) = new zeroWriter(nil).Write(bf);
        } else {
            // In a data fragment
            var bf = b[..(int)(min(((int64)len(b)), dataEnd - sw.pos))];
            (nf, err) = sw.fw.Write(bf);
        }
        b = b[(int)(nf)..];
        sw.pos += ((int64)nf);
        if (sw.pos >= dataEnd && len(sw.sp) > 1) {
            sw.sp = sw.sp[1..];
        }
    }
    // Ensure last fragment always remains
    n = len(b0) - len(b);
    switch (ᐧ) {
    case {} when err is ErrWriteTooLong: {
        return (n, errMissData);
    }
    case {} when err != default!: {
        return (n, err);
    }
    case {} when sw.logicalRemaining() == 0 && sw.physicalRemaining() > 0: {
        return (n, errUnrefData);
    }
    case {} when overwrite: {
        return (n, ErrWriteTooLong);
    }
    default: {
        return (n, default!);
    }}

}

[GoType("dyn")] partial struct ReadFrom_dstᴛ1 {
    public partial ref io_package.Writer Writer { get; }
}

// Not possible; implies bug in validation logic
// Not possible; implies bug in validation logic
[GoRecv] internal static (int64 n, error err) ReadFrom(this ref sparseFileWriter sw, io.Reader r) {
    int64 n = default!;
    error err = default!;

    var (rs, ok) = r._<io.ReadSeeker>(ᐧ);
    if (ok) {
        {
            var (_, errΔ1) = rs.Seek(0, io.SeekCurrent); if (errΔ1 != default!) {
                ok = false;
            }
        }
    }
    // Not all io.Seeker can really seek
    if (!ok) {
        return io.Copy(new ReadFrom_dstᴛ1(sw), r);
    }
    bool readLastByte = default!;
    var pos0 = sw.pos;
    while (sw.logicalRemaining() > 0 && !readLastByte && err == default!) {
        int64 nf = default!;          // Size of fragment
        var (dataStart, dataEnd) = (sw.sp[0].Offset, sw.sp[0].endOffset());
        if (sw.pos < dataStart){
            // In a hole fragment
            nf = dataStart - sw.pos;
            if (sw.physicalRemaining() == 0) {
                readLastByte = true;
                nf--;
            }
            (_, err) = rs.Seek(nf, io.SeekCurrent);
        } else {
            // In a data fragment
            nf = dataEnd - sw.pos;
            (nf, err) = io.CopyN(sw.fw, rs, nf);
        }
        sw.pos += nf;
        if (sw.pos >= dataEnd && len(sw.sp) > 1) {
            sw.sp = sw.sp[1..];
        }
    }
    // Ensure last fragment always remains
    // If the last fragment is a hole, then seek to 1-byte before EOF, and
    // read a single byte to ensure the file is the right size.
    if (readLastByte && err == default!) {
        (_, err) = mustReadFull(rs, new byte[]{0}.slice());
        sw.pos++;
    }
    n = sw.pos - pos0;
    switch (ᐧ) {
    case {} when err is io.EOF: {
        return (n, io.ErrUnexpectedEOF);
    }
    case {} when err is ErrWriteTooLong: {
        return (n, errMissData);
    }
    case {} when err != default!: {
        return (n, err);
    }
    case {} when sw.logicalRemaining() == 0 && sw.physicalRemaining() > 0: {
        return (n, errUnrefData);
    }
    default: {
        return (n, ensureEOF(rs));
    }}

}

// Not possible; implies bug in validation logic
// Not possible; implies bug in validation logic
internal static int64 logicalRemaining(this sparseFileWriter sw) {
    return sw.sp[len(sw.sp) - 1].endOffset() - sw.pos;
}

internal static int64 physicalRemaining(this sparseFileWriter sw) {
    return sw.fw.physicalRemaining();
}

// zeroWriter may only be written with NULs, otherwise it returns errWriteHole.
[GoType] partial struct zeroWriter {
}

internal static (nint, error) Write(this zeroWriter _, slice<byte> b) {
    foreach (var (i, c) in b) {
        if (c != 0) {
            return (i, errWriteHole);
        }
    }
    return (len(b), default!);
}

// ensureEOF checks whether r is at EOF, reporting ErrWriteTooLong if not so.
internal static error ensureEOF(io.Reader r) {
    var (n, err) = tryReadFull(r, new byte[]{0}.slice());
    switch (ᐧ) {
    case {} when n is > 0: {
        return ErrWriteTooLong;
    }
    case {} when err is io.EOF: {
        return default!;
    }
    default: {
        return err;
    }}

}

} // end tar_package
