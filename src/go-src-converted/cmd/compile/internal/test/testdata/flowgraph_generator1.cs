// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:28:27 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\internal\test\testdata\flowgraph_generator1.go
namespace go;

using fmt = fmt_package;
using strings = strings_package;


// make fake flow graph.

// The blocks of the flow graph are designated with letters A
// through Z, always including A (start block) and Z (exit
// block) The specification of a flow graph is a comma-
// separated list of block successor words, for blocks ordered
// A, B, C etc, where each block except Z has one or two
// successors, and any block except A can be a target. Within
// the generated code, each block with two successors includes
// a conditional testing x & 1 != 0 (x is the input parameter
// to the generated function) and also unconditionally shifts x
// right by one, so that different inputs generate different
// execution paths, including loops. Every block inverts a
// global binary to ensure it is not empty. For a flow graph
// with J words (J+1 blocks), a J-1 bit serial number specifies
// which blocks (not including A and Z) include an increment of
// the return variable y by increasing powers of 10, and a
// different version of the test function is created for each
// of the 2-to-the-(J-1) serial numbers.

// For each generated function a compact summary is also
// created so that the generated function can be simulated
// with a simple interpreter to sanity check the behavior of
// the compiled code.

// For example:

// func BC_CD_BE_BZ_CZ101(x int64) int64 {
//     y := int64(0)
//     var b int64
//     _ = b
//     b = x & 1
//     x = x >> 1
//     if b != 0 {
//         goto C
//     }
//     goto B
// B:
//     glob_ = !glob_
//     y += 1
//     b = x & 1
//     x = x >> 1
//     if b != 0 {
//         goto D
//     }
//     goto C
// C:
//     glob_ = !glob_
//     // no y increment
//     b = x & 1
//     x = x >> 1
//     if b != 0 {
//         goto E
//     }
//     goto B
// D:
//     glob_ = !glob_
//     y += 10
//     b = x & 1
//     x = x >> 1
//     if b != 0 {
//         goto Z
//     }
//     goto B
// E:
//     glob_ = !glob_
//     // no y increment
//     b = x & 1
//     x = x >> 1
//     if b != 0 {
//         goto Z
//     }
//     goto C
// Z:
//     return y
// }

// {f:BC_CD_BE_BZ_CZ101,
//  maxin:32, blocks:[]blo{
//      blo{inc:0, cond:true, succs:[2]int64{1, 2}},
//      blo{inc:1, cond:true, succs:[2]int64{2, 3}},
//      blo{inc:0, cond:true, succs:[2]int64{1, 4}},
//      blo{inc:10, cond:true, succs:[2]int64{1, 25}},
//      blo{inc:0, cond:true, succs:[2]int64{2, 25}},}},

public static partial class main_package {

private static @string labels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

private static (slice<@string>, @string) blocks(@string spec) {
    slice<@string> blocks = default;
    @string fnameBase = default;

    spec = strings.ToUpper(spec);
    blocks = strings.Split(spec, ",");
    fnameBase = strings.Replace(spec, ",", "_", -1);
    return ;
}

private static @string makeFunctionFromFlowGraph(slice<blo> blocks, @string fname) {
    @string s = "";

    foreach (var (j) in blocks) { 
        // begin block
        if (j == 0) { 
            // block A, implicit label
            s += "\nfunc " + fname + "(x int64) int64 {\n\ty := int64(0)\n\tvar b int64\n\t_ = b";
        }
        else
 { 
            // block B,C, etc, explicit label w/ conditional increment
            var l = labels[(int)j..(int)j + 1];
            @string yeq = "\n\t// no y increment";
            if (blocks[j].inc != 0) {
                yeq = "\n\ty += " + fmt.Sprintf("%d", blocks[j].inc);
            }
            s += "\n" + l + ":\n\tglob = !glob" + yeq;
        }
        if (blocks[j].cond) { // conditionally branch to second successor
            s += "\n\tb = x & 1\n\tx = x >> 1\n\tif b != 0 {" + "\n\t\tgoto " + string(labels[blocks[j].succs[1]]) + "\n\t}";
        }
        s += "\n\tgoto " + string(labels[blocks[j].succs[0]]);
    }    s += "\nZ:\n\treturn y\n}\n";
    return s;
}

private static slice<@string> graphs = new slice<@string>(new @string[] { "Z", "BZ,Z", "B,BZ", "BZ,BZ", "ZB,Z", "B,ZB", "ZB,BZ", "ZB,ZB", "BC,C,Z", "BC,BC,Z", "BC,BC,BZ", "BC,Z,Z", "BC,ZC,Z", "BC,ZC,BZ", "BZ,C,Z", "BZ,BC,Z", "BZ,CZ,Z", "BZ,C,BZ", "BZ,BC,BZ", "BZ,CZ,BZ", "BZ,C,CZ", "BZ,BC,CZ", "BZ,CZ,CZ", "BC,CD,BE,BZ,CZ", "BC,BD,CE,CZ,BZ", "BC,BD,CE,FZ,GZ,F,G", "BC,BD,CE,FZ,GZ,G,F", "BC,DE,BE,FZ,FZ,Z", "BC,DE,BE,FZ,ZF,Z", "BC,DE,BE,ZF,FZ,Z", "BC,DE,EB,FZ,FZ,Z", "BC,ED,BE,FZ,FZ,Z", "CB,DE,BE,FZ,FZ,Z", "CB,ED,BE,FZ,FZ,Z", "BC,ED,EB,FZ,ZF,Z", "CB,DE,EB,ZF,FZ,Z", "CB,ED,EB,FZ,FZ,Z", "BZ,CD,CD,CE,BZ", "EC,DF,FG,ZC,GB,BE,FD", "BH,CF,DG,HE,BF,CG,DH,BZ" });

// blo describes a block in the generated/interpreted code
private partial struct blo {
    public long inc; // increment amount
    public bool cond; // block ends in conditional
    public array<long> succs;
}

// strings2blocks converts a slice of strings specifying
// successors into a slice of blo encoding the blocks in a
// common form easy to execute or interpret.
private static (slice<blo>, nuint) strings2blocks(slice<@string> blocks, @string fname, nint i) {
    slice<blo> bs = default;
    nuint cond = default;

    bs = make_slice<blo>(len(blocks));
    var edge = int64(1);
    cond = 0;
    var k = uint(0);
    foreach (var (j, s) in blocks) {
        if (j == 0) {
        }
        else
 {
            if ((i >> (int)(k)) & 1 != 0) {
                bs[j].inc = edge;
                edge *= 10;
            }
            k++;
        }
        if (len(s) > 1) {
            bs[j].succs[1] = int64(blocks[j][1] - 'A');
            bs[j].cond = true;
            cond++;
        }
        bs[j].succs[0] = int64(blocks[j][0] - 'A');
    }    return (bs, cond);
}

// fmtBlocks writes out the blocks for consumption in the generated test
private static @string fmtBlocks(slice<blo> bs) {
    @string s = "[]blo{";
    foreach (var (_, b) in bs) {
        s += fmt.Sprintf("blo{inc:%d, cond:%v, succs:[2]int64{%d, %d}},", b.inc, b.cond, b.succs[0], b.succs[1]);
    }    s += "}";
    return s;
}

private static void Main() {
    fmt.Printf("// This is a machine-generated test file from flowgraph_generator1.go.\npackage ma" +
    "in\nimport \"fmt\"\nvar glob bool\n");
    @string s = "var funs []fun = []fun{";
    foreach (var (_, g) in graphs) {
        var (split, fnameBase) = blocks(g);
        nint nconfigs = 1 << (int)(uint(len(split) - 1));

        for (nint i = 0; i < nconfigs; i++) {
            var fname = fnameBase + fmt.Sprintf("%b", i);
            var (bs, k) = strings2blocks(split, fname, i);
            fmt.Printf("%s", makeFunctionFromFlowGraph(bs, fname));
            s += "\n\t\t{f:" + fname + ", maxin:" + fmt.Sprintf("%d", 1 << (int)(k)) + ", blocks:" + fmtBlocks(bs) + "},";
        }
    }    s += "}\n"; 
    // write types for name+array tables.
    fmt.Printf("%s", "\ntype blo struct {\n\tinc   int64\n\tcond  bool\n\tsuccs [2]int64\n}\ntype fun struct {\n\t" +
    "f      func(int64) int64\n\tmaxin  int64\n\tblocks []blo\n}\n"); 
    // write table of function names and blo arrays.
    fmt.Printf("%s", s); 

    // write interpreter and main/test
    fmt.Printf("%s", @"
func interpret(blocks []blo, x int64) (int64, bool) {
	y := int64(0)
	last := int64(25) // 'Z'-'A'
	j := int64(0)
	for i := 0; i < 4*len(blocks); i++ {
		b := blocks[j]
		y += b.inc
		next := b.succs[0]
		if b.cond {
			c := x&1 != 0
			x = x>>1
			if c {
				next = b.succs[1]
			}
		}
		if next == last {
			return y, true
		}
		j = next
	}
	return -1, false
}

func main() {
	sum := int64(0)
	for i, f := range funs {
		for x := int64(0); x < 16*f.maxin; x++ {
			y, ok := interpret(f.blocks, x)
			if ok {
				yy := f.f(x)
				if y != yy {
					fmt.Printf(""y(%d) != yy(%d), x=%b, i=%d, blocks=%v\n"", y, yy, x, i, f.blocks)
					return
				}
				sum += y
			}
		}
	}
//	fmt.Printf(""Sum of all returns over all terminating inputs is %d\n"", sum)
}
");
}

} // end main_package
