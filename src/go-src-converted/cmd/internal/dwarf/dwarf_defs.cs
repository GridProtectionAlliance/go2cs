// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarf -- go2cs converted at 2022 March 13 05:57:23 UTC
// import "cmd/internal/dwarf" ==> using dwarf = go.cmd.@internal.dwarf_package
// Original source: C:\Program Files\Go\src\cmd\internal\dwarf\dwarf_defs.go
namespace go.cmd.@internal;

public static partial class dwarf_package {

// Cut, pasted, tr-and-awk'ed from tables in
// http://dwarfstd.org/doc/Dwarf3.pdf

// Table 18
public static readonly nuint DW_TAG_array_type = 0x01;
public static readonly nuint DW_TAG_class_type = 0x02;
public static readonly nuint DW_TAG_entry_point = 0x03;
public static readonly nuint DW_TAG_enumeration_type = 0x04;
public static readonly nuint DW_TAG_formal_parameter = 0x05;
public static readonly nuint DW_TAG_imported_declaration = 0x08;
public static readonly nuint DW_TAG_label = 0x0a;
public static readonly nuint DW_TAG_lexical_block = 0x0b;
public static readonly nuint DW_TAG_member = 0x0d;
public static readonly nuint DW_TAG_pointer_type = 0x0f;
public static readonly nuint DW_TAG_reference_type = 0x10;
public static readonly nuint DW_TAG_compile_unit = 0x11;
public static readonly nuint DW_TAG_string_type = 0x12;
public static readonly nuint DW_TAG_structure_type = 0x13;
public static readonly nuint DW_TAG_subroutine_type = 0x15;
public static readonly nuint DW_TAG_typedef = 0x16;
public static readonly nuint DW_TAG_union_type = 0x17;
public static readonly nuint DW_TAG_unspecified_parameters = 0x18;
public static readonly nuint DW_TAG_variant = 0x19;
public static readonly nuint DW_TAG_common_block = 0x1a;
public static readonly nuint DW_TAG_common_inclusion = 0x1b;
public static readonly nuint DW_TAG_inheritance = 0x1c;
public static readonly nuint DW_TAG_inlined_subroutine = 0x1d;
public static readonly nuint DW_TAG_module = 0x1e;
public static readonly nuint DW_TAG_ptr_to_member_type = 0x1f;
public static readonly nuint DW_TAG_set_type = 0x20;
public static readonly nuint DW_TAG_subrange_type = 0x21;
public static readonly nuint DW_TAG_with_stmt = 0x22;
public static readonly nuint DW_TAG_access_declaration = 0x23;
public static readonly nuint DW_TAG_base_type = 0x24;
public static readonly nuint DW_TAG_catch_block = 0x25;
public static readonly nuint DW_TAG_const_type = 0x26;
public static readonly nuint DW_TAG_constant = 0x27;
public static readonly nuint DW_TAG_enumerator = 0x28;
public static readonly nuint DW_TAG_file_type = 0x29;
public static readonly nuint DW_TAG_friend = 0x2a;
public static readonly nuint DW_TAG_namelist = 0x2b;
public static readonly nuint DW_TAG_namelist_item = 0x2c;
public static readonly nuint DW_TAG_packed_type = 0x2d;
public static readonly nuint DW_TAG_subprogram = 0x2e;
public static readonly nuint DW_TAG_template_type_parameter = 0x2f;
public static readonly nuint DW_TAG_template_value_parameter = 0x30;
public static readonly nuint DW_TAG_thrown_type = 0x31;
public static readonly nuint DW_TAG_try_block = 0x32;
public static readonly nuint DW_TAG_variant_part = 0x33;
public static readonly nuint DW_TAG_variable = 0x34;
public static readonly nuint DW_TAG_volatile_type = 0x35; 
// Dwarf3
public static readonly nuint DW_TAG_dwarf_procedure = 0x36;
public static readonly nuint DW_TAG_restrict_type = 0x37;
public static readonly nuint DW_TAG_interface_type = 0x38;
public static readonly nuint DW_TAG_namespace = 0x39;
public static readonly nuint DW_TAG_imported_module = 0x3a;
public static readonly nuint DW_TAG_unspecified_type = 0x3b;
public static readonly nuint DW_TAG_partial_unit = 0x3c;
public static readonly nuint DW_TAG_imported_unit = 0x3d;
public static readonly nuint DW_TAG_condition = 0x3f;
public static readonly nuint DW_TAG_shared_type = 0x40; 
// Dwarf4
public static readonly nuint DW_TAG_type_unit = 0x41;
public static readonly nuint DW_TAG_rvalue_reference_type = 0x42;
public static readonly nuint DW_TAG_template_alias = 0x43; 

// User defined
public static readonly nuint DW_TAG_lo_user = 0x4080;
public static readonly nuint DW_TAG_hi_user = 0xffff;

// Table 19
public static readonly nuint DW_CHILDREN_no = 0x00;
public static readonly nuint DW_CHILDREN_yes = 0x01;

// Not from the spec, but logically belongs here
public static readonly nuint DW_CLS_ADDRESS = 0x01 + iota;
public static readonly var DW_CLS_BLOCK = 0;
public static readonly var DW_CLS_CONSTANT = 1;
public static readonly var DW_CLS_FLAG = 2;
public static readonly var DW_CLS_PTR = 3; // lineptr, loclistptr, macptr, rangelistptr
public static readonly var DW_CLS_REFERENCE = 4;
public static readonly var DW_CLS_ADDRLOC = 5;
public static readonly var DW_CLS_STRING = 6; 

// Go-specific internal hackery.
public static readonly var DW_CLS_GO_TYPEREF = 7;

// Table 20
public static readonly nuint DW_AT_sibling = 0x01; // reference
public static readonly nuint DW_AT_location = 0x02; // block, loclistptr
public static readonly nuint DW_AT_name = 0x03; // string
public static readonly nuint DW_AT_ordering = 0x09; // constant
public static readonly nuint DW_AT_byte_size = 0x0b; // block, constant, reference
public static readonly nuint DW_AT_bit_offset = 0x0c; // block, constant, reference
public static readonly nuint DW_AT_bit_size = 0x0d; // block, constant, reference
public static readonly nuint DW_AT_stmt_list = 0x10; // lineptr
public static readonly nuint DW_AT_low_pc = 0x11; // address
public static readonly nuint DW_AT_high_pc = 0x12; // address
public static readonly nuint DW_AT_language = 0x13; // constant
public static readonly nuint DW_AT_discr = 0x15; // reference
public static readonly nuint DW_AT_discr_value = 0x16; // constant
public static readonly nuint DW_AT_visibility = 0x17; // constant
public static readonly nuint DW_AT_import = 0x18; // reference
public static readonly nuint DW_AT_string_length = 0x19; // block, loclistptr
public static readonly nuint DW_AT_common_reference = 0x1a; // reference
public static readonly nuint DW_AT_comp_dir = 0x1b; // string
public static readonly nuint DW_AT_const_value = 0x1c; // block, constant, string
public static readonly nuint DW_AT_containing_type = 0x1d; // reference
public static readonly nuint DW_AT_default_value = 0x1e; // reference
public static readonly nuint DW_AT_inline = 0x20; // constant
public static readonly nuint DW_AT_is_optional = 0x21; // flag
public static readonly nuint DW_AT_lower_bound = 0x22; // block, constant, reference
public static readonly nuint DW_AT_producer = 0x25; // string
public static readonly nuint DW_AT_prototyped = 0x27; // flag
public static readonly nuint DW_AT_return_addr = 0x2a; // block, loclistptr
public static readonly nuint DW_AT_start_scope = 0x2c; // constant
public static readonly nuint DW_AT_bit_stride = 0x2e; // constant
public static readonly nuint DW_AT_upper_bound = 0x2f; // block, constant, reference
public static readonly nuint DW_AT_abstract_origin = 0x31; // reference
public static readonly nuint DW_AT_accessibility = 0x32; // constant
public static readonly nuint DW_AT_address_class = 0x33; // constant
public static readonly nuint DW_AT_artificial = 0x34; // flag
public static readonly nuint DW_AT_base_types = 0x35; // reference
public static readonly nuint DW_AT_calling_convention = 0x36; // constant
public static readonly nuint DW_AT_count = 0x37; // block, constant, reference
public static readonly nuint DW_AT_data_member_location = 0x38; // block, constant, loclistptr
public static readonly nuint DW_AT_decl_column = 0x39; // constant
public static readonly nuint DW_AT_decl_file = 0x3a; // constant
public static readonly nuint DW_AT_decl_line = 0x3b; // constant
public static readonly nuint DW_AT_declaration = 0x3c; // flag
public static readonly nuint DW_AT_discr_list = 0x3d; // block
public static readonly nuint DW_AT_encoding = 0x3e; // constant
public static readonly nuint DW_AT_external = 0x3f; // flag
public static readonly nuint DW_AT_frame_base = 0x40; // block, loclistptr
public static readonly nuint DW_AT_friend = 0x41; // reference
public static readonly nuint DW_AT_identifier_case = 0x42; // constant
public static readonly nuint DW_AT_macro_info = 0x43; // macptr
public static readonly nuint DW_AT_namelist_item = 0x44; // block
public static readonly nuint DW_AT_priority = 0x45; // reference
public static readonly nuint DW_AT_segment = 0x46; // block, loclistptr
public static readonly nuint DW_AT_specification = 0x47; // reference
public static readonly nuint DW_AT_static_link = 0x48; // block, loclistptr
public static readonly nuint DW_AT_type = 0x49; // reference
public static readonly nuint DW_AT_use_location = 0x4a; // block, loclistptr
public static readonly nuint DW_AT_variable_parameter = 0x4b; // flag
public static readonly nuint DW_AT_virtuality = 0x4c; // constant
public static readonly nuint DW_AT_vtable_elem_location = 0x4d; // block, loclistptr
// Dwarf3
public static readonly nuint DW_AT_allocated = 0x4e; // block, constant, reference
public static readonly nuint DW_AT_associated = 0x4f; // block, constant, reference
public static readonly nuint DW_AT_data_location = 0x50; // block
public static readonly nuint DW_AT_byte_stride = 0x51; // block, constant, reference
public static readonly nuint DW_AT_entry_pc = 0x52; // address
public static readonly nuint DW_AT_use_UTF8 = 0x53; // flag
public static readonly nuint DW_AT_extension = 0x54; // reference
public static readonly nuint DW_AT_ranges = 0x55; // rangelistptr
public static readonly nuint DW_AT_trampoline = 0x56; // address, flag, reference, string
public static readonly nuint DW_AT_call_column = 0x57; // constant
public static readonly nuint DW_AT_call_file = 0x58; // constant
public static readonly nuint DW_AT_call_line = 0x59; // constant
public static readonly nuint DW_AT_description = 0x5a; // string
public static readonly nuint DW_AT_binary_scale = 0x5b; // constant
public static readonly nuint DW_AT_decimal_scale = 0x5c; // constant
public static readonly nuint DW_AT_small = 0x5d; // reference
public static readonly nuint DW_AT_decimal_sign = 0x5e; // constant
public static readonly nuint DW_AT_digit_count = 0x5f; // constant
public static readonly nuint DW_AT_picture_string = 0x60; // string
public static readonly nuint DW_AT_mutable = 0x61; // flag
public static readonly nuint DW_AT_threads_scaled = 0x62; // flag
public static readonly nuint DW_AT_explicit = 0x63; // flag
public static readonly nuint DW_AT_object_pointer = 0x64; // reference
public static readonly nuint DW_AT_endianity = 0x65; // constant
public static readonly nuint DW_AT_elemental = 0x66; // flag
public static readonly nuint DW_AT_pure = 0x67; // flag
public static readonly nuint DW_AT_recursive = 0x68; // flag

public static readonly nuint DW_AT_lo_user = 0x2000; // ---
public static readonly nuint DW_AT_hi_user = 0x3fff; // ---

// Table 21
public static readonly nuint DW_FORM_addr = 0x01; // address
public static readonly nuint DW_FORM_block2 = 0x03; // block
public static readonly nuint DW_FORM_block4 = 0x04; // block
public static readonly nuint DW_FORM_data2 = 0x05; // constant
public static readonly nuint DW_FORM_data4 = 0x06; // constant, lineptr, loclistptr, macptr, rangelistptr
public static readonly nuint DW_FORM_data8 = 0x07; // constant, lineptr, loclistptr, macptr, rangelistptr
public static readonly nuint DW_FORM_string = 0x08; // string
public static readonly nuint DW_FORM_block = 0x09; // block
public static readonly nuint DW_FORM_block1 = 0x0a; // block
public static readonly nuint DW_FORM_data1 = 0x0b; // constant
public static readonly nuint DW_FORM_flag = 0x0c; // flag
public static readonly nuint DW_FORM_sdata = 0x0d; // constant
public static readonly nuint DW_FORM_strp = 0x0e; // string
public static readonly nuint DW_FORM_udata = 0x0f; // constant
public static readonly nuint DW_FORM_ref_addr = 0x10; // reference
public static readonly nuint DW_FORM_ref1 = 0x11; // reference
public static readonly nuint DW_FORM_ref2 = 0x12; // reference
public static readonly nuint DW_FORM_ref4 = 0x13; // reference
public static readonly nuint DW_FORM_ref8 = 0x14; // reference
public static readonly nuint DW_FORM_ref_udata = 0x15; // reference
public static readonly nuint DW_FORM_indirect = 0x16; // (see Section 7.5.3)
// Dwarf4
public static readonly nuint DW_FORM_sec_offset = 0x17; // lineptr, loclistptr, macptr, rangelistptr
public static readonly nuint DW_FORM_exprloc = 0x18; // exprloc
public static readonly nuint DW_FORM_flag_present = 0x19; // flag
public static readonly nuint DW_FORM_ref_sig8 = 0x20; // reference
// Pseudo-form: expanded to data4 on IOS, udata elsewhere.
public static readonly nuint DW_FORM_udata_pseudo = 0x99;

// Table 24 (#operands, notes)
public static readonly nuint DW_OP_addr = 0x03; // 1 constant address (size target specific)
public static readonly nuint DW_OP_deref = 0x06; // 0
public static readonly nuint DW_OP_const1u = 0x08; // 1 1-byte constant
public static readonly nuint DW_OP_const1s = 0x09; // 1 1-byte constant
public static readonly nuint DW_OP_const2u = 0x0a; // 1 2-byte constant
public static readonly nuint DW_OP_const2s = 0x0b; // 1 2-byte constant
public static readonly nuint DW_OP_const4u = 0x0c; // 1 4-byte constant
public static readonly nuint DW_OP_const4s = 0x0d; // 1 4-byte constant
public static readonly nuint DW_OP_const8u = 0x0e; // 1 8-byte constant
public static readonly nuint DW_OP_const8s = 0x0f; // 1 8-byte constant
public static readonly nuint DW_OP_constu = 0x10; // 1 ULEB128 constant
public static readonly nuint DW_OP_consts = 0x11; // 1 SLEB128 constant
public static readonly nuint DW_OP_dup = 0x12; // 0
public static readonly nuint DW_OP_drop = 0x13; // 0
public static readonly nuint DW_OP_over = 0x14; // 0
public static readonly nuint DW_OP_pick = 0x15; // 1 1-byte stack index
public static readonly nuint DW_OP_swap = 0x16; // 0
public static readonly nuint DW_OP_rot = 0x17; // 0
public static readonly nuint DW_OP_xderef = 0x18; // 0
public static readonly nuint DW_OP_abs = 0x19; // 0
public static readonly nuint DW_OP_and = 0x1a; // 0
public static readonly nuint DW_OP_div = 0x1b; // 0
public static readonly nuint DW_OP_minus = 0x1c; // 0
public static readonly nuint DW_OP_mod = 0x1d; // 0
public static readonly nuint DW_OP_mul = 0x1e; // 0
public static readonly nuint DW_OP_neg = 0x1f; // 0
public static readonly nuint DW_OP_not = 0x20; // 0
public static readonly nuint DW_OP_or = 0x21; // 0
public static readonly nuint DW_OP_plus = 0x22; // 0
public static readonly nuint DW_OP_plus_uconst = 0x23; // 1 ULEB128 addend
public static readonly nuint DW_OP_shl = 0x24; // 0
public static readonly nuint DW_OP_shr = 0x25; // 0
public static readonly nuint DW_OP_shra = 0x26; // 0
public static readonly nuint DW_OP_xor = 0x27; // 0
public static readonly nuint DW_OP_skip = 0x2f; // 1 signed 2-byte constant
public static readonly nuint DW_OP_bra = 0x28; // 1 signed 2-byte constant
public static readonly nuint DW_OP_eq = 0x29; // 0
public static readonly nuint DW_OP_ge = 0x2a; // 0
public static readonly nuint DW_OP_gt = 0x2b; // 0
public static readonly nuint DW_OP_le = 0x2c; // 0
public static readonly nuint DW_OP_lt = 0x2d; // 0
public static readonly nuint DW_OP_ne = 0x2e; // 0
public static readonly nuint DW_OP_lit0 = 0x30; // 0 ...
public static readonly nuint DW_OP_lit31 = 0x4f; // 0 literals 0..31 = (DW_OP_lit0 + literal)
public static readonly nuint DW_OP_reg0 = 0x50; // 0 ..
public static readonly nuint DW_OP_reg31 = 0x6f; // 0 reg 0..31 = (DW_OP_reg0 + regnum)
public static readonly nuint DW_OP_breg0 = 0x70; // 1 ...
public static readonly nuint DW_OP_breg31 = 0x8f; // 1 SLEB128 offset base register 0..31 = (DW_OP_breg0 + regnum)
public static readonly nuint DW_OP_regx = 0x90; // 1 ULEB128 register
public static readonly nuint DW_OP_fbreg = 0x91; // 1 SLEB128 offset
public static readonly nuint DW_OP_bregx = 0x92; // 2 ULEB128 register followed by SLEB128 offset
public static readonly nuint DW_OP_piece = 0x93; // 1 ULEB128 size of piece addressed
public static readonly nuint DW_OP_deref_size = 0x94; // 1 1-byte size of data retrieved
public static readonly nuint DW_OP_xderef_size = 0x95; // 1 1-byte size of data retrieved
public static readonly nuint DW_OP_nop = 0x96; // 0
public static readonly nuint DW_OP_push_object_address = 0x97; // 0
public static readonly nuint DW_OP_call2 = 0x98; // 1 2-byte offset of DIE
public static readonly nuint DW_OP_call4 = 0x99; // 1 4-byte offset of DIE
public static readonly nuint DW_OP_call_ref = 0x9a; // 1 4- or 8-byte offset of DIE
public static readonly nuint DW_OP_form_tls_address = 0x9b; // 0
public static readonly nuint DW_OP_call_frame_cfa = 0x9c; // 0
public static readonly nuint DW_OP_bit_piece = 0x9d; // 2
public static readonly nuint DW_OP_lo_user = 0xe0;
public static readonly nuint DW_OP_hi_user = 0xff;

// Table 25
public static readonly nuint DW_ATE_address = 0x01;
public static readonly nuint DW_ATE_boolean = 0x02;
public static readonly nuint DW_ATE_complex_float = 0x03;
public static readonly nuint DW_ATE_float = 0x04;
public static readonly nuint DW_ATE_signed = 0x05;
public static readonly nuint DW_ATE_signed_char = 0x06;
public static readonly nuint DW_ATE_unsigned = 0x07;
public static readonly nuint DW_ATE_unsigned_char = 0x08;
public static readonly nuint DW_ATE_imaginary_float = 0x09;
public static readonly nuint DW_ATE_packed_decimal = 0x0a;
public static readonly nuint DW_ATE_numeric_string = 0x0b;
public static readonly nuint DW_ATE_edited = 0x0c;
public static readonly nuint DW_ATE_signed_fixed = 0x0d;
public static readonly nuint DW_ATE_unsigned_fixed = 0x0e;
public static readonly nuint DW_ATE_decimal_float = 0x0f;
public static readonly nuint DW_ATE_lo_user = 0x80;
public static readonly nuint DW_ATE_hi_user = 0xff;

// Table 26
public static readonly nuint DW_DS_unsigned = 0x01;
public static readonly nuint DW_DS_leading_overpunch = 0x02;
public static readonly nuint DW_DS_trailing_overpunch = 0x03;
public static readonly nuint DW_DS_leading_separate = 0x04;
public static readonly nuint DW_DS_trailing_separate = 0x05;

// Table 27
public static readonly nuint DW_END_default = 0x00;
public static readonly nuint DW_END_big = 0x01;
public static readonly nuint DW_END_little = 0x02;
public static readonly nuint DW_END_lo_user = 0x40;
public static readonly nuint DW_END_hi_user = 0xff;

// Table 28
public static readonly nuint DW_ACCESS_public = 0x01;
public static readonly nuint DW_ACCESS_protected = 0x02;
public static readonly nuint DW_ACCESS_private = 0x03;

// Table 29
public static readonly nuint DW_VIS_local = 0x01;
public static readonly nuint DW_VIS_exported = 0x02;
public static readonly nuint DW_VIS_qualified = 0x03;

// Table 30
public static readonly nuint DW_VIRTUALITY_none = 0x00;
public static readonly nuint DW_VIRTUALITY_virtual = 0x01;
public static readonly nuint DW_VIRTUALITY_pure_virtual = 0x02;

// Table 31
public static readonly nuint DW_LANG_C89 = 0x0001;
public static readonly nuint DW_LANG_C = 0x0002;
public static readonly nuint DW_LANG_Ada83 = 0x0003;
public static readonly nuint DW_LANG_C_plus_plus = 0x0004;
public static readonly nuint DW_LANG_Cobol74 = 0x0005;
public static readonly nuint DW_LANG_Cobol85 = 0x0006;
public static readonly nuint DW_LANG_Fortran77 = 0x0007;
public static readonly nuint DW_LANG_Fortran90 = 0x0008;
public static readonly nuint DW_LANG_Pascal83 = 0x0009;
public static readonly nuint DW_LANG_Modula2 = 0x000a; 
// Dwarf3
public static readonly nuint DW_LANG_Java = 0x000b;
public static readonly nuint DW_LANG_C99 = 0x000c;
public static readonly nuint DW_LANG_Ada95 = 0x000d;
public static readonly nuint DW_LANG_Fortran95 = 0x000e;
public static readonly nuint DW_LANG_PLI = 0x000f;
public static readonly nuint DW_LANG_ObjC = 0x0010;
public static readonly nuint DW_LANG_ObjC_plus_plus = 0x0011;
public static readonly nuint DW_LANG_UPC = 0x0012;
public static readonly nuint DW_LANG_D = 0x0013; 
// Dwarf4
public static readonly nuint DW_LANG_Python = 0x0014; 
// Dwarf5
public static readonly nuint DW_LANG_Go = 0x0016;

public static readonly nuint DW_LANG_lo_user = 0x8000;
public static readonly nuint DW_LANG_hi_user = 0xffff;

// Table 32
public static readonly nuint DW_ID_case_sensitive = 0x00;
public static readonly nuint DW_ID_up_case = 0x01;
public static readonly nuint DW_ID_down_case = 0x02;
public static readonly nuint DW_ID_case_insensitive = 0x03;

// Table 33
public static readonly nuint DW_CC_normal = 0x01;
public static readonly nuint DW_CC_program = 0x02;
public static readonly nuint DW_CC_nocall = 0x03;
public static readonly nuint DW_CC_lo_user = 0x40;
public static readonly nuint DW_CC_hi_user = 0xff;

// Table 34
public static readonly nuint DW_INL_not_inlined = 0x00;
public static readonly nuint DW_INL_inlined = 0x01;
public static readonly nuint DW_INL_declared_not_inlined = 0x02;
public static readonly nuint DW_INL_declared_inlined = 0x03;

// Table 35
public static readonly nuint DW_ORD_row_major = 0x00;
public static readonly nuint DW_ORD_col_major = 0x01;

// Table 36
public static readonly nuint DW_DSC_label = 0x00;
public static readonly nuint DW_DSC_range = 0x01;

// Table 37
public static readonly nuint DW_LNS_copy = 0x01;
public static readonly nuint DW_LNS_advance_pc = 0x02;
public static readonly nuint DW_LNS_advance_line = 0x03;
public static readonly nuint DW_LNS_set_file = 0x04;
public static readonly nuint DW_LNS_set_column = 0x05;
public static readonly nuint DW_LNS_negate_stmt = 0x06;
public static readonly nuint DW_LNS_set_basic_block = 0x07;
public static readonly nuint DW_LNS_const_add_pc = 0x08;
public static readonly nuint DW_LNS_fixed_advance_pc = 0x09; 
// Dwarf3
public static readonly nuint DW_LNS_set_prologue_end = 0x0a;
public static readonly nuint DW_LNS_set_epilogue_begin = 0x0b;
public static readonly nuint DW_LNS_set_isa = 0x0c;

// Table 38
public static readonly nuint DW_LNE_end_sequence = 0x01;
public static readonly nuint DW_LNE_set_address = 0x02;
public static readonly nuint DW_LNE_define_file = 0x03;
public static readonly nuint DW_LNE_lo_user = 0x80;
public static readonly nuint DW_LNE_hi_user = 0xff;

// Table 39
public static readonly nuint DW_MACINFO_define = 0x01;
public static readonly nuint DW_MACINFO_undef = 0x02;
public static readonly nuint DW_MACINFO_start_file = 0x03;
public static readonly nuint DW_MACINFO_end_file = 0x04;
public static readonly nuint DW_MACINFO_vendor_ext = 0xff;

// Table 40.
 
// operand,...
public static readonly nuint DW_CFA_nop = 0x00;
public static readonly nuint DW_CFA_set_loc = 0x01; // address
public static readonly nuint DW_CFA_advance_loc1 = 0x02; // 1-byte delta
public static readonly nuint DW_CFA_advance_loc2 = 0x03; // 2-byte delta
public static readonly nuint DW_CFA_advance_loc4 = 0x04; // 4-byte delta
public static readonly nuint DW_CFA_offset_extended = 0x05; // ULEB128 register, ULEB128 offset
public static readonly nuint DW_CFA_restore_extended = 0x06; // ULEB128 register
public static readonly nuint DW_CFA_undefined = 0x07; // ULEB128 register
public static readonly nuint DW_CFA_same_value = 0x08; // ULEB128 register
public static readonly nuint DW_CFA_register = 0x09; // ULEB128 register, ULEB128 register
public static readonly nuint DW_CFA_remember_state = 0x0a;
public static readonly nuint DW_CFA_restore_state = 0x0b;

public static readonly nuint DW_CFA_def_cfa = 0x0c; // ULEB128 register, ULEB128 offset
public static readonly nuint DW_CFA_def_cfa_register = 0x0d; // ULEB128 register
public static readonly nuint DW_CFA_def_cfa_offset = 0x0e; // ULEB128 offset
public static readonly nuint DW_CFA_def_cfa_expression = 0x0f; // BLOCK
public static readonly nuint DW_CFA_expression = 0x10; // ULEB128 register, BLOCK
public static readonly nuint DW_CFA_offset_extended_sf = 0x11; // ULEB128 register, SLEB128 offset
public static readonly nuint DW_CFA_def_cfa_sf = 0x12; // ULEB128 register, SLEB128 offset
public static readonly nuint DW_CFA_def_cfa_offset_sf = 0x13; // SLEB128 offset
public static readonly nuint DW_CFA_val_offset = 0x14; // ULEB128, ULEB128
public static readonly nuint DW_CFA_val_offset_sf = 0x15; // ULEB128, SLEB128
public static readonly nuint DW_CFA_val_expression = 0x16; // ULEB128, BLOCK

public static readonly nuint DW_CFA_lo_user = 0x1c;
public static readonly nuint DW_CFA_hi_user = 0x3f; 

// Opcodes that take an addend operand.
public static readonly nuint DW_CFA_advance_loc = 0x1 << 6; // +delta
public static readonly nuint DW_CFA_offset = 0x2 << 6; // +register (ULEB128 offset)
public static readonly nuint DW_CFA_restore = 0x3 << 6; // +register

} // end dwarf_package
