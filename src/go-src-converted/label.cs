// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package label -- go2cs converted at 2022 March 06 23:31:36 UTC
// import "golang.org/x/tools/internal/event/label" ==> using label = go.golang.org.x.tools.@internal.@event.label_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\label\label.go
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using @unsafe = go.@unsafe_package;

namespace go.golang.org.x.tools.@internal.@event;

public static partial class label_package {

    // Key is used as the identity of a Label.
    // Keys are intended to be compared by pointer only, the name should be unique
    // for communicating with external systems, but it is not required or enforced.
public partial interface Key {
    @string Name(); // Description returns a string that can be used to describe the value.
    @string Description(); // Format is used in formatting to append the value of the label to the
// supplied buffer.
// The formatter may use the supplied buf as a scratch area to avoid
// allocations.
    @string Format(io.Writer w, slice<byte> buf, Label l);
}

// Label holds a key and value pair.
// It is normally used when passing around lists of labels.
public partial struct Label {
    public Key key;
    public ulong packed;
}

// Map is the interface to a collection of Labels indexed by key.
public partial interface Map {
    Label Find(Key key);
}

// List is the interface to something that provides an iterable
// list of labels.
// Iteration should start from 0 and continue until Valid returns false.
public partial interface List {
    Label Valid(nint index); // Label returns the label at the given index.
    Label Label(nint index);
}

// list implements LabelList for a list of Labels.
private partial struct list {
    public slice<Label> labels;
}

// filter wraps a LabelList filtering out specific labels.
private partial struct filter {
    public slice<Key> keys;
    public List underlying;
}

// listMap implements LabelMap for a simple list of labels.
private partial struct listMap {
    public slice<Label> labels;
}

// mapChain implements LabelMap for a list of underlying LabelMap.
private partial struct mapChain {
    public slice<Map> maps;
}

// OfValue creates a new label from the key and value.
// This method is for implementing new key types, label creation should
// normally be done with the Of method of the key.
public static Label OfValue(Key k, object value) {
    return new Label(key:k,untyped:value);
}

// UnpackValue assumes the label was built using LabelOfValue and returns the value
// that was passed to that constructor.
// This method is for implementing new key types, for type safety normal
// access should be done with the From method of the key.
public static void UnpackValue(this Label t) {
    return t.untyped;
}

// Of64 creates a new label from a key and a uint64. This is often
// used for non uint64 values that can be packed into a uint64.
// This method is for implementing new key types, label creation should
// normally be done with the Of method of the key.
public static Label Of64(Key k, ulong v) {
    return new Label(key:k,packed:v);
}

// Unpack64 assumes the label was built using LabelOf64 and returns the value that
// was passed to that constructor.
// This method is for implementing new key types, for type safety normal
// access should be done with the From method of the key.
public static ulong Unpack64(this Label t) {
    return t.packed;
}

// OfString creates a new label from a key and a string.
// This method is for implementing new key types, label creation should
// normally be done with the Of method of the key.
public static Label OfString(Key k, @string v) {
    var hdr = (reflect.StringHeader.val)(@unsafe.Pointer(_addr_v));
    return new Label(key:k,packed:uint64(hdr.Len),untyped:unsafe.Pointer(hdr.Data),);
}

// UnpackString assumes the label was built using LabelOfString and returns the
// value that was passed to that constructor.
// This method is for implementing new key types, for type safety normal
// access should be done with the From method of the key.
public static @string UnpackString(this Label t) {
    ref @string v = ref heap(out ptr<@string> _addr_v);
    var hdr = (reflect.StringHeader.val)(@unsafe.Pointer(_addr_v));
    hdr.Data = uintptr(t.untyped._<unsafe.Pointer>());
    hdr.Len = int(t.packed);
    return new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(hdr));
}

// Valid returns true if the Label is a valid one (it has a key).
public static bool Valid(this Label t) {
    return t.key != null;
}

// Key returns the key of this Label.
public static Key Key(this Label t) {
    return t.key;
}

// Format is used for debug printing of labels.
public static void Format(this Label t, fmt.State f, int r) {
    if (!t.Valid()) {
        io.WriteString(f, "nil");
        return ;
    }
    io.WriteString(f, t.Key().Name());
    io.WriteString(f, "=");
    array<byte> buf = new array<byte>(128);
    t.Key().Format(f, buf[..(int)0], t);

}

private static bool Valid(this ptr<list> _addr_l, nint index) {
    ref list l = ref _addr_l.val;

    return index >= 0 && index < len(l.labels);
}

private static Label Label(this ptr<list> _addr_l, nint index) {
    ref list l = ref _addr_l.val;

    return l.labels[index];
}

private static bool Valid(this ptr<filter> _addr_f, nint index) {
    ref filter f = ref _addr_f.val;

    return f.underlying.Valid(index);
}

private static Label Label(this ptr<filter> _addr_f, nint index) {
    ref filter f = ref _addr_f.val;

    var l = f.underlying.Label(index);
    foreach (var (_, f) in f.keys) {
        if (l.Key() == f) {
            return new Label();
        }
    }    return l;

}

private static Label Find(this listMap lm, Key key) {
    foreach (var (_, l) in lm.labels) {
        if (l.Key() == key) {
            return l;
        }
    }    return new Label();

}

private static Label Find(this mapChain c, Key key) {
    foreach (var (_, src) in c.maps) {
        var l = src.Find(key);
        if (l.Valid()) {
            return l;
        }
    }    return new Label();

}

private static ptr<list> emptyList = addr(new list());

public static List NewList(params Label[] labels) {
    labels = labels.Clone();

    if (len(labels) == 0) {
        return emptyList;
    }
    return addr(new list(labels:labels));

}

public static List Filter(List l, params Key[] keys) {
    keys = keys.Clone();

    if (len(keys) == 0) {
        return l;
    }
    return addr(new filter(keys:keys,underlying:l));

}

public static Map NewMap(params Label[] labels) {
    labels = labels.Clone();

    return new listMap(labels:labels);
}

public static Map MergeMaps(params Map[] srcs) {
    srcs = srcs.Clone();

    slice<Map> nonNil = default;
    foreach (var (_, src) in srcs) {
        if (src != null) {
            nonNil = append(nonNil, src);
        }
    }    if (len(nonNil) == 1) {
        return nonNil[0];
    }
    return new mapChain(maps:nonNil);

}

} // end label_package
