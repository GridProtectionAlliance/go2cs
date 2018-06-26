//******************************************************************************************************
//  Converter_TypeSpec.cs - Gbtc
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
using static go2cs.Common;

#pragma warning disable SCS0018 // Path traversal

namespace go2cs
{
    public partial class Converter
    {
        public override void ExitTypeSpec(GolangParser.TypeSpecContext context)
        {
            string originalIdentifier = context.IDENTIFIER().GetText();
            string scope = char.IsUpper(originalIdentifier[0]) ? "public" : "private";
            string target = Path.GetFileNameWithoutExtension(TargetFileName);
            string identifier = SanitizedIdentifier(originalIdentifier);

            if (!m_firstTopLevelDeclaration)
                m_targetFile.AppendLine();

            if (Metadata.Interfaces.TryGetValue(originalIdentifier, out InterfaceInfo interfaceInfo))
            {
                string ancillaryInterfaceFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Interface.cs");

                FunctionSignature[] localMethods = interfaceInfo.GetLocalMethods().ToArray();
                List<FunctionSignature> allMethods = localMethods.ToList();
                List<string> inheritedTypeNames = new List<string>();

                RecurseInheritedInterfaces(context, originalIdentifier, interfaceInfo, allMethods, inheritedTypeNames);

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
                            InheritedTypeNames = inheritedTypeNames,
                            Functions = allMethods
                        }
                        .TransformText());
                }

                // Track file name associated with package
                AddFileToPackage(Package, ancillaryInterfaceFileName, PackageNamespace);

                string inheritedInterfaces = interfaceInfo.GenerateInheritedInterfaceList();

                if (inheritedInterfaces.Length > 0)
                    inheritedInterfaces = $" : {inheritedInterfaces}";

                m_targetFile.AppendLine($"{Spacing()}{scope} partial interface {identifier}{inheritedInterfaces}");
                m_targetFile.AppendLine($"{Spacing()}{{");

                foreach (FunctionSignature function in localMethods)
                {
                    m_targetFile.AppendLine($"{Spacing(1)}{function.Signature.GenerateResultSignature()} {SanitizedIdentifier(function.Name)}({function.Signature.GenerateParametersSignature(false)});");
                }

                m_targetFile.AppendLine($"{Spacing()}}}");
            }

            //if (m_interfaceMethods.TryGetValue(context.type()?.typeLit()?.interfaceType(), out List<(string functionName, string parameterSignature, string namedParameters, string parameterTypes, string resultType)> functions))
            //{

            //}
            //else if (m_structFields.TryGetValue(context.type()?.typeLit()?.structType(), out string fields))
            //{

            //string getStructureField((string fieldName, string fieldType, string description, string comment) field)
            //{
            //    StringBuilder fieldDecl = new StringBuilder();

            //    if (!string.IsNullOrWhiteSpace(field.description))
            //    {
            //        string description = field.description.Trim();

            //        if (description.Length > 2)
            //        {
            //            RequiredUsings.Add("System.ComponentModel");
            //            fieldDecl.AppendLine($"{Spacing(1)}[Description({description})]");
            //        }
            //    }

            //    fieldDecl.Append($"{Spacing(1)}public {field.fieldType} {field.fieldName};");

            //    if (!string.IsNullOrEmpty(field.comment))
            //        fieldDecl.Append(field.comment);

            //    return fieldDecl.ToString();
            //}

            //m_structFields[context] = string.Join(Environment.NewLine, fields.Select(getStructureField));

            //    // Handle struct type declaration
            //    string ancillaryStructFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Struct.cs");

            //    using (StreamWriter writer = File.CreateText(ancillaryStructFileName))
            //        writer.Write(new StructTypeTemplate
            //        {
            //            NamespacePrefix = PackageNamespace,
            //            NamespaceHeader = m_namespaceHeader,
            //            NamespaceFooter = m_namespaceFooter,
            //            PackageName = Package,
            //            StructName = identifier,
            //            Scope = scope,
            //            InheritedInterfaces = new[] { "" }, // TODO <<
            //            PromotedStructs = new[] { "" }     // TODO <<
            //        }
            //        .TransformText());

            //    // Track file name associated with package
            //    AddFileToPackage(Package, ancillaryStructFileName, PackageNamespace);

            //    m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier}");
            //    m_targetFile.AppendLine($"{Spacing()}{{");
            //    m_targetFile.AppendLine(fields);
            //    m_targetFile.AppendLine($"{Spacing()}}}");
            //}
            //else if (m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo))
            //{
            //    // Handle declaration like "type MyFloat float64"
            //    string ancillaryInheritedTypeFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}StructOf({RemoveInvalidCharacters(typeInfo.PrimitiveName)}).cs");

            //    // TODO: The following works well for a primitive type definition, but new templates will be needed for other inherited types, e.g., Map / Pointer / Array etc.
            //    using (StreamWriter writer = File.CreateText(ancillaryInheritedTypeFileName))
            //        writer.Write(new InheritedTypeTemplate
            //        {
            //            NamespacePrefix = PackageNamespace,
            //            NamespaceHeader = m_namespaceHeader,
            //            NamespaceFooter = m_namespaceFooter,
            //            PackageName = Package,
            //            StructName = identifier,
            //            Scope = scope,
            //            TypeName = typeInfo.PrimitiveName
            //        }
            //        .TransformText());

            //    // Track file name associated with package
            //    AddFileToPackage(Package, ancillaryInheritedTypeFileName, PackageNamespace);

            //    m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier} // {typeInfo.PrimitiveName}");
            //    m_targetFile.AppendLine($"{Spacing()}{{");
            //    m_targetFile.AppendLine($"{Spacing()}}}");
            //}
        }
        private void RecurseInheritedInterfaces(GolangParser.TypeSpecContext context, string identifier, InterfaceInfo interfaceInfo, List<FunctionSignature> functions, List<string> inheritedTypeNames)
        {
            foreach (string interfaceName in interfaceInfo.GetInheritedInterfaceNames())
            {
                if (TryFindInheritedInterfaceInfo(interfaceName, out InterfaceInfo inheritedInferfaceInfo, out string fullName))
                {
                    inheritedTypeNames?.Add(fullName);
                    functions.AddRange(inheritedInferfaceInfo.GetLocalMethods());
                    RecurseInheritedInterfaces(context, identifier, inheritedInferfaceInfo, functions, null);
                }
                else
                {
                    AddWarning(context, $"Failed to find metadata for promoted interface \"{interfaceName}\" declared in \"{identifier}\" interface type.");
                }
            }
        }

        private bool TryFindInheritedInterfaceInfo(string interfaceName, out InterfaceInfo interfaceInfo, out string fullName)
        {
            interfaceInfo = default;
            fullName = default;

            // Handle built-in error interface as a special case
            if (interfaceName.Equals("error", StringComparison.Ordinal))
            {
                interfaceInfo = InterfaceInfo.error();
                fullName = "go.BuiltInFunctions.error";
                return true;
            }

            if (Metadata.Interfaces.TryGetValue(interfaceName, out interfaceInfo))
            {
                fullName = $"{GetPackageNamespace(PackageImport)}.{SanitizedIdentifier(interfaceName)}";
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
                            fullName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{SanitizedIdentifier(identifier)}";
                            return true;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, FolderMetadata> importMetadata in ImportMetadata)
            {
                if (TryFindIntefaceInfo(importMetadata.Value, interfaceName, out FileMetadata fileMetadata, out interfaceInfo))
                {
                    fullName = $"{GetPackageNamespace(fileMetadata.PackageImport)}.{SanitizedIdentifier(interfaceName)}";
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
    }
}
