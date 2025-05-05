// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using io = io_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using path;

partial class tar_package {

// Reader provides sequential access to the contents of a tar archive.
// Reader.Next advances to the next file in the archive (including the first),
// and then Reader can be treated as an io.Reader to access the file's data.
[GoType] partial struct Reader {
    internal io_package.Reader r;
    internal int64 pad;      // Amount of padding (ignored) after current file entry
    internal fileReader curr; // Reader for current file entry
    internal block blk;      // Buffer to use as temporary local storage
    // err is a persistent error.
    // It is only the responsibility of every exported method of Reader to
    // ensure that this error is sticky.
    internal error err;
}

[GoType] partial interface fileReader :
    io.Reader,
    fileState
{
    (int64, error) WriteTo(io.Writer _);
}

// NewReader creates a new [Reader] reading from r.
public static ж<Reader> NewReader(io.Reader r) {
    return Ꮡ(new Reader(r: r, curr: Ꮡ(new regFileReader(r, 0))));
}

// Next advances to the next entry in the tar archive.
// The Header.Size determines how many bytes can be read for the next file.
// Any remaining data in the current file is automatically discarded.
// At the end of the archive, Next returns the error io.EOF.
//
// If Next encounters a non-local name (as defined by [filepath.IsLocal])
// and the GODEBUG environment variable contains `tarinsecurepath=0`,
// Next returns the header with an [ErrInsecurePath] error.
// A future version of Go may introduce this behavior by default.
// Programs that want to accept non-local names can ignore
// the [ErrInsecurePath] error and use the returned header.
[GoRecv] public static (ж<Header>, error) Next(this ref Reader tr) {
    if (tr.err != default!) {
        return (default!, tr.err);
    }
    (hdr, err) = tr.next();
    tr.err = err;
    if (err == default! && !filepath.IsLocal((~hdr).Name)) {
        if (tarinsecurepath.Value() == "0"u8) {
            tarinsecurepath.IncNonDefault();
            err = ErrInsecurePath;
        }
    }
    return (hdr, err);
}

[GoRecv] internal static (ж<Header>, error) next(this ref Reader tr) {
    map<@string, @string> paxHdrs = default!;
    @string gnuLongName = default!;
    @string gnuLongLink = default!;
    // Externally, Next iterates through the tar archive as if it is a series of
    // files. Internally, the tar format often uses fake "files" to add meta
    // data that describes the next file. These meta data "files" should not
    // normally be visible to the outside. As such, this loop iterates through
    // one or more "header files" until it finds a "normal file".
    ref var format = ref heap<Format>(out var Ꮡformat);
    format = (Format)((Format)(FormatUSTAR | FormatPAX) | FormatGNU);
    while (ᐧ) {
        // Discard the remainder of the file and any padding.
        {
            var err = discard(tr.r, tr.curr.physicalRemaining()); if (err != default!) {
                return (default!, err);
            }
        }
        {
            var (_, err) = tryReadFull(tr.r, tr.blk[..(int)(tr.pad)]); if (err != default!) {
                return (default!, err);
            }
        }
        tr.pad = 0;
        (hdr, rawHdr, err) = tr.readHeader();
        if (err != default!) {
            return (default!, err);
        }
        {
            var errΔ1 = tr.handleRegularFile(hdr); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        format.mayOnlyBe((~hdr).Format);
        // Check for PAX/GNU special headers and files.
        switch ((~hdr).Typeflag) {
        case TypeXHeader or TypeXGlobalHeader: {
            format.mayOnlyBe(FormatPAX);
            (paxHdrs, err) = parsePAX(~tr);
            if (err != default!) {
                return (default!, err);
            }
            if ((~hdr).Typeflag == TypeXGlobalHeader) {
                mergePAX(hdr, paxHdrs);
                return (Ꮡ(new Header(
                    Name: (~hdr).Name,
                    Typeflag: (~hdr).Typeflag,
                    Xattrs: (~hdr).Xattrs,
                    PAXRecords: (~hdr).PAXRecords,
                    Format: format
                )), default!);
            }
            continue;
            break;
        }
        case TypeGNULongName or TypeGNULongLink: {
            format.mayOnlyBe(FormatGNU);
            (realname, errΔ6) = readSpecialFile(~tr);
            if (errΔ6 != default!) {
                // This is a meta header affecting the next header
                return (default!, errΔ6);
            }
            parser p = default!;
            switch ((~hdr).Typeflag) {
            case TypeGNULongName: {
                gnuLongName = p.parseString(realname);
                break;
            }
            case TypeGNULongLink: {
                gnuLongLink = p.parseString(realname);
                break;
            }}

            continue;
            break;
        }
        default: {
            {
                var errΔ7 = mergePAX(hdr, // This is a meta header affecting the next header
 // The old GNU sparse format is handled here since it is technically
 // just a regular file with additional attributes.
 paxHdrs); if (errΔ7 != default!) {
                    return (default!, errΔ7);
                }
            }
            if (gnuLongName != ""u8) {
                hdr.val.Name = gnuLongName;
            }
            if (gnuLongLink != ""u8) {
                hdr.val.Linkname = gnuLongLink;
            }
            if ((~hdr).Typeflag == TypeRegA) {
                if (strings.HasSuffix((~hdr).Name, "/"u8)){
                    hdr.val.Typeflag = TypeDir;
                } else {
                    // Legacy archives use trailing slash for directories
                    hdr.val.Typeflag = TypeReg;
                }
            }
            {
                var errΔ8 = tr.handleRegularFile(hdr); if (errΔ8 != default!) {
                    // The extended headers may have updated the size.
                    // Thus, setup the regFileReader again after merging PAX headers.
                    return (default!, errΔ8);
                }
            }
            {
                var errΔ9 = tr.handleSparseFile(hdr, // Sparse formats rely on being able to read from the logical data
 // section; there must be a preceding call to handleRegularFile.
 rawHdr); if (errΔ9 != default!) {
                    return (default!, errΔ9);
                }
            }
            if (format.has(FormatUSTAR) && format.has(FormatPAX)) {
                // Set the final guess at the format.
                format.mayOnlyBe(FormatUSTAR);
            }
            hdr.val.Format = format;
            return (hdr, default!);
        }}

    }
}

// This is a file, so stop

// handleRegularFile sets up the current file reader and padding such that it
// can only read the following logical data section. It will properly handle
// special headers that contain no data section.
[GoRecv] public static error handleRegularFile(this ref Reader tr, ж<Header> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.val;

    ref var nb = ref heap<int64>(out var Ꮡnb);
    nb = hdr.Size;
    if (isHeaderOnlyType(hdr.Typeflag)) {
        nb = 0;
    }
    if (nb < 0) {
        return ErrHeader;
    }
    tr.pad = blockPadding(nb);
    tr.curr = Ꮡ(new regFileReader(r: tr.r, nb: nb));
    return default!;
}

// handleSparseFile checks if the current file is a sparse format of any type
// and sets the curr reader appropriately.
[GoRecv] public static error handleSparseFile(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> ᏑrawHdr) {
    ref var hdr = ref Ꮡhdr.val;
    ref var rawHdr = ref ᏑrawHdr.val;

    sparseDatas spd = default!;
    error err = default!;
    if (hdr.Typeflag == TypeGNUSparse){
        (spd, err) = tr.readOldGNUSparseMap(Ꮡhdr, ᏑrawHdr);
    } else {
        (spd, err) = tr.readGNUSparsePAXHeaders(Ꮡhdr);
    }
    // If sp is non-nil, then this is a sparse file.
    // Note that it is possible for len(sp) == 0.
    if (err == default! && spd != default!) {
        if (isHeaderOnlyType(hdr.Typeflag) || !validateSparseEntries(spd, hdr.Size)) {
            return ErrHeader;
        }
        var sph = invertSparseEntries(spd, hdr.Size);
        tr.curr = Ꮡ(new sparseFileReader(tr.curr, sph, 0));
    }
    return err;
}

// readGNUSparsePAXHeaders checks the PAX headers for GNU sparse headers.
// If they are found, then this function reads the sparse map and returns it.
// This assumes that 0.0 headers have already been converted to 0.1 headers
// by the PAX header parsing logic.
[GoRecv] public static (sparseDatas, error) readGNUSparsePAXHeaders(this ref Reader tr, ж<Header> Ꮡhdr) {
    ref var hdr = ref Ꮡhdr.val;

    // Identify the version of GNU headers.
    bool is1x0 = default!;
    @string major = hdr.PAXRecords[paxGNUSparseMajor];
    @string minor = hdr.PAXRecords[paxGNUSparseMinor];
    switch (ᐧ) {
    case {} when major == "0"u8 && (minor == "0"u8 || minor == "1"u8): {
        is1x0 = false;
        break;
    }
    case {} when major == "1"u8 && minor == "0"u8: {
        is1x0 = true;
        break;
    }
    case {} when major != ""u8 || minor != ""u8: {
        return (default!, default!);
    }
    case {} when hdr.PAXRecords[paxGNUSparseMap] != "": {
        is1x0 = false;
        break;
    }
    default: {
        return (default!, default!);
    }}

    // Unknown GNU sparse PAX version
    // 0.0 and 0.1 did not have explicit version records, so guess
    // Not a PAX format GNU sparse file.
    hdr.Format.mayOnlyBe(FormatPAX);
    // Update hdr from GNU sparse PAX headers.
    {
        @string name = hdr.PAXRecords[paxGNUSparseName]; if (name != ""u8) {
            hdr.Name = name;
        }
    }
    @string size = hdr.PAXRecords[paxGNUSparseSize];
    if (size == ""u8) {
        size = hdr.PAXRecords[paxGNUSparseRealSize];
    }
    if (size != ""u8) {
        var (n, err) = strconv.ParseInt(size, 10, 64);
        if (err != default!) {
            return (default!, ErrHeader);
        }
        hdr.Size = n;
    }
    // Read the sparse map according to the appropriate format.
    if (is1x0) {
        return readGNUSparseMap1x0(tr.curr);
    }
    return readGNUSparseMap0x1(hdr.PAXRecords);
}

// mergePAX merges paxHdrs into hdr for all relevant fields of Header.
internal static error /*err*/ mergePAX(ж<Header> Ꮡhdr, map<@string, @string> paxHdrs) {
    error err = default!;

    ref var hdr = ref Ꮡhdr.val;
    foreach (var (k, v) in paxHdrs) {
        if (v == ""u8) {
            continue;
        }
        // Keep the original USTAR value
        int64 id64 = default!;
        var exprᴛ1 = k;
        if (exprᴛ1 == paxPath) {
            hdr.Name = v;
        }
        else if (exprᴛ1 == paxLinkpath) {
            hdr.Linkname = v;
        }
        else if (exprᴛ1 == paxUname) {
            hdr.Uname = v;
        }
        else if (exprᴛ1 == paxGname) {
            hdr.Gname = v;
        }
        else if (exprᴛ1 == paxUid) {
            (id64, err) = strconv.ParseInt(v, 10, 64);
            hdr.Uid = ((nint)id64);
        }
        else if (exprᴛ1 == paxGid) {
            (id64, err) = strconv.ParseInt(v, // Integer overflow possible
 10, 64);
            hdr.Gid = ((nint)id64);
        }
        else if (exprᴛ1 == paxAtime) {
            (hdr.AccessTime, err) = parsePAXTime(v);
        }
        else if (exprᴛ1 == paxMtime) {
            (hdr.ModTime, err) = parsePAXTime(v);
        }
        else if (exprᴛ1 == paxCtime) {
            (hdr.ChangeTime, err) = parsePAXTime(v);
        }
        else if (exprᴛ1 == paxSize) {
            (hdr.Size, err) = strconv.ParseInt(v, // Integer overflow possible
 10, 64);
        }
        else { /* default: */
            if (strings.HasPrefix(k, paxSchilyXattr)) {
                if (hdr.Xattrs == default!) {
                    hdr.Xattrs = new map<@string, @string>();
                }
                hdr.Xattrs[k[(int)(len(paxSchilyXattr))..]] = v;
            }
        }

        if (err != default!) {
            return ErrHeader;
        }
    }
    hdr.PAXRecords = paxHdrs;
    return default!;
}

// parsePAX parses PAX headers.
// If an extended header (type 'x') is invalid, ErrHeader is returned.
internal static (map<@string, @string>, error) parsePAX(io.Reader r) {
    (buf, err) = readSpecialFile(r);
    if (err != default!) {
        return (default!, err);
    }
    @string sbuf = ((@string)buf);
    // For GNU PAX sparse format 0.0 support.
    // This function transforms the sparse format 0.0 headers into format 0.1
    // headers since 0.0 headers were not PAX compliant.
    slice<@string> sparseMap = default!;
    var paxHdrs = new map<@string, @string>();
    while (len(sbuf) > 0) {
        var (key, value, residual, errΔ1) = parsePAXRecord(sbuf);
        if (errΔ1 != default!) {
            return (default!, ErrHeader);
        }
        sbuf = residual;
        var exprᴛ1 = key;
        if (exprᴛ1 == paxGNUSparseOffset || exprᴛ1 == paxGNUSparseNumBytes) {
            if ((len(sparseMap) % 2 == 0 && key != paxGNUSparseOffset) || (len(sparseMap) % 2 == 1 && key != paxGNUSparseNumBytes) || strings.Contains(value, // Validate sparse header order and value.
 ","u8)) {
                return (default!, ErrHeader);
            }
            sparseMap = append(sparseMap, value);
        }
        else { /* default: */
            paxHdrs[key] = value;
        }

    }
    if (len(sparseMap) > 0) {
        paxHdrs[paxGNUSparseMap] = strings.Join(sparseMap, ","u8);
    }
    return (paxHdrs, default!);
}

// readHeader reads the next block header and assumes that the underlying reader
// is already aligned to a block boundary. It returns the raw block of the
// header in case further processing is required.
//
// The err will be set to io.EOF only when one of the following occurs:
//   - Exactly 0 bytes are read and EOF is hit.
//   - Exactly 1 block of zeros is read and EOF is hit.
//   - At least 2 blocks of zeros are read.
[GoRecv] internal static (ж<Header>, ж<block>, error) readHeader(this ref Reader tr) {
    // Two blocks of zero bytes marks the end of the archive.
    {
        var (_, err) = io.ReadFull(tr.r, tr.blk[..]); if (err != default!) {
            return (default!, default!, err);
        }
    }
    // EOF is okay here; exactly 0 bytes read
    if (bytes.Equal(tr.blk[..], zeroBlock[..])) {
        {
            var (_, err) = io.ReadFull(tr.r, tr.blk[..]); if (err != default!) {
                return (default!, default!, err);
            }
        }
        // EOF is okay here; exactly 1 block of zeros read
        if (bytes.Equal(tr.blk[..], zeroBlock[..])) {
            return (default!, default!, io.EOF);
        }
        // normal EOF; exactly 2 block of zeros read
        return (default!, default!, ErrHeader);
    }
    // Zero block and then non-zero block
    // Verify the header matches a known format.
    Format format = tr.blk.getFormat();
    if (format == FormatUnknown) {
        return (default!, default!, ErrHeader);
    }
    parser p = default!;
    var hdr = @new<Header>();
    // Unpack the V7 header.
    var v7 = tr.blk.toV7();
    hdr.val.Typeflag = v7.typeFlag()[0];
    hdr.val.Name = p.parseString(v7.name());
    hdr.val.Linkname = p.parseString(v7.linkName());
    hdr.val.Size = p.parseNumeric(v7.size());
    hdr.val.Mode = p.parseNumeric(v7.mode());
    hdr.val.Uid = ((nint)p.parseNumeric(v7.uid()));
    hdr.val.Gid = ((nint)p.parseNumeric(v7.gid()));
    hdr.val.ModTime = time.Unix(p.parseNumeric(v7.modTime()), 0);
    // Unpack format specific fields.
    if (format > formatV7) {
        var ustar = tr.blk.toUSTAR();
        hdr.val.Uname = p.parseString(ustar.userName());
        hdr.val.Gname = p.parseString(ustar.groupName());
        hdr.val.Devmajor = p.parseNumeric(ustar.devMajor());
        hdr.val.Devminor = p.parseNumeric(ustar.devMinor());
        @string prefix = default!;
        switch (ᐧ) {
        case {} when format.has((Format)(FormatUSTAR | FormatPAX)): {
            hdr.val.Format = format;
            var ustarΔ3 = tr.blk.toUSTAR();
            prefix = p.parseString(ustarΔ3.prefix());
            var notASCII = (rune r) => r >= 128;
            if (bytes.IndexFunc(tr.blk[..], notASCII) >= 0) {
                hdr.val.Format = FormatUnknown;
            }
            var nul = (slice<byte> b) => ((nint)b[len(b) - 1]) == 0;
            if (!(nul(v7.size()) && nul(v7.mode()) && nul(v7.uid()) && nul(v7.gid()) && nul(v7.modTime()) && nul(ustarΔ3.devMajor()) && nul(ustarΔ3.devMinor()))) {
                hdr.val.Format = FormatUnknown;
            }
            break;
        }
        case {} when format.has(formatSTAR): {
            var star = tr.blk.toSTAR();
            prefix = p.parseString(star.prefix());
            hdr.val.AccessTime = time.Unix(p.parseNumeric(star.accessTime()), // Numeric fields must end in NUL
 0);
            hdr.val.ChangeTime = time.Unix(p.parseNumeric(star.changeTime()), 0);
            break;
        }
        case {} when format.has(FormatGNU): {
            hdr.val.Format = format;
            parser p2 = default!;
            var gnu = tr.blk.toGNU();
            {
                var b = gnu.accessTime(); if (b[0] != 0) {
                    hdr.val.AccessTime = time.Unix(p2.parseNumeric(b), 0);
                }
            }
            {
                var b = gnu.changeTime(); if (b[0] != 0) {
                    hdr.val.ChangeTime = time.Unix(p2.parseNumeric(b), 0);
                }
            }
            if (p2.err != default!) {
                // Prior to Go1.8, the Writer had a bug where it would output
                // an invalid tar file in certain rare situations because the logic
                // incorrectly believed that the old GNU format had a prefix field.
                // This is wrong and leads to an output file that mangles the
                // atime and ctime fields, which are often left unused.
                //
                // In order to continue reading tar files created by former, buggy
                // versions of Go, we skeptically parse the atime and ctime fields.
                // If we are unable to parse them and the prefix field looks like
                // an ASCII string, then we fallback on the pre-Go1.8 behavior
                // of treating these fields as the USTAR prefix field.
                //
                // Note that this will not use the fallback logic for all possible
                // files generated by a pre-Go1.8 toolchain. If the generated file
                // happened to have a prefix field that parses as valid
                // atime and ctime fields (e.g., when they are valid octal strings),
                // then it is impossible to distinguish between a valid GNU file
                // and an invalid pre-Go1.8 file.
                //
                // See https://golang.org/issues/12594
                // See https://golang.org/issues/21005
                (hdr.val.AccessTime, hdr.val.ChangeTime) = (new time.Time(nil), new time.Time(nil));
                var ustarΔ4 = tr.blk.toUSTAR();
                {
                    @string s = p.parseString(ustarΔ4.prefix()); if (isASCII(s)) {
                        prefix = s;
                    }
                }
                hdr.val.Format = FormatUnknown;
            }
            break;
        }}

        // Buggy file is not GNU
        if (len(prefix) > 0) {
            hdr.val.Name = prefix + "/"u8 + (~hdr).Name;
        }
    }
    return (hdr, Ꮡ(tr.blk), p.err);
}

// readOldGNUSparseMap reads the sparse map from the old GNU sparse format.
// The sparse map is stored in the tar header if it's small enough.
// If it's larger than four entries, then one or more extension headers are used
// to store the rest of the sparse map.
//
// The Header.Size does not reflect the size of any extended headers used.
// Thus, this function will read from the raw io.Reader to fetch extra headers.
// This method mutates blk in the process.
[GoRecv] public static (sparseDatas, error) readOldGNUSparseMap(this ref Reader tr, ж<Header> Ꮡhdr, ж<block> Ꮡblk) {
    ref var hdr = ref Ꮡhdr.val;
    ref var blk = ref Ꮡblk.val;

    // Make sure that the input format is GNU.
    // Unfortunately, the STAR format also has a sparse header format that uses
    // the same type flag but has a completely different layout.
    if (blk.getFormat() != FormatGNU) {
        return (default!, ErrHeader);
    }
    hdr.Format.mayOnlyBe(FormatGNU);
    parser p = default!;
    hdr.Size = p.parseNumeric(blk.toGNU().realSize());
    if (p.err != default!) {
        return (default!, p.err);
    }
    var s = blk.toGNU().sparse();
    var spd = new sparseDatas(0, s.maxEntries());
    while (ᐧ) {
        for (nint i = 0; i < s.maxEntries(); i++) {
            // This termination condition is identical to GNU and BSD tar.
            if (s.entry(i).offset()[0] == 0) {
                break;
            }
            // Don't return, need to process extended headers (even if empty)
            var offset = p.parseNumeric(s.entry(i).offset());
            var length = p.parseNumeric(s.entry(i).length());
            if (p.err != default!) {
                return (default!, p.err);
            }
            spd = append(spd, new sparseEntry(Offset: offset, Length: length));
        }
        if (s.isExtended()[0] > 0) {
            // There are more entries. Read an extension header and parse its entries.
            {
                var (_, err) = mustReadFull(tr.r, blk[..]); if (err != default!) {
                    return (default!, err);
                }
            }
            s = blk.toSparse();
            continue;
        }
        return (spd, default!);
    }
}

// Done

// readGNUSparseMap1x0 reads the sparse map as stored in GNU's PAX sparse format
// version 1.0. The format of the sparse map consists of a series of
// newline-terminated numeric fields. The first field is the number of entries
// and is always present. Following this are the entries, consisting of two
// fields (offset, length). This function must stop reading at the end
// boundary of the block containing the last newline.
//
// Note that the GNU manual says that numeric values should be encoded in octal
// format. However, the GNU tar utility itself outputs these values in decimal.
// As such, this library treats values as being encoded in decimal.
internal static (sparseDatas, error) readGNUSparseMap1x0(io.Reader r) {
    int64 cntNewline = default!;
    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    ref var blk = ref heap(new block(), out var Ꮡblk);
    // feedTokens copies data in blocks from r into buf until there are
    // at least cnt newlines in buf. It will not read more blocks than needed.
    var feedTokens = 
    var blkʗ1 = blk;
    var bufʗ1 = buf;
    (int64 n) => {
        while (cntNewline < n) {
            {
                var (_, errΔ1) = mustReadFull(r, blkʗ1[..]); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            bufʗ1.Write(blkʗ1[..]);
            foreach (var (_, c) in blkʗ1) {
                if (c == (rune)'\n') {
                    cntNewline++;
                }
            }
        }
        return default!;
    };
    // nextToken gets the next token delimited by a newline. This assumes that
    // at least one newline exists in the buffer.
    var nextToken = 
    var bufʗ2 = buf;
    () => {
        cntNewline--;
        var (tok, _) = bufʗ2.ReadString((rune)'\n');
        return strings.TrimRight(tok, "\n"u8);
    };
    // Parse for the number of entries.
    // Use integer overflow resistant math to check this.
    {
        var errΔ2 = feedTokens(1); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    var (numEntries, err) = strconv.ParseInt(nextToken(), 10, 0);
    // Intentionally parse as native int
    if (err != default! || numEntries < 0 || ((nint)(2 * numEntries)) < ((nint)numEntries)) {
        return (default!, ErrHeader);
    }
    // Parse for all member entries.
    // numEntries is trusted after this since a potential attacker must have
    // committed resources proportional to what this library used.
    {
        var errΔ3 = feedTokens(2 * numEntries); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    var spd = new sparseDatas(0, numEntries);
    for (var i = ((int64)0); i < numEntries; i++) {
        var (offset, err1) = strconv.ParseInt(nextToken(), 10, 64);
        var (length, err2) = strconv.ParseInt(nextToken(), 10, 64);
        if (err1 != default! || err2 != default!) {
            return (default!, ErrHeader);
        }
        spd = append(spd, new sparseEntry(Offset: offset, Length: length));
    }
    return (spd, default!);
}

// readGNUSparseMap0x1 reads the sparse map as stored in GNU's PAX sparse format
// version 0.1. The sparse map is stored in the PAX headers.
internal static (sparseDatas, error) readGNUSparseMap0x1(map<@string, @string> paxHdrs) {
    // Get number of entries.
    // Use integer overflow resistant math to check this.
    @string numEntriesStr = paxHdrs[paxGNUSparseNumBlocks];
    var (numEntries, err) = strconv.ParseInt(numEntriesStr, 10, 0);
    // Intentionally parse as native int
    if (err != default! || numEntries < 0 || ((nint)(2 * numEntries)) < ((nint)numEntries)) {
        return (default!, ErrHeader);
    }
    // There should be two numbers in sparseMap for each entry.
    var sparseMap = strings.Split(paxHdrs[paxGNUSparseMap], ","u8);
    if (len(sparseMap) == 1 && sparseMap[0] == "") {
        sparseMap = sparseMap[..0];
    }
    if (((int64)len(sparseMap)) != 2 * numEntries) {
        return (default!, ErrHeader);
    }
    // Loop through the entries in the sparse map.
    // numEntries is trusted now.
    var spd = new sparseDatas(0, numEntries);
    while (len(sparseMap) >= 2) {
        var (offset, err1) = strconv.ParseInt(sparseMap[0], 10, 64);
        var (length, err2) = strconv.ParseInt(sparseMap[1], 10, 64);
        if (err1 != default! || err2 != default!) {
            return (default!, ErrHeader);
        }
        spd = append(spd, new sparseEntry(Offset: offset, Length: length));
        sparseMap = sparseMap[2..];
    }
    return (spd, default!);
}

// Read reads from the current file in the tar archive.
// It returns (0, io.EOF) when it reaches the end of that file,
// until [Next] is called to advance to the next file.
//
// If the current file is sparse, then the regions marked as a hole
// are read back as NUL-bytes.
//
// Calling Read on special types like [TypeLink], [TypeSymlink], [TypeChar],
// [TypeBlock], [TypeDir], and [TypeFifo] returns (0, [io.EOF]) regardless of what
// the [Header.Size] claims.
[GoRecv] public static (nint, error) Read(this ref Reader tr, slice<byte> b) {
    if (tr.err != default!) {
        return (0, tr.err);
    }
    var (n, err) = tr.curr.Read(b);
    if (err != default! && !AreEqual(err, io.EOF)) {
        tr.err = err;
    }
    return (n, err);
}

// writeTo writes the content of the current file to w.
// The bytes written matches the number of remaining bytes in the current file.
//
// If the current file is sparse and w is an io.WriteSeeker,
// then writeTo uses Seek to skip past holes defined in Header.SparseHoles,
// assuming that skipped regions are filled with NULs.
// This always writes the last byte to ensure w is the right size.
//
// TODO(dsnet): Re-export this when adding sparse file support.
// See https://golang.org/issue/22735
[GoRecv] internal static (int64, error) writeTo(this ref Reader tr, io.Writer w) {
    if (tr.err != default!) {
        return (0, tr.err);
    }
    var (n, err) = tr.curr.WriteTo(w);
    if (err != default!) {
        tr.err = err;
    }
    return (n, err);
}

// regFileReader is a fileReader for reading data from a regular file entry.
[GoType] partial struct regFileReader {
    internal io_package.Reader r; // Underlying Reader
    internal int64 nb;     // Number of remaining bytes to read
}

[GoRecv] internal static (nint n, error err) Read(this ref regFileReader fr, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (((int64)len(b)) > fr.nb) {
        b = b[..(int)(fr.nb)];
    }
    if (len(b) > 0) {
        (n, err) = fr.r.Read(b);
        fr.nb -= ((int64)n);
    }
    switch (ᐧ) {
    case {} when AreEqual(err, io.EOF) && fr.nb > 0: {
        return (n, io.ErrUnexpectedEOF);
    }
    case {} when err == default! && fr.nb == 0: {
        return (n, io.EOF);
    }
    default: {
        return (n, err);
    }}

}

[GoType("dyn")] partial struct WriteTo_src {
    public partial ref io_package.Reader Reader { get; }
}

[GoRecv] internal static (int64, error) WriteTo(this ref regFileReader fr, io.Writer w) {
    return io.Copy(w, new WriteTo_src(fr));
}

// logicalRemaining implements fileState.logicalRemaining.
internal static int64 logicalRemaining(this regFileReader fr) {
    return fr.nb;
}

// physicalRemaining implements fileState.physicalRemaining.
internal static int64 physicalRemaining(this regFileReader fr) {
    return fr.nb;
}

// sparseFileReader is a fileReader for reading data from a sparse file entry.
[GoType] partial struct sparseFileReader {
    internal fileReader fr;  // Underlying fileReader
    internal sparseHoles sp; // Normalized list of sparse holes
    internal int64 pos;       // Current position in sparse file
}

[GoRecv] internal static (nint n, error err) Read(this ref sparseFileReader sr, slice<byte> b) {
    nint n = default!;
    error err = default!;

    var finished = ((int64)len(b)) >= sr.logicalRemaining();
    if (finished) {
        b = b[..(int)(sr.logicalRemaining())];
    }
    var b0 = b;
    var endPos = sr.pos + ((int64)len(b));
    while (endPos > sr.pos && err == default!) {
        nint nf = default!;      // Bytes read in fragment
        var (holeStart, holeEnd) = (sr.sp[0].Offset, sr.sp[0].endOffset());
        if (sr.pos < holeStart){
            // In a data fragment
            var bf = b[..(int)(min(((int64)len(b)), holeStart - sr.pos))];
            (nf, err) = tryReadFull(sr.fr, bf);
        } else {
            // In a hole fragment
            var bf = b[..(int)(min(((int64)len(b)), holeEnd - sr.pos))];
            (nf, err) = tryReadFull(new zeroReader(nil), bf);
        }
        b = b[(int)(nf)..];
        sr.pos += ((int64)nf);
        if (sr.pos >= holeEnd && len(sr.sp) > 1) {
            sr.sp = sr.sp[1..];
        }
    }
    // Ensure last fragment always remains
    n = len(b0) - len(b);
    switch (ᐧ) {
    case {} when err is io.EOF: {
        return (n, errMissData);
    }
    case {} when err is != default!: {
        return (n, err);
    }
    case {} when sr.logicalRemaining() == 0 && sr.physicalRemaining() > 0: {
        return (n, errUnrefData);
    }
    case {} when finished: {
        return (n, io.EOF);
    }
    default: {
        return (n, default!);
    }}

}

[GoType("dyn")] partial struct WriteTo_srcᴛ1 {
    public partial ref io_package.Reader Reader { get; }
}

// Less data in dense file than sparse file
// More data in dense file than sparse file
[GoRecv] internal static (int64 n, error err) WriteTo(this ref sparseFileReader sr, io.Writer w) {
    int64 n = default!;
    error err = default!;

    var (ws, ok) = w._<io.WriteSeeker>(ᐧ);
    if (ok) {
        {
            var (_, errΔ1) = ws.Seek(0, io.SeekCurrent); if (errΔ1 != default!) {
                ok = false;
            }
        }
    }
    // Not all io.Seeker can really seek
    if (!ok) {
        return io.Copy(w, new WriteTo_srcᴛ1(sr));
    }
    bool writeLastByte = default!;
    var pos0 = sr.pos;
    while (sr.logicalRemaining() > 0 && !writeLastByte && err == default!) {
        int64 nf = default!;          // Size of fragment
        var (holeStart, holeEnd) = (sr.sp[0].Offset, sr.sp[0].endOffset());
        if (sr.pos < holeStart){
            // In a data fragment
            nf = holeStart - sr.pos;
            (nf, err) = io.CopyN(ws, sr.fr, nf);
        } else {
            // In a hole fragment
            nf = holeEnd - sr.pos;
            if (sr.physicalRemaining() == 0) {
                writeLastByte = true;
                nf--;
            }
            (_, err) = ws.Seek(nf, io.SeekCurrent);
        }
        sr.pos += nf;
        if (sr.pos >= holeEnd && len(sr.sp) > 1) {
            sr.sp = sr.sp[1..];
        }
    }
    // Ensure last fragment always remains
    // If the last fragment is a hole, then seek to 1-byte before EOF, and
    // write a single byte to ensure the file is the right size.
    if (writeLastByte && err == default!) {
        (_, err) = ws.Write(new byte[]{0}.slice());
        sr.pos++;
    }
    n = sr.pos - pos0;
    switch (ᐧ) {
    case {} when err is io.EOF: {
        return (n, errMissData);
    }
    case {} when err is != default!: {
        return (n, err);
    }
    case {} when sr.logicalRemaining() == 0 && sr.physicalRemaining() > 0: {
        return (n, errUnrefData);
    }
    default: {
        return (n, default!);
    }}

}

// Less data in dense file than sparse file
// More data in dense file than sparse file
internal static int64 logicalRemaining(this sparseFileReader sr) {
    return sr.sp[len(sr.sp) - 1].endOffset() - sr.pos;
}

internal static int64 physicalRemaining(this sparseFileReader sr) {
    return sr.fr.physicalRemaining();
}

[GoType] partial struct zeroReader {
}

internal static (nint, error) Read(this zeroReader _, slice<byte> b) {
    clear(b);
    return (len(b), default!);
}

// mustReadFull is like io.ReadFull except it returns
// io.ErrUnexpectedEOF when io.EOF is hit before len(b) bytes are read.
internal static (nint, error) mustReadFull(io.Reader r, slice<byte> b) {
    var (n, err) = tryReadFull(r, b);
    if (AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return (n, err);
}

// tryReadFull is like io.ReadFull except it returns
// io.EOF when it is hit before len(b) bytes are read.
internal static (nint n, error err) tryReadFull(io.Reader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    while (len(b) > n && err == default!) {
        nint nn = default!;
        (nn, err) = r.Read(b[(int)(n)..]);
        n += nn;
    }
    if (len(b) == n && AreEqual(err, io.EOF)) {
        err = default!;
    }
    return (n, err);
}

// readSpecialFile is like io.ReadAll except it returns
// ErrFieldTooLong if more than maxSpecialFileSize is read.
internal static (slice<byte>, error) readSpecialFile(io.Reader r) {
    (buf, err) = io.ReadAll(io.LimitReader(r, maxSpecialFileSize + 1));
    if (len(buf) > maxSpecialFileSize) {
        return (default!, ErrFieldTooLong);
    }
    return (buf, err);
}

// discard skips n bytes in r, reporting an error if unable to do so.
internal static error discard(io.Reader r, int64 n) {
    // If possible, Seek to the last byte before the end of the data section.
    // Do this because Seek is often lazy about reporting errors; this will mask
    // the fact that the stream may be truncated. We can rely on the
    // io.CopyN done shortly afterwards to trigger any IO errors.
    int64 seekSkipped = default!; // Number of bytes skipped via Seek
    {
        var (sr, ok) = r._<io.Seeker>(ᐧ); if (ok && n > 1) {
            // Not all io.Seeker can actually Seek. For example, os.Stdin implements
            // io.Seeker, but calling Seek always returns an error and performs
            // no action. Thus, we try an innocent seek to the current position
            // to see if Seek is really supported.
            var (pos1, errΔ1) = sr.Seek(0, io.SeekCurrent);
            if (pos1 >= 0 && errΔ1 == default!) {
                // Seek seems supported, so perform the real Seek.
                var (pos2, errΔ2) = sr.Seek(n - 1, io.SeekCurrent);
                if (pos2 < 0 || errΔ2 != default!) {
                    return errΔ2;
                }
                seekSkipped = pos2 - pos1;
            }
        }
    }
    var (copySkipped, err) = io.CopyN(io.Discard, r, n - seekSkipped);
    if (AreEqual(err, io.EOF) && seekSkipped + copySkipped < n) {
        err = io.ErrUnexpectedEOF;
    }
    return err;
}

} // end tar_package
