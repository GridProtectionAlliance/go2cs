// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflect -- go2cs converted at 2022 March 13 05:41:53 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Program Files\Go\src\reflect\visiblefields.go
namespace go;

public static partial class reflect_package {

// VisibleFields returns all the visible fields in t, which must be a
// struct type. A field is defined as visible if it's accessible
// directly with a FieldByName call. The returned fields include fields
// inside anonymous struct members and unexported fields. They follow
// the same order found in the struct, with anonymous fields followed
// immediately by their promoted fields.
//
// For each element e of the returned slice, the corresponding field
// can be retrieved from a value v of type t by calling v.FieldByIndex(e.Index).
public static slice<StructField> VisibleFields(Type t) => func((_, panic, _) => {
    if (t == null) {
        panic("reflect: VisibleFields(nil)");
    }
    if (t.Kind() != Struct) {
        panic("reflect.VisibleFields of non-struct type");
    }
    ptr<visibleFieldsWalker> w = addr(new visibleFieldsWalker(byName:make(map[string]int),visiting:make(map[Type]bool),fields:make([]StructField,0,t.NumField()),index:make([]int,0,2),));
    w.walk(t); 
    // Remove all the fields that have been hidden.
    // Use an in-place removal that avoids copying in
    // the common case that there are no hidden fields.
    nint j = 0;
    foreach (var (i) in w.fields) {
        var f = _addr_w.fields[i];
        if (f.Name == "") {
            continue;
        }
        if (i != j) { 
            // A field has been removed. We need to shuffle
            // all the subsequent elements up.
            w.fields[j] = f.val;
        }
        j++;
    }    return w.fields[..(int)j];
});

private partial struct visibleFieldsWalker {
    public map<@string, nint> byName;
    public map<Type, bool> visiting;
    public slice<StructField> fields;
    public slice<nint> index;
}

// walk walks all the fields in the struct type t, visiting
// fields in index preorder and appending them to w.fields
// (this maintains the required ordering).
// Fields that have been overridden have their
// Name field cleared.
private static void walk(this ptr<visibleFieldsWalker> _addr_w, Type t) {
    ref visibleFieldsWalker w = ref _addr_w.val;

    if (w.visiting[t]) {
        return ;
    }
    w.visiting[t] = true;
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        w.index = append(w.index, i);
        var add = true;
        {
            var (oldIndex, ok) = w.byName[f.Name];

            if (ok) {
                var old = _addr_w.fields[oldIndex];
                if (len(w.index) == len(old.Index)) { 
                    // Fields with the same name at the same depth
                    // cancel one another out. Set the field name
                    // to empty to signify that has happened, and
                    // there's no need to add this field.
                    old.Name = "";
                    add = false;
                }
                else if (len(w.index) < len(old.Index)) { 
                    // The old field loses because it's deeper than the new one.
                    old.Name = "";
                }
                else
 { 
                    // The old field wins because it's shallower than the new one.
                    add = false;
                }
            }

        }
        if (add) { 
            // Copy the index so that it's not overwritten
            // by the other appends.
            f.Index = append((slice<nint>)null, w.index);
            w.byName[f.Name] = len(w.fields);
            w.fields = append(w.fields, f);
        }
        if (f.Anonymous) {
            if (f.Type.Kind() == Ptr) {
                f.Type = f.Type.Elem();
            }
            if (f.Type.Kind() == Struct) {
                w.walk(f.Type);
            }
        }
        w.index = w.index[..(int)len(w.index) - 1];
    }
    delete(w.visiting, t);
}

} // end reflect_package
