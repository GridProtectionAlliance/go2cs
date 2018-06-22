//******************************************************************************************************
//  ScannerBase_Type.cs - Gbtc
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
//  05/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System.Linq;

namespace go2cs
{
    public partial class ScannerBase
    {
        // Stack handlers:
        //  constSpec (optional)
        //  typeSpec (required)
        //  varSpec (required)
        //  typeList (required)
        //  type (optional)
        //  elementType (required)
        //  pointerType (required)
        //  mapType (required)
        //  result (optional)
        //  parameterDecl (required)
        //  fieldDecl (optional)
        //  typeAssertion (required)
        //  arguments (optional)
        //  conversion (required)
        protected readonly ParseTreeValues<TypeInfo> Types = new ParseTreeValues<TypeInfo>();

        public override void EnterTypeName(GolangParser.TypeNameContext context)
        {
            string type = context.GetText();

            Types[context.Parent] = new TypeInfo
            {
                Name = type,
                PrimitiveName = ConvertToPrimitiveType(type),
                FrameworkName = ConvertToFrameworkType(type),
                TypeClass = TypeClass.Simple
            };
        }

        public override void ExitPointerType(GolangParser.PointerTypeContext context)
        {
            string type = context.GetText();

            Types.TryGetValue(context.type(), out TypeInfo typeInfo);

            if (typeInfo == null)
                return; // throw new InvalidOperationException("Pointer type undefined.");

            if (type.StartsWith("**"))
            {
                int count = 0;

                while (type[count] == '*')
                    count++;

                count = count - (typeInfo.IsByRefPointer ? 1 : 0);

                string prefix = string.Join("", Enumerable.Range(0, count).Select(i => "Ptr<"));
                string suffix = new string('>', count);

                typeInfo = ConvertByRefToBasicPointer(typeInfo);

                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = type,
                    PrimitiveName = $"{prefix}{typeInfo.PrimitiveName}{suffix}",
                    FrameworkName = $"{prefix}{typeInfo.FrameworkName}{suffix}",
                    IsPointer = true,
                    IsByRefPointer = false,
                    TypeClass = TypeClass.Simple
                };
            }
            else
            {
                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = type,
                    PrimitiveName = $"ref {typeInfo.PrimitiveName}",
                    FrameworkName = $"ref {typeInfo.FrameworkName}",
                    IsPointer = true,
                    IsByRefPointer = true,
                    TypeClass = TypeClass.Simple
                };
            }
        }

        public override void ExitArrayType(GolangParser.ArrayTypeContext context)
        {
            string type = context.GetText();

            Types.TryGetValue(context.elementType().type(), out TypeInfo typeInfo);

            if (typeInfo == null)
                return; // throw new InvalidOperationException("Array type undefined.");

            Expressions.TryGetValue(context.arrayLength().expression(), out string expression);

            string arrayLength = $"[{expression}]";

            typeInfo.PrimitiveName += arrayLength;
            typeInfo.FrameworkName += arrayLength;
            typeInfo.IsArray = true;

            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = type,
                PrimitiveName = $"{typeInfo.PrimitiveName}{arrayLength}",
                FrameworkName = $"{typeInfo.FrameworkName}{arrayLength}",
                TypeClass = TypeClass.Simple,
                IsArray = true
            };
        }

        public override void ExitMapType(GolangParser.MapTypeContext context)
        {
            string type = context.GetText();

            Types.TryGetValue(context.type(), out TypeInfo keyTypeInfo);

            if (keyTypeInfo == null)
                return; // throw new InvalidOperationException("Map key type undefined.");

            Types.TryGetValue(context.elementType().type(), out TypeInfo elementTypeInfo);

            if (elementTypeInfo == null)
                return; // throw new InvalidOperationException("Map element type undefined.");

            RequiredUsings.Add("System.Collections.Generic");

            Types[context.Parent.Parent] = new MapTypeInfo
            {
                Name = type,
                PrimitiveName = $"Dictionary<{keyTypeInfo.PrimitiveName}, {elementTypeInfo.PrimitiveName}>",
                FrameworkName = $"System.Collections.Generic.Dictionary<{keyTypeInfo.FrameworkName}, {elementTypeInfo.FrameworkName}>",
                ElementTypeInfo = elementTypeInfo,
                KeyTypeInfo = keyTypeInfo,
                TypeClass = TypeClass.Map
            };
        }

        public override void ExitSliceType(GolangParser.SliceTypeContext context)
        {
            Types.TryGetValue(context.elementType().type(), out TypeInfo typeInfo);

            if (typeInfo == null)
                typeInfo = TypeInfo.ObjectType;

            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = typeInfo.Name,
                PrimitiveName = $"Slice<{typeInfo.PrimitiveName}>",
                FrameworkName = $"goutil.Slice<{typeInfo.FrameworkName}>",
                TypeClass = TypeClass.Simple,
                IsSlice = true
            };
        }

        public override void ExitChannelType(GolangParser.ChannelTypeContext context)
        {
            // TODO: Add new channel type
            //m_types.TryGetValue(context.elementType().type(), out GoTypeInfo typeInfo);
        }

        private string ConvertToPrimitiveType(string type)
        {
            switch (type)
            {
                case "int8":
                    return "sbyte";
                case "uint8":
                case "byte":
                    return "byte";
                case "int16":
                    return "short";
                case "uint16":
                    return "ushort";
                case "int32":
                    return "int";
                case "uint32":
                    return "uint";
                case "int64":
                case "int":
                    return "long";
                case "uint64":
                case "uint":
                    return "ulong";
                case "float32":
                    return "single";
                case "float64":
                    return "double";
                case "rune":
                    return "char";
                case "uintptr":
                    return "System.UIntPtr";
                case "complex64 ":
                case "complex128":
                    RequiredUsings.Add("System.Numerics");
                    return "Complex";
                case "Type":
                    return "object";
                default:
                    return $"{type}";
            }
        }

        private string ConvertToFrameworkType(string type)
        {
            switch (type)
            {
                case "int8":
                    return "System.SByte";
                case "uint8":
                case "byte":
                    return "System.Byte";
                case "int16":
                    return "System.Int16";
                case "uint16":
                    return "System.UInt16";
                case "int32":
                    return "System.Int32";
                case "uint32":
                    return "System.UInt32";
                case "int64":
                case "int":
                    return "System.Int64";
                case "uint64":
                case "uint":
                    return "System.UInt64";
                case "float32":
                    return "System.Single";
                case "float64":
                    return "System.Double";
                case "rune":
                    return "System.Char";
                case "uintptr":
                    return "System.UIntPtr";
                case "complex64 ":
                case "complex128":
                    return "System.Numerics.Complex";
                case "Type":
                    return "System.Object";
                default:
                    return $"{type}";
            }
        }
    }
}