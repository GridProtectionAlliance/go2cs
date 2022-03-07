// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dwarf generates DWARF debugging information.
// DWARF generation is split between the compiler and the linker,
// this package contains the shared code.
// package dwarf -- go2cs converted at 2022 March 06 22:46:11 UTC
// import "cmd/internal/dwarf" ==> using dwarf = go.cmd.@internal.dwarf_package
// Original source: C:\Program Files\Go\src\cmd\internal\dwarf\dwarf.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using exec = go.@internal.execabs_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using objabi = go.cmd.@internal.objabi_package;
using System;


namespace go.cmd.@internal;

public static partial class dwarf_package {

    // InfoPrefix is the prefix for all the symbols containing DWARF info entries.
public static readonly @string InfoPrefix = "go.info.";

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
public partial interface Sym {
    long Length(object dwarfContext);
}

// A Var represents a local variable or a function parameter.
public partial struct Var {
    public @string Name;
    public nint Abbrev; // Either DW_ABRV_AUTO[_LOCLIST] or DW_ABRV_PARAM[_LOCLIST]
    public bool IsReturnValue;
    public bool IsInlFormal;
    public int StackOffset; // This package can't use the ssa package, so it can't mention ssa.FuncDebug,
// so indirect through a closure.
    public Action<Sym, Sym> PutLocationList;
    public int Scope;
    public Sym Type;
    public @string DeclFile;
    public nuint DeclLine;
    public nuint DeclCol;
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
public partial struct Scope {
    public int Parent;
    public slice<Range> Ranges;
    public slice<ptr<Var>> Vars;
}

// A Range represents a half-open interval [Start, End).
public partial struct Range {
    public long Start;
    public long End;
}

// This container is used by the PutFunc* variants below when
// creating the DWARF subprogram DIE(s) for a function.
public partial struct FnState {
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
    public bool UseBASEntries;
}

public static void EnableLogging(bool doit) {
    logDwarf = doit;
}

// MergeRanges creates a new range list by merging the ranges from
// its two arguments, then returns the new list.
public static slice<Range> MergeRanges(slice<Range> in1, slice<Range> in2) {
    var @out = make_slice<Range>(0, len(in1) + len(in2));
    nint i = 0;
    nint j = 0;
    while (true) {
        Range cur = default;
        if (i < len(in2) && j < len(in1)) {
            if (in2[i].Start < in1[j].Start) {
                cur = in2[i];
                i++;
            }
            else
 {
                cur = in1[j];
                j++;
            }

        }
        else if (i < len(in2)) {
            cur = in2[i];
            i++;
        }
        else if (j < len(in1)) {
            cur = in1[j];
            j++;
        }
        else
 {
            break;
        }
        {
            var n = len(out);

            if (n > 0 && cur.Start <= out[n - 1].End) {
                out[n - 1].End = cur.End;
            }
            else
 {
                out = append(out, cur);
            }

        }

    }

    return out;

}

// UnifyRanges merges the ranges from 'c' into the list of ranges for 's'.
private static void UnifyRanges(this ptr<Scope> _addr_s, ptr<Scope> _addr_c) {
    ref Scope s = ref _addr_s.val;
    ref Scope c = ref _addr_c.val;

    s.Ranges = MergeRanges(s.Ranges, c.Ranges);
}

// AppendRange adds r to s, if r is non-empty.
// If possible, it extends the last Range in s.Ranges; if not, it creates a new one.
private static void AppendRange(this ptr<Scope> _addr_s, Range r) {
    ref Scope s = ref _addr_s.val;

    if (r.End <= r.Start) {
        return ;
    }
    var i = len(s.Ranges);
    if (i > 0 && s.Ranges[i - 1].End == r.Start) {
        s.Ranges[i - 1].End = r.End;
        return ;
    }
    s.Ranges = append(s.Ranges, r);

}

public partial struct InlCalls {
    public slice<InlCall> Calls;
}

public partial struct InlCall {
    public nint InlIndex; // Symbol of file containing inlined call site (really *obj.LSym).
    public Sym CallFile; // Line number of inlined call site.
    public uint CallLine; // Dwarf abstract subroutine symbol (really *obj.LSym).
    public Sym AbsFunSym; // Indices of child inlines within Calls array above.
    public slice<nint> Children; // entries in this list are PAUTO's created by the inliner to
// capture the promoted formals and locals of the inlined callee.
    public slice<ptr<Var>> InlVars; // PC ranges for this inlined call.
    public slice<Range> Ranges; // Root call (not a child of some other call).
    public bool Root;
}

// A Context specifies how to add data to a Sym.
public partial interface Context {
    void PtrSize();
    void AddInt(Sym s, nint size, long i);
    void AddBytes(Sym s, slice<byte> b);
    void AddAddress(Sym s, object t, long ofs);
    void AddCURelativeAddress(Sym s, object t, long ofs);
    void AddSectionOffset(Sym s, nint size, object t, long ofs);
    void AddDWARFAddrSectionOffset(Sym s, object t, long ofs);
    void CurrentOffset(Sym s);
    void RecordDclReference(Sym from, Sym to, nint dclIdx, nint inlIndex);
    void RecordChildDieOffsets(Sym s, slice<ptr<Var>> vars, slice<int> offsets);
    void AddString(Sym s, @string v);
    void AddFileRef(Sym s, object f);
    void Logf(@string format, params object[] args);
}

// AppendUleb128 appends v to b using DWARF's unsigned LEB128 encoding.
public static slice<byte> AppendUleb128(slice<byte> b, ulong v) {
    while (true) {
        var c = uint8(v & 0x7f);
        v>>=7;
        if (v != 0) {
            c |= 0x80;
        }
        b = append(b, c);
        if (c & 0x80 == 0) {
            break;
        }
    }
    return b;

}

// AppendSleb128 appends v to b using DWARF's signed LEB128 encoding.
public static slice<byte> AppendSleb128(slice<byte> b, long v) {
    while (true) {
        var c = uint8(v & 0x7f);
        var s = uint8(v & 0x40);
        v>>=7;
        if ((v != -1 || s == 0) && (v != 0 || s != 0)) {
            c |= 0x80;
        }
        b = append(b, c);
        if (c & 0x80 == 0) {
            break;
        }
    }
    return b;

}

// sevenbits contains all unsigned seven bit numbers, indexed by their value.
private static array<byte> sevenbits = new array<byte>(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f });

// sevenBitU returns the unsigned LEB128 encoding of v if v is seven bits and nil otherwise.
// The contents of the returned slice must not be modified.
private static slice<byte> sevenBitU(long v) {
    if (uint64(v) < uint64(len(sevenbits))) {
        return sevenbits[(int)v..(int)v + 1];
    }
    return null;

}

// sevenBitS returns the signed LEB128 encoding of v if v is seven bits and nil otherwise.
// The contents of the returned slice must not be modified.
private static slice<byte> sevenBitS(long v) {
    if (uint64(v) <= 63) {
        return sevenbits[(int)v..(int)v + 1];
    }
    if (uint64(-v) <= 64) {
        return sevenbits[(int)128 + v..(int)128 + v + 1];
    }
    return null;

}

// Uleb128put appends v to s using DWARF's unsigned LEB128 encoding.
public static void Uleb128put(Context ctxt, Sym s, long v) {
    var b = sevenBitU(v);
    if (b == null) {
        array<byte> encbuf = new array<byte>(20);
        b = AppendUleb128(encbuf[..(int)0], uint64(v));
    }
    ctxt.AddBytes(s, b);

}

// Sleb128put appends v to s using DWARF's signed LEB128 encoding.
public static void Sleb128put(Context ctxt, Sym s, long v) {
    var b = sevenBitS(v);
    if (b == null) {
        array<byte> encbuf = new array<byte>(20);
        b = AppendSleb128(encbuf[..(int)0], v);
    }
    ctxt.AddBytes(s, b);

}

/*
 * Defining Abbrevs. This is hardcoded on a per-platform basis (that is,
 * each platform will see a fixed abbrev table for all objects); the number
 * of abbrev entries is fairly small (compared to C++ objects).  The DWARF
 * spec places no restriction on the ordering of attributes in the
 * Abbrevs and DIEs, and we will always write them out in the order
 * of declaration in the abbrev.
 */
private partial struct dwAttrForm {
    public ushort attr;
    public byte form;
}

// Go-specific type attributes.
public static readonly nuint DW_AT_go_kind = 0x2900;
public static readonly nuint DW_AT_go_key = 0x2901;
public static readonly nuint DW_AT_go_elem = 0x2902; 
// Attribute for DW_TAG_member of a struct type.
// Nonzero value indicates the struct field is an embedded field.
public static readonly nuint DW_AT_go_embedded_field = 0x2903;
public static readonly nuint DW_AT_go_runtime_type = 0x2904;

public static readonly nuint DW_AT_go_package_name = 0x2905; // Attribute for DW_TAG_compile_unit

public static readonly nint DW_AT_internal_location = 253; // params and locals; not emitted

// Index into the abbrevs table below.
public static readonly var DW_ABRV_NULL = iota;
public static readonly var DW_ABRV_COMPUNIT = 0;
public static readonly var DW_ABRV_COMPUNIT_TEXTLESS = 1;
public static readonly var DW_ABRV_FUNCTION = 2;
public static readonly var DW_ABRV_FUNCTION_ABSTRACT = 3;
public static readonly var DW_ABRV_FUNCTION_CONCRETE = 4;
public static readonly var DW_ABRV_INLINED_SUBROUTINE = 5;
public static readonly var DW_ABRV_INLINED_SUBROUTINE_RANGES = 6;
public static readonly var DW_ABRV_VARIABLE = 7;
public static readonly var DW_ABRV_INT_CONSTANT = 8;
public static readonly var DW_ABRV_AUTO = 9;
public static readonly var DW_ABRV_AUTO_LOCLIST = 10;
public static readonly var DW_ABRV_AUTO_ABSTRACT = 11;
public static readonly var DW_ABRV_AUTO_CONCRETE = 12;
public static readonly var DW_ABRV_AUTO_CONCRETE_LOCLIST = 13;
public static readonly var DW_ABRV_PARAM = 14;
public static readonly var DW_ABRV_PARAM_LOCLIST = 15;
public static readonly var DW_ABRV_PARAM_ABSTRACT = 16;
public static readonly var DW_ABRV_PARAM_CONCRETE = 17;
public static readonly var DW_ABRV_PARAM_CONCRETE_LOCLIST = 18;
public static readonly var DW_ABRV_LEXICAL_BLOCK_RANGES = 19;
public static readonly var DW_ABRV_LEXICAL_BLOCK_SIMPLE = 20;
public static readonly var DW_ABRV_STRUCTFIELD = 21;
public static readonly var DW_ABRV_FUNCTYPEPARAM = 22;
public static readonly var DW_ABRV_DOTDOTDOT = 23;
public static readonly var DW_ABRV_ARRAYRANGE = 24;
public static readonly var DW_ABRV_NULLTYPE = 25;
public static readonly var DW_ABRV_BASETYPE = 26;
public static readonly var DW_ABRV_ARRAYTYPE = 27;
public static readonly var DW_ABRV_CHANTYPE = 28;
public static readonly var DW_ABRV_FUNCTYPE = 29;
public static readonly var DW_ABRV_IFACETYPE = 30;
public static readonly var DW_ABRV_MAPTYPE = 31;
public static readonly var DW_ABRV_PTRTYPE = 32;
public static readonly var DW_ABRV_BARE_PTRTYPE = 33; // only for void*, no DW_AT_type attr to please gdb 6.
public static readonly var DW_ABRV_SLICETYPE = 34;
public static readonly var DW_ABRV_STRINGTYPE = 35;
public static readonly var DW_ABRV_STRUCTTYPE = 36;
public static readonly var DW_ABRV_TYPEDECL = 37;
public static readonly var DW_NABRV = 38;


private partial struct dwAbbrev {
    public byte tag;
    public byte children;
    public slice<dwAttrForm> attr;
}

private static bool abbrevsFinalized = default;

// expandPseudoForm takes an input DW_FORM_xxx value and translates it
// into a platform-appropriate concrete form. Existing concrete/real
// DW_FORM values are left untouched. For the moment the only
// pseudo-form is DW_FORM_udata_pseudo, which gets expanded to
// DW_FORM_data4 on Darwin and DW_FORM_udata everywhere else. See
// issue #31459 for more context.
private static byte expandPseudoForm(byte form) { 
    // Is this a pseudo-form?
    if (form != DW_FORM_udata_pseudo) {
        return form;
    }
    var expandedForm = DW_FORM_udata;
    if (buildcfg.GOOS == "darwin" || buildcfg.GOOS == "ios") {
        expandedForm = DW_FORM_data4;
    }
    return uint8(expandedForm);

}

// Abbrevs() returns the finalized abbrev array for the platform,
// expanding any DW_FORM pseudo-ops to real values.
public static slice<dwAbbrev> Abbrevs() {
    if (abbrevsFinalized) {
        return abbrevs[..];
    }
    for (nint i = 1; i < DW_NABRV; i++) {
        for (nint j = 0; j < len(abbrevs[i].attr); j++) {
            abbrevs[i].attr[j].form = expandPseudoForm(abbrevs[i].attr[j].form);
        }
    }
    abbrevsFinalized = true;
    return abbrevs[..];

}

// abbrevs is a raw table of abbrev entries; it needs to be post-processed
// by the Abbrevs() function above prior to being consumed, to expand
// the 'pseudo-form' entries below to real DWARF form values.

private static array<dwAbbrev> abbrevs = new array<dwAbbrev>(new dwAbbrev[] { {0,0,[]dwAttrForm{}}, {DW_TAG_compile_unit,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_language,DW_FORM_data1},{DW_AT_stmt_list,DW_FORM_sec_offset},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_ranges,DW_FORM_sec_offset},{DW_AT_comp_dir,DW_FORM_string},{DW_AT_producer,DW_FORM_string},{DW_AT_go_package_name,DW_FORM_string},},}, {DW_TAG_compile_unit,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_language,DW_FORM_data1},{DW_AT_comp_dir,DW_FORM_string},{DW_AT_producer,DW_FORM_string},{DW_AT_go_package_name,DW_FORM_string},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_frame_base,DW_FORM_block1},{DW_AT_decl_file,DW_FORM_data4},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_inline,DW_FORM_data1},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_subprogram,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_frame_base,DW_FORM_block1},},}, {DW_TAG_inlined_subroutine,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},{DW_AT_call_file,DW_FORM_data4},{DW_AT_call_line,DW_FORM_udata_pseudo},},}, {DW_TAG_inlined_subroutine,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_ranges,DW_FORM_sec_offset},{DW_AT_call_file,DW_FORM_data4},{DW_AT_call_line,DW_FORM_udata_pseudo},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_location,DW_FORM_block1},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_external,DW_FORM_flag},},}, {DW_TAG_constant,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_const_value,DW_FORM_sdata},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_variable,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_decl_line,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_variable_parameter,DW_FORM_flag},{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_block1},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_abstract_origin,DW_FORM_ref_addr},{DW_AT_location,DW_FORM_sec_offset},},}, {DW_TAG_lexical_block,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_ranges,DW_FORM_sec_offset},},}, {DW_TAG_lexical_block,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_low_pc,DW_FORM_addr},{DW_AT_high_pc,DW_FORM_addr},},}, {DW_TAG_member,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_data_member_location,DW_FORM_udata},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_embedded_field,DW_FORM_flag},},}, {DW_TAG_formal_parameter,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_type,DW_FORM_ref_addr},},}, {DW_TAG_unspecified_parameters,DW_CHILDREN_no,[]dwAttrForm{},}, {DW_TAG_subrange_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_type,DW_FORM_ref_addr},{DW_AT_count,DW_FORM_udata},},}, {DW_TAG_unspecified_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},},}, {DW_TAG_base_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_encoding,DW_FORM_data1},{DW_AT_byte_size,DW_FORM_data1},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_array_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_subroutine_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_typedef,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},{DW_AT_go_key,DW_FORM_ref_addr},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_pointer_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_pointer_type,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},{DW_AT_go_elem,DW_FORM_ref_addr},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_structure_type,DW_CHILDREN_yes,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_byte_size,DW_FORM_udata},{DW_AT_go_kind,DW_FORM_data1},{DW_AT_go_runtime_type,DW_FORM_addr},},}, {DW_TAG_typedef,DW_CHILDREN_no,[]dwAttrForm{{DW_AT_name,DW_FORM_string},{DW_AT_type,DW_FORM_ref_addr},},} });

// GetAbbrev returns the contents of the .debug_abbrev section.
public static slice<byte> GetAbbrev() {
    var abbrevs = Abbrevs();
    slice<byte> buf = default;
    for (nint i = 1; i < DW_NABRV; i++) { 
        // See section 7.5.3
        buf = AppendUleb128(buf, uint64(i));
        buf = AppendUleb128(buf, uint64(abbrevs[i].tag));
        buf = append(buf, abbrevs[i].children);
        foreach (var (_, f) in abbrevs[i].attr) {
            buf = AppendUleb128(buf, uint64(f.attr));
            buf = AppendUleb128(buf, uint64(f.form));
        }        buf = append(buf, 0, 0);

    }
    return append(buf, 0);

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
public partial struct DWAttr {
    public ptr<DWAttr> Link;
    public ushort Atr; // DW_AT_
    public byte Cls; // DW_CLS_
    public long Value;
}

// DWDie represents a DWARF debug info entry.
public partial struct DWDie {
    public nint Abbrev;
    public ptr<DWDie> Link;
    public ptr<DWDie> Child;
    public ptr<DWAttr> Attr;
    public Sym Sym;
}

private static error putattr(Context ctxt, Sym s, nint abbrev, nint form, nint cls, long value, object data) {

    if (form == DW_FORM_addr) // address
    {
        // Allow nil addresses for DW_AT_go_runtime_type.
        if (data == null && value == 0) {
            ctxt.AddInt(s, ctxt.PtrSize(), 0);
            break;
        }
        if (cls == DW_CLS_GO_TYPEREF) {
            ctxt.AddSectionOffset(s, ctxt.PtrSize(), data, value);
            break;
        }
        ctxt.AddAddress(s, data, value);
        goto __switch_break0;
    }
    if (form == DW_FORM_block1) // block
    {
        if (cls == DW_CLS_ADDRESS) {
            ctxt.AddInt(s, 1, int64(1 + ctxt.PtrSize()));
            ctxt.AddInt(s, 1, DW_OP_addr);
            ctxt.AddAddress(s, data, 0);
            break;
        }
        value &= 0xff;
        ctxt.AddInt(s, 1, value);
        slice<byte> p = data._<slice<byte>>()[..(int)value];
        ctxt.AddBytes(s, p);
        goto __switch_break0;
    }
    if (form == DW_FORM_block2) // block
    {
        value &= 0xffff;

        ctxt.AddInt(s, 2, value);
        p = data._<slice<byte>>()[..(int)value];
        ctxt.AddBytes(s, p);
        goto __switch_break0;
    }
    if (form == DW_FORM_block4) // block
    {
        value &= 0xffffffff;

        ctxt.AddInt(s, 4, value);
        p = data._<slice<byte>>()[..(int)value];
        ctxt.AddBytes(s, p);
        goto __switch_break0;
    }
    if (form == DW_FORM_block) // block
    {
        Uleb128put(ctxt, s, value);

        p = data._<slice<byte>>()[..(int)value];
        ctxt.AddBytes(s, p);
        goto __switch_break0;
    }
    if (form == DW_FORM_data1) // constant
    {
        ctxt.AddInt(s, 1, value);
        goto __switch_break0;
    }
    if (form == DW_FORM_data2) // constant
    {
        ctxt.AddInt(s, 2, value);
        goto __switch_break0;
    }
    if (form == DW_FORM_data4) // constant, {line,loclist,mac,rangelist}ptr
    {
        if (cls == DW_CLS_PTR) { // DW_AT_stmt_list and DW_AT_ranges
            ctxt.AddDWARFAddrSectionOffset(s, data, value);
            break;

        }
        ctxt.AddInt(s, 4, value);
        goto __switch_break0;
    }
    if (form == DW_FORM_data8) // constant, {line,loclist,mac,rangelist}ptr
    {
        ctxt.AddInt(s, 8, value);
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
        for (var i = int64(len(str)); i < value; i++) {
            ctxt.AddInt(s, 1, 0);
        }
        goto __switch_break0;
    }
    if (form == DW_FORM_flag) // flag
    {
        if (value != 0) {
            ctxt.AddInt(s, 1, 1);
        }
        else
 {
            ctxt.AddInt(s, 1, 0);
        }
        goto __switch_break0;
    }
    if (form == DW_FORM_ref_addr) // reference to a DIE in the .info section
    {
        fallthrough = true;
    }
    if (fallthrough || form == DW_FORM_sec_offset) // offset into a DWARF section other than .info
    {
        if (data == null) {
            return error.As(fmt.Errorf("dwarf: null reference in %d", abbrev))!;
        }
        ctxt.AddDWARFAddrSectionOffset(s, data, value);
        goto __switch_break0;
    }
    if (form == DW_FORM_ref1 || form == DW_FORM_ref2 || form == DW_FORM_ref4 || form == DW_FORM_ref8 || form == DW_FORM_ref_udata || form == DW_FORM_strp || form == DW_FORM_indirect) // (see Section 7.5.3)
    {
    }
    // default: 
        return error.As(fmt.Errorf("dwarf: unsupported attribute form %d / class %d", form, cls))!;

    __switch_break0:;
    return error.As(null!)!;

}

// PutAttrs writes the attributes for a DIE to symbol 's'.
//
// Note that we can (and do) add arbitrary attributes to a DIE, but
// only the ones actually listed in the Abbrev will be written out.
public static void PutAttrs(Context ctxt, Sym s, nint abbrev, ptr<DWAttr> _addr_attr) {
    ref DWAttr attr = ref _addr_attr.val;

    var abbrevs = Abbrevs();
Outer:
    foreach (var (_, f) in abbrevs[abbrev].attr) {
        {
            var ap = attr;

            while (ap != null) {
                if (ap.Atr == f.attr) {
                    putattr(ctxt, s, abbrev, int(f.form), int(ap.Cls), ap.Value, ap.Data);
                    _continueOuter = true;
                    break;
                ap = ap.Link;
                }

            }

        }

        putattr(ctxt, s, abbrev, int(f.form), 0, 0, null);

    }
}

// HasChildren reports whether 'die' uses an abbrev that supports children.
public static bool HasChildren(ptr<DWDie> _addr_die) {
    ref DWDie die = ref _addr_die.val;

    var abbrevs = Abbrevs();
    return abbrevs[die.Abbrev].children != 0;
}

// PutIntConst writes a DIE for an integer constant
public static void PutIntConst(Context ctxt, Sym info, Sym typ, @string name, long val) {
    Uleb128put(ctxt, info, DW_ABRV_INT_CONSTANT);
    putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_string, DW_CLS_STRING, int64(len(name)), name);
    putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, typ);
    putattr(ctxt, info, DW_ABRV_INT_CONSTANT, DW_FORM_sdata, DW_CLS_CONSTANT, val, null);
}

// PutGlobal writes a DIE for a global variable.
public static void PutGlobal(Context ctxt, Sym info, Sym typ, Sym gvar, @string name) {
    Uleb128put(ctxt, info, DW_ABRV_VARIABLE);
    putattr(ctxt, info, DW_ABRV_VARIABLE, DW_FORM_string, DW_CLS_STRING, int64(len(name)), name);
    putattr(ctxt, info, DW_ABRV_VARIABLE, DW_FORM_block1, DW_CLS_ADDRESS, 0, gvar);
    putattr(ctxt, info, DW_ABRV_VARIABLE, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, typ);
    putattr(ctxt, info, DW_ABRV_VARIABLE, DW_FORM_flag, DW_CLS_FLAG, 1, null);
}

// PutBasedRanges writes a range table to sym. All addresses in ranges are
// relative to some base address, which must be arranged by the caller
// (e.g., with a DW_AT_low_pc attribute, or in a BASE-prefixed range).
public static void PutBasedRanges(Context ctxt, Sym sym, slice<Range> ranges) {
    var ps = ctxt.PtrSize(); 
    // Write ranges.
    foreach (var (_, r) in ranges) {
        ctxt.AddInt(sym, ps, r.Start);
        ctxt.AddInt(sym, ps, r.End);
    }    ctxt.AddInt(sym, ps, 0);
    ctxt.AddInt(sym, ps, 0);

}

// PutRanges writes a range table to s.Ranges.
// All addresses in ranges are relative to s.base.
private static void PutRanges(this ptr<FnState> _addr_s, Context ctxt, slice<Range> ranges) {
    ref FnState s = ref _addr_s.val;

    var ps = ctxt.PtrSize();
    var sym = s.Ranges;
    var @base = s.StartPC;

    if (s.UseBASEntries) { 
        // Using a Base Address Selection Entry reduces the number of relocations, but
        // this is not done on macOS because it is not supported by dsymutil/dwarfdump/lldb
        ctxt.AddInt(sym, ps, -1);
        ctxt.AddAddress(sym, base, 0);
        PutBasedRanges(ctxt, sym, ranges);
        return ;

    }
    foreach (var (_, r) in ranges) {
        ctxt.AddCURelativeAddress(sym, base, r.Start);
        ctxt.AddCURelativeAddress(sym, base, r.End);
    }    ctxt.AddInt(sym, ps, 0);
    ctxt.AddInt(sym, ps, 0);

}

// Return TRUE if the inlined call in the specified slot is empty,
// meaning it has a zero-length range (no instructions), and all
// of its children are empty.
private static bool isEmptyInlinedCall(nint slot, ptr<InlCalls> _addr_calls) {
    ref InlCalls calls = ref _addr_calls.val;

    var ic = _addr_calls.Calls[slot];
    if (ic.InlIndex == -2) {
        return true;
    }
    var live = false;
    foreach (var (_, k) in ic.Children) {
        if (!isEmptyInlinedCall(k, _addr_calls)) {
            live = true;
        }
    }    if (len(ic.Ranges) > 0) {
        live = true;
    }
    if (!live) {
        ic.InlIndex = -2;
    }
    return !live;

}

// Slot -1:    return top-level inlines
// Slot >= 0:  return children of that slot
private static slice<nint> inlChildren(nint slot, ptr<InlCalls> _addr_calls) {
    ref InlCalls calls = ref _addr_calls.val;

    slice<nint> kids = default;
    if (slot != -1) {
        {
            var k__prev1 = k;

            foreach (var (_, __k) in calls.Calls[slot].Children) {
                k = __k;
                if (!isEmptyInlinedCall(k, _addr_calls)) {
                    kids = append(kids, k);
                }
            }
    else

            k = k__prev1;
        }
    } {
        {
            var k__prev1 = k;

            nint k = 0;

            while (k < len(calls.Calls)) {
                if (calls.Calls[k].Root && !isEmptyInlinedCall(k, _addr_calls)) {
                    kids = append(kids, k);
                k += 1;
                }

            }


            k = k__prev1;
        }

    }
    return kids;

}

private static map<ptr<Var>, bool> inlinedVarTable(ptr<InlCalls> _addr_inlcalls) {
    ref InlCalls inlcalls = ref _addr_inlcalls.val;

    var vars = make_map<ptr<Var>, bool>();
    foreach (var (_, ic) in inlcalls.Calls) {
        foreach (var (_, v) in ic.InlVars) {
            vars[v] = true;
        }
    }    return vars;

}

// The s.Scopes slice contains variables were originally part of the
// function being emitted, as well as variables that were imported
// from various callee functions during the inlining process. This
// function prunes out any variables from the latter category (since
// they will be emitted as part of DWARF inlined_subroutine DIEs) and
// then generates scopes for vars in the former category.
private static error putPrunedScopes(Context ctxt, ptr<FnState> _addr_s, nint fnabbrev) {
    ref FnState s = ref _addr_s.val;

    if (len(s.Scopes) == 0) {
        return error.As(null!)!;
    }
    var scopes = make_slice<Scope>(len(s.Scopes), len(s.Scopes));
    var pvars = inlinedVarTable(_addr_s.InlCalls);
    foreach (var (k, s) in s.Scopes) {
        Scope pruned = new Scope(Parent:s.Parent,Ranges:s.Ranges);
        for (nint i = 0; i < len(s.Vars); i++) {
            var (_, found) = pvars[s.Vars[i]];
            if (!found) {
                pruned.Vars = append(pruned.Vars, s.Vars[i]);
            }
        }
        sort.Sort(byChildIndex(pruned.Vars));
        scopes[k] = pruned;
    }    array<byte> encbuf = new array<byte>(20);
    if (putscope(ctxt, _addr_s, scopes, 0, fnabbrev, encbuf[..(int)0]) < int32(len(scopes))) {
        return error.As(errors.New("multiple toplevel scopes"))!;
    }
    return error.As(null!)!;

}

// Emit DWARF attributes and child DIEs for an 'abstract' subprogram.
// The abstract subprogram DIE for a function contains its
// location-independent attributes (name, type, etc). Other instances
// of the function (any inlined copy of it, or the single out-of-line
// 'concrete' instance) will contain a pointer back to this abstract
// DIE (as a space-saving measure, so that name/type etc doesn't have
// to be repeated for each inlined copy).
public static error PutAbstractFunc(Context ctxt, ptr<FnState> _addr_s) {
    ref FnState s = ref _addr_s.val;

    if (logDwarf) {
        ctxt.Logf("PutAbstractFunc(%v)\n", s.Absfn);
    }
    var abbrev = DW_ABRV_FUNCTION_ABSTRACT;
    Uleb128put(ctxt, s.Absfn, int64(abbrev));

    var fullname = s.Name;
    if (strings.HasPrefix(s.Name, "\"\".")) { 
        // Generate a fully qualified name for the function in the
        // abstract case. This is so as to avoid the need for the
        // linker to process the DIE with patchDWARFName(); we can't
        // allow the name attribute of an abstract subprogram DIE to
        // be rewritten, since it would change the offsets of the
        // child DIEs (which we're relying on in order for abstract
        // origin references to work).
        fullname = objabi.PathToPrefix(s.Importpath) + "." + s.Name[(int)3..];

    }
    putattr(ctxt, s.Absfn, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(fullname)), fullname); 

    // DW_AT_inlined value
    putattr(ctxt, s.Absfn, abbrev, DW_FORM_data1, DW_CLS_CONSTANT, int64(DW_INL_inlined), null);

    long ev = default;
    if (s.External) {
        ev = 1;
    }
    putattr(ctxt, s.Absfn, abbrev, DW_FORM_flag, DW_CLS_FLAG, ev, 0); 

    // Child variables (may be empty)
    slice<ptr<Var>> flattened = default; 

    // This slice will hold the offset in bytes for each child var DIE
    // with respect to the start of the parent subprogram DIE.
    slice<int> offsets = default; 

    // Scopes/vars
    if (len(s.Scopes) > 0) { 
        // For abstract subprogram DIEs we want to flatten out scope info:
        // lexical scope DIEs contain range and/or hi/lo PC attributes,
        // which we explicitly don't want for the abstract subprogram DIE.
        var pvars = inlinedVarTable(_addr_s.InlCalls);
        foreach (var (_, scope) in s.Scopes) {
            {
                nint i__prev2 = i;

                for (nint i = 0; i < len(scope.Vars); i++) {
                    var (_, found) = pvars[scope.Vars[i]];
                    if (found || !scope.Vars[i].IsInAbstract) {
                        continue;
                    }
                    flattened = append(flattened, scope.Vars[i]);
                }


                i = i__prev2;
            }

        }        if (len(flattened) > 0) {
            sort.Sort(byChildIndex(flattened));

            if (logDwarf) {
                ctxt.Logf("putAbstractScope(%v): vars:", s.Info);
                {
                    nint i__prev1 = i;
                    var v__prev1 = v;

                    foreach (var (__i, __v) in flattened) {
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

                foreach (var (_, __v) in flattened) {
                    v = __v;
                    offsets = append(offsets, int32(ctxt.CurrentOffset(s.Absfn)));
                    putAbstractVar(ctxt, s.Absfn, _addr_v);
                }

                v = v__prev1;
            }
        }
    }
    ctxt.RecordChildDieOffsets(s.Absfn, flattened, offsets);

    Uleb128put(ctxt, s.Absfn, 0);
    return error.As(null!)!;

}

// Emit DWARF attributes and child DIEs for an inlined subroutine. The
// first attribute of an inlined subroutine DIE is a reference back to
// its corresponding 'abstract' DIE (containing location-independent
// attributes such as name, type, etc). Inlined subroutine DIEs can
// have other inlined subroutine DIEs as children.
private static error putInlinedFunc(Context ctxt, ptr<FnState> _addr_s, Sym callersym, nint callIdx) {
    ref FnState s = ref _addr_s.val;

    var ic = s.InlCalls.Calls[callIdx];
    var callee = ic.AbsFunSym;

    var abbrev = DW_ABRV_INLINED_SUBROUTINE_RANGES;
    if (len(ic.Ranges) == 1) {
        abbrev = DW_ABRV_INLINED_SUBROUTINE;
    }
    Uleb128put(ctxt, s.Info, int64(abbrev));

    if (logDwarf) {
        ctxt.Logf("putInlinedFunc(caller=%v,callee=%v,abbrev=%d)\n", callersym, callee, abbrev);
    }
    putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, callee);

    if (abbrev == DW_ABRV_INLINED_SUBROUTINE_RANGES) {
        putattr(ctxt, s.Info, abbrev, DW_FORM_sec_offset, DW_CLS_PTR, s.Ranges.Length(ctxt), s.Ranges);
        s.PutRanges(ctxt, ic.Ranges);
    }
    else
 {
        var st = ic.Ranges[0].Start;
        var en = ic.Ranges[0].End;
        putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, st, s.StartPC);
        putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, en, s.StartPC);
    }
    ctxt.AddFileRef(s.Info, ic.CallFile);
    var form = int(expandPseudoForm(DW_FORM_udata_pseudo));
    putattr(ctxt, s.Info, abbrev, form, DW_CLS_CONSTANT, int64(ic.CallLine), null); 

    // Variables associated with this inlined routine instance.
    var vars = ic.InlVars;
    sort.Sort(byChildIndex(vars));
    var inlIndex = ic.InlIndex;
    array<byte> encbuf = new array<byte>(20);
    foreach (var (_, v) in vars) {
        if (!v.IsInAbstract) {
            continue;
        }
        putvar(ctxt, _addr_s, _addr_v, callee, abbrev, inlIndex, encbuf[..(int)0]);

    }    foreach (var (_, sib) in inlChildren(callIdx, _addr_s.InlCalls)) {
        var absfn = s.InlCalls.Calls[sib].AbsFunSym;
        var err = putInlinedFunc(ctxt, _addr_s, absfn, sib);
        if (err != null) {
            return error.As(err)!;
        }
    }    Uleb128put(ctxt, s.Info, 0);
    return error.As(null!)!;

}

// Emit DWARF attributes and child DIEs for a 'concrete' subprogram,
// meaning the out-of-line copy of a function that was inlined at some
// point during the compilation of its containing package. The first
// attribute for a concrete DIE is a reference to the 'abstract' DIE
// for the function (which holds location-independent attributes such
// as name, type), then the remainder of the attributes are specific
// to this instance (location, frame base, etc).
public static error PutConcreteFunc(Context ctxt, ptr<FnState> _addr_s) {
    ref FnState s = ref _addr_s.val;

    if (logDwarf) {
        ctxt.Logf("PutConcreteFunc(%v)\n", s.Info);
    }
    var abbrev = DW_ABRV_FUNCTION_CONCRETE;
    Uleb128put(ctxt, s.Info, int64(abbrev)); 

    // Abstract origin.
    putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, s.Absfn); 

    // Start/end PC.
    putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, 0, s.StartPC);
    putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, s.Size, s.StartPC); 

    // cfa / frame base
    putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, 1, new slice<byte>(new byte[] { DW_OP_call_frame_cfa })); 

    // Scopes
    {
        var err__prev1 = err;

        var err = putPrunedScopes(ctxt, _addr_s, abbrev);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Inlined subroutines.
    foreach (var (_, sib) in inlChildren(-1, _addr_s.InlCalls)) {
        var absfn = s.InlCalls.Calls[sib].AbsFunSym;
        err = putInlinedFunc(ctxt, _addr_s, absfn, sib);
        if (err != null) {
            return error.As(err)!;
        }
    }    Uleb128put(ctxt, s.Info, 0);
    return error.As(null!)!;

}

// Emit DWARF attributes and child DIEs for a subprogram. Here
// 'default' implies that the function in question was not inlined
// when its containing package was compiled (hence there is no need to
// emit an abstract version for it to use as a base for inlined
// routine records).
public static error PutDefaultFunc(Context ctxt, ptr<FnState> _addr_s) {
    ref FnState s = ref _addr_s.val;

    if (logDwarf) {
        ctxt.Logf("PutDefaultFunc(%v)\n", s.Info);
    }
    var abbrev = DW_ABRV_FUNCTION;
    Uleb128put(ctxt, s.Info, int64(abbrev)); 

    // Expand '"".' to import path.
    var name = s.Name;
    if (s.Importpath != "") {
        name = strings.Replace(name, "\"\".", objabi.PathToPrefix(s.Importpath) + ".", -1);
    }
    putattr(ctxt, s.Info, DW_ABRV_FUNCTION, DW_FORM_string, DW_CLS_STRING, int64(len(name)), name);
    putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, 0, s.StartPC);
    putattr(ctxt, s.Info, abbrev, DW_FORM_addr, DW_CLS_ADDRESS, s.Size, s.StartPC);
    putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, 1, new slice<byte>(new byte[] { DW_OP_call_frame_cfa }));
    ctxt.AddFileRef(s.Info, s.Filesym);

    long ev = default;
    if (s.External) {
        ev = 1;
    }
    putattr(ctxt, s.Info, abbrev, DW_FORM_flag, DW_CLS_FLAG, ev, 0); 

    // Scopes
    {
        var err__prev1 = err;

        var err = putPrunedScopes(ctxt, _addr_s, abbrev);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Inlined subroutines.
    foreach (var (_, sib) in inlChildren(-1, _addr_s.InlCalls)) {
        var absfn = s.InlCalls.Calls[sib].AbsFunSym;
        err = putInlinedFunc(ctxt, _addr_s, absfn, sib);
        if (err != null) {
            return error.As(err)!;
        }
    }    Uleb128put(ctxt, s.Info, 0);
    return error.As(null!)!;

}

private static int putscope(Context ctxt, ptr<FnState> _addr_s, slice<Scope> scopes, int curscope, nint fnabbrev, slice<byte> encbuf) {
    ref FnState s = ref _addr_s.val;

    if (logDwarf) {
        ctxt.Logf("putscope(%v,%d): vars:", s.Info, curscope);
        {
            var v__prev1 = v;

            foreach (var (__i, __v) in scopes[curscope].Vars) {
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

        foreach (var (_, __v) in scopes[curscope].Vars) {
            v = __v;
            putvar(ctxt, _addr_s, _addr_v, s.Absfn, fnabbrev, -1, encbuf);
        }
        v = v__prev1;
    }

    var @this = curscope;
    curscope++;
    while (curscope < int32(len(scopes))) {
        var scope = scopes[curscope];
        if (scope.Parent != this) {
            return curscope;
        }
        if (len(scopes[curscope].Vars) == 0) {
            curscope = putscope(ctxt, _addr_s, scopes, curscope, fnabbrev, encbuf);
            continue;
        }
        if (len(scope.Ranges) == 1) {
            Uleb128put(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE);
            putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE, DW_FORM_addr, DW_CLS_ADDRESS, scope.Ranges[0].Start, s.StartPC);
            putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_SIMPLE, DW_FORM_addr, DW_CLS_ADDRESS, scope.Ranges[0].End, s.StartPC);
        }
        else
 {
            Uleb128put(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_RANGES);
            putattr(ctxt, s.Info, DW_ABRV_LEXICAL_BLOCK_RANGES, DW_FORM_sec_offset, DW_CLS_PTR, s.Ranges.Length(ctxt), s.Ranges);

            s.PutRanges(ctxt, scope.Ranges);
        }
        curscope = putscope(ctxt, _addr_s, scopes, curscope, fnabbrev, encbuf);

        Uleb128put(ctxt, s.Info, 0);

    }
    return curscope;

}

// Given a default var abbrev code, select corresponding concrete code.
private static nint concreteVarAbbrev(nint varAbbrev) => func((_, panic, _) => {

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
private static (nint, bool, bool) determineVarAbbrev(ptr<Var> _addr_v, nint fnabbrev) => func((_, panic, _) => {
    nint _p0 = default;
    bool _p0 = default;
    bool _p0 = default;
    ref Var v = ref _addr_v.val;

    var abbrev = v.Abbrev; 

    // If the variable was entirely optimized out, don't emit a location list;
    // convert to an inline abbreviation and emit an empty location.
    var missing = false;

    if (abbrev == DW_ABRV_AUTO_LOCLIST && v.PutLocationList == null) 
        missing = true;
        abbrev = DW_ABRV_AUTO;
    else if (abbrev == DW_ABRV_PARAM_LOCLIST && v.PutLocationList == null) 
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
        if (!v.IsInAbstract) {
            concrete = false;
        }
    else if (fnabbrev == DW_ABRV_INLINED_SUBROUTINE || fnabbrev == DW_ABRV_INLINED_SUBROUTINE_RANGES)     else 
        panic("should never happen");
    // Select proper abbrev based on concrete/non-concrete
    if (concrete) {
        abbrev = concreteVarAbbrev(abbrev);
    }
    return (abbrev, missing, concrete);

});

private static bool abbrevUsesLoclist(nint abbrev) {

    if (abbrev == DW_ABRV_AUTO_LOCLIST || abbrev == DW_ABRV_AUTO_CONCRETE_LOCLIST || abbrev == DW_ABRV_PARAM_LOCLIST || abbrev == DW_ABRV_PARAM_CONCRETE_LOCLIST) 
        return true;
    else 
        return false;
    
}

// Emit DWARF attributes for a variable belonging to an 'abstract' subprogram.
private static void putAbstractVar(Context ctxt, Sym info, ptr<Var> _addr_v) {
    ref Var v = ref _addr_v.val;
 
    // Remap abbrev
    var abbrev = v.Abbrev;

    if (abbrev == DW_ABRV_AUTO || abbrev == DW_ABRV_AUTO_LOCLIST) 
        abbrev = DW_ABRV_AUTO_ABSTRACT;
    else if (abbrev == DW_ABRV_PARAM || abbrev == DW_ABRV_PARAM_LOCLIST) 
        abbrev = DW_ABRV_PARAM_ABSTRACT;
        Uleb128put(ctxt, info, int64(abbrev));
    putattr(ctxt, info, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(v.Name)), v.Name); 

    // Isreturn attribute if this is a param
    if (abbrev == DW_ABRV_PARAM_ABSTRACT) {
        long isReturn = default;
        if (v.IsReturnValue) {
            isReturn = 1;
        }
        putattr(ctxt, info, abbrev, DW_FORM_flag, DW_CLS_FLAG, isReturn, null);

    }
    if (abbrev != DW_ABRV_PARAM_ABSTRACT) { 
        // See issue 23374 for more on why decl line is skipped for abs params.
        putattr(ctxt, info, abbrev, DW_FORM_udata, DW_CLS_CONSTANT, int64(v.DeclLine), null);

    }
    putattr(ctxt, info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, v.Type); 

    // Var has no children => no terminator
}

private static void putvar(Context ctxt, ptr<FnState> _addr_s, ptr<Var> _addr_v, Sym absfn, nint fnabbrev, nint inlIndex, slice<byte> encbuf) {
    ref FnState s = ref _addr_s.val;
    ref Var v = ref _addr_v.val;
 
    // Remap abbrev according to parent DIE abbrev
    var (abbrev, missing, concrete) = determineVarAbbrev(_addr_v, fnabbrev);

    Uleb128put(ctxt, s.Info, int64(abbrev)); 

    // Abstract origin for concrete / inlined case
    if (concrete) { 
        // Here we are making a reference to a child DIE of an abstract
        // function subprogram DIE. The child DIE has no LSym, so instead
        // after the call to 'putattr' below we make a call to register
        // the child DIE reference.
        putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, absfn);
        ctxt.RecordDclReference(s.Info, absfn, int(v.ChildIndex), inlIndex);

    }
    else
 { 
        // Var name, line for abstract and default cases
        var n = v.Name;
        putattr(ctxt, s.Info, abbrev, DW_FORM_string, DW_CLS_STRING, int64(len(n)), n);
        if (abbrev == DW_ABRV_PARAM || abbrev == DW_ABRV_PARAM_LOCLIST || abbrev == DW_ABRV_PARAM_ABSTRACT) {
            long isReturn = default;
            if (v.IsReturnValue) {
                isReturn = 1;
            }
            putattr(ctxt, s.Info, abbrev, DW_FORM_flag, DW_CLS_FLAG, isReturn, null);
        }
        putattr(ctxt, s.Info, abbrev, DW_FORM_udata, DW_CLS_CONSTANT, int64(v.DeclLine), null);
        putattr(ctxt, s.Info, abbrev, DW_FORM_ref_addr, DW_CLS_REFERENCE, 0, v.Type);

    }
    if (abbrevUsesLoclist(abbrev)) {
        putattr(ctxt, s.Info, abbrev, DW_FORM_sec_offset, DW_CLS_PTR, s.Loc.Length(ctxt), s.Loc);
        v.PutLocationList(s.Loc, s.StartPC);
    }
    else
 {
        var loc = encbuf[..(int)0];

        if (missing) 
            break; // no location
        else if (v.StackOffset == 0) 
            loc = append(loc, DW_OP_call_frame_cfa);
        else 
            loc = append(loc, DW_OP_fbreg);
            loc = AppendSleb128(loc, int64(v.StackOffset));
                putattr(ctxt, s.Info, abbrev, DW_FORM_block1, DW_CLS_BLOCK, int64(len(loc)), loc);

    }
}

// byChildIndex implements sort.Interface for []*dwarf.Var by child index.
private partial struct byChildIndex { // : slice<ptr<Var>>
}

private static nint Len(this byChildIndex s) {
    return len(s);
}
private static bool Less(this byChildIndex s, nint i, nint j) {
    return s[i].ChildIndex < s[j].ChildIndex;
}
private static void Swap(this byChildIndex s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

// IsDWARFEnabledOnAIX returns true if DWARF is possible on the
// current extld.
// AIX ld doesn't support DWARF with -bnoobjreorder with version
// prior to 7.2.2.
public static (bool, error) IsDWARFEnabledOnAIXLd(@string extld) {
    bool _p0 = default;
    error _p0 = default!;

    var (out, err) = exec.Command(extld, "-Wl,-V").CombinedOutput();
    if (err != null) { 
        // The normal output should display ld version and
        // then fails because ".main" is not defined:
        // ld: 0711-317 ERROR: Undefined symbol: .main
        if (!bytes.Contains(out, (slice<byte>)"0711-317")) {
            return (false, error.As(fmt.Errorf("%s -Wl,-V failed: %v\n%s", extld, err, out))!);
        }
    }
    out = bytes.TrimPrefix(out, (slice<byte>)"/usr/bin/ld: LD ");
    var vers = string(bytes.Split(out, (slice<byte>)"(")[0]);
    var subvers = strings.Split(vers, ".");
    if (len(subvers) != 3) {
        return (false, error.As(fmt.Errorf("cannot parse %s -Wl,-V (%s): %v\n", extld, out, err))!);
    }
    {
        var v__prev1 = v;

        var (v, err) = strconv.Atoi(subvers[0]);

        if (err != null || v < 7) {
            return (false, error.As(null!)!);
        }
        else if (v > 7) {
            return (true, error.As(null!)!);
        }

        v = v__prev1;

    }

    {
        var v__prev1 = v;

        (v, err) = strconv.Atoi(subvers[1]);

        if (err != null || v < 2) {
            return (false, error.As(null!)!);
        }
        else if (v > 2) {
            return (true, error.As(null!)!);
        }

        v = v__prev1;

    }

    {
        var v__prev1 = v;

        (v, err) = strconv.Atoi(subvers[2]);

        if (err != null || v < 2) {
            return (false, error.As(null!)!);
        }
        v = v__prev1;

    }

    return (true, error.As(null!)!);

}

} // end dwarf_package
