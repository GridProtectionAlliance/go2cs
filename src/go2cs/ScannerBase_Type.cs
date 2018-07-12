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
            string name = context.GetText();

            if (!Types.TryGetValue(context.type(), out TypeInfo typeInfo))
            {
                AddWarning(context, $"Failed to find pointer type info for \"{name}\"");
                return;
            }

            if (name.StartsWith("**"))
            {
                int count = 0;

                while (name[count] == '*')
                    count++;

                count = count - (typeInfo.IsByRefPointer ? 1 : 0);

                string prefix = string.Join("", Enumerable.Range(0, count).Select(i => "Ptr<"));
                string suffix = new string('>', count);

                typeInfo = ConvertByRefToBasicPointer(typeInfo);

                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = name,
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
                    Name = name,
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
            string name = context.GetText();

            if (!Types.TryGetValue(context.elementType().type(), out TypeInfo typeInfo))
            {
                AddWarning(context, $"Failed to find array type info for \"{name}\"");
                return;
            }

            // TODO: Remove once expressions dictionary holds expression info
            ExpressionInfo length = new ExpressionInfo
            {
                Type = new TypeInfo
                {
                    TypeClass = TypeClass.Simple,
                    PrimitiveName = "int",
                    FrameworkName = "System.Int32"
                }
            };

            if (Expressions.TryGetValue(context.arrayLength().expression(), out string expression))
            {
                length.Text = expression;
            }
            else
            {
                length.Text = "0";
                AddWarning(context, $"Failed to find array length expression for \"{name}\"");
            }

            Types[context.Parent.Parent] = new ArrayTypeInfo
            {
                Name = name,
                PrimitiveName = $"{typeInfo.PrimitiveName}[]",
                FrameworkName = $"{typeInfo.FrameworkName}[]",
                TypeClass = TypeClass.Array,
                Length = length
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
                FrameworkName = $"go.Slice<{typeInfo.FrameworkName}>",
                TypeClass = TypeClass.Slice
            };
        }

        public override void ExitChannelType(GolangParser.ChannelTypeContext context)
        {
            // TODO: Update to reference proper channel type name when added
            Types.TryGetValue(context.elementType().type(), out TypeInfo typeInfo);

            if (typeInfo == null)
                typeInfo = TypeInfo.ObjectType;

            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = typeInfo.Name,
                PrimitiveName = $"Channel<{typeInfo.PrimitiveName}>",
                FrameworkName = $"go.Channel<{typeInfo.FrameworkName}>",
                TypeClass = TypeClass.Channel
            };
        }

        public override void ExitInterfaceType(GolangParser.InterfaceTypeContext context)
        {
            if (context.methodSpec()?.Length == 0)
            {
                // Handle empty interface as a C# object, i.e., the type
                // that all classes inherit from - this is how an empty
                // interface is used in Go
                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = "object",
                    PrimitiveName = "object",
                    FrameworkName = "System.Object",
                    TypeClass = TypeClass.Simple
                };
            }
            else
            {
                // All other intra-function scoped declared interfaces
                // are defined as dynamic so they can behave like ducks
                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = "dynamic",
                    PrimitiveName = "dynamic",
                    FrameworkName = "System.Dynamic.DynamicObject",
                    TypeClass = TypeClass.Simple
                };
            }
        }

        public override void ExitStructType(GolangParser.StructTypeContext context)
        {
            // All intra-function scoped declared structures are
            // defined as dynamic so they can behave like ducks
            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = "dynamic",
                PrimitiveName = "dynamic",
                FrameworkName = "System.Dynamic.DynamicObject",
                TypeClass = TypeClass.Simple
            };
        }

        public override void ExitFunctionType(GolangParser.FunctionTypeContext context)
        {
            Signatures.TryGetValue(context.signature(), out Signature signature);

            string typeList = signature.GenerateParameterTypeList();
            string resultSignature = signature.GenerateResultSignature();
            string primitiveName, frameworkName;

            RequiredUsings.Add("System");

            if (resultSignature == "void")
            {
                if (string.IsNullOrEmpty(typeList))
                {
                    primitiveName = "Action";
                    frameworkName = "System.Action";
                }
                else
                {
                    primitiveName = $"Action<{typeList}>";
                    frameworkName = $"System.Action<{typeList}>";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(typeList))
                    typeList = $"{typeList}, ";

                primitiveName = $"Func<{typeList}{resultSignature}>";
                frameworkName = $"System.Func<{typeList}{resultSignature}>";
            }

            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = context.GetText(),
                PrimitiveName = primitiveName,
                FrameworkName = frameworkName,
                TypeClass = TypeClass.Function
            };
        }

        protected string ConvertToPrimitiveType(string type)
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
                case "string":
                    return "GoString";
                default:
                    return $"{type}";
            }
        }

        protected string ConvertToFrameworkType(string type)
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
                case "string":
                    return "go.GoString";
                default:
                    return $"{type}";
            }
        }
    }
}