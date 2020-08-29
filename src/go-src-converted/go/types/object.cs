// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:47:45 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\object.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // An Object describes a named language entity such as a package,
        // constant, type, variable, function (incl. methods), or label.
        // All objects implement the Object interface.
        //
        public partial interface Object
        {
            token.Pos Parent(); // scope in which this object is declared; nil for methods and struct fields
            token.Pos Pos(); // position of object identifier in declaration
            token.Pos Pkg(); // package to which this object belongs; nil for labels and objects in the Universe scope
            token.Pos Name(); // package local object name
            token.Pos Type(); // object type
            token.Pos Exported(); // reports whether the name starts with a capital letter
            token.Pos Id(); // object name if exported, qualified name if not exported (see func Id)

// String returns a human-readable string of the object.
            token.Pos String(); // order reflects a package-level object's source order: if object
// a is before object b in the source, then a.order() < b.order().
// order returns a value > 0 for package-level objects; it returns
// 0 for all other objects (including objects in file scopes).
            token.Pos order(); // setOrder sets the order number of the object. It must be > 0.
            token.Pos setOrder(uint _p0); // setParent sets the parent scope of the object.
            token.Pos setParent(ref Scope _p0); // sameId reports whether obj.Id() and Id(pkg, name) are the same.
            token.Pos sameId(ref Package pkg, @string name); // scopePos returns the start position of the scope of this Object
            token.Pos scopePos(); // setScopePos sets the start position of the scope for this Object.
            token.Pos setScopePos(token.Pos pos);
        }

        // Id returns name if it is exported, otherwise it
        // returns the name qualified with the package path.
        public static @string Id(ref Package pkg, @string name)
        {
            if (ast.IsExported(name))
            {
                return name;
            } 
            // unexported names need the package path for differentiation
            // (if there's no package, make sure we don't start with '.'
            // as that may change the order of methods between a setup
            // inside a package and outside a package - which breaks some
            // tests)
            @string path = "_"; 
            // pkg is nil for objects in Universe scope and possibly types
            // introduced via Eval (see also comment in object.sameId)
            if (pkg != null && pkg.path != "")
            {
                path = pkg.path;
            }
            return path + "." + name;
        }

        // An object implements the common parts of an Object.
        private partial struct @object
        {
            public ptr<Scope> parent;
            public token.Pos pos;
            public ptr<Package> pkg;
            public @string name;
            public Type typ;
            public uint order_;
            public token.Pos scopePos_;
        }

        private static ref Scope Parent(this ref object obj)
        {
            return obj.parent;
        }
        private static token.Pos Pos(this ref object obj)
        {
            return obj.pos;
        }
        private static ref Package Pkg(this ref object obj)
        {
            return obj.pkg;
        }
        private static @string Name(this ref object obj)
        {
            return obj.name;
        }
        private static Type Type(this ref object obj)
        {
            return obj.typ;
        }
        private static bool Exported(this ref object obj)
        {
            return ast.IsExported(obj.name);
        }
        private static @string Id(this ref object obj)
        {
            return Id(obj.pkg, obj.name);
        }
        private static @string String(this ref object _obj) => func(_obj, (ref object obj, Defer _, Panic panic, Recover __) =>
        {
            panic("abstract");

        });
        private static uint order(this ref object obj)
        {
            return obj.order_;
        }
        private static token.Pos scopePos(this ref object obj)
        {
            return obj.scopePos_;
        }

        private static void setParent(this ref object obj, ref Scope parent)
        {
            obj.parent = parent;

        }
        private static void setOrder(this ref object obj, uint order)
        {
            assert(order > 0L);

            obj.order_ = order;

        }
        private static void setScopePos(this ref object obj, token.Pos pos)
        {
            obj.scopePos_ = pos;

        }

        private static bool sameId(this ref object obj, ref Package pkg, @string name)
        { 
            // spec:
            // "Two identifiers are different if they are spelled differently,
            // or if they appear in different packages and are not exported.
            // Otherwise, they are the same."
            if (name != obj.name)
            {
                return false;
            } 
            // obj.Name == name
            if (obj.Exported())
            {
                return true;
            } 
            // not exported, so packages must be the same (pkg == nil for
            // fields in Universe scope; this can only happen for types
            // introduced via Eval)
            if (pkg == null || obj.pkg == null)
            {
                return pkg == obj.pkg;
            } 
            // pkg != nil && obj.pkg != nil
            return pkg.path == obj.pkg.path;
        }

        // A PkgName represents an imported Go package.
        // PkgNames don't have a type.
        public partial struct PkgName
        {
            public ref object @object => ref @object_val;
            public ptr<Package> imported;
            public bool used; // set if the package was used
        }

        // NewPkgName returns a new PkgName object representing an imported package.
        // The remaining arguments set the attributes found with all Objects.
        public static ref PkgName NewPkgName(token.Pos pos, ref Package pkg, @string name, ref Package imported)
        {
            return ref new PkgName(object{nil,pos,pkg,name,Typ[Invalid],0,token.NoPos},imported,false);
        }

        // Imported returns the package that was imported.
        // It is distinct from Pkg(), which is the package containing the import statement.
        private static ref Package Imported(this ref PkgName obj)
        {
            return obj.imported;
        }

        // A Const represents a declared constant.
        public partial struct Const
        {
            public ref object @object => ref @object_val;
            public constant.Value val;
            public bool visited; // for initialization cycle detection
        }

        // NewConst returns a new constant with value val.
        // The remaining arguments set the attributes found with all Objects.
        public static ref Const NewConst(token.Pos pos, ref Package pkg, @string name, Type typ, constant.Value val)
        {
            return ref new Const(object{nil,pos,pkg,name,typ,0,token.NoPos},val,false);
        }

        private static constant.Value Val(this ref Const obj)
        {
            return obj.val;
        }
        private static void isDependency(this ref Const _p0)
        {
        } // a constant may be a dependency of an initialization expression

        // A TypeName represents a name for a (named or alias) type.
        public partial struct TypeName
        {
            public ref object @object => ref @object_val;
        }

        // NewTypeName returns a new type name denoting the given typ.
        // The remaining arguments set the attributes found with all Objects.
        //
        // The typ argument may be a defined (Named) type or an alias type.
        // It may also be nil such that the returned TypeName can be used as
        // argument for NewNamed, which will set the TypeName's type as a side-
        // effect.
        public static ref TypeName NewTypeName(token.Pos pos, ref Package pkg, @string name, Type typ)
        {
            return ref new TypeName(object{nil,pos,pkg,name,typ,0,token.NoPos});
        }

        // IsAlias reports whether obj is an alias name for a type.
        private static bool IsAlias(this ref TypeName obj)
        {
            switch (obj.typ.type())
            {
                case 
                    return false;
                    break;
                case ref Basic t:
                    if (obj.pkg == Unsafe)
                    {
                        return false;
                    } 
                    // Any user-defined type name for a basic type is an alias for a
                    // basic type (because basic types are pre-declared in the Universe
                    // scope, outside any package scope), and so is any type name with
                    // a different name than the name of the basic type it refers to.
                    // Additionally, we need to look for "byte" and "rune" because they
                    // are aliases but have the same names (for better error messages).
                    return obj.pkg != null || t.name != obj.name || t == universeByte || t == universeRune;
                    break;
                case ref Named t:
                    return obj != t.obj;
                    break;
                default:
                {
                    var t = obj.typ.type();
                    return true;
                    break;
                }
            }
        }

        // A Variable represents a declared variable (including function parameters and results, and struct fields).
        public partial struct Var
        {
            public ref object @object => ref @object_val;
            public bool anonymous; // if set, the variable is an anonymous struct field, and name is the type name
            public bool visited; // for initialization cycle detection
            public bool isField; // var is struct field
            public bool used; // set if the variable was used
        }

        // NewVar returns a new variable.
        // The arguments set the attributes found with all Objects.
        public static ref Var NewVar(token.Pos pos, ref Package pkg, @string name, Type typ)
        {
            return ref new Var(object:object{nil,pos,pkg,name,typ,0,token.NoPos});
        }

        // NewParam returns a new variable representing a function parameter.
        public static ref Var NewParam(token.Pos pos, ref Package pkg, @string name, Type typ)
        {
            return ref new Var(object:object{nil,pos,pkg,name,typ,0,token.NoPos},used:true); // parameters are always 'used'
        }

        // NewField returns a new variable representing a struct field.
        // For anonymous (embedded) fields, the name is the unqualified
        // type name under which the field is accessible.
        public static ref Var NewField(token.Pos pos, ref Package pkg, @string name, Type typ, bool anonymous)
        {
            return ref new Var(object:object{nil,pos,pkg,name,typ,0,token.NoPos},anonymous:anonymous,isField:true);
        }

        // Anonymous reports whether the variable is an anonymous field.
        private static bool Anonymous(this ref Var obj)
        {
            return obj.anonymous;
        }

        // IsField reports whether the variable is a struct field.
        private static bool IsField(this ref Var obj)
        {
            return obj.isField;
        }

        private static void isDependency(this ref Var _p0)
        {
        } // a variable may be a dependency of an initialization expression

        // A Func represents a declared function, concrete method, or abstract
        // (interface) method. Its Type() is always a *Signature.
        // An abstract method may belong to many interfaces due to embedding.
        public partial struct Func
        {
            public ref object @object => ref @object_val;
        }

        // NewFunc returns a new function with the given signature, representing
        // the function's type.
        public static ref Func NewFunc(token.Pos pos, ref Package pkg, @string name, ref Signature sig)
        { 
            // don't store a nil signature
            Type typ = default;
            if (sig != null)
            {
                typ = sig;
            }
            return ref new Func(object{nil,pos,pkg,name,typ,0,token.NoPos});
        }

        // FullName returns the package- or receiver-type-qualified name of
        // function or method obj.
        private static @string FullName(this ref Func obj)
        {
            bytes.Buffer buf = default;
            writeFuncName(ref buf, obj, null);
            return buf.String();
        }

        // Scope returns the scope of the function's body block.
        private static ref Scope Scope(this ref Func obj)
        {
            return obj.typ._<ref Signature>().scope;
        }

        private static void isDependency(this ref Func _p0)
        {
        } // a function may be a dependency of an initialization expression

        // A Label represents a declared label.
        // Labels don't have a type.
        public partial struct Label
        {
            public ref object @object => ref @object_val;
            public bool used; // set if the label was used
        }

        // NewLabel returns a new label.
        public static ref Label NewLabel(token.Pos pos, ref Package pkg, @string name)
        {
            return ref new Label(object{pos:pos,pkg:pkg,name:name,typ:Typ[Invalid]},false);
        }

        // A Builtin represents a built-in function.
        // Builtins don't have a valid type.
        public partial struct Builtin
        {
            public ref object @object => ref @object_val;
            public builtinId id;
        }

        private static ref Builtin newBuiltin(builtinId id)
        {
            return ref new Builtin(object{name:predeclaredFuncs[id].name,typ:Typ[Invalid]},id);
        }

        // Nil represents the predeclared value nil.
        public partial struct Nil
        {
            public ref object @object => ref @object_val;
        }

        private static void writeObject(ref bytes.Buffer _buf, Object obj, Qualifier qf) => func(_buf, (ref bytes.Buffer buf, Defer _, Panic panic, Recover __) =>
        {
            ref TypeName tname = default;
            var typ = obj.Type();

            switch (obj.type())
            {
                case ref PkgName obj:
                    fmt.Fprintf(buf, "package %s", obj.Name());
                    {
                        var path = obj.imported.path;

                        if (path != "" && path != obj.name)
                        {
                            fmt.Fprintf(buf, " (%q)", path);
                        }

                    }
                    return;
                    break;
                case ref Const obj:
                    buf.WriteString("const");
                    break;
                case ref TypeName obj:
                    tname = obj;
                    buf.WriteString("type");
                    break;
                case ref Var obj:
                    if (obj.isField)
                    {
                        buf.WriteString("field");
                    }
                    else
                    {
                        buf.WriteString("var");
                    }
                    break;
                case ref Func obj:
                    buf.WriteString("func ");
                    writeFuncName(buf, obj, qf);
                    if (typ != null)
                    {
                        WriteSignature(buf, typ._<ref Signature>(), qf);
                    }
                    return;
                    break;
                case ref Label obj:
                    buf.WriteString("label");
                    typ = null;
                    break;
                case ref Builtin obj:
                    buf.WriteString("builtin");
                    typ = null;
                    break;
                case ref Nil obj:
                    buf.WriteString("nil");
                    return;
                    break;
                default:
                {
                    var obj = obj.type();
                    panic(fmt.Sprintf("writeObject(%T)", obj));
                    break;
                }

            }

            buf.WriteByte(' '); 

            // For package-level objects, qualify the name.
            if (obj.Pkg() != null && obj.Pkg().scope.Lookup(obj.Name()) == obj)
            {
                writePackage(buf, obj.Pkg(), qf);
            }
            buf.WriteString(obj.Name());

            if (typ == null)
            {
                return;
            }
            if (tname != null)
            { 
                // We have a type object: Don't print anything more for
                // basic types since there's no more information (names
                // are the same; see also comment in TypeName.IsAlias).
                {
                    ref Basic (_, ok) = typ._<ref Basic>();

                    if (ok)
                    {
                        return;
                    }

                }
                if (tname.IsAlias())
                {
                    buf.WriteString(" =");
                }
                else
                {
                    typ = typ.Underlying();
                }
            }
            buf.WriteByte(' ');
            WriteType(buf, typ, qf);
        });

        private static void writePackage(ref bytes.Buffer buf, ref Package pkg, Qualifier qf)
        {
            if (pkg == null)
            {
                return;
            }
            @string s = default;
            if (qf != null)
            {
                s = qf(pkg);
            }
            else
            {
                s = pkg.Path();
            }
            if (s != "")
            {
                buf.WriteString(s);
                buf.WriteByte('.');
            }
        }

        // ObjectString returns the string form of obj.
        // The Qualifier controls the printing of
        // package-level objects, and may be nil.
        public static @string ObjectString(Object obj, Qualifier qf)
        {
            bytes.Buffer buf = default;
            writeObject(ref buf, obj, qf);
            return buf.String();
        }

        private static @string String(this ref PkgName obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Const obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref TypeName obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Var obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Func obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Label obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Builtin obj)
        {
            return ObjectString(obj, null);
        }
        private static @string String(this ref Nil obj)
        {
            return ObjectString(obj, null);
        }

        private static void writeFuncName(ref bytes.Buffer buf, ref Func f, Qualifier qf)
        {
            if (f.typ != null)
            {
                ref Signature sig = f.typ._<ref Signature>();
                {
                    var recv = sig.Recv();

                    if (recv != null)
                    {
                        buf.WriteByte('(');
                        {
                            ref Interface (_, ok) = recv.Type()._<ref Interface>();

                            if (ok)
                            { 
                                // gcimporter creates abstract methods of
                                // named interfaces using the interface type
                                // (not the named type) as the receiver.
                                // Don't print it in full.
                                buf.WriteString("interface");
                            }
                            else
                            {
                                WriteType(buf, recv.Type(), qf);
                            }

                        }
                        buf.WriteByte(')');
                        buf.WriteByte('.');
                    }
                    else if (f.pkg != null)
                    {
                        writePackage(buf, f.pkg, qf);
                    }

                }
            }
            buf.WriteString(f.name);
        }
    }
}}
