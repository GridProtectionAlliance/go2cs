// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
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

// package sym -- go2cs converted at 2020 October 08 04:40:32 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\symkind.go

using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        // A SymKind describes the kind of memory represented by a symbol.
        public partial struct SymKind // : byte
        {
        }

        // Defined SymKind values.
        //
        // TODO(rsc): Give idiomatic Go names.
        //go:generate stringer -type=SymKind
        public static readonly SymKind Sxxx = (SymKind)iota;
        public static readonly var STEXT = (var)0;
        public static readonly var SELFRXSECT = (var)1; 

        // Read-only sections.
        public static readonly var STYPE = (var)2;
        public static readonly var SSTRING = (var)3;
        public static readonly var SGOSTRING = (var)4;
        public static readonly var SGOFUNC = (var)5;
        public static readonly var SGCBITS = (var)6;
        public static readonly var SRODATA = (var)7;
        public static readonly var SFUNCTAB = (var)8;

        public static readonly var SELFROSECT = (var)9;
        public static readonly var SMACHOPLT = (var)10; 

        // Read-only sections with relocations.
        //
        // Types STYPE-SFUNCTAB above are written to the .rodata section by default.
        // When linking a shared object, some conceptually "read only" types need to
        // be written to by relocations and putting them in a section called
        // ".rodata" interacts poorly with the system linkers. The GNU linkers
        // support this situation by arranging for sections of the name
        // ".data.rel.ro.XXX" to be mprotected read only by the dynamic linker after
        // relocations have applied, so when the Go linker is creating a shared
        // object it checks all objects of the above types and bumps any object that
        // has a relocation to it to the corresponding type below, which are then
        // written to sections with appropriate magic names.
        public static readonly var STYPERELRO = (var)11;
        public static readonly var SSTRINGRELRO = (var)12;
        public static readonly var SGOSTRINGRELRO = (var)13;
        public static readonly var SGOFUNCRELRO = (var)14;
        public static readonly var SGCBITSRELRO = (var)15;
        public static readonly var SRODATARELRO = (var)16;
        public static readonly var SFUNCTABRELRO = (var)17; 

        // Part of .data.rel.ro if it exists, otherwise part of .rodata.
        public static readonly var STYPELINK = (var)18;
        public static readonly var SITABLINK = (var)19;
        public static readonly var SSYMTAB = (var)20;
        public static readonly var SPCLNTAB = (var)21; 

        // Writable sections.
        public static readonly var SFirstWritable = (var)22;
        public static readonly var SBUILDINFO = (var)23;
        public static readonly var SELFSECT = (var)24;
        public static readonly var SMACHO = (var)25;
        public static readonly var SMACHOGOT = (var)26;
        public static readonly var SWINDOWS = (var)27;
        public static readonly var SELFGOT = (var)28;
        public static readonly var SNOPTRDATA = (var)29;
        public static readonly var SINITARR = (var)30;
        public static readonly var SDATA = (var)31;
        public static readonly var SXCOFFTOC = (var)32;
        public static readonly var SBSS = (var)33;
        public static readonly var SNOPTRBSS = (var)34;
        public static readonly var SLIBFUZZER_EXTRA_COUNTER = (var)35;
        public static readonly var STLSBSS = (var)36;
        public static readonly var SXREF = (var)37;
        public static readonly var SMACHOSYMSTR = (var)38;
        public static readonly var SMACHOSYMTAB = (var)39;
        public static readonly var SMACHOINDIRECTPLT = (var)40;
        public static readonly var SMACHOINDIRECTGOT = (var)41;
        public static readonly var SFILEPATH = (var)42;
        public static readonly var SCONST = (var)43;
        public static readonly var SDYNIMPORT = (var)44;
        public static readonly var SHOSTOBJ = (var)45;
        public static readonly var SUNDEFEXT = (var)46; // Undefined symbol for resolution by external linker

        // Sections for debugging information
        public static readonly var SDWARFSECT = (var)47;
        public static readonly var SDWARFINFO = (var)48;
        public static readonly var SDWARFRANGE = (var)49;
        public static readonly var SDWARFLOC = (var)50;
        public static readonly var SDWARFLINES = (var)51; 

        // ABI aliases (these never appear in the output)
        public static readonly var SABIALIAS = (var)52;


        // AbiSymKindToSymKind maps values read from object files (which are
        // of type cmd/internal/objabi.SymKind) to values of type SymKind.
        public static array<SymKind> AbiSymKindToSymKind = new array<SymKind>(new SymKind[] { Sxxx, STEXT, SRODATA, SNOPTRDATA, SDATA, SBSS, SNOPTRBSS, STLSBSS, SDWARFINFO, SDWARFRANGE, SDWARFLOC, SDWARFLINES, SABIALIAS, SLIBFUZZER_EXTRA_COUNTER });

        // ReadOnly are the symbol kinds that form read-only sections. In some
        // cases, if they will require relocations, they are transformed into
        // rel-ro sections using relROMap.
        public static SymKind ReadOnly = new slice<SymKind>(new SymKind[] { STYPE, SSTRING, SGOSTRING, SGOFUNC, SGCBITS, SRODATA, SFUNCTAB });

        // RelROMap describes the transformation of read-only symbols to rel-ro
        // symbols.
        public static map RelROMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<SymKind, SymKind>{STYPE:STYPERELRO,SSTRING:SSTRINGRELRO,SGOSTRING:SGOSTRINGRELRO,SGOFUNC:SGOFUNCRELRO,SGCBITS:SGCBITSRELRO,SRODATA:SRODATARELRO,SFUNCTAB:SFUNCTABRELRO,};

        // IsData returns true if the type is a data type.
        public static bool IsData(this SymKind t)
        {
            return t == SDATA || t == SNOPTRDATA || t == SBSS || t == SNOPTRBSS;
        }
    }
}}}}
