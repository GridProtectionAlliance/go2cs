// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarf -- go2cs converted at 2020 August 29 08:51:32 UTC
// import "cmd/internal/dwarf" ==> using dwarf = go.cmd.@internal.dwarf_package
// Original source: C:\Go\src\cmd\internal\dwarf\dwarf_defs.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class dwarf_package
    {
        // Cut, pasted, tr-and-awk'ed from tables in
        // http://dwarfstd.org/doc/Dwarf3.pdf

        // Table 18
        public static readonly ulong DW_TAG_array_type = 0x01UL;
        public static readonly ulong DW_TAG_class_type = 0x02UL;
        public static readonly ulong DW_TAG_entry_point = 0x03UL;
        public static readonly ulong DW_TAG_enumeration_type = 0x04UL;
        public static readonly ulong DW_TAG_formal_parameter = 0x05UL;
        public static readonly ulong DW_TAG_imported_declaration = 0x08UL;
        public static readonly ulong DW_TAG_label = 0x0aUL;
        public static readonly ulong DW_TAG_lexical_block = 0x0bUL;
        public static readonly ulong DW_TAG_member = 0x0dUL;
        public static readonly ulong DW_TAG_pointer_type = 0x0fUL;
        public static readonly ulong DW_TAG_reference_type = 0x10UL;
        public static readonly ulong DW_TAG_compile_unit = 0x11UL;
        public static readonly ulong DW_TAG_string_type = 0x12UL;
        public static readonly ulong DW_TAG_structure_type = 0x13UL;
        public static readonly ulong DW_TAG_subroutine_type = 0x15UL;
        public static readonly ulong DW_TAG_typedef = 0x16UL;
        public static readonly ulong DW_TAG_union_type = 0x17UL;
        public static readonly ulong DW_TAG_unspecified_parameters = 0x18UL;
        public static readonly ulong DW_TAG_variant = 0x19UL;
        public static readonly ulong DW_TAG_common_block = 0x1aUL;
        public static readonly ulong DW_TAG_common_inclusion = 0x1bUL;
        public static readonly ulong DW_TAG_inheritance = 0x1cUL;
        public static readonly ulong DW_TAG_inlined_subroutine = 0x1dUL;
        public static readonly ulong DW_TAG_module = 0x1eUL;
        public static readonly ulong DW_TAG_ptr_to_member_type = 0x1fUL;
        public static readonly ulong DW_TAG_set_type = 0x20UL;
        public static readonly ulong DW_TAG_subrange_type = 0x21UL;
        public static readonly ulong DW_TAG_with_stmt = 0x22UL;
        public static readonly ulong DW_TAG_access_declaration = 0x23UL;
        public static readonly ulong DW_TAG_base_type = 0x24UL;
        public static readonly ulong DW_TAG_catch_block = 0x25UL;
        public static readonly ulong DW_TAG_const_type = 0x26UL;
        public static readonly ulong DW_TAG_constant = 0x27UL;
        public static readonly ulong DW_TAG_enumerator = 0x28UL;
        public static readonly ulong DW_TAG_file_type = 0x29UL;
        public static readonly ulong DW_TAG_friend = 0x2aUL;
        public static readonly ulong DW_TAG_namelist = 0x2bUL;
        public static readonly ulong DW_TAG_namelist_item = 0x2cUL;
        public static readonly ulong DW_TAG_packed_type = 0x2dUL;
        public static readonly ulong DW_TAG_subprogram = 0x2eUL;
        public static readonly ulong DW_TAG_template_type_parameter = 0x2fUL;
        public static readonly ulong DW_TAG_template_value_parameter = 0x30UL;
        public static readonly ulong DW_TAG_thrown_type = 0x31UL;
        public static readonly ulong DW_TAG_try_block = 0x32UL;
        public static readonly ulong DW_TAG_variant_part = 0x33UL;
        public static readonly ulong DW_TAG_variable = 0x34UL;
        public static readonly ulong DW_TAG_volatile_type = 0x35UL; 
        // Dwarf3
        public static readonly ulong DW_TAG_dwarf_procedure = 0x36UL;
        public static readonly ulong DW_TAG_restrict_type = 0x37UL;
        public static readonly ulong DW_TAG_interface_type = 0x38UL;
        public static readonly ulong DW_TAG_namespace = 0x39UL;
        public static readonly ulong DW_TAG_imported_module = 0x3aUL;
        public static readonly ulong DW_TAG_unspecified_type = 0x3bUL;
        public static readonly ulong DW_TAG_partial_unit = 0x3cUL;
        public static readonly ulong DW_TAG_imported_unit = 0x3dUL;
        public static readonly ulong DW_TAG_condition = 0x3fUL;
        public static readonly ulong DW_TAG_shared_type = 0x40UL; 
        // Dwarf4
        public static readonly ulong DW_TAG_type_unit = 0x41UL;
        public static readonly ulong DW_TAG_rvalue_reference_type = 0x42UL;
        public static readonly ulong DW_TAG_template_alias = 0x43UL; 

        // User defined
        public static readonly ulong DW_TAG_lo_user = 0x4080UL;
        public static readonly ulong DW_TAG_hi_user = 0xffffUL;

        // Table 19
        public static readonly ulong DW_CHILDREN_no = 0x00UL;
        public static readonly ulong DW_CHILDREN_yes = 0x01UL;

        // Not from the spec, but logically belongs here
        public static readonly ulong DW_CLS_ADDRESS = 0x01UL + iota;
        public static readonly var DW_CLS_BLOCK = 0;
        public static readonly var DW_CLS_CONSTANT = 1;
        public static readonly var DW_CLS_FLAG = 2;
        public static readonly var DW_CLS_PTR = 3; // lineptr, loclistptr, macptr, rangelistptr
        public static readonly var DW_CLS_REFERENCE = 4;
        public static readonly var DW_CLS_ADDRLOC = 5;
        public static readonly var DW_CLS_STRING = 6;

        // Table 20
        public static readonly ulong DW_AT_sibling = 0x01UL; // reference
        public static readonly ulong DW_AT_location = 0x02UL; // block, loclistptr
        public static readonly ulong DW_AT_name = 0x03UL; // string
        public static readonly ulong DW_AT_ordering = 0x09UL; // constant
        public static readonly ulong DW_AT_byte_size = 0x0bUL; // block, constant, reference
        public static readonly ulong DW_AT_bit_offset = 0x0cUL; // block, constant, reference
        public static readonly ulong DW_AT_bit_size = 0x0dUL; // block, constant, reference
        public static readonly ulong DW_AT_stmt_list = 0x10UL; // lineptr
        public static readonly ulong DW_AT_low_pc = 0x11UL; // address
        public static readonly ulong DW_AT_high_pc = 0x12UL; // address
        public static readonly ulong DW_AT_language = 0x13UL; // constant
        public static readonly ulong DW_AT_discr = 0x15UL; // reference
        public static readonly ulong DW_AT_discr_value = 0x16UL; // constant
        public static readonly ulong DW_AT_visibility = 0x17UL; // constant
        public static readonly ulong DW_AT_import = 0x18UL; // reference
        public static readonly ulong DW_AT_string_length = 0x19UL; // block, loclistptr
        public static readonly ulong DW_AT_common_reference = 0x1aUL; // reference
        public static readonly ulong DW_AT_comp_dir = 0x1bUL; // string
        public static readonly ulong DW_AT_const_value = 0x1cUL; // block, constant, string
        public static readonly ulong DW_AT_containing_type = 0x1dUL; // reference
        public static readonly ulong DW_AT_default_value = 0x1eUL; // reference
        public static readonly ulong DW_AT_inline = 0x20UL; // constant
        public static readonly ulong DW_AT_is_optional = 0x21UL; // flag
        public static readonly ulong DW_AT_lower_bound = 0x22UL; // block, constant, reference
        public static readonly ulong DW_AT_producer = 0x25UL; // string
        public static readonly ulong DW_AT_prototyped = 0x27UL; // flag
        public static readonly ulong DW_AT_return_addr = 0x2aUL; // block, loclistptr
        public static readonly ulong DW_AT_start_scope = 0x2cUL; // constant
        public static readonly ulong DW_AT_bit_stride = 0x2eUL; // constant
        public static readonly ulong DW_AT_upper_bound = 0x2fUL; // block, constant, reference
        public static readonly ulong DW_AT_abstract_origin = 0x31UL; // reference
        public static readonly ulong DW_AT_accessibility = 0x32UL; // constant
        public static readonly ulong DW_AT_address_class = 0x33UL; // constant
        public static readonly ulong DW_AT_artificial = 0x34UL; // flag
        public static readonly ulong DW_AT_base_types = 0x35UL; // reference
        public static readonly ulong DW_AT_calling_convention = 0x36UL; // constant
        public static readonly ulong DW_AT_count = 0x37UL; // block, constant, reference
        public static readonly ulong DW_AT_data_member_location = 0x38UL; // block, constant, loclistptr
        public static readonly ulong DW_AT_decl_column = 0x39UL; // constant
        public static readonly ulong DW_AT_decl_file = 0x3aUL; // constant
        public static readonly ulong DW_AT_decl_line = 0x3bUL; // constant
        public static readonly ulong DW_AT_declaration = 0x3cUL; // flag
        public static readonly ulong DW_AT_discr_list = 0x3dUL; // block
        public static readonly ulong DW_AT_encoding = 0x3eUL; // constant
        public static readonly ulong DW_AT_external = 0x3fUL; // flag
        public static readonly ulong DW_AT_frame_base = 0x40UL; // block, loclistptr
        public static readonly ulong DW_AT_friend = 0x41UL; // reference
        public static readonly ulong DW_AT_identifier_case = 0x42UL; // constant
        public static readonly ulong DW_AT_macro_info = 0x43UL; // macptr
        public static readonly ulong DW_AT_namelist_item = 0x44UL; // block
        public static readonly ulong DW_AT_priority = 0x45UL; // reference
        public static readonly ulong DW_AT_segment = 0x46UL; // block, loclistptr
        public static readonly ulong DW_AT_specification = 0x47UL; // reference
        public static readonly ulong DW_AT_static_link = 0x48UL; // block, loclistptr
        public static readonly ulong DW_AT_type = 0x49UL; // reference
        public static readonly ulong DW_AT_use_location = 0x4aUL; // block, loclistptr
        public static readonly ulong DW_AT_variable_parameter = 0x4bUL; // flag
        public static readonly ulong DW_AT_virtuality = 0x4cUL; // constant
        public static readonly ulong DW_AT_vtable_elem_location = 0x4dUL; // block, loclistptr
        // Dwarf3
        public static readonly ulong DW_AT_allocated = 0x4eUL; // block, constant, reference
        public static readonly ulong DW_AT_associated = 0x4fUL; // block, constant, reference
        public static readonly ulong DW_AT_data_location = 0x50UL; // block
        public static readonly ulong DW_AT_byte_stride = 0x51UL; // block, constant, reference
        public static readonly ulong DW_AT_entry_pc = 0x52UL; // address
        public static readonly ulong DW_AT_use_UTF8 = 0x53UL; // flag
        public static readonly ulong DW_AT_extension = 0x54UL; // reference
        public static readonly ulong DW_AT_ranges = 0x55UL; // rangelistptr
        public static readonly ulong DW_AT_trampoline = 0x56UL; // address, flag, reference, string
        public static readonly ulong DW_AT_call_column = 0x57UL; // constant
        public static readonly ulong DW_AT_call_file = 0x58UL; // constant
        public static readonly ulong DW_AT_call_line = 0x59UL; // constant
        public static readonly ulong DW_AT_description = 0x5aUL; // string
        public static readonly ulong DW_AT_binary_scale = 0x5bUL; // constant
        public static readonly ulong DW_AT_decimal_scale = 0x5cUL; // constant
        public static readonly ulong DW_AT_small = 0x5dUL; // reference
        public static readonly ulong DW_AT_decimal_sign = 0x5eUL; // constant
        public static readonly ulong DW_AT_digit_count = 0x5fUL; // constant
        public static readonly ulong DW_AT_picture_string = 0x60UL; // string
        public static readonly ulong DW_AT_mutable = 0x61UL; // flag
        public static readonly ulong DW_AT_threads_scaled = 0x62UL; // flag
        public static readonly ulong DW_AT_explicit = 0x63UL; // flag
        public static readonly ulong DW_AT_object_pointer = 0x64UL; // reference
        public static readonly ulong DW_AT_endianity = 0x65UL; // constant
        public static readonly ulong DW_AT_elemental = 0x66UL; // flag
        public static readonly ulong DW_AT_pure = 0x67UL; // flag
        public static readonly ulong DW_AT_recursive = 0x68UL; // flag

        public static readonly ulong DW_AT_lo_user = 0x2000UL; // ---
        public static readonly ulong DW_AT_hi_user = 0x3fffUL; // ---

        // Table 21
        public static readonly ulong DW_FORM_addr = 0x01UL; // address
        public static readonly ulong DW_FORM_block2 = 0x03UL; // block
        public static readonly ulong DW_FORM_block4 = 0x04UL; // block
        public static readonly ulong DW_FORM_data2 = 0x05UL; // constant
        public static readonly ulong DW_FORM_data4 = 0x06UL; // constant, lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_data8 = 0x07UL; // constant, lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_string = 0x08UL; // string
        public static readonly ulong DW_FORM_block = 0x09UL; // block
        public static readonly ulong DW_FORM_block1 = 0x0aUL; // block
        public static readonly ulong DW_FORM_data1 = 0x0bUL; // constant
        public static readonly ulong DW_FORM_flag = 0x0cUL; // flag
        public static readonly ulong DW_FORM_sdata = 0x0dUL; // constant
        public static readonly ulong DW_FORM_strp = 0x0eUL; // string
        public static readonly ulong DW_FORM_udata = 0x0fUL; // constant
        public static readonly ulong DW_FORM_ref_addr = 0x10UL; // reference
        public static readonly ulong DW_FORM_ref1 = 0x11UL; // reference
        public static readonly ulong DW_FORM_ref2 = 0x12UL; // reference
        public static readonly ulong DW_FORM_ref4 = 0x13UL; // reference
        public static readonly ulong DW_FORM_ref8 = 0x14UL; // reference
        public static readonly ulong DW_FORM_ref_udata = 0x15UL; // reference
        public static readonly ulong DW_FORM_indirect = 0x16UL; // (see Section 7.5.3)
        // Dwarf4
        public static readonly ulong DW_FORM_sec_offset = 0x17UL; // lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_exprloc = 0x18UL; // exprloc
        public static readonly ulong DW_FORM_flag_present = 0x19UL; // flag
        public static readonly ulong DW_FORM_ref_sig8 = 0x20UL; // reference

        // Table 24 (#operands, notes)
        public static readonly ulong DW_OP_addr = 0x03UL; // 1 constant address (size target specific)
        public static readonly ulong DW_OP_deref = 0x06UL; // 0
        public static readonly ulong DW_OP_const1u = 0x08UL; // 1 1-byte constant
        public static readonly ulong DW_OP_const1s = 0x09UL; // 1 1-byte constant
        public static readonly ulong DW_OP_const2u = 0x0aUL; // 1 2-byte constant
        public static readonly ulong DW_OP_const2s = 0x0bUL; // 1 2-byte constant
        public static readonly ulong DW_OP_const4u = 0x0cUL; // 1 4-byte constant
        public static readonly ulong DW_OP_const4s = 0x0dUL; // 1 4-byte constant
        public static readonly ulong DW_OP_const8u = 0x0eUL; // 1 8-byte constant
        public static readonly ulong DW_OP_const8s = 0x0fUL; // 1 8-byte constant
        public static readonly ulong DW_OP_constu = 0x10UL; // 1 ULEB128 constant
        public static readonly ulong DW_OP_consts = 0x11UL; // 1 SLEB128 constant
        public static readonly ulong DW_OP_dup = 0x12UL; // 0
        public static readonly ulong DW_OP_drop = 0x13UL; // 0
        public static readonly ulong DW_OP_over = 0x14UL; // 0
        public static readonly ulong DW_OP_pick = 0x15UL; // 1 1-byte stack index
        public static readonly ulong DW_OP_swap = 0x16UL; // 0
        public static readonly ulong DW_OP_rot = 0x17UL; // 0
        public static readonly ulong DW_OP_xderef = 0x18UL; // 0
        public static readonly ulong DW_OP_abs = 0x19UL; // 0
        public static readonly ulong DW_OP_and = 0x1aUL; // 0
        public static readonly ulong DW_OP_div = 0x1bUL; // 0
        public static readonly ulong DW_OP_minus = 0x1cUL; // 0
        public static readonly ulong DW_OP_mod = 0x1dUL; // 0
        public static readonly ulong DW_OP_mul = 0x1eUL; // 0
        public static readonly ulong DW_OP_neg = 0x1fUL; // 0
        public static readonly ulong DW_OP_not = 0x20UL; // 0
        public static readonly ulong DW_OP_or = 0x21UL; // 0
        public static readonly ulong DW_OP_plus = 0x22UL; // 0
        public static readonly ulong DW_OP_plus_uconst = 0x23UL; // 1 ULEB128 addend
        public static readonly ulong DW_OP_shl = 0x24UL; // 0
        public static readonly ulong DW_OP_shr = 0x25UL; // 0
        public static readonly ulong DW_OP_shra = 0x26UL; // 0
        public static readonly ulong DW_OP_xor = 0x27UL; // 0
        public static readonly ulong DW_OP_skip = 0x2fUL; // 1 signed 2-byte constant
        public static readonly ulong DW_OP_bra = 0x28UL; // 1 signed 2-byte constant
        public static readonly ulong DW_OP_eq = 0x29UL; // 0
        public static readonly ulong DW_OP_ge = 0x2aUL; // 0
        public static readonly ulong DW_OP_gt = 0x2bUL; // 0
        public static readonly ulong DW_OP_le = 0x2cUL; // 0
        public static readonly ulong DW_OP_lt = 0x2dUL; // 0
        public static readonly ulong DW_OP_ne = 0x2eUL; // 0
        public static readonly ulong DW_OP_lit0 = 0x30UL; // 0 ...
        public static readonly ulong DW_OP_lit31 = 0x4fUL; // 0 literals 0..31 = (DW_OP_lit0 + literal)
        public static readonly ulong DW_OP_reg0 = 0x50UL; // 0 ..
        public static readonly ulong DW_OP_reg31 = 0x6fUL; // 0 reg 0..31 = (DW_OP_reg0 + regnum)
        public static readonly ulong DW_OP_breg0 = 0x70UL; // 1 ...
        public static readonly ulong DW_OP_breg31 = 0x8fUL; // 1 SLEB128 offset base register 0..31 = (DW_OP_breg0 + regnum)
        public static readonly ulong DW_OP_regx = 0x90UL; // 1 ULEB128 register
        public static readonly ulong DW_OP_fbreg = 0x91UL; // 1 SLEB128 offset
        public static readonly ulong DW_OP_bregx = 0x92UL; // 2 ULEB128 register followed by SLEB128 offset
        public static readonly ulong DW_OP_piece = 0x93UL; // 1 ULEB128 size of piece addressed
        public static readonly ulong DW_OP_deref_size = 0x94UL; // 1 1-byte size of data retrieved
        public static readonly ulong DW_OP_xderef_size = 0x95UL; // 1 1-byte size of data retrieved
        public static readonly ulong DW_OP_nop = 0x96UL; // 0
        public static readonly ulong DW_OP_push_object_address = 0x97UL; // 0
        public static readonly ulong DW_OP_call2 = 0x98UL; // 1 2-byte offset of DIE
        public static readonly ulong DW_OP_call4 = 0x99UL; // 1 4-byte offset of DIE
        public static readonly ulong DW_OP_call_ref = 0x9aUL; // 1 4- or 8-byte offset of DIE
        public static readonly ulong DW_OP_form_tls_address = 0x9bUL; // 0
        public static readonly ulong DW_OP_call_frame_cfa = 0x9cUL; // 0
        public static readonly ulong DW_OP_bit_piece = 0x9dUL; // 2
        public static readonly ulong DW_OP_lo_user = 0xe0UL;
        public static readonly ulong DW_OP_hi_user = 0xffUL;

        // Table 25
        public static readonly ulong DW_ATE_address = 0x01UL;
        public static readonly ulong DW_ATE_boolean = 0x02UL;
        public static readonly ulong DW_ATE_complex_float = 0x03UL;
        public static readonly ulong DW_ATE_float = 0x04UL;
        public static readonly ulong DW_ATE_signed = 0x05UL;
        public static readonly ulong DW_ATE_signed_char = 0x06UL;
        public static readonly ulong DW_ATE_unsigned = 0x07UL;
        public static readonly ulong DW_ATE_unsigned_char = 0x08UL;
        public static readonly ulong DW_ATE_imaginary_float = 0x09UL;
        public static readonly ulong DW_ATE_packed_decimal = 0x0aUL;
        public static readonly ulong DW_ATE_numeric_string = 0x0bUL;
        public static readonly ulong DW_ATE_edited = 0x0cUL;
        public static readonly ulong DW_ATE_signed_fixed = 0x0dUL;
        public static readonly ulong DW_ATE_unsigned_fixed = 0x0eUL;
        public static readonly ulong DW_ATE_decimal_float = 0x0fUL;
        public static readonly ulong DW_ATE_lo_user = 0x80UL;
        public static readonly ulong DW_ATE_hi_user = 0xffUL;

        // Table 26
        public static readonly ulong DW_DS_unsigned = 0x01UL;
        public static readonly ulong DW_DS_leading_overpunch = 0x02UL;
        public static readonly ulong DW_DS_trailing_overpunch = 0x03UL;
        public static readonly ulong DW_DS_leading_separate = 0x04UL;
        public static readonly ulong DW_DS_trailing_separate = 0x05UL;

        // Table 27
        public static readonly ulong DW_END_default = 0x00UL;
        public static readonly ulong DW_END_big = 0x01UL;
        public static readonly ulong DW_END_little = 0x02UL;
        public static readonly ulong DW_END_lo_user = 0x40UL;
        public static readonly ulong DW_END_hi_user = 0xffUL;

        // Table 28
        public static readonly ulong DW_ACCESS_public = 0x01UL;
        public static readonly ulong DW_ACCESS_protected = 0x02UL;
        public static readonly ulong DW_ACCESS_private = 0x03UL;

        // Table 29
        public static readonly ulong DW_VIS_local = 0x01UL;
        public static readonly ulong DW_VIS_exported = 0x02UL;
        public static readonly ulong DW_VIS_qualified = 0x03UL;

        // Table 30
        public static readonly ulong DW_VIRTUALITY_none = 0x00UL;
        public static readonly ulong DW_VIRTUALITY_virtual = 0x01UL;
        public static readonly ulong DW_VIRTUALITY_pure_virtual = 0x02UL;

        // Table 31
        public static readonly ulong DW_LANG_C89 = 0x0001UL;
        public static readonly ulong DW_LANG_C = 0x0002UL;
        public static readonly ulong DW_LANG_Ada83 = 0x0003UL;
        public static readonly ulong DW_LANG_C_plus_plus = 0x0004UL;
        public static readonly ulong DW_LANG_Cobol74 = 0x0005UL;
        public static readonly ulong DW_LANG_Cobol85 = 0x0006UL;
        public static readonly ulong DW_LANG_Fortran77 = 0x0007UL;
        public static readonly ulong DW_LANG_Fortran90 = 0x0008UL;
        public static readonly ulong DW_LANG_Pascal83 = 0x0009UL;
        public static readonly ulong DW_LANG_Modula2 = 0x000aUL; 
        // Dwarf3
        public static readonly ulong DW_LANG_Java = 0x000bUL;
        public static readonly ulong DW_LANG_C99 = 0x000cUL;
        public static readonly ulong DW_LANG_Ada95 = 0x000dUL;
        public static readonly ulong DW_LANG_Fortran95 = 0x000eUL;
        public static readonly ulong DW_LANG_PLI = 0x000fUL;
        public static readonly ulong DW_LANG_ObjC = 0x0010UL;
        public static readonly ulong DW_LANG_ObjC_plus_plus = 0x0011UL;
        public static readonly ulong DW_LANG_UPC = 0x0012UL;
        public static readonly ulong DW_LANG_D = 0x0013UL; 
        // Dwarf4
        public static readonly ulong DW_LANG_Python = 0x0014UL; 
        // Dwarf5
        public static readonly ulong DW_LANG_Go = 0x0016UL;

        public static readonly ulong DW_LANG_lo_user = 0x8000UL;
        public static readonly ulong DW_LANG_hi_user = 0xffffUL;

        // Table 32
        public static readonly ulong DW_ID_case_sensitive = 0x00UL;
        public static readonly ulong DW_ID_up_case = 0x01UL;
        public static readonly ulong DW_ID_down_case = 0x02UL;
        public static readonly ulong DW_ID_case_insensitive = 0x03UL;

        // Table 33
        public static readonly ulong DW_CC_normal = 0x01UL;
        public static readonly ulong DW_CC_program = 0x02UL;
        public static readonly ulong DW_CC_nocall = 0x03UL;
        public static readonly ulong DW_CC_lo_user = 0x40UL;
        public static readonly ulong DW_CC_hi_user = 0xffUL;

        // Table 34
        public static readonly ulong DW_INL_not_inlined = 0x00UL;
        public static readonly ulong DW_INL_inlined = 0x01UL;
        public static readonly ulong DW_INL_declared_not_inlined = 0x02UL;
        public static readonly ulong DW_INL_declared_inlined = 0x03UL;

        // Table 35
        public static readonly ulong DW_ORD_row_major = 0x00UL;
        public static readonly ulong DW_ORD_col_major = 0x01UL;

        // Table 36
        public static readonly ulong DW_DSC_label = 0x00UL;
        public static readonly ulong DW_DSC_range = 0x01UL;

        // Table 37
        public static readonly ulong DW_LNS_copy = 0x01UL;
        public static readonly ulong DW_LNS_advance_pc = 0x02UL;
        public static readonly ulong DW_LNS_advance_line = 0x03UL;
        public static readonly ulong DW_LNS_set_file = 0x04UL;
        public static readonly ulong DW_LNS_set_column = 0x05UL;
        public static readonly ulong DW_LNS_negate_stmt = 0x06UL;
        public static readonly ulong DW_LNS_set_basic_block = 0x07UL;
        public static readonly ulong DW_LNS_const_add_pc = 0x08UL;
        public static readonly ulong DW_LNS_fixed_advance_pc = 0x09UL; 
        // Dwarf3
        public static readonly ulong DW_LNS_set_prologue_end = 0x0aUL;
        public static readonly ulong DW_LNS_set_epilogue_begin = 0x0bUL;
        public static readonly ulong DW_LNS_set_isa = 0x0cUL;

        // Table 38
        public static readonly ulong DW_LNE_end_sequence = 0x01UL;
        public static readonly ulong DW_LNE_set_address = 0x02UL;
        public static readonly ulong DW_LNE_define_file = 0x03UL;
        public static readonly ulong DW_LNE_lo_user = 0x80UL;
        public static readonly ulong DW_LNE_hi_user = 0xffUL;

        // Table 39
        public static readonly ulong DW_MACINFO_define = 0x01UL;
        public static readonly ulong DW_MACINFO_undef = 0x02UL;
        public static readonly ulong DW_MACINFO_start_file = 0x03UL;
        public static readonly ulong DW_MACINFO_end_file = 0x04UL;
        public static readonly ulong DW_MACINFO_vendor_ext = 0xffUL;

        // Table 40.
 
        // operand,...
        public static readonly ulong DW_CFA_nop = 0x00UL;
        public static readonly ulong DW_CFA_set_loc = 0x01UL; // address
        public static readonly ulong DW_CFA_advance_loc1 = 0x02UL; // 1-byte delta
        public static readonly ulong DW_CFA_advance_loc2 = 0x03UL; // 2-byte delta
        public static readonly ulong DW_CFA_advance_loc4 = 0x04UL; // 4-byte delta
        public static readonly ulong DW_CFA_offset_extended = 0x05UL; // ULEB128 register, ULEB128 offset
        public static readonly ulong DW_CFA_restore_extended = 0x06UL; // ULEB128 register
        public static readonly ulong DW_CFA_undefined = 0x07UL; // ULEB128 register
        public static readonly ulong DW_CFA_same_value = 0x08UL; // ULEB128 register
        public static readonly ulong DW_CFA_register = 0x09UL; // ULEB128 register, ULEB128 register
        public static readonly ulong DW_CFA_remember_state = 0x0aUL;
        public static readonly ulong DW_CFA_restore_state = 0x0bUL;

        public static readonly ulong DW_CFA_def_cfa = 0x0cUL; // ULEB128 register, ULEB128 offset
        public static readonly ulong DW_CFA_def_cfa_register = 0x0dUL; // ULEB128 register
        public static readonly ulong DW_CFA_def_cfa_offset = 0x0eUL; // ULEB128 offset
        public static readonly ulong DW_CFA_def_cfa_expression = 0x0fUL; // BLOCK
        public static readonly ulong DW_CFA_expression = 0x10UL; // ULEB128 register, BLOCK
        public static readonly ulong DW_CFA_offset_extended_sf = 0x11UL; // ULEB128 register, SLEB128 offset
        public static readonly ulong DW_CFA_def_cfa_sf = 0x12UL; // ULEB128 register, SLEB128 offset
        public static readonly ulong DW_CFA_def_cfa_offset_sf = 0x13UL; // SLEB128 offset
        public static readonly ulong DW_CFA_val_offset = 0x14UL; // ULEB128, ULEB128
        public static readonly ulong DW_CFA_val_offset_sf = 0x15UL; // ULEB128, SLEB128
        public static readonly ulong DW_CFA_val_expression = 0x16UL; // ULEB128, BLOCK

        public static readonly ulong DW_CFA_lo_user = 0x1cUL;
        public static readonly ulong DW_CFA_hi_user = 0x3fUL; 

        // Opcodes that take an addend operand.
        public static readonly ulong DW_CFA_advance_loc = 0x1UL << (int)(6L); // +delta
        public static readonly ulong DW_CFA_offset = 0x2UL << (int)(6L); // +register (ULEB128 offset)
        public static readonly ulong DW_CFA_restore = 0x3UL << (int)(6L); // +register
    }
}}}
