//******************************************************************************************************
//  Converter_Type.cs - Gbtc
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

using System;

namespace go2cs
{
    public partial class Converter
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
        private readonly ParseTreeValues<GoTypeInfo> m_types = new ParseTreeValues<GoTypeInfo>();

        private enum GoTypeClass
        {
            Simple,
            Struct,
            Function,   // Func<..., T> / Action<...>
            Interface,
            Map,        // Dictionary<kT, vT>
            Channel
        }

        private class GoTypeInfo
        {
            public string Name;
            public string MappedName;       // Intra-function declaration name
            public string PrimitiveName;
            public string FrameworkName;
            public GoTypeClass TypeClass;
            public bool IsArray;
            public bool IsPointer;          // Nullable<T> (as '?' operator)
            public bool IsSlice;            // Slice<T>
        }

        private class GoMapTypeInfo : GoTypeInfo
        {
            public GoTypeInfo KeyTypeInfo;
            public GoTypeInfo ElementTypeInfo;
        }

        private readonly GoTypeInfo ObjectType = new GoTypeInfo
        {
            Name = "object",
            MappedName = "object",
            PrimitiveName = "object",
            FrameworkName = "System.Object",
            TypeClass = GoTypeClass.Simple
        };

        private string GetMappedName(string name)
        {
            if (!string.IsNullOrWhiteSpace(m_currentFunction))
                return $"_{m_currentFunction}_{name}";

            return name;
        }

        public override void EnterTypeName(GolangParser.TypeNameContext context)
        {
            string type = context.GetText();

            m_types[context.Parent] = new GoTypeInfo
            {
                Name = type,
                MappedName = GetMappedName(type),
                PrimitiveName = ConvertToPrimitiveType(type),
                FrameworkName = ConvertToFrameworkType(type),
                TypeClass = GoTypeClass.Simple
            };
        }

        public override void ExitPointerType(GolangParser.PointerTypeContext context)
        {
            m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo);

            if (typeInfo == null)
                return; // throw new InvalidOperationException("Pointer type undefined.");

            typeInfo.PrimitiveName += "?";
            typeInfo.FrameworkName += "?";
            typeInfo.IsPointer = true;
        }

        public override void ExitArrayType(GolangParser.ArrayTypeContext context)
        {
            m_types.TryGetValue(context.elementType().type(), out GoTypeInfo typeInfo);

            if (typeInfo == null)
                return; // throw new InvalidOperationException("Array type undefined.");

            m_expressions.TryGetValue(context.arrayLength().expression(), out string expression);

            string arrayLength = $"[{expression}]";

            typeInfo.PrimitiveName += arrayLength;
            typeInfo.FrameworkName += arrayLength;
            typeInfo.IsArray = true;

            m_types[context.Parent.Parent] = new GoTypeInfo
            {
                Name = typeInfo.Name,
                MappedName = typeInfo.MappedName,
                PrimitiveName = $"{typeInfo.PrimitiveName}{arrayLength}",
                FrameworkName = $"{typeInfo.FrameworkName}{arrayLength}",
                TypeClass = GoTypeClass.Simple,
                IsArray = true
            };
        }

        public override void ExitMapType(GolangParser.MapTypeContext context)
        {
            string type = context.GetText();

            m_types.TryGetValue(context.type(), out GoTypeInfo keyTypeInfo);

            if (keyTypeInfo == null)
                return; // throw new InvalidOperationException("Map key type undefined.");

            m_types.TryGetValue(context.elementType().type(), out GoTypeInfo elementTypeInfo);

            if (elementTypeInfo == null)
                return; // throw new InvalidOperationException("Map element type undefined.");

            m_requiredUsings.Add("System.Collections.Generic");

            m_types[context.Parent.Parent] = new GoMapTypeInfo
            {
                Name = type,
                MappedName = type,
                PrimitiveName = $"Dictionary<{keyTypeInfo.PrimitiveName}, {elementTypeInfo.PrimitiveName}>",
                FrameworkName = $"System.Collections.Generic.Dictionary<{keyTypeInfo.FrameworkName}, {elementTypeInfo.FrameworkName}>",
                ElementTypeInfo = elementTypeInfo,
                KeyTypeInfo = keyTypeInfo,
                TypeClass = GoTypeClass.Map
            };
        }

        public override void ExitSliceType(GolangParser.SliceTypeContext context)
        {
            m_types.TryGetValue(context.elementType().type(), out GoTypeInfo typeInfo);

            if (typeInfo == null)
                typeInfo = ObjectType;

            m_types[context.Parent.Parent] = new GoTypeInfo
            {
                Name = typeInfo.Name,
                MappedName = typeInfo.MappedName,
                PrimitiveName = $"Slice<{typeInfo.PrimitiveName}>",
                FrameworkName = $"goutil.Slice<{typeInfo.FrameworkName}>",
                TypeClass = GoTypeClass.Simple,
                IsSlice = true
            };
        }

        public override void ExitStructType(GolangParser.StructTypeContext context)
        {
            // TODO: Add new struct type...
            //m_types.TryGetValue(context., out GoTypeInfo typeInfo);
            //m_types.Peek().TypeClass = GoTypeClass.Struct;



            m_types[context.Parent.Parent] = new GoTypeInfo
            {
                //Name = typeInfo.Name,
                //MappedName = typeInfo.MappedName,
                //PrimitiveName = $"Slice<{typeInfo.PrimitiveName}>",
                //FrameworkName = $"goutil.Slice<{typeInfo.FrameworkName}>",
                //TypeClass = GoTypeClass.Simple,
                //IsSlice = true
            };
        }

        public override void ExitFunctionType(GolangParser.FunctionTypeContext context)
        {
            m_signatures.TryGetValue(context.signature(), out (string parameters, string result) signature);

            string functionSignature = ExtractFunctionSignature(signature.parameters);
            string primitiveName, frameworkName;

            if (signature.result == "void")
            {
                primitiveName = $"Action<{functionSignature}>";
                frameworkName = $"System.Action<{functionSignature}>";
            }
            else
            {
                primitiveName = $"Func<{functionSignature}, {signature.result}>";
                frameworkName = $"System.Func<{functionSignature}, {signature.result}>";
            }

            m_types[context.Parent.Parent] = new GoTypeInfo
            {
                Name = context.GetText(),
                PrimitiveName = primitiveName,
                FrameworkName = frameworkName,
                TypeClass = GoTypeClass.Function
            };
        }

        public override void ExitInterfaceType(GolangParser.InterfaceTypeContext context)
        {
            // TODO: Add new interface type
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
                    m_requiredUsings.Add("System.Numerics");
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