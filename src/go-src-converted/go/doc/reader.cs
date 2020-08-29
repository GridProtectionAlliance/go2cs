// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package doc -- go2cs converted at 2020 August 29 08:47:09 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\reader.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        // ----------------------------------------------------------------------------
        // function/method sets
        //
        // Internally, we treat functions like methods and collect them in method sets.

        // A methodSet describes a set of methods. Entries where Decl == nil are conflict
        // entries (more than one method with the same name at the same embedding level).
        //
        private partial struct methodSet // : map<@string, ref Func>
        {
        }

        // recvString returns a string representation of recv of the
        // form "T", "*T", or "BADRECV" (if not a proper receiver type).
        //
        private static @string recvString(ast.Expr recv)
        {
            switch (recv.type())
            {
                case ref ast.Ident t:
                    return t.Name;
                    break;
                case ref ast.StarExpr t:
                    return "*" + recvString(t.X);
                    break;
            }
            return "BADRECV";
        }

        // set creates the corresponding Func for f and adds it to mset.
        // If there are multiple f's with the same name, set keeps the first
        // one with documentation; conflicts are ignored.
        //
        private static void set(this methodSet mset, ref ast.FuncDecl f)
        {
            var name = f.Name.Name;
            {
                var g = mset[name];

                if (g != null && g.Doc != "")
                { 
                    // A function with the same name has already been registered;
                    // since it has documentation, assume f is simply another
                    // implementation and ignore it. This does not happen if the
                    // caller is using go/build.ScanDir to determine the list of
                    // files implementing a package.
                    return;
                } 
                // function doesn't exist or has no documentation; use f

            } 
            // function doesn't exist or has no documentation; use f
            @string recv = "";
            if (f.Recv != null)
            {
                ast.Expr typ = default; 
                // be careful in case of incorrect ASTs
                {
                    var list = f.Recv.List;

                    if (len(list) == 1L)
                    {
                        typ = list[0L].Type;
                    }

                }
                recv = recvString(typ);
            }
            mset[name] = ref new Func(Doc:f.Doc.Text(),Name:name,Decl:f,Recv:recv,Orig:recv,);
            f.Doc = null; // doc consumed - remove from AST
        }

        // add adds method m to the method set; m is ignored if the method set
        // already contains a method with the same name at the same or a higher
        // level than m.
        //
        private static void add(this methodSet mset, ref Func m)
        {
            var old = mset[m.Name];
            if (old == null || m.Level < old.Level)
            {
                mset[m.Name] = m;
                return;
            }
            if (old != null && m.Level == old.Level)
            { 
                // conflict - mark it using a method with nil Decl
                mset[m.Name] = ref new Func(Name:m.Name,Level:m.Level,);
            }
        }

        // ----------------------------------------------------------------------------
        // Named types

        // baseTypeName returns the name of the base type of x (or "")
        // and whether the type is imported or not.
        //
        private static (@string, bool) baseTypeName(ast.Expr x)
        {
            switch (x.type())
            {
                case ref ast.Ident t:
                    return (t.Name, false);
                    break;
                case ref ast.SelectorExpr t:
                    {
                        ref ast.Ident (_, ok) = t.X._<ref ast.Ident>();

                        if (ok)
                        { 
                            // only possible for qualified type names;
                            // assume type is imported
                            return (t.Sel.Name, true);
                        }

                    }
                    break;
                case ref ast.StarExpr t:
                    return baseTypeName(t.X);
                    break;
            }
            return;
        }

        // An embeddedSet describes a set of embedded types.
        private partial struct embeddedSet // : map<ref namedType, bool>
        {
        }

        // A namedType represents a named unqualified (package local, or possibly
        // predeclared) type. The namedType for a type name is always found via
        // reader.lookupType.
        //
        private partial struct namedType
        {
            public @string doc; // doc comment for type
            public @string name; // type name
            public ptr<ast.GenDecl> decl; // nil if declaration hasn't been seen yet

            public bool isEmbedded; // true if this type is embedded
            public bool isStruct; // true if this type is a struct
            public embeddedSet embedded; // true if the embedded type is a pointer

// associated declarations
            public slice<ref Value> values; // consts and vars
            public methodSet funcs;
            public methodSet methods;
        }

        // ----------------------------------------------------------------------------
        // AST reader

        // reader accumulates documentation for a single package.
        // It modifies the AST: Comments (declaration documentation)
        // that have been collected by the reader are set to nil
        // in the respective AST nodes so that they are not printed
        // twice (once when printing the documentation and once when
        // printing the corresponding AST node).
        //
        private partial struct reader
        {
            public Mode mode; // package properties
            public @string doc; // package documentation, if any
            public slice<@string> filenames;
            public map<@string, slice<ref Note>> notes; // declarations
            public map<@string, long> imports;
            public bool hasDotImp; // if set, package contains a dot import
            public slice<ref Value> values; // consts and vars
            public long order; // sort order of const and var declarations (when we can't use a name)
            public map<@string, ref namedType> types;
            public methodSet funcs; // support for package-local error type declarations
            public bool errorDecl; // if set, type "error" was declared locally
            public slice<ref ast.InterfaceType> fixlist; // list of interfaces containing anonymous field "error"
        }

        private static bool isVisible(this ref reader r, @string name)
        {
            return r.mode & AllDecls != 0L || ast.IsExported(name);
        }

        // lookupType returns the base type with the given name.
        // If the base type has not been encountered yet, a new
        // type with the given name but no associated declaration
        // is added to the type map.
        //
        private static ref namedType lookupType(this ref reader r, @string name)
        {
            if (name == "" || name == "_")
            {
                return null; // no type docs for anonymous types
            }
            {
                var typ__prev1 = typ;

                var (typ, found) = r.types[name];

                if (found)
                {
                    return typ;
                } 
                // type not found - add one without declaration

                typ = typ__prev1;

            } 
            // type not found - add one without declaration
            namedType typ = ref new namedType(name:name,embedded:make(embeddedSet),funcs:make(methodSet),methods:make(methodSet),);
            r.types[name] = typ;
            return typ;
        }

        // recordAnonymousField registers fieldType as the type of an
        // anonymous field in the parent type. If the field is imported
        // (qualified name) or the parent is nil, the field is ignored.
        // The function returns the field name.
        //
        private static @string recordAnonymousField(this ref reader r, ref namedType parent, ast.Expr fieldType)
        {
            var (fname, imp) = baseTypeName(fieldType);
            if (parent == null || imp)
            {
                return;
            }
            {
                var ftype = r.lookupType(fname);

                if (ftype != null)
                {
                    ftype.isEmbedded = true;
                    ref ast.StarExpr (_, ptr) = fieldType._<ref ast.StarExpr>();
                    parent.embedded[ftype] = ptr;
                }

            }
            return;
        }

        private static void readDoc(this ref reader r, ref ast.CommentGroup comment)
        { 
            // By convention there should be only one package comment
            // but collect all of them if there are more than one.
            var text = comment.Text();
            if (r.doc == "")
            {
                r.doc = text;
                return;
            }
            r.doc += "\n" + text;
        }

        private static void remember(this ref reader r, ref ast.InterfaceType typ)
        {
            r.fixlist = append(r.fixlist, typ);
        }

        private static slice<@string> specNames(slice<ast.Spec> specs)
        {
            var names = make_slice<@string>(0L, len(specs)); // reasonable estimate
            foreach (var (_, s) in specs)
            { 
                // s guaranteed to be an *ast.ValueSpec by readValue
                foreach (var (_, ident) in s._<ref ast.ValueSpec>().Names)
                {
                    names = append(names, ident.Name);
                }
            }
            return names;
        }

        // readValue processes a const or var declaration.
        //
        private static void readValue(this ref reader r, ref ast.GenDecl decl)
        { 
            // determine if decl should be associated with a type
            // Heuristic: For each typed entry, determine the type name, if any.
            //            If there is exactly one type name that is sufficiently
            //            frequent, associate the decl with the respective type.
            @string domName = "";
            long domFreq = 0L;
            @string prev = "";
            long n = 0L;
            foreach (var (_, spec) in decl.Specs)
            {
                ref ast.ValueSpec (s, ok) = spec._<ref ast.ValueSpec>();
                if (!ok)
                {
                    continue; // should not happen, but be conservative
                }
                @string name = "";

                if (s.Type != null) 
                    // a type is present; determine its name
                    {
                        long n__prev1 = n;

                        var (n, imp) = baseTypeName(s.Type);

                        if (!imp)
                        {
                            name = n;
                        }

                        n = n__prev1;

                    }
                else if (decl.Tok == token.CONST && len(s.Values) == 0L) 
                    // no type or value is present but we have a constant declaration;
                    // use the previous type name (possibly the empty string)
                    name = prev;
                                if (name != "")
                { 
                    // entry has a named type
                    if (domName != "" && domName != name)
                    { 
                        // more than one type name - do not associate
                        // with any type
                        domName = "";
                        break;
                    }
                    domName = name;
                    domFreq++;
                }
                prev = name;
                n++;
            } 

            // nothing to do w/o a legal declaration
            if (n == 0L)
            {
                return;
            } 

            // determine values list with which to associate the Value for this decl
            var values = ref r.values;
            const float threshold = 0.75F;

            if (domName != "" && r.isVisible(domName) && domFreq >= int(float64(len(decl.Specs)) * threshold))
            { 
                // typed entries are sufficiently frequent
                {
                    var typ = r.lookupType(domName);

                    if (typ != null)
                    {
                        values = ref typ.values; // associate with that type
                    }

                }
            }
            values.Value = append(values.Value, ref new Value(Doc:decl.Doc.Text(),Names:specNames(decl.Specs),Decl:decl,order:r.order,));
            decl.Doc = null; // doc consumed - remove from AST

            // Note: It's important that the order used here is global because the cleanupTypes
            // methods may move values associated with types back into the global list. If the
            // order is list-specific, sorting is not deterministic because the same order value
            // may appear multiple times (was bug, found when fixing #16153).
            r.order++;
        }

        // fields returns a struct's fields or an interface's methods.
        //
        private static (slice<ref ast.Field>, bool) fields(ast.Expr typ)
        {
            ref ast.FieldList fields = default;
            switch (typ.type())
            {
                case ref ast.StructType t:
                    fields = t.Fields;
                    isStruct = true;
                    break;
                case ref ast.InterfaceType t:
                    fields = t.Methods;
                    break;
            }
            if (fields != null)
            {
                list = fields.List;
            }
            return;
        }

        // readType processes a type declaration.
        //
        private static void readType(this ref reader r, ref ast.GenDecl decl, ref ast.TypeSpec spec)
        {
            var typ = r.lookupType(spec.Name.Name);
            if (typ == null)
            {
                return; // no name or blank name - ignore the type
            } 

            // A type should be added at most once, so typ.decl
            // should be nil - if it is not, simply overwrite it.
            typ.decl = decl; 

            // compute documentation
            var doc = spec.Doc;
            spec.Doc = null; // doc consumed - remove from AST
            if (doc == null)
            { 
                // no doc associated with the spec, use the declaration doc, if any
                doc = decl.Doc;
            }
            decl.Doc = null; // doc consumed - remove from AST
            typ.doc = doc.Text(); 

            // record anonymous fields (they may contribute methods)
            // (some fields may have been recorded already when filtering
            // exports, but that's ok)
            slice<ref ast.Field> list = default;
            list, typ.isStruct = fields(spec.Type);
            foreach (var (_, field) in list)
            {
                if (len(field.Names) == 0L)
                {
                    r.recordAnonymousField(typ, field.Type);
                }
            }
        }

        // readFunc processes a func or method declaration.
        //
        private static void readFunc(this ref reader r, ref ast.FuncDecl fun)
        { 
            // strip function body
            fun.Body = null; 

            // associate methods with the receiver type, if any
            if (fun.Recv != null)
            { 
                // method
                if (len(fun.Recv.List) == 0L)
                { 
                    // should not happen (incorrect AST); (See issue 17788)
                    // don't show this method
                    return;
                }
                var (recvTypeName, imp) = baseTypeName(fun.Recv.List[0L].Type);
                if (imp)
                { 
                    // should not happen (incorrect AST);
                    // don't show this method
                    return;
                }
                {
                    var typ__prev2 = typ;

                    var typ = r.lookupType(recvTypeName);

                    if (typ != null)
                    {
                        typ.methods.set(fun);
                    } 
                    // otherwise ignore the method
                    // TODO(gri): There may be exported methods of non-exported types
                    // that can be called because of exported values (consts, vars, or
                    // function results) of that type. Could determine if that is the
                    // case and then show those methods in an appropriate section.

                    typ = typ__prev2;

                } 
                // otherwise ignore the method
                // TODO(gri): There may be exported methods of non-exported types
                // that can be called because of exported values (consts, vars, or
                // function results) of that type. Could determine if that is the
                // case and then show those methods in an appropriate section.
                return;
            } 

            // associate factory functions with the first visible result type, if any
            if (fun.Type.Results.NumFields() >= 1L)
            {
                var res = fun.Type.Results.List[0L];
                if (len(res.Names) <= 1L)
                { 
                    // exactly one (named or anonymous) result associated
                    // with the first type in result signature (there may
                    // be more than one result)
                    var factoryType = res.Type;
                    {
                        ref ast.ArrayType (t, ok) = factoryType._<ref ast.ArrayType>();

                        if (ok && t.Len == null)
                        { 
                            // We consider functions that return slices of type T (or
                            // pointers to T) as factory functions of T.
                            factoryType = t.Elt;
                        }

                    }
                    {
                        var (n, imp) = baseTypeName(factoryType);

                        if (!imp && r.isVisible(n))
                        {
                            {
                                var typ__prev4 = typ;

                                typ = r.lookupType(n);

                                if (typ != null)
                                { 
                                    // associate function with typ
                                    typ.funcs.set(fun);
                                    return;
                                }

                                typ = typ__prev4;

                            }
                        }

                    }
                }
            } 

            // just an ordinary function
            r.funcs.set(fun);
        }

        private static @string noteMarker = "([A-Z][A-Z]+)\\(([^)]+)\\):?";        private static var noteMarkerRx = regexp.MustCompile("^[ \\t]*" + noteMarker);        private static var noteCommentRx = regexp.MustCompile("^/[/*][ \\t]*" + noteMarker);

        // readNote collects a single note from a sequence of comments.
        //
        private static void readNote(this ref reader r, slice<ref ast.Comment> list)
        {
            ast.CommentGroup text = (ref new ast.CommentGroup(List:list)).Text();
            {
                var m = noteMarkerRx.FindStringSubmatchIndex(text);

                if (m != null)
                { 
                    // The note body starts after the marker.
                    // We remove any formatting so that we don't
                    // get spurious line breaks/indentation when
                    // showing the TODO body.
                    var body = clean(text[m[1L]..], keepNL);
                    if (body != "")
                    {
                        var marker = text[m[2L]..m[3L]];
                        r.notes[marker] = append(r.notes[marker], ref new Note(Pos:list[0].Pos(),End:list[len(list)-1].End(),UID:text[m[4]:m[5]],Body:body,));
                    }
                }

            }
        }

        // readNotes extracts notes from comments.
        // A note must start at the beginning of a comment with "MARKER(uid):"
        // and is followed by the note body (e.g., "// BUG(gri): fix this").
        // The note ends at the end of the comment group or at the start of
        // another note in the same comment group, whichever comes first.
        //
        private static void readNotes(this ref reader r, slice<ref ast.CommentGroup> comments)
        {
            foreach (var (_, group) in comments)
            {
                long i = -1L; // comment index of most recent note start, valid if >= 0
                var list = group.List;
                foreach (var (j, c) in list)
                {
                    if (noteCommentRx.MatchString(c.Text))
                    {
                        if (i >= 0L)
                        {
                            r.readNote(list[i..j]);
                        }
                        i = j;
                    }
                }
                if (i >= 0L)
                {
                    r.readNote(list[i..]);
                }
            }
        }

        // readFile adds the AST for a source file to the reader.
        //
        private static void readFile(this ref reader r, ref ast.File src)
        { 
            // add package documentation
            if (src.Doc != null)
            {
                r.readDoc(src.Doc);
                src.Doc = null; // doc consumed - remove from AST
            } 

            // add all declarations
            foreach (var (_, decl) in src.Decls)
            {
                switch (decl.type())
                {
                    case ref ast.GenDecl d:

                        if (d.Tok == token.IMPORT) 
                            // imports are handled individually
                            {
                                var spec__prev2 = spec;

                                foreach (var (_, __spec) in d.Specs)
                                {
                                    spec = __spec;
                                    {
                                        ref ast.ImportSpec s__prev1 = s;

                                        ref ast.ImportSpec (s, ok) = spec._<ref ast.ImportSpec>();

                                        if (ok)
                                        {
                                            {
                                                var (import_, err) = strconv.Unquote(s.Path.Value);

                                                if (err == null)
                                                {
                                                    r.imports[import_] = 1L;
                                                    if (s.Name != null && s.Name.Name == ".")
                                                    {
                                                        r.hasDotImp = true;
                                                    }
                                                }

                                            }
                                        }

                                        s = s__prev1;

                                    }
                                }

                                spec = spec__prev2;
                            }
                        else if (d.Tok == token.CONST || d.Tok == token.VAR) 
                            // constants and variables are always handled as a group
                            r.readValue(d);
                        else if (d.Tok == token.TYPE) 
                            // types are handled individually
                            if (len(d.Specs) == 1L && !d.Lparen.IsValid())
                            { 
                                // common case: single declaration w/o parentheses
                                // (if a single declaration is parenthesized,
                                // create a new fake declaration below, so that
                                // go/doc type declarations always appear w/o
                                // parentheses)
                                {
                                    ref ast.ImportSpec s__prev2 = s;

                                    (s, ok) = d.Specs[0L]._<ref ast.TypeSpec>();

                                    if (ok)
                                    {
                                        r.readType(d, s);
                                    }

                                    s = s__prev2;

                                }
                                break;
                            }
                            {
                                var spec__prev2 = spec;

                                foreach (var (_, __spec) in d.Specs)
                                {
                                    spec = __spec;
                                    {
                                        ref ast.ImportSpec s__prev1 = s;

                                        (s, ok) = spec._<ref ast.TypeSpec>();

                                        if (ok)
                                        { 
                                            // use an individual (possibly fake) declaration
                                            // for each type; this also ensures that each type
                                            // gets to (re-)use the declaration documentation
                                            // if there's none associated with the spec itself
                                            ast.GenDecl fake = ref new ast.GenDecl(Doc:d.Doc,TokPos:s.Pos(),Tok:token.TYPE,Specs:[]ast.Spec{s},);
                                            r.readType(fake, s);
                                        }

                                        s = s__prev1;

                                    }
                                }

                                spec = spec__prev2;
                            }
                                                break;
                    case ref ast.FuncDecl d:
                        r.readFunc(d);
                        break;
                }
            } 

            // collect MARKER(...): annotations
            r.readNotes(src.Comments);
            src.Comments = null; // consumed unassociated comments - remove from AST
        }

        private static void readPackage(this ref reader r, ref ast.Package pkg, Mode mode)
        { 
            // initialize reader
            r.filenames = make_slice<@string>(len(pkg.Files));
            r.imports = make_map<@string, long>();
            r.mode = mode;
            r.types = make_map<@string, ref namedType>();
            r.funcs = make(methodSet);
            r.notes = make_map<@string, slice<ref Note>>(); 

            // sort package files before reading them so that the
            // result does not depend on map iteration order
            long i = 0L;
            {
                var filename__prev1 = filename;

                foreach (var (__filename) in pkg.Files)
                {
                    filename = __filename;
                    r.filenames[i] = filename;
                    i++;
                }

                filename = filename__prev1;
            }

            sort.Strings(r.filenames); 

            // process files in sorted order
            {
                var filename__prev1 = filename;

                foreach (var (_, __filename) in r.filenames)
                {
                    filename = __filename;
                    var f = pkg.Files[filename];
                    if (mode & AllDecls == 0L)
                    {
                        r.fileExports(f);
                    }
                    r.readFile(f);
                }

                filename = filename__prev1;
            }

        }

        // ----------------------------------------------------------------------------
        // Types

        private static ref Func customizeRecv(ref Func f, @string recvTypeName, bool embeddedIsPtr, long level)
        {
            if (f == null || f.Decl == null || f.Decl.Recv == null || len(f.Decl.Recv.List) != 1L)
            {
                return f; // shouldn't happen, but be safe
            } 

            // copy existing receiver field and set new type
            var newField = f.Decl.Recv.List[0L].Value;
            var origPos = newField.Type.Pos();
            ref ast.StarExpr (_, origRecvIsPtr) = newField.Type._<ref ast.StarExpr>();
            ast.Ident newIdent = ref new ast.Ident(NamePos:origPos,Name:recvTypeName);
            ast.Expr typ = newIdent;
            if (!embeddedIsPtr && origRecvIsPtr)
            {
                newIdent.NamePos++; // '*' is one character
                typ = ref new ast.StarExpr(Star:origPos,X:newIdent);
            }
            newField.Type = typ; 

            // copy existing receiver field list and set new receiver field
            var newFieldList = f.Decl.Recv.Value;
            newFieldList.List = new slice<ref ast.Field>(new ref ast.Field[] { &newField }); 

            // copy existing function declaration and set new receiver field list
            var newFuncDecl = f.Decl.Value;
            newFuncDecl.Recv = ref newFieldList; 

            // copy existing function documentation and set new declaration
            var newF = f.Value;
            newF.Decl = ref newFuncDecl;
            newF.Recv = recvString(typ); 
            // the Orig field never changes
            newF.Level = level;

            return ref newF;
        }

        // collectEmbeddedMethods collects the embedded methods of typ in mset.
        //
        private static void collectEmbeddedMethods(this ref reader r, methodSet mset, ref namedType typ, @string recvTypeName, bool embeddedIsPtr, long level, embeddedSet visited)
        {
            visited[typ] = true;
            foreach (var (embedded, isPtr) in typ.embedded)
            { 
                // Once an embedded type is embedded as a pointer type
                // all embedded types in those types are treated like
                // pointer types for the purpose of the receiver type
                // computation; i.e., embeddedIsPtr is sticky for this
                // embedding hierarchy.
                var thisEmbeddedIsPtr = embeddedIsPtr || isPtr;
                foreach (var (_, m) in embedded.methods)
                { 
                    // only top-level methods are embedded
                    if (m.Level == 0L)
                    {
                        mset.add(customizeRecv(m, recvTypeName, thisEmbeddedIsPtr, level));
                    }
                }
                if (!visited[embedded])
                {
                    r.collectEmbeddedMethods(mset, embedded, recvTypeName, thisEmbeddedIsPtr, level + 1L, visited);
                }
            }
            delete(visited, typ);
        }

        // computeMethodSets determines the actual method sets for each type encountered.
        //
        private static void computeMethodSets(this ref reader r)
        {
            foreach (var (_, t) in r.types)
            { 
                // collect embedded methods for t
                if (t.isStruct)
                { 
                    // struct
                    r.collectEmbeddedMethods(t.methods, t, t.name, false, 1L, make(embeddedSet));
                }
                else
                { 
                    // interface
                    // TODO(gri) fix this
                }
            } 

            // if error was declared locally, don't treat it as exported field anymore
            if (r.errorDecl)
            {
                foreach (var (_, ityp) in r.fixlist)
                {
                    removeErrorField(ityp);
                }
            }
        }

        // cleanupTypes removes the association of functions and methods with
        // types that have no declaration. Instead, these functions and methods
        // are shown at the package level. It also removes types with missing
        // declarations or which are not visible.
        //
        private static void cleanupTypes(this ref reader r)
        {
            foreach (var (_, t) in r.types)
            {
                var visible = r.isVisible(t.name);
                var predeclared = predeclaredTypes[t.name];

                if (t.decl == null && (predeclared || visible && (t.isEmbedded || r.hasDotImp)))
                { 
                    // t.name is a predeclared type (and was not redeclared in this package),
                    // or it was embedded somewhere but its declaration is missing (because
                    // the AST is incomplete), or we have a dot-import (and all bets are off):
                    // move any associated values, funcs, and methods back to the top-level so
                    // that they are not lost.
                    // 1) move values
                    r.values = append(r.values, t.values); 
                    // 2) move factory functions
                    {
                        var name__prev2 = name;

                        foreach (var (__name, __f) in t.funcs)
                        {
                            name = __name;
                            f = __f; 
                            // in a correct AST, package-level function names
                            // are all different - no need to check for conflicts
                            r.funcs[name] = f;
                        } 
                        // 3) move methods

                        name = name__prev2;
                    }

                    if (!predeclared)
                    {
                        {
                            var name__prev2 = name;

                            foreach (var (__name, __m) in t.methods)
                            {
                                name = __name;
                                m = __m; 
                                // don't overwrite functions with the same name - drop them
                                {
                                    var (_, found) = r.funcs[name];

                                    if (!found)
                                    {
                                        r.funcs[name] = m;
                                    }

                                }
                            }

                            name = name__prev2;
                        }

                    }
                } 
                // remove types w/o declaration or which are not visible
                if (t.decl == null || !visible)
                {
                    delete(r.types, t.name);
                }
            }
        }

        // ----------------------------------------------------------------------------
        // Sorting

        private partial struct data
        {
            public long n;
            public Action<long, long> swap;
            public Func<long, long, bool> less;
        }

        private static long Len(this ref data d)
        {
            return d.n;
        }
        private static void Swap(this ref data d, long i, long j)
        {
            d.swap(i, j);

        }
        private static bool Less(this ref data d, long i, long j)
        {
            return d.less(i, j);
        }

        // sortBy is a helper function for sorting
        private static void sortBy(Func<long, long, bool> less, Action<long, long> swap, long n)
        {
            sort.Sort(ref new data(n,swap,less));
        }

        private static slice<@string> sortedKeys(map<@string, long> m)
        {
            var list = make_slice<@string>(len(m));
            long i = 0L;
            foreach (var (key) in m)
            {
                list[i] = key;
                i++;
            }
            sort.Strings(list);
            return list;
        }

        // sortingName returns the name to use when sorting d into place.
        //
        private static @string sortingName(ref ast.GenDecl d)
        {
            if (len(d.Specs) == 1L)
            {
                {
                    ref ast.ValueSpec (s, ok) = d.Specs[0L]._<ref ast.ValueSpec>();

                    if (ok)
                    {
                        return s.Names[0L].Name;
                    }

                }
            }
            return "";
        }

        private static slice<ref Value> sortedValues(slice<ref Value> m, token.Token tok)
        {
            var list = make_slice<ref Value>(len(m)); // big enough in any case
            long i = 0L;
            foreach (var (_, val) in m)
            {
                if (val.Decl.Tok == tok)
                {
                    list[i] = val;
                    i++;
                }
            }
            list = list[0L..i];

            sortBy((i, j) =>
            {
                {
                    var ni = sortingName(list[i].Decl);
                    var nj = sortingName(list[j].Decl);

                    if (ni != nj)
                    {
                        return ni < nj;
                    }

                }
                return list[i].order < list[j].order;
            }, (i, j) =>
            {
                list[i] = list[j];
                list[j] = list[i];

            }, len(list));

            return list;
        }

        private static slice<ref Type> sortedTypes(map<@string, ref namedType> m, bool allMethods)
        {
            var list = make_slice<ref Type>(len(m));
            long i = 0L;
            foreach (var (_, t) in m)
            {
                list[i] = ref new Type(Doc:t.doc,Name:t.name,Decl:t.decl,Consts:sortedValues(t.values,token.CONST),Vars:sortedValues(t.values,token.VAR),Funcs:sortedFuncs(t.funcs,true),Methods:sortedFuncs(t.methods,allMethods),);
                i++;
            }
            sortBy((i, j) => list[i].Name < list[j].Name, (i, j) =>
            {
                list[i] = list[j];
                list[j] = list[i];

            }, len(list));

            return list;
        }

        private static @string removeStar(@string s)
        {
            if (len(s) > 0L && s[0L] == '*')
            {
                return s[1L..];
            }
            return s;
        }

        private static slice<ref Func> sortedFuncs(methodSet m, bool allMethods)
        {
            var list = make_slice<ref Func>(len(m));
            long i = 0L;
            foreach (var (_, m) in m)
            { 
                // determine which methods to include

                if (m.Decl == null)                 else if (allMethods || m.Level == 0L || !ast.IsExported(removeStar(m.Orig))) 
                    // forced inclusion, method not embedded, or method
                    // embedded but original receiver type not exported
                    list[i] = m;
                    i++;
                            }
            list = list[0L..i];
            sortBy((i, j) => list[i].Name < list[j].Name, (i, j) =>
            {
                list[i] = list[j];
                list[j] = list[i];

            }, len(list));
            return list;
        }

        // noteBodies returns a list of note body strings given a list of notes.
        // This is only used to populate the deprecated Package.Bugs field.
        //
        private static slice<@string> noteBodies(slice<ref Note> notes)
        {
            slice<@string> list = default;
            foreach (var (_, n) in notes)
            {
                list = append(list, n.Body);
            }
            return list;
        }

        // ----------------------------------------------------------------------------
        // Predeclared identifiers

        // IsPredeclared reports whether s is a predeclared identifier.
        public static bool IsPredeclared(@string s)
        {
            return predeclaredTypes[s] || predeclaredFuncs[s] || predeclaredConstants[s];
        }

        private static map predeclaredTypes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"bool":true,"byte":true,"complex64":true,"complex128":true,"error":true,"float32":true,"float64":true,"int":true,"int8":true,"int16":true,"int32":true,"int64":true,"rune":true,"string":true,"uint":true,"uint8":true,"uint16":true,"uint32":true,"uint64":true,"uintptr":true,};

        private static map predeclaredFuncs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"append":true,"cap":true,"close":true,"complex":true,"copy":true,"delete":true,"imag":true,"len":true,"make":true,"new":true,"panic":true,"print":true,"println":true,"real":true,"recover":true,};

        private static map predeclaredConstants = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"false":true,"iota":true,"nil":true,"true":true,};
    }
}}
