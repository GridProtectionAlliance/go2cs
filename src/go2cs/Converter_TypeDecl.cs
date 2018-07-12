//******************************************************************************************************
//  Converter_TypeDecl.cs - Gbtc
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
using go2cs.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static go2cs.Common;

#pragma warning disable SCS0018 // Path traversal

namespace go2cs
{
    public partial class Converter
    {
        private bool m_firstTypeSpec;

        // TODO: Consider strongly typing sub-function type declarations with function name prefix declared directly prior to function definition - sub-function type declarations are syntactically invalid in C#

        public override void EnterTypeDecl(GolangParser.TypeDeclContext context)
        {
            m_firstTypeSpec = !m_inFunction;
        }

        public override void ExitTypeSpec(GolangParser.TypeSpecContext context)
        {
            // typeSpec
            //     : IDENTIFIER type

            if (m_firstTypeSpec)
            {
                m_firstTypeSpec = false;

                string comments = CheckForCommentsLeft(context);

                if (!string.IsNullOrEmpty(comments))
                    m_targetFile.Append(FixForwardSpacing(comments));
            }

            string originalIdentifier = context.IDENTIFIER().GetText();
            string scope = char.IsUpper(originalIdentifier[0]) ? "public" : "private";
            string target = Path.GetFileNameWithoutExtension(TargetFileName);
            string identifier = SanitizedIdentifier(originalIdentifier);

            // TODO: Sub-function strategy, declare directly prior to function using PushBlock / PopBlock operations and a new replacement marker
            if (m_inFunction)
                AddWarning(context, $"Type specification made from within function \"{m_currentFunctionName}\" - this is will not compile in C#");

            if (Metadata.Interfaces.TryGetValue(originalIdentifier, out InterfaceInfo interfaceInfo))
            {
                // Handle interface type declaration
                string ancillaryInterfaceFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Interface.cs");

                FunctionSignature[] localFunctions = interfaceInfo.GetLocalMethods().ToArray();
                List<FunctionSignature> allFunctions = localFunctions.ToList();

                RecurseInheritedInterfaces(context, originalIdentifier, interfaceInfo, allFunctions);

                using (StreamWriter writer = File.CreateText(ancillaryInterfaceFileName))
                {
                    writer.Write(new InterfaceTypeTemplate
                    {
                        NamespacePrefix = PackageNamespace,
                        NamespaceHeader = m_namespaceHeader,
                        NamespaceFooter = m_namespaceFooter,
                        PackageName = Package,
                        InterfaceName = identifier,
                        Scope = scope,
                        Interface = interfaceInfo,
                        Functions = allFunctions,
                        UsingStatements = m_usingStatements
                    }
                    .TransformText());
                }

                // Track file name associated with package
                AddFileToPackage(Package, ancillaryInterfaceFileName, PackageNamespace);

                string inheritedInterfaces = interfaceInfo.GenerateInheritedInterfaceList();

                if (inheritedInterfaces.Length > 0)
                    inheritedInterfaces = $" : {inheritedInterfaces}";

                m_targetFile.AppendLine($"{Spacing()}{scope} interface {identifier}{inheritedInterfaces}");
                m_targetFile.AppendLine($"{Spacing()}{{");

                foreach (FunctionSignature function in localFunctions)
                {
                    m_targetFile.Append($"{Spacing(1)}{function.Signature.GenerateResultSignature()} {SanitizedIdentifier(function.Name)}({function.Signature.GenerateParametersSignature(false)});{(string.IsNullOrEmpty(function.Comments) ? Environment.NewLine : function.Comments)}");
                }

                m_targetFile.AppendLine($"{Spacing()}}}");
                m_targetFile.AppendLine();

                string comments = CheckForCommentsRight(context);

                if (!string.IsNullOrEmpty(comments))
                    m_targetFile.Append(FixForwardSpacing(comments));
            }
            else if (Metadata.Structs.TryGetValue(originalIdentifier, out StructInfo structInfo))
            {
                // Handle struct type declaration
                string ancillaryStructFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Struct.cs");

                Dictionary<string, List<FunctionSignature>> promotedFunctions = new Dictionary<string, List<FunctionSignature>>(StringComparer.Ordinal);
                HashSet<string> inheritedTypeNames = new HashSet<string>(StringComparer.Ordinal);

                RecurseInheritedInterfaces(context, originalIdentifier, structInfo, promotedFunctions, inheritedTypeNames);

                Dictionary<string, List<FieldInfo>> promotedFields = new Dictionary<string, List<FieldInfo>>(StringComparer.Ordinal);
                HashSet<string> promotedStructs = new HashSet<string>(StringComparer.Ordinal);

                SearchPromotedStructFields(context, originalIdentifier, structInfo, inheritedTypeNames, promotedFields, promotedStructs);

                using (StreamWriter writer = File.CreateText(ancillaryStructFileName))
                {
                    writer.Write(new StructTypeTemplate
                    {
                        NamespacePrefix = PackageNamespace,
                        NamespaceHeader = m_namespaceHeader,
                        NamespaceFooter = m_namespaceFooter,
                        PackageName = Package,
                        StructName = identifier,
                        Scope = scope,
                        StructFields = structInfo.Fields,
                        PromotedStructs = promotedStructs,
                        PromotedFunctions = promotedFunctions,
                        PromotedFields = promotedFields,
                        UsingStatements = m_usingStatements
                    }
                    .TransformText());
                }

                // Track file name associated with package
                AddFileToPackage(Package, ancillaryStructFileName, PackageNamespace);

                List<FieldInfo> fields = new List<FieldInfo>();

                foreach (FieldInfo field in structInfo.Fields)
                {
                    if (field.IsPromoted && !inheritedTypeNames.Contains(field.Name))
                    {
                        FieldInfo promotedStruct = field.Clone();

                        promotedStruct.Type.PrimitiveName = $"ref {promotedStruct.Type.Name}";
                        promotedStruct.Name = $"{promotedStruct.Name} => ref {promotedStruct.Name}_{(promotedStruct.Type.IsPointer ? "ptr" : "val")}";

                        fields.Add(promotedStruct);
                    }
                    else
                    {
                        fields.Add(field);
                    }
                }

                m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier}{GetInheritedTypeList(inheritedTypeNames)}");
                m_targetFile.AppendLine($"{Spacing()}{{");

                foreach (FieldInfo field in fields)
                {
                    StringBuilder fieldDecl = new StringBuilder();

                    if (!string.IsNullOrWhiteSpace(field.Description))
                    {
                        string description = field.Description.Trim();

                        if (description.Length > 2)
                        {
                            RequiredUsings.Add("System.ComponentModel");
                            fieldDecl.AppendLine($"{Spacing(1)}[Description({description})]");
                        }
                    }

                    fieldDecl.Append($"{Spacing(1)}public {field.Type.PrimitiveName} {field.Name};{(string.IsNullOrEmpty(field.Comments) ? Environment.NewLine : field.Comments)}");
                    m_targetFile.Append(fieldDecl);
                }

                m_targetFile.AppendLine($"{Spacing()}}}");
                m_targetFile.AppendLine();

                string comments = CheckForCommentsRight(context);

                if (!string.IsNullOrEmpty(comments))
                    m_targetFile.Append(FixForwardSpacing(comments));
            }
            else if (Types.TryGetValue(context.type(), out TypeInfo typeInfo))
            {
                if (typeInfo.TypeClass == TypeClass.Function)
                {
                    RequiredUsings.Decrement("System");

                    // Handle delegate type declaration
                    string signature = typeInfo.PrimitiveName;

                    if (signature.Equals("Action", StringComparison.Ordinal))
                    {
                        m_targetFile.Append($"{Spacing()}public delegate void {identifier}();");

                        string comments = CheckForCommentsRight(context);

                        if (!string.IsNullOrEmpty(comments))
                            m_targetFile.Append(FixForwardSpacing(comments));

                        m_targetFile.AppendLine();
                    }
                    else if (signature.StartsWith("Action<", StringComparison.Ordinal))
                    {
                        signature = RemoveSurrounding(signature.Substring(6), "<", ">");
                        m_targetFile.Append($"{Spacing()}public delegate void {identifier}({signature});");

                        string comments = CheckForCommentsRight(context);

                        if (!string.IsNullOrEmpty(comments))
                            m_targetFile.Append(FixForwardSpacing(comments));

                        m_targetFile.AppendLine();
                    }
                    else if (signature.StartsWith("Func<", StringComparison.Ordinal))
                    {
                        signature = RemoveSurrounding(signature.Substring(4), "<", ">");
                        string[] parts = signature.Split(',');

                        if (parts.Length > 0)
                        {
                            string result = parts[parts.Length - 1];
                            signature = string.Join(", ", parts.Take(parts.Length - 1));

                            m_targetFile.Append($"{Spacing()}public delegate {result} {identifier}({signature});");

                            string comments = CheckForCommentsRight(context);

                            if (!string.IsNullOrEmpty(comments))
                                m_targetFile.Append(FixForwardSpacing(comments));

                            m_targetFile.AppendLine();
                        }
                        else
                        {
                            AddWarning(context, $"Could not determine function based delegate signature from \"{signature}\"");
                        }
                    }
                    else
                    {
                        AddWarning(context, $"Could not determine delegate signature from \"{signature}\"");
                    }
                }
                else
                {
                    // Handle named type declaration, e.g., "type MyFloat float64"
                    string ancillaryInheritedTypeFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}StructOf({RemoveInvalidCharacters(typeInfo.PrimitiveName)}).cs");

                    // TODO: The following works OK for a primitive type re-definition, but new templates will be needed for other inherited types, e.g., Map / Pointer / Array etc.
                    using (StreamWriter writer = File.CreateText(ancillaryInheritedTypeFileName))
                    {
                        writer.Write(new InheritedTypeTemplate
                        {
                            NamespacePrefix = PackageNamespace,
                            NamespaceHeader = m_namespaceHeader,
                            NamespaceFooter = m_namespaceFooter,
                            PackageName = Package,
                            StructName = identifier,
                            Scope = scope,
                            TypeName = typeInfo.PrimitiveName
                        }
                        .TransformText());
                    }

                    // Track file name associated with package
                    AddFileToPackage(Package, ancillaryInheritedTypeFileName, PackageNamespace);

                    m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier} // : {typeInfo.PrimitiveName}");
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    m_targetFile.AppendLine($"{Spacing()}}}");
                }
            }
        }

        private void RecurseInheritedInterfaces(GolangParser.TypeSpecContext context, string identifier, InterfaceInfo interfaceInfo, List<FunctionSignature> functions, List<string> inheritedTypeNames = null, bool useFullTypeName = false)
        {
            foreach (string interfaceName in interfaceInfo.GetInheritedInterfaceNames())
            {
                if (TryFindInheritedInterfaceInfo(interfaceName, out InterfaceInfo inheritedInferfaceInfo, out string shortTypeName, out string fullTypeName))
                {
                    inheritedTypeNames?.Add(useFullTypeName ? fullTypeName : shortTypeName);
                    functions.AddRange(inheritedInferfaceInfo.GetLocalMethods());
                    RecurseInheritedInterfaces(context, identifier, inheritedInferfaceInfo, functions);
                }
                else
                {
                    AddWarning(context, $"Failed to find metadata for promoted interface \"{interfaceName}\" declared in \"{identifier}\" interface type.");
                }
            }
        }

        private void RecurseInheritedInterfaces(GolangParser.TypeSpecContext context, string identifier, StructInfo structInfo, Dictionary<string, List<FunctionSignature>> fieldFunctions, HashSet<string> inheritedTypeNames = null, bool useFullTypeName = false)
        {
            foreach (FieldInfo field in structInfo.GetAnonymousFields())
            {
                string interfaceName = field.Type.PrimitiveName;

                if (TryFindInheritedInterfaceInfo(interfaceName, out InterfaceInfo inheritedInferfaceInfo, out string shortTypeName, out string fullTypeName))
                {
                    inheritedTypeNames?.Add(useFullTypeName ? fullTypeName : shortTypeName);
                    List<FunctionSignature> functions = fieldFunctions.GetOrAdd(field.Name, name => new List<FunctionSignature>());
                    functions.AddRange(inheritedInferfaceInfo.GetLocalMethods());
                    RecurseInheritedInterfaces(context, identifier, inheritedInferfaceInfo, functions);
                }
            }
        }

        private bool TryFindInheritedInterfaceInfo(string interfaceName, out InterfaceInfo interfaceInfo, out string shortTypeName, out string fullTypeName)
        {
            interfaceInfo = default;
            shortTypeName = SanitizedIdentifier(interfaceName);
            fullTypeName = default;

            // Handle built-in error interface as a special case
            if (interfaceName.Equals("error", StringComparison.Ordinal))
            {
                interfaceInfo = InterfaceInfo.error();
                fullTypeName = "go.BuiltInFunctions.error";
                return true;
            }

            if (Metadata.Interfaces.TryGetValue(interfaceName, out interfaceInfo))
            {
                fullTypeName = $"{GetPackageNamespace(PackageImport)}.{shortTypeName}";
                return true;
            }

            string[] qualifiedIdentifierParts = interfaceName.Split('.');

            if (qualifiedIdentifierParts.Length == 2)
            {
                string alias = qualifiedIdentifierParts[0];
                string identifier = qualifiedIdentifierParts[1];

                if (ImportAliases.TryGetValue(alias, out (string targetImport, string targetUsing) import))
                {
                    if (ImportMetadata.TryGetValue(import.targetImport, out FolderMetadata folderMetadata))
                    {
                        if (TryFindIntefaceInfo(folderMetadata, identifier, out FileMetadata fileMetadata, out interfaceInfo))
                        {
                            fullTypeName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{SanitizedIdentifier(identifier)}";
                            return true;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, FolderMetadata> importMetadata in ImportMetadata)
            {
                if (TryFindIntefaceInfo(importMetadata.Value, interfaceName, out FileMetadata fileMetadata, out interfaceInfo))
                {
                    fullTypeName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{shortTypeName}";
                    return true;
                }
            }

            return false;
        }

        private bool TryFindIntefaceInfo(FolderMetadata folderMetadata, string interfaceName, out FileMetadata interfaceFileMetadata, out InterfaceInfo interfaceInfo)
        {
            interfaceInfo = default;
            interfaceFileMetadata = null;

            foreach (FileMetadata fileMetadata in folderMetadata.Files.Values)
            {
                if (fileMetadata.Interfaces.TryGetValue(interfaceName, out interfaceInfo))
                {
                    interfaceFileMetadata = fileMetadata;
                    break;
                }
            }

            return interfaceFileMetadata != null;
        }

        private void SearchPromotedStructFields(GolangParser.TypeSpecContext context, string identifier, StructInfo structInfo, HashSet<string> inheritedTypeNames, Dictionary<string, List<FieldInfo>> promotedFields, HashSet<string> promotedStructTypeNames = null, bool useFullTypeName = false)
        {
            foreach (FieldInfo field in structInfo.GetAnonymousFields())
            {
                // If type is a known interface, skip ahead
                if (inheritedTypeNames.Contains(field.Name))
                    continue;

                string structName = field.Type.PrimitiveName;

                if (TryFindPromotedStructInfo(structName, out StructInfo promotedStructInfo, out string shortTypeName, out string fullTypeName))
                {
                    promotedStructTypeNames?.Add(useFullTypeName ? fullTypeName : shortTypeName);
                    List<FieldInfo> fields = promotedFields.GetOrAdd(field.Name, name => new List<FieldInfo>());
                    fields.AddRange(promotedStructInfo.GetLocalFields());                   
                }
                else
                {
                    AddWarning(context, $"Failed to find metadata for promoted struct or interface \"{structName}\" declared in \"{identifier}\" struct type.");
                }
            }
        }

        private bool TryFindPromotedStructInfo(string structName, out StructInfo structInfo, out string shortTypeName, out string fullTypeName)
        {
            structInfo = default;
            shortTypeName = SanitizedIdentifier(structName);
            fullTypeName = default;

            if (Metadata.Structs.TryGetValue(structName, out structInfo))
            {
                fullTypeName = $"{GetPackageNamespace(PackageImport)}.{shortTypeName}";
                return true;
            }

            string[] qualifiedIdentifierParts = structName.Split('.');

            if (qualifiedIdentifierParts.Length == 2)
            {
                string alias = qualifiedIdentifierParts[0];
                string identifier = qualifiedIdentifierParts[1];

                if (ImportAliases.TryGetValue(alias, out (string targetImport, string targetUsing) import))
                {
                    if (ImportMetadata.TryGetValue(import.targetImport, out FolderMetadata folderMetadata))
                    {
                        if (TryFindStructInfo(folderMetadata, identifier, out FileMetadata fileMetadata, out structInfo))
                        {
                            fullTypeName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{SanitizedIdentifier(identifier)}";
                            return true;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, FolderMetadata> importMetadata in ImportMetadata)
            {
                if (TryFindStructInfo(importMetadata.Value, structName, out FileMetadata fileMetadata, out structInfo))
                {
                    fullTypeName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{shortTypeName}";
                    return true;
                }
            }

            return false;
        }

        private bool TryFindStructInfo(FolderMetadata folderMetadata, string structName, out FileMetadata structFileMetadata, out StructInfo structInfo)
        {
            structInfo = default;
            structFileMetadata = null;

            foreach (FileMetadata fileMetadata in folderMetadata.Files.Values)
            {
                if (fileMetadata.Structs.TryGetValue(structName, out structInfo))
                {
                    structFileMetadata = fileMetadata;
                    break;
                }
            }

            return structFileMetadata != null;
        }

        private string GetInheritedTypeList(HashSet<string> inheritedTypeNames)
        {
            if ((object)inheritedTypeNames == null || inheritedTypeNames.Count == 0)
                return "";

            return $" : {string.Join(", ", inheritedTypeNames)}";
        }
    }
}
