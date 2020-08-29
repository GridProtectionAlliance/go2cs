// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue18063 -- go2cs converted at 2020 August 29 08:47:13 UTC
// import "go/doc.issue18063" ==> using issue18063 = go.go.doc.issue18063_package
// Original source: C:\Go\src\go\doc\testdata\issue18063.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class issue18063_package
    {
        public partial struct T
        {
        }

        public static T New()
        {
            return new T();
        }
        public static ref T NewPointer()
        {
            return ref new T();
        }
        public static slice<ref T> NewPointerSlice()
        {
            return new slice<ref T>(new ref T[] { &T{} });
        }
        public static slice<T> NewSlice()
        {
            return new slice<T>(new T[] { T{} });
        }
        public static ptr<ptr<T>> NewPointerOfPointer()
        {
            T x = ref new T();

            return ref x;
        }

        // NewArray is not a factory function because arrays of type T are not
        // factory functions of type T.
        public static array<T> NewArray()
        {
            return new array<T>(new T[] { T{} });
        }

        // NewPointerArray is not a factory function because arrays of type *T are not
        // factory functions of type T.
        public static array<ref T> NewPointerArray()
        {
            return new array<ref T>(new ref T[] { &T{} });
        }

        // NewSliceOfSlice is not a factory function because slices of a slice of
        // type *T are not factory functions of type T.
        public static slice<slice<T>> NewSliceOfSlice()
        {
            return new slice<T>(new T[] { []T{} });
        }

        // NewPointerSliceOfSlice is not a factory function because slices of a
        // slice of type *T are not factory functions of type T.
        public static slice<slice<ref T>> NewPointerSliceOfSlice()
        {
            return new slice<ref T>(new ref T[] { []*T{} });
        }

        // NewSlice3 is not a factory function because 3 nested slices of type T
        // are not factory functions of type T.
        public static slice<slice<slice<T>>> NewSlice3()
        {
            return new slice<T>(new T[] { []T{[]T{}} });
        }
    }
}}
