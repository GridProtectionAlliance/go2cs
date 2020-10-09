// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tar -- go2cs converted at 2020 October 09 05:08:01 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\format.go
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        // Format represents the tar archive format.
        //
        // The original tar format was introduced in Unix V7.
        // Since then, there have been multiple competing formats attempting to
        // standardize or extend the V7 format to overcome its limitations.
        // The most common formats are the USTAR, PAX, and GNU formats,
        // each with their own advantages and limitations.
        //
        // The following table captures the capabilities of each format:
        //
        //                      |  USTAR |       PAX |       GNU
        //    ------------------+--------+-----------+----------
        //    Name              |   256B | unlimited | unlimited
        //    Linkname          |   100B | unlimited | unlimited
        //    Size              | uint33 | unlimited |    uint89
        //    Mode              | uint21 |    uint21 |    uint57
        //    Uid/Gid           | uint21 | unlimited |    uint57
        //    Uname/Gname       |    32B | unlimited |       32B
        //    ModTime           | uint33 | unlimited |     int89
        //    AccessTime        |    n/a | unlimited |     int89
        //    ChangeTime        |    n/a | unlimited |     int89
        //    Devmajor/Devminor | uint21 |    uint21 |    uint57
        //    ------------------+--------+-----------+----------
        //    string encoding   |  ASCII |     UTF-8 |    binary
        //    sub-second times  |     no |       yes |        no
        //    sparse files      |     no |       yes |       yes
        //
        // The table's upper portion shows the Header fields, where each format reports
        // the maximum number of bytes allowed for each string field and
        // the integer type used to store each numeric field
        // (where timestamps are stored as the number of seconds since the Unix epoch).
        //
        // The table's lower portion shows specialized features of each format,
        // such as supported string encodings, support for sub-second timestamps,
        // or support for sparse files.
        //
        // The Writer currently provides no support for sparse files.
        public partial struct Format // : long
        {
        }

        // Constants to identify various tar formats.
 
        // Deliberately hide the meaning of constants from public API.
        private static readonly Format _ = (Format)(1L << (int)(iota)) / 4L; // Sequence of 0, 0, 1, 2, 4, 8, etc...

        // FormatUnknown indicates that the format is unknown.
        public static readonly var FormatUnknown = 0; 

        // The format of the original Unix V7 tar tool prior to standardization.
        private static readonly var formatV7 = 1; 

        // FormatUSTAR represents the USTAR header format defined in POSIX.1-1988.
        //
        // While this format is compatible with most tar readers,
        // the format has several limitations making it unsuitable for some usages.
        // Most notably, it cannot support sparse files, files larger than 8GiB,
        // filenames larger than 256 characters, and non-ASCII filenames.
        //
        // Reference:
        //    http://pubs.opengroup.org/onlinepubs/9699919799/utilities/pax.html#tag_20_92_13_06
        public static readonly var FormatUSTAR = 2; 

        // FormatPAX represents the PAX header format defined in POSIX.1-2001.
        //
        // PAX extends USTAR by writing a special file with Typeflag TypeXHeader
        // preceding the original header. This file contains a set of key-value
        // records, which are used to overcome USTAR's shortcomings, in addition to
        // providing the ability to have sub-second resolution for timestamps.
        //
        // Some newer formats add their own extensions to PAX by defining their
        // own keys and assigning certain semantic meaning to the associated values.
        // For example, sparse file support in PAX is implemented using keys
        // defined by the GNU manual (e.g., "GNU.sparse.map").
        //
        // Reference:
        //    http://pubs.opengroup.org/onlinepubs/009695399/utilities/pax.html
        public static readonly var FormatPAX = 3; 

        // FormatGNU represents the GNU header format.
        //
        // The GNU header format is older than the USTAR and PAX standards and
        // is not compatible with them. The GNU format supports
        // arbitrary file sizes, filenames of arbitrary encoding and length,
        // sparse files, and other features.
        //
        // It is recommended that PAX be chosen over GNU unless the target
        // application can only parse GNU formatted archives.
        //
        // Reference:
        //    https://www.gnu.org/software/tar/manual/html_node/Standard.html
        public static readonly var FormatGNU = 4; 

        // Schily's tar format, which is incompatible with USTAR.
        // This does not cover STAR extensions to the PAX format; these fall under
        // the PAX format.
        private static readonly var formatSTAR = 5;

        private static readonly var formatMax = 6;


        public static bool has(this Format f, Format f2)
        {
            return f & f2 != 0L;
        }
        private static void mayBe(this ptr<Format> _addr_f, Format f2)
        {
            ref Format f = ref _addr_f.val;

            f.val |= f2;
        }
        private static void mayOnlyBe(this ptr<Format> _addr_f, Format f2)
        {
            ref Format f = ref _addr_f.val;

            f.val &= f2;
        }
        private static void mustNotBe(this ptr<Format> _addr_f, Format f2)
        {
            ref Format f = ref _addr_f.val;

            f.val &= f2;
        }

        private static map formatNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Format, @string>{formatV7:"V7",FormatUSTAR:"USTAR",FormatPAX:"PAX",FormatGNU:"GNU",formatSTAR:"STAR",};

        public static @string String(this Format f)
        {
            slice<@string> ss = default;
            {
                var f2 = Format(1L);

                while (f2 < formatMax)
                {
                    if (f.has(f2))
                    {
                        ss = append(ss, formatNames[f2]);
                    f2 <<= 1L;
                    }

                }

            }
            switch (len(ss))
            {
                case 0L: 
                    return "<unknown>";
                    break;
                case 1L: 
                    return ss[0L];
                    break;
                default: 
                    return "(" + strings.Join(ss, " | ") + ")";
                    break;
            }

        }

        // Magics used to identify various formats.
        private static readonly @string magicGNU = (@string)"ustar ";
        private static readonly @string versionGNU = (@string)" \x00";
        private static readonly @string magicUSTAR = (@string)"ustar\x00";
        private static readonly @string versionUSTAR = (@string)"00";
        private static readonly @string trailerSTAR = (@string)"tar\x00";


        // Size constants from various tar specifications.
        private static readonly long blockSize = (long)512L; // Size of each block in a tar stream
        private static readonly long nameSize = (long)100L; // Max length of the name field in USTAR format
        private static readonly long prefixSize = (long)155L; // Max length of the prefix field in USTAR format

        // blockPadding computes the number of bytes needed to pad offset up to the
        // nearest block edge where 0 <= n < blockSize.
        private static long blockPadding(long offset)
        {
            long n = default;

            return -offset & (blockSize - 1L);
        }

        private static block zeroBlock = default;

        private partial struct block // : array<byte>
        {
        }

        // Convert block to any number of formats.
        private static ptr<headerV7> V7(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            return _addr_(headerV7.val)(b)!;
        }
        private static ptr<headerGNU> GNU(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            return _addr_(headerGNU.val)(b)!;
        }
        private static ptr<headerSTAR> STAR(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            return _addr_(headerSTAR.val)(b)!;
        }
        private static ptr<headerUSTAR> USTAR(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            return _addr_(headerUSTAR.val)(b)!;
        }
        private static sparseArray Sparse(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            return sparseArray(b[..]);
        }

        // GetFormat checks that the block is a valid tar header based on the checksum.
        // It then attempts to guess the specific format based on magic values.
        // If the checksum fails, then FormatUnknown is returned.
        private static Format GetFormat(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;
 
            // Verify checksum.
            parser p = default;
            var value = p.parseOctal(b.V7().Chksum());
            var (chksum1, chksum2) = b.ComputeChecksum();
            if (p.err != null || (value != chksum1 && value != chksum2))
            {
                return FormatUnknown;
            } 

            // Guess the magic values.
            var magic = string(b.USTAR().Magic());
            var version = string(b.USTAR().Version());
            var trailer = string(b.STAR().Trailer());

            if (magic == magicUSTAR && trailer == trailerSTAR) 
                return formatSTAR;
            else if (magic == magicUSTAR) 
                return FormatUSTAR | FormatPAX;
            else if (magic == magicGNU && version == versionGNU) 
                return FormatGNU;
            else 
                return formatV7;
            
        }

        // SetFormat writes the magic values necessary for specified format
        // and then updates the checksum accordingly.
        private static void SetFormat(this ptr<block> _addr_b, Format format) => func((_, panic, __) =>
        {
            ref block b = ref _addr_b.val;
 
            // Set the magic values.

            if (format.has(formatV7))             else if (format.has(FormatGNU)) 
                copy(b.GNU().Magic(), magicGNU);
                copy(b.GNU().Version(), versionGNU);
            else if (format.has(formatSTAR)) 
                copy(b.STAR().Magic(), magicUSTAR);
                copy(b.STAR().Version(), versionUSTAR);
                copy(b.STAR().Trailer(), trailerSTAR);
            else if (format.has(FormatUSTAR | FormatPAX)) 
                copy(b.USTAR().Magic(), magicUSTAR);
                copy(b.USTAR().Version(), versionUSTAR);
            else 
                panic("invalid format");
            // Update checksum.
            // This field is special in that it is terminated by a NULL then space.
            formatter f = default;
            var field = b.V7().Chksum();
            var (chksum, _) = b.ComputeChecksum(); // Possible values are 256..128776
            f.formatOctal(field[..7L], chksum); // Never fails since 128776 < 262143
            field[7L] = ' ';

        });

        // ComputeChecksum computes the checksum for the header block.
        // POSIX specifies a sum of the unsigned byte values, but the Sun tar used
        // signed byte values.
        // We compute and return both.
        private static (long, long) ComputeChecksum(this ptr<block> _addr_b)
        {
            long unsigned = default;
            long signed = default;
            ref block b = ref _addr_b.val;

            foreach (var (i, c) in b)
            {
                if (148L <= i && i < 156L)
                {
                    c = ' '; // Treat the checksum field itself as all spaces.
                }

                unsigned += int64(c);
                signed += int64(int8(c));

            }
            return (unsigned, signed);

        }

        // Reset clears the block with all zeros.
        private static void Reset(this ptr<block> _addr_b)
        {
            ref block b = ref _addr_b.val;

            b.val = new block();
        }

        private partial struct headerV7 // : array<byte>
        {
        }

        private static slice<byte> Name(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[000L..][..100L];
        }
        private static slice<byte> Mode(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[100L..][..8L];
        }
        private static slice<byte> UID(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[108L..][..8L];
        }
        private static slice<byte> GID(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[116L..][..8L];
        }
        private static slice<byte> Size(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[124L..][..12L];
        }
        private static slice<byte> ModTime(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[136L..][..12L];
        }
        private static slice<byte> Chksum(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[148L..][..8L];
        }
        private static slice<byte> TypeFlag(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[156L..][..1L];
        }
        private static slice<byte> LinkName(this ptr<headerV7> _addr_h)
        {
            ref headerV7 h = ref _addr_h.val;

            return h[157L..][..100L];
        }

        private partial struct headerGNU // : array<byte>
        {
        }

        private static ptr<headerV7> V7(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return _addr_(headerV7.val)(h)!;
        }
        private static slice<byte> Magic(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[257L..][..6L];
        }
        private static slice<byte> Version(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[263L..][..2L];
        }
        private static slice<byte> UserName(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[265L..][..32L];
        }
        private static slice<byte> GroupName(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[297L..][..32L];
        }
        private static slice<byte> DevMajor(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[329L..][..8L];
        }
        private static slice<byte> DevMinor(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[337L..][..8L];
        }
        private static slice<byte> AccessTime(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[345L..][..12L];
        }
        private static slice<byte> ChangeTime(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[357L..][..12L];
        }
        private static sparseArray Sparse(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return sparseArray(h[386L..][..24L * 4L + 1L]);
        }
        private static slice<byte> RealSize(this ptr<headerGNU> _addr_h)
        {
            ref headerGNU h = ref _addr_h.val;

            return h[483L..][..12L];
        }

        private partial struct headerSTAR // : array<byte>
        {
        }

        private static ptr<headerV7> V7(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return _addr_(headerV7.val)(h)!;
        }
        private static slice<byte> Magic(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[257L..][..6L];
        }
        private static slice<byte> Version(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[263L..][..2L];
        }
        private static slice<byte> UserName(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[265L..][..32L];
        }
        private static slice<byte> GroupName(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[297L..][..32L];
        }
        private static slice<byte> DevMajor(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[329L..][..8L];
        }
        private static slice<byte> DevMinor(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[337L..][..8L];
        }
        private static slice<byte> Prefix(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[345L..][..131L];
        }
        private static slice<byte> AccessTime(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[476L..][..12L];
        }
        private static slice<byte> ChangeTime(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[488L..][..12L];
        }
        private static slice<byte> Trailer(this ptr<headerSTAR> _addr_h)
        {
            ref headerSTAR h = ref _addr_h.val;

            return h[508L..][..4L];
        }

        private partial struct headerUSTAR // : array<byte>
        {
        }

        private static ptr<headerV7> V7(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return _addr_(headerV7.val)(h)!;
        }
        private static slice<byte> Magic(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[257L..][..6L];
        }
        private static slice<byte> Version(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[263L..][..2L];
        }
        private static slice<byte> UserName(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[265L..][..32L];
        }
        private static slice<byte> GroupName(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[297L..][..32L];
        }
        private static slice<byte> DevMajor(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[329L..][..8L];
        }
        private static slice<byte> DevMinor(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[337L..][..8L];
        }
        private static slice<byte> Prefix(this ptr<headerUSTAR> _addr_h)
        {
            ref headerUSTAR h = ref _addr_h.val;

            return h[345L..][..155L];
        }

        private partial struct sparseArray // : slice<byte>
        {
        }

        private static sparseElem Entry(this sparseArray s, long i)
        {
            return sparseElem(s[i * 24L..]);
        }
        private static slice<byte> IsExtended(this sparseArray s)
        {
            return s[24L * s.MaxEntries()..][..1L];
        }
        private static long MaxEntries(this sparseArray s)
        {
            return len(s) / 24L;
        }

        private partial struct sparseElem // : slice<byte>
        {
        }

        private static slice<byte> Offset(this sparseElem s)
        {
            return s[00L..][..12L];
        }
        private static slice<byte> Length(this sparseElem s)
        {
            return s[12L..][..12L];
        }
    }
}}
