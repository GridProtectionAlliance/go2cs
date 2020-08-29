// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Binary package export.

/*
1) Export data encoding principles:

The export data is a serialized description of the graph of exported
"objects": constants, types, variables, and functions. Aliases may be
directly reexported, and unaliased types may be indirectly reexported
(as part of the type of a directly exported object). More generally,
objects referred to from inlined function bodies can be reexported.
We need to know which package declares these reexported objects, and
therefore packages are also part of the export graph.

The roots of the graph are two lists of objects. The 1st list (phase 1,
see Export) contains all objects that are exported at the package level.
These objects are the full representation of the package's API, and they
are the only information a platform-independent tool (e.g., go/types)
needs to know to type-check against a package.

The 2nd list of objects contains all objects referred to from exported
inlined function bodies. These objects are needed by the compiler to
make sense of the function bodies; the exact list contents are compiler-
specific.

Finally, the export data contains a list of representations for inlined
function bodies. The format of this representation is compiler specific.

The graph is serialized in in-order fashion, starting with the roots.
Each object in the graph is serialized by writing its fields sequentially.
If the field is a pointer to another object, that object is serialized in
place, recursively. Otherwise the field is written in place. Non-pointer
fields are all encoded as integer or string values.

Some objects (packages, types) may be referred to more than once. When
reaching an object that was not serialized before, an integer _index_
is assigned to it, starting at 0. In this case, the encoding starts
with an integer _tag_ < 0. The tag value indicates the kind of object
that follows and that this is the first time that we see this object.
If the object was already serialized, the encoding is simply the object
index >= 0. An importer can trivially determine if an object needs to
be read in for the first time (tag < 0) and entered into the respective
object table, or if the object was seen already (index >= 0), in which
case the index is used to look up the object in the respective table.

Before exporting or importing, the type tables are populated with the
predeclared types (int, string, error, unsafe.Pointer, etc.). This way
they are automatically encoded with a known and fixed type index.

2) Encoding format:

The export data starts with two newline-terminated strings: a version
string and either an empty string, or "debug", when emitting the debug
format. These strings are followed by version-specific encoding options.

(The Go1.7 version starts with a couple of bytes specifying the format.
That format encoding is no longer used but is supported to avoid spurious
errors when importing old installed package files.)

This header is followed by the package object for the exported package,
two lists of objects, and the list of inlined function bodies.

The encoding of objects is straight-forward: Constants, variables, and
functions start with their name, type, and possibly a value. Named types
record their name and package so that they can be canonicalized: If the
same type was imported before via another import, the importer must use
the previously imported type pointer so that we have exactly one version
(i.e., one pointer) for each named type (and read but discard the current
type encoding). Unnamed types simply encode their respective fields.
Aliases are encoded starting with their name followed by the qualified
identifier denoting the original (aliased) object, which was exported
earlier.

In the encoding, some lists start with the list length. Some lists are
terminated with an end marker (usually for lists where we may not know
the length a priori).

Integers use variable-length encoding for compact representation.

Strings are canonicalized similar to objects that may occur multiple times:
If the string was exported already, it is represented by its index only.
Otherwise, the export data starts with the negative string length (negative,
so we can distinguish from string index), followed by the string bytes.
The empty string is mapped to index 0. (The initial format string is an
exception; it is encoded as the string bytes followed by a newline).

The exporter and importer are completely symmetric in implementation: For
each encoding routine there is a matching and symmetric decoding routine.
This symmetry makes it very easy to change or extend the format: If a new
field needs to be encoded, a symmetric change can be made to exporter and
importer.

3) Making changes to the encoding format:

Any change to the encoding format requires a respective change in the
exporter below and a corresponding symmetric change to the importer in
bimport.go.

Furthermore, it requires a corresponding change to go/internal/gcimporter
and golang.org/x/tools/go/gcimporter15. Changes to the latter must preserve
compatibility with both the last release of the compiler, and with the
corresponding compiler at tip. That change is necessarily more involved,
as it must switch based on the version number in the export data file.

It is recommended to turn on debugFormat temporarily when working on format
changes as it will help finding encoding/decoding inconsistencies quickly.
*/

// package gc -- go2cs converted at 2020 August 29 09:25:43 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bexport.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using types = go.cmd.compile.@internal.types_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // If debugFormat is set, each integer and string value is preceded by a marker
        // and position information in the encoding. This mechanism permits an importer
        // to recognize immediately when it is out of sync. The importer recognizes this
        // mode automatically (i.e., it can import export data produced with debugging
        // support even if debugFormat is not set at the time of import). This mode will
        // lead to massively larger export data (by a factor of 2 to 3) and should only
        // be enabled during development and debugging.
        //
        // NOTE: This flag is the first flag to enable if importing dies because of
        // (suspected) format errors, and whenever a change is made to the format.
        private static readonly var debugFormat = false; // default: false

        // Current export format version. Increase with each format change.
        // 5: improved position encoding efficiency (issue 20080, CL 41619)
        // 4: type name objects support type aliases, uses aliasTag
        // 3: Go1.8 encoding (same as version 2, aliasTag defined but never used)
        // 2: removed unused bool in ODCL export (compiler only)
        // 1: header format change (more regular), export package for _ struct fields
        // 0: Go1.7 encoding
 // default: false

        // Current export format version. Increase with each format change.
        // 5: improved position encoding efficiency (issue 20080, CL 41619)
        // 4: type name objects support type aliases, uses aliasTag
        // 3: Go1.8 encoding (same as version 2, aliasTag defined but never used)
        // 2: removed unused bool in ODCL export (compiler only)
        // 1: header format change (more regular), export package for _ struct fields
        // 0: Go1.7 encoding
        private static readonly long exportVersion = 5L;

        // exportInlined enables the export of inlined function bodies and related
        // dependencies. The compiler should work w/o any loss of functionality with
        // the flag disabled, but the generated code will lose access to inlined
        // function bodies across packages, leading to performance bugs.
        // Leave for debugging.


        // exportInlined enables the export of inlined function bodies and related
        // dependencies. The compiler should work w/o any loss of functionality with
        // the flag disabled, but the generated code will lose access to inlined
        // function bodies across packages, leading to performance bugs.
        // Leave for debugging.
        private static readonly var exportInlined = true; // default: true

        // trackAllTypes enables cycle tracking for all types, not just named
        // types. The existing compiler invariants assume that unnamed types
        // that are not completely set up are not used, or else there are spurious
        // errors.
        // If disabled, only named types are tracked, possibly leading to slightly
        // less efficient encoding in rare cases. It also prevents the export of
        // some corner-case type declarations (but those were not handled correctly
        // with the former textual export format either).
        // Note that when a type is only seen once, as many unnamed types are,
        // it is less efficient to track it, since we then also record an index for it.
        // See CLs 41622 and 41623 for some data and discussion.
        // TODO(gri) enable selectively and remove once issues caused by it are fixed
 // default: true

        // trackAllTypes enables cycle tracking for all types, not just named
        // types. The existing compiler invariants assume that unnamed types
        // that are not completely set up are not used, or else there are spurious
        // errors.
        // If disabled, only named types are tracked, possibly leading to slightly
        // less efficient encoding in rare cases. It also prevents the export of
        // some corner-case type declarations (but those were not handled correctly
        // with the former textual export format either).
        // Note that when a type is only seen once, as many unnamed types are,
        // it is less efficient to track it, since we then also record an index for it.
        // See CLs 41622 and 41623 for some data and discussion.
        // TODO(gri) enable selectively and remove once issues caused by it are fixed
        private static readonly var trackAllTypes = false;



        private partial struct exporter
        {
            public ptr<bufio.Writer> @out; // object -> index maps, indexed in order of serialization
            public map<@string, long> strIndex;
            public map<@string, long> pathIndex;
            public map<ref types.Pkg, long> pkgIndex;
            public map<ref types.Type, long> typIndex;
            public slice<ref Func> funcList;
            public map<ref types.Type, bool> marked; // types already seen by markType

// position encoding
            public bool posInfoFormat;
            public @string prevFile;
            public long prevLine; // debugging support
            public long written; // bytes written
            public long indent; // for p.trace
            public bool trace;
        }

        // export writes the exportlist for localpkg to out and returns the number of bytes written.
        private static long export(ref bufio.Writer @out, bool trace)
        {
            exporter p = new exporter(out:out,strIndex:map[string]int{"":0},pathIndex:map[string]int{"":0},pkgIndex:make(map[*types.Pkg]int),typIndex:make(map[*types.Type]int),posInfoFormat:true,trace:trace,); 

            // write version info
            // The version string must start with "version %d" where %d is the version
            // number. Additional debugging information may follow after a blank; that
            // text is ignored by the importer.
            p.rawStringln(fmt.Sprintf("version %d", exportVersion));
            @string debug = default;
            if (debugFormat)
            {
                debug = "debug";
            }
            p.rawStringln(debug); // cannot use p.bool since it's affected by debugFormat; also want to see this clearly
            p.@bool(trackAllTypes);
            p.@bool(p.posInfoFormat); 

            // --- generic export data ---

            // populate type map with predeclared "known" types
            var predecl = predeclared();
            foreach (var (index, typ) in predecl)
            {
                p.typIndex[typ] = index;
            }
            if (len(p.typIndex) != len(predecl))
            {
                Fatalf("exporter: duplicate entries in type map?");
            } 

            // write package data
            if (localpkg.Path != "")
            {
                Fatalf("exporter: local package path not empty: %q", localpkg.Path);
            }
            p.pkg(localpkg);
            if (p.trace)
            {
                p.tracef("\n");
            } 

            // Mark all inlineable functions that the importer could call.
            // This is done by tracking down all inlineable methods
            // reachable from exported types.
            p.marked = make_map<ref types.Type, bool>();
            {
                var n__prev1 = n;

                foreach (var (_, __n) in exportlist)
                {
                    n = __n;
                    var sym = n.Sym;
                    if (sym.Exported())
                    { 
                        // Closures are added to exportlist, but with Exported
                        // already set. The export code below skips over them, so
                        // we have to here as well.
                        // TODO(mdempsky): Investigate why. This seems suspicious.
                        continue;
                    }
                    p.markType(asNode(sym.Def).Type);
                }

                n = n__prev1;
            }

            p.marked = null; 

            // export objects
            //
            // First, export all exported (package-level) objects; i.e., all objects
            // in the current exportlist. These objects represent all information
            // required to import this package and type-check against it; i.e., this
            // is the platform-independent export data. The format is generic in the
            // sense that different compilers can use the same representation.
            //
            // During this first phase, more objects may be added to the exportlist
            // (due to inlined function bodies and their dependencies). Export those
            // objects in a second phase. That data is platform-specific as it depends
            // on the inlining decisions of the compiler and the representation of the
            // inlined function bodies.

            // remember initial exportlist length
            var numglobals = len(exportlist); 

            // Phase 1: Export objects in _current_ exportlist; exported objects at
            //          package level.
            // Use range since we want to ignore objects added to exportlist during
            // this phase.
            long objcount = 0L;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in exportlist)
                {
                    n = __n;
                    sym = n.Sym;

                    if (sym.Exported())
                    {
                        continue;
                    }
                    sym.SetExported(true); 

                    // TODO(gri) Closures have dots in their names;
                    // e.g., TestFloatZeroValue.func1 in math/big tests.
                    if (strings.Contains(sym.Name, "."))
                    {
                        Fatalf("exporter: unexpected symbol: %v", sym);
                    }
                    if (sym.Def == null)
                    {
                        Fatalf("exporter: unknown export symbol: %v", sym);
                    } 

                    // TODO(gri) Optimization: Probably worthwhile collecting
                    // long runs of constants and export them "in bulk" (saving
                    // tags and types, and making import faster).
                    if (p.trace)
                    {
                        p.tracef("\n");
                    }
                    p.obj(sym);
                    objcount++;
                } 

                // indicate end of list

                n = n__prev1;
            }

            if (p.trace)
            {
                p.tracef("\n");
            }
            p.tag(endTag); 

            // for self-verification only (redundant)
            p.@int(objcount); 

            // --- compiler-specific export data ---

            if (p.trace)
            {
                p.tracef("\n--- compiler-specific export data ---\n[ ");
                if (p.indent != 0L)
                {
                    Fatalf("exporter: incorrect indentation");
                }
            } 

            // write compiler-specific flags
            if (p.trace)
            {
                p.tracef("\n");
            } 

            // Phase 2: Export objects added to exportlist during phase 1.
            // Don't use range since exportlist may grow during this phase
            // and we want to export all remaining objects.
            objcount = 0L;
            {
                var i__prev1 = i;

                for (var i = numglobals; exportInlined && i < len(exportlist); i++)
                {
                    var n = exportlist[i];
                    sym = n.Sym; 

                    // TODO(gri) The rest of this loop body is identical with
                    // the loop body above. Leave alone for now since there
                    // are different optimization opportunities, but factor
                    // eventually.

                    if (sym.Exported())
                    {
                        continue;
                    }
                    sym.SetExported(true); 

                    // TODO(gri) Closures have dots in their names;
                    // e.g., TestFloatZeroValue.func1 in math/big tests.
                    if (strings.Contains(sym.Name, "."))
                    {
                        Fatalf("exporter: unexpected symbol: %v", sym);
                    }
                    if (sym.Def == null)
                    {
                        Fatalf("exporter: unknown export symbol: %v", sym);
                    } 

                    // TODO(gri) Optimization: Probably worthwhile collecting
                    // long runs of constants and export them "in bulk" (saving
                    // tags and types, and making import faster).
                    if (p.trace)
                    {
                        p.tracef("\n");
                    }
                    if (IsAlias(sym))
                    {
                        Fatalf("exporter: unexpected type alias %v in inlined function body", sym);
                    }
                    p.obj(sym);
                    objcount++;
                } 

                // indicate end of list


                i = i__prev1;
            } 

            // indicate end of list
            if (p.trace)
            {
                p.tracef("\n");
            }
            p.tag(endTag); 

            // for self-verification only (redundant)
            p.@int(objcount); 

            // --- inlined function bodies ---

            if (p.trace)
            {
                p.tracef("\n--- inlined function bodies ---\n");
                if (p.indent != 0L)
                {
                    Fatalf("exporter: incorrect indentation");
                }
            } 

            // write inlineable function bodies
            // Don't use range since funcList may grow.
            objcount = 0L;
            {
                var i__prev1 = i;

                for (i = 0L; i < len(p.funcList); i++)
                {
                    {
                        var f = p.funcList[i];

                        if (f != null)
                        { 
                            // function has inlineable body:
                            // write index and body
                            if (p.trace)
                            {
                                p.tracef("\n----\nfunc { %#v }\n", f.Inl);
                            }
                            p.@int(i);
                            p.@int(int(f.InlCost));
                            p.stmtList(f.Inl);
                            if (p.trace)
                            {
                                p.tracef("\n");
                            }
                            objcount++;
                        }

                    }
                } 

                // indicate end of list


                i = i__prev1;
            } 

            // indicate end of list
            if (p.trace)
            {
                p.tracef("\n");
            }
            p.@int(-1L); // invalid index terminates list

            // for self-verification only (redundant)
            p.@int(objcount);

            if (p.trace)
            {
                p.tracef("\n--- end ---\n");
            } 

            // --- end of export data ---
            return p.written;
        }

        private static void pkg(this ref exporter _p, ref types.Pkg _pkg) => func(_p, _pkg, (ref exporter p, ref types.Pkg pkg, Defer defer, Panic _, Recover __) =>
        {
            if (pkg == null)
            {
                Fatalf("exporter: unexpected nil pkg");
            } 

            // if we saw the package before, write its index (>= 0)
            {
                var (i, ok) = p.pkgIndex[pkg];

                if (ok)
                {
                    p.index('P', i);
                    return;
                } 

                // otherwise, remember the package, write the package tag (< 0) and package data

            } 

            // otherwise, remember the package, write the package tag (< 0) and package data
            if (p.trace)
            {
                p.tracef("P%d = { ", len(p.pkgIndex));
                defer(p.tracef("} "));
            }
            p.pkgIndex[pkg] = len(p.pkgIndex);

            p.tag(packageTag);
            p.@string(pkg.Name);
            p.path(pkg.Path);
        });

        private static ref types.Type unidealType(ref types.Type typ, Val val)
        { 
            // Untyped (ideal) constants get their own type. This decouples
            // the constant type from the encoding of the constant value.
            if (typ == null || typ.IsUntyped())
            {
                typ = untype(val.Ctype());
            }
            return typ;
        }

        // markType recursively visits types reachable from t to identify
        // functions whose inline bodies may be needed.
        private static void markType(this ref exporter p, ref types.Type t)
        {
            if (p.marked[t])
            {
                return;
            }
            p.marked[t] = true; 

            // If this is a named type, mark all of its associated
            // methods. Skip interface types because t.Methods contains
            // only their unexpanded method set (i.e., exclusive of
            // interface embeddings), and the switch statement below
            // handles their full method set.
            if (t.Sym != null && t.Etype != TINTER)
            {
                foreach (var (_, m) in t.Methods().Slice())
                {
                    if (exportname(m.Sym.Name))
                    {
                        p.markType(m.Type);
                    }
                }
            } 

            // Recursively mark any types that can be produced given a
            // value of type t: dereferencing a pointer; indexing an
            // array, slice, or map; receiving from a channel; accessing a
            // struct field or interface method; or calling a function.
            //
            // Notably, we don't mark map key or function parameter types,
            // because the user already needs some way to construct values
            // of those types.
            //
            // It's not critical for correctness that this algorithm is
            // perfect. Worst case, we might miss opportunities to inline
            // some function calls in downstream packages.

            if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TCHAN) 
                p.markType(t.Elem());
            else if (t.Etype == TMAP) 
                p.markType(t.Val());
            else if (t.Etype == TSTRUCT) 
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        if (exportname(f.Sym.Name) || f.Embedded != 0L)
                        {
                            p.markType(f.Type);
                        }
                    }

                    f = f__prev1;
                }
            else if (t.Etype == TFUNC) 
                // If t is the type of a function or method, then
                // t.Nname() is its ONAME. Mark its inline body and
                // any recursively called functions for export.
                inlFlood(asNode(t.Nname()));

                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.Results().FieldSlice())
                    {
                        f = __f;
                        p.markType(f.Type);
                    }

                    f = f__prev1;
                }
            else if (t.Etype == TINTER) 
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        if (exportname(f.Sym.Name))
                        {
                            p.markType(f.Type);
                        }
                    }

                    f = f__prev1;
                }
                    }

        private static void obj(this ref exporter p, ref types.Sym sym)
        { 
            // Exported objects may be from different packages because they
            // may be re-exported via an exported alias or as dependencies in
            // exported inlined function bodies. Thus, exported object names
            // must be fully qualified.
            //
            // (This can only happen for aliased objects or during phase 2
            // (exportInlined enabled) of object export. Unaliased Objects
            // exported in phase 1 (compiler-indendepent objects) are by
            // definition only the objects from the current package and not
            // pulled in via inlined function bodies. In that case the package
            // qualifier is not needed. Possible space optimization.)

            var n = asNode(sym.Def);

            if (n.Op == OLITERAL) 
                // constant
                // TODO(gri) determine if we need the typecheck call here
                n = typecheck(n, Erv);
                if (n == null || n.Op != OLITERAL)
                {
                    Fatalf("exporter: dumpexportconst: oconst nil: %v", sym);
                }
                p.tag(constTag);
                p.pos(n); 
                // TODO(gri) In inlined functions, constants are used directly
                // so they should never occur as re-exported objects. We may
                // not need the qualified name here. See also comment above.
                // Possible space optimization.
                p.qualifiedName(sym);
                p.typ(unidealType(n.Type, n.Val()));
                p.value(n.Val());
            else if (n.Op == OTYPE) 
                // named type
                var t = n.Type;
                if (t.Etype == TFORW)
                {
                    Fatalf("exporter: export of incomplete type %v", sym);
                }
                if (IsAlias(sym))
                {
                    p.tag(aliasTag);
                    p.pos(n);
                    p.qualifiedName(sym);
                }
                else
                {
                    p.tag(typeTag);
                }
                p.typ(t);
            else if (n.Op == ONAME) 
                // variable or function
                n = typecheck(n, Erv | Ecall);
                if (n == null || n.Type == null)
                {
                    Fatalf("exporter: variable/function exported but not defined: %v", sym);
                }
                if (n.Type.Etype == TFUNC && n.Class() == PFUNC)
                { 
                    // function
                    p.tag(funcTag);
                    p.pos(n);
                    p.qualifiedName(sym);

                    var sig = asNode(sym.Def).Type;
                    var inlineable = isInlineable(asNode(sym.Def));

                    p.paramList(sig.Params(), inlineable);
                    p.paramList(sig.Results(), inlineable);

                    ref Func f = default;
                    if (inlineable && asNode(sym.Def).Func.ExportInline())
                    {
                        f = asNode(sym.Def).Func; 
                        // TODO(gri) re-examine reexportdeplist:
                        // Because we can trivially export types
                        // in-place, we don't need to collect types
                        // inside function bodies in the exportlist.
                        // With an adjusted reexportdeplist used only
                        // by the binary exporter, we can also avoid
                        // the global exportlist.
                        reexportdeplist(f.Inl);
                    }
                    p.funcList = append(p.funcList, f);
                }
                else
                { 
                    // variable
                    p.tag(varTag);
                    p.pos(n);
                    p.qualifiedName(sym);
                    p.typ(asNode(sym.Def).Type);
                }
            else 
                Fatalf("exporter: unexpected export symbol: %v %v", n.Op, sym);
                    }

        // deltaNewFile is a magic line delta offset indicating a new file.
        // We use -64 because it is rare; see issue 20080 and CL 41619.
        // -64 is the smallest int that fits in a single byte as a varint.
        private static readonly long deltaNewFile = -64L;



        private static void pos(this ref exporter p, ref Node n)
        {
            if (!p.posInfoFormat)
            {
                return;
            }
            var (file, line) = fileLine(n);
            if (file == p.prevFile)
            { 
                // common case: write line delta
                // delta == deltaNewFile means different file
                // if the actual line delta is deltaNewFile,
                // follow up with a negative int to indicate that.
                // only non-negative ints can follow deltaNewFile
                // when writing a new file.
                var delta = line - p.prevLine;
                p.@int(delta);
                if (delta == deltaNewFile)
                {
                    p.@int(-1L); // -1 means no file change
                }
            }
            else
            { 
                // different file
                p.@int(deltaNewFile);
                p.@int(line); // line >= 0
                p.path(file);
                p.prevFile = file;
            }
            p.prevLine = line;
        }

        private static void path(this ref exporter p, @string s)
        {
            {
                var (i, ok) = p.pathIndex[s];

                if (ok)
                { 
                    // Note: Using p.index(i) here requires the use of p.tag(-len(c)) below
                    //       to get matching debug markers ('t'). But in trace mode p.tag
                    //       assumes that the tag argument is a valid tag that can be looked
                    //       up in the tagString list, rather then some arbitrary slice length.
                    //       Use p.int instead.
                    p.@int(i); // i >= 0
                    return;
                }

            }
            p.pathIndex[s] = len(p.pathIndex);
            var c = strings.Split(s, "/");
            p.@int(-len(c)); // -len(c) < 0
            foreach (var (_, x) in c)
            {
                p.@string(x);
            }
        }

        private static (@string, long) fileLine(ref Node n)
        {
            if (n != null)
            {
                var pos = Ctxt.PosTable.Pos(n.Pos);
                file = pos.Base().AbsFilename();
                line = int(pos.RelLine());
            }
            return;
        }

        private static bool isInlineable(ref Node n)
        {
            if (exportInlined && n != null && n.Func != null)
            { 
                // When lazily typechecking inlined bodies, some
                // re-exported ones may not have been typechecked yet.
                // Currently that can leave unresolved ONONAMEs in
                // import-dot-ed packages in the wrong package.
                //
                // TODO(mdempsky): Having the ExportInline check here
                // instead of the outer if statement means we end up
                // exporting parameter names even for functions whose
                // inline body won't be exported by this package. This
                // is currently necessary because we might first
                // import a function/method from a package where it
                // doesn't need to be re-exported, and then from a
                // package where it does. If this happens, we'll need
                // the parameter names.
                //
                // We could initially do without the parameter names,
                // and then fill them in when importing the inline
                // body. But parameter names are attached to the
                // function type, and modifying types after the fact
                // is a little sketchy.
                if (Debug_typecheckinl == 0L && n.Func.ExportInline())
                {
                    typecheckinl(n);
                }
                return true;
            }
            return false;
        }

        private static void typ(this ref exporter _p, ref types.Type _t) => func(_p, _t, (ref exporter p, ref types.Type t, Defer defer, Panic _, Recover __) =>
        {
            if (t == null)
            {
                Fatalf("exporter: nil type");
            } 

            // Possible optimization: Anonymous pointer types *T where
            // T is a named type are common. We could canonicalize all
            // such types *T to a single type PT = *T. This would lead
            // to at most one *T entry in typIndex, and all future *T's
            // would be encoded as the respective index directly. Would
            // save 1 byte (pointerTag) per *T and reduce the typIndex
            // size (at the cost of a canonicalization map). We can do
            // this later, without encoding format change.

            // if we saw the type before, write its index (>= 0)
            {
                var (i, ok) = p.typIndex[t];

                if (ok)
                {
                    p.index('T', i);
                    return;
                } 

                // otherwise, remember the type, write the type tag (< 0) and type data

            } 

            // otherwise, remember the type, write the type tag (< 0) and type data
            if (trackAllTypes)
            {
                if (p.trace)
                {
                    p.tracef("T%d = {>\n", len(p.typIndex));
                    defer(p.tracef("<\n} "));
                }
                p.typIndex[t] = len(p.typIndex);
            } 

            // pick off named types
            {
                var tsym = t.Sym;

                if (tsym != null)
                {
                    if (!trackAllTypes)
                    { 
                        // if we don't track all types, track named types now
                        p.typIndex[t] = len(p.typIndex);
                    } 

                    // Predeclared types should have been found in the type map.
                    if (t.Orig == t)
                    {
                        Fatalf("exporter: predeclared type missing from type map?");
                    }
                    var n = typenod(t);
                    if (n.Type != t)
                    {
                        Fatalf("exporter: named type definition incorrectly set up");
                    }
                    p.tag(namedTag);
                    p.pos(n);
                    p.qualifiedName(tsym); 

                    // write underlying type
                    p.typ(t.Orig); 

                    // interfaces don't have associated methods
                    if (t.Orig.IsInterface())
                    {
                        return;
                    } 

                    // sort methods for reproducible export format
                    // TODO(gri) Determine if they are already sorted
                    // in which case we can drop this step.
                    slice<ref types.Field> methods = default;
                    methods = append(methods, t.Methods().Slice());
                    sort.Sort(methodbyname(methods));
                    p.@int(len(methods));

                    if (p.trace && len(methods) > 0L)
                    {
                        p.tracef("associated methods {>");
                    }
                    foreach (var (_, m) in methods)
                    {
                        if (p.trace)
                        {
                            p.tracef("\n");
                        }
                        if (strings.Contains(m.Sym.Name, "."))
                        {
                            Fatalf("invalid symbol name: %s (%v)", m.Sym.Name, m.Sym);
                        }
                        p.pos(asNode(m.Nname));
                        p.fieldSym(m.Sym, false);

                        var sig = m.Type;
                        var mfn = asNode(sig.FuncType().Nname);
                        var inlineable = isInlineable(mfn);

                        p.paramList(sig.Recvs(), inlineable);
                        p.paramList(sig.Params(), inlineable);
                        p.paramList(sig.Results(), inlineable);
                        p.@bool(m.Nointerface()); // record go:nointerface pragma value (see also #16243)

                        ref Func f = default;
                        if (inlineable && mfn.Func.ExportInline())
                        {
                            f = mfn.Func;
                            reexportdeplist(mfn.Func.Inl);
                        }
                        p.funcList = append(p.funcList, f);
                    }
                    if (p.trace && len(methods) > 0L)
                    {
                        p.tracef("<\n} ");
                    }
                    return;
                } 

                // otherwise we have a type literal

            } 

            // otherwise we have a type literal

            if (t.Etype == TARRAY) 
                if (t.IsDDDArray())
                {
                    Fatalf("array bounds should be known at export time: %v", t);
                }
                p.tag(arrayTag);
                p.int64(t.NumElem());
                p.typ(t.Elem());
            else if (t.Etype == TSLICE) 
                p.tag(sliceTag);
                p.typ(t.Elem());
            else if (t.Etype == TDDDFIELD) 
                // see p.param use of TDDDFIELD
                p.tag(dddTag);
                p.typ(t.DDDField());
            else if (t.Etype == TSTRUCT) 
                p.tag(structTag);
                p.fieldList(t);
            else if (t.Etype == TPTR32 || t.Etype == TPTR64) // could use Tptr but these are constants
                p.tag(pointerTag);
                p.typ(t.Elem());
            else if (t.Etype == TFUNC) 
                p.tag(signatureTag);
                p.paramList(t.Params(), false);
                p.paramList(t.Results(), false);
            else if (t.Etype == TINTER) 
                p.tag(interfaceTag);
                p.methodList(t);
            else if (t.Etype == TMAP) 
                p.tag(mapTag);
                p.typ(t.Key());
                p.typ(t.Val());
            else if (t.Etype == TCHAN) 
                p.tag(chanTag);
                p.@int(int(t.ChanDir()));
                p.typ(t.Elem());
            else 
                Fatalf("exporter: unexpected type: %v (Etype = %d)", t, t.Etype);
                    });

        private static void qualifiedName(this ref exporter p, ref types.Sym sym)
        {
            p.@string(sym.Name);
            p.pkg(sym.Pkg);
        }

        private static void fieldList(this ref exporter _p, ref types.Type _t) => func(_p, _t, (ref exporter p, ref types.Type t, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace && t.NumFields() > 0L)
            {
                p.tracef("fields {>");
                defer(p.tracef("<\n} "));
            }
            p.@int(t.NumFields());
            foreach (var (_, f) in t.Fields().Slice())
            {
                if (p.trace)
                {
                    p.tracef("\n");
                }
                p.field(f);
            }
        });

        private static void field(this ref exporter p, ref types.Field f)
        {
            p.pos(asNode(f.Nname));
            p.fieldName(f);
            p.typ(f.Type);
            p.@string(f.Note);
        }

        private static void methodList(this ref exporter p, ref types.Type t)
        {
            slice<ref types.Field> embeddeds = default;            slice<ref types.Field> methods = default;



            {
                var m__prev1 = m;

                foreach (var (_, __m) in t.Methods().Slice())
                {
                    m = __m;
                    if (m.Sym != null)
                    {
                        methods = append(methods, m);
                    }
                    else
                    {
                        embeddeds = append(embeddeds, m);
                    }
                }

                m = m__prev1;
            }

            if (p.trace && len(embeddeds) > 0L)
            {
                p.tracef("embeddeds {>");
            }
            p.@int(len(embeddeds));
            {
                var m__prev1 = m;

                foreach (var (_, __m) in embeddeds)
                {
                    m = __m;
                    if (p.trace)
                    {
                        p.tracef("\n");
                    }
                    p.pos(asNode(m.Nname));
                    p.typ(m.Type);
                }

                m = m__prev1;
            }

            if (p.trace && len(embeddeds) > 0L)
            {
                p.tracef("<\n} ");
            }
            if (p.trace && len(methods) > 0L)
            {
                p.tracef("methods {>");
            }
            p.@int(len(methods));
            {
                var m__prev1 = m;

                foreach (var (_, __m) in methods)
                {
                    m = __m;
                    if (p.trace)
                    {
                        p.tracef("\n");
                    }
                    p.method(m);
                }

                m = m__prev1;
            }

            if (p.trace && len(methods) > 0L)
            {
                p.tracef("<\n} ");
            }
        }

        private static void method(this ref exporter p, ref types.Field m)
        {
            p.pos(asNode(m.Nname));
            p.methodName(m.Sym);
            p.paramList(m.Type.Params(), false);
            p.paramList(m.Type.Results(), false);
        }

        private static void fieldName(this ref exporter p, ref types.Field t)
        {
            var name = t.Sym.Name;
            if (t.Embedded != 0L)
            { 
                // anonymous field - we distinguish between 3 cases:
                // 1) field name matches base type name and is exported
                // 2) field name matches base type name and is not exported
                // 3) field name doesn't match base type name (alias name)
                var bname = basetypeName(t.Type);
                if (name == bname)
                {
                    if (exportname(name))
                    {
                        name = ""; // 1) we don't need to know the field name or package
                    }
                    else
                    {
                        name = "?"; // 2) use unexported name "?" to force package export
                    }
                }
                else
                { 
                    // 3) indicate alias and export name as is
                    // (this requires an extra "@" but this is a rare case)
                    p.@string("@");
                }
            }
            p.@string(name);
            if (name != "" && !exportname(name))
            {
                p.pkg(t.Sym.Pkg);
            }
        }

        // methodName is like qualifiedName but it doesn't record the package for exported names.
        private static void methodName(this ref exporter p, ref types.Sym sym)
        {
            p.@string(sym.Name);
            if (!exportname(sym.Name))
            {
                p.pkg(sym.Pkg);
            }
        }

        private static @string basetypeName(ref types.Type t)
        {
            var s = t.Sym;
            if (s == null && t.IsPtr())
            {
                s = t.Elem().Sym; // deref
            }
            if (s != null)
            {
                return s.Name;
            }
            return ""; // unnamed type
        }

        private static void paramList(this ref exporter p, ref types.Type @params, bool numbered)
        {
            if (!@params.IsFuncArgStruct())
            {
                Fatalf("exporter: parameter list expected");
            } 

            // use negative length to indicate unnamed parameters
            // (look at the first parameter only since either all
            // names are present or all are absent)
            //
            // TODO(gri) If we don't have an exported function
            // body, the parameter names are irrelevant for the
            // compiler (though they may be of use for other tools).
            // Possible space optimization.
            var n = @params.NumFields();
            if (n > 0L && parName(@params.Field(0L), numbered) == "")
            {
                n = -n;
            }
            p.@int(n);
            foreach (var (_, q) in @params.Fields().Slice())
            {
                p.param(q, n, numbered);
            }
        }

        private static void param(this ref exporter p, ref types.Field q, long n, bool numbered)
        {
            var t = q.Type;
            if (q.Isddd())
            { 
                // create a fake type to encode ... just for the p.typ call
                t = types.NewDDDField(t.Elem());
            }
            p.typ(t);
            if (n > 0L)
            {
                var name = parName(q, numbered);
                if (name == "")
                { 
                    // Sometimes we see an empty name even for n > 0.
                    // This appears to happen for interface methods
                    // with _ (blank) parameter names. Make sure we
                    // have a proper name and package so we don't crash
                    // during import (see also issue #15470).
                    // (parName uses "" instead of "?" as in fmt.go)
                    // TODO(gri) review parameter name encoding
                    name = "_";
                }
                p.@string(name);
                if (name != "_")
                { 
                    // Because of (re-)exported inlined functions
                    // the importpkg may not be the package to which this
                    // function (and thus its parameter) belongs. We need to
                    // supply the parameter package here. We need the package
                    // when the function is inlined so we can properly resolve
                    // the name. The _ (blank) parameter cannot be accessed, so
                    // we don't need to export a package.
                    //
                    // TODO(gri) This is compiler-specific. Try using importpkg
                    // here and then update the symbols if we find an inlined
                    // body only. Otherwise, the parameter name is ignored and
                    // the package doesn't matter. This would remove an int
                    // (likely 1 byte) for each named parameter.
                    p.pkg(q.Sym.Pkg);
                }
            } 
            // TODO(gri) This is compiler-specific (escape info).
            // Move into compiler-specific section eventually?
            // (Not having escape info causes tests to fail, e.g. runtime GCInfoTest)
            p.@string(q.Note);
        }

        private static @string parName(ref types.Field f, bool numbered)
        {
            var s = f.Sym;
            if (s == null)
            {
                return "";
            } 

            // Take the name from the original, lest we substituted it with ~r%d or ~b%d.
            // ~r%d is a (formerly) unnamed result.
            if (asNode(f.Nname) != null)
            {
                if (asNode(f.Nname).Orig == null)
                {
                    return ""; // s = nil
                }
                s = asNode(f.Nname).Orig.Sym;
                if (s != null && s.Name[0L] == '~')
                {
                    if (s.Name[1L] == 'r')
                    { // originally an unnamed result
                        return ""; // s = nil
                    }
                    else if (s.Name[1L] == 'b')
                    { // originally the blank identifier _
                        return "_"; // belongs to localpkg
                    }
                }
            }
            if (s == null)
            {
                return "";
            } 

            // print symbol with Vargen number or not as desired
            var name = s.Name;
            if (strings.Contains(name, "."))
            {
                Fatalf("invalid symbol name: %s", name);
            } 

            // Functions that can be inlined use numbered parameters so we can distinguish them
            // from other names in their context after inlining (i.e., the parameter numbering
            // is a form of parameter rewriting). See issue 4326 for an example and test case.
            if (numbered)
            {
                if (!strings.Contains(name, "·") && asNode(f.Nname) != null && asNode(f.Nname).Name != null && asNode(f.Nname).Name.Vargen > 0L)
                {
                    name = fmt.Sprintf("%s·%d", name, asNode(f.Nname).Name.Vargen); // append Vargen
                }
            }
            else
            {
                {
                    var i = strings.Index(name, "·");

                    if (i > 0L)
                    {
                        name = name[..i]; // cut off Vargen
                    }

                }
            }
            return name;
        }

        private static void value(this ref exporter p, Val x)
        {
            if (p.trace)
            {
                p.tracef("= ");
            }
            switch (x.U.type())
            {
                case bool x:
                    var tag = falseTag;
                    if (x)
                    {
                        tag = trueTag;
                    }
                    p.tag(tag);
                    break;
                case ref Mpint x:
                    if (minintval[TINT64].Cmp(x) <= 0L && x.Cmp(maxintval[TINT64]) <= 0L)
                    { 
                        // common case: x fits into an int64 - use compact encoding
                        p.tag(int64Tag);
                        p.int64(x.Int64());
                        return;
                    } 
                    // uncommon case: large x - use float encoding
                    // (powers of 2 will be encoded efficiently with exponent)
                    var f = newMpflt();
                    f.SetInt(x);
                    p.tag(floatTag);
                    p.@float(f);
                    break;
                case ref Mpflt x:
                    p.tag(floatTag);
                    p.@float(x);
                    break;
                case ref Mpcplx x:
                    p.tag(complexTag);
                    p.@float(ref x.Real);
                    p.@float(ref x.Imag);
                    break;
                case @string x:
                    p.tag(stringTag);
                    p.@string(x);
                    break;
                case ref NilVal x:
                    p.tag(nilTag);
                    break;
                default:
                {
                    var x = x.U.type();
                    Fatalf("exporter: unexpected value %v (%T)", x, x);
                    break;
                }
            }
        }

        private static void @float(this ref exporter p, ref Mpflt x)
        { 
            // extract sign (there is no -0)
            var f = ref x.Val;
            var sign = f.Sign();
            if (sign == 0L)
            { 
                // x == 0
                p.@int(0L);
                return;
            } 
            // x != 0

            // extract exponent such that 0.5 <= m < 1.0
            big.Float m = default;
            var exp = f.MantExp(ref m); 

            // extract mantissa as *big.Int
            // - set exponent large enough so mant satisfies mant.IsInt()
            // - get *big.Int from mant
            m.SetMantExp(ref m, int(m.MinPrec()));
            var (mant, acc) = m.Int(null);
            if (acc != big.Exact)
            {
                Fatalf("exporter: internal error");
            }
            p.@int(sign);
            p.@int(exp);
            p.@string(string(mant.Bytes()));
        }

        // ----------------------------------------------------------------------------
        // Inlined function bodies

        // Approach: More or less closely follow what fmt.go is doing for FExp mode
        // but instead of emitting the information textually, emit the node tree in
        // binary form.

        // TODO(gri) Improve tracing output. The current format is difficult to read.

        // stmtList may emit more (or fewer) than len(list) nodes.
        private static void stmtList(this ref exporter _p, Nodes list) => func(_p, (ref exporter p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                if (list.Len() == 0L)
                {
                    p.tracef("{}");
                }
                else
                {
                    p.tracef("{>");
                    defer(p.tracef("<\n}"));
                }
            }
            foreach (var (_, n) in list.Slice())
            {
                if (p.trace)
                {
                    p.tracef("\n");
                } 
                // TODO inlining produces expressions with ninits. we can't export these yet.
                // (from fmt.go:1461ff)
                if (opprec[n.Op] < 0L)
                {
                    p.stmt(n);
                }
                else
                {
                    p.expr(n);
                }
            }
            p.op(OEND);
        });

        private static void exprList(this ref exporter _p, Nodes list) => func(_p, (ref exporter p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                if (list.Len() == 0L)
                {
                    p.tracef("{}");
                }
                else
                {
                    p.tracef("{>");
                    defer(p.tracef("<\n}"));
                }
            }
            foreach (var (_, n) in list.Slice())
            {
                if (p.trace)
                {
                    p.tracef("\n");
                }
                p.expr(n);
            }
            p.op(OEND);
        });

        private static void elemList(this ref exporter _p, Nodes list) => func(_p, (ref exporter p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                p.tracef("[ ");
            }
            p.@int(list.Len());
            if (p.trace)
            {
                if (list.Len() == 0L)
                {
                    p.tracef("] {}");
                }
                else
                {
                    p.tracef("] {>");
                    defer(p.tracef("<\n}"));
                }
            }
            foreach (var (_, n) in list.Slice())
            {
                if (p.trace)
                {
                    p.tracef("\n");
                }
                p.fieldSym(n.Sym, false);
                p.expr(n.Left);
            }
        });

        private static void expr(this ref exporter _p, ref Node _n) => func(_p, _n, (ref exporter p, ref Node n, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                p.tracef("( ");
                defer(p.tracef(") "));
            } 

            // from nodefmt (fmt.go)
            //
            // nodefmt reverts nodes back to their original - we don't need to do
            // it because we are not bound to produce valid Go syntax when exporting
            //
            // if (fmtmode != FExp || n.Op != OLITERAL) && n.Orig != nil {
            //     n = n.Orig
            // }

            // from exprfmt (fmt.go)
            while (n != null && n.Implicit() && (n.Op == OIND || n.Op == OADDR))
            {
                n = n.Left;
            }


            {
                var op = n.Op;


                // expressions
                // (somewhat closely following the structure of exprfmt in fmt.go)
                if (op == OPAREN) 
                    p.expr(n.Left); // unparen

                    // case ODDDARG:
                    //    unimplemented - handled by default case
                else if (op == OLITERAL) 
                    if (n.Val().Ctype() == CTNIL && n.Orig != null && n.Orig != n)
                    {
                        p.expr(n.Orig);
                        break;
                    }
                    p.op(OLITERAL);
                    p.pos(n);
                    p.typ(unidealType(n.Type, n.Val()));
                    p.value(n.Val());
                else if (op == ONAME) 
                    // Special case: explicit name of func (*T) method(...) is turned into pkg.(*T).method,
                    // but for export, this should be rendered as (*pkg.T).meth.
                    // These nodes have the special property that they are names with a left OTYPE and a right ONAME.
                    if (n.isMethodExpression())
                    {
                        p.op(OXDOT);
                        p.pos(n);
                        p.expr(n.Left); // n.Left.Op == OTYPE
                        p.fieldSym(n.Right.Sym, true);
                        break;
                    }
                    p.op(ONAME);
                    p.pos(n);
                    p.sym(n); 

                    // case OPACK, ONONAME:
                    //     should have been resolved by typechecking - handled by default case
                else if (op == OTYPE) 
                    p.op(OTYPE);
                    p.pos(n);
                    p.typ(n.Type); 

                    // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
                    //     should have been resolved by typechecking - handled by default case

                    // case OCLOSURE:
                    //    unimplemented - handled by default case

                    // case OCOMPLIT:
                    //     should have been resolved by typechecking - handled by default case
                else if (op == OPTRLIT) 
                    p.op(OPTRLIT);
                    p.pos(n);
                    p.expr(n.Left);
                    p.@bool(n.Implicit());
                else if (op == OSTRUCTLIT) 
                    p.op(OSTRUCTLIT);
                    p.pos(n);
                    p.typ(n.Type);
                    p.elemList(n.List); // special handling of field names
                else if (op == OARRAYLIT || op == OSLICELIT || op == OMAPLIT) 
                    p.op(OCOMPLIT);
                    p.pos(n);
                    p.typ(n.Type);
                    p.exprList(n.List);
                else if (op == OKEY) 
                    p.op(OKEY);
                    p.pos(n);
                    p.exprsOrNil(n.Left, n.Right); 

                    // case OSTRUCTKEY:
                    //    unreachable - handled in case OSTRUCTLIT by elemList

                    // case OCALLPART:
                    //    unimplemented - handled by default case
                else if (op == OXDOT || op == ODOT || op == ODOTPTR || op == ODOTINTER || op == ODOTMETH) 
                    p.op(OXDOT);
                    p.pos(n);
                    p.expr(n.Left);
                    p.fieldSym(n.Sym, true);
                else if (op == ODOTTYPE || op == ODOTTYPE2) 
                    p.op(ODOTTYPE);
                    p.pos(n);
                    p.expr(n.Left);
                    p.typ(n.Type);
                else if (op == OINDEX || op == OINDEXMAP) 
                    p.op(OINDEX);
                    p.pos(n);
                    p.expr(n.Left);
                    p.expr(n.Right);
                else if (op == OSLICE || op == OSLICESTR || op == OSLICEARR) 
                    p.op(OSLICE);
                    p.pos(n);
                    p.expr(n.Left);
                    var (low, high, _) = n.SliceBounds();
                    p.exprsOrNil(low, high);
                else if (op == OSLICE3 || op == OSLICE3ARR) 
                    p.op(OSLICE3);
                    p.pos(n);
                    p.expr(n.Left);
                    var (low, high, max) = n.SliceBounds();
                    p.exprsOrNil(low, high);
                    p.expr(max);
                else if (op == OCOPY || op == OCOMPLEX) 
                    // treated like other builtin calls (see e.g., OREAL)
                    p.op(op);
                    p.pos(n);
                    p.expr(n.Left);
                    p.expr(n.Right);
                    p.op(OEND);
                else if (op == OCONV || op == OCONVIFACE || op == OCONVNOP || op == OARRAYBYTESTR || op == OARRAYRUNESTR || op == OSTRARRAYBYTE || op == OSTRARRAYRUNE || op == ORUNESTR) 
                    p.op(OCONV);
                    p.pos(n);
                    p.expr(n.Left);
                    p.typ(n.Type);
                else if (op == OREAL || op == OIMAG || op == OAPPEND || op == OCAP || op == OCLOSE || op == ODELETE || op == OLEN || op == OMAKE || op == ONEW || op == OPANIC || op == ORECOVER || op == OPRINT || op == OPRINTN) 
                    p.op(op);
                    p.pos(n);
                    if (n.Left != null)
                    {
                        p.expr(n.Left);
                        p.op(OEND);
                    }
                    else
                    {
                        p.exprList(n.List); // emits terminating OEND
                    } 
                    // only append() calls may contain '...' arguments
                    if (op == OAPPEND)
                    {
                        p.@bool(n.Isddd());
                    }
                    else if (n.Isddd())
                    {
                        Fatalf("exporter: unexpected '...' with %v call", op);
                    }
                else if (op == OCALL || op == OCALLFUNC || op == OCALLMETH || op == OCALLINTER || op == OGETG) 
                    p.op(OCALL);
                    p.pos(n);
                    p.expr(n.Left);
                    p.exprList(n.List);
                    p.@bool(n.Isddd());
                else if (op == OMAKEMAP || op == OMAKECHAN || op == OMAKESLICE) 
                    p.op(op); // must keep separate from OMAKE for importer
                    p.pos(n);
                    p.typ(n.Type);

                    if (n.List.Len() != 0L) // pre-typecheck
                        p.exprList(n.List); // emits terminating OEND
                    else if (n.Right != null) 
                        p.expr(n.Left);
                        p.expr(n.Right);
                        p.op(OEND);
                    else if (n.Left != null && (n.Op == OMAKESLICE || !n.Left.Type.IsUntyped())) 
                        p.expr(n.Left);
                        p.op(OEND);
                    else 
                        // empty list
                        p.op(OEND);
                    // unary expressions
                else if (op == OPLUS || op == OMINUS || op == OADDR || op == OCOM || op == OIND || op == ONOT || op == ORECV) 
                    p.op(op);
                    p.pos(n);
                    p.expr(n.Left); 

                    // binary expressions
                else if (op == OADD || op == OAND || op == OANDAND || op == OANDNOT || op == ODIV || op == OEQ || op == OGE || op == OGT || op == OLE || op == OLT || op == OLSH || op == OMOD || op == OMUL || op == ONE || op == OOR || op == OOROR || op == ORSH || op == OSEND || op == OSUB || op == OXOR) 
                    p.op(op);
                    p.pos(n);
                    p.expr(n.Left);
                    p.expr(n.Right);
                else if (op == OADDSTR) 
                    p.op(OADDSTR);
                    p.pos(n);
                    p.exprList(n.List);
                else if (op == OCMPSTR || op == OCMPIFACE) 
                    p.op(Op(n.Etype));
                    p.pos(n);
                    p.expr(n.Left);
                    p.expr(n.Right);
                else if (op == ODCLCONST) 
                    // if exporting, DCLCONST should just be removed as its usage
                    // has already been replaced with literals
                    // TODO(gri) these should not be exported in the first place
                    // TODO(gri) why is this considered an expression in fmt.go?
                    p.op(ODCLCONST);
                    p.pos(n);
                else 
                    Fatalf("cannot export %v (%d) node\n" + "==> please file an issue and assign to gri@\n", n.Op, int(n.Op));

            }
        });

        // Caution: stmt will emit more than one node for statement nodes n that have a non-empty
        // n.Ninit and where n cannot have a natural init section (such as in "if", "for", etc.).
        private static void stmt(this ref exporter _p, ref Node _n) => func(_p, _n, (ref exporter p, ref Node n, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                p.tracef("( ");
                defer(p.tracef(") "));
            }
            if (n.Ninit.Len() > 0L && !stmtwithinit(n.Op))
            {
                if (p.trace)
                {
                    p.tracef("( /* Ninits */ ");
                } 

                // can't use stmtList here since we don't want the final OEND
                foreach (var (_, n) in n.Ninit.Slice())
                {
                    p.stmt(n);
                }
                if (p.trace)
                {
                    p.tracef(") ");
                }
            }
            {
                var op = n.Op;


                if (op == ODCL) 
                    p.op(ODCL);
                    p.pos(n);
                    p.sym(n.Left);
                    p.typ(n.Left.Type); 

                    // case ODCLFIELD:
                    //    unimplemented - handled by default case
                else if (op == OAS) 
                    // Don't export "v = <N>" initializing statements, hope they're always
                    // preceded by the DCL which will be re-parsed and typecheck to reproduce
                    // the "v = <N>" again.
                    if (n.Right != null)
                    {
                        p.op(OAS);
                        p.pos(n);
                        p.expr(n.Left);
                        p.expr(n.Right);
                    }
                else if (op == OASOP) 
                    p.op(OASOP);
                    p.pos(n);
                    p.@int(int(n.Etype));
                    p.expr(n.Left);
                    if (p.@bool(!n.Implicit()))
                    {
                        p.expr(n.Right);
                    }
                else if (op == OAS2 || op == OAS2DOTTYPE || op == OAS2FUNC || op == OAS2MAPR || op == OAS2RECV) 
                    p.op(OAS2);
                    p.pos(n);
                    p.exprList(n.List);
                    p.exprList(n.Rlist);
                else if (op == ORETURN) 
                    p.op(ORETURN);
                    p.pos(n);
                    p.exprList(n.List); 

                    // case ORETJMP:
                    //     unreachable - generated by compiler for trampolin routines
                else if (op == OPROC || op == ODEFER) 
                    p.op(op);
                    p.pos(n);
                    p.expr(n.Left);
                else if (op == OIF) 
                    p.op(OIF);
                    p.pos(n);
                    p.stmtList(n.Ninit);
                    p.expr(n.Left);
                    p.stmtList(n.Nbody);
                    p.stmtList(n.Rlist);
                else if (op == OFOR) 
                    p.op(OFOR);
                    p.pos(n);
                    p.stmtList(n.Ninit);
                    p.exprsOrNil(n.Left, n.Right);
                    p.stmtList(n.Nbody);
                else if (op == ORANGE) 
                    p.op(ORANGE);
                    p.pos(n);
                    p.stmtList(n.List);
                    p.expr(n.Right);
                    p.stmtList(n.Nbody);
                else if (op == OSELECT || op == OSWITCH) 
                    p.op(op);
                    p.pos(n);
                    p.stmtList(n.Ninit);
                    p.exprsOrNil(n.Left, null);
                    p.stmtList(n.List);
                else if (op == OCASE || op == OXCASE) 
                    p.op(OXCASE);
                    p.pos(n);
                    p.stmtList(n.List);
                    p.stmtList(n.Nbody);
                else if (op == OFALL) 
                    p.op(OFALL);
                    p.pos(n);
                else if (op == OBREAK || op == OCONTINUE) 
                    p.op(op);
                    p.pos(n);
                    p.exprsOrNil(n.Left, null);
                else if (op == OEMPTY)                 else if (op == OGOTO || op == OLABEL) 
                    p.op(op);
                    p.pos(n);
                    p.expr(n.Left);
                else 
                    Fatalf("exporter: CANNOT EXPORT: %v\nPlease notify gri@\n", n.Op);

            }
        });

        private static void exprsOrNil(this ref exporter p, ref Node a, ref Node b)
        {
            long ab = 0L;
            if (a != null)
            {
                ab |= 1L;
            }
            if (b != null)
            {
                ab |= 2L;
            }
            p.@int(ab);
            if (ab & 1L != 0L)
            {
                p.expr(a);
            }
            if (ab & 2L != 0L)
            {
                p.expr(b);
            }
        }

        private static void fieldSym(this ref exporter p, ref types.Sym s, bool @short)
        {
            var name = s.Name; 

            // remove leading "type." in method names ("(T).m" -> "m")
            if (short)
            {
                {
                    var i = strings.LastIndex(name, ".");

                    if (i >= 0L)
                    {
                        name = name[i + 1L..];
                    }

                }
            } 

            // we should never see a _ (blank) here - these are accessible ("read") fields
            // TODO(gri) can we assert this with an explicit check?
            p.@string(name);
            if (!exportname(name))
            {
                p.pkg(s.Pkg);
            }
        }

        // sym must encode the _ (blank) identifier as a single string "_" since
        // encoding for some nodes is based on this assumption (e.g. ONAME nodes).
        private static void sym(this ref exporter _p, ref Node _n) => func(_p, _n, (ref exporter p, ref Node n, Defer defer, Panic _, Recover __) =>
        {
            var s = n.Sym;
            if (s.Pkg != null)
            {
                if (len(s.Name) > 0L && s.Name[0L] == '.')
                {
                    Fatalf("exporter: exporting synthetic symbol %s", s.Name);
                }
            }
            if (p.trace)
            {
                p.tracef("{ SYM ");
                defer(p.tracef("} "));
            }
            var name = s.Name; 

            // remove leading "type." in method names ("(T).m" -> "m")
            {
                var i__prev1 = i;

                var i = strings.LastIndex(name, ".");

                if (i >= 0L)
                {
                    name = name[i + 1L..];
                }

                i = i__prev1;

            }

            if (strings.Contains(name, "·") && n.Name.Vargen > 0L)
            {
                Fatalf("exporter: unexpected · in symbol name");
            }
            {
                var i__prev1 = i;

                i = n.Name.Vargen;

                if (i > 0L)
                {
                    name = fmt.Sprintf("%s·%d", name, i);
                }

                i = i__prev1;

            }

            p.@string(name);
            if (name != "_")
            {
                p.pkg(s.Pkg);
            } 
            // Fixes issue #18167.
            p.@string(s.Linkname);
        });

        private static bool @bool(this ref exporter _p, bool b) => func(_p, (ref exporter p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                p.tracef("[");
                defer(p.tracef("= %v] ", b));
            }
            long x = 0L;
            if (b)
            {
                x = 1L;
            }
            p.@int(x);
            return b;
        });

        private static void op(this ref exporter _p, Op op) => func(_p, (ref exporter p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                p.tracef("[");
                defer(p.tracef("= %v] ", op));
            }
            p.@int(int(op));
        });

        // ----------------------------------------------------------------------------
        // Low-level encoders

        private static void index(this ref exporter p, byte marker, long index)
        {
            if (index < 0L)
            {
                Fatalf("exporter: invalid index < 0");
            }
            if (debugFormat)
            {
                p.marker('t');
            }
            if (p.trace)
            {
                p.tracef("%c%d ", marker, index);
            }
            p.rawInt64(int64(index));
        }

        private static void tag(this ref exporter p, long tag)
        {
            if (tag >= 0L)
            {
                Fatalf("exporter: invalid tag >= 0");
            }
            if (debugFormat)
            {
                p.marker('t');
            }
            if (p.trace)
            {
                p.tracef("%s ", tagString[-tag]);
            }
            p.rawInt64(int64(tag));
        }

        private static void @int(this ref exporter p, long x)
        {
            p.int64(int64(x));
        }

        private static void int64(this ref exporter p, long x)
        {
            if (debugFormat)
            {
                p.marker('i');
            }
            if (p.trace)
            {
                p.tracef("%d ", x);
            }
            p.rawInt64(x);
        }

        private static void @string(this ref exporter p, @string s)
        {
            if (debugFormat)
            {
                p.marker('s');
            }
            if (p.trace)
            {
                p.tracef("%q ", s);
            } 
            // if we saw the string before, write its index (>= 0)
            // (the empty string is mapped to 0)
            {
                var i__prev1 = i;

                var (i, ok) = p.strIndex[s];

                if (ok)
                {
                    p.rawInt64(int64(i));
                    return;
                } 
                // otherwise, remember string and write its negative length and bytes

                i = i__prev1;

            } 
            // otherwise, remember string and write its negative length and bytes
            p.strIndex[s] = len(p.strIndex);
            p.rawInt64(-int64(len(s)));
            {
                var i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    p.rawByte(s[i]);
                }


                i = i__prev1;
            }
        }

        // marker emits a marker byte and position information which makes
        // it easy for a reader to detect if it is "out of sync". Used only
        // if debugFormat is set.
        private static void marker(this ref exporter p, byte m)
        {
            p.rawByte(m); 
            // Uncomment this for help tracking down the location
            // of an incorrect marker when running in debugFormat.
            // if p.trace {
            //     p.tracef("#%d ", p.written)
            // }
            p.rawInt64(int64(p.written));
        }

        // rawInt64 should only be used by low-level encoders.
        private static void rawInt64(this ref exporter p, long x)
        {
            array<byte> tmp = new array<byte>(binary.MaxVarintLen64);
            var n = binary.PutVarint(tmp[..], x);
            for (long i = 0L; i < n; i++)
            {
                p.rawByte(tmp[i]);
            }

        }

        // rawStringln should only be used to emit the initial version string.
        private static void rawStringln(this ref exporter p, @string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                p.rawByte(s[i]);
            }

            p.rawByte('\n');
        }

        // rawByte is the bottleneck interface to write to p.out.
        // rawByte escapes b as follows (any encoding does that
        // hides '$'):
        //
        //    '$'  => '|' 'S'
        //    '|'  => '|' '|'
        //
        // Necessary so other tools can find the end of the
        // export data by searching for "$$".
        // rawByte should only be used by low-level encoders.
        private static void rawByte(this ref exporter p, byte b)
        {

            if (b == '$') 
            {
                // write '$' as '|' 'S'
                b = 'S';
                fallthrough = true;
            }
            if (fallthrough || b == '|') 
            {
                // write '|' as '|' '|'
                p.@out.WriteByte('|');
                p.written++;
                goto __switch_break0;
            }

            __switch_break0:;
            p.@out.WriteByte(b);
            p.written++;
        }

        // tracef is like fmt.Printf but it rewrites the format string
        // to take care of indentation.
        private static void tracef(this ref exporter p, @string format, params object[] args)
        {
            if (strings.ContainsAny(format, "<>\n"))
            {
                bytes.Buffer buf = default;
                for (long i = 0L; i < len(format); i++)
                { 
                    // no need to deal with runes
                    var ch = format[i];
                    switch (ch)
                    {
                        case '>': 
                            p.indent++;
                            continue;
                            break;
                        case '<': 
                            p.indent--;
                            continue;
                            break;
                    }
                    buf.WriteByte(ch);
                    if (ch == '\n')
                    {
                        for (var j = p.indent; j > 0L; j--)
                        {
                            buf.WriteString(".  ");
                        }

                    }
                }

                format = buf.String();
            }
            fmt.Printf(format, args);
        }

        // ----------------------------------------------------------------------------
        // Export format

        // Tags. Must be < 0.
 
        // Objects
        private static readonly var packageTag = -(iota + 1L);
        private static readonly var constTag = 0;
        private static readonly var typeTag = 1;
        private static readonly var varTag = 2;
        private static readonly var funcTag = 3;
        private static readonly var endTag = 4; 

        // Types
        private static readonly var namedTag = 5;
        private static readonly var arrayTag = 6;
        private static readonly var sliceTag = 7;
        private static readonly var dddTag = 8;
        private static readonly var structTag = 9;
        private static readonly var pointerTag = 10;
        private static readonly var signatureTag = 11;
        private static readonly var interfaceTag = 12;
        private static readonly var mapTag = 13;
        private static readonly var chanTag = 14; 

        // Values
        private static readonly var falseTag = 15;
        private static readonly var trueTag = 16;
        private static readonly var int64Tag = 17;
        private static readonly var floatTag = 18;
        private static readonly var fractionTag = 19; // not used by gc
        private static readonly var complexTag = 20;
        private static readonly var stringTag = 21;
        private static readonly var nilTag = 22;
        private static readonly var unknownTag = 23; // not used by gc (only appears in packages with errors)

        // Type aliases
        private static readonly var aliasTag = 24;

        // Debugging support.
        // (tagString is only used when tracing is enabled)
        private static array<@string> tagString = new array<@string>(InitKeyedValues<@string>((-packageTag, "package"), (-constTag, "const"), (-typeTag, "type"), (-varTag, "var"), (-funcTag, "func"), (-endTag, "end"), (-namedTag, "named type"), (-arrayTag, "array"), (-sliceTag, "slice"), (-dddTag, "ddd"), (-structTag, "struct"), (-pointerTag, "pointer"), (-signatureTag, "signature"), (-interfaceTag, "interface"), (-mapTag, "map"), (-chanTag, "chan"), (-falseTag, "false"), (-trueTag, "true"), (-int64Tag, "int64"), (-floatTag, "float"), (-fractionTag, "fraction"), (-complexTag, "complex"), (-stringTag, "string"), (-nilTag, "nil"), (-unknownTag, "unknown"), (-aliasTag, "alias")));

        // untype returns the "pseudo" untyped type for a Ctype (import/export use only).
        // (we can't use an pre-initialized array because we must be sure all types are
        // set up)
        private static ref types.Type untype(Ctype ctype)
        {

            if (ctype == CTINT) 
                return types.Idealint;
            else if (ctype == CTRUNE) 
                return types.Idealrune;
            else if (ctype == CTFLT) 
                return types.Idealfloat;
            else if (ctype == CTCPLX) 
                return types.Idealcomplex;
            else if (ctype == CTSTR) 
                return types.Idealstring;
            else if (ctype == CTBOOL) 
                return types.Idealbool;
            else if (ctype == CTNIL) 
                return types.Types[TNIL];
                        Fatalf("exporter: unknown Ctype");
            return null;
        }

        private static slice<ref types.Type> predecl = default; // initialized lazily

        private static slice<ref types.Type> predeclared()
        {
            if (predecl == null)
            { 
                // initialize lazily to be sure that all
                // elements have been initialized before
                predecl = new slice<ref types.Type>(new ref types.Type[] { types.Types[TBOOL], types.Types[TINT], types.Types[TINT8], types.Types[TINT16], types.Types[TINT32], types.Types[TINT64], types.Types[TUINT], types.Types[TUINT8], types.Types[TUINT16], types.Types[TUINT32], types.Types[TUINT64], types.Types[TUINTPTR], types.Types[TFLOAT32], types.Types[TFLOAT64], types.Types[TCOMPLEX64], types.Types[TCOMPLEX128], types.Types[TSTRING], types.Bytetype, types.Runetype, types.Errortype, untype(CTBOOL), untype(CTINT), untype(CTRUNE), untype(CTFLT), untype(CTCPLX), untype(CTSTR), untype(CTNIL), types.Types[TUNSAFEPTR], types.Types[Txxx], types.Types[TANY] });
            }
            return predecl;
        }
    }
}}}}
