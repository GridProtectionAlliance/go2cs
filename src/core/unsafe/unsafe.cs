// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package unsafe contains operations that step around the type safety of Go programs.

Packages that import unsafe may be non-portable and are not protected by the
Go 1 compatibility guidelines.
*/

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using go.runtime;
using go;

[module: GoManualConversion]

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable IL2070
#pragma warning disable IL2072

namespace go;

/// <summary>
/// The unsafe package contains operations that step around the type safety of Go programs.
/// Note that the operations in this package are not type safe and can lead to undefined behavior.
/// In the case of C# operations, the return values will be in context of the C# type system,
/// not Go. Any Go code that has been converted to C# and is dependent on memory layout of Go
/// types will certainly not work as expected and could cause unexpected behavior.
/// </summary>
unsafe partial class unsafe_package  {

// ArbitraryType is here for the purposes of documentation only and is not actually
// part of the unsafe package. It represents the type of an arbitrary Go expression.
[GoType("num:nint")] partial struct ArbitraryType;

// IntegerType is here for the purposes of documentation only and is not actually
// part of the unsafe package. It represents any arbitrary integer type.
[GoType("num:nint")] partial struct IntegerType;

// Pointer represents a pointer to an arbitrary type. There are four special operations
// available for type Pointer that are not available for other types:
//   - A pointer value of any type can be converted to a Pointer.
//   - A Pointer can be converted to a pointer value of any type.
//   - A uintptr can be converted to a Pointer.
//   - A Pointer can be converted to a uintptr.
//
// Pointer therefore allows a program to defeat the type system and read and write
// arbitrary memory. It should be used with extreme care.
//
// The following patterns involving Pointer are valid.
// Code not using these patterns is likely to be invalid today
// or to become invalid in the future.
// Even the valid patterns below come with important caveats.
//
// Running "go vet" can help find uses of Pointer that do not conform to these patterns,
// but silence from "go vet" is not a guarantee that the code is valid.
//
// (1) Conversion of a *T1 to Pointer to *T2.
//
// Provided that T2 is no larger than T1 and that the two share an equivalent
// memory layout, this conversion allows reinterpreting data of one type as
// data of another type. An example is the implementation of
// math.Float64bits:
//
//	func Float64bits(f float64) uint64 {
//		return *(*uint64)(unsafe.Pointer(&f))
//	}
//
// (2) Conversion of a Pointer to a uintptr (but not back to Pointer).
//
// Converting a Pointer to a uintptr produces the memory address of the value
// pointed at, as an integer. The usual use for such a uintptr is to print it.
//
// Conversion of a uintptr back to Pointer is not valid in general.
//
// A uintptr is an integer, not a reference.
// Converting a Pointer to a uintptr creates an integer value
// with no pointer semantics.
// Even if a uintptr holds the address of some object,
// the garbage collector will not update that uintptr's value
// if the object moves, nor will that uintptr keep the object
// from being reclaimed.
//
// The remaining patterns enumerate the only valid conversions
// from uintptr to Pointer.
//
// (3) Conversion of a Pointer to a uintptr and back, with arithmetic.
//
// If p points into an allocated object, it can be advanced through the object
// by conversion to uintptr, addition of an offset, and conversion back to Pointer.
//
//	p = unsafe.Pointer(uintptr(p) + offset)
//
// The most common use of this pattern is to access fields in a struct
// or elements of an array:
//
//	// equivalent to f := unsafe.Pointer(&s.f)
//	f := unsafe.Pointer(uintptr(unsafe.Pointer(&s)) + unsafe.Offsetof(s.f))
//
//	// equivalent to e := unsafe.Pointer(&x[i])
//	e := unsafe.Pointer(uintptr(unsafe.Pointer(&x[0])) + i*unsafe.Sizeof(x[0]))
//
// It is valid both to add and to subtract offsets from a pointer in this way.
// It is also valid to use &^ to round pointers, usually for alignment.
// In all cases, the result must continue to point into the original allocated object.
//
// Unlike in C, it is not valid to advance a pointer just beyond the end of
// its original allocation:
//
//	// INVALID: end points outside allocated space.
//	var s thing
//	end = unsafe.Pointer(uintptr(unsafe.Pointer(&s)) + unsafe.Sizeof(s))
//
//	// INVALID: end points outside allocated space.
//	b := make([]byte, n)
//	end = unsafe.Pointer(uintptr(unsafe.Pointer(&b[0])) + uintptr(n))
//
// Note that both conversions must appear in the same expression, with only
// the intervening arithmetic between them:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before conversion back to Pointer.
//	u := uintptr(p)
//	p = unsafe.Pointer(u + offset)
//
// Note that the pointer must point into an allocated object, so it may not be nil.
//
//	// INVALID: conversion of nil pointer
//	u := unsafe.Pointer(nil)
//	p := unsafe.Pointer(uintptr(u) + offset)
//
// (4) Conversion of a Pointer to a uintptr when calling functions like [syscall.Syscall].
//
// The Syscall functions in package syscall pass their uintptr arguments directly
// to the operating system, which then may, depending on the details of the call,
// reinterpret some of them as pointers.
// That is, the system call implementation is implicitly converting certain arguments
// back from uintptr to pointer.
//
// If a pointer argument must be converted to uintptr for use as an argument,
// that conversion must appear in the call expression itself:
//
//	syscall.Syscall(SYS_READ, uintptr(fd), uintptr(unsafe.Pointer(p)), uintptr(n))
//
// The compiler handles a Pointer converted to a uintptr in the argument list of
// a call to a function implemented in assembly by arranging that the referenced
// allocated object, if any, is retained and not moved until the call completes,
// even though from the types alone it would appear that the object is no longer
// needed during the call.
//
// For the compiler to recognize this pattern,
// the conversion must appear in the argument list:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before implicit conversion back to Pointer during system call.
//	u := uintptr(unsafe.Pointer(p))
//	syscall.Syscall(SYS_READ, uintptr(fd), u, uintptr(n))
//
// (5) Conversion of the result of [reflect.Value.Pointer] or [reflect.Value.UnsafeAddr]
// from uintptr to Pointer.
//
// Package reflect's Value methods named Pointer and UnsafeAddr return type uintptr
// instead of unsafe.Pointer to keep callers from changing the result to an arbitrary
// type without first importing "unsafe". However, this means that the result is
// fragile and must be converted to Pointer immediately after making the call,
// in the same expression:
//
//	p := (*int)(unsafe.Pointer(reflect.ValueOf(new(int)).Pointer()))
//
// As in the cases above, it is invalid to store the result before the conversion:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before conversion back to Pointer.
//	u := reflect.ValueOf(new(int)).Pointer()
//	p := (*int)(unsafe.Pointer(u))
//
// (6) Conversion of a [reflect.SliceHeader] or [reflect.StringHeader] Data field to or from Pointer.
//
// As in the previous case, the reflect data structures SliceHeader and StringHeader
// declare the field Data as a uintptr to keep callers from changing the result to
// an arbitrary type without first importing "unsafe". However, this means that
// SliceHeader and StringHeader are only valid when interpreting the content
// of an actual slice or string value.
//
//	var s string
//	hdr := (*reflect.StringHeader)(unsafe.Pointer(&s)) // case 1
//	hdr.Data = uintptr(unsafe.Pointer(p))              // case 6 (this case)
//	hdr.Len = n
//
// In this usage hdr.Data is really an alternate way to refer to the underlying
// pointer in the string header, not a uintptr variable itself.
//
// In general, [reflect.SliceHeader] and [reflect.StringHeader] should be used
// only as *reflect.SliceHeader and *reflect.StringHeader pointing at actual
// slices or strings, never as plain structs.
// A program should not declare or allocate variables of these struct types.
//
//	// INVALID: a directly-declared header will not hold Data as a reference.
//	var hdr reflect.StringHeader
//	hdr.Data = uintptr(unsafe.Pointer(p))
//	hdr.Len = n
//	s := *(*string)(unsafe.Pointer(&hdr)) // p possibly already lost
public class Pointer(uintptr value) : ж<uintptr>(value) {
    public static implicit operator Pointer(uintptr value) {
        return new Pointer(value);
    }

    public static implicit operator uintptr(Pointer value) {
        return value.val;
    }

    public static implicit operator Pointer(void* value) {
        return new Pointer((uintptr)value);
    }

    public static implicit operator void*(Pointer value) {
        return (void*)value.val;
    }
}

// Sizeof takes an expression x of any type and returns the size in bytes
// of a hypothetical variable v as if v was declared via var v = x.
// The size does not include any memory possibly referenced by x.
// For instance, if x is a slice, Sizeof returns the size of the slice
// descriptor, not the size of the memory referenced by the slice;
// if x is an interface, Sizeof returns the size of the interface value itself,
// not the size of the value stored in the interface.
// For a struct, the size includes any padding introduced by field alignment.
// The return value of Sizeof is a Go constant if the type of the argument x
// does not have variable size.
// (A type has variable size if it is a type parameter or if it is an array
// or struct type with elements of variable size).
public static uintptr Sizeof<T>(T x) {
    return (uintptr)Marshal.SizeOf<T>();
}

// Offsetof returns the offset within the struct of the field represented by x,
// which must be of the form structValue.field. In other words, it returns the
// number of bytes between the start of the struct and the start of the field.
// The return value of Offsetof is a Go constant if the type of the argument x
// does not have variable size.
// (See the description of [Sizeof] for a definition of variable sized types.)
// go2cs conversion converts:
// `unsafe.Offsetof(structValue.field)` to
// `@unsafe.Offsetof(structValue.GetType(), "field")`
public static uintptr Offsetof(Type structType, string fieldName) {
    return (uintptr)Marshal.OffsetOf(structType, fieldName);
}

    // Alignof takes an expression x of any type and returns the required alignment
    // of a hypothetical variable v as if v was declared via var v = x.
    // It is the largest value m such that the address of v is always zero mod m.
    // It is the same as the value returned by [reflect.TypeOf](x).Align().
    // As a special case, if a variable s is of struct type and f is a field
    // within that struct, then Alignof(s.f) will return the required alignment
    // of a field of that type within a struct. This case is the same as the
    // value returned by [reflect.TypeOf](s.f).FieldAlign().
    // The return value of Alignof is a Go constant if the type of the argument
    // does not have variable size.
    // (See the description of [Sizeof] for a definition of variable sized types.)
    // go2cs conversion converts:
    // `unsafe.Alignof(x)` to
    // `@unsafe.Alignof(x.GetType())` and
    // `unsafe.Alignof(s.f)` to
    // `@unsafe.Alignof(s.GetType(), "f")`
    public static uintptr Alignof(Type type, string? fieldName = null) {
    // Handle the special case for struct fields
    if (fieldName is not null && type is { IsValueType: true, IsPrimitive: false })
    {
        // Find the specified field
        FieldInfo? field = type.GetField(fieldName, 
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic);

        if (field is not null)
        {
            // Get the field type and determine its alignment
            Type fieldType = field.FieldType;

            // Call Alignof on the field type (without a fieldName parameter)
            // This would require a non-generic version since we can't pass fieldType to a generic method directly
            return AlignofType(fieldType);
        }

        // If field not found, fall back to normal behavior
    }

    // Basic primitive type alignment rules similar to how Go handles them
    if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
        return 1;

    if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
        return 2;

    if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
        return 4;

    if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) || type == typeof(nint) || type == typeof(nuint))
        return 8;

    // For structs, get the largest alignment of any field
    if (type is
    {
        IsValueType: true,
        IsPrimitive: false
    })
    {
        uintptr maxAlignment = 1;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            uintptr fieldAlignment;

            Type fieldType = field.FieldType;

            // Recursively get alignment for field types
            // This would need to call Alignof for the field type
            // We'll use a simplified approach here
            if (fieldType == typeof(bool) || fieldType == typeof(byte) || fieldType == typeof(sbyte))
                fieldAlignment = 1;
            else if (fieldType == typeof(char) || fieldType == typeof(short) || fieldType == typeof(ushort))
                fieldAlignment = 2;
            else if (fieldType == typeof(int) || fieldType == typeof(uint) || fieldType == typeof(float))
                fieldAlignment = 4;
            else if (fieldType == typeof(long) || fieldType == typeof(ulong) || fieldType == typeof(double) || fieldType == typeof(nint) || fieldType == typeof(nuint))
                fieldAlignment = 8;
            else if (fieldType.IsClass)
                fieldAlignment = (uintptr)nint.Size; // Reference types are pointer-aligned
            else
                fieldAlignment = 8; // Default for complex types

            maxAlignment = Math.Max(maxAlignment, fieldAlignment);
        }

        return maxAlignment;
    }

    // For reference types (classes), return the pointer size
    if (type.IsClass)
        return (uintptr)nint.Size;

    // For arrays, return the alignment of the element type
    if (type.IsArray)
    {
        Type? elementType = type.GetElementType();

        // This would ideally call Alignof for the element type
        // For simplicity, we'll use a fixed alignment based on element size
        if (elementType == typeof(bool) || elementType == typeof(byte) || elementType == typeof(sbyte))
            return 1;
        if (elementType == typeof(char) || elementType == typeof(short) || elementType == typeof(ushort))
            return 2;
        if (elementType == typeof(int) || elementType == typeof(uint) || elementType == typeof(float))
            return 4;
        if (elementType == typeof(long) || elementType == typeof(ulong) || elementType == typeof(double))
            return 8;

        return (uintptr)nint.Size; // Default alignment for complex element types
    }

    // Default alignment for unknown types
    return (uintptr)nint.Size;
}

private static uintptr AlignofType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] Type type)
{
    // Basic primitive type alignment rules similar to how Go handles them
    if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
        return 1;

    if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
        return 2;

    if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
        return 4;

    if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) || type == typeof(nint) || type == typeof(nuint))
        return 8;

    // For structs, get the largest alignment of any field
    if (type is
    {
        IsValueType: true,
        IsPrimitive: false
    })
    {
        uintptr maxAlignment = 1;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            uintptr fieldAlignment = AlignofType(field.FieldType);
            maxAlignment = Math.Max(maxAlignment, fieldAlignment);
        }

        return maxAlignment;
    }

    // For reference types (classes), return the pointer size
    if (type.IsClass)
        return (uintptr)nint.Size;

    // For arrays, return the alignment of the element type
    if (type.IsArray)
    {
        Type? elementType = type.GetElementType();

         if (elementType is null)
             return (uintptr)nint.Size;

         return AlignofType(elementType);
    }

    // Default alignment for unknown types
    return (uintptr)nint.Size;
}

// The function Add adds len to ptr and returns the updated pointer
// [Pointer](uintptr(ptr) + uintptr(len)).
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be representable by a value of type int;
// if it is an untyped constant it is given type int.
// The rules for valid uses of Pointer still apply.
public static ж<T> Add<T>(ж<T> ptr, nint len) {
    if (ptr == nil)
        return new ж<T>();

    (IArray array, int index)? arrayRef  = ptr.ArrayRef;

    if (arrayRef is null)
        return new ж<T>();

    (IArray array, int index) = arrayRef.Value;

    return new ж<T>(array, index + (int)len);
}

// The function Slice returns a slice whose underlying array starts at ptr
// and whose length and capacity are len.
// Slice(ptr, len) is equivalent to
//
//	(*[len]ArbitraryType)(unsafe.Pointer(ptr))[:]
//
// except that, as a special case, if ptr is nil and len is zero,
// Slice returns nil.
//
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be non-negative and representable by a value of type int;
// if it is an untyped constant it is given type int.
// At run time, if len is negative, or if ptr is nil and len is not zero,
// a run-time panic occurs.
public static slice<T> Slice<T>(ж<T> ptr, nint len) {
    if (len < 0)
        panic("len is negative");

    if (ptr == nil)
    {
        if (len == 0)
            return [];

        panic("ptr is nil and len is not zero");
    }

    fixed (T* pointer = &ptr.val)
        return new slice<T>(new ReadOnlySpan<T>(pointer, (int)len));
}

// SliceData returns a pointer to the underlying array of the argument
// slice.
//   - If cap(slice) > 0, SliceData returns &slice[:1][0].
//   - If slice == nil, SliceData returns nil.
//   - Otherwise, SliceData returns a non-nil pointer to an
//     unspecified memory address.
public static ж<T> SliceData<T>(slice<T> slice) {
    if (slice == nil)
        return new ж<T>();

    PinnedBuffer buffer = slice.buffer;
    return new ж<T>(buffer, 0);
}

// String returns a string value whose underlying bytes
// start at ptr and whose length is len.
//
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be non-negative and representable by a value of type int;
// if it is an untyped constant it is given type int.
// At run time, if len is negative, or if ptr is nil and len is not zero,
// a run-time panic occurs.
//
// Since Go strings are immutable, the bytes passed to String
// must not be modified as long as the returned string value exists.
public static @string String(ж<byte> ptr, nint len) {
    if (len < 0)
        panic("len is negative");

    if (ptr == nil)
    {
        if (len == 0)
            return [];

        panic("ptr is nil and len is not zero");
    }

    fixed (byte* pointer = &ptr.val)
        return new @string(new ReadOnlySpan<byte>(pointer, (int)len));
}

// StringData returns a pointer to the underlying bytes of str.
// For an empty string the return value is unspecified, and may be nil.
//
// Since Go strings are immutable, the bytes returned by StringData
// must not be modified.
public static ж<byte> StringData(@string str) {
    PinnedBuffer buffer = str.buffer;
    return new ж<byte>(buffer, 0);
}

} // end unsafe_package
