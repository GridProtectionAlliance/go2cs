// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 08 04:37:52 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\attribute.go
using atomic = go.sync.atomic_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
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
        public static readonly var AttrExternal = (var)0; 
        // AttrNoSplit marks functions that cannot split the stack; the linker
        // cares because it checks that there are no call chains of nosplit
        // functions that require more than StackLimit bytes (see
        // lib.go:dostkcheck)
        public static readonly var AttrNoSplit = (var)1; 
        // AttrReachable marks symbols that are transitively referenced from the
        // entry points. Unreachable symbols are not written to the output.
        public static readonly var AttrReachable = (var)2; 
        // AttrCgoExportDynamic and AttrCgoExportStatic mark symbols referenced
        // by directives written by cgo (in response to //export directives in
        // the source).
        public static readonly var AttrCgoExportDynamic = (var)3;
        public static readonly var AttrCgoExportStatic = (var)4; 
        // AttrSpecial marks symbols that do not have their address (i.e. Value)
        // computed by the usual mechanism of data.go:dodata() &
        // data.go:address().
        public static readonly var AttrSpecial = (var)5; 
        // AttrStackCheck is used by dostkcheck to only check each NoSplit
        // function's stack usage once.
        public static readonly var AttrStackCheck = (var)6; 
        // AttrNotInSymbolTable marks symbols that are not written to the symbol table.
        public static readonly var AttrNotInSymbolTable = (var)7; 
        // AttrOnList marks symbols that are on some list (such as the list of
        // all text symbols, or one of the lists of data symbols) and is
        // consulted to avoid bugs where a symbol is put on a list twice.
        public static readonly var AttrOnList = (var)8; 
        // AttrLocal marks symbols that are only visible within the module
        // (executable or shared library) being linked. Only relevant when
        // dynamically linking Go code.
        public static readonly var AttrLocal = (var)9; 
        // AttrReflectMethod marks certain methods from the reflect package that
        // can be used to call arbitrary methods. If no symbol with this bit set
        // is marked as reachable, more dead code elimination can be done.
        public static readonly var AttrReflectMethod = (var)10; 
        // AttrMakeTypelink Amarks types that should be added to the typelink
        // table. See typelinks.go:typelinks().
        public static readonly var AttrMakeTypelink = (var)11; 
        // AttrShared marks symbols compiled with the -shared option.
        public static readonly var AttrShared = (var)12; 
        // AttrVisibilityHidden symbols are ELF symbols with
        // visibility set to STV_HIDDEN. They become local symbols in
        // the final executable. Only relevant when internally linking
        // on an ELF platform.
        public static readonly var AttrVisibilityHidden = (var)13; 
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
        public static readonly var AttrSubSymbol = (var)14; 
        // AttrContainer is set on text symbols that are present as the .Outer for some
        // other symbol.
        public static readonly var AttrContainer = (var)15; 
        // AttrTopFrame means that the function is an entry point and unwinders
        // should stop when they hit this function.
        public static readonly var AttrTopFrame = (var)16; 
        // AttrReadOnly indicates whether the symbol's content (Symbol.P) is backed by
        // read-only memory.
        public static readonly var AttrReadOnly = (var)17; 
        // 19 attributes defined so far.

        private static Attribute load(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return Attribute(atomic.LoadInt32((int32.val)(a)));
        }

        private static bool DuplicateOK(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrDuplicateOK != 0L;
        }
        private static bool External(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrExternal != 0L;
        }
        private static bool NoSplit(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrNoSplit != 0L;
        }
        private static bool Reachable(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrReachable != 0L;
        }
        private static bool CgoExportDynamic(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrCgoExportDynamic != 0L;
        }
        private static bool CgoExportStatic(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrCgoExportStatic != 0L;
        }
        private static bool Special(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrSpecial != 0L;
        }
        private static bool StackCheck(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrStackCheck != 0L;
        }
        private static bool NotInSymbolTable(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrNotInSymbolTable != 0L;
        }
        private static bool OnList(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrOnList != 0L;
        }
        private static bool Local(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrLocal != 0L;
        }
        private static bool ReflectMethod(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrReflectMethod != 0L;
        }
        private static bool MakeTypelink(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrMakeTypelink != 0L;
        }
        private static bool Shared(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrShared != 0L;
        }
        private static bool VisibilityHidden(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrVisibilityHidden != 0L;
        }
        private static bool SubSymbol(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrSubSymbol != 0L;
        }
        private static bool Container(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrContainer != 0L;
        }
        private static bool TopFrame(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrTopFrame != 0L;
        }
        private static bool ReadOnly(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.load() & AttrReadOnly != 0L;
        }

        private static bool CgoExport(this ptr<Attribute> _addr_a)
        {
            ref Attribute a = ref _addr_a.val;

            return a.CgoExportDynamic() || a.CgoExportStatic();
        }

        private static void Set(this ptr<Attribute> _addr_a, Attribute flag, bool value)
        {
            ref Attribute a = ref _addr_a.val;
 
            // XXX it would be nice if we have atomic And, Or.
            while (true)
            {
                var a0 = a.load();
                Attribute anew = default;
                if (value)
                {
                    anew = a0 | flag;
                }
                else
                {
                    anew = a0 & ~flag;
                }

                if (atomic.CompareAndSwapInt32((int32.val)(a), int32(a0), int32(anew)))
                {
                    return ;
                }

            }


        }
    }
}}}}
