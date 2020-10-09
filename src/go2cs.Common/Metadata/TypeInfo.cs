//******************************************************************************************************
//  GoTypeInfo.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Dahomey.Json.Attributes;

namespace go2cs.Metadata
{
    [Serializable]
    public enum TypeClass
    {
        Simple,
        Array,
        Slice,
        Map,        // Dictionary<kT, vT>
        Channel,
        Struct,
        Function,   // Func<..., T> / Action<...>
        Interface
    }

    public enum DerivedTypeInfo
    {
        Pointer,
        Array,
        Map
    }

    [Serializable]
    public class TypeInfo
    {
        public string Name;
        public string TypeName;
        public string FullTypeName;
        public TypeClass TypeClass;
        public bool IsDerefPointer;
        public bool IsByRefPointer;
        public bool IsConst;

        public virtual TypeInfo Clone()
        {
            return new TypeInfo
            {
                Name = Name,
                TypeName = TypeName,
                FullTypeName = FullTypeName,
                TypeClass = TypeClass,
                IsDerefPointer = IsDerefPointer,
                IsByRefPointer = IsByRefPointer,
                IsConst = IsConst
            };
        }

        public static readonly TypeInfo ObjectType = new TypeInfo
        {
            Name = "object",
            TypeName = "object",
            FullTypeName = "System.Object",
            TypeClass = TypeClass.Simple
        };

        public static readonly TypeInfo VoidType = new TypeInfo
        {
            Name = "void",
            TypeName = "void",
            FullTypeName = "void",
            TypeClass = TypeClass.Simple
        };

        public static readonly TypeInfo VarType = new TypeInfo
        {
            Name = "var",
            TypeName = "var",
            FullTypeName = "var",
            TypeClass = TypeClass.Simple
        };

        public static readonly TypeInfo DynamicType = new TypeInfo
        {
            Name = "dynamic",
            TypeName = "dynamic",
            FullTypeName = "System.Dynamic.DynamicObject",
            TypeClass = TypeClass.Simple
        };

        public static readonly TypeInfo EmptyInterfaceType = new TypeInfo
        {
            Name = "object",
            TypeName = "object",
            FullTypeName = "System.Object",
            TypeClass = TypeClass.Simple
        };
    }

    [Serializable]
    [JsonDiscriminator(DerivedTypeInfo.Pointer)]
    public class PointerTypeInfo : TypeInfo
    {
        public TypeInfo TargetTypeInfo;

        public override TypeInfo Clone()
        {
            return new PointerTypeInfo
            {
                Name = Name,
                TypeName = $"ptr<{TypeName}>",
                FullTypeName = $"go.ptr<{FullTypeName}>",
                TypeClass = TypeClass,
                IsDerefPointer = IsDerefPointer,
                IsByRefPointer = IsByRefPointer,
                IsConst = IsConst,
                TargetTypeInfo = TargetTypeInfo
            };
        }
    }

    [Serializable]
    [JsonDiscriminator(DerivedTypeInfo.Array)]
    public class ArrayTypeInfo : TypeInfo
    {
        public TypeInfo TargetTypeInfo;
        public ExpressionInfo Length;

        public override TypeInfo Clone()
        {
            return new ArrayTypeInfo
            {
                Name = Name,
                TypeName = $"array<{TypeName}>",
                FullTypeName = $"go.array<{FullTypeName}>",
                TypeClass = TypeClass,
                IsDerefPointer = IsDerefPointer,
                IsByRefPointer = IsByRefPointer,
                IsConst = IsConst,
                TargetTypeInfo = TargetTypeInfo,
                Length = Length
            };
        }
    }

    [Serializable]
    [JsonDiscriminator(DerivedTypeInfo.Map)]
    public class MapTypeInfo : TypeInfo
    {
        public TypeInfo KeyTypeInfo;
        public TypeInfo ElementTypeInfo;

        public override TypeInfo Clone()
        {
            return new MapTypeInfo
            {
                Name = Name,
                TypeName = TypeName,
                FullTypeName = FullTypeName,
                TypeClass = TypeClass,
                IsDerefPointer = IsDerefPointer,
                IsByRefPointer = IsByRefPointer,
                IsConst = IsConst,
                KeyTypeInfo = KeyTypeInfo,
                ElementTypeInfo = ElementTypeInfo
            };
        }
    }
}
