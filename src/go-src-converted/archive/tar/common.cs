// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tar implements access to tar archives.
//
// Tape archives (tar) are a file format for storing a sequence of files that
// can be read and written in a streaming manner.
// This package aims to cover most variations of the format,
// including those produced by GNU and BSD tar tools.
// package tar -- go2cs converted at 2020 October 09 04:45:15 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\common.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using math = go.math_package;
using os = go.os_package;
using path = go.path_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        // BUG: Use of the Uid and Gid fields in Header could overflow on 32-bit
        // architectures. If a large value is encountered when decoding, the result
        // stored in Header will be the truncated version.
        public static var ErrHeader = errors.New("archive/tar: invalid tar header");        public static var ErrWriteTooLong = errors.New("archive/tar: write too long");        public static var ErrFieldTooLong = errors.New("archive/tar: header field too long");        public static var ErrWriteAfterClose = errors.New("archive/tar: write after close");        private static var errMissData = errors.New("archive/tar: sparse file references non-existent data");        private static var errUnrefData = errors.New("archive/tar: sparse file contains unreferenced data");        private static var errWriteHole = errors.New("archive/tar: write non-NUL byte in sparse hole");

        private partial struct headerError // : slice<@string>
        {
        }

        private static @string Error(this headerError he)
        {
            const @string prefix = (@string)"archive/tar: cannot encode header";

            slice<@string> ss = default;
            foreach (var (_, s) in he)
            {
                if (s != "")
                {
                    ss = append(ss, s);
                }

            }
            if (len(ss) == 0L)
            {
                return prefix;
            }

            return fmt.Sprintf("%s: %v", prefix, strings.Join(ss, "; and "));

        }

        // Type flags for Header.Typeflag.
 
        // Type '0' indicates a regular file.
        public static readonly char TypeReg = (char)'0';
        public static readonly char TypeRegA = (char)'\x00'; // Deprecated: Use TypeReg instead.

        // Type '1' to '6' are header-only flags and may not have a data body.
        public static readonly char TypeLink = (char)'1'; // Hard link
        public static readonly char TypeSymlink = (char)'2'; // Symbolic link
        public static readonly char TypeChar = (char)'3'; // Character device node
        public static readonly char TypeBlock = (char)'4'; // Block device node
        public static readonly char TypeDir = (char)'5'; // Directory
        public static readonly char TypeFifo = (char)'6'; // FIFO node

        // Type '7' is reserved.
        public static readonly char TypeCont = (char)'7'; 

        // Type 'x' is used by the PAX format to store key-value records that
        // are only relevant to the next file.
        // This package transparently handles these types.
        public static readonly char TypeXHeader = (char)'x'; 

        // Type 'g' is used by the PAX format to store key-value records that
        // are relevant to all subsequent files.
        // This package only supports parsing and composing such headers,
        // but does not currently support persisting the global state across files.
        public static readonly char TypeXGlobalHeader = (char)'g'; 

        // Type 'S' indicates a sparse file in the GNU format.
        public static readonly char TypeGNUSparse = (char)'S'; 

        // Types 'L' and 'K' are used by the GNU format for a meta file
        // used to store the path or link name for the next file.
        // This package transparently handles these types.
        public static readonly char TypeGNULongName = (char)'L';
        public static readonly char TypeGNULongLink = (char)'K';


        // Keywords for PAX extended header records.
        private static readonly @string paxNone = (@string)""; // Indicates that no PAX key is suitable
        private static readonly @string paxPath = (@string)"path";
        private static readonly @string paxLinkpath = (@string)"linkpath";
        private static readonly @string paxSize = (@string)"size";
        private static readonly @string paxUid = (@string)"uid";
        private static readonly @string paxGid = (@string)"gid";
        private static readonly @string paxUname = (@string)"uname";
        private static readonly @string paxGname = (@string)"gname";
        private static readonly @string paxMtime = (@string)"mtime";
        private static readonly @string paxAtime = (@string)"atime";
        private static readonly @string paxCtime = (@string)"ctime"; // Removed from later revision of PAX spec, but was valid
        private static readonly @string paxCharset = (@string)"charset"; // Currently unused
        private static readonly @string paxComment = (@string)"comment"; // Currently unused

        private static readonly @string paxSchilyXattr = (@string)"SCHILY.xattr."; 

        // Keywords for GNU sparse files in a PAX extended header.
        private static readonly @string paxGNUSparse = (@string)"GNU.sparse.";
        private static readonly @string paxGNUSparseNumBlocks = (@string)"GNU.sparse.numblocks";
        private static readonly @string paxGNUSparseOffset = (@string)"GNU.sparse.offset";
        private static readonly @string paxGNUSparseNumBytes = (@string)"GNU.sparse.numbytes";
        private static readonly @string paxGNUSparseMap = (@string)"GNU.sparse.map";
        private static readonly @string paxGNUSparseName = (@string)"GNU.sparse.name";
        private static readonly @string paxGNUSparseMajor = (@string)"GNU.sparse.major";
        private static readonly @string paxGNUSparseMinor = (@string)"GNU.sparse.minor";
        private static readonly @string paxGNUSparseSize = (@string)"GNU.sparse.size";
        private static readonly @string paxGNUSparseRealSize = (@string)"GNU.sparse.realsize";


        // basicKeys is a set of the PAX keys for which we have built-in support.
        // This does not contain "charset" or "comment", which are both PAX-specific,
        // so adding them as first-class features of Header is unlikely.
        // Users can use the PAXRecords field to set it themselves.
        private static map basicKeys = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{paxPath:true,paxLinkpath:true,paxSize:true,paxUid:true,paxGid:true,paxUname:true,paxGname:true,paxMtime:true,paxAtime:true,paxCtime:true,};

        // A Header represents a single header in a tar archive.
        // Some fields may not be populated.
        //
        // For forward compatibility, users that retrieve a Header from Reader.Next,
        // mutate it in some ways, and then pass it back to Writer.WriteHeader
        // should do so by creating a new Header and copying the fields
        // that they are interested in preserving.
        public partial struct Header
        {
            public byte Typeflag;
            public @string Name; // Name of file entry
            public @string Linkname; // Target name of link (valid for TypeLink or TypeSymlink)

            public long Size; // Logical file size in bytes
            public long Mode; // Permission and mode bits
            public long Uid; // User ID of owner
            public long Gid; // Group ID of owner
            public @string Uname; // User name of owner
            public @string Gname; // Group name of owner

// If the Format is unspecified, then Writer.WriteHeader rounds ModTime
// to the nearest second and ignores the AccessTime and ChangeTime fields.
//
// To use AccessTime or ChangeTime, specify the Format as PAX or GNU.
// To use sub-second resolution, specify the Format as PAX.
            public time.Time ModTime; // Modification time
            public time.Time AccessTime; // Access time (requires either PAX or GNU support)
            public time.Time ChangeTime; // Change time (requires either PAX or GNU support)

            public long Devmajor; // Major device number (valid for TypeChar or TypeBlock)
            public long Devminor; // Minor device number (valid for TypeChar or TypeBlock)

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
            public map<@string, @string> Xattrs; // PAXRecords is a map of PAX extended header records.
//
// User-defined records should have keys of the following form:
//    VENDOR.keyword
// Where VENDOR is some namespace in all uppercase, and keyword may
// not contain the '=' character (e.g., "GOLANG.pkg.version").
// The key and value should be non-empty UTF-8 strings.
//
// When Writer.WriteHeader is called, PAX records derived from the
// other fields in Header take precedence over PAXRecords.
            public map<@string, @string> PAXRecords; // Format specifies the format of the tar header.
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
        private partial struct sparseEntry
        {
            public long Offset;
            public long Length;
        }

        private static long endOffset(this sparseEntry s)
        {
            return s.Offset + s.Length;
        }

        // A sparse file can be represented as either a sparseDatas or a sparseHoles.
        // As long as the total size is known, they are equivalent and one can be
        // converted to the other form and back. The various tar formats with sparse
        // file support represent sparse files in the sparseDatas form. That is, they
        // specify the fragments in the file that has data, and treat everything else as
        // having zero bytes. As such, the encoding and decoding logic in this package
        // deals with sparseDatas.
        //
        // However, the external API uses sparseHoles instead of sparseDatas because the
        // zero value of sparseHoles logically represents a normal file (i.e., there are
        // no holes in it). On the other hand, the zero value of sparseDatas implies
        // that the file has no data in it, which is rather odd.
        //
        // As an example, if the underlying raw file contains the 10-byte data:
        //    var compactFile = "abcdefgh"
        //
        // And the sparse map has the following entries:
        //    var spd sparseDatas = []sparseEntry{
        //        {Offset: 2,  Length: 5},  // Data fragment for 2..6
        //        {Offset: 18, Length: 3},  // Data fragment for 18..20
        //    }
        //    var sph sparseHoles = []sparseEntry{
        //        {Offset: 0,  Length: 2},  // Hole fragment for 0..1
        //        {Offset: 7,  Length: 11}, // Hole fragment for 7..17
        //        {Offset: 21, Length: 4},  // Hole fragment for 21..24
        //    }
        //
        // Then the content of the resulting sparse file with a Header.Size of 25 is:
        //    var sparseFile = "\x00"*2 + "abcde" + "\x00"*11 + "fgh" + "\x00"*4
        private partial struct sparseDatas // : slice<sparseEntry>
        {
        }
        private partial struct sparseHoles // : slice<sparseEntry>
        {
        }
        private static bool validateSparseEntries(slice<sparseEntry> sp, long size)
        { 
            // Validate all sparse entries. These are the same checks as performed by
            // the BSD tar utility.
            if (size < 0L)
            {
                return false;
            }

            sparseEntry pre = default;
            foreach (var (_, cur) in sp)
            {

                if (cur.Offset < 0L || cur.Length < 0L) 
                    return false; // Negative values are never okay
                else if (cur.Offset > math.MaxInt64 - cur.Length) 
                    return false; // Integer overflow with large length
                else if (cur.endOffset() > size) 
                    return false; // Region extends beyond the actual size
                else if (pre.endOffset() > cur.Offset) 
                    return false; // Regions cannot overlap and must be in order
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
        private static slice<sparseEntry> alignSparseEntries(slice<sparseEntry> src, long size)
        {
            var dst = src[..0L];
            foreach (var (_, s) in src)
            {
                var pos = s.Offset;
                var end = s.endOffset();
                pos += blockPadding(+pos); // Round-up to nearest blockSize
                if (end != size)
                {
                    end -= blockPadding(-end); // Round-down to nearest blockSize
                }

                if (pos < end)
                {
                    dst = append(dst, new sparseEntry(Offset:pos,Length:end-pos));
                }

            }
            return dst;

        }

        // invertSparseEntries converts a sparse map from one form to the other.
        // If the input is sparseHoles, then it will output sparseDatas and vice-versa.
        // The input must have been already validated.
        //
        // This function mutates src and returns a normalized map where:
        //    * adjacent fragments are coalesced together
        //    * only the last fragment may be empty
        //    * the endOffset of the last fragment is the total size
        private static slice<sparseEntry> invertSparseEntries(slice<sparseEntry> src, long size)
        {
            var dst = src[..0L];
            sparseEntry pre = default;
            foreach (var (_, cur) in src)
            {
                if (cur.Length == 0L)
                {
                    continue; // Skip empty fragments
                }

                pre.Length = cur.Offset - pre.Offset;
                if (pre.Length > 0L)
                {
                    dst = append(dst, pre); // Only add non-empty fragments
                }

                pre.Offset = cur.endOffset();

            }
            pre.Length = size - pre.Offset; // Possibly the only empty fragment
            return append(dst, pre);

        }

        // fileState tracks the number of logical (includes sparse holes) and physical
        // (actual in tar archive) bytes remaining for the current file.
        //
        // Invariant: LogicalRemaining >= PhysicalRemaining
        private partial interface fileState
        {
            long LogicalRemaining();
            long PhysicalRemaining();
        }

        // allowedFormats determines which formats can be used.
        // The value returned is the logical OR of multiple possible formats.
        // If the value is FormatUnknown, then the input Header cannot be encoded
        // and an error is returned explaining why.
        //
        // As a by-product of checking the fields, this function returns paxHdrs, which
        // contain all fields that could not be directly encoded.
        // A value receiver ensures that this method does not mutate the source Header.
        public static (Format, map<@string, @string>, error) allowedFormats(this Header h)
        {
            Format format = default;
            map<@string, @string> paxHdrs = default;
            error err = default!;

            format = FormatUSTAR | FormatPAX | FormatGNU;
            paxHdrs = make_map<@string, @string>();

            @string whyNoUSTAR = default;            @string whyNoPAX = default;            @string whyNoGNU = default;

            bool preferPAX = default; // Prefer PAX over USTAR
            Action<@string, long, @string, @string> verifyString = (s, size, name, paxKey) =>
            { 
                // NUL-terminator is optional for path and linkpath.
                // Technically, it is required for uname and gname,
                // but neither GNU nor BSD tar checks for it.
                var tooLong = len(s) > size;
                var allowLongGNU = paxKey == paxPath || paxKey == paxLinkpath;
                if (hasNUL(s) || (tooLong && !allowLongGNU))
                {
                    whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%q", name, s);
                    format.mustNotBe(FormatGNU);
                }

                if (!isASCII(s) || tooLong)
                {
                    var canSplitUSTAR = paxKey == paxPath;
                    {
                        var (_, _, ok) = splitUSTARPath(s);

                        if (!canSplitUSTAR || !ok)
                        {
                            whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%q", name, s);
                            format.mustNotBe(FormatUSTAR);
                        }

                    }

                    if (paxKey == paxNone)
                    {
                        whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%q", name, s);
                        format.mustNotBe(FormatPAX);
                    }
                    else
                    {
                        paxHdrs[paxKey] = s;
                    }

                }

                {
                    var v__prev1 = v;

                    var (v, ok) = h.PAXRecords[paxKey];

                    if (ok && v == s)
                    {
                        paxHdrs[paxKey] = v;
                    }

                    v = v__prev1;

                }

            }
;
            Action<long, long, @string, @string> verifyNumeric = (n, size, name, paxKey) =>
            {
                if (!fitsInBase256(size, n))
                {
                    whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%d", name, n);
                    format.mustNotBe(FormatGNU);
                }

                if (!fitsInOctal(size, n))
                {
                    whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%d", name, n);
                    format.mustNotBe(FormatUSTAR);
                    if (paxKey == paxNone)
                    {
                        whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%d", name, n);
                        format.mustNotBe(FormatPAX);
                    }
                    else
                    {
                        paxHdrs[paxKey] = strconv.FormatInt(n, 10L);
                    }

                }

                {
                    var v__prev1 = v;

                    (v, ok) = h.PAXRecords[paxKey];

                    if (ok && v == strconv.FormatInt(n, 10L))
                    {
                        paxHdrs[paxKey] = v;
                    }

                    v = v__prev1;

                }

            }
;
            Action<time.Time, long, @string, @string> verifyTime = (ts, size, name, paxKey) =>
            {
                if (ts.IsZero())
                {
                    return ; // Always okay
                }

                if (!fitsInBase256(size, ts.Unix()))
                {
                    whyNoGNU = fmt.Sprintf("GNU cannot encode %s=%v", name, ts);
                    format.mustNotBe(FormatGNU);
                }

                var isMtime = paxKey == paxMtime;
                var fitsOctal = fitsInOctal(size, ts.Unix());
                if ((isMtime && !fitsOctal) || !isMtime)
                {
                    whyNoUSTAR = fmt.Sprintf("USTAR cannot encode %s=%v", name, ts);
                    format.mustNotBe(FormatUSTAR);
                }

                var needsNano = ts.Nanosecond() != 0L;
                if (!isMtime || !fitsOctal || needsNano)
                {
                    preferPAX = true; // USTAR may truncate sub-second measurements
                    if (paxKey == paxNone)
                    {
                        whyNoPAX = fmt.Sprintf("PAX cannot encode %s=%v", name, ts);
                        format.mustNotBe(FormatPAX);
                    }
                    else
                    {
                        paxHdrs[paxKey] = formatPAXTime(ts);
                    }

                }

                {
                    var v__prev1 = v;

                    (v, ok) = h.PAXRecords[paxKey];

                    if (ok && v == formatPAXTime(ts))
                    {
                        paxHdrs[paxKey] = v;
                    }

                    v = v__prev1;

                }

            } 

            // Check basic fields.
; 

            // Check basic fields.
            block blk = default;
            var v7 = blk.V7();
            var ustar = blk.USTAR();
            var gnu = blk.GNU();
            verifyString(h.Name, len(v7.Name()), "Name", paxPath);
            verifyString(h.Linkname, len(v7.LinkName()), "Linkname", paxLinkpath);
            verifyString(h.Uname, len(ustar.UserName()), "Uname", paxUname);
            verifyString(h.Gname, len(ustar.GroupName()), "Gname", paxGname);
            verifyNumeric(h.Mode, len(v7.Mode()), "Mode", paxNone);
            verifyNumeric(int64(h.Uid), len(v7.UID()), "Uid", paxUid);
            verifyNumeric(int64(h.Gid), len(v7.GID()), "Gid", paxGid);
            verifyNumeric(h.Size, len(v7.Size()), "Size", paxSize);
            verifyNumeric(h.Devmajor, len(ustar.DevMajor()), "Devmajor", paxNone);
            verifyNumeric(h.Devminor, len(ustar.DevMinor()), "Devminor", paxNone);
            verifyTime(h.ModTime, len(v7.ModTime()), "ModTime", paxMtime);
            verifyTime(h.AccessTime, len(gnu.AccessTime()), "AccessTime", paxAtime);
            verifyTime(h.ChangeTime, len(gnu.ChangeTime()), "ChangeTime", paxCtime); 

            // Check for header-only types.
            @string whyOnlyPAX = default;            @string whyOnlyGNU = default;


            if (h.Typeflag == TypeReg || h.Typeflag == TypeChar || h.Typeflag == TypeBlock || h.Typeflag == TypeFifo || h.Typeflag == TypeGNUSparse) 
                // Exclude TypeLink and TypeSymlink, since they may reference directories.
                if (strings.HasSuffix(h.Name, "/"))
                {
                    return (FormatUnknown, null, error.As(new headerError("filename may not have trailing slash"))!);
                }

            else if (h.Typeflag == TypeXHeader || h.Typeflag == TypeGNULongName || h.Typeflag == TypeGNULongLink) 
                return (FormatUnknown, null, error.As(new headerError("cannot manually encode TypeXHeader, TypeGNULongName, or TypeGNULongLink headers"))!);
            else if (h.Typeflag == TypeXGlobalHeader) 
                Header h2 = new Header(Name:h.Name,Typeflag:h.Typeflag,Xattrs:h.Xattrs,PAXRecords:h.PAXRecords,Format:h.Format);
                if (!reflect.DeepEqual(h, h2))
                {
                    return (FormatUnknown, null, error.As(new headerError("only PAXRecords should be set for TypeXGlobalHeader"))!);
                }

                whyOnlyPAX = "only PAX supports TypeXGlobalHeader";
                format.mayOnlyBe(FormatPAX);
                        if (!isHeaderOnlyType(h.Typeflag) && h.Size < 0L)
            {
                return (FormatUnknown, null, error.As(new headerError("negative size on header-only type"))!);
            } 

            // Check PAX records.
            if (len(h.Xattrs) > 0L)
            {
                {
                    var k__prev1 = k;
                    var v__prev1 = v;

                    foreach (var (__k, __v) in h.Xattrs)
                    {
                        k = __k;
                        v = __v;
                        paxHdrs[paxSchilyXattr + k] = v;
                    }

                    k = k__prev1;
                    v = v__prev1;
                }

                whyOnlyPAX = "only PAX supports Xattrs";
                format.mayOnlyBe(FormatPAX);

            }

            if (len(h.PAXRecords) > 0L)
            {
                {
                    var k__prev1 = k;
                    var v__prev1 = v;

                    foreach (var (__k, __v) in h.PAXRecords)
                    {
                        k = __k;
                        v = __v;
                        {
                            var (_, exists) = paxHdrs[k];


                            if (exists) 
                                continue; // Do not overwrite existing records
                            else if (h.Typeflag == TypeXGlobalHeader) 
                                paxHdrs[k] = v; // Copy all records
                            else if (!basicKeys[k] && !strings.HasPrefix(k, paxGNUSparse)) 
                                paxHdrs[k] = v; // Ignore local records that may conflict

                        }

                    }

                    k = k__prev1;
                    v = v__prev1;
                }

                whyOnlyPAX = "only PAX supports PAXRecords";
                format.mayOnlyBe(FormatPAX);

            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in paxHdrs)
                {
                    k = __k;
                    v = __v;
                    if (!validPAXRecord(k, v))
                    {
                        return (FormatUnknown, null, error.As(new headerError(fmt.Sprintf("invalid PAX record: %q",k+" = "+v)))!);
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

                k = k__prev1;
                v = v__prev1;
            }

            {
                var wantFormat = h.Format;

                if (wantFormat != FormatUnknown)
                {
                    if (wantFormat.has(FormatPAX) && !preferPAX)
                    {
                        wantFormat.mayBe(FormatUSTAR); // PAX implies USTAR allowed too
                    }

                    format.mayOnlyBe(wantFormat); // Set union of formats allowed and format wanted
                }

            }

            if (format == FormatUnknown)
            {

                if (h.Format == FormatUSTAR) 
                    err = new headerError("Format specifies USTAR",whyNoUSTAR,whyOnlyPAX,whyOnlyGNU);
                else if (h.Format == FormatPAX) 
                    err = new headerError("Format specifies PAX",whyNoPAX,whyOnlyGNU);
                else if (h.Format == FormatGNU) 
                    err = new headerError("Format specifies GNU",whyNoGNU,whyOnlyPAX);
                else 
                    err = new headerError(whyNoUSTAR,whyNoPAX,whyNoGNU,whyOnlyPAX,whyOnlyGNU);
                
            }

            return (format, paxHdrs, error.As(err)!);

        }

        // FileInfo returns an os.FileInfo for the Header.
        private static os.FileInfo FileInfo(this ptr<Header> _addr_h)
        {
            ref Header h = ref _addr_h.val;

            return new headerFileInfo(h);
        }

        // headerFileInfo implements os.FileInfo.
        private partial struct headerFileInfo
        {
            public ptr<Header> h;
        }

        private static long Size(this headerFileInfo fi)
        {
            return fi.h.Size;
        }
        private static bool IsDir(this headerFileInfo fi)
        {
            return fi.Mode().IsDir();
        }
        private static time.Time ModTime(this headerFileInfo fi)
        {
            return fi.h.ModTime;
        }
        private static void Sys(this headerFileInfo fi)
        {
            return fi.h;
        }

        // Name returns the base name of the file.
        private static @string Name(this headerFileInfo fi)
        {
            if (fi.IsDir())
            {
                return path.Base(path.Clean(fi.h.Name));
            }

            return path.Base(fi.h.Name);

        }

        // Mode returns the permission and mode bits for the headerFileInfo.
        private static os.FileMode Mode(this headerFileInfo fi)
        {
            os.FileMode mode = default;
 
            // Set file permission bits.
            mode = os.FileMode(fi.h.Mode).Perm(); 

            // Set setuid, setgid and sticky bits.
            if (fi.h.Mode & c_ISUID != 0L)
            {
                mode |= os.ModeSetuid;
            }

            if (fi.h.Mode & c_ISGID != 0L)
            {
                mode |= os.ModeSetgid;
            }

            if (fi.h.Mode & c_ISVTX != 0L)
            {
                mode |= os.ModeSticky;
            } 

            // Set file mode bits; clear perm, setuid, setgid, and sticky bits.
            {
                var m = os.FileMode(fi.h.Mode) & ~07777L;


                if (m == c_ISDIR) 
                    mode |= os.ModeDir;
                else if (m == c_ISFIFO) 
                    mode |= os.ModeNamedPipe;
                else if (m == c_ISLNK) 
                    mode |= os.ModeSymlink;
                else if (m == c_ISBLK) 
                    mode |= os.ModeDevice;
                else if (m == c_ISCHR) 
                    mode |= os.ModeDevice;
                    mode |= os.ModeCharDevice;
                else if (m == c_ISSOCK) 
                    mode |= os.ModeSocket;

            }


            if (fi.h.Typeflag == TypeSymlink) 
                mode |= os.ModeSymlink;
            else if (fi.h.Typeflag == TypeChar) 
                mode |= os.ModeDevice;
                mode |= os.ModeCharDevice;
            else if (fi.h.Typeflag == TypeBlock) 
                mode |= os.ModeDevice;
            else if (fi.h.Typeflag == TypeDir) 
                mode |= os.ModeDir;
            else if (fi.h.Typeflag == TypeFifo) 
                mode |= os.ModeNamedPipe;
                        return mode;

        }

        // sysStat, if non-nil, populates h from system-dependent fields of fi.
        private static Func<os.FileInfo, ptr<Header>, error> sysStat = default;

 
        // Mode constants from the USTAR spec:
        // See http://pubs.opengroup.org/onlinepubs/9699919799/utilities/pax.html#tag_20_92_13_06
        private static readonly long c_ISUID = (long)04000L; // Set uid
        private static readonly long c_ISGID = (long)02000L; // Set gid
        private static readonly long c_ISVTX = (long)01000L; // Save text (sticky bit)

        // Common Unix mode constants; these are not defined in any common tar standard.
        // Header.FileInfo understands these, but FileInfoHeader will never produce these.
        private static readonly long c_ISDIR = (long)040000L; // Directory
        private static readonly long c_ISFIFO = (long)010000L; // FIFO
        private static readonly long c_ISREG = (long)0100000L; // Regular file
        private static readonly long c_ISLNK = (long)0120000L; // Symbolic link
        private static readonly long c_ISBLK = (long)060000L; // Block special file
        private static readonly long c_ISCHR = (long)020000L; // Character special file
        private static readonly long c_ISSOCK = (long)0140000L; // Socket

        // FileInfoHeader creates a partially-populated Header from fi.
        // If fi describes a symlink, FileInfoHeader records link as the link target.
        // If fi describes a directory, a slash is appended to the name.
        //
        // Since os.FileInfo's Name method only returns the base name of
        // the file it describes, it may be necessary to modify Header.Name
        // to provide the full path name of the file.
        public static (ptr<Header>, error) FileInfoHeader(os.FileInfo fi, @string link)
        {
            ptr<Header> _p0 = default!;
            error _p0 = default!;

            if (fi == null)
            {
                return (_addr_null!, error.As(errors.New("archive/tar: FileInfo is nil"))!);
            }

            var fm = fi.Mode();
            ptr<Header> h = addr(new Header(Name:fi.Name(),ModTime:fi.ModTime(),Mode:int64(fm.Perm()),));

            if (fm.IsRegular()) 
                h.Typeflag = TypeReg;
                h.Size = fi.Size();
            else if (fi.IsDir()) 
                h.Typeflag = TypeDir;
                h.Name += "/";
            else if (fm & os.ModeSymlink != 0L) 
                h.Typeflag = TypeSymlink;
                h.Linkname = link;
            else if (fm & os.ModeDevice != 0L) 
                if (fm & os.ModeCharDevice != 0L)
                {
                    h.Typeflag = TypeChar;
                }
                else
                {
                    h.Typeflag = TypeBlock;
                }

            else if (fm & os.ModeNamedPipe != 0L) 
                h.Typeflag = TypeFifo;
            else if (fm & os.ModeSocket != 0L) 
                return (_addr_null!, error.As(fmt.Errorf("archive/tar: sockets not supported"))!);
            else 
                return (_addr_null!, error.As(fmt.Errorf("archive/tar: unknown file mode %v", fm))!);
                        if (fm & os.ModeSetuid != 0L)
            {
                h.Mode |= c_ISUID;
            }

            if (fm & os.ModeSetgid != 0L)
            {
                h.Mode |= c_ISGID;
            }

            if (fm & os.ModeSticky != 0L)
            {
                h.Mode |= c_ISVTX;
            } 
            // If possible, populate additional fields from OS-specific
            // FileInfo fields.
            {
                ptr<Header> (sys, ok) = fi.Sys()._<ptr<Header>>();

                if (ok)
                { 
                    // This FileInfo came from a Header (not the OS). Use the
                    // original Header to populate all remaining fields.
                    h.Uid = sys.Uid;
                    h.Gid = sys.Gid;
                    h.Uname = sys.Uname;
                    h.Gname = sys.Gname;
                    h.AccessTime = sys.AccessTime;
                    h.ChangeTime = sys.ChangeTime;
                    if (sys.Xattrs != null)
                    {
                        h.Xattrs = make_map<@string, @string>();
                        {
                            var k__prev1 = k;
                            var v__prev1 = v;

                            foreach (var (__k, __v) in sys.Xattrs)
                            {
                                k = __k;
                                v = __v;
                                h.Xattrs[k] = v;
                            }

                            k = k__prev1;
                            v = v__prev1;
                        }
                    }

                    if (sys.Typeflag == TypeLink)
                    { 
                        // hard link
                        h.Typeflag = TypeLink;
                        h.Size = 0L;
                        h.Linkname = sys.Linkname;

                    }

                    if (sys.PAXRecords != null)
                    {
                        h.PAXRecords = make_map<@string, @string>();
                        {
                            var k__prev1 = k;
                            var v__prev1 = v;

                            foreach (var (__k, __v) in sys.PAXRecords)
                            {
                                k = __k;
                                v = __v;
                                h.PAXRecords[k] = v;
                            }

                            k = k__prev1;
                            v = v__prev1;
                        }
                    }

                }

            }

            if (sysStat != null)
            {
                return (_addr_h!, error.As(sysStat(fi, h))!);
            }

            return (_addr_h!, error.As(null!)!);

        }

        // isHeaderOnlyType checks if the given type flag is of the type that has no
        // data section even if a size is specified.
        private static bool isHeaderOnlyType(byte flag)
        {

            if (flag == TypeLink || flag == TypeSymlink || flag == TypeChar || flag == TypeBlock || flag == TypeDir || flag == TypeFifo) 
                return true;
            else 
                return false;
            
        }

        private static long min(long a, long b)
        {
            if (a < b)
            {
                return a;
            }

            return b;

        }
    }
}}
