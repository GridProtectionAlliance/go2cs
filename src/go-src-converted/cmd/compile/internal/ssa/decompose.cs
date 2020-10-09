// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:38 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\decompose.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // decompose converts phi ops on compound builtin types into phi
        // ops on simple types, then invokes rewrite rules to decompose
        // other ops on those types.
        private static void decomposeBuiltIn(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;
 
            // Decompose phis
            foreach (var (_, b) in f.Blocks)
            {
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op != OpPhi)
                        {
                            continue;
                        }
                        decomposeBuiltInPhi(_addr_v);

                    }
                    v = v__prev2;
                }
            }            applyRewrite(f, rewriteBlockdec, rewriteValuedec);
            if (f.Config.RegSize == 4L)
            {
                applyRewrite(f, rewriteBlockdec64, rewriteValuedec64);
            }
            slice<LocalSlot> newNames = default;
            foreach (var (_, name) in f.Names)
            {
                var t = name.Type;

                if (t.IsInteger() && t.Size() > f.Config.RegSize) 
                    var (hiName, loName) = f.fe.SplitInt64(name);
                    newNames = append(newNames, hiName, loName);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            if (v.Op != OpInt64Make)
                            {
                                continue;
                            }
                            f.NamedValues[hiName] = append(f.NamedValues[hiName], v.Args[0L]);
                            f.NamedValues[loName] = append(f.NamedValues[loName], v.Args[1L]);

                        }
                        v = v__prev2;
                    }

                    delete(f.NamedValues, name);
                else if (t.IsComplex()) 
                    var (rName, iName) = f.fe.SplitComplex(name);
                    newNames = append(newNames, rName, iName);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            if (v.Op != OpComplexMake)
                            {
                                continue;
                            }
                            f.NamedValues[rName] = append(f.NamedValues[rName], v.Args[0L]);
                            f.NamedValues[iName] = append(f.NamedValues[iName], v.Args[1L]);


                        }
                        v = v__prev2;
                    }

                    delete(f.NamedValues, name);
                else if (t.IsString()) 
                    var (ptrName, lenName) = f.fe.SplitString(name);
                    newNames = append(newNames, ptrName, lenName);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            if (v.Op != OpStringMake)
                            {
                                continue;
                            }
                            f.NamedValues[ptrName] = append(f.NamedValues[ptrName], v.Args[0L]);
                            f.NamedValues[lenName] = append(f.NamedValues[lenName], v.Args[1L]);

                        }
                        v = v__prev2;
                    }

                    delete(f.NamedValues, name);
                else if (t.IsSlice()) 
                    var (ptrName, lenName, capName) = f.fe.SplitSlice(name);
                    newNames = append(newNames, ptrName, lenName, capName);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            if (v.Op != OpSliceMake)
                            {
                                continue;
                            }
                            f.NamedValues[ptrName] = append(f.NamedValues[ptrName], v.Args[0L]);
                            f.NamedValues[lenName] = append(f.NamedValues[lenName], v.Args[1L]);
                            f.NamedValues[capName] = append(f.NamedValues[capName], v.Args[2L]);

                        }
                        v = v__prev2;
                    }

                    delete(f.NamedValues, name);
                else if (t.IsInterface()) 
                    var (typeName, dataName) = f.fe.SplitInterface(name);
                    newNames = append(newNames, typeName, dataName);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            if (v.Op != OpIMake)
                            {
                                continue;
                            }
                            f.NamedValues[typeName] = append(f.NamedValues[typeName], v.Args[0L]);
                            f.NamedValues[dataName] = append(f.NamedValues[dataName], v.Args[1L]);

                        }
                        v = v__prev2;
                    }

                    delete(f.NamedValues, name);
                else if (t.IsFloat()) 
                    // floats are never decomposed, even ones bigger than RegSize
                    newNames = append(newNames, name);
                else if (t.Size() > f.Config.RegSize) 
                    f.Fatalf("undecomposed named type %s %v", name, t);
                else 
                    newNames = append(newNames, name);
                
            }            f.Names = newNames;

        }

        private static void decomposeBuiltInPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Type.IsInteger() && v.Type.Size() > v.Block.Func.Config.RegSize) 
                decomposeInt64Phi(_addr_v);
            else if (v.Type.IsComplex()) 
                decomposeComplexPhi(_addr_v);
            else if (v.Type.IsString()) 
                decomposeStringPhi(_addr_v);
            else if (v.Type.IsSlice()) 
                decomposeSlicePhi(_addr_v);
            else if (v.Type.IsInterface()) 
                decomposeInterfacePhi(_addr_v);
            else if (v.Type.IsFloat())             else if (v.Type.Size() > v.Block.Func.Config.RegSize) 
                v.Fatalf("undecomposed type %s", v.Type);
            
        }

        private static void decomposeStringPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var types = _addr_v.Block.Func.Config.Types;
            var ptrType = types.BytePtr;
            var lenType = types.Int;

            var ptr = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
            var len = v.Block.NewValue0(v.Pos, OpPhi, lenType);
            foreach (var (_, a) in v.Args)
            {
                ptr.AddArg(a.Block.NewValue1(v.Pos, OpStringPtr, ptrType, a));
                len.AddArg(a.Block.NewValue1(v.Pos, OpStringLen, lenType, a));
            }
            v.reset(OpStringMake);
            v.AddArg(ptr);
            v.AddArg(len);

        }

        private static void decomposeSlicePhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var types = _addr_v.Block.Func.Config.Types;
            var ptrType = types.BytePtr;
            var lenType = types.Int;

            var ptr = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
            var len = v.Block.NewValue0(v.Pos, OpPhi, lenType);
            var cap = v.Block.NewValue0(v.Pos, OpPhi, lenType);
            foreach (var (_, a) in v.Args)
            {
                ptr.AddArg(a.Block.NewValue1(v.Pos, OpSlicePtr, ptrType, a));
                len.AddArg(a.Block.NewValue1(v.Pos, OpSliceLen, lenType, a));
                cap.AddArg(a.Block.NewValue1(v.Pos, OpSliceCap, lenType, a));
            }
            v.reset(OpSliceMake);
            v.AddArg(ptr);
            v.AddArg(len);
            v.AddArg(cap);

        }

        private static void decomposeInt64Phi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var cfgtypes = _addr_v.Block.Func.Config.Types;
            ptr<types.Type> partType;
            if (v.Type.IsSigned())
            {
                partType = cfgtypes.Int32;
            }
            else
            {
                partType = cfgtypes.UInt32;
            }

            var hi = v.Block.NewValue0(v.Pos, OpPhi, partType);
            var lo = v.Block.NewValue0(v.Pos, OpPhi, cfgtypes.UInt32);
            foreach (var (_, a) in v.Args)
            {
                hi.AddArg(a.Block.NewValue1(v.Pos, OpInt64Hi, partType, a));
                lo.AddArg(a.Block.NewValue1(v.Pos, OpInt64Lo, cfgtypes.UInt32, a));
            }
            v.reset(OpInt64Make);
            v.AddArg(hi);
            v.AddArg(lo);

        }

        private static void decomposeComplexPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var cfgtypes = _addr_v.Block.Func.Config.Types;
            ptr<types.Type> partType;
            {
                var z = v.Type.Size();

                switch (z)
                {
                    case 8L: 
                        partType = cfgtypes.Float32;
                        break;
                    case 16L: 
                        partType = cfgtypes.Float64;
                        break;
                    default: 
                        v.Fatalf("decomposeComplexPhi: bad complex size %d", z);
                        break;
                }
            }

            var real = v.Block.NewValue0(v.Pos, OpPhi, partType);
            var imag = v.Block.NewValue0(v.Pos, OpPhi, partType);
            foreach (var (_, a) in v.Args)
            {
                real.AddArg(a.Block.NewValue1(v.Pos, OpComplexReal, partType, a));
                imag.AddArg(a.Block.NewValue1(v.Pos, OpComplexImag, partType, a));
            }
            v.reset(OpComplexMake);
            v.AddArg(real);
            v.AddArg(imag);

        }

        private static void decomposeInterfacePhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var uintptrType = v.Block.Func.Config.Types.Uintptr;
            var ptrType = v.Block.Func.Config.Types.BytePtr;

            var itab = v.Block.NewValue0(v.Pos, OpPhi, uintptrType);
            var data = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
            foreach (var (_, a) in v.Args)
            {
                itab.AddArg(a.Block.NewValue1(v.Pos, OpITab, uintptrType, a));
                data.AddArg(a.Block.NewValue1(v.Pos, OpIData, ptrType, a));
            }
            v.reset(OpIMake);
            v.AddArg(itab);
            v.AddArg(data);

        }

        private static void decomposeArgs(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            applyRewrite(f, rewriteBlockdecArgs, rewriteValuedecArgs);
        }

        private static void decomposeUser(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }

                    decomposeUserPhi(_addr_v);

                }

            } 
            // Split up named values into their components.
            long i = 0L;
            slice<LocalSlot> newNames = default;
            foreach (var (_, name) in f.Names)
            {
                var t = name.Type;

                if (t.IsStruct()) 
                    newNames = decomposeUserStructInto(_addr_f, name, newNames);
                else if (t.IsArray()) 
                    newNames = decomposeUserArrayInto(_addr_f, name, newNames);
                else 
                    f.Names[i] = name;
                    i++;
                
            }
            f.Names = f.Names[..i];
            f.Names = append(f.Names, newNames);

        }

        // decomposeUserArrayInto creates names for the element(s) of arrays referenced
        // by name where possible, and appends those new names to slots, which is then
        // returned.
        private static slice<LocalSlot> decomposeUserArrayInto(ptr<Func> _addr_f, LocalSlot name, slice<LocalSlot> slots)
        {
            ref Func f = ref _addr_f.val;

            var t = name.Type;
            if (t.NumElem() == 0L)
            { 
                // TODO(khr): Not sure what to do here.  Probably nothing.
                // Names for empty arrays aren't important.
                return slots;

            }

            if (t.NumElem() != 1L)
            { 
                // shouldn't get here due to CanSSA
                f.Fatalf("array not of size 1");

            }

            var elemName = f.fe.SplitArray(name);
            foreach (var (_, v) in f.NamedValues[name])
            {
                if (v.Op != OpArrayMake1)
                {
                    continue;
                }

                f.NamedValues[elemName] = append(f.NamedValues[elemName], v.Args[0L]);

            } 
            // delete the name for the array as a whole
            delete(f.NamedValues, name);

            if (t.Elem().IsArray())
            {
                return decomposeUserArrayInto(_addr_f, elemName, slots);
            }
            else if (t.Elem().IsStruct())
            {
                return decomposeUserStructInto(_addr_f, elemName, slots);
            }

            return append(slots, elemName);

        }

        // decomposeUserStructInto creates names for the fields(s) of structs referenced
        // by name where possible, and appends those new names to slots, which is then
        // returned.
        private static slice<LocalSlot> decomposeUserStructInto(ptr<Func> _addr_f, LocalSlot name, slice<LocalSlot> slots)
        {
            ref Func f = ref _addr_f.val;

            LocalSlot fnames = new slice<LocalSlot>(new LocalSlot[] {  }); // slots for struct in name
            var t = name.Type;
            var n = t.NumFields();

            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    var fs = f.fe.SplitStruct(name, i);
                    fnames = append(fnames, fs); 
                    // arrays and structs will be decomposed further, so
                    // there's no need to record a name
                    if (!fs.Type.IsArray() && !fs.Type.IsStruct())
                    {
                        slots = append(slots, fs);
                    }

                }


                i = i__prev1;
            }

            var makeOp = StructMakeOp(n); 
            // create named values for each struct field
            foreach (var (_, v) in f.NamedValues[name])
            {
                if (v.Op != makeOp)
                {
                    continue;
                }

                {
                    long i__prev2 = i;

                    for (i = 0L; i < len(fnames); i++)
                    {
                        f.NamedValues[fnames[i]] = append(f.NamedValues[fnames[i]], v.Args[i]);
                    }


                    i = i__prev2;
                }

            } 
            // remove the name of the struct as a whole
            delete(f.NamedValues, name); 

            // now that this f.NamedValues contains values for the struct
            // fields, recurse into nested structs
            {
                long i__prev1 = i;

                for (i = 0L; i < n; i++)
                {
                    if (name.Type.FieldType(i).IsStruct())
                    {
                        slots = decomposeUserStructInto(_addr_f, fnames[i], slots);
                        delete(f.NamedValues, fnames[i]);
                    }
                    else if (name.Type.FieldType(i).IsArray())
                    {
                        slots = decomposeUserArrayInto(_addr_f, fnames[i], slots);
                        delete(f.NamedValues, fnames[i]);
                    }

                }


                i = i__prev1;
            }
            return slots;

        }
        private static void decomposeUserPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Type.IsStruct()) 
                decomposeStructPhi(_addr_v);
            else if (v.Type.IsArray()) 
                decomposeArrayPhi(_addr_v);
            
        }

        // decomposeStructPhi replaces phi-of-struct with structmake(phi-for-each-field),
        // and then recursively decomposes the phis for each field.
        private static void decomposeStructPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var t = v.Type;
            var n = t.NumFields();
            array<ptr<Value>> fields = new array<ptr<Value>>(MaxStruct);
            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    fields[i] = v.Block.NewValue0(v.Pos, OpPhi, t.FieldType(i));
                }


                i = i__prev1;
            }
            foreach (var (_, a) in v.Args)
            {
                {
                    long i__prev2 = i;

                    for (i = 0L; i < n; i++)
                    {
                        fields[i].AddArg(a.Block.NewValue1I(v.Pos, OpStructSelect, t.FieldType(i), int64(i), a));
                    }


                    i = i__prev2;
                }

            }
            v.reset(StructMakeOp(n));
            v.AddArgs(fields[..n]); 

            // Recursively decompose phis for each field.
            foreach (var (_, f) in fields[..n])
            {
                decomposeUserPhi(_addr_f);
            }

        }

        // decomposeArrayPhi replaces phi-of-array with arraymake(phi-of-array-element),
        // and then recursively decomposes the element phi.
        private static void decomposeArrayPhi(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var t = v.Type;
            if (t.NumElem() == 0L)
            {
                v.reset(OpArrayMake0);
                return ;
            }

            if (t.NumElem() != 1L)
            {
                v.Fatalf("SSAable array must have no more than 1 element");
            }

            var elem = v.Block.NewValue0(v.Pos, OpPhi, t.Elem());
            foreach (var (_, a) in v.Args)
            {
                elem.AddArg(a.Block.NewValue1I(v.Pos, OpArraySelect, t.Elem(), 0L, a));
            }
            v.reset(OpArrayMake1);
            v.AddArg(elem); 

            // Recursively decompose elem phi.
            decomposeUserPhi(_addr_elem);

        }

        // MaxStruct is the maximum number of fields a struct
        // can have and still be SSAable.
        public static readonly long MaxStruct = (long)4L;

        // StructMakeOp returns the opcode to construct a struct with the
        // given number of fields.


        // StructMakeOp returns the opcode to construct a struct with the
        // given number of fields.
        public static Op StructMakeOp(long nf) => func((_, panic, __) =>
        {
            switch (nf)
            {
                case 0L: 
                    return OpStructMake0;
                    break;
                case 1L: 
                    return OpStructMake1;
                    break;
                case 2L: 
                    return OpStructMake2;
                    break;
                case 3L: 
                    return OpStructMake3;
                    break;
                case 4L: 
                    return OpStructMake4;
                    break;
            }
            panic("too many fields in an SSAable struct");

        });
    }
}}}}
