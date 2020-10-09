// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 09 05:51:11 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\attribute.go

using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        // Attribute is a set of common symbol attributes.
        public partial struct Attribute // : int
        {
        }

 
        // AttrDuplicateOK marks a symbol that can be present in multiple object
        // files.
        public static readonly Attribute AttrDuplicateOK = (Attribute)1L << (int)(iota); 
        // AttrExternal marks function symbols loaded from host object files.
        public static readonly var AttrExternal = 0; 
        // AttrNoSplit marks functions that cannot split the stack; the linker
        // cares because it checks that there are no call chains of nosplit
        // functions that require more than StackLimit bytes (see
        // lib.go:dostkcheck)
        public static readonly var AttrNoSplit = 1; 
        // AttrReachable marks symbols that are transitively referenced from the
        // entry points. Unreachable symbols are not written to the output.
        public static readonly var AttrReachable = 2; 
        // AttrCgoExportDynamic and AttrCgoExportStatic mark symbols referenced
        // by directives written by cgo (in response to //export directives in
        // the source).
        public static readonly var AttrCgoExportDynamic = 3;
        public static readonly var AttrCgoExportStatic = 4; 
        // AttrSpecial marks symbols that do not have their address (i.e. Value)
        // computed by the usual mechanism of data.go:dodata() &
        // data.go:address().
        public static readonly var AttrSpecial = 5; 
        // AttrStackCheck is used by dostkcheck to only check each NoSplit
        // function's stack usage once.
        public static readonly var AttrStackCheck = 6; 
        // AttrNotInSymbolTable marks symbols that are not written to the symbol table.
        public static readonly var AttrNotInSymbolTable = 7; 
        // AttrOnList marks symbols that are on some list (such as the list of
        // all text symbols, or one of the lists of data symbols) and is
        // consulted to avoid bugs where a symbol is put on a list twice.
        public static readonly var AttrOnList = 8; 
        // AttrLocal marks symbols that are only visible within the module
        // (executable or shared library) being linked. Only relevant when
        // dynamically linking Go code.
        public static readonly var AttrLocal = 9; 
        // AttrReflectMethod marks certain methods from the reflect package that
        // can be used to call arbitrary methods. If no symbol with this bit set
        // is marked as reachable, more dead code elimination can be done.
        public static readonly var AttrReflectMethod = 10; 
        // AttrMakeTypelink Amarks types that should be added to the typelink
        // table. See typelinks.go:typelinks().
        public static readonly var AttrMakeTypelink = 11; 
        // AttrShared marks symbols compiled with the -shared option.
        public static readonly var AttrShared = 12; 
        // AttrVisibilityHidden symbols are ELF symbols with
        // visibility set to STV_HIDDEN. They become local symbols in
        // the final executable. Only relevant when internally linking
        // on an ELF platform.
        public static readonly var AttrVisibilityHidden = 13; 
        // AttrSubSymbol mostly means that the symbol appears on the Sub list of some
        // other symbol.  Unfortunately, it's not 100% reliable; at least, it's not set
        // correctly for the .TOC. symbol in Link.dodata.  Usually the Outer field of the
        // symbol points to the symbol whose list it is on, but that it is not set for the
        // symbols added to .windynamic in initdynimport in pe.go.
        //
        // TODO(mwhudson): fix the inconsistencies noticed above.
        //
        // Sub lists are used when loading host objects (sections from the host object
        // become regular linker symbols and symbols go on the Sub list of their section)
        // and for constructing the global offset table when internally linking a dynamic
        // executable.
        //
        // TODO(mwhudson): perhaps a better name for this is AttrNonGoSymbol.
        public static readonly var AttrSubSymbol = 14; 
        // AttrContainer is set on text symbols that are present as the .Outer for some
        // other symbol.
        public static readonly var AttrContainer = 15; 
        // AttrTopFrame means that the function is an entry point and unwinders
        // should stop when they hit this function.
        public static readonly var AttrTopFrame = 16; 
        // AttrReadOnly indicates whether the symbol's content (Symbol.P) is backed by
        // read-only memory.
        public static readonly var AttrReadOnly = 17; 
        // AttrDeferReturnTramp indicates the symbol is a trampoline of a deferreturn
        // call.
        public static readonly var AttrDeferReturnTramp = 18; 
        // 20 attributes defined so far.

        public static bool DuplicateOK(this Attribute a)
        {
            return a & AttrDuplicateOK != 0L;
        }
        public static bool External(this Attribute a)
        {
            return a & AttrExternal != 0L;
        }
        public static bool NoSplit(this Attribute a)
        {
            return a & AttrNoSplit != 0L;
        }
        public static bool Reachable(this Attribute a)
        {
            return a & AttrReachable != 0L;
        }
        public static bool CgoExportDynamic(this Attribute a)
        {
            return a & AttrCgoExportDynamic != 0L;
        }
        public static bool CgoExportStatic(this Attribute a)
        {
            return a & AttrCgoExportStatic != 0L;
        }
        public static bool Special(this Attribute a)
        {
            return a & AttrSpecial != 0L;
        }
        public static bool StackCheck(this Attribute a)
        {
            return a & AttrStackCheck != 0L;
        }
        public static bool NotInSymbolTable(this Attribute a)
        {
            return a & AttrNotInSymbolTable != 0L;
        }
        public static bool OnList(this Attribute a)
        {
            return a & AttrOnList != 0L;
        }
        public static bool Local(this Attribute a)
        {
            return a & AttrLocal != 0L;
        }
        public static bool ReflectMethod(this Attribute a)
        {
            return a & AttrReflectMethod != 0L;
        }
        public static bool MakeTypelink(this Attribute a)
        {
            return a & AttrMakeTypelink != 0L;
        }
        public static bool Shared(this Attribute a)
        {
            return a & AttrShared != 0L;
        }
        public static bool VisibilityHidden(this Attribute a)
        {
            return a & AttrVisibilityHidden != 0L;
        }
        public static bool SubSymbol(this Attribute a)
        {
            return a & AttrSubSymbol != 0L;
        }
        public static bool Container(this Attribute a)
        {
            return a & AttrContainer != 0L;
        }
        public static bool TopFrame(this Attribute a)
        {
            return a & AttrTopFrame != 0L;
        }
        public static bool ReadOnly(this Attribute a)
        {
            return a & AttrReadOnly != 0L;
        }
        public static bool DeferReturnTramp(this Attribute a)
        {
            return a & AttrDeferReturnTramp != 0L;
        }

        public static bool CgoExport(this Attribute a)
        {
            return a.CgoExportDynamic() || a.CgoExportStatic();
        }

        private static void Set(this ptr<Attribute> _addr_a, Attribute flag, bool value)
        {
            ref Attribute a = ref _addr_a.val;

            if (value)
            {
                a.val |= flag;
            }
            else
            {
                a.val &= flag;
            }

        }
    }
}}}}
