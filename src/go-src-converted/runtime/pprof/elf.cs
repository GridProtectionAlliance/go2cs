// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 August 29 08:21:47 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\elf.go
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        private static var errBadELF = errors.New("malformed ELF binary");        private static var errNoBuildID = errors.New("no NT_GNU_BUILD_ID found in ELF binary");

        // elfBuildID returns the GNU build ID of the named ELF binary,
        // without introducing a dependency on debug/elf and its dependencies.
        private static (@string, error) elfBuildID(@string file) => func((defer, _, __) =>
        {
            var buf = make_slice<byte>(256L);
            var (f, err) = os.Open(file);
            if (err != null)
            {
                return ("", err);
            }
            defer(f.Close());

            {
                var (_, err) = f.ReadAt(buf[..64L], 0L);

                if (err != null)
                {
                    return ("", err);
                } 

                // ELF file begins with \x7F E L F.

            } 

            // ELF file begins with \x7F E L F.
            if (buf[0L] != 0x7FUL || buf[1L] != 'E' || buf[2L] != 'L' || buf[3L] != 'F')
            {
                return ("", errBadELF);
            }
            binary.ByteOrder byteOrder = default;
            switch (buf[5L])
            {
                case 1L: // little-endian
                    byteOrder = binary.LittleEndian;
                    break;
                case 2L: // big-endian
                    byteOrder = binary.BigEndian;
                    break;
                default: 
                    return ("", errBadELF);
                    break;
            }

            long shnum = default;
            long shoff = default;            long shentsize = default;

            switch (buf[4L])
            {
                case 1L: // 32-bit file header
                    shoff = int64(byteOrder.Uint32(buf[32L..]));
                    shentsize = int64(byteOrder.Uint16(buf[46L..]));
                    if (shentsize != 40L)
                    {
                        return ("", errBadELF);
                    }
                    shnum = int(byteOrder.Uint16(buf[48L..]));
                    break;
                case 2L: // 64-bit file header
                    shoff = int64(byteOrder.Uint64(buf[40L..]));
                    shentsize = int64(byteOrder.Uint16(buf[58L..]));
                    if (shentsize != 64L)
                    {
                        return ("", errBadELF);
                    }
                    shnum = int(byteOrder.Uint16(buf[60L..]));
                    break;
                default: 
                    return ("", errBadELF);
                    break;
            }

            for (long i = 0L; i < shnum; i++)
            {
                {
                    (_, err) = f.ReadAt(buf[..shentsize], shoff + int64(i) * shentsize);

                    if (err != null)
                    {
                        return ("", err);
                    }

                }
                {
                    var typ = byteOrder.Uint32(buf[4L..]);

                    if (typ != 7L)
                    { // SHT_NOTE
                        continue;
                    }

                }
                long off = default;                long size = default;

                if (shentsize == 40L)
                { 
                    // 32-bit section header
                    off = int64(byteOrder.Uint32(buf[16L..]));
                    size = int64(byteOrder.Uint32(buf[20L..]));
                }
                else
                { 
                    // 64-bit section header
                    off = int64(byteOrder.Uint64(buf[24L..]));
                    size = int64(byteOrder.Uint64(buf[32L..]));
                }
                size += off;
                while (off < size)
                {
                    {
                        (_, err) = f.ReadAt(buf[..16L], off);

                        if (err != null)
                        { // room for header + name GNU\x00
                            return ("", err);
                        }

                    }
                    var nameSize = int(byteOrder.Uint32(buf[0L..]));
                    var descSize = int(byteOrder.Uint32(buf[4L..]));
                    var noteType = int(byteOrder.Uint32(buf[8L..]));
                    var descOff = off + int64(12L + (nameSize + 3L) & ~3L);
                    off = descOff + int64((descSize + 3L) & ~3L);
                    if (nameSize != 4L || noteType != 3L || buf[12L] != 'G' || buf[13L] != 'N' || buf[14L] != 'U' || buf[15L] != '\x00')
                    { // want name GNU\x00 type 3 (NT_GNU_BUILD_ID)
                        continue;
                    }
                    if (descSize > len(buf))
                    {
                        return ("", errBadELF);
                    }
                    {
                        (_, err) = f.ReadAt(buf[..descSize], descOff);

                        if (err != null)
                        {
                            return ("", err);
                        }

                    }
                    return (fmt.Sprintf("%x", buf[..descSize]), null);
                }

            }

            return ("", errNoBuildID);
        });
    }
}}
