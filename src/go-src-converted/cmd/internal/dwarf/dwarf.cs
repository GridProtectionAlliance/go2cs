// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dwarf generates DWARF debugging information.
// DWARF generation is split between the compiler and the linker,
// this package contains the shared code.
// package dwarf -- go2cs converted at 2020 August 29 08:51:31 UTC
// import "cmd/internal/dwarf" ==> using dwarf = go.cmd.@internal.dwarf_package
// Original source: C:\Go\src\cmd\internal\dwarf\dwarf.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class dwarf_package
    {
        // InfoPrefix is the prefix for all the symbols containing DWARF info entries.
        public static readonly @string InfoPrefix = "go.info.";

        // RangePrefix is the prefix for all the symbols containing DWARF location lists.


        // RangePrefix is the prefix for all the symbols containing DWARF location lists.
        public static readonly @string LocPrefix = "go.loc.";

        // RangePrefix is the prefix for all the symbols containing DWARF range lists.


        // RangePrefix is the prefix for all the symbols containing DWARF range lists.
        public static readonly @string RangePrefix = "go.range.";

        // ConstInfoPrefix is the prefix for all symbols containing DWARF info
        // entries that contain constants.


        // ConstInfoPrefix is the prefix for all symbols containing DWARF info
        // entries that contain constants.
        public static readonly @string ConstInfoPrefix = "go.constinfo.";

        // CUInfoPrefix is the prefix for symbols containing information to
        // populate the DWARF compilation unit info entries.


        // CUInfoPrefix is the prefix for symbols containing information to
        // populate the DWARF compilation unit info entries.
        public static readonly @string CUInfoPrefix = "go.cuinfo.";

        // Used to form the symbol name assigned to the DWARF 'abstract subprogram"
        // info entry for a function


        // Used to form the symbol name assigned to the DWARF 'abstract subprogram"
        // info entry for a function
        public static readonly @string AbstractFuncSuffix = "$abstract";

        // Controls logging/debugging for selected aspects of DWARF subprogram
        // generation (functions, scopes).


        // Controls logging/debugging for selected aspects of DWARF subprogram
        // generation (functions, scopes).
        private static bool logDwarf = default;

        // Sym represents a symbol.
        public partial interface Sym
        {
            long Len();
        }

        // A Location represents a variable's location at a particular PC range.
        // It becomes a location list entry in the DWARF.
        public partial struct Location
        {
            public long StartPC;
            public long EndPC;
            public slice<Piece> Pieces;
        }

        // A Piece represents the location of a particular part of a variable.
        // It becomes part of a location list entry (a DW_OP_piece) in the DWARF.
        public partial struct Piece
        {
            public long Length;
            public int StackOffset;
            public short RegNum;
            public bool Missing;
            public bool OnStack; // if true, RegNum is unset.
        }

        // A Var represents a local variable or a function parameter.
        public partial struct Var
        {
            public @string Name;
            public long Abbrev; // Either DW_ABRV_AUTO[_LOCLIST] or DW_ABRV_PARAM[_LOCLIST]
            public bool IsReturnValue;
            public bool IsInlFormal;
            public int StackOffset;
            public slice<Location> LocationList;
            public int Scope;
            public Sym Type;
            public @string DeclFile;
            public ulong DeclLine;
            public ulong DeclCol;
            public int InlIndex; // subtract 1 to form real index into InlTree
            public int ChildIndex; // child DIE index in abstract function
            public bool IsInAbstract; // variable exists in abstract function
        }

        // A Scope represents a lexical scope. All variables declared within a
        // scope will only be visible to instructions covered by the scope.
        // Lexical scopes are contiguous in source files but can end up being
        // compiled to discontiguous blocks of instructions in the executable.
        // The Ranges field lists all the blocks of instructions that belong
        // in this scope.
        public partial struct Scope
        {
            public int Parent;
            public slice<Range> Ranges;
            public slice<ref Var> Vars;
        }

        // A Range represents a half-open interval [Start, End).
        public partial struct Range
        {
            public long Start;
            public long End;
        }

        // This container is used by the PutFunc* variants below when
        // creating the DWARF subprogram DIE(s) for a function.
        public partial struct FnState
        {
            public @string Name;
            public @string Importpath;
            public Sym Info;
            public Sym Filesym;
            public Sym Loc;
            public Sym Ranges;
            public Sym Absfn;
            public Sym StartPC;
            public long Size;
            public bool External;
            public slice<Scope> Scopes;
            public InlCalls InlCalls;
        }

        public static void EnableLogging(bool doit)
        {
            logDwarf = doit;
        }

        // UnifyRanges merges the list of ranges of c into the list of ranges of s
        private static void UnifyRanges(this ref Scope s, ref Scope c)
        {
            var @out = make_slice<Range>(0L, len(s.Ranges) + len(c.Ranges));

            long i = 0L;
            long j = 0L;
            while (true)
            {
                Range cur = default;
                if (i < len(s.Ranges) && j < len(c.Ranges))
                {
                    if (s.Ranges[i].Start < c.Ranges[j].Start)
                    {
                        cur = s.Ranges[i];
                        i++;
                    }
                    else
                    {
                        cur = c.Ranges[j];
                        j++;
                    }
                }
                else if (i < len(s.Ranges))
                {
                    cur = s.Ranges[i];
                    i++;
                }
                else if (j < len(c.Ranges))
                {
                    cur = c.Ranges[j];
                    j++;
                }
                else
                {
                    break;
                }
                {
                    var n = len(out);

                    if (n > 0L && cur.Start <= out[n - 1L].End)
                    {
                        out[n - 1L].End = cur.End;
                    }
                    else
                    {
                        out = append(out, cur);
                    }

                }
            }


            s.Ranges = out;
        }

        public partial struct InlCalls
        {
            public slice<InlCall> Calls;
        }

        public partial struct InlCall
        {
            public long InlIndex; // Symbol of file containing inlined call site (really *obj.LSym).
            public Sym CallFile; // Line number of inlined call site.
            public uint CallLine; // Dwarf abstract subroutine symbol (really *obj.LSym).
            public Sym AbsFunSym; // Indices of child inlines within Calls array above.
            public slice<long> Children; // entries in this list are PAUTO's created by the inliner to
// capture the promoted formals and locals of the inlined callee.
            public slice<ref Var> InlVars; // PC ranges for this inlined call.
            public slice<Range> Ranges; // Root call (not a child of some other call).
            public bool Root;
        }

        // A Context specifies how to add data to a Sym.
        public partial interface Context
        {
            void PtrSize();
            void AddInt(Sym s, long size, long i);
            void AddBytes(Sym s, slice<byte> b);
            void AddAddress(Sym s, object t, long ofs);
            void AddCURelativeAddress(Sym s, object t, long ofs);
            void AddSectionOffset(Sym s, long size, object t, long ofs);
            void CurrentOffset(Sym s);
            void RecordDclReference(Sym from, Sym to, long dclIdx, long inlIndex);
            void RecordChildDieOffsets(Sym s, slice<ref Var> vars, slice<int> offsets);
            void AddString(Sym s, @string v);
            void AddFileRef(Sym s, object f);
            void Logf(@string format, params object[] args);
        }

        // AppendUleb128 appends v to b using DWARF's unsigned LEB128 encoding.
        public static slice<byte> AppendUleb128(slice<byte> b, ulong v)
        {
            while (true)
            {
                var c = uint8(v & 0x7fUL);
                v >>= 7L;
                if (v != 0L)
                {
                    c |= 0x80UL;
                }
                b = append(b, c);
                if (c & 0x80UL == 0L)
                {
                    break;
                }
            }

            return b;
        }

        // AppendSleb128 appends v to b using DWARF's signed LEB128 encoding.
        public static slice<byte> AppendSleb128(slice<byte> b, long v)
        {
            while (true)
            {
                var c = uint8(v & 0x7fUL);
                var s = uint8(v & 0x40UL);
                v >>= 7L;
                if ((v != -1L || s == 0L) && (v != 0L || s != 0L))
                {
                    c |= 0x80UL;
                }
                b = append(b, c);
                if (c & 0x80UL == 0L)
                {
                    break;
                }
            }

            return b;
        }

        // sevenbits contains all unsigned seven bit numbers, indexed by their value.
        private static array<byte> sevenbits = new array<byte>(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f });

        // sevenBitU returns the unsigned LEB128 encoding of v if v is seven bits and nil otherwise.
        // The contents of the returned slice must not be modified.
        private static slice<byte> sevenBitU(long v)
        {
            if (uint64(v) < uint64(len(sevenbits)))
            {
                return sevenbits[v..v + 1L];
            }
            return null;
        }

        // sevenBitS returns the signed LEB128 encoding of v if v is seven bits and nil otherwise.
        // The contents of the returned slice must not be modified.
        private static slice<byte> sevenBitS(long v)
        {
            if (uint64(v) <= 63L)
            {
                return sevenbits[v..v + 1L];
            }
            if (uint64(-v) <= 64L)
            {
                return sevenbits[128L + v..128L + v + 1L];
            }
            return null;
        }

        // Uleb128put appends v to s using DWARF's unsigned LEB128 encoding.
        public static void Uleb128put(Context ctxt, Sym s, long v)
        {
            var b = sevenBitU(v);
            if (b == null)
            {
                array<byte> encbuf = new array<byte>(20L);
                b = AppendUleb128(encbuf[..0L], uint64(v));
            }
            ctxt.AddBytes(s, b);
        }

        // Sleb128put appends v to s using DWARF's signed LEB128 encoding.
        public static void Sleb128put(Context ctxt, Sym s, long v)
        {
            var b = sevenBitS(v);
            if (b == null)
            {
                array<byte> encbuf = new array<byte>(20L);
                b = AppendSleb128(encbuf[..0L], v);
            }
            ctxt.AddBytes(s, b);
        }

        /*
         * Defining Abbrevs.  This is hardcoded, and there will be
         * only a handful of them.  The DWARF spec places no restriction on
         * the ordering of attributes in the Abbrevs and DIEs, and we will
         * always write them out in the order of declaration in the abbrev.
         */
        private partial struct dwAttrForm
        {
            public ushort attr;
            public byte form;
        }

        // Go-specific type attributes.
        public static readonly ulong DW_AT_go_kind = 0x2900UL;
        public static readonly ulong DW_AT_go_key = 0x2901UL;
        public static readonly ulong DW_AT_go_elem = 0x2902UL; 
        // Attribute for DW_TAG_member of a struct type.
        // Nonzero value indicates the struct field is an embedded field.
        public static readonly ulong DW_AT_go_embedded_field = 0x2903UL;

        public static readonly long DW_AT_internal_location = 253L; // params and locals; not emitted

        // Index into the abbrevs table below.
        // Keep in sync with ispubname() and ispubtype() in ld/dwarf.go.
        // ispubtype considers >= NULLTYPE public
        public static readonly var DW_ABRV_NULL = iota;
        public static readonly var DW_ABRV_COMPUNIT = 0;
        public static readonly var DW_ABRV_FUNCTION = 1;
        public static readonly var DW_ABRV_FUNCTION_ABSTRACT = 2;
        public static readonly var DW_ABRV_FUNCTION_CONCRETE = 3;
        public static readonly var DW_ABRV_INLINED_SUBROUTINE = 4;
        public static readonly var DW_ABRV_INLINED_SUBROUTINE_RANGES = 5;
        public static readonly var DW_ABRV_VARIABLE = 6;
        public static readonly var DW_ABRV_INT_CONSTANT = 7;
        public static readonly var DW_ABRV_AUTO = 8;
        public static readonly var DW_ABRV_AUTO_LOCLIST = 9;
        public static readonly var DW_ABRV_AUTO_ABSTRACT = 10;
        public static readonly var DW_ABRV_AUTO_CONCRETE = 11;
        public static readonly var DW_ABRV_AUTO_CONCRETE_LOCLIST = 12;
        public static readonly var DW_ABRV_PARAM = 13;
        public static readonly var DW_ABRV_PARAM_LOCLIST = 14;
        public static readonly var DW_ABRV_PARAM_ABSTRACT = 15;
        public static readonly var DW_ABRV_PARAM_CONCRETE = 16;
        public static readonly var DW_ABRV_PARAM_CONCRETE_LOCLIST = 17;
        public static readonly var DW_ABRV_LEXICAL_BLOCK_RANGES = 18;
        public static readonly var DW_ABRV_LEXICAL_BLOCK_SIMPLE = 19;
        public static readonly var DW_ABRV_STRUCTFIELD = 20;
        public static readonly var DW_ABRV_FUNCTYPEPARAM = 21;
        public static readonly var DW_ABRV_DOTDOTDOT = 22;
        public static readonly var DW_ABRV_ARRAYRANGE = 23;
        public static readonly var DW_ABRV_NULLTYPE = 24;
        public static readonly var DW_ABRV_BASETYPE = 25;
        public static readonly var DW_ABRV_ARRAYTYPE = 26;
        public static readonly var DW_ABRV_CHANTYPE = 27;
        public static readonly var DW_ABRV_FUNCTYPE = 28;
        public static readonly var DW_ABRV_IFACETYPE = 29;
        public static readonly var DW_ABRV_MAPTYPE = 30;
        public static readonly var DW_ABRV_PTRTYPE = 31;
        public static readonly var DW_ABRV_BARE_PTRTYPE = 32; // only for void*, no DW_AT_type attr to please gdb 6.
        public static readonly var DW_ABRV_SLICETYPE = 33;
        public static readonly var DW_ABRV_STRINGTYPE = 34;
        public static readonly var DW_ABRV_STRUCTTYPE = 35;
        public static readonly var DW_ABRV_TYPEDECL = 36;
        public static readonly var DW_NABRV = 37;

        private partial struct dwAbbrev
        {
            public byte tag;
            public byte children;
            public slice<dwAttrForm> attr;
        }

        private static array<dwAbbrev> abbrevs = new array<dwAbbrev>(new dwAbbrev[] { {0,0,[]dwAttrForm{}}, {DW_TAG_compile_unit,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_language,DW_FORM_data1},{DW_AT_stmt_list,DW_FORM_sec_offset},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_ranges,DW_FORM_sec_offset},{DW_AT_comp_dir,DW_FORM_string},{DW_AT_producer,DW_FORM_string},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_frame_base,DW_FORM_block1},{DW_AT_decl_file,DW_FORM_data4},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_inline,DW_FORM_data1},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_frame_base,DW_FORM_block1},},}, {DW_TAG_inlined_subroutine,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_call_file,DW_FORM_data4},{DW_AT_call_line,DW_FORM_udata},},}, {DW_TAG_inlined_subroutine,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_ranges,DW_FORM_sec_offset},{DW_AT_call_file,DW_FORM_data4},{DW_AT_call_line,DW_FORM_udata},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_location,DW_FORM_block1},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_constant,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_const_value,DW_FORM_sdata},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_lexical_block,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_ranges,DW_FORM_sec_offset},},}, {DW_TAG_lexical_block,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},},}, {DW_TAG_member,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_data_member_location,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_embedded_field,DW_FORM_flag},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_unspecified_parameters,DW_CHILDREN_no,[]dwAttrForm{},}, {DW_TAG_subrange_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_type,DW_FORM_ref_addr},{DW_AT_count,DW_FORM_udata},},}, {DW_TAG_unspecified_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},},}, {DW_TAG_base_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_encoding,DW_FORM_data1},{DW_AT_byte_size,DW_FORM_data1},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_array_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_subroutine_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_typedef,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_key,DW_FORM_ref_addr},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_pointer_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_pointer_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},},} });

        // GetAbbrev returns the contents of the .debug_abbrev section.
        public static slice<byte> GetAbbrev()
        {
            slice<byte> buf = default;
            for (long i = 1L; i < DW_NABRV; i++)
            { 
                // See section 7.5.3
                buf = AppendUleb128(buf, uint64(i));
                buf = AppendUleb128(buf, uint64(abbrevs[i].tag));
                buf = append(buf, byte(abbrevs[i].children));
                foreach (var (_, f) in abbrevs[i].attr)
                {
                    buf = AppendUleb128(buf, uint64(f.attr));
                    buf = AppendUleb128(buf, uint64(f.form));
                }
                buf = append(buf, 0L, 0L);
            }

            return append(buf, 0L);
        }

        /*
         * Debugging Information Entries and their attributes.
         */

        // DWAttr represents an attribute of a DWDie.
        //
        // For DW_CLS_string and _block, value should contain the length, and
        // data the data, for _reference, value is 0 and data is a DWDie* to
        // the referenced instance, for all others, value is the whole thing
        // and data is null.
        public partial struct DWAttr
        {
            public ptr<DWAttr> Link;
            public ushort Atr; // DW_AT_
            public byte Cls; // DW_CLS_
            public long Value;
        }

        // DWDie represents a DWARF debug info entry.
        public partial struct DWDie
        {
            public long Abbrev;
            public ptr<DWDie> Link;
            public ptr<DWDie> Child;
            public ptr<DWAttr> Attr;
            public Sym Sym;
        }

        private static error putattr(Context ctxt, Sym s, long abbrev, long form, long cls, long value, object data)
        {

            if (form == DW_FORM_addr) // address
            {
                ctxt.AddAddress(s, data, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_block1) // block
            {
                if (cls == DW_CLS_ADDRESS)
                {
                    ctxt.AddInt(s, 1L, int64(1L + ctxt.PtrSize()));
                    ctxt.AddInt(s, 1L, DW_OP_addr);
                    ctxt.AddAddress(s, data, 0L);
                    break;
                }
                value &= 0xffUL;
                ctxt.AddInt(s, 1L, value);
                slice<byte> p = data._<slice<byte>>()[..value];
                ctxt.AddBytes(s, p);
                goto __switch_break0;
            }
            if (form == DW_FORM_block2) // block
            {
                value &= 0xffffUL;

                ctxt.AddInt(s, 2L, value);
                p = data._<slice<byte>>()[..value];
                ctxt.AddBytes(s, p);
                goto __switch_break0;
            }
            if (form == DW_FORM_block4) // block
            {
                value &= 0xffffffffUL;

                ctxt.AddInt(s, 4L, value);
                p = data._<slice<byte>>()[..value];
                ctxt.AddBytes(s, p);
                goto __switch_break0;
            }
            if (form == DW_FORM_block) // block
            {
                Uleb128put(ctxt, s, value);

                p = data._<slice<byte>>()[..value];
                ctxt.AddBytes(s, p);
                goto __switch_break0;
            }
            if (form == DW_FORM_data1) // constant
            {
                ctxt.AddInt(s, 1L, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_data2) // constant
            {
                ctxt.AddInt(s, 2L, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_data4) // constant, {line,loclist,mac,rangelist}ptr
            {
                if (cls == DW_CLS_PTR)
                { // DW_AT_stmt_list and DW_AT_ranges
                    ctxt.AddSectionOffset(s, 4L, data, value);
                    break;
                }
                ctxt.AddInt(s, 4L, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_data8) // constant, {line,loclist,mac,rangelist}ptr
            {
                ctxt.AddInt(s, 8L, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_sdata) // constant
            {
                Sleb128put(ctxt, s, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_udata) // constant
            {
                Uleb128put(ctxt, s, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_string) // string
            {
                @string str = data._<@string>();
                ctxt.AddString(s, str); 
                // TODO(ribrdb): verify padded strings are never used and remove this
                for (var i = int64(len(str)); i < value; i++)
                {
                    ctxt.AddInt(s, 1L, 0L);
                }

                goto __switch_break0;
            }
            if (form == DW_FORM_flag) // flag
            {
                if (value != 0L)
                {
                    ctxt.AddInt(s, 1L, 1L);
                }
                else
                {
                    ctxt.AddInt(s, 1L, 0L);
                } 

                // As of DWARF 3 the ref_addr is always 32 bits, unless emitting a large
                // (> 4 GB of debug info aka "64-bit") unit, which we don't implement.
                goto __switch_break0;
            }
            if (form == DW_FORM_ref_addr) // reference to a DIE in the .info section
            {
                fallthrough = true;
            }
            if (fallthrough || form == DW_FORM_sec_offset) // offset into a DWARF section other than .info
            {
                if (data == null)
                {
                    return error.As(fmt.Errorf("dwarf: null reference in %d", abbrev));
                }
                ctxt.AddSectionOffset(s, 4L, data, value);
                goto __switch_break0;
            }
            if (form == DW_FORM_ref1 || form == DW_FORM_ref2 || form == DW_FORM_ref4 || form == DW_FORM_ref8 || form == DW_FORM_ref_udata || form == DW_FORM_strp || form == DW_FORM_indirect) // (see Section 7.5.3)
            {
            }
            // default: 
                return error.As(fmt.Errorf("dwarf: unsupported attribute form %d / class %d", form, cls));

            __switch_break0:;
            return error.As(null);
        }

        // PutAttrs writes the attributes for a DIE to symbol 's'.
        //
        // Note that we can (and do) add arbitrary attributes to a DIE, but
        // only the ones actually listed in the Abbrev will be written out.
        public static void PutAttrs(Context ctxt, Sym s, long abbrev, ref DWAttr attr)
        {
Outer:
            foreach (var (_, f) in abbrevs[abbrev].attr)
            {
                {
                    var ap = attr;

                    while (ap != null)
                    {
                        if (ap.Atr == f.attr)
                        {
                            putattr(ctxt, s, abbrev, int(f.form), int(ap.Cls), ap.Value, ap.Data);
                            _continueOuter = true;
                            break;
                        ap = ap.Link;
                        }
                    }

                }

                putattr(ctxt, s, abbrev, int(f.form), 0L, 0L, null);
            }
        }

        // HasChildren returns true if 'die' uses an abbrev that supports children.
        public static bool HasChildren(ref DWDie die)
        {
            return abbrevs[die.Abbrev].children != 0L;
        }

        // PutIntConst writes a DIE for an integer constant
        public static void PutIntConst(Context ctxt, Sym info, Sym typ, @string name, long val)
        {
            Uleb128put(ctxt, info, DW_ABRV_INT_CONSTANT);
            putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_string, DW_CLS_STRING, int64(len(name)), name);
            putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, typ);
            putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_sdata, DW_CLS_CONSTANT, val, null);
        }

        // PutRanges writes a range table to sym. All addresses in ranges are
        // relative to some base address. If base is not nil, then they're
        // relative to the start of base. If base is nil, then the caller must
        // arrange a base address some other way (such as a DW_AT_low_pc
        // attribute).
        public static void PutRanges(Context ctxt, Sym sym, Sym @base, slice<Range> ranges)
        {
            var ps = ctxt.PtrSize(); 
            // Write ranges.
            // We do not emit base address entries here, even though they would reduce
            // the number of relocations, because dsymutil (which is used on macOS when
            // linking externally) does not support them.
            foreach (var (_, r) in ranges)
            {
                if (base == null)
                {
                    ctxt.AddInt(sym, ps, r.Start);
                    ctxt.AddInt(sym, ps, r.End);
                }
                else
                {
                    ctxt.AddCURelativeAddress(sym, base, r.Start);
                    ctxt.AddCURelativeAddress(sym, base, r.End);
                }
            } 
            // Write trailer.
            ctxt.AddInt(sym, ps, 0L);
            ctxt.AddInt(sym, ps, 0L);
        }

        // Return TRUE if the inlined call in the specified slot is empty,
        // meaning it has a zero-length range (no instructions), and all
        // of its children are empty.
        private static bool isEmptyInlinedCall(long slot, ref InlCalls calls)
        {
            var ic = ref calls.Calls[slot];
            if (ic.InlIndex == -2L)
            {
                return true;
            }
            var live = false;
            foreach (var (_, k) in ic.Children)
            {
                if (!isEmptyInlinedCall(k, calls))
                {
                    live = true;
                }
            }
            if (len(ic.Ranges) > 0L)
            {
                live = true;
            }
            if (!live)
            {
                ic.InlIndex = -2L;
            }
            return !live;
        }

        // Slot -1:    return top-level inlines
        // Slot >= 0:  return children of that slot
        private static slice<long> inlChildren(long slot, ref InlCalls calls)
        {
            slice<long> kids = default;
            if (slot != -1L)
            {
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in calls.Calls[slot].Children)
                    {
                        k = __k;
                        if (!isEmptyInlinedCall(k, calls))
                        {
                            kids = append(kids, k);
                        }
                    }
            else

                    k = k__prev1;
                }

            }            {
                {
                    var k__prev1 = k;

                    long k = 0L;

                    while (k < len(calls.Calls))
                    {
                        if (calls.Calls[k].Root && !isEmptyInlinedCall(k, calls))
                        {
                            kids = append(kids, k);
                        k += 1L;
                        }
                    }


                    k = k__prev1;
                }
            }
            return kids;
        }

        private static map<ref Var, bool> inlinedVarTable(ref InlCalls inlcalls)
        {
            var vars = make_map<ref Var, bool>();
            foreach (var (_, ic) in inlcalls.Calls)
            {
                foreach (var (_, v) in ic.InlVars)
                {
                    vars[v] = true;
                }
            }
            return vars;
        }

        // The s.Scopes slice contains variables were originally part of the
        // function being emitted, as well as variables that were imported
        // from various callee functions during the inlining process. This
        // function prunes out any variables from the latter category (since
        // they will be emitted as part of DWARF inlined_subroutine DIEs) and
        // then generates scopes for vars in the former category.
        private static error putPrunedScopes(Context ctxt, ref FnState s, long fnabbrev)
        {
            if (len(s.Scopes) == 0L)
            {
                return error.As(null);
            }
            var scopes = make_slice<Scope>(len(s.Scopes), len(s.Scopes));
            var pvars = inlinedVarTable(ref s.InlCalls);
            foreach (var (k, s) in s.Scopes)
            {
                Scope pruned = new Scope(Parent:s.Parent,Ranges:s.Ranges);
                for (long i = 0L; i < len(s.Vars); i++)
                {
                    var (_, found) = pvars[s.Vars[i]];
                    if (!found)
                    {
                        pruned.Vars = append(pruned.Vars, s.Vars[i]);
                    }
                }

                sort.Sort(byChildIndex(pruned.Vars));
                scopes[k] = pruned;
            }
            array<byte> encbuf = new array<byte>(20L);
            if (putscope(ctxt, s, scopes, 0L, fnabbrev, encbuf[..0L]) < int32(len(scopes)))
            {
                return error.As(errors.New("multiple toplevel scopes"));
            }
            return error.As(null);
        }

        // Emit DWARF attributes and child DIEs for an 'abstract' subprogram.
        // The abstract subprogram DIE for a function contains its
        // location-independent attributes (name, type, etc). Other instances
        // of the function (any inlined copy of it, or the single out-of-line
        // 'concrete' instance) will contain a pointer back to this abstract
        // DIE (as a space-saving measure, so that name/type etc doesn't have
        // to be repeated for each inlined copy).
        public static error PutAbstractFunc(Context ctxt, ref FnState s)
        {
            if (logDwarf)
            {
                ctxt.Logf("PutAbstractFunc(%v)\n", s.Absfn);
            }
            var abbrev = DW_ABRV_FUNCTION_ABSTRACT;
            Uleb128put(ctxt, s.Absfn, int64(abbrev));

            var fullname = s.Name;
            if (strings.HasPrefix(s.Name, "\"\"."))
            { 
                // Generate a fully qualified name for the function in the
                // abstract case. This is so as to avoid the need for the
                // linker to process the DIE with patchDWARFName(); we can't
                // allow the name attribute of an abstract subprogram DIE to
                // be rewritten, since it would change the offsets of the
                // child DIEs (which we're relying on in order for abstract
                // origin references to work).
                fullname = s.Importpath + "." + s.Name[3L..];
            }
            putattr(ctxt, s.Absfn, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(fullname)), fullname); 

            // DW_AT_inlined value
            putattr(ctxt, s.Absfn, abbrev, DW_FORM_data1, DW_CLS_CONSTANT, int64(DW_INL_inlined), null);

            long ev = default;
            if (s.External)
            {
                ev = 1L;
            }
            putattr(ctxt, s.Absfn, abbrev, DW_FORM_flag, DW_CLS_FLAG, ev, 0L); 

            // Child variables (may be empty)
            slice<ref Var> flattened = default; 

            // This slice will hold the offset in bytes for each child var DIE
            // with respect to the start of the parent subprogram DIE.
            slice<int> offsets = default; 

            // Scopes/vars
            if (len(s.Scopes) > 0L)
            { 
                // For abstract subprogram DIEs we want to flatten out scope info:
                // lexical scope DIEs contain range and/or hi/lo PC attributes,
                // which we explicitly don't want for the abstract subprogram DIE.
                var pvars = inlinedVarTable(ref s.InlCalls);
                foreach (var (_, scope) in s.Scopes)
                {
                    {
                        long i__prev2 = i;

                        for (long i = 0L; i < len(scope.Vars); i++)
                        {
                            var (_, found) = pvars[scope.Vars[i]];
                            if (found || !scope.Vars[i].IsInAbstract)
                            {
                                continue;
                            }
                            flattened = append(flattened, scope.Vars[i]);
                        }


                        i = i__prev2;
                    }
                }
                if (len(flattened) > 0L)
                {
                    sort.Sort(byChildIndex(flattened));

                    if (logDwarf)
                    {
                        ctxt.Logf("putAbstractScope(%v): vars:", s.Info);
                        {
                            long i__prev1 = i;
                            var v__prev1 = v;

                            foreach (var (__i, __v) in flattened)
                            {
                                i = __i;
                                v = __v;
                                ctxt.Logf(" %d:%s", i, v.Name);
                            }

                            i = i__prev1;
                            v = v__prev1;
                        }

                        ctxt.Logf("\n");
                    } 

                    // This slice will hold the offset in bytes for each child
                    // variable DIE with respect to the start of the parent
                    // subprogram DIE.
                    {
                        var v__prev1 = v;

                        foreach (var (_, __v) in flattened)
                        {
                            v = __v;
                            offsets = append(offsets, int32(ctxt.CurrentOffset(s.Absfn)));
                            putAbstractVar(ctxt, s.Absfn, v);
                        }

                        v = v__prev1;
                    }

                }
            }
            ctxt.RecordChildDieOffsets(s.Absfn, flattened, offsets);

            Uleb128put(ctxt, s.Absfn, 0L);
            return error.As(null);
        }

        // Emit DWARF attributes and child DIEs for an inlined subroutine. The
        // first attribute of an inlined subroutine DIE is a reference back to
        // its corresponding 'abstract' DIE (containing location-independent
        // attributes such as name, type, etc). Inlined subroutine DIEs can
        // have other inlined subroutine DIEs as children.
        public static error PutInlinedFunc(Context ctxt, ref FnState s, Sym callersym, long callIdx)
        {
            var ic = s.InlCalls.Calls[callIdx];
            var callee = ic.AbsFunSym;

            var abbrev = DW_ABRV_INLINED_SUBROUTINE_RANGES;
            if (len(ic.Ranges) == 1L)
            {
                abbrev = DW_ABRV_INLINED_SUBROUTINE;
            }
            Uleb128put(ctxt, s.Info, int64(abbrev));

            if (logDwarf)
            {
                ctxt.Logf("PutInlinedFunc(caller=%v,callee=%v,abbrev=%d)\n", callersym, callee, abbrev);
            } 

            // Abstract origin.
            putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, callee);

            if (abbrev == DW_ABRV_INLINED_SUBROUTINE_RANGES)
            {
                putattr(ctxt, s.Info, abbrev, DW_FORM_sec_offset, DW_CLS_PTR, s.Ranges.Len(), s.Ranges);
                PutRanges(ctxt, s.Ranges, s.StartPC, ic.Ranges);
            }
            else
            {
                var st = ic.Ranges[0L].Start;
                var en = ic.Ranges[0L].End;
                putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, st, s.StartPC);
                putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, en, s.StartPC);
            } 

            // Emit call file, line attrs.
            ctxt.AddFileRef(s.Info, ic.CallFile);
            putattr(ctxt, s.Info, abbrev, DW_FORM_udata, DW_CLS_CONSTANT, int64(ic.CallLine), null); 

            // Variables associated with this inlined routine instance.
            var vars = ic.InlVars;
            sort.Sort(byChildIndex(vars));
            var inlIndex = ic.InlIndex;
            array<byte> encbuf = new array<byte>(20L);
            foreach (var (_, v) in vars)
            {
                if (!v.IsInAbstract)
                {
                    continue;
                }
                putvar(ctxt, s, v, callee, abbrev, inlIndex, encbuf[..0L]);
            } 

            // Children of this inline.
            foreach (var (_, sib) in inlChildren(callIdx, ref s.InlCalls))
            {
                var absfn = s.InlCalls.Calls[sib].AbsFunSym;
                var err = PutInlinedFunc(ctxt, s, absfn, sib);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            Uleb128put(ctxt, s.Info, 0L);
            return error.As(null);
        }

        // Emit DWARF attributes and child DIEs for a 'concrete' subprogram,
        // meaning the out-of-line copy of a function that was inlined at some
        // point during the compilation of its containing package. The first
        // attribute for a concrete DIE is a reference to the 'abstract' DIE
        // for the function (which holds location-independent attributes such
        // as name, type), then the remainder of the attributes are specific
        // to this instance (location, frame base, etc).
        public static error PutConcreteFunc(Context ctxt, ref FnState s)
        {
            if (logDwarf)
            {
                ctxt.Logf("PutConcreteFunc(%v)\n", s.Info);
            }
            var abbrev = DW_ABRV_FUNCTION_CONCRETE;
            Uleb128put(ctxt, s.Info, int64(abbrev)); 

            // Abstract origin.
            putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, s.Absfn); 

            // Start/end PC.
            putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, 0L, s.StartPC);
            putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, s.Size, s.StartPC); 

            // cfa / frame base
            putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, 1L, new slice<byte>(new byte[] { DW_OP_call_frame_cfa })); 

            // Scopes
            {
                var err__prev1 = err;

                var err = putPrunedScopes(ctxt, s, abbrev);

                if (err != null)
                {
                    return error.As(err);
                } 

                // Inlined subroutines.

                err = err__prev1;

            } 

            // Inlined subroutines.
            foreach (var (_, sib) in inlChildren(-1L, ref s.InlCalls))
            {
                var absfn = s.InlCalls.Calls[sib].AbsFunSym;
                err = PutInlinedFunc(ctxt, s, absfn, sib);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            Uleb128put(ctxt, s.Info, 0L);
            return error.As(null);
        }

        // Emit DWARF attributes and child DIEs for a subprogram. Here
        // 'default' implies that the function in question was not inlined
        // when its containing package was compiled (hence there is no need to
        // emit an abstract version for it to use as a base for inlined
        // routine records).
        public static error PutDefaultFunc(Context ctxt, ref FnState s)
        {
            if (logDwarf)
            {
                ctxt.Logf("PutDefaultFunc(%v)\n", s.Info);
            }
            var abbrev = DW_ABRV_FUNCTION;
            Uleb128put(ctxt, s.Info, int64(abbrev));

            putattr(ctxt, s.Info, DW_ABRV_FUNCTION, DW_FORM_string, DW_CLS_STRING, int64(len(s.Name)), s.Name);
            putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, 0L, s.StartPC);
            putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, s.Size, s.StartPC);
            putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, 1L, new slice<byte>(new byte[] { DW_OP_call_frame_cfa }));
            ctxt.AddFileRef(s.Info, s.Filesym);

            long ev = default;
            if (s.External)
            {
                ev = 1L;
            }
            putattr(ctxt, s.Info, abbrev, DW_FORM_flag, DW_CLS_FLAG, ev, 0L); 

            // Scopes
            {
                var err__prev1 = err;

                var err = putPrunedScopes(ctxt, s, abbrev);

                if (err != null)
                {
                    return error.As(err);
                } 

                // Inlined subroutines.

                err = err__prev1;

            } 

            // Inlined subroutines.
            foreach (var (_, sib) in inlChildren(-1L, ref s.InlCalls))
            {
                var absfn = s.InlCalls.Calls[sib].AbsFunSym;
                err = PutInlinedFunc(ctxt, s, absfn, sib);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            Uleb128put(ctxt, s.Info, 0L);
            return error.As(null);
        }

        private static int putscope(Context ctxt, ref FnState s, slice<Scope> scopes, int curscope, long fnabbrev, slice<byte> encbuf)
        {
            if (logDwarf)
            {
                ctxt.Logf("putscope(%v,%d): vars:", s.Info, curscope);
                {
                    var v__prev1 = v;

                    foreach (var (__i, __v) in scopes[curscope].Vars)
                    {
                        i = __i;
                        v = __v;
                        ctxt.Logf(" %d:%d:%s", i, v.ChildIndex, v.Name);
                    }

                    v = v__prev1;
                }

                ctxt.Logf("\n");
            }
            {
                var v__prev1 = v;

                foreach (var (_, __v) in scopes[curscope].Vars)
                {
                    v = __v;
                    putvar(ctxt, s, v, s.Absfn, fnabbrev, -1L, encbuf);
                }

                v = v__prev1;
            }

            var @this = curscope;
            curscope++;
            while (curscope < int32(len(scopes)))
            {
                var scope = scopes[curscope];
                if (scope.Parent != this)
                {
                    return curscope;
                }
                if (len(scope.Ranges) == 1L)
                {
                    Uleb128put(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE);
                    putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE, DW_FORM_addr, DW_CLS_ADDRESS, scope.Ranges[0L].Start, s.StartPC);
                    putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE, DW_FORM_addr, DW_CLS_ADDRESS, scope.Ranges[0L].End, s.StartPC);
                }
                else
                {
                    Uleb128put(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_RANGES);
                    putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_RANGES, DW_FORM_sec_offset, DW_CLS_PTR, s.Ranges.Len(), s.Ranges);

                    PutRanges(ctxt, s.Ranges, s.StartPC, scope.Ranges);
                }
                curscope = putscope(ctxt, s, scopes, curscope, fnabbrev, encbuf);
                Uleb128put(ctxt, s.Info, 0L);
            }

            return curscope;
        }

        // Given a default var abbrev code, select corresponding concrete code.
        private static long concreteVarAbbrev(long varAbbrev) => func((_, panic, __) =>
        {

            if (varAbbrev == DW_ABRV_AUTO) 
                return DW_ABRV_AUTO_CONCRETE;
            else if (varAbbrev == DW_ABRV_PARAM) 
                return DW_ABRV_PARAM_CONCRETE;
            else if (varAbbrev == DW_ABRV_AUTO_LOCLIST) 
                return DW_ABRV_AUTO_CONCRETE_LOCLIST;
            else if (varAbbrev == DW_ABRV_PARAM_LOCLIST) 
                return DW_ABRV_PARAM_CONCRETE_LOCLIST;
            else 
                panic("should never happen");
                    });

        // Pick the correct abbrev code for variable or parameter DIE.
        private static (long, bool, bool) determineVarAbbrev(ref Var _v, long fnabbrev) => func(_v, (ref Var v, Defer _, Panic panic, Recover __) =>
        {
            var abbrev = v.Abbrev; 

            // If the variable was entirely optimized out, don't emit a location list;
            // convert to an inline abbreviation and emit an empty location.
            var missing = false;

            if (abbrev == DW_ABRV_AUTO_LOCLIST && len(v.LocationList) == 0L) 
                missing = true;
                abbrev = DW_ABRV_AUTO;
            else if (abbrev == DW_ABRV_PARAM_LOCLIST && len(v.LocationList) == 0L) 
                missing = true;
                abbrev = DW_ABRV_PARAM;
            // Determine whether to use a concrete variable or regular variable DIE.
            var concrete = true;

            if (fnabbrev == DW_ABRV_FUNCTION) 
                concrete = false;
                break;
            else if (fnabbrev == DW_ABRV_FUNCTION_CONCRETE) 
                // If we're emitting a concrete subprogram DIE and the variable
                // in question is not part of the corresponding abstract function DIE,
                // then use the default (non-concrete) abbrev for this param.
                if (!v.IsInAbstract)
                {
                    concrete = false;
                }
            else if (fnabbrev == DW_ABRV_INLINED_SUBROUTINE || fnabbrev == DW_ABRV_INLINED_SUBROUTINE_RANGES)             else 
                panic("should never happen");
            // Select proper abbrev based on concrete/non-concrete
            if (concrete)
            {
                abbrev = concreteVarAbbrev(abbrev);
            }
            return (abbrev, missing, concrete);
        });

        private static bool abbrevUsesLoclist(long abbrev)
        {

            if (abbrev == DW_ABRV_AUTO_LOCLIST || abbrev == DW_ABRV_AUTO_CONCRETE_LOCLIST || abbrev == DW_ABRV_PARAM_LOCLIST || abbrev == DW_ABRV_PARAM_CONCRETE_LOCLIST) 
                return true;
            else 
                return false;
                    }

        // Emit DWARF attributes for a variable belonging to an 'abstract' subprogram.
        private static void putAbstractVar(Context ctxt, Sym info, ref Var v)
        { 
            // Remap abbrev
            var abbrev = v.Abbrev;

            if (abbrev == DW_ABRV_AUTO || abbrev == DW_ABRV_AUTO_LOCLIST) 
                abbrev = DW_ABRV_AUTO_ABSTRACT;
            else if (abbrev == DW_ABRV_PARAM || abbrev == DW_ABRV_PARAM_LOCLIST) 
                abbrev = DW_ABRV_PARAM_ABSTRACT;
                        Uleb128put(ctxt, info, int64(abbrev));
            putattr(ctxt, info, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(v.Name)), v.Name); 

            // Isreturn attribute if this is a param
            if (abbrev == DW_ABRV_PARAM_ABSTRACT)
            {
                long isReturn = default;
                if (v.IsReturnValue)
                {
                    isReturn = 1L;
                }
                putattr(ctxt, info, abbrev, DW_FORM_flag, DW_CLS_FLAG, isReturn, null);
            } 

            // Line
            if (abbrev != DW_ABRV_PARAM_ABSTRACT)
            { 
                // See issue 23374 for more on why decl line is skipped for abs params.
                putattr(ctxt, info, abbrev, DW_FORM_udata, DW_CLS_CONSTANT, int64(v.DeclLine), null);
            } 

            // Type
            putattr(ctxt, info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, v.Type); 

            // Var has no children => no terminator
        }

        private static void putvar(Context ctxt, ref FnState s, ref Var v, Sym absfn, long fnabbrev, long inlIndex, slice<byte> encbuf)
        { 
            // Remap abbrev according to parent DIE abbrev
            var (abbrev, missing, concrete) = determineVarAbbrev(v, fnabbrev);

            Uleb128put(ctxt, s.Info, int64(abbrev)); 

            // Abstract origin for concrete / inlined case
            if (concrete)
            { 
                // Here we are making a reference to a child DIE of an abstract
                // function subprogram DIE. The child DIE has no LSym, so instead
                // after the call to 'putattr' below we make a call to register
                // the child DIE reference.
                putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, absfn);
                ctxt.RecordDclReference(s.Info, absfn, int(v.ChildIndex), inlIndex);
            }
            else
            { 
                // Var name, line for abstract and default cases
                var n = v.Name;
                putattr(ctxt, s.Info, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(n)), n);
                if (abbrev == DW_ABRV_PARAM || abbrev == DW_ABRV_PARAM_LOCLIST || abbrev == DW_ABRV_PARAM_ABSTRACT)
                {
                    long isReturn = default;
                    if (v.IsReturnValue)
                    {
                        isReturn = 1L;
                    }
                    putattr(ctxt, s.Info, abbrev, DW_FORM_flag, DW_CLS_FLAG, isReturn, null);
                }
                putattr(ctxt, s.Info, abbrev, DW_FORM_udata, DW_CLS_CONSTANT, int64(v.DeclLine), null);
                putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0L, v.Type);
            }
            if (abbrevUsesLoclist(abbrev))
            {
                putattr(ctxt, s.Info, abbrev, DW_FORM_sec_offset, DW_CLS_PTR, int64(s.Loc.Len()), s.Loc);
                addLocList(ctxt, s.Loc, s.StartPC, v, encbuf);
            }
            else
            {
                var loc = encbuf[..0L];

                if (missing) 
                    break; // no location
                else if (v.StackOffset == 0L) 
                    loc = append(loc, DW_OP_call_frame_cfa);
                else 
                    loc = append(loc, DW_OP_fbreg);
                    loc = AppendSleb128(loc, int64(v.StackOffset));
                                putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, int64(len(loc)), loc);
            } 

            // Var has no children => no terminator
        }

        private static void addLocList(Context ctxt, Sym listSym, Sym startPC, ref Var v, slice<byte> encbuf)
        { 
            // Base address entry: max ptr followed by the base address.
            ctxt.AddInt(listSym, ctxt.PtrSize(), ~0L);
            ctxt.AddAddress(listSym, startPC, 0L);
            foreach (var (_, entry) in v.LocationList)
            {
                ctxt.AddInt(listSym, ctxt.PtrSize(), entry.StartPC);
                ctxt.AddInt(listSym, ctxt.PtrSize(), entry.EndPC);
                var locBuf = encbuf[..0L];
                foreach (var (_, piece) in entry.Pieces)
                {
                    if (!piece.Missing)
                    {
                        if (piece.OnStack)
                        {
                            if (piece.StackOffset == 0L)
                            {
                                locBuf = append(locBuf, DW_OP_call_frame_cfa);
                            }
                            else
                            {
                                locBuf = append(locBuf, DW_OP_fbreg);
                                locBuf = AppendSleb128(locBuf, int64(piece.StackOffset));
                            }
                        }
                        else
                        {
                            if (piece.RegNum < 32L)
                            {
                                locBuf = append(locBuf, DW_OP_reg0 + byte(piece.RegNum));
                            }
                            else
                            {
                                locBuf = append(locBuf, DW_OP_regx);
                                locBuf = AppendUleb128(locBuf, uint64(piece.RegNum));
                            }
                        }
                    }
                    if (len(entry.Pieces) > 1L)
                    {
                        locBuf = append(locBuf, DW_OP_piece);
                        locBuf = AppendUleb128(locBuf, uint64(piece.Length));
                    }
                }
                ctxt.AddInt(listSym, 2L, int64(len(locBuf)));
                ctxt.AddBytes(listSym, locBuf);
            } 
            // End list
            ctxt.AddInt(listSym, ctxt.PtrSize(), 0L);
            ctxt.AddInt(listSym, ctxt.PtrSize(), 0L);
        }

        // VarsByOffset attaches the methods of sort.Interface to []*Var,
        // sorting in increasing StackOffset.
        public partial struct VarsByOffset // : slice<ref Var>
        {
        }

        public static long Len(this VarsByOffset s)
        {
            return len(s);
        }
        public static bool Less(this VarsByOffset s, long i, long j)
        {
            return s[i].StackOffset < s[j].StackOffset;
        }
        public static void Swap(this VarsByOffset s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        // byChildIndex implements sort.Interface for []*dwarf.Var by child index.
        private partial struct byChildIndex // : slice<ref Var>
        {
        }

        private static long Len(this byChildIndex s)
        {
            return len(s);
        }
        private static bool Less(this byChildIndex s, long i, long j)
        {
            return s[i].ChildIndex < s[j].ChildIndex;
        }
        private static void Swap(this byChildIndex s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }
    }
}}}
