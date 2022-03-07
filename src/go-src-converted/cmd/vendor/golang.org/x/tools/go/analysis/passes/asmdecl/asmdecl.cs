// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asmdecl defines an Analyzer that reports mismatches between
// assembly files and Go declarations.
// package asmdecl -- go2cs converted at 2022 March 06 23:34:27 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/asmdecl" ==> using asmdecl = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.asmdecl_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\asmdecl\asmdecl.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using token = go.go.token_package;
using types = go.go.types_package;
using log = go.log_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class asmdecl_package {

public static readonly @string Doc = "report mismatches between assembly files and Go declarations";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"asmdecl",Doc:Doc,Run:run,));

// 'kind' is a kind of assembly variable.
// The kinds 1, 2, 4, 8 stand for values of that size.
private partial struct asmKind { // : nint
}

// These special kinds are not valid sizes.
private static readonly asmKind asmString = 100 + iota;
private static readonly var asmSlice = 0;
private static readonly var asmArray = 1;
private static readonly var asmInterface = 2;
private static readonly var asmEmptyInterface = 3;
private static readonly var asmStruct = 4;
private static readonly var asmComplex = 5;


// An asmArch describes assembly parameters for an architecture
private partial struct asmArch {
    public @string name;
    public bool bigEndian;
    public @string stack;
    public bool lr; // calculated during initialization
    public types.Sizes sizes;
    public nint intSize;
    public nint ptrSize;
    public nint maxAlign;
}

// An asmFunc describes the expected variables for a function on a given architecture.
private partial struct asmFunc {
    public ptr<asmArch> arch;
    public nint size; // size of all arguments
    public map<@string, ptr<asmVar>> vars;
    public map<nint, ptr<asmVar>> varByOffset;
}

// An asmVar describes a single assembly variable.
private partial struct asmVar {
    public @string name;
    public asmKind kind;
    public @string typ;
    public nint off;
    public nint size;
    public slice<ptr<asmVar>> inner;
}

private static asmArch asmArch386 = new asmArch(name:"386",bigEndian:false,stack:"SP",lr:false);private static asmArch asmArchArm = new asmArch(name:"arm",bigEndian:false,stack:"R13",lr:true);private static asmArch asmArchArm64 = new asmArch(name:"arm64",bigEndian:false,stack:"RSP",lr:true);private static asmArch asmArchAmd64 = new asmArch(name:"amd64",bigEndian:false,stack:"SP",lr:false);private static asmArch asmArchMips = new asmArch(name:"mips",bigEndian:true,stack:"R29",lr:true);private static asmArch asmArchMipsLE = new asmArch(name:"mipsle",bigEndian:false,stack:"R29",lr:true);private static asmArch asmArchMips64 = new asmArch(name:"mips64",bigEndian:true,stack:"R29",lr:true);private static asmArch asmArchMips64LE = new asmArch(name:"mips64le",bigEndian:false,stack:"R29",lr:true);private static asmArch asmArchPpc64 = new asmArch(name:"ppc64",bigEndian:true,stack:"R1",lr:true);private static asmArch asmArchPpc64LE = new asmArch(name:"ppc64le",bigEndian:false,stack:"R1",lr:true);private static asmArch asmArchRISCV64 = new asmArch(name:"riscv64",bigEndian:false,stack:"SP",lr:true);private static asmArch asmArchS390X = new asmArch(name:"s390x",bigEndian:true,stack:"R15",lr:true);private static asmArch asmArchWasm = new asmArch(name:"wasm",bigEndian:false,stack:"SP",lr:false);private static ptr<asmArch> arches = new slice<ptr<asmArch>>(new ptr<asmArch>[] { &asmArch386, &asmArchArm, &asmArchArm64, &asmArchAmd64, &asmArchMips, &asmArchMipsLE, &asmArchMips64, &asmArchMips64LE, &asmArchPpc64, &asmArchPpc64LE, &asmArchRISCV64, &asmArchS390X, &asmArchWasm });

private static void init() {
    foreach (var (_, arch) in arches) {
        arch.sizes = types.SizesFor("gc", arch.name);
        if (arch.sizes == null) { 
            // TODO(adonovan): fix: now that asmdecl is not in the standard
            // library we cannot assume types.SizesFor is consistent with arches.
            // For now, assume 64-bit norms and print a warning.
            // But this warning should really be deferred until we attempt to use
            // arch, which is very unlikely. Better would be
            // to defer size computation until we have Pass.TypesSizes.
            arch.sizes = types.SizesFor("gc", "amd64");
            log.Printf("unknown architecture %s", arch.name);

        }
        arch.intSize = int(arch.sizes.Sizeof(types.Typ[types.Int]));
        arch.ptrSize = int(arch.sizes.Sizeof(types.Typ[types.UnsafePointer]));
        arch.maxAlign = int(arch.sizes.Alignof(types.Typ[types.Int64]));

    }
}

private static var re = regexp.MustCompile;private static var asmPlusBuild = re("//\\s+\\+build\\s+([^\\n]+)");private static var asmTEXT = re("\\bTEXT\\b(.*)·([^\\(]+)\\(SB\\)(?:\\s*,\\s*([0-9A-Z|+()]+))?(?:\\s*,\\s*\\$(-?[0-9]+)(?:-(" +
    "[0-9]+))?)?");private static var asmDATA = re("\\b(DATA|GLOBL)\\b");private static var asmNamedFP = re("\\$?([a-zA-Z0-9_\\xFF-\\x{10FFFF}]+)(?:\\+([0-9]+))\\(FP\\)");private static var asmUnnamedFP = re("[^+\\-0-9](([0-9]+)\\(FP\\))");private static var asmSP = re("[^+\\-0-9](([0-9]+)\\(([A-Z0-9]+)\\))");private static var asmOpcode = re("^\\s*(?:[A-Z0-9a-z_]+:)?\\s*([A-Z]+)\\s*([^,]*)(?:,\\s*(.*))?");private static var ppc64Suff = re("([BHWD])(ZU|Z|U|BR)?$");private static var abiSuff = re("^(.+)<ABI.+>$");

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;
 
    // No work if no assembly files.
    slice<@string> sfiles = default;
    {
        var fname__prev1 = fname;

        foreach (var (_, __fname) in pass.OtherFiles) {
            fname = __fname;
            if (strings.HasSuffix(fname, ".s")) {
                sfiles = append(sfiles, fname);
            }
        }
        fname = fname__prev1;
    }

    if (sfiles == null) {
        return (null, error.As(null!)!);
    }
    var knownFunc = make_map<@string, map<@string, ptr<asmFunc>>>();

    foreach (var (_, f) in pass.Files) {
        {
            var decl__prev2 = decl;

            foreach (var (_, __decl) in f.Decls) {
                decl = __decl;
                {
                    var decl__prev1 = decl;

                    ptr<ast.FuncDecl> (decl, ok) = decl._<ptr<ast.FuncDecl>>();

                    if (ok && decl.Body == null) {
                        knownFunc[decl.Name.Name] = asmParseDecl(_addr_pass, _addr_decl);
                    }

                    decl = decl__prev1;

                }

            }

            decl = decl__prev2;
        }
    }Files:
    {
        var fname__prev1 = fname;

        foreach (var (_, __fname) in sfiles) {
            fname = __fname;
            var (content, tf, err) = analysisutil.ReadFile(pass.Fset, fname);
            if (err != null) {
                return (null, error.As(err)!);
            } 

            // Determine architecture from file name if possible.
            @string arch = default;
            ptr<asmArch> archDef;
            {
                var a__prev2 = a;

                foreach (var (_, __a) in arches) {
                    a = __a;
                    if (strings.HasSuffix(fname, "_" + a.name + ".s")) {
                        arch = a.name;
                        archDef = a;
                        break;
                    }
                }

                a = a__prev2;
            }

            var lines = strings.SplitAfter(string(content), "\n");
            ptr<asmFunc> fn;            @string fnName = default;            nint localSize = default;            nint argSize = default;
            bool wroteSP = default;            bool noframe = default;            bool haveRetArg = default;            slice<nint> retLine = default;

            Action flushRet = () => {
                if (fn != null && fn.vars["ret"] != null && !haveRetArg && len(retLine) > 0) {
                    var v = fn.vars["ret"];
                    {
                        var line__prev2 = line;

                        foreach (var (_, __line) in retLine) {
                            line = __line;
                            pass.Reportf(analysisutil.LineStart(tf, line), "[%s] %s: RET without writing to %d-byte ret+%d(FP)", arch, fnName, v.size, v.off);
                        }

                        line = line__prev2;
                    }
                }

                retLine = null;

            }
;
            Func<@string, @string> trimABI = fnName => {
                var m = abiSuff.FindStringSubmatch(fnName);
                if (m != null) {
                    return m[1];
                }
                return fnName;
            }
;
            {
                var line__prev2 = line;

                foreach (var (__lineno, __line) in lines) {
                    lineno = __lineno;
                    line = __line;
                    lineno++;

                    Action<@string, object[]> badf = (format, args) => {
                        pass.Reportf(analysisutil.LineStart(tf, lineno), "[%s] %s: %s", arch, fnName, fmt.Sprintf(format, args));
                    }
;

                    if (arch == "") { 
                        // Determine architecture from +build line if possible.
                        {
                            var m__prev2 = m;

                            m = asmPlusBuild.FindStringSubmatch(line);

                            if (m != null) { 
                                // There can be multiple architectures in a single +build line,
                                // so accumulate them all and then prefer the one that
                                // matches build.Default.GOARCH.
                                slice<ptr<asmArch>> archCandidates = default;
                                foreach (var (_, fld) in strings.Fields(m[1])) {
                                    {
                                        var a__prev4 = a;

                                        foreach (var (_, __a) in arches) {
                                            a = __a;
                                            if (a.name == fld) {
                                                archCandidates = append(archCandidates, a);
                                            }
                                        }

                                        a = a__prev4;
                                    }
                                }
                                {
                                    var a__prev3 = a;

                                    foreach (var (_, __a) in archCandidates) {
                                        a = __a;
                                        if (a.name == build.Default.GOARCH) {
                                            archCandidates = new slice<ptr<asmArch>>(new ptr<asmArch>[] { a });
                                            break;
                                        }
                                    }

                                    a = a__prev3;
                                }

                                if (len(archCandidates) > 0) {
                                    arch = archCandidates[0].name;
                                    archDef = archCandidates[0];
                                }

                            }

                            m = m__prev2;

                        }

                    } 

                    // Ignore comments and commented-out code.
                    {
                        var i = strings.Index(line, "//");

                        if (i >= 0) {
                            line = line[..(int)i];
                        }

                    }


                    {
                        var m__prev1 = m;

                        m = asmTEXT.FindStringSubmatch(line);

                        if (m != null) {
                            flushRet();
                            if (arch == "") { 
                                // Arch not specified by filename or build tags.
                                // Fall back to build.Default.GOARCH.
                                {
                                    var a__prev3 = a;

                                    foreach (var (_, __a) in arches) {
                                        a = __a;
                                        if (a.name == build.Default.GOARCH) {
                                            arch = a.name;
                                            archDef = a;
                                            break;
                                        }
                                    }

                                    a = a__prev3;
                                }

                                if (arch == "") {
                                    log.Printf("%s: cannot determine architecture for assembly file", fname);
                                    _continueFiles = true;
                                    break;
                                }

                            }

                            fnName = m[2];
                            {
                                var pkgPath = strings.TrimSpace(m[1]);

                                if (pkgPath != "") { 
                                    // The assembler uses Unicode division slash within
                                    // identifiers to represent the directory separator.
                                    pkgPath = strings.Replace(pkgPath, "∕", "/", -1);
                                    if (pkgPath != pass.Pkg.Path()) { 
                                        // log.Printf("%s:%d: [%s] cannot check cross-package assembly function: %s is in package %s", fname, lineno, arch, fnName, pkgPath)
                                        fn = null;
                                        fnName = "";
                                        continue;

                                    }

                                } 
                                // Trim off optional ABI selector.

                            } 
                            // Trim off optional ABI selector.
                            fnName = trimABI(fnName);
                            var flag = m[3];
                            fn = knownFunc[fnName][arch];
                            if (fn != null) {
                                var (size, _) = strconv.Atoi(m[5]);
                                if (size != fn.size && (flag != "7" && !strings.Contains(flag, "NOSPLIT") || size != 0)) {
                                    badf("wrong argument size %d; expected $...-%d", size, fn.size);
                                }
                            }

                            localSize, _ = strconv.Atoi(m[4]);
                            localSize += archDef.intSize;
                            if (archDef.lr && !strings.Contains(flag, "NOFRAME")) { 
                                // Account for caller's saved LR
                                localSize += archDef.intSize;

                            }

                            argSize, _ = strconv.Atoi(m[5]);
                            noframe = strings.Contains(flag, "NOFRAME");
                            if (fn == null && !strings.Contains(fnName, "<>") && !noframe) {
                                badf("function %s missing Go declaration", fnName);
                            }

                            wroteSP = false;
                            haveRetArg = false;
                            continue;

                        }
                        else if (strings.Contains(line, "TEXT") && strings.Contains(line, "SB")) { 
                            // function, but not visible from Go (didn't match asmTEXT), so stop checking
                            flushRet();
                            fn = null;
                            fnName = "";
                            continue;

                        }


                        m = m__prev1;

                    }


                    if (strings.Contains(line, "RET") && !strings.Contains(line, "(SB)")) { 
                        // RET f(SB) is a tail call. It is okay to not write the results.
                        retLine = append(retLine, lineno);

                    }

                    if (fnName == "") {
                        continue;
                    }

                    if (asmDATA.FindStringSubmatch(line) != null) {
                        fn = null;
                    }

                    if (archDef == null) {
                        continue;
                    }

                    if (strings.Contains(line, ", " + archDef.stack) || strings.Contains(line, ",\t" + archDef.stack) || strings.Contains(line, "NOP " + archDef.stack) || strings.Contains(line, "NOP\t" + archDef.stack)) {
                        wroteSP = true;
                        continue;
                    }

                    if (arch == "wasm" && strings.Contains(line, "CallImport")) { 
                        // CallImport is a call out to magic that can write the result.
                        haveRetArg = true;

                    }

                    {
                        var m__prev3 = m;

                        foreach (var (_, __m) in asmSP.FindAllStringSubmatch(line, -1)) {
                            m = __m;
                            if (m[3] != archDef.stack || wroteSP || noframe) {
                                continue;
                            }
                            nint off = 0;
                            if (m[1] != "") {
                                off, _ = strconv.Atoi(m[2]);
                            }
                            if (off >= localSize) {
                                if (fn != null) {
                                    v = fn.varByOffset[off - localSize];
                                    if (v != null) {
                                        badf("%s should be %s+%d(FP)", m[1], v.name, off - localSize);
                                        continue;
                                    }
                                }
                                if (off >= localSize + argSize) {
                                    badf("use of %s points beyond argument frame", m[1]);
                                    continue;
                                }
                                badf("use of %s to access argument frame", m[1]);
                            }
                        }

                        m = m__prev3;
                    }

                    if (fn == null) {
                        continue;
                    }

                    {
                        var m__prev3 = m;

                        foreach (var (_, __m) in asmUnnamedFP.FindAllStringSubmatch(line, -1)) {
                            m = __m;
                            var (off, _) = strconv.Atoi(m[2]);
                            v = fn.varByOffset[off];
                            if (v != null) {
                                badf("use of unnamed argument %s; offset %d is %s+%d(FP)", m[1], off, v.name, v.off);
                            }
                            else
 {
                                badf("use of unnamed argument %s", m[1]);
                            }

                        }

                        m = m__prev3;
                    }

                    {
                        var m__prev3 = m;

                        foreach (var (_, __m) in asmNamedFP.FindAllStringSubmatch(line, -1)) {
                            m = __m;
                            var name = m[1];
                            off = 0;
                            if (m[2] != "") {
                                off, _ = strconv.Atoi(m[2]);
                            }
                            if (name == "ret" || strings.HasPrefix(name, "ret_")) {
                                haveRetArg = true;
                            }
                            v = fn.vars[name];
                            if (v == null) { 
                                // Allow argframe+0(FP).
                                if (name == "argframe" && off == 0) {
                                    continue;
                                }

                                v = fn.varByOffset[off];
                                if (v != null) {
                                    badf("unknown variable %s; offset %d is %s+%d(FP)", name, off, v.name, v.off);
                                }
                                else
 {
                                    badf("unknown variable %s", name);
                                }

                                continue;

                            }

                            asmCheckVar(badf, fn, line, m[0], off, _addr_v, archDef);

                        }

                        m = m__prev3;
                    }
                }

                line = line__prev2;
            }

            flushRet();

        }
        fname = fname__prev1;
    }
    return (null, error.As(null!)!);

}

private static asmKind asmKindForType(types.Type t, nint size) => func((_, panic, _) => {
    switch (t.Underlying().type()) {
        case ptr<types.Basic> t:

            if (t.Kind() == types.String) 
                return asmString;
            else if (t.Kind() == types.Complex64 || t.Kind() == types.Complex128) 
                return asmComplex;
                        return asmKind(size);
            break;
        case ptr<types.Pointer> t:
            return asmKind(size);
            break;
        case ptr<types.Chan> t:
            return asmKind(size);
            break;
        case ptr<types.Map> t:
            return asmKind(size);
            break;
        case ptr<types.Signature> t:
            return asmKind(size);
            break;
        case ptr<types.Struct> t:
            return asmStruct;
            break;
        case ptr<types.Interface> t:
            if (t.Empty()) {
                return asmEmptyInterface;
            }
            return asmInterface;
            break;
        case ptr<types.Array> t:
            return asmArray;
            break;
        case ptr<types.Slice> t:
            return asmSlice;
            break;
    }
    panic("unreachable");

});

// A component is an assembly-addressable component of a composite type,
// or a composite type itself.
private partial struct component {
    public nint size;
    public nint offset;
    public asmKind kind;
    public @string typ;
    public @string suffix; // Such as _base for string base, _0_lo for lo half of first element of [1]uint64 on 32 bit machine.
    public @string outer; // The suffix for immediately containing composite type.
}

private static component newComponent(@string suffix, asmKind kind, @string typ, nint offset, nint size, @string outer) {
    return new component(suffix:suffix,kind:kind,typ:typ,offset:offset,size:size,outer:outer);
}

// componentsOfType generates a list of components of type t.
// For example, given string, the components are the string itself, the base, and the length.
private static slice<component> componentsOfType(ptr<asmArch> _addr_arch, types.Type t) {
    ref asmArch arch = ref _addr_arch.val;

    return appendComponentsRecursive(_addr_arch, t, null, "", 0);
}

// appendComponentsRecursive implements componentsOfType.
// Recursion is required to correct handle structs and arrays,
// which can contain arbitrary other types.
private static slice<component> appendComponentsRecursive(ptr<asmArch> _addr_arch, types.Type t, slice<component> cc, @string suffix, nint off) {
    ref asmArch arch = ref _addr_arch.val;

    var s = t.String();
    var size = int(arch.sizes.Sizeof(t));
    var kind = asmKindForType(t, size);
    cc = append(cc, newComponent(suffix, kind, s, off, size, suffix));


    if (kind == 8) 
        if (arch.ptrSize == 4) {
            @string w1 = "lo";
            @string w2 = "hi";
            if (arch.bigEndian) {
                (w1, w2) = (w2, w1);
            }

            cc = append(cc, newComponent(suffix + "_" + w1, 4, "half " + s, off, 4, suffix));
            cc = append(cc, newComponent(suffix + "_" + w2, 4, "half " + s, off + 4, 4, suffix));

        }
    else if (kind == asmEmptyInterface) 
        cc = append(cc, newComponent(suffix + "_type", asmKind(arch.ptrSize), "interface type", off, arch.ptrSize, suffix));
        cc = append(cc, newComponent(suffix + "_data", asmKind(arch.ptrSize), "interface data", off + arch.ptrSize, arch.ptrSize, suffix));
    else if (kind == asmInterface) 
        cc = append(cc, newComponent(suffix + "_itable", asmKind(arch.ptrSize), "interface itable", off, arch.ptrSize, suffix));
        cc = append(cc, newComponent(suffix + "_data", asmKind(arch.ptrSize), "interface data", off + arch.ptrSize, arch.ptrSize, suffix));
    else if (kind == asmSlice) 
        cc = append(cc, newComponent(suffix + "_base", asmKind(arch.ptrSize), "slice base", off, arch.ptrSize, suffix));
        cc = append(cc, newComponent(suffix + "_len", asmKind(arch.intSize), "slice len", off + arch.ptrSize, arch.intSize, suffix));
        cc = append(cc, newComponent(suffix + "_cap", asmKind(arch.intSize), "slice cap", off + arch.ptrSize + arch.intSize, arch.intSize, suffix));
    else if (kind == asmString) 
        cc = append(cc, newComponent(suffix + "_base", asmKind(arch.ptrSize), "string base", off, arch.ptrSize, suffix));
        cc = append(cc, newComponent(suffix + "_len", asmKind(arch.intSize), "string len", off + arch.ptrSize, arch.intSize, suffix));
    else if (kind == asmComplex) 
        var fsize = size / 2;
        cc = append(cc, newComponent(suffix + "_real", asmKind(fsize), fmt.Sprintf("real(complex%d)", size * 8), off, fsize, suffix));
        cc = append(cc, newComponent(suffix + "_imag", asmKind(fsize), fmt.Sprintf("imag(complex%d)", size * 8), off + fsize, fsize, suffix));
    else if (kind == asmStruct) 
        ptr<types.Struct> tu = t.Underlying()._<ptr<types.Struct>>();
        var fields = make_slice<ptr<types.Var>>(tu.NumFields());
        {
            nint i__prev1 = i;

            for (nint i = 0; i < tu.NumFields(); i++) {
                fields[i] = tu.Field(i);
            }


            i = i__prev1;
        }
        var offsets = arch.sizes.Offsetsof(fields);
        {
            nint i__prev1 = i;

            foreach (var (__i, __f) in fields) {
                i = __i;
                f = __f;
                cc = appendComponentsRecursive(_addr_arch, f.Type(), cc, suffix + "_" + f.Name(), off + int(offsets[i]));
            }

            i = i__prev1;
        }
    else if (kind == asmArray) 
        tu = t.Underlying()._<ptr<types.Array>>();
        var elem = tu.Elem(); 
        // Calculate offset of each element array.
        fields = new slice<ptr<types.Var>>(new ptr<types.Var>[] { types.NewVar(token.NoPos,nil,"fake0",elem), types.NewVar(token.NoPos,nil,"fake1",elem) });
        offsets = arch.sizes.Offsetsof(fields);
        var elemoff = int(offsets[1]);
        {
            nint i__prev1 = i;

            for (i = 0; i < int(tu.Len()); i++) {
                cc = appendComponentsRecursive(_addr_arch, elem, cc, suffix + "_" + strconv.Itoa(i), off + i * elemoff);
            }


            i = i__prev1;
        }
        return cc;

}

// asmParseDecl parses a function decl for expected assembly variables.
private static map<@string, ptr<asmFunc>> asmParseDecl(ptr<analysis.Pass> _addr_pass, ptr<ast.FuncDecl> _addr_decl) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.FuncDecl decl = ref _addr_decl.val;

    ptr<asmArch> arch;    ptr<asmFunc> fn;    nint offset = default; 

    // addParams adds asmVars for each of the parameters in list.
    // isret indicates whether the list are the arguments or the return values.
    // TODO(adonovan): simplify by passing (*types.Signature).{Params,Results}
    // instead of list.
    Action<slice<ptr<ast.Field>>, bool> addParams = (list, isret) => {
        nint argnum = 0;
        foreach (var (_, fld) in list) {
            var t = pass.TypesInfo.Types[fld.Type].Type; 

            // Work around https://golang.org/issue/28277.
            if (t == null) {
                {
                    ptr<ast.Ellipsis> (ell, ok) = fld.Type._<ptr<ast.Ellipsis>>();

                    if (ok) {
                        t = types.NewSlice(pass.TypesInfo.Types[ell.Elt].Type);
                    }

                }

            }

            var align = int(arch.sizes.Alignof(t));
            var size = int(arch.sizes.Sizeof(t));
            offset += -offset & (align - 1);
            var cc = componentsOfType(arch, t); 

            // names is the list of names with this type.
            var names = fld.Names;
            if (len(names) == 0) { 
                // Anonymous args will be called arg, arg1, arg2, ...
                // Similarly so for return values: ret, ret1, ret2, ...
                @string name = "arg";
                if (isret) {
                    name = "ret";
                }

                if (argnum > 0) {
                    name += strconv.Itoa(argnum);
                }

                names = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { ast.NewIdent(name) });

            }

            argnum += len(names); 

            // Create variable for each name.
            foreach (var (_, id) in names) {
                name = id.Name;
                foreach (var (_, c) in cc) {
                    var outer = name + c.outer;
                    ref asmVar v = ref heap(new asmVar(name:name+c.suffix,kind:c.kind,typ:c.typ,off:offset+c.offset,size:c.size,), out ptr<asmVar> _addr_v);
                    {
                        var vo = fn.vars[outer];

                        if (vo != null) {
                            vo.inner = append(vo.inner, _addr_v);
                        }

                    }

                    _addr_fn.vars[v.name] = _addr_v;
                    fn.vars[v.name] = ref _addr_fn.vars[v.name].val;
                    for (nint i = 0; i < v.size; i++) {
                        _addr_fn.varByOffset[v.off + i] = _addr_v;
                        fn.varByOffset[v.off + i] = ref _addr_fn.varByOffset[v.off + i].val;

                    }


                }
                offset += size;

            }

        }
    };

    var m = make_map<@string, ptr<asmFunc>>();
    foreach (var (_, __arch) in arches) {
        arch = __arch;
        fn = addr(new asmFunc(arch:arch,vars:make(map[string]*asmVar),varByOffset:make(map[int]*asmVar),));
        offset = 0;
        addParams(decl.Type.Params.List, false);
        if (decl.Type.Results != null && len(decl.Type.Results.List) > 0) {
            offset += -offset & (arch.maxAlign - 1);
            addParams(decl.Type.Results.List, true);
        }
        fn.size = offset;
        m[arch.name] = fn;

    }
    return m;

}

// asmCheckVar checks a single variable reference.
private static void asmCheckVar(params Action<@string, object>[] badf, ptr<asmFunc> _addr_fn, @string line, @string expr, nint off, ptr<asmVar> _addr_v, ptr<asmArch> _addr_archDef) {
    badf = badf.Clone();
    ref asmFunc fn = ref _addr_fn.val;
    ref asmVar v = ref _addr_v.val;
    ref asmArch archDef = ref _addr_archDef.val;

    var m = asmOpcode.FindStringSubmatch(line);
    if (m == null) {
        if (!strings.HasPrefix(strings.TrimSpace(line), "//")) {
            badf("cannot find assembly opcode");
        }
        return ;

    }
    var addr = strings.HasPrefix(expr, "$"); 

    // Determine operand sizes from instruction.
    // Typically the suffix suffices, but there are exceptions.
    asmKind src = default;    asmKind dst = default;    asmKind kind = default;

    var op = m[1];
    switch (fn.arch.name + "." + op) {
        case "386.FMOVLP": 
            (src, dst) = (8, 4);
            break;
        case "arm.MOVD": 
            src = 8;
            break;
        case "arm.MOVW": 
            src = 4;
            break;
        case "arm.MOVH": 

        case "arm.MOVHU": 
            src = 2;
            break;
        case "arm.MOVB": 

        case "arm.MOVBU": 
            src = 1; 
            // LEA* opcodes don't really read the second arg.
            // They just take the address of it.
            break;
        case "386.LEAL": 
            dst = 4;
            addr = true;
            break;
        case "amd64.LEAQ": 
            dst = 8;
            addr = true;
            break;
        default: 
            switch (fn.arch.name) {
                case "386": 

                case "amd64": 
                    if (strings.HasPrefix(op, "F") && (strings.HasSuffix(op, "D") || strings.HasSuffix(op, "DP"))) { 
                        // FMOVDP, FXCHD, etc
                        src = 8;
                        break;

                    }

                    if (strings.HasPrefix(op, "P") && strings.HasSuffix(op, "RD")) { 
                        // PINSRD, PEXTRD, etc
                        src = 4;
                        break;

                    }

                    if (strings.HasPrefix(op, "F") && (strings.HasSuffix(op, "F") || strings.HasSuffix(op, "FP"))) { 
                        // FMOVFP, FXCHF, etc
                        src = 4;
                        break;

                    }

                    if (strings.HasSuffix(op, "SD")) { 
                        // MOVSD, SQRTSD, etc
                        src = 8;
                        break;

                    }

                    if (strings.HasSuffix(op, "SS")) { 
                        // MOVSS, SQRTSS, etc
                        src = 4;
                        break;

                    }

                    if (op == "MOVO" || op == "MOVOU") {
                        src = 16;
                        break;
                    }

                    if (strings.HasPrefix(op, "SET")) { 
                        // SETEQ, etc
                        src = 1;
                        break;

                    }

                    switch (op[len(op) - 1]) {
                        case 'B': 
                            src = 1;
                            break;
                        case 'W': 
                            src = 2;
                            break;
                        case 'L': 
                            src = 4;
                            break;
                        case 'D': 

                        case 'Q': 
                            src = 8;
                            break;
                    }

                    break;
                case "ppc64": 
                    // Strip standard suffixes to reveal size letter.

                case "ppc64le": 
                    // Strip standard suffixes to reveal size letter.
                    m = ppc64Suff.FindStringSubmatch(op);
                    if (m != null) {
                        switch (m[1][0]) {
                            case 'B': 
                                src = 1;
                                break;
                            case 'H': 
                                src = 2;
                                break;
                            case 'W': 
                                src = 4;
                                break;
                            case 'D': 
                                src = 8;
                                break;
                        }

                    }

                    break;
                case "mips": 

                case "mipsle": 

                case "mips64": 

                case "mips64le": 
                    switch (op) {
                        case "MOVB": 

                        case "MOVBU": 
                            src = 1;
                            break;
                        case "MOVH": 

                        case "MOVHU": 
                            src = 2;
                            break;
                        case "MOVW": 

                        case "MOVWU": 

                        case "MOVF": 
                            src = 4;
                            break;
                        case "MOVV": 

                        case "MOVD": 
                            src = 8;
                            break;
                    }

                    break;
                case "s390x": 
                    switch (op) {
                        case "MOVB": 

                        case "MOVBZ": 
                            src = 1;
                            break;
                        case "MOVH": 

                        case "MOVHZ": 
                            src = 2;
                            break;
                        case "MOVW": 

                        case "MOVWZ": 

                        case "FMOVS": 
                            src = 4;
                            break;
                        case "MOVD": 

                        case "FMOVD": 
                            src = 8;
                            break;
                    }

                    break;
            }

            break;
    }
    if (dst == 0) {
        dst = src;
    }
    if (strings.Index(line, expr) > strings.Index(line, ",")) {
        kind = dst;
    }
    else
 {
        kind = src;
    }
    var vk = v.kind;
    var vs = v.size;
    var vt = v.typ;

    if (vk == asmInterface || vk == asmEmptyInterface || vk == asmString || vk == asmSlice) 
        // allow reference to first word (pointer)
        vk = v.inner[0].kind;
        vs = v.inner[0].size;
        vt = v.inner[0].typ;
    else if (vk == asmComplex) 
        // Allow a single instruction to load both parts of a complex.
        if (int(kind) == vs) {
            kind = asmComplex;
        }
        if (addr) {
        vk = asmKind(archDef.ptrSize);
        vs = archDef.ptrSize;
        vt = "address";
    }
    if (off != v.off) {
        ref bytes.Buffer inner = ref heap(out ptr<bytes.Buffer> _addr_inner);
        {
            var i__prev1 = i;
            var vi__prev1 = vi;

            foreach (var (__i, __vi) in v.inner) {
                i = __i;
                vi = __vi;
                if (len(v.inner) > 1) {
                    fmt.Fprintf(_addr_inner, ",");
                }
                fmt.Fprintf(_addr_inner, " ");
                if (i == len(v.inner) - 1) {
                    fmt.Fprintf(_addr_inner, "or ");
                }
                fmt.Fprintf(_addr_inner, "%s+%d(FP)", vi.name, vi.off);
            }

            i = i__prev1;
            vi = vi__prev1;
        }

        badf("invalid offset %s; expected %s+%d(FP)%s", expr, v.name, v.off, inner.String());
        return ;

    }
    if (kind != 0 && kind != vk) {
        inner = default;
        if (len(v.inner) > 0) {
            fmt.Fprintf(_addr_inner, " containing");
            {
                var i__prev1 = i;
                var vi__prev1 = vi;

                foreach (var (__i, __vi) in v.inner) {
                    i = __i;
                    vi = __vi;
                    if (i > 0 && len(v.inner) > 2) {
                        fmt.Fprintf(_addr_inner, ",");
                    }
                    fmt.Fprintf(_addr_inner, " ");
                    if (i > 0 && i == len(v.inner) - 1) {
                        fmt.Fprintf(_addr_inner, "and ");
                    }
                    fmt.Fprintf(_addr_inner, "%s+%d(FP)", vi.name, vi.off);
                }

                i = i__prev1;
                vi = vi__prev1;
            }
        }
        badf("invalid %s of %s; %s is %d-byte value%s", op, expr, vt, vs, inner.String());

    }
}

} // end asmdecl_package
