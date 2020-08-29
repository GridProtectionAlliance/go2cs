// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 August 29 10:07:39 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\gnu.go
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm64
{
    public static partial class arm64asm_package
    {
        // GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
        // This form typically matches the syntax defined in the ARM Reference Manual.
        public static @string GNUSyntax(Inst inst)
        {

            if (inst.Op == RET) 
                {
                    Reg (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok && r == X30)
                    {
                        return "ret";
                    }
                }
            else if (inst.Op == B) 
                {
                    Cond (_, ok) = inst.Args[0L]._<Cond>();

                    if (ok)
                    {
                        return strings.ToLower("b." + inst.Args[0L].String() + " " + inst.Args[1L].String());
                    }
                }
            else if (inst.Op == SYSL) 
                var result = strings.ToLower(inst.String());
                return strings.Replace(result, "c", "C", -1L);
            else if (inst.Op == DCPS1 || inst.Op == DCPS2 || inst.Op == DCPS3 || inst.Op == CLREX) 
                return strings.ToLower(strings.TrimSpace(inst.String()));
            else if (inst.Op == ISB) 
                if (strings.Contains(inst.String(), "SY"))
                {
                    result = strings.TrimSuffix(inst.String(), " SY");
                    return strings.ToLower(result);
                }
                        return strings.ToLower(inst.String());
        }
    }
}}}}}}}
