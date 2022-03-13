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

// package sym -- go2cs converted at 2022 March 13 06:33:29 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\sym\symkind.go
namespace go.cmd.link.@internal;

public static partial class sym_package {

// A SymKind describes the kind of memory represented by a symbol.
public partial struct SymKind { // : byte
}

// Defined SymKind values.
//
// TODO(rsc): Give idiomatic Go names.
//go:generate stringer -type=SymKind
public static readonly SymKind Sxxx = iota;
public static readonly var STEXT = 0;
public static readonly var SELFRXSECT = 1;
public static readonly var SMACHOPLT = 2; 

// Read-only sections.
public static readonly var STYPE = 3;
public static readonly var SSTRING = 4;
public static readonly var SGOSTRING = 5;
public static readonly var SGOFUNC = 6;
public static readonly var SGCBITS = 7;
public static readonly var SRODATA = 8;
public static readonly var SFUNCTAB = 9;

public static readonly var SELFROSECT = 10; 

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
public static readonly var STYPERELRO = 11;
public static readonly var SSTRINGRELRO = 12;
public static readonly var SGOSTRINGRELRO = 13;
public static readonly var SGOFUNCRELRO = 14;
public static readonly var SGCBITSRELRO = 15;
public static readonly var SRODATARELRO = 16;
public static readonly var SFUNCTABRELRO = 17; 

// Part of .data.rel.ro if it exists, otherwise part of .rodata.
public static readonly var STYPELINK = 18;
public static readonly var SITABLINK = 19;
public static readonly var SSYMTAB = 20;
public static readonly var SPCLNTAB = 21; 

// Writable sections.
public static readonly var SFirstWritable = 22;
public static readonly var SBUILDINFO = 23;
public static readonly var SELFSECT = 24;
public static readonly var SMACHO = 25;
public static readonly var SMACHOGOT = 26;
public static readonly var SWINDOWS = 27;
public static readonly var SELFGOT = 28;
public static readonly var SNOPTRDATA = 29;
public static readonly var SINITARR = 30;
public static readonly var SDATA = 31;
public static readonly var SXCOFFTOC = 32;
public static readonly var SBSS = 33;
public static readonly var SNOPTRBSS = 34;
public static readonly var SLIBFUZZER_EXTRA_COUNTER = 35;
public static readonly var STLSBSS = 36;
public static readonly var SXREF = 37;
public static readonly var SMACHOSYMSTR = 38;
public static readonly var SMACHOSYMTAB = 39;
public static readonly var SMACHOINDIRECTPLT = 40;
public static readonly var SMACHOINDIRECTGOT = 41;
public static readonly var SFILEPATH = 42;
public static readonly var SDYNIMPORT = 43;
public static readonly var SHOSTOBJ = 44;
public static readonly var SUNDEFEXT = 45; // Undefined symbol for resolution by external linker

// Sections for debugging information
public static readonly var SDWARFSECT = 46; 
// DWARF symbol types
public static readonly var SDWARFCUINFO = 47;
public static readonly var SDWARFCONST = 48;
public static readonly var SDWARFFCN = 49;
public static readonly var SDWARFABSFCN = 50;
public static readonly var SDWARFTYPE = 51;
public static readonly var SDWARFVAR = 52;
public static readonly var SDWARFRANGE = 53;
public static readonly var SDWARFLOC = 54;
public static readonly var SDWARFLINES = 55; 

// ABI aliases (these never appear in the output)
public static readonly var SABIALIAS = 56;

// AbiSymKindToSymKind maps values read from object files (which are
// of type cmd/internal/objabi.SymKind) to values of type SymKind.
public static array<SymKind> AbiSymKindToSymKind = new array<SymKind>(new SymKind[] { Sxxx, STEXT, SRODATA, SNOPTRDATA, SDATA, SBSS, SNOPTRBSS, STLSBSS, SDWARFCUINFO, SDWARFCONST, SDWARFFCN, SDWARFABSFCN, SDWARFTYPE, SDWARFVAR, SDWARFRANGE, SDWARFLOC, SDWARFLINES, SABIALIAS, SLIBFUZZER_EXTRA_COUNTER });

// ReadOnly are the symbol kinds that form read-only sections. In some
// cases, if they will require relocations, they are transformed into
// rel-ro sections using relROMap.
public static SymKind ReadOnly = new slice<SymKind>(new SymKind[] { STYPE, SSTRING, SGOSTRING, SGOFUNC, SGCBITS, SRODATA, SFUNCTAB });

// RelROMap describes the transformation of read-only symbols to rel-ro
// symbols.
public static map RelROMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<SymKind, SymKind>{STYPE:STYPERELRO,SSTRING:SSTRINGRELRO,SGOSTRING:SGOSTRINGRELRO,SGOFUNC:SGOFUNCRELRO,SGCBITS:SGCBITSRELRO,SRODATA:SRODATARELRO,SFUNCTAB:SFUNCTABRELRO,};

// IsData returns true if the type is a data type.
public static bool IsData(this SymKind t) {
    return t == SDATA || t == SNOPTRDATA || t == SBSS || t == SNOPTRBSS;
}

} // end sym_package
