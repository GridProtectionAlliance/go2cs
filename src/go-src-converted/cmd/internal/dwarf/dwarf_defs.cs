// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarf -- go2cs converted at 2020 October 08 04:07:48 UTC
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
        public static readonly ulong DW_TAG_array_type = (ulong)0x01UL;
        public static readonly ulong DW_TAG_class_type = (ulong)0x02UL;
        public static readonly ulong DW_TAG_entry_point = (ulong)0x03UL;
        public static readonly ulong DW_TAG_enumeration_type = (ulong)0x04UL;
        public static readonly ulong DW_TAG_formal_parameter = (ulong)0x05UL;
        public static readonly ulong DW_TAG_imported_declaration = (ulong)0x08UL;
        public static readonly ulong DW_TAG_label = (ulong)0x0aUL;
        public static readonly ulong DW_TAG_lexical_block = (ulong)0x0bUL;
        public static readonly ulong DW_TAG_member = (ulong)0x0dUL;
        public static readonly ulong DW_TAG_pointer_type = (ulong)0x0fUL;
        public static readonly ulong DW_TAG_reference_type = (ulong)0x10UL;
        public static readonly ulong DW_TAG_compile_unit = (ulong)0x11UL;
        public static readonly ulong DW_TAG_string_type = (ulong)0x12UL;
        public static readonly ulong DW_TAG_structure_type = (ulong)0x13UL;
        public static readonly ulong DW_TAG_subroutine_type = (ulong)0x15UL;
        public static readonly ulong DW_TAG_typedef = (ulong)0x16UL;
        public static readonly ulong DW_TAG_union_type = (ulong)0x17UL;
        public static readonly ulong DW_TAG_unspecified_parameters = (ulong)0x18UL;
        public static readonly ulong DW_TAG_variant = (ulong)0x19UL;
        public static readonly ulong DW_TAG_common_block = (ulong)0x1aUL;
        public static readonly ulong DW_TAG_common_inclusion = (ulong)0x1bUL;
        public static readonly ulong DW_TAG_inheritance = (ulong)0x1cUL;
        public static readonly ulong DW_TAG_inlined_subroutine = (ulong)0x1dUL;
        public static readonly ulong DW_TAG_module = (ulong)0x1eUL;
        public static readonly ulong DW_TAG_ptr_to_member_type = (ulong)0x1fUL;
        public static readonly ulong DW_TAG_set_type = (ulong)0x20UL;
        public static readonly ulong DW_TAG_subrange_type = (ulong)0x21UL;
        public static readonly ulong DW_TAG_with_stmt = (ulong)0x22UL;
        public static readonly ulong DW_TAG_access_declaration = (ulong)0x23UL;
        public static readonly ulong DW_TAG_base_type = (ulong)0x24UL;
        public static readonly ulong DW_TAG_catch_block = (ulong)0x25UL;
        public static readonly ulong DW_TAG_const_type = (ulong)0x26UL;
        public static readonly ulong DW_TAG_constant = (ulong)0x27UL;
        public static readonly ulong DW_TAG_enumerator = (ulong)0x28UL;
        public static readonly ulong DW_TAG_file_type = (ulong)0x29UL;
        public static readonly ulong DW_TAG_friend = (ulong)0x2aUL;
        public static readonly ulong DW_TAG_namelist = (ulong)0x2bUL;
        public static readonly ulong DW_TAG_namelist_item = (ulong)0x2cUL;
        public static readonly ulong DW_TAG_packed_type = (ulong)0x2dUL;
        public static readonly ulong DW_TAG_subprogram = (ulong)0x2eUL;
        public static readonly ulong DW_TAG_template_type_parameter = (ulong)0x2fUL;
        public static readonly ulong DW_TAG_template_value_parameter = (ulong)0x30UL;
        public static readonly ulong DW_TAG_thrown_type = (ulong)0x31UL;
        public static readonly ulong DW_TAG_try_block = (ulong)0x32UL;
        public static readonly ulong DW_TAG_variant_part = (ulong)0x33UL;
        public static readonly ulong DW_TAG_variable = (ulong)0x34UL;
        public static readonly ulong DW_TAG_volatile_type = (ulong)0x35UL; 
        // Dwarf3
        public static readonly ulong DW_TAG_dwarf_procedure = (ulong)0x36UL;
        public static readonly ulong DW_TAG_restrict_type = (ulong)0x37UL;
        public static readonly ulong DW_TAG_interface_type = (ulong)0x38UL;
        public static readonly ulong DW_TAG_namespace = (ulong)0x39UL;
        public static readonly ulong DW_TAG_imported_module = (ulong)0x3aUL;
        public static readonly ulong DW_TAG_unspecified_type = (ulong)0x3bUL;
        public static readonly ulong DW_TAG_partial_unit = (ulong)0x3cUL;
        public static readonly ulong DW_TAG_imported_unit = (ulong)0x3dUL;
        public static readonly ulong DW_TAG_condition = (ulong)0x3fUL;
        public static readonly ulong DW_TAG_shared_type = (ulong)0x40UL; 
        // Dwarf4
        public static readonly ulong DW_TAG_type_unit = (ulong)0x41UL;
        public static readonly ulong DW_TAG_rvalue_reference_type = (ulong)0x42UL;
        public static readonly ulong DW_TAG_template_alias = (ulong)0x43UL; 

        // User defined
        public static readonly ulong DW_TAG_lo_user = (ulong)0x4080UL;
        public static readonly ulong DW_TAG_hi_user = (ulong)0xffffUL;


        // Table 19
        public static readonly ulong DW_CHILDREN_no = (ulong)0x00UL;
        public static readonly ulong DW_CHILDREN_yes = (ulong)0x01UL;


        // Not from the spec, but logically belongs here
        public static readonly ulong DW_CLS_ADDRESS = (ulong)0x01UL + iota;
        public static readonly var DW_CLS_BLOCK = (var)0;
        public static readonly var DW_CLS_CONSTANT = (var)1;
        public static readonly var DW_CLS_FLAG = (var)2;
        public static readonly var DW_CLS_PTR = (var)3; // lineptr, loclistptr, macptr, rangelistptr
        public static readonly var DW_CLS_REFERENCE = (var)4;
        public static readonly var DW_CLS_ADDRLOC = (var)5;
        public static readonly var DW_CLS_STRING = (var)6; 

        // Go-specific internal hackery.
        public static readonly var DW_CLS_GO_TYPEREF = (var)7;


        // Table 20
        public static readonly ulong DW_AT_sibling = (ulong)0x01UL; // reference
        public static readonly ulong DW_AT_location = (ulong)0x02UL; // block, loclistptr
        public static readonly ulong DW_AT_name = (ulong)0x03UL; // string
        public static readonly ulong DW_AT_ordering = (ulong)0x09UL; // constant
        public static readonly ulong DW_AT_byte_size = (ulong)0x0bUL; // block, constant, reference
        public static readonly ulong DW_AT_bit_offset = (ulong)0x0cUL; // block, constant, reference
        public static readonly ulong DW_AT_bit_size = (ulong)0x0dUL; // block, constant, reference
        public static readonly ulong DW_AT_stmt_list = (ulong)0x10UL; // lineptr
        public static readonly ulong DW_AT_low_pc = (ulong)0x11UL; // address
        public static readonly ulong DW_AT_high_pc = (ulong)0x12UL; // address
        public static readonly ulong DW_AT_language = (ulong)0x13UL; // constant
        public static readonly ulong DW_AT_discr = (ulong)0x15UL; // reference
        public static readonly ulong DW_AT_discr_value = (ulong)0x16UL; // constant
        public static readonly ulong DW_AT_visibility = (ulong)0x17UL; // constant
        public static readonly ulong DW_AT_import = (ulong)0x18UL; // reference
        public static readonly ulong DW_AT_string_length = (ulong)0x19UL; // block, loclistptr
        public static readonly ulong DW_AT_common_reference = (ulong)0x1aUL; // reference
        public static readonly ulong DW_AT_comp_dir = (ulong)0x1bUL; // string
        public static readonly ulong DW_AT_const_value = (ulong)0x1cUL; // block, constant, string
        public static readonly ulong DW_AT_containing_type = (ulong)0x1dUL; // reference
        public static readonly ulong DW_AT_default_value = (ulong)0x1eUL; // reference
        public static readonly ulong DW_AT_inline = (ulong)0x20UL; // constant
        public static readonly ulong DW_AT_is_optional = (ulong)0x21UL; // flag
        public static readonly ulong DW_AT_lower_bound = (ulong)0x22UL; // block, constant, reference
        public static readonly ulong DW_AT_producer = (ulong)0x25UL; // string
        public static readonly ulong DW_AT_prototyped = (ulong)0x27UL; // flag
        public static readonly ulong DW_AT_return_addr = (ulong)0x2aUL; // block, loclistptr
        public static readonly ulong DW_AT_start_scope = (ulong)0x2cUL; // constant
        public static readonly ulong DW_AT_bit_stride = (ulong)0x2eUL; // constant
        public static readonly ulong DW_AT_upper_bound = (ulong)0x2fUL; // block, constant, reference
        public static readonly ulong DW_AT_abstract_origin = (ulong)0x31UL; // reference
        public static readonly ulong DW_AT_accessibility = (ulong)0x32UL; // constant
        public static readonly ulong DW_AT_address_class = (ulong)0x33UL; // constant
        public static readonly ulong DW_AT_artificial = (ulong)0x34UL; // flag
        public static readonly ulong DW_AT_base_types = (ulong)0x35UL; // reference
        public static readonly ulong DW_AT_calling_convention = (ulong)0x36UL; // constant
        public static readonly ulong DW_AT_count = (ulong)0x37UL; // block, constant, reference
        public static readonly ulong DW_AT_data_member_location = (ulong)0x38UL; // block, constant, loclistptr
        public static readonly ulong DW_AT_decl_column = (ulong)0x39UL; // constant
        public static readonly ulong DW_AT_decl_file = (ulong)0x3aUL; // constant
        public static readonly ulong DW_AT_decl_line = (ulong)0x3bUL; // constant
        public static readonly ulong DW_AT_declaration = (ulong)0x3cUL; // flag
        public static readonly ulong DW_AT_discr_list = (ulong)0x3dUL; // block
        public static readonly ulong DW_AT_encoding = (ulong)0x3eUL; // constant
        public static readonly ulong DW_AT_external = (ulong)0x3fUL; // flag
        public static readonly ulong DW_AT_frame_base = (ulong)0x40UL; // block, loclistptr
        public static readonly ulong DW_AT_friend = (ulong)0x41UL; // reference
        public static readonly ulong DW_AT_identifier_case = (ulong)0x42UL; // constant
        public static readonly ulong DW_AT_macro_info = (ulong)0x43UL; // macptr
        public static readonly ulong DW_AT_namelist_item = (ulong)0x44UL; // block
        public static readonly ulong DW_AT_priority = (ulong)0x45UL; // reference
        public static readonly ulong DW_AT_segment = (ulong)0x46UL; // block, loclistptr
        public static readonly ulong DW_AT_specification = (ulong)0x47UL; // reference
        public static readonly ulong DW_AT_static_link = (ulong)0x48UL; // block, loclistptr
        public static readonly ulong DW_AT_type = (ulong)0x49UL; // reference
        public static readonly ulong DW_AT_use_location = (ulong)0x4aUL; // block, loclistptr
        public static readonly ulong DW_AT_variable_parameter = (ulong)0x4bUL; // flag
        public static readonly ulong DW_AT_virtuality = (ulong)0x4cUL; // constant
        public static readonly ulong DW_AT_vtable_elem_location = (ulong)0x4dUL; // block, loclistptr
        // Dwarf3
        public static readonly ulong DW_AT_allocated = (ulong)0x4eUL; // block, constant, reference
        public static readonly ulong DW_AT_associated = (ulong)0x4fUL; // block, constant, reference
        public static readonly ulong DW_AT_data_location = (ulong)0x50UL; // block
        public static readonly ulong DW_AT_byte_stride = (ulong)0x51UL; // block, constant, reference
        public static readonly ulong DW_AT_entry_pc = (ulong)0x52UL; // address
        public static readonly ulong DW_AT_use_UTF8 = (ulong)0x53UL; // flag
        public static readonly ulong DW_AT_extension = (ulong)0x54UL; // reference
        public static readonly ulong DW_AT_ranges = (ulong)0x55UL; // rangelistptr
        public static readonly ulong DW_AT_trampoline = (ulong)0x56UL; // address, flag, reference, string
        public static readonly ulong DW_AT_call_column = (ulong)0x57UL; // constant
        public static readonly ulong DW_AT_call_file = (ulong)0x58UL; // constant
        public static readonly ulong DW_AT_call_line = (ulong)0x59UL; // constant
        public static readonly ulong DW_AT_description = (ulong)0x5aUL; // string
        public static readonly ulong DW_AT_binary_scale = (ulong)0x5bUL; // constant
        public static readonly ulong DW_AT_decimal_scale = (ulong)0x5cUL; // constant
        public static readonly ulong DW_AT_small = (ulong)0x5dUL; // reference
        public static readonly ulong DW_AT_decimal_sign = (ulong)0x5eUL; // constant
        public static readonly ulong DW_AT_digit_count = (ulong)0x5fUL; // constant
        public static readonly ulong DW_AT_picture_string = (ulong)0x60UL; // string
        public static readonly ulong DW_AT_mutable = (ulong)0x61UL; // flag
        public static readonly ulong DW_AT_threads_scaled = (ulong)0x62UL; // flag
        public static readonly ulong DW_AT_explicit = (ulong)0x63UL; // flag
        public static readonly ulong DW_AT_object_pointer = (ulong)0x64UL; // reference
        public static readonly ulong DW_AT_endianity = (ulong)0x65UL; // constant
        public static readonly ulong DW_AT_elemental = (ulong)0x66UL; // flag
        public static readonly ulong DW_AT_pure = (ulong)0x67UL; // flag
        public static readonly ulong DW_AT_recursive = (ulong)0x68UL; // flag

        public static readonly ulong DW_AT_lo_user = (ulong)0x2000UL; // ---
        public static readonly ulong DW_AT_hi_user = (ulong)0x3fffUL; // ---

        // Table 21
        public static readonly ulong DW_FORM_addr = (ulong)0x01UL; // address
        public static readonly ulong DW_FORM_block2 = (ulong)0x03UL; // block
        public static readonly ulong DW_FORM_block4 = (ulong)0x04UL; // block
        public static readonly ulong DW_FORM_data2 = (ulong)0x05UL; // constant
        public static readonly ulong DW_FORM_data4 = (ulong)0x06UL; // constant, lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_data8 = (ulong)0x07UL; // constant, lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_string = (ulong)0x08UL; // string
        public static readonly ulong DW_FORM_block = (ulong)0x09UL; // block
        public static readonly ulong DW_FORM_block1 = (ulong)0x0aUL; // block
        public static readonly ulong DW_FORM_data1 = (ulong)0x0bUL; // constant
        public static readonly ulong DW_FORM_flag = (ulong)0x0cUL; // flag
        public static readonly ulong DW_FORM_sdata = (ulong)0x0dUL; // constant
        public static readonly ulong DW_FORM_strp = (ulong)0x0eUL; // string
        public static readonly ulong DW_FORM_udata = (ulong)0x0fUL; // constant
        public static readonly ulong DW_FORM_ref_addr = (ulong)0x10UL; // reference
        public static readonly ulong DW_FORM_ref1 = (ulong)0x11UL; // reference
        public static readonly ulong DW_FORM_ref2 = (ulong)0x12UL; // reference
        public static readonly ulong DW_FORM_ref4 = (ulong)0x13UL; // reference
        public static readonly ulong DW_FORM_ref8 = (ulong)0x14UL; // reference
        public static readonly ulong DW_FORM_ref_udata = (ulong)0x15UL; // reference
        public static readonly ulong DW_FORM_indirect = (ulong)0x16UL; // (see Section 7.5.3)
        // Dwarf4
        public static readonly ulong DW_FORM_sec_offset = (ulong)0x17UL; // lineptr, loclistptr, macptr, rangelistptr
        public static readonly ulong DW_FORM_exprloc = (ulong)0x18UL; // exprloc
        public static readonly ulong DW_FORM_flag_present = (ulong)0x19UL; // flag
        public static readonly ulong DW_FORM_ref_sig8 = (ulong)0x20UL; // reference
        // Pseudo-form: expanded to data4 on IOS, udata elsewhere.
        public static readonly ulong DW_FORM_udata_pseudo = (ulong)0x99UL;


        // Table 24 (#operands, notes)
        public static readonly ulong DW_OP_addr = (ulong)0x03UL; // 1 constant address (size target specific)
        public static readonly ulong DW_OP_deref = (ulong)0x06UL; // 0
        public static readonly ulong DW_OP_const1u = (ulong)0x08UL; // 1 1-byte constant
        public static readonly ulong DW_OP_const1s = (ulong)0x09UL; // 1 1-byte constant
        public static readonly ulong DW_OP_const2u = (ulong)0x0aUL; // 1 2-byte constant
        public static readonly ulong DW_OP_const2s = (ulong)0x0bUL; // 1 2-byte constant
        public static readonly ulong DW_OP_const4u = (ulong)0x0cUL; // 1 4-byte constant
        public static readonly ulong DW_OP_const4s = (ulong)0x0dUL; // 1 4-byte constant
        public static readonly ulong DW_OP_const8u = (ulong)0x0eUL; // 1 8-byte constant
        public static readonly ulong DW_OP_const8s = (ulong)0x0fUL; // 1 8-byte constant
        public static readonly ulong DW_OP_constu = (ulong)0x10UL; // 1 ULEB128 constant
        public static readonly ulong DW_OP_consts = (ulong)0x11UL; // 1 SLEB128 constant
        public static readonly ulong DW_OP_dup = (ulong)0x12UL; // 0
        public static readonly ulong DW_OP_drop = (ulong)0x13UL; // 0
        public static readonly ulong DW_OP_over = (ulong)0x14UL; // 0
        public static readonly ulong DW_OP_pick = (ulong)0x15UL; // 1 1-byte stack index
        public static readonly ulong DW_OP_swap = (ulong)0x16UL; // 0
        public static readonly ulong DW_OP_rot = (ulong)0x17UL; // 0
        public static readonly ulong DW_OP_xderef = (ulong)0x18UL; // 0
        public static readonly ulong DW_OP_abs = (ulong)0x19UL; // 0
        public static readonly ulong DW_OP_and = (ulong)0x1aUL; // 0
        public static readonly ulong DW_OP_div = (ulong)0x1bUL; // 0
        public static readonly ulong DW_OP_minus = (ulong)0x1cUL; // 0
        public static readonly ulong DW_OP_mod = (ulong)0x1dUL; // 0
        public static readonly ulong DW_OP_mul = (ulong)0x1eUL; // 0
        public static readonly ulong DW_OP_neg = (ulong)0x1fUL; // 0
        public static readonly ulong DW_OP_not = (ulong)0x20UL; // 0
        public static readonly ulong DW_OP_or = (ulong)0x21UL; // 0
        public static readonly ulong DW_OP_plus = (ulong)0x22UL; // 0
        public static readonly ulong DW_OP_plus_uconst = (ulong)0x23UL; // 1 ULEB128 addend
        public static readonly ulong DW_OP_shl = (ulong)0x24UL; // 0
        public static readonly ulong DW_OP_shr = (ulong)0x25UL; // 0
        public static readonly ulong DW_OP_shra = (ulong)0x26UL; // 0
        public static readonly ulong DW_OP_xor = (ulong)0x27UL; // 0
        public static readonly ulong DW_OP_skip = (ulong)0x2fUL; // 1 signed 2-byte constant
        public static readonly ulong DW_OP_bra = (ulong)0x28UL; // 1 signed 2-byte constant
        public static readonly ulong DW_OP_eq = (ulong)0x29UL; // 0
        public static readonly ulong DW_OP_ge = (ulong)0x2aUL; // 0
        public static readonly ulong DW_OP_gt = (ulong)0x2bUL; // 0
        public static readonly ulong DW_OP_le = (ulong)0x2cUL; // 0
        public static readonly ulong DW_OP_lt = (ulong)0x2dUL; // 0
        public static readonly ulong DW_OP_ne = (ulong)0x2eUL; // 0
        public static readonly ulong DW_OP_lit0 = (ulong)0x30UL; // 0 ...
        public static readonly ulong DW_OP_lit31 = (ulong)0x4fUL; // 0 literals 0..31 = (DW_OP_lit0 + literal)
        public static readonly ulong DW_OP_reg0 = (ulong)0x50UL; // 0 ..
        public static readonly ulong DW_OP_reg31 = (ulong)0x6fUL; // 0 reg 0..31 = (DW_OP_reg0 + regnum)
        public static readonly ulong DW_OP_breg0 = (ulong)0x70UL; // 1 ...
        public static readonly ulong DW_OP_breg31 = (ulong)0x8fUL; // 1 SLEB128 offset base register 0..31 = (DW_OP_breg0 + regnum)
        public static readonly ulong DW_OP_regx = (ulong)0x90UL; // 1 ULEB128 register
        public static readonly ulong DW_OP_fbreg = (ulong)0x91UL; // 1 SLEB128 offset
        public static readonly ulong DW_OP_bregx = (ulong)0x92UL; // 2 ULEB128 register followed by SLEB128 offset
        public static readonly ulong DW_OP_piece = (ulong)0x93UL; // 1 ULEB128 size of piece addressed
        public static readonly ulong DW_OP_deref_size = (ulong)0x94UL; // 1 1-byte size of data retrieved
        public static readonly ulong DW_OP_xderef_size = (ulong)0x95UL; // 1 1-byte size of data retrieved
        public static readonly ulong DW_OP_nop = (ulong)0x96UL; // 0
        public static readonly ulong DW_OP_push_object_address = (ulong)0x97UL; // 0
        public static readonly ulong DW_OP_call2 = (ulong)0x98UL; // 1 2-byte offset of DIE
        public static readonly ulong DW_OP_call4 = (ulong)0x99UL; // 1 4-byte offset of DIE
        public static readonly ulong DW_OP_call_ref = (ulong)0x9aUL; // 1 4- or 8-byte offset of DIE
        public static readonly ulong DW_OP_form_tls_address = (ulong)0x9bUL; // 0
        public static readonly ulong DW_OP_call_frame_cfa = (ulong)0x9cUL; // 0
        public static readonly ulong DW_OP_bit_piece = (ulong)0x9dUL; // 2
        public static readonly ulong DW_OP_lo_user = (ulong)0xe0UL;
        public static readonly ulong DW_OP_hi_user = (ulong)0xffUL;


        // Table 25
        public static readonly ulong DW_ATE_address = (ulong)0x01UL;
        public static readonly ulong DW_ATE_boolean = (ulong)0x02UL;
        public static readonly ulong DW_ATE_complex_float = (ulong)0x03UL;
        public static readonly ulong DW_ATE_float = (ulong)0x04UL;
        public static readonly ulong DW_ATE_signed = (ulong)0x05UL;
        public static readonly ulong DW_ATE_signed_char = (ulong)0x06UL;
        public static readonly ulong DW_ATE_unsigned = (ulong)0x07UL;
        public static readonly ulong DW_ATE_unsigned_char = (ulong)0x08UL;
        public static readonly ulong DW_ATE_imaginary_float = (ulong)0x09UL;
        public static readonly ulong DW_ATE_packed_decimal = (ulong)0x0aUL;
        public static readonly ulong DW_ATE_numeric_string = (ulong)0x0bUL;
        public static readonly ulong DW_ATE_edited = (ulong)0x0cUL;
        public static readonly ulong DW_ATE_signed_fixed = (ulong)0x0dUL;
        public static readonly ulong DW_ATE_unsigned_fixed = (ulong)0x0eUL;
        public static readonly ulong DW_ATE_decimal_float = (ulong)0x0fUL;
        public static readonly ulong DW_ATE_lo_user = (ulong)0x80UL;
        public static readonly ulong DW_ATE_hi_user = (ulong)0xffUL;


        // Table 26
        public static readonly ulong DW_DS_unsigned = (ulong)0x01UL;
        public static readonly ulong DW_DS_leading_overpunch = (ulong)0x02UL;
        public static readonly ulong DW_DS_trailing_overpunch = (ulong)0x03UL;
        public static readonly ulong DW_DS_leading_separate = (ulong)0x04UL;
        public static readonly ulong DW_DS_trailing_separate = (ulong)0x05UL;


        // Table 27
        public static readonly ulong DW_END_default = (ulong)0x00UL;
        public static readonly ulong DW_END_big = (ulong)0x01UL;
        public static readonly ulong DW_END_little = (ulong)0x02UL;
        public static readonly ulong DW_END_lo_user = (ulong)0x40UL;
        public static readonly ulong DW_END_hi_user = (ulong)0xffUL;


        // Table 28
        public static readonly ulong DW_ACCESS_public = (ulong)0x01UL;
        public static readonly ulong DW_ACCESS_protected = (ulong)0x02UL;
        public static readonly ulong DW_ACCESS_private = (ulong)0x03UL;


        // Table 29
        public static readonly ulong DW_VIS_local = (ulong)0x01UL;
        public static readonly ulong DW_VIS_exported = (ulong)0x02UL;
        public static readonly ulong DW_VIS_qualified = (ulong)0x03UL;


        // Table 30
        public static readonly ulong DW_VIRTUALITY_none = (ulong)0x00UL;
        public static readonly ulong DW_VIRTUALITY_virtual = (ulong)0x01UL;
        public static readonly ulong DW_VIRTUALITY_pure_virtual = (ulong)0x02UL;


        // Table 31
        public static readonly ulong DW_LANG_C89 = (ulong)0x0001UL;
        public static readonly ulong DW_LANG_C = (ulong)0x0002UL;
        public static readonly ulong DW_LANG_Ada83 = (ulong)0x0003UL;
        public static readonly ulong DW_LANG_C_plus_plus = (ulong)0x0004UL;
        public static readonly ulong DW_LANG_Cobol74 = (ulong)0x0005UL;
        public static readonly ulong DW_LANG_Cobol85 = (ulong)0x0006UL;
        public static readonly ulong DW_LANG_Fortran77 = (ulong)0x0007UL;
        public static readonly ulong DW_LANG_Fortran90 = (ulong)0x0008UL;
        public static readonly ulong DW_LANG_Pascal83 = (ulong)0x0009UL;
        public static readonly ulong DW_LANG_Modula2 = (ulong)0x000aUL; 
        // Dwarf3
        public static readonly ulong DW_LANG_Java = (ulong)0x000bUL;
        public static readonly ulong DW_LANG_C99 = (ulong)0x000cUL;
        public static readonly ulong DW_LANG_Ada95 = (ulong)0x000dUL;
        public static readonly ulong DW_LANG_Fortran95 = (ulong)0x000eUL;
        public static readonly ulong DW_LANG_PLI = (ulong)0x000fUL;
        public static readonly ulong DW_LANG_ObjC = (ulong)0x0010UL;
        public static readonly ulong DW_LANG_ObjC_plus_plus = (ulong)0x0011UL;
        public static readonly ulong DW_LANG_UPC = (ulong)0x0012UL;
        public static readonly ulong DW_LANG_D = (ulong)0x0013UL; 
        // Dwarf4
        public static readonly ulong DW_LANG_Python = (ulong)0x0014UL; 
        // Dwarf5
        public static readonly ulong DW_LANG_Go = (ulong)0x0016UL;

        public static readonly ulong DW_LANG_lo_user = (ulong)0x8000UL;
        public static readonly ulong DW_LANG_hi_user = (ulong)0xffffUL;


        // Table 32
        public static readonly ulong DW_ID_case_sensitive = (ulong)0x00UL;
        public static readonly ulong DW_ID_up_case = (ulong)0x01UL;
        public static readonly ulong DW_ID_down_case = (ulong)0x02UL;
        public static readonly ulong DW_ID_case_insensitive = (ulong)0x03UL;


        // Table 33
        public static readonly ulong DW_CC_normal = (ulong)0x01UL;
        public static readonly ulong DW_CC_program = (ulong)0x02UL;
        public static readonly ulong DW_CC_nocall = (ulong)0x03UL;
        public static readonly ulong DW_CC_lo_user = (ulong)0x40UL;
        public static readonly ulong DW_CC_hi_user = (ulong)0xffUL;


        // Table 34
        public static readonly ulong DW_INL_not_inlined = (ulong)0x00UL;
        public static readonly ulong DW_INL_inlined = (ulong)0x01UL;
        public static readonly ulong DW_INL_declared_not_inlined = (ulong)0x02UL;
        public static readonly ulong DW_INL_declared_inlined = (ulong)0x03UL;


        // Table 35
        public static readonly ulong DW_ORD_row_major = (ulong)0x00UL;
        public static readonly ulong DW_ORD_col_major = (ulong)0x01UL;


        // Table 36
        public static readonly ulong DW_DSC_label = (ulong)0x00UL;
        public static readonly ulong DW_DSC_range = (ulong)0x01UL;


        // Table 37
        public static readonly ulong DW_LNS_copy = (ulong)0x01UL;
        public static readonly ulong DW_LNS_advance_pc = (ulong)0x02UL;
        public static readonly ulong DW_LNS_advance_line = (ulong)0x03UL;
        public static readonly ulong DW_LNS_set_file = (ulong)0x04UL;
        public static readonly ulong DW_LNS_set_column = (ulong)0x05UL;
        public static readonly ulong DW_LNS_negate_stmt = (ulong)0x06UL;
        public static readonly ulong DW_LNS_set_basic_block = (ulong)0x07UL;
        public static readonly ulong DW_LNS_const_add_pc = (ulong)0x08UL;
        public static readonly ulong DW_LNS_fixed_advance_pc = (ulong)0x09UL; 
        // Dwarf3
        public static readonly ulong DW_LNS_set_prologue_end = (ulong)0x0aUL;
        public static readonly ulong DW_LNS_set_epilogue_begin = (ulong)0x0bUL;
        public static readonly ulong DW_LNS_set_isa = (ulong)0x0cUL;


        // Table 38
        public static readonly ulong DW_LNE_end_sequence = (ulong)0x01UL;
        public static readonly ulong DW_LNE_set_address = (ulong)0x02UL;
        public static readonly ulong DW_LNE_define_file = (ulong)0x03UL;
        public static readonly ulong DW_LNE_lo_user = (ulong)0x80UL;
        public static readonly ulong DW_LNE_hi_user = (ulong)0xffUL;


        // Table 39
        public static readonly ulong DW_MACINFO_define = (ulong)0x01UL;
        public static readonly ulong DW_MACINFO_undef = (ulong)0x02UL;
        public static readonly ulong DW_MACINFO_start_file = (ulong)0x03UL;
        public static readonly ulong DW_MACINFO_end_file = (ulong)0x04UL;
        public static readonly ulong DW_MACINFO_vendor_ext = (ulong)0xffUL;


        // Table 40.
 
        // operand,...
        public static readonly ulong DW_CFA_nop = (ulong)0x00UL;
        public static readonly ulong DW_CFA_set_loc = (ulong)0x01UL; // address
        public static readonly ulong DW_CFA_advance_loc1 = (ulong)0x02UL; // 1-byte delta
        public static readonly ulong DW_CFA_advance_loc2 = (ulong)0x03UL; // 2-byte delta
        public static readonly ulong DW_CFA_advance_loc4 = (ulong)0x04UL; // 4-byte delta
        public static readonly ulong DW_CFA_offset_extended = (ulong)0x05UL; // ULEB128 register, ULEB128 offset
        public static readonly ulong DW_CFA_restore_extended = (ulong)0x06UL; // ULEB128 register
        public static readonly ulong DW_CFA_undefined = (ulong)0x07UL; // ULEB128 register
        public static readonly ulong DW_CFA_same_value = (ulong)0x08UL; // ULEB128 register
        public static readonly ulong DW_CFA_register = (ulong)0x09UL; // ULEB128 register, ULEB128 register
        public static readonly ulong DW_CFA_remember_state = (ulong)0x0aUL;
        public static readonly ulong DW_CFA_restore_state = (ulong)0x0bUL;

        public static readonly ulong DW_CFA_def_cfa = (ulong)0x0cUL; // ULEB128 register, ULEB128 offset
        public static readonly ulong DW_CFA_def_cfa_register = (ulong)0x0dUL; // ULEB128 register
        public static readonly ulong DW_CFA_def_cfa_offset = (ulong)0x0eUL; // ULEB128 offset
        public static readonly ulong DW_CFA_def_cfa_expression = (ulong)0x0fUL; // BLOCK
        public static readonly ulong DW_CFA_expression = (ulong)0x10UL; // ULEB128 register, BLOCK
        public static readonly ulong DW_CFA_offset_extended_sf = (ulong)0x11UL; // ULEB128 register, SLEB128 offset
        public static readonly ulong DW_CFA_def_cfa_sf = (ulong)0x12UL; // ULEB128 register, SLEB128 offset
        public static readonly ulong DW_CFA_def_cfa_offset_sf = (ulong)0x13UL; // SLEB128 offset
        public static readonly ulong DW_CFA_val_offset = (ulong)0x14UL; // ULEB128, ULEB128
        public static readonly ulong DW_CFA_val_offset_sf = (ulong)0x15UL; // ULEB128, SLEB128
        public static readonly ulong DW_CFA_val_expression = (ulong)0x16UL; // ULEB128, BLOCK

        public static readonly ulong DW_CFA_lo_user = (ulong)0x1cUL;
        public static readonly ulong DW_CFA_hi_user = (ulong)0x3fUL; 

        // Opcodes that take an addend operand.
        public static readonly ulong DW_CFA_advance_loc = (ulong)0x1UL << (int)(6L); // +delta
        public static readonly ulong DW_CFA_offset = (ulong)0x2UL << (int)(6L); // +register (ULEB128 offset)
        public static readonly ulong DW_CFA_restore = (ulong)0x3UL << (int)(6L); // +register
    }
}}}
