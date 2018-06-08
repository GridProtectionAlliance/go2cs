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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static go2cs.Common;

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
        private readonly ParseTreeValues<string> m_structFields = new ParseTreeValues<string>();
        private readonly ParseTreeValues<List<(string functionName, string parameterSignature, string namedParameters, string parameterTypes, string resultType)>> m_interfaceMethods = new ParseTreeValues<List<(string, string, string, string, string)>>();

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
                PrimitiveName = ConvertToPrimitiveType(type),
                FrameworkName = ConvertToFrameworkType(type),
                TypeClass = GoTypeClass.Simple
            };
        }

        public override void ExitPointerType(GolangParser.PointerTypeContext context)
        {
            string type = context.GetText();

            m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo);

            if (typeInfo == null)
                return; // throw new InvalidOperationException("Pointer type undefined.");

            m_types[context.Parent.Parent] = new GoTypeInfo
            {
                Name = type,
                PrimitiveName = $"ref {typeInfo.PrimitiveName}",
                FrameworkName = $"ref {typeInfo.FrameworkName}",
                IsPointer = true,
                TypeClass = GoTypeClass.Simple
            };
        }

        public override void ExitArrayType(GolangParser.ArrayTypeContext context)
        {
            string type = context.GetText();

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
                Name = type,
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
                PrimitiveName = $"Slice<{typeInfo.PrimitiveName}>",
                FrameworkName = $"goutil.Slice<{typeInfo.FrameworkName}>",
                TypeClass = GoTypeClass.Simple,
                IsSlice = true
            };
        }

        public override void ExitStructType(GolangParser.StructTypeContext context)
        {
            List<(string fieldName, string fieldType, string description, string comment)> fields = new List<(string, string, string, string)>();

            for (int i = 0; i < context.fieldDecl().Length; i++)
            {
                GolangParser.FieldDeclContext fieldDecl = context.fieldDecl(i);
                GoTypeInfo typeInfo;
                string description = ToStringLiteral(fieldDecl.STRING_LIT()?.GetText());

                if (m_identifiers.TryGetValue(fieldDecl.identifierList(), out string[] identifiers) && m_types.TryGetValue(fieldDecl.type(), out typeInfo))
                {
                    foreach (string identifier in identifiers)
                        fields.Add((SanitizedIdentifier(identifier), typeInfo.PrimitiveName, description, CheckForCommentsRight(fieldDecl)));
                }
                else if (m_types.TryGetValue(fieldDecl.anonymousField(), out typeInfo))
                {
                    // TODO: Handle promoted structures and interfaces
                    fields.Add((GetValidIdentifierName(typeInfo.PrimitiveName), typeInfo.PrimitiveName, description, CheckForCommentsRight(fieldDecl)));
                }
            }

            string getStructureField((string fieldName, string fieldType, string description, string comment) field)
            {
                StringBuilder fieldDecl = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(field.description))
                {
                    string description = field.description.Trim();

                    if (description.Length > 2)
                    {
                        m_requiredUsings.Add("System.ComponentModel");
                        fieldDecl.AppendLine($"{Spacing(1)}[Description({description})]");
                    }
                }

                fieldDecl.Append($"{Spacing(1)}public {field.fieldType} {field.fieldName};");

                if (!string.IsNullOrEmpty(field.comment))
                    fieldDecl.Append(field.comment);

                return fieldDecl.ToString();
            }

            m_structFields[context] = string.Join(Environment.NewLine, fields.Select(getStructureField));
        }

        public override void ExitInterfaceType(GolangParser.InterfaceTypeContext context)
        {
            List<(string functionName, string parameterSignature, string namedParameters, string parameterTypes, string resultType)> functions = new List<(string, string, string, string, string)>();

            for (int i = 0; i < context.methodSpec().Length; i++)
            {
                GolangParser.MethodSpecContext methodSpec = context.methodSpec(i);

                string identifier = methodSpec.IDENTIFIER()?.GetText();

                if (string.IsNullOrEmpty(identifier))
                {
                    // TODO: Handle promoted interfaces
                    //if (m_types.TryGetValue(methodSpec, out GoTypeInfo typeInfo))
                    //    methods.Add($"{Spacing(1)}public {typeInfo.PrimitiveName} {GetValidIdentifierName(typeInfo.PrimitiveName)} {{ get; set; }}");
                }
                else
                {
                    //(string parameters, string result) signature;
                    identifier = GetValidIdentifierName(identifier);

                    if (m_signatures.TryGetValue(methodSpec.signature(), out (string parameters, string result) signature))
                    {
                        string parameters = RemoveSurrounding(signature.parameters, "(", ")");
                        ExtractParameterLists(parameters, out string namedParameters, out string parameterTypes);
                        functions.Add((identifier, parameters, namedParameters, parameterTypes, signature.result));
                    }
                }
            }

            m_interfaceMethods[context] = functions;
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

        private static string GetValidIdentifierName(string identifier)
        {
            int lastDotIndex = identifier.LastIndexOf('.');

            if (lastDotIndex > 0)
                identifier = identifier.Substring(lastDotIndex + 1);

            return SanitizedIdentifier(identifier);
        }
    }
}