// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Identify mismatches between assembly files and Go func declarations.

// package main -- go2cs converted at 2020 August 29 10:08:46 UTC
// Original source: C:\Go\src\cmd\vet\asmdecl.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using token = go.go.token_package;
using types = go.go.types_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // 'kind' is a kind of assembly variable.
        // The kinds 1, 2, 4, 8 stand for values of that size.
        private partial struct asmKind // : long
        {
        }

        // These special kinds are not valid sizes.
        private static readonly asmKind asmString = 100L + iota;
        private static readonly var asmSlice = 0;
        private static readonly var asmArray = 1;
        private static readonly var asmInterface = 2;
        private static readonly var asmEmptyInterface = 3;
        private static readonly var asmStruct = 4;
        private static readonly var asmComplex = 5;

        // An asmArch describes assembly parameters for an architecture
        private partial struct asmArch
        {
            public @string name;
            public bool bigEndian;
            public @string stack;
            public bool lr; // calculated during initialization
            public types.Sizes sizes;
            public long intSize;
            public long ptrSize;
            public long maxAlign;
        }

        // An asmFunc describes the expected variables for a function on a given architecture.
        private partial struct asmFunc
        {
            public ptr<asmArch> arch;
            public long size; // size of all arguments
            public map<@string, ref asmVar> vars;
            public map<long, ref asmVar> varByOffset;
        }

        // An asmVar describes a single assembly variable.
        private partial struct asmVar
        {
            public @string name;
            public asmKind kind;
            public @string typ;
            public long off;
            public long size;
            public slice<ref asmVar> inner;
        }

        private static asmArch asmArch386 = new asmArch(name:"386",bigEndian:false,stack:"SP",lr:false);        private static asmArch asmArchArm = new asmArch(name:"arm",bigEndian:false,stack:"R13",lr:true);        private static asmArch asmArchArm64 = new asmArch(name:"arm64",bigEndian:false,stack:"RSP",lr:true);        private static asmArch asmArchAmd64 = new asmArch(name:"amd64",bigEndian:false,stack:"SP",lr:false);        private static asmArch asmArchAmd64p32 = new asmArch(name:"amd64p32",bigEndian:false,stack:"SP",lr:false);        private static asmArch asmArchMips = new asmArch(name:"mips",bigEndian:true,stack:"R29",lr:true);        private static asmArch asmArchMipsLE = new asmArch(name:"mipsle",bigEndian:false,stack:"R29",lr:true);        private static asmArch asmArchMips64 = new asmArch(name:"mips64",bigEndian:true,stack:"R29",lr:true);        private static asmArch asmArchMips64LE = new asmArch(name:"mips64le",bigEndian:false,stack:"R29",lr:true);        private static asmArch asmArchPpc64 = new asmArch(name:"ppc64",bigEndian:true,stack:"R1",lr:true);        private static asmArch asmArchPpc64LE = new asmArch(name:"ppc64le",bigEndian:false,stack:"R1",lr:true);        private static asmArch asmArchS390X = new asmArch(name:"s390x",bigEndian:true,stack:"R15",lr:true);        private static ref asmArch arches = new slice<ref asmArch>(new ref asmArch[] { &asmArch386, &asmArchArm, &asmArchArm64, &asmArchAmd64, &asmArchAmd64p32, &asmArchMips, &asmArchMipsLE, &asmArchMips64, &asmArchMips64LE, &asmArchPpc64, &asmArchPpc64LE, &asmArchS390X });

        private static void init() => func((_, panic, __) =>
        {
            foreach (var (_, arch) in arches)
            {
                arch.sizes = types.SizesFor("gc", arch.name);
                if (arch.sizes == null)
                {
                    panic("missing SizesFor for gc/" + arch.name);
                }
                arch.intSize = int(arch.sizes.Sizeof(types.Typ[types.Int]));
                arch.ptrSize = int(arch.sizes.Sizeof(types.Typ[types.UnsafePointer]));
                arch.maxAlign = int(arch.sizes.Alignof(types.Typ[types.Int64]));
            }
        });

        private static var re = regexp.MustCompile;        private static var asmPlusBuild = re("//\\s+\\+build\\s+([^\\n]+)");        private static var asmTEXT = re("\\bTEXT\\b(.*)·([^\\(]+)\\(SB\\)(?:\\s*,\\s*([0-9A-Z|+()]+))?(?:\\s*,\\s*\\$(-?[0-9]+)(?:-(" +
    "[0-9]+))?)?");        private static var asmDATA = re("\\b(DATA|GLOBL)\\b");        private static var asmNamedFP = re("([a-zA-Z0-9_\\xFF-\\x{10FFFF}]+)(?:\\+([0-9]+))\\(FP\\)");        private static var asmUnnamedFP = re("[^+\\-0-9](([0-9]+)\\(FP\\))");        private static var asmSP = re("[^+\\-0-9](([0-9]+)\\(([A-Z0-9]+)\\))");        private static var asmOpcode = re("^\\s*(?:[A-Z0-9a-z_]+:)?\\s*([A-Z]+)\\s*([^,]*)(?:,\\s*(.*))?");        private static var ppc64Suff = re("([BHWD])(ZU|Z|U|BR)?$");

        private static void asmCheck(ref Package pkg)
        {
            if (!vet("asmdecl"))
            {
                return;
            } 

            // No work if no assembly files.
            if (!pkg.hasFileWithSuffix(".s"))
            {
                return;
            } 

            // Gather declarations. knownFunc[name][arch] is func description.
            var knownFunc = make_map<@string, map<@string, ref asmFunc>>();

            {
                var f__prev1 = f;

                foreach (var (_, __f) in pkg.files)
                {
                    f = __f;
                    if (f.file != null)
                    {
                        {
                            var decl__prev2 = decl;

                            foreach (var (_, __decl) in f.file.Decls)
                            {
                                decl = __decl;
                                {
                                    var decl__prev2 = decl;

                                    ref ast.FuncDecl (decl, ok) = decl._<ref ast.FuncDecl>();

                                    if (ok && decl.Body == null)
                                    {
                                        knownFunc[decl.Name.Name] = f.asmParseDecl(decl);
                                    }

                                    decl = decl__prev2;

                                }
                            }

                            decl = decl__prev2;
                        }

                    }
                }

                f = f__prev1;
            }

Files:
            {
                var f__prev1 = f;

                foreach (var (_, __f) in pkg.files)
                {
                    f = __f;
                    if (!strings.HasSuffix(f.name, ".s"))
                    {
                        continue;
                    }
                    Println("Checking file", f.name); 

                    // Determine architecture from file name if possible.
                    @string arch = default;
                    ref asmArch archDef = default;
                    {
                        var a__prev2 = a;

                        foreach (var (_, __a) in arches)
                        {
                            a = __a;
                            if (strings.HasSuffix(f.name, "_" + a.name + ".s"))
                            {
                                arch = a.name;
                                archDef = a;
                                break;
                            }
                        }

                        a = a__prev2;
                    }

                    var lines = strings.SplitAfter(string(f.content), "\n");
                    ref asmFunc fn = default;                    @string fnName = default;                    long localSize = default;                    long argSize = default;
                    bool wroteSP = default;                    bool haveRetArg = default;                    slice<long> retLine = default;

                    Action flushRet = () =>
                    {
                        if (fn != null && fn.vars["ret"] != null && !haveRetArg && len(retLine) > 0L)
                        {
                            var v = fn.vars["ret"];
                            {
                                var line__prev2 = line;

                                foreach (var (_, __line) in retLine)
                                {
                                    line = __line;
                                    f.Badf(token.NoPos, "%s:%d: [%s] %s: RET without writing to %d-byte ret+%d(FP)", f.name, line, arch, fnName, v.size, v.off);
                                }

                                line = line__prev2;
                            }

                        }
                        retLine = null;
                    }
;
                    {
                        var line__prev2 = line;

                        foreach (var (__lineno, __line) in lines)
                        {
                            lineno = __lineno;
                            line = __line;
                            lineno++;

                            Action<@string, object[]> badf = (format, args) =>
                            {
                                f.Badf(token.NoPos, "%s:%d: [%s] %s: %s", f.name, lineno, arch, fnName, fmt.Sprintf(format, args));
                            }
;

                            if (arch == "")
                            { 
                                // Determine architecture from +build line if possible.
                                {
                                    var m__prev2 = m;

                                    var m = asmPlusBuild.FindStringSubmatch(line);

                                    if (m != null)
                                    { 
                                        // There can be multiple architectures in a single +build line,
                                        // so accumulate them all and then prefer the one that
                                        // matches build.Default.GOARCH.
                                        slice<ref asmArch> archCandidates = default;
                                        foreach (var (_, fld) in strings.Fields(m[1L]))
                                        {
                                            {
                                                var a__prev4 = a;

                                                foreach (var (_, __a) in arches)
                                                {
                                                    a = __a;
                                                    if (a.name == fld)
                                                    {
                                                        archCandidates = append(archCandidates, a);
                                                    }
                                                }

                                                a = a__prev4;
                                            }

                                        }
                                        {
                                            var a__prev3 = a;

                                            foreach (var (_, __a) in archCandidates)
                                            {
                                                a = __a;
                                                if (a.name == build.Default.GOARCH)
                                                {
                                                    archCandidates = new slice<ref asmArch>(new ref asmArch[] { a });
                                                    break;
                                                }
                                            }

                                            a = a__prev3;
                                        }

                                        if (len(archCandidates) > 0L)
                                        {
                                            arch = archCandidates[0L].name;
                                            archDef = archCandidates[0L];
                                        }
                                    }

                                    m = m__prev2;

                                }
                            }
                            {
                                var m__prev1 = m;

                                m = asmTEXT.FindStringSubmatch(line);

                                if (m != null)
                                {
                                    flushRet();
                                    if (arch == "")
                                    { 
                                        // Arch not specified by filename or build tags.
                                        // Fall back to build.Default.GOARCH.
                                        {
                                            var a__prev3 = a;

                                            foreach (var (_, __a) in arches)
                                            {
                                                a = __a;
                                                if (a.name == build.Default.GOARCH)
                                                {
                                                    arch = a.name;
                                                    archDef = a;
                                                    break;
                                                }
                                            }

                                            a = a__prev3;
                                        }

                                        if (arch == "")
                                        {
                                            f.Warnf(token.NoPos, "%s: cannot determine architecture for assembly file", f.name);
                                            _continueFiles = true;
                                            break;
                                        }
                                    }
                                    fnName = m[2L];
                                    {
                                        var pkgName = strings.TrimSpace(m[1L]);

                                        if (pkgName != "")
                                        {
                                            var pathParts = strings.Split(pkgName, "∕");
                                            pkgName = pathParts[len(pathParts) - 1L];
                                            if (pkgName != f.pkg.path)
                                            {
                                                f.Warnf(token.NoPos, "%s:%d: [%s] cannot check cross-package assembly function: %s is in package %s", f.name, lineno, arch, fnName, pkgName);
                                                fn = null;
                                                fnName = "";
                                                continue;
                                            }
                                        }

                                    }
                                    fn = knownFunc[fnName][arch];
                                    if (fn != null)
                                    {
                                        var (size, _) = strconv.Atoi(m[5L]);
                                        var flag = m[3L];
                                        if (size != fn.size && (flag != "7" && !strings.Contains(flag, "NOSPLIT") || size != 0L))
                                        {
                                            badf("wrong argument size %d; expected $...-%d", size, fn.size);
                                        }
                                    }
                                    localSize, _ = strconv.Atoi(m[4L]);
                                    localSize += archDef.intSize;
                                    if (archDef.lr)
                                    { 
                                        // Account for caller's saved LR
                                        localSize += archDef.intSize;
                                    }
                                    argSize, _ = strconv.Atoi(m[5L]);
                                    if (fn == null && !strings.Contains(fnName, "<>"))
                                    {
                                        badf("function %s missing Go declaration", fnName);
                                    }
                                    wroteSP = false;
                                    haveRetArg = false;
                                    continue;
                                }
                                else if (strings.Contains(line, "TEXT") && strings.Contains(line, "SB"))
                                { 
                                    // function, but not visible from Go (didn't match asmTEXT), so stop checking
                                    flushRet();
                                    fn = null;
                                    fnName = "";
                                    continue;
                                }

                                m = m__prev1;

                            }

                            if (strings.Contains(line, "RET"))
                            {
                                retLine = append(retLine, lineno);
                            }
                            if (fnName == "")
                            {
                                continue;
                            }
                            if (asmDATA.FindStringSubmatch(line) != null)
                            {
                                fn = null;
                            }
                            if (archDef == null)
                            {
                                continue;
                            }
                            if (strings.Contains(line, ", " + archDef.stack) || strings.Contains(line, ",\t" + archDef.stack))
                            {
                                wroteSP = true;
                                continue;
                            }
                            {
                                var m__prev3 = m;

                                foreach (var (_, __m) in asmSP.FindAllStringSubmatch(line, -1L))
                                {
                                    m = __m;
                                    if (m[3L] != archDef.stack || wroteSP)
                                    {
                                        continue;
                                    }
                                    long off = 0L;
                                    if (m[1L] != "")
                                    {
                                        off, _ = strconv.Atoi(m[2L]);
                                    }
                                    if (off >= localSize)
                                    {
                                        if (fn != null)
                                        {
                                            v = fn.varByOffset[off - localSize];
                                            if (v != null)
                                            {
                                                badf("%s should be %s+%d(FP)", m[1L], v.name, off - localSize);
                                                continue;
                                            }
                                        }
                                        if (off >= localSize + argSize)
                                        {
                                            badf("use of %s points beyond argument frame", m[1L]);
                                            continue;
                                        }
                                        badf("use of %s to access argument frame", m[1L]);
                                    }
                                }

                                m = m__prev3;
                            }

                            if (fn == null)
                            {
                                continue;
                            }
                            {
                                var m__prev3 = m;

                                foreach (var (_, __m) in asmUnnamedFP.FindAllStringSubmatch(line, -1L))
                                {
                                    m = __m;
                                    var (off, _) = strconv.Atoi(m[2L]);
                                    v = fn.varByOffset[off];
                                    if (v != null)
                                    {
                                        badf("use of unnamed argument %s; offset %d is %s+%d(FP)", m[1L], off, v.name, v.off);
                                    }
                                    else
                                    {
                                        badf("use of unnamed argument %s", m[1L]);
                                    }
                                }

                                m = m__prev3;
                            }

                            {
                                var m__prev3 = m;

                                foreach (var (_, __m) in asmNamedFP.FindAllStringSubmatch(line, -1L))
                                {
                                    m = __m;
                                    var name = m[1L];
                                    off = 0L;
                                    if (m[2L] != "")
                                    {
                                        off, _ = strconv.Atoi(m[2L]);
                                    }
                                    if (name == "ret" || strings.HasPrefix(name, "ret_"))
                                    {
                                        haveRetArg = true;
                                    }
                                    v = fn.vars[name];
                                    if (v == null)
                                    { 
                                        // Allow argframe+0(FP).
                                        if (name == "argframe" && off == 0L)
                                        {
                                            continue;
                                        }
                                        v = fn.varByOffset[off];
                                        if (v != null)
                                        {
                                            badf("unknown variable %s; offset %d is %s+%d(FP)", name, off, v.name, v.off);
                                        }
                                        else
                                        {
                                            badf("unknown variable %s", name);
                                        }
                                        continue;
                                    }
                                    asmCheckVar(badf, fn, line, m[0L], off, v);
                                }

                                m = m__prev3;
                            }

                        }

                        line = line__prev2;
                    }

                    flushRet();
                }

                f = f__prev1;
            }
        }

        private static asmKind asmKindForType(types.Type t, long size) => func((_, panic, __) =>
        {
            switch (t.Underlying().type())
            {
                case ref types.Basic t:

                    if (t.Kind() == types.String) 
                        return asmString;
                    else if (t.Kind() == types.Complex64 || t.Kind() == types.Complex128) 
                        return asmComplex;
                                        return asmKind(size);
                    break;
                case ref types.Pointer t:
                    return asmKind(size);
                    break;
                case ref types.Chan t:
                    return asmKind(size);
                    break;
                case ref types.Map t:
                    return asmKind(size);
                    break;
                case ref types.Signature t:
                    return asmKind(size);
                    break;
                case ref types.Struct t:
                    return asmStruct;
                    break;
                case ref types.Interface t:
                    if (t.Empty())
                    {
                        return asmEmptyInterface;
                    }
                    return asmInterface;
                    break;
                case ref types.Array t:
                    return asmArray;
                    break;
                case ref types.Slice t:
                    return asmSlice;
                    break;
            }
            panic("unreachable");
        });

        // A component is an assembly-addressable component of a composite type,
        // or a composite type itself.
        private partial struct component
        {
            public long size;
            public long offset;
            public asmKind kind;
            public @string typ;
            public @string suffix; // Such as _base for string base, _0_lo for lo half of first element of [1]uint64 on 32 bit machine.
            public @string outer; // The suffix for immediately containing composite type.
        }

        private static component newComponent(@string suffix, asmKind kind, @string typ, long offset, long size, @string outer)
        {
            return new component(suffix:suffix,kind:kind,typ:typ,offset:offset,size:size,outer:outer);
        }

        // componentsOfType generates a list of components of type t.
        // For example, given string, the components are the string itself, the base, and the length.
        private static slice<component> componentsOfType(ref asmArch arch, types.Type t)
        {
            return appendComponentsRecursive(arch, t, null, "", 0L);
        }

        // appendComponentsRecursive implements componentsOfType.
        // Recursion is required to correct handle structs and arrays,
        // which can contain arbitrary other types.
        private static slice<component> appendComponentsRecursive(ref asmArch arch, types.Type t, slice<component> cc, @string suffix, long off)
        {
            var s = t.String();
            var size = int(arch.sizes.Sizeof(t));
            var kind = asmKindForType(t, size);
            cc = append(cc, newComponent(suffix, kind, s, off, size, suffix));


            if (kind == 8L) 
                if (arch.ptrSize == 4L)
                {
                    @string w1 = "lo";
                    @string w2 = "hi";
                    if (arch.bigEndian)
                    {
                        w1 = w2;
                        w2 = w1;
                    }
                    cc = append(cc, newComponent(suffix + "_" + w1, 4L, "half " + s, off, 4L, suffix));
                    cc = append(cc, newComponent(suffix + "_" + w2, 4L, "half " + s, off + 4L, 4L, suffix));
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
                var fsize = size / 2L;
                cc = append(cc, newComponent(suffix + "_real", asmKind(fsize), fmt.Sprintf("real(complex%d)", size * 8L), off, fsize, suffix));
                cc = append(cc, newComponent(suffix + "_imag", asmKind(fsize), fmt.Sprintf("imag(complex%d)", size * 8L), off + fsize, fsize, suffix));
            else if (kind == asmStruct) 
                ref types.Struct tu = t.Underlying()._<ref types.Struct>();
                var fields = make_slice<ref types.Var>(tu.NumFields());
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < tu.NumFields(); i++)
                    {
                        fields[i] = tu.Field(i);
                    }


                    i = i__prev1;
                }
                var offsets = arch.sizes.Offsetsof(fields);
                {
                    long i__prev1 = i;

                    foreach (var (__i, __f) in fields)
                    {
                        i = __i;
                        f = __f;
                        cc = appendComponentsRecursive(arch, f.Type(), cc, suffix + "_" + f.Name(), off + int(offsets[i]));
                    }

                    i = i__prev1;
                }
            else if (kind == asmArray) 
                tu = t.Underlying()._<ref types.Array>();
                var elem = tu.Elem(); 
                // Calculate offset of each element array.
                fields = new slice<ref types.Var>(new ref types.Var[] { types.NewVar(token.NoPos,nil,"fake0",elem), types.NewVar(token.NoPos,nil,"fake1",elem) });
                offsets = arch.sizes.Offsetsof(fields);
                var elemoff = int(offsets[1L]);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < int(tu.Len()); i++)
                    {
                        cc = appendComponentsRecursive(arch, elem, cc, suffix + "_" + strconv.Itoa(i), i * elemoff);
                    }


                    i = i__prev1;
                }
                        return cc;
        }

        // asmParseDecl parses a function decl for expected assembly variables.
        private static map<@string, ref asmFunc> asmParseDecl(this ref File f, ref ast.FuncDecl decl)
        {
            ref asmArch arch = default;            ref asmFunc fn = default;            long offset = default; 

            // addParams adds asmVars for each of the parameters in list.
            // isret indicates whether the list are the arguments or the return values.
            Action<slice<ref ast.Field>, bool> addParams = (list, isret) =>
            {
                long argnum = 0L;
                foreach (var (_, fld) in list)
                {
                    var t = f.pkg.types[fld.Type].Type;
                    var align = int(arch.sizes.Alignof(t));
                    var size = int(arch.sizes.Sizeof(t));
                    offset += -offset & (align - 1L);
                    var cc = componentsOfType(arch, t); 

                    // names is the list of names with this type.
                    var names = fld.Names;
                    if (len(names) == 0L)
                    { 
                        // Anonymous args will be called arg, arg1, arg2, ...
                        // Similarly so for return values: ret, ret1, ret2, ...
                        @string name = "arg";
                        if (isret)
                        {
                            name = "ret";
                        }
                        if (argnum > 0L)
                        {
                            name += strconv.Itoa(argnum);
                        }
                        names = new slice<ref ast.Ident>(new ref ast.Ident[] { ast.NewIdent(name) });
                    }
                    argnum += len(names); 

                    // Create variable for each name.
                    foreach (var (_, id) in names)
                    {
                        name = id.Name;
                        foreach (var (_, c) in cc)
                        {
                            var outer = name + c.outer;
                            asmVar v = new asmVar(name:name+c.suffix,kind:c.kind,typ:c.typ,off:offset+c.offset,size:c.size,);
                            {
                                var vo = fn.vars[outer];

                                if (vo != null)
                                {
                                    vo.inner = append(vo.inner, ref v);
                                }

                            }
                            fn.vars[v.name] = ref v;
                            for (long i = 0L; i < v.size; i++)
                            {
                                fn.varByOffset[v.off + i] = ref v;
                            }

                        }
                        offset += size;
                    }
                }
            }
;

            var m = make_map<@string, ref asmFunc>();
            foreach (var (_, __arch) in arches)
            {
                arch = __arch;
                fn = ref new asmFunc(arch:arch,vars:make(map[string]*asmVar),varByOffset:make(map[int]*asmVar),);
                offset = 0L;
                addParams(decl.Type.Params.List, false);
                if (decl.Type.Results != null && len(decl.Type.Results.List) > 0L)
                {
                    offset += -offset & (arch.maxAlign - 1L);
                    addParams(decl.Type.Results.List, true);
                }
                fn.size = offset;
                m[arch.name] = fn;
            }

            return m;
        }

        // asmCheckVar checks a single variable reference.
        private static void asmCheckVar(params Action<@string, object>[] badf, ref asmFunc fn, @string line, @string expr, long off, ref asmVar v)
        {
            badf = badf.Clone();

            var m = asmOpcode.FindStringSubmatch(line);
            if (m == null)
            {
                if (!strings.HasPrefix(strings.TrimSpace(line), "//"))
                {
                    badf("cannot find assembly opcode");
                }
                return;
            } 

            // Determine operand sizes from instruction.
            // Typically the suffix suffices, but there are exceptions.
            asmKind src = default;            asmKind dst = default;            asmKind kind = default;

            var op = m[1L];
            switch (fn.arch.name + "." + op)
            {
                case "386.FMOVLP": 
                    src = 8L;
                    dst = 4L;
                    break;
                case "arm.MOVD": 
                    src = 8L;
                    break;
                case "arm.MOVW": 
                    src = 4L;
                    break;
                case "arm.MOVH": 

                case "arm.MOVHU": 
                    src = 2L;
                    break;
                case "arm.MOVB": 

                case "arm.MOVBU": 
                    src = 1L; 
                    // LEA* opcodes don't really read the second arg.
                    // They just take the address of it.
                    break;
                case "386.LEAL": 
                    dst = 4L;
                    break;
                case "amd64.LEAQ": 
                    dst = 8L;
                    break;
                case "amd64p32.LEAL": 
                    dst = 4L;
                    break;
                default: 
                    switch (fn.arch.name)
                    {
                        case "386": 

                        case "amd64": 
                            if (strings.HasPrefix(op, "F") && (strings.HasSuffix(op, "D") || strings.HasSuffix(op, "DP")))
                            { 
                                // FMOVDP, FXCHD, etc
                                src = 8L;
                                break;
                            }
                            if (strings.HasPrefix(op, "P") && strings.HasSuffix(op, "RD"))
                            { 
                                // PINSRD, PEXTRD, etc
                                src = 4L;
                                break;
                            }
                            if (strings.HasPrefix(op, "F") && (strings.HasSuffix(op, "F") || strings.HasSuffix(op, "FP")))
                            { 
                                // FMOVFP, FXCHF, etc
                                src = 4L;
                                break;
                            }
                            if (strings.HasSuffix(op, "SD"))
                            { 
                                // MOVSD, SQRTSD, etc
                                src = 8L;
                                break;
                            }
                            if (strings.HasSuffix(op, "SS"))
                            { 
                                // MOVSS, SQRTSS, etc
                                src = 4L;
                                break;
                            }
                            if (strings.HasPrefix(op, "SET"))
                            { 
                                // SETEQ, etc
                                src = 1L;
                                break;
                            }
                            switch (op[len(op) - 1L])
                            {
                                case 'B': 
                                    src = 1L;
                                    break;
                                case 'W': 
                                    src = 2L;
                                    break;
                                case 'L': 
                                    src = 4L;
                                    break;
                                case 'D': 

                                case 'Q': 
                                    src = 8L;
                                    break;
                            }
                            break;
                        case "ppc64": 
                            // Strip standard suffixes to reveal size letter.

                        case "ppc64le": 
                            // Strip standard suffixes to reveal size letter.
                            m = ppc64Suff.FindStringSubmatch(op);
                            if (m != null)
                            {
                                switch (m[1L][0L])
                                {
                                    case 'B': 
                                        src = 1L;
                                        break;
                                    case 'H': 
                                        src = 2L;
                                        break;
                                    case 'W': 
                                        src = 4L;
                                        break;
                                    case 'D': 
                                        src = 8L;
                                        break;
                                }
                            }
                            break;
                        case "mips": 

                        case "mipsle": 

                        case "mips64": 

                        case "mips64le": 
                            switch (op)
                            {
                                case "MOVB": 

                                case "MOVBU": 
                                    src = 1L;
                                    break;
                                case "MOVH": 

                                case "MOVHU": 
                                    src = 2L;
                                    break;
                                case "MOVW": 

                                case "MOVWU": 

                                case "MOVF": 
                                    src = 4L;
                                    break;
                                case "MOVV": 

                                case "MOVD": 
                                    src = 8L;
                                    break;
                            }
                            break;
                        case "s390x": 
                            switch (op)
                            {
                                case "MOVB": 

                                case "MOVBZ": 
                                    src = 1L;
                                    break;
                                case "MOVH": 

                                case "MOVHZ": 
                                    src = 2L;
                                    break;
                                case "MOVW": 

                                case "MOVWZ": 

                                case "FMOVS": 
                                    src = 4L;
                                    break;
                                case "MOVD": 

                                case "FMOVD": 
                                    src = 8L;
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (dst == 0L)
            {
                dst = src;
            } 

            // Determine whether the match we're holding
            // is the first or second argument.
            if (strings.Index(line, expr) > strings.Index(line, ","))
            {
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
                vk = v.inner[0L].kind;
                vs = v.inner[0L].size;
                vt = v.inner[0L].typ;
                        if (off != v.off)
            {
                bytes.Buffer inner = default;
                {
                    var i__prev1 = i;
                    var vi__prev1 = vi;

                    foreach (var (__i, __vi) in v.inner)
                    {
                        i = __i;
                        vi = __vi;
                        if (len(v.inner) > 1L)
                        {
                            fmt.Fprintf(ref inner, ",");
                        }
                        fmt.Fprintf(ref inner, " ");
                        if (i == len(v.inner) - 1L)
                        {
                            fmt.Fprintf(ref inner, "or ");
                        }
                        fmt.Fprintf(ref inner, "%s+%d(FP)", vi.name, vi.off);
                    }

                    i = i__prev1;
                    vi = vi__prev1;
                }

                badf("invalid offset %s; expected %s+%d(FP)%s", expr, v.name, v.off, inner.String());
                return;
            }
            if (kind != 0L && kind != vk)
            {
                inner = default;
                if (len(v.inner) > 0L)
                {
                    fmt.Fprintf(ref inner, " containing");
                    {
                        var i__prev1 = i;
                        var vi__prev1 = vi;

                        foreach (var (__i, __vi) in v.inner)
                        {
                            i = __i;
                            vi = __vi;
                            if (i > 0L && len(v.inner) > 2L)
                            {
                                fmt.Fprintf(ref inner, ",");
                            }
                            fmt.Fprintf(ref inner, " ");
                            if (i > 0L && i == len(v.inner) - 1L)
                            {
                                fmt.Fprintf(ref inner, "and ");
                            }
                            fmt.Fprintf(ref inner, "%s+%d(FP)", vi.name, vi.off);
                        }

                        i = i__prev1;
                        vi = vi__prev1;
                    }

                }
                badf("invalid %s of %s; %s is %d-byte value%s", op, expr, vt, vs, inner.String());
            }
        }
    }
}
