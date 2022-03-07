// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue22856 -- go2cs converted at 2022 March 06 22:41:35 UTC
// import "go/doc.issue22856" ==> using issue22856 = go.go.doc.issue22856_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\issue22856.go


namespace go.go;

public static partial class issue22856_package {

public partial struct T {
}

public static T New() {
    return new T();
}
public static ptr<T> NewPointer() {
    return addr(new T());
}
public static slice<ptr<T>> NewPointerSlice() {
    return new slice<ptr<T>>(new ptr<T>[] { &T{} });
}
public static slice<T> NewSlice() {
    return new slice<T>(new T[] { T{} });
}
public static ptr<ptr<T>> NewPointerOfPointer() {
    ptr<T> x = addr(new T());

    return _addr__addr_x!;
}
public static array<T> NewArray() {
    return new array<T>(new T[] { T{} });
}
public static array<ptr<T>> NewPointerArray() {
    return new array<ptr<T>>(new ptr<T>[] { &T{} });
}

// NewSliceOfSlice is not a factory function because slices of a slice of
// type *T are not factory functions of type T.
public static slice<slice<T>> NewSliceOfSlice() {
    return new slice<T>(new T[] { []T{} });
}

// NewPointerSliceOfSlice is not a factory function because slices of a
// slice of type *T are not factory functions of type T.
public static slice<slice<ptr<T>>> NewPointerSliceOfSlice() {
    return new slice<ptr<T>>(new ptr<T>[] { []*T{} });
}

// NewSlice3 is not a factory function because 3 nested slices of type T
// are not factory functions of type T.
public static slice<slice<slice<T>>> NewSlice3() {
    return new slice<T>(new T[] { []T{[]T{}} });
}

} // end issue22856_package
