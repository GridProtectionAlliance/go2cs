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

        public override void ExitType(GolangParser.TypeContext context)
        {
            if (context.type() != null)
            {
                if (Types.TryGetValue(context.type(), out TypeInfo typeInfo))
                    Types[context] = typeInfo;
                else
                    AddWarning(context, $"Failed to find sub-type in type expression \"{context.GetText()}\"");
            }
        }

        public override void EnterTypeName(GolangParser.TypeNameContext context)
        {
            string type = context.GetText();

            Types[context.Parent] = new TypeInfo
            {
                Name = type,
                TypeName = ConvertToPrimitiveType(type),
                FullTypeName = ConvertToFrameworkType(type),
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
                    TypeName = $"{prefix}{typeInfo.TypeName}{suffix}",
                    FullTypeName = $"{prefix}{typeInfo.FullTypeName}{suffix}",
                    IsPointer = true,
                    IsByRefPointer = false,
                    TypeClass = TypeClass.Simple
                };
            }
            else if (name.StartsWith("*(*") && name.EndsWith(")"))
            {
                typeInfo = ConvertByRefToNativePointer(typeInfo);

                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = name,
                    TypeName = $"*({typeInfo.TypeName})",
                    FullTypeName = $"*({typeInfo.FullTypeName})",
                    IsPointer = true,
                    IsByRefPointer = false,
                    TypeClass = TypeClass.Simple
                };

                UsesUnsafePointers = true;
            }
            else
            {
                Types[context.Parent.Parent] = new TypeInfo
                {
                    Name = name,
                    TypeName = $"ref {typeInfo.TypeName}",
                    FullTypeName = $"ref {typeInfo.FullTypeName}",
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

            ExpressionInfo length;

            if (Expressions.TryGetValue(context.arrayLength().expression(), out string expression))
            {
                // TODO: Remove once expressions dictionary holds expression info
                length = new ExpressionInfo
                {
                    Text = expression,
                    Type = new TypeInfo
                    {
                        TypeClass = TypeClass.Simple,
                        TypeName = "@int",
                        FullTypeName = "go.@int"
                    }
                };
            }
            else
            {
                length = new ExpressionInfo
                {
                    Text = "0",
                    Type = new TypeInfo
                    {
                        TypeClass = TypeClass.Simple,
                        TypeName = "@int",
                        FullTypeName = "go.@int"
                    }
                };

                AddWarning(context, $"Failed to find array length expression for \"{name}\"");
            }

            Types[context.Parent.Parent] = new ArrayTypeInfo
            {
                Name = name,
                TypeName = $"{typeInfo.TypeName}[]",
                FullTypeName = $"{typeInfo.FullTypeName}[]",
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
                TypeName = $"Dictionary<{keyTypeInfo.TypeName}, {elementTypeInfo.TypeName}>",
                FullTypeName = $"System.Collections.Generic.Dictionary<{keyTypeInfo.FullTypeName}, {elementTypeInfo.FullTypeName}>",
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
                TypeName = $"slice<{typeInfo.TypeName}>",
                FullTypeName = $"go.slice<{typeInfo.FullTypeName}>",
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
                TypeName = $"channel<{typeInfo.TypeName}>",
                FullTypeName = $"go.channel<{typeInfo.FullTypeName}>",
                TypeClass = TypeClass.Channel
            };
        }

        public override void ExitInterfaceType(GolangParser.InterfaceTypeContext context)
        {
            if (context.methodSpec()?.Length == 0)
            {
                // Handle empty interface type as a special case
                Types[context.Parent.Parent] = TypeInfo.ObjectType;

                // Object is more universal than EmptyInterface - which is handy when literals are involved
                //Types[context.Parent.Parent] = TypeInfo.EmptyInterfaceType;
            }
            else
            {
                // TODO: Turn into a strongly typed object and declare prior to function
                // All other intra-function scoped declared interfaces
                // are defined as dynamic so they can behave like ducks
                Types[context.Parent.Parent] = TypeInfo.DynamicType;
            }
        }

        public override void ExitStructType(GolangParser.StructTypeContext context)
        {
            // TODO: Turn into a strongly typed object and declare prior to function
            // All intra-function scoped declared structures are
            // defined as dynamic so they can behave like ducks
            Types[context.Parent.Parent] = TypeInfo.DynamicType;
        }

        public override void ExitFunctionType(GolangParser.FunctionTypeContext context)
        {
            Signatures.TryGetValue(context.signature(), out Signature signature);

            string typeList = signature.GenerateParameterTypeList();
            string resultSignature = signature.GenerateResultSignature();
            string typeName, fullTypeName;

            RequiredUsings.Add("System");

            if (resultSignature == "void")
            {
                if (string.IsNullOrEmpty(typeList))
                {
                    typeName = "Action";
                    fullTypeName = "System.Action";
                }
                else
                {
                    typeName = $"Action<{typeList}>";
                    fullTypeName = $"System.Action<{typeList}>";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(typeList))
                    typeList = $"{typeList}, ";

                typeName = $"Func<{typeList}{resultSignature}>";
                fullTypeName = $"System.Func<{typeList}{resultSignature}>";
            }

            Types[context.Parent.Parent] = new TypeInfo
            {
                Name = context.GetText(),
                TypeName = typeName,
                FullTypeName = fullTypeName,
                TypeClass = TypeClass.Function
            };
        }

        protected string ConvertToPrimitiveType(string type)
        {
            switch (type)
            {
                case "bool":
                    return "@bool";
                case "byte":
                    return "@byte";
                case "int":
                    return "@int";
                case "uint":
                    return "@uint";
                case "string":
                    return "@string";
                default:
                    return $"{type}";
            }
        }

        protected string ConvertToFrameworkType(string type)
        {
            switch (type)
            {
                case "bool":
                    return "go.@bool";
                case "int8":
                    return "go.int8";
                case "uint8":
                    return "go.uint8";
                case "byte":
                    return "go.@byte";
                case "int16":
                    return "go.int16";
                case "uint16":
                    return "go.uint16";
                case "int32":
                    return "go.int32";
                case "uint32":
                    return "go.uint32";
                case "int64":
                    return "go.int64";
                case "int":
                    return "go.@int";
                case "uint64":
                    return "go.uint64";
                case "uint":
                    return "go.@uint";
                case "float32":
                    return "go.float32";
                case "float64":
                    return "go.float64";
                case "rune":
                    return "go.rune";
                case "uintptr":
                    return "go.uintptr";
                case "complex64":
                    return "go.complex64";
                case "complex128":
                    return "go.complex128";
                case "string":
                    return "go.@string";
                default:
                    return $"{type}";
            }
        }
    }
}