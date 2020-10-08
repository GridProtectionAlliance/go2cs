// Inferno utils/include/ar.h
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/include/ar.h
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package ld -- go2cs converted at 2020 October 08 04:40:09 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\ar.go
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        public static readonly long SARMAG = (long)8L;
        public static readonly long SAR_HDR = (long)16L + 44L;


        public static readonly @string ARMAG = (@string)"!<arch>\n";


        public partial struct ArHdr
        {
            public @string name;
            public @string date;
            public @string uid;
            public @string gid;
            public @string mode;
            public @string size;
            public @string fmag;
        }

        // hostArchive reads an archive file holding host objects and links in
        // required objects. The general format is the same as a Go archive
        // file, but it has an armap listing symbols and the objects that
        // define them. This is used for the compiler support library
        // libgcc.a.
        private static void hostArchive(ptr<Link> _addr_ctxt, @string name) => func((defer, _, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var (f, err) = bio.Open(name);
            if (err != null)
            {
                if (os.IsNotExist(err))
                { 
                    // It's OK if we don't have a libgcc file at all.
                    if (ctxt.Debugvlog != 0L)
                    {
                        ctxt.Logf("skipping libgcc file: %v\n", err);
                    }

                    return ;

                }

                Exitf("cannot open file %s: %v", name, err);

            }

            defer(f.Close());

            array<byte> magbuf = new array<byte>(len(ARMAG));
            {
                var (_, err) = io.ReadFull(f, magbuf[..]);

                if (err != null)
                {
                    Exitf("file %s too short", name);
                }

            }


            if (string(magbuf[..]) != ARMAG)
            {
                Exitf("%s is not an archive file", name);
            }

            ref ArHdr arhdr = ref heap(out ptr<ArHdr> _addr_arhdr);
            var l = nextar(f, f.Offset(), _addr_arhdr);
            if (l <= 0L)
            {
                Exitf("%s missing armap", name);
            }

            archiveMap armap = default;
            if (arhdr.name == "/" || arhdr.name == "/SYM64/")
            {
                armap = readArmap(name, _addr_f, arhdr);
            }
            else
            {
                Exitf("%s missing armap", name);
            }

            var loaded = make_map<ulong, bool>();
            var any = true;
            while (any)
            {
                slice<ulong> load = default;
                foreach (var (_, s) in ctxt.Syms.Allsym)
                {
                    foreach (var (i) in s.R)
                    {
                        var r = _addr_s.R[i]; // Copying sym.Reloc has measurable impact on performance
                        if (r.Sym != null && r.Sym.Type == sym.SXREF)
                        {
                            {
                                var off__prev2 = off;

                                var off = armap[r.Sym.Name];

                                if (off != 0L && !loaded[off])
                                {
                                    load = append(load, off);
                                    loaded[off] = true;
                                }

                                off = off__prev2;

                            }

                        }

                    }

                }
                {
                    var off__prev2 = off;

                    foreach (var (_, __off) in load)
                    {
                        off = __off;
                        l = nextar(f, int64(off), _addr_arhdr);
                        if (l <= 0L)
                        {
                            Exitf("%s missing archive entry at offset %d", name, off);
                        }

                        var pname = fmt.Sprintf("%s(%s)", name, arhdr.name);
                        l = atolwhex(arhdr.size);

                        ref sym.Library libgcc = ref heap(new sym.Library(Pkg:"libgcc"), out ptr<sym.Library> _addr_libgcc);
                        var h = ldobj(ctxt, f, _addr_libgcc, l, pname, name);
                        f.MustSeek(h.off, 0L);
                        h.ld(ctxt, f, h.pkg, h.length, h.pn);

                    }

                    off = off__prev2;
                }

                any = len(load) > 0L;

            }


        });

        // archiveMap is an archive symbol map: a mapping from symbol name to
        // offset within the archive file.
        private partial struct archiveMap // : map<@string, ulong>
        {
        }

        // readArmap reads the archive symbol map.
        private static archiveMap readArmap(@string filename, ptr<bio.Reader> _addr_f, ArHdr arhdr)
        {
            ref bio.Reader f = ref _addr_f.val;

            var is64 = arhdr.name == "/SYM64/";
            long wordSize = 4L;
            if (is64)
            {
                wordSize = 8L;
            }

            var contents = make_slice<byte>(atolwhex(arhdr.size));
            {
                var (_, err) = io.ReadFull(f, contents);

                if (err != null)
                {
                    Exitf("short read from %s", filename);
                }

            }


            ulong c = default;
            if (is64)
            {
                c = binary.BigEndian.Uint64(contents);
            }
            else
            {
                c = uint64(binary.BigEndian.Uint32(contents));
            }

            contents = contents[wordSize..];

            var ret = make(archiveMap);

            var names = contents[c * uint64(wordSize)..];
            for (var i = uint64(0L); i < c; i++)
            {
                long n = 0L;
                while (names[n] != 0L)
                {
                    n++;
                }

                var name = string(names[..n]);
                names = names[n + 1L..]; 

                // For Mach-O and PE/386 files we strip a leading
                // underscore from the symbol name.
                if (objabi.GOOS == "darwin" || (objabi.GOOS == "windows" && objabi.GOARCH == "386"))
                {
                    if (name[0L] == '_' && len(name) > 1L)
                    {
                        name = name[1L..];
                    }

                }

                ulong off = default;
                if (is64)
                {
                    off = binary.BigEndian.Uint64(contents);
                }
                else
                {
                    off = uint64(binary.BigEndian.Uint32(contents));
                }

                contents = contents[wordSize..];

                ret[name] = off;

            }


            return ret;

        }
    }
}}}}
