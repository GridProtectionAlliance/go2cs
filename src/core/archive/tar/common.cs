// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tar implements access to tar archives.
//
// Tape archives (tar) are a file format for storing a sequence of files that
// can be read and written in a streaming manner.
// This package aims to cover most variations of the format,
// including those produced by GNU and BSD tar tools.
namespace go.archive;

using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using fs = io.fs_package;
using math = math_package;
using path = path_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using @internal;
using io;

partial class tar_package {

// BUG: Use of the Uid and Gid fields in Header could overflow on 32-bit
// architectures. If a large value is encountered when decoding, the result
// stored in Header will be the truncated version.
internal static ж<godebug.Setting> tarinsecurepath = godebug.New("tarinsecurepath"u8);

public static error ErrHeader = errors.New("archive/tar: invalid tar header"u8);
public static error ErrWriteTooLong = errors.New("archive/tar: write too long"u8);
public static error ErrFieldTooLong = errors.New("archive/tar: header field too long"u8);
public static error ErrWriteAfterClose = errors.New("archive/tar: write after close"u8);
public static error ErrInsecurePath = errors.New("archive/tar: insecure file path"u8);
internal static error errMissData = errors.New("archive/tar: sparse file references non-existent data"u8);
internal static error errUnrefData = errors.New("archive/tar: sparse file contains unreferenced data"u8);
internal static error errWriteHole = errors.New("archive/tar: write non-NUL byte in sparse hole"u8);

[GoType("[]@string")] partial struct headerError;

internal static @string Error(this headerError he) {
    @string prefix = "archive/tar: cannot encode header"u8;
    slice<@string> ss = default!;
    foreach (var (_, s) in he) {
        if (s != ""u8) {
            ss = append(ss, s);
        }
    }
    if (len(ss) == 0) {
        return prefix;
    }
    return fmt.Sprintf("%s: %v"u8, prefix, strings.Join(ss, "; and "u8));
}

// Type flags for Header.Typeflag.
public static readonly UntypedInt TypeReg = /* '0' */ 48;

public static readonly UntypedInt TypeRegA = /* '\x00' */ 0;

public static readonly UntypedInt TypeLink = /* '1' */ 49; // Hard link

public static readonly UntypedInt TypeSymlink = /* '2' */ 50; // Symbolic link

public static readonly UntypedInt TypeChar = /* '3' */ 51; // Character device node

public static readonly UntypedInt TypeBlock = /* '4' */ 52; // Block device node

public static readonly UntypedInt TypeDir = /* '5' */ 53; // Directory

public static readonly UntypedInt TypeFifo = /* '6' */ 54; // FIFO node

public static readonly UntypedInt TypeCont = /* '7' */ 55;

public static readonly UntypedInt TypeXHeader = /* 'x' */ 120;

public static readonly UntypedInt TypeXGlobalHeader = /* 'g' */ 103;

public static readonly UntypedInt TypeGNUSparse = /* 'S' */ 83;

public static readonly UntypedInt TypeGNULongName = /* 'L' */ 76;

public static readonly UntypedInt TypeGNULongLink = /* 'K' */ 75;

// Keywords for PAX extended header records.
internal static readonly @string paxNone = ""u8; // Indicates that no PAX key is suitable

internal static readonly @string paxPath = "path"u8;

internal static readonly @string paxLinkpath = "linkpath"u8;

internal static readonly @string paxSize = "size"u8;

internal static readonly @string paxUid = "uid"u8;

internal static readonly @string paxGid = "gid"u8;

internal static readonly @string paxUname = "uname"u8;

internal static readonly @string paxGname = "gname"u8;

internal static readonly @string paxMtime = "mtime"u8;

internal static readonly @string paxAtime = "atime"u8;

internal static readonly @string paxCtime = "ctime"u8; // Removed from later revision of PAX spec, but was valid

internal static readonly @string paxCharset = "charset"u8; // Currently unused

internal static readonly @string paxComment = "comment"u8; // Currently unused

internal static readonly @string paxSchilyXattr = "SCHILY.xattr."u8;

internal static readonly @string paxGNUSparse = "GNU.sparse."u8;

internal static readonly @string paxGNUSparseNumBlocks = "GNU.sparse.numblocks"u8;

internal static readonly @string paxGNUSparseOffset = "GNU.sparse.offset"u8;

internal static readonly @string paxGNUSparseNumBytes = "GNU.sparse.numbytes"u8;

internal static readonly @string paxGNUSparseMap = "GNU.sparse.map"u8;

internal static readonly @string paxGNUSparseName = "GNU.sparse.name"u8;

internal static readonly @string paxGNUSparseMajor = "GNU.sparse.major"u8;

internal static readonly @string paxGNUSparseMinor = "GNU.sparse.minor"u8;

internal static readonly @string paxGNUSparseSize = "GNU.sparse.size"u8;

internal static readonly @string paxGNUSparseRealSize = "GNU.sparse.realsize"u8;

// basicKeys is a set of the PAX keys for which we have built-in support.
// This does not contain "charset" or "comment", which are both PAX-specific,
// so adding them as first-class features of Header is unlikely.
// Users can use the PAXRecords field to set it themselves.
internal static map<@string, bool> basicKeys = new map<@string, bool>{
    [paxPath] = true, [paxLinkpath] = true, [paxSize] = true, [paxUid] = true, [paxGid] = true,
    [paxUname] = true, [paxGname] = true, [paxMtime] = true, [paxAtime] = true, [paxCtime] = true
};

// A Header represents a single header in a tar archive.
// Some fields may not be populated.
//
// For forward compatibility, users that retrieve a Header from Reader.Next,
// mutate it in some ways, and then pass it back to Writer.WriteHeader
// should do so by creating a new Header and copying the fields
// that they are interested in preserving.
[GoType] partial struct Header {
    // Typeflag is the type of header entry.
    // The zero value is automatically promoted to either TypeReg or TypeDir
    // depending on the presence of a trailing slash in Name.
    public byte Typeflag;
    public @string Name; // Name of file entry
    public @string Linkname; // Target name of link (valid for TypeLink or TypeSymlink)
    public int64 Size;  // Logical file size in bytes
    public int64 Mode;  // Permission and mode bits
    public nint Uid;   // User ID of owner
    public nint Gid;   // Group ID of owner
    public @string Uname; // User name of owner
    public @string Gname; // Group name of owner
    // If the Format is unspecified, then Writer.WriteHeader rounds ModTime
    // to the nearest second and ignores the AccessTime and ChangeTime fields.
    //
    // To use AccessTime or ChangeTime, specify the Format as PAX or GNU.
    // To use sub-second resolution, specify the Format as PAX.
    public time_package.Time ModTime; // Modification time
    public time_package.Time AccessTime; // Access time (requires either PAX or GNU support)
    public time_package.Time ChangeTime; // Change time (requires either PAX or GNU support)
    public int64 Devmajor; // Major device number (valid for TypeChar or TypeBlock)
    public int64 Devminor; // Minor device number (valid for TypeChar or TypeBlock)
    // Xattrs stores extended attributes as PAX records under the
    // "SCHILY.xattr." namespace.
    //
    // The following are semantically equivalent:
    //  h.Xattrs[key] = value
    //  h.PAXRecords["SCHILY.xattr."+key] = value
    //
    // When Writer.WriteHeader is called, the contents of Xattrs will take
    // precedence over those in PAXRecords.
    //
    // Deprecated: Use PAXRecords instead.
    public map<@string, @string> Xattrs;
    // PAXRecords is a map of PAX extended header records.
    //
    // User-defined records should have keys of the following form:
    //	VENDOR.keyword
    // Where VENDOR is some namespace in all uppercase, and keyword may
    // not contain the '=' character (e.g., "GOLANG.pkg.version").
    // The key and value should be non-empty UTF-8 strings.
    //
    // When Writer.WriteHeader is called, PAX records derived from the
    // other fields in Header take precedence over PAXRecords.
    public map<@string, @string> PAXRecords;
    // Format specifies the format of the tar header.
    //
    // This is set by Reader.Next as a best-effort guess at the format.
    // Since the Reader liberally reads some non-compliant files,
    // it is possible for this to be FormatUnknown.
    //
    // If the format is unspecified when Writer.WriteHeader is called,
    // then it uses the first format (in the order of USTAR, PAX, GNU)
    // capable of encoding this Header (see Format).
    public Format Format;
}

// sparseEntry represents a Length-sized fragment at Offset in the file.
[GoType] partial struct sparseEntry {
    public int64 Offset;
    public int64 Length;
}

internal static int64 endOffset(this sparseEntry s) {
    return s.Offset + s.Length;
}

[GoType("[]sparseEntry")] partial struct sparseDatas;

[GoType("[]sparseEntry")] partial struct sparseHoles;

// validateSparseEntries reports whether sp is a valid sparse map.
// It does not matter whether sp represents data fragments or hole fragments.
internal static bool validateSparseEntries(slice<sparseEntry> sp, int64 size) {
    // Validate all sparse entries. These are the same checks as performed by
    // the BSD tar utility.
    if (size < 0) {
        return false;
    }
    sparseEntry pre = default!;
    foreach (var (_, cur) in sp) {
        switch (ᐧ) {
        case {} when cur.Offset < 0 || cur.Length < 0: {
            return false;
        }
        case {} when cur.Offset is > math.MaxInt64 - cur.Length: {
            return false;
        }
        case {} when cur.endOffset() is > size: {
            return false;
        }
        case {} when pre.endOffset() is > cur.Offset: {
            return false;
        }}

        // Negative values are never okay
        // Integer overflow with large length
        // Region extends beyond the actual size
        // Regions cannot overlap and must be in order
        pre = cur;
    }
    return true;
}

// alignSparseEntries mutates src and returns dst where each fragment's
// starting offset is aligned up to the nearest block edge, and each
// ending offset is aligned down to the nearest block edge.
//
// Even though the Go tar Reader and the BSD tar utility can handle entries
// with arbitrary offsets and lengths, the GNU tar utility can only handle
// offsets and lengths that are multiples of blockSize.
internal static slice<sparseEntry> alignSparseEntries(slice<sparseEntry> src, int64 size) {
    var dst = src[..0];
    foreach (var (_, s) in src) {
        var (pos, end) = (s.Offset, s.endOffset());
        pos += blockPadding(+pos);
        // Round-up to nearest blockSize
        if (end != size) {
            end -= blockPadding(-end);
        }
        // Round-down to nearest blockSize
        if (pos < end) {
            dst = append(dst, new sparseEntry(Offset: pos, Length: end - pos));
        }
    }
    return dst;
}

// invertSparseEntries converts a sparse map from one form to the other.
// If the input is sparseHoles, then it will output sparseDatas and vice-versa.
// The input must have been already validated.
//
// This function mutates src and returns a normalized map where:
//   - adjacent fragments are coalesced together
//   - only the last fragment may be empty
//   - the endOffset of the last fragment is the total size
internal static slice<sparseEntry> invertSparseEntries(slice<sparseEntry> src, int64 size) {
    var dst = src[..0];
    sparseEntry pre = default!;
    foreach (var (_, cur) in src) {
        if (cur.Length == 0) {
            continue;
        }
        // Skip empty fragments
        pre.Length = cur.Offset - pre.Offset;
        if (pre.Length > 0) {
            dst = append(dst, pre);
        }
        // Only add non-empty fragments
        pre.Offset = cur.endOffset();
    }
    pre.Length = size - pre.Offset;
    // Possibly the only empty fragment
    return append(dst, pre);
}

// fileState tracks the number of logical (includes sparse holes) and physical
// (actual in tar archive) bytes remaining for the current file.
//
// Invariant: logicalRemaining >= physicalRemaining
[GoType] partial interface fileState {
    int64 logicalRemaining();
    int64 physicalRemaining();
}

// allowedFormats determines which formats can be used.
// The value returned is the logical OR of multiple possible formats.
// If the value is FormatUnknown, then the input Header cannot be encoded
// and an error is returned explaining why.
//
// As a by-product of checking the fields, this function returns paxHdrs, which
// contain all fields that could not be directly encoded.
// A value receiver ensures that this method does not mutate the source Header.
internal static (Format format, map<@string, @string> paxHdrs, error err) allowedFormats(this Header h) {
    Format format = default!;
    map<@string, @string> paxHdrs = default!;
    error err = default!;

    format = (Format)((Format)(FormatUSTAR | FormatPAX) | FormatGNU);
    paxHdrs = new map<@string, @string>();
    @string whyNoUSTAR = default!;
    @string whyNoPAX = default!;
    @string whyNoGNU = default!;
    bool preferPAX = default!; // Prefer PAX over USTAR
    var verifyString = 
    var hʗ1 = h;
    var paxHdrsʗ1 = paxHdrs;
    (@string s, nint size, @string name, @string paxKey) => {
        // NUL-terminator is optional for path and linkpath.
        // Technically, it is required for uname and gname,
        // but neither GNU nor BSD tar checks for it.
        var tooLong = len(s) > size;
        var allowLongGNU = paxKey == paxPath || paxKey == paxLinkpath;
        if (hasNUL(s) || (tooLong && !allowLongGNU)) {
            whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%q"u8, name, s);
            format.mustNotBe(FormatGNU);
        }
        if (!isASCII(s) || tooLong) {
            var canSplitUSTAR = paxKey == paxPath;
            {
                var (_, _, ok) = splitUSTARPath(s); if (!canSplitUSTAR || !ok) {
                    whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%q"u8, name, s);
                    format.mustNotBe(FormatUSTAR);
                }
            }
            if (paxKey == paxNone){
                whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%q"u8, name, s);
                format.mustNotBe(FormatPAX);
            } else {
                paxHdrsʗ1[paxKey] = s;
            }
        }
        {
            @string v = hʗ1.PAXRecords[paxKey];
            var ok = hʗ1.PAXRecords[paxKey]; if (ok && v == s) {
                paxHdrsʗ1[paxKey] = v;
            }
        }
    };
    var verifyNumeric = 
    var hʗ2 = h;
    var paxHdrsʗ2 = paxHdrs;
    (int64 n, nint size, @string name, @string paxKey) => {
        if (!fitsInBase256(size, n)) {
            whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%d"u8, name, n);
            format.mustNotBe(FormatGNU);
        }
        if (!fitsInOctal(size, n)) {
            whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%d"u8, name, n);
            format.mustNotBe(FormatUSTAR);
            if (paxKey == paxNone){
                whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%d"u8, name, n);
                format.mustNotBe(FormatPAX);
            } else {
                paxHdrsʗ2[paxKey] = strconv.FormatInt(n, 10);
            }
        }
        {
            @string v = hʗ2.PAXRecords[paxKey];
            var ok = hʗ2.PAXRecords[paxKey]; if (ok && v == strconv.FormatInt(n, 10)) {
                paxHdrsʗ2[paxKey] = v;
            }
        }
    };
    var verifyTime = 
    var hʗ3 = h;
    var paxHdrsʗ3 = paxHdrs;
    (time.Time ts, nint size, @string name, @string paxKey) => {
        if (ts.IsZero()) {
            return (format, paxHdrsʗ3, err);
        }
        // Always okay
        if (!fitsInBase256(size, ts.Unix())) {
            whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%v"u8, name, ts);
            format.mustNotBe(FormatGNU);
        }
        var isMtime = paxKey == paxMtime;
        var fitsOctal = fitsInOctal(size, ts.Unix());
        if ((isMtime && !fitsOctal) || !isMtime) {
            whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%v"u8, name, ts);
            format.mustNotBe(FormatUSTAR);
        }
        var needsNano = ts.Nanosecond() != 0;
        if (!isMtime || !fitsOctal || needsNano) {
            preferPAX = true;
            // USTAR may truncate sub-second measurements
            if (paxKey == paxNone){
                whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%v"u8, name, ts);
                format.mustNotBe(FormatPAX);
            } else {
                paxHdrsʗ3[paxKey] = formatPAXTime(ts);
            }
        }
        {
            @string v = hʗ3.PAXRecords[paxKey];
            var ok = hʗ3.PAXRecords[paxKey]; if (ok && v == formatPAXTime(ts)) {
                paxHdrsʗ3[paxKey] = v;
            }
        }
    };
    // Check basic fields.
    block blk = default!;
    var v7 = blk.toV7();
    var ustar = blk.toUSTAR();
    var gnu = blk.toGNU();
    verifyString(h.Name, len(v7.name()), "Name"u8, paxPath);
    verifyString(h.Linkname, len(v7.linkName()), "Linkname"u8, paxLinkpath);
    verifyString(h.Uname, len(ustar.userName()), "Uname"u8, paxUname);
    verifyString(h.Gname, len(ustar.groupName()), "Gname"u8, paxGname);
    verifyNumeric(h.Mode, len(v7.mode()), "Mode"u8, paxNone);
    verifyNumeric(((int64)h.Uid), len(v7.uid()), "Uid"u8, paxUid);
    verifyNumeric(((int64)h.Gid), len(v7.gid()), "Gid"u8, paxGid);
    verifyNumeric(h.Size, len(v7.size()), "Size"u8, paxSize);
    verifyNumeric(h.Devmajor, len(ustar.devMajor()), "Devmajor"u8, paxNone);
    verifyNumeric(h.Devminor, len(ustar.devMinor()), "Devminor"u8, paxNone);
    verifyTime(h.ModTime, len(v7.modTime()), "ModTime"u8, paxMtime);
    verifyTime(h.AccessTime, len(gnu.accessTime()), "AccessTime"u8, paxAtime);
    verifyTime(h.ChangeTime, len(gnu.changeTime()), "ChangeTime"u8, paxCtime);
    // Check for header-only types.
    @string whyOnlyPAX = default!;
    @string whyOnlyGNU = default!;
    switch (h.Typeflag) {
    case TypeReg or TypeChar or TypeBlock or TypeFifo or TypeGNUSparse: {
        if (strings.HasSuffix(h.Name, // Exclude TypeLink and TypeSymlink, since they may reference directories.
 "/"u8)) {
            return (FormatUnknown, default!, new headerError{"filename may not have trailing slash"});
        }
        break;
    }
    case TypeXHeader or TypeGNULongName or TypeGNULongLink: {
        return (FormatUnknown, default!, new headerError{"cannot manually encode TypeXHeader, TypeGNULongName, or TypeGNULongLink headers"});
    }
    case TypeXGlobalHeader: {
        var h2 = new Header(Name: h.Name, Typeflag: h.Typeflag, Xattrs: h.Xattrs, PAXRecords: h.PAXRecords, Format: h.Format);
        if (!reflect.DeepEqual(h, h2)) {
            return (FormatUnknown, default!, new headerError{"only PAXRecords should be set for TypeXGlobalHeader"});
        }
        whyOnlyPAX = "only PAX supports TypeXGlobalHeader"u8;
        format.mayOnlyBe(FormatPAX);
        break;
    }}

    if (!isHeaderOnlyType(h.Typeflag) && h.Size < 0) {
        return (FormatUnknown, default!, new headerError{"negative size on header-only type"});
    }
    // Check PAX records.
    if (len(h.Xattrs) > 0) {
        foreach (var (k, v) in h.Xattrs) {
            paxHdrs[paxSchilyXattr + k] = v;
        }
        whyOnlyPAX = "only PAX supports Xattrs"u8;
        format.mayOnlyBe(FormatPAX);
    }
    if (len(h.PAXRecords) > 0) {
        foreach (var (k, v) in h.PAXRecords) {
            {
                @string _ = paxHdrs[k];
                var exists = paxHdrs[k];
                switch (ᐧ) {
                case {} when exists: {
                    continue;
                    break;
                }
                case {} when h.Typeflag is TypeXGlobalHeader: {
                    paxHdrs[k] = v;
                    break;
                }
                case {} when !basicKeys[k] && !strings.HasPrefix(k, // Do not overwrite existing records
 // Copy all records
 paxGNUSparse): {
                    paxHdrs[k] = v;
                    break;
                }}
            }

        }
        // Ignore local records that may conflict
        whyOnlyPAX = "only PAX supports PAXRecords"u8;
        format.mayOnlyBe(FormatPAX);
    }
    foreach (var (k, v) in paxHdrs) {
        if (!validPAXRecord(k, v)) {
            return (FormatUnknown, default!, new headerError{fmt.Sprintf("invalid PAX record: %q"u8, k + " = "u8 + v)});
        }
    }
    // TODO(dsnet): Re-enable this when adding sparse support.
    // See https://golang.org/issue/22735
    /*
		// Check sparse files.
		if len(h.SparseHoles) > 0 || h.Typeflag == TypeGNUSparse {
			if isHeaderOnlyType(h.Typeflag) {
				return FormatUnknown, nil, headerError{"header-only type cannot be sparse"}
			}
			if !validateSparseEntries(h.SparseHoles, h.Size) {
				return FormatUnknown, nil, headerError{"invalid sparse holes"}
			}
			if h.Typeflag == TypeGNUSparse {
				whyOnlyGNU = "only GNU supports TypeGNUSparse"
				format.mayOnlyBe(FormatGNU)
			} else {
				whyNoGNU = "GNU supports sparse files only with TypeGNUSparse"
				format.mustNotBe(FormatGNU)
			}
			whyNoUSTAR = "USTAR does not support sparse files"
			format.mustNotBe(FormatUSTAR)
		}
	*/
    // Check desired format.
    {
        Format wantFormat = h.Format; if (wantFormat != FormatUnknown) {
            if (wantFormat.has(FormatPAX) && !preferPAX) {
                wantFormat.mayBe(FormatUSTAR);
            }
            // PAX implies USTAR allowed too
            format.mayOnlyBe(wantFormat);
        }
    }
    // Set union of formats allowed and format wanted
    if (format == FormatUnknown) {
        var exprᴛ1 = h.Format;
        if (exprᴛ1 == FormatUSTAR) {
            err = new headerError{"Format specifies USTAR", whyNoUSTAR, whyOnlyPAX, whyOnlyGNU};
        }
        else if (exprᴛ1 == FormatPAX) {
            err = new headerError{"Format specifies PAX", whyNoPAX, whyOnlyGNU};
        }
        else if (exprᴛ1 == FormatGNU) {
            err = new headerError{"Format specifies GNU", whyNoGNU, whyOnlyPAX};
        }
        else { /* default: */
            err = new headerError{whyNoUSTAR, whyNoPAX, whyNoGNU, whyOnlyPAX, whyOnlyGNU};
        }

    }
    return (format, paxHdrs, err);
}

// FileInfo returns an fs.FileInfo for the Header.
[GoRecv] public static fs.FileInfo FileInfo(this ref Header h) {
    return new headerFileInfo(h);
}

// headerFileInfo implements fs.FileInfo.
[GoType] partial struct headerFileInfo {
    internal ж<Header> h;
}

internal static int64 Size(this headerFileInfo fi) {
    return fi.h.Size;
}

internal static bool IsDir(this headerFileInfo fi) {
    return fi.Mode().IsDir();
}

internal static time.Time ModTime(this headerFileInfo fi) {
    return fi.h.ModTime;
}

internal static any Sys(this headerFileInfo fi) {
    return fi.h;
}

// Name returns the base name of the file.
internal static @string Name(this headerFileInfo fi) {
    if (fi.IsDir()) {
        return path.Base(path.Clean(fi.h.Name));
    }
    return path.Base(fi.h.Name);
}

// Mode returns the permission and mode bits for the headerFileInfo.
internal static fs.FileMode /*mode*/ Mode(this headerFileInfo fi) {
    fs.FileMode mode = default!;

    // Set file permission bits.
    mode = ((fs.FileMode)fi.h.Mode).Perm();
    // Set setuid, setgid and sticky bits.
    if ((int64)(fi.h.Mode & c_ISUID) != 0) {
        mode |= (fs.FileMode)(fs.ModeSetuid);
    }
    if ((int64)(fi.h.Mode & c_ISGID) != 0) {
        mode |= (fs.FileMode)(fs.ModeSetgid);
    }
    if ((int64)(fi.h.Mode & c_ISVTX) != 0) {
        mode |= (fs.FileMode)(fs.ModeSticky);
    }
    // Set file mode bits; clear perm, setuid, setgid, and sticky bits.
    {
        var m = (fs.FileMode)(((fs.FileMode)fi.h.Mode) & ~4095);
        var exprᴛ1 = m;
        if (exprᴛ1 == c_ISDIR) {
            mode |= (fs.FileMode)(fs.ModeDir);
        }
        else if (exprᴛ1 == c_ISFIFO) {
            mode |= (fs.FileMode)(fs.ModeNamedPipe);
        }
        else if (exprᴛ1 == c_ISLNK) {
            mode |= (fs.FileMode)(fs.ModeSymlink);
        }
        else if (exprᴛ1 == c_ISBLK) {
            mode |= (fs.FileMode)(fs.ModeDevice);
        }
        else if (exprᴛ1 == c_ISCHR) {
            mode |= (fs.FileMode)(fs.ModeDevice);
            mode |= (fs.FileMode)(fs.ModeCharDevice);
        }
        else if (exprᴛ1 == c_ISSOCK) {
            mode |= (fs.FileMode)(fs.ModeSocket);
        }
    }

    switch (fi.h.Typeflag) {
    case TypeSymlink: {
        mode |= (fs.FileMode)(fs.ModeSymlink);
        break;
    }
    case TypeChar: {
        mode |= (fs.FileMode)(fs.ModeDevice);
        mode |= (fs.FileMode)(fs.ModeCharDevice);
        break;
    }
    case TypeBlock: {
        mode |= (fs.FileMode)(fs.ModeDevice);
        break;
    }
    case TypeDir: {
        mode |= (fs.FileMode)(fs.ModeDir);
        break;
    }
    case TypeFifo: {
        mode |= (fs.FileMode)(fs.ModeNamedPipe);
        break;
    }}

    return mode;
}

internal static @string String(this headerFileInfo fi) {
    return fs.FormatFileInfo(fi);
}

// sysStat, if non-nil, populates h from system-dependent fields of fi.
internal static fs.FileInfo, h *Header, doNameLookups bool) error sysStat;

internal static readonly UntypedInt c_ISUID = /* 04000 */ 2048; // Set uid
internal static readonly UntypedInt c_ISGID = /* 02000 */ 1024; // Set gid
internal static readonly UntypedInt c_ISVTX = /* 01000 */ 512; // Save text (sticky bit)
internal static readonly UntypedInt c_ISDIR = /* 040000 */ 16384; // Directory
internal static readonly UntypedInt c_ISFIFO = /* 010000 */ 4096; // FIFO
internal static readonly UntypedInt c_ISREG = /* 0100000 */ 32768; // Regular file
internal static readonly UntypedInt c_ISLNK = /* 0120000 */ 40960; // Symbolic link
internal static readonly UntypedInt c_ISBLK = /* 060000 */ 24576; // Block special file
internal static readonly UntypedInt c_ISCHR = /* 020000 */ 8192; // Character special file
internal static readonly UntypedInt c_ISSOCK = /* 0140000 */ 49152; // Socket

// FileInfoHeader creates a partially-populated [Header] from fi.
// If fi describes a symlink, FileInfoHeader records link as the link target.
// If fi describes a directory, a slash is appended to the name.
//
// Since fs.FileInfo's Name method only returns the base name of
// the file it describes, it may be necessary to modify Header.Name
// to provide the full path name of the file.
//
// If fi implements [FileInfoNames]
// Header.Gname and Header.Uname
// are provided by the methods of the interface.
public static (ж<Header>, error) FileInfoHeader(fs.FileInfo fi, @string link) {
    if (fi == default!) {
        return (default!, errors.New("archive/tar: FileInfo is nil"u8));
    }
    var fm = fi.Mode();
    var h = Ꮡ(new Header(
        Name: fi.Name(),
        ModTime: fi.ModTime(),
        Mode: ((int64)fm.Perm())
    ));
    // or'd with c_IS* constants later
    switch (ᐧ) {
    case {} when fm.IsRegular(): {
        h.val.Typeflag = TypeReg;
        h.val.Size = fi.Size();
        break;
    }
    case {} when fi.IsDir(): {
        h.val.Typeflag = TypeDir;
        h.val.Name += "/"u8;
        break;
    }
    case {} when (fs.FileMode)(fm & fs.ModeSymlink) != 0: {
        h.val.Typeflag = TypeSymlink;
        h.val.Linkname = link;
        break;
    }
    case {} when (fs.FileMode)(fm & fs.ModeDevice) != 0: {
        if ((fs.FileMode)(fm & fs.ModeCharDevice) != 0){
            h.val.Typeflag = TypeChar;
        } else {
            h.val.Typeflag = TypeBlock;
        }
        break;
    }
    case {} when (fs.FileMode)(fm & fs.ModeNamedPipe) != 0: {
        h.val.Typeflag = TypeFifo;
        break;
    }
    case {} when (fs.FileMode)(fm & fs.ModeSocket) != 0: {
        return (default!, fmt.Errorf("archive/tar: sockets not supported"u8));
    }
    default: {
        return (default!, fmt.Errorf("archive/tar: unknown file mode %v"u8, fm));
    }}

    if ((fs.FileMode)(fm & fs.ModeSetuid) != 0) {
        h.val.Mode |= (int64)(c_ISUID);
    }
    if ((fs.FileMode)(fm & fs.ModeSetgid) != 0) {
        h.val.Mode |= (int64)(c_ISGID);
    }
    if ((fs.FileMode)(fm & fs.ModeSticky) != 0) {
        h.val.Mode |= (int64)(c_ISVTX);
    }
    // If possible, populate additional fields from OS-specific
    // FileInfo fields.
    {
        var (sys, ok) = fi.Sys()._<Header.val>(ᐧ); if (ok) {
            // This FileInfo came from a Header (not the OS). Use the
            // original Header to populate all remaining fields.
            h.val.Uid = sys.val.Uid;
            h.val.Gid = sys.val.Gid;
            h.val.Uname = sys.val.Uname;
            h.val.Gname = sys.val.Gname;
            h.val.AccessTime = sys.val.AccessTime;
            h.val.ChangeTime = sys.val.ChangeTime;
            if ((~sys).Xattrs != default!) {
                h.val.Xattrs = new map<@string, @string>();
                foreach (var (k, v) in (~sys).Xattrs) {
                    (~h).Xattrs[k] = v;
                }
            }
            if ((~sys).Typeflag == TypeLink) {
                // hard link
                h.val.Typeflag = TypeLink;
                h.val.Size = 0;
                h.val.Linkname = sys.val.Linkname;
            }
            if ((~sys).PAXRecords != default!) {
                h.val.PAXRecords = new map<@string, @string>();
                foreach (var (k, v) in (~sys).PAXRecords) {
                    (~h).PAXRecords[k] = v;
                }
            }
        }
    }
    bool doNameLookups = true;
    {
        var (iface, ok) = fi._<FileInfoNames>(ᐧ); if (ok) {
            doNameLookups = false;
            error err = default!;
            (h.val.Gname, err) = iface.Gname();
            if (err != default!) {
                return (default!, err);
            }
            (h.val.Uname, err) = iface.Uname();
            if (err != default!) {
                return (default!, err);
            }
        }
    }
    if (sysStat != default!) {
        return (h, sysStat(fi, h, doNameLookups));
    }
    return (h, default!);
}

// FileInfoNames extends [fs.FileInfo].
// Passing an instance of this to [FileInfoHeader] permits the caller
// to avoid a system-dependent name lookup by specifying the Uname and Gname directly.
[GoType] partial interface FileInfoNames :
    fs.FileInfo
{
    // Uname should give a user name.
    (@string, error) Uname();
    // Gname should give a group name.
    (@string, error) Gname();
}

// isHeaderOnlyType checks if the given type flag is of the type that has no
// data section even if a size is specified.
internal static bool isHeaderOnlyType(byte flag) {
    switch (flag) {
    case TypeLink or TypeSymlink or TypeChar or TypeBlock or TypeDir or TypeFifo: {
        return true;
    }
    default: {
        return false;
    }}

}

} // end tar_package
