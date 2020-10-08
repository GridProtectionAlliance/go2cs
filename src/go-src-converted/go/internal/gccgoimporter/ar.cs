// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gccgoimporter -- go2cs converted at 2020 October 08 04:56:07 UTC
// import "go/internal/gccgoimporter" ==> using gccgoimporter = go.go.@internal.gccgoimporter_package
// Original source: C:\Go\src\go\internal\gccgoimporter\ar.go
using bytes = go.bytes_package;
using elf = go.debug.elf_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gccgoimporter_package
    {
        // Magic strings for different archive file formats.
        private static readonly @string armag = (@string)"!<arch>\n";
        private static readonly @string armagt = (@string)"!<thin>\n";
        private static readonly @string armagb = (@string)"<bigaf>\n";


        // Offsets and sizes for fields in a standard archive header.
        private static readonly long arNameOff = (long)0L;
        private static readonly long arNameSize = (long)16L;
        private static readonly var arDateOff = (var)arNameOff + arNameSize;
        private static readonly long arDateSize = (long)12L;
        private static readonly var arUIDOff = (var)arDateOff + arDateSize;
        private static readonly long arUIDSize = (long)6L;
        private static readonly var arGIDOff = (var)arUIDOff + arUIDSize;
        private static readonly long arGIDSize = (long)6L;
        private static readonly var arModeOff = (var)arGIDOff + arGIDSize;
        private static readonly long arModeSize = (long)8L;
        private static readonly var arSizeOff = (var)arModeOff + arModeSize;
        private static readonly long arSizeSize = (long)10L;
        private static readonly var arFmagOff = (var)arSizeOff + arSizeSize;
        private static readonly long arFmagSize = (long)2L;

        private static readonly var arHdrSize = (var)arFmagOff + arFmagSize;


        // The contents of the fmag field of a standard archive header.
        private static readonly @string arfmag = (@string)"`\n";

        // arExportData takes an archive file and returns a ReadSeeker for the
        // export data in that file. This assumes that there is only one
        // object in the archive containing export data, which is not quite
        // what gccgo does; gccgo concatenates together all the export data
        // for all the objects in the file.  In practice that case does not arise.


        // arExportData takes an archive file and returns a ReadSeeker for the
        // export data in that file. This assumes that there is only one
        // object in the archive containing export data, which is not quite
        // what gccgo does; gccgo concatenates together all the export data
        // for all the objects in the file.  In practice that case does not arise.
        private static (io.ReadSeeker, error) arExportData(io.ReadSeeker archive)
        {
            io.ReadSeeker _p0 = default;
            error _p0 = default!;

            {
                var (_, err) = archive.Seek(0L, io.SeekStart);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            array<byte> buf = new array<byte>(len(armag));
            {
                (_, err) = archive.Read(buf[..]);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }



            if (string(buf[..]) == armag) 
                return standardArExportData(archive);
            else if (string(buf[..]) == armagt) 
                return (null, error.As(errors.New("unsupported thin archive"))!);
            else if (string(buf[..]) == armagb) 
                return aixBigArExportData(archive);
            else 
                return (null, error.As(fmt.Errorf("unrecognized archive file format %q", buf[..]))!);
            
        }

        // standardArExportData returns export data from a standard archive.
        private static (io.ReadSeeker, error) standardArExportData(io.ReadSeeker archive)
        {
            io.ReadSeeker _p0 = default;
            error _p0 = default!;

            var off = int64(len(armag));
            while (true)
            {
                array<byte> hdrBuf = new array<byte>(arHdrSize);
                {
                    var (_, err) = archive.Read(hdrBuf[..]);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

                off += arHdrSize;

                if (bytes.Compare(hdrBuf[arFmagOff..arFmagOff + arFmagSize], (slice<byte>)arfmag) != 0L)
                {
                    return (null, error.As(fmt.Errorf("archive header format header (%q)", hdrBuf[..]))!);
                }

                var (size, err) = strconv.ParseInt(strings.TrimSpace(string(hdrBuf[arSizeOff..arSizeOff + arSizeSize])), 10L, 64L);
                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("error parsing size in archive header (%q): %v", hdrBuf[..], err))!);
                }

                var fn = hdrBuf[arNameOff..arNameOff + arNameSize];
                if (fn[0L] == '/' && (fn[1L] == ' ' || fn[1L] == '/' || bytes.Compare(fn[..8L], (slice<byte>)"/SYM64/ ") == 0L))
                { 
                    // Archive symbol table or extended name table,
                    // which we don't care about.
                }
                else
                {
                    var archiveAt = readerAtFromSeeker(archive);
                    var (ret, err) = elfFromAr(_addr_io.NewSectionReader(archiveAt, off, size));
                    if (ret != null || err != null)
                    {
                        return (ret, error.As(err)!);
                    }

                }

                if (size & 1L != 0L)
                {
                    size++;
                }

                off += size;
                {
                    (_, err) = archive.Seek(off, io.SeekStart);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

            }


        }

        // elfFromAr tries to get export data from an archive member as an ELF file.
        // If there is no export data, this returns nil, nil.
        private static (io.ReadSeeker, error) elfFromAr(ptr<io.SectionReader> _addr_member)
        {
            io.ReadSeeker _p0 = default;
            error _p0 = default!;
            ref io.SectionReader member = ref _addr_member.val;

            var (ef, err) = elf.NewFile(member);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var sec = ef.Section(".go_export");
            if (sec == null)
            {
                return (null, error.As(null!)!);
            }

            return (sec.Open(), error.As(null!)!);

        }

        // aixBigArExportData returns export data from an AIX big archive.
        private static (io.ReadSeeker, error) aixBigArExportData(io.ReadSeeker archive)
        {
            io.ReadSeeker _p0 = default;
            error _p0 = default!;

            var archiveAt = readerAtFromSeeker(archive);
            var (arch, err) = xcoff.NewArchive(archiveAt);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            foreach (var (_, mem) in arch.Members)
            {
                var (f, err) = arch.GetFile(mem.Name);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                var sdat = f.CSect(".go_export");
                if (sdat != null)
                {
                    return (bytes.NewReader(sdat), error.As(null!)!);
                }

            }
            return (null, error.As(fmt.Errorf(".go_export not found in this archive"))!);

        }

        // readerAtFromSeeker turns an io.ReadSeeker into an io.ReaderAt.
        // This is only safe because there won't be any concurrent seeks
        // while this code is executing.
        private static io.ReaderAt readerAtFromSeeker(io.ReadSeeker rs)
        {
            {
                io.ReaderAt (ret, ok) = rs._<io.ReaderAt>();

                if (ok)
                {
                    return ret;
                }

            }

            return new seekerReadAt(rs);

        }

        private partial struct seekerReadAt
        {
            public io.ReadSeeker seeker;
        }

        private static (long, error) ReadAt(this seekerReadAt sra, slice<byte> p, long off)
        {
            long _p0 = default;
            error _p0 = default!;

            {
                var (_, err) = sra.seeker.Seek(off, io.SeekStart);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            return sra.seeker.Read(p);

        }
    }
}}}
