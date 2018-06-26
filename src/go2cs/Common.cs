//******************************************************************************************************
//  Common.cs - Gbtc
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
//  05/17/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable UnusedMember.Global

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

#pragma warning disable SCS0006 // Weak hash
#pragma warning disable SCS0018 // Path traversal

namespace go2cs
{
    public static class Common
    {
        public const string RootNamespace = "go2cs";

        public static readonly Assembly EntryAssembly;

        public static string GoStandardLibraryProject { get; private set; }

        public static string GoUtilSharedProject { get; private set; }

        private static readonly HashSet<string> s_keywords;

        private static readonly CodeDomProvider s_provider;

        private static readonly CodeGeneratorOptions s_generatorOptions;

        private static readonly char[] s_dirVolChars;

        static Common()
        {
            EntryAssembly = Assembly.GetEntryAssembly();

            s_keywords = new HashSet<string>(new[]
            {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
                "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
                "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
                "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
                "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
                "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
                "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
            },
            StringComparer.Ordinal);

            s_provider = CodeDomProvider.CreateProvider("CSharp");
            s_generatorOptions = new CodeGeneratorOptions { IndentString = "    " };
            s_dirVolChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };
        }

        public static void RestoreGoUtilSources(string targetPath)
        {
            const string prefix = RootNamespace + ".goutil.";

            if (!targetPath.EndsWith("goutil"))
                targetPath = Path.Combine(targetPath, "goutil");

            targetPath = AddPathSuffix(targetPath);

            foreach (string name in EntryAssembly.GetManifestResourceNames().Where(name => name.StartsWith(prefix)))
            {
                using (Stream resourceStream = EntryAssembly.GetManifestResourceStream(name))
                {
                    if ((object)resourceStream != null)
                    {
                        string targetFileName = Path.Combine(targetPath, name.Substring(prefix.Length));
                        bool restoreFile = true;

                        if (File.Exists(targetFileName))
                        {
                            string resourceMD5 = GetMD5HashFromStream(resourceStream);
                            resourceStream.Seek(0, SeekOrigin.Begin);
                            restoreFile = !resourceMD5.Equals(GetMD5HashFromFile(targetFileName));
                        }

                        if (restoreFile)
                        {
                            byte[] buffer = new byte[resourceStream.Length];
                            resourceStream.Read(buffer, 0, (int)resourceStream.Length);

                            string directory = Path.GetDirectoryName(targetFileName);

                            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                                Directory.CreateDirectory(directory);

                            using (StreamWriter writer = File.CreateText(targetFileName))
                                writer.Write(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                        }

                        if (targetFileName.EndsWith(".projitems"))
                            GoUtilSharedProject = targetFileName;
                    }
                }
            }

            GoStandardLibraryProject = Path.Combine(targetPath, $"{Converter.StandardLibrary}.projitems");
        }

        // Use this function to preserve existing shared project Guid when overwriting
        public static string GetProjectGuid(string projectFileName, string tagName)
        {
            if (File.Exists(projectFileName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(projectFileName);

                XmlNodeList guidList = xmlDoc.GetElementsByTagName(tagName);

                if (guidList.Count > 0)
                    return guidList[0].InnerText;
            }

            return Guid.NewGuid().ToString();
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return GetMD5HashFromStream(stream);
        }

        public static string GetMD5HashFromString(string source)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(source)))
                return GetMD5HashFromStream(stream);
        }

        public static string GetMD5HashFromStream(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }

        public static string AddPathSuffix(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.DirectorySeparatorChar.ToString();
            }
            else
            {
                char suffixChar = filePath[filePath.Length - 1];

                if (suffixChar != Path.DirectorySeparatorChar && suffixChar != Path.AltDirectorySeparatorChar)
                    filePath += Path.DirectorySeparatorChar;
            }

            return filePath;
        }

        public static string RemovePathSuffix(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "";
            }
            else
            {
                char suffixChar = filePath[filePath.Length - 1];

                while ((suffixChar == Path.DirectorySeparatorChar || suffixChar == Path.AltDirectorySeparatorChar) && filePath.Length > 0)
                {
                    filePath = filePath.Substring(0, filePath.Length - 1);

                    if (filePath.Length > 0)
                        suffixChar = filePath[filePath.Length - 1];
                }
            }

            return filePath;
        }

        public static string GetDirectoryName(string filePath)
        {
            // Test for case where valid path does not end in directory separator, Path.GetDirectoryName assumes
            // this is a file name - whether is exists or not
            string directoryName = AddPathSuffix(filePath);

            if (Directory.Exists(directoryName))
                return directoryName;

            return AddPathSuffix(Path.GetDirectoryName(filePath) ?? filePath);
        }

        public static string GetLastDirectoryName(string filePath)
        {
            if ((object)filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            int index;

            // Remove file name and trailing directory separator character from the file path
            filePath = RemovePathSuffix(GetDirectoryName(filePath));

            // Keep going through the file path until all directory separator characters are removed
            while ((index = filePath.IndexOfAny(s_dirVolChars)) > -1)
                filePath = filePath.Substring(index + 1);

            return filePath;
        }

        public static bool PathHasFiles(string filePath, string searchPattern)
        {
            return Directory.Exists(filePath) && Directory.EnumerateFiles(filePath, searchPattern, SearchOption.TopDirectoryOnly).Any();
        }

        public static string RemoveInvalidCharacters(string fileName)
        {
            return fileName.Replace('<', '(').Replace('>', ')');
        }

        public static string GetRelativePath(string fileName, string targetPath)
        {
            return new DirectoryInfo(targetPath).GetRelativePathTo(new FileInfo(fileName));
        }

        public static string GetRelativePathFrom(this FileSystemInfo to, FileSystemInfo from)
        {
            return from.GetRelativePathTo(to);
        }

        public static string GetRelativePathTo(this FileSystemInfo from, FileSystemInfo to)
        {
            string getPath(FileSystemInfo fsi) => !(fsi is DirectoryInfo d) ? fsi.FullName : AddPathSuffix(d.FullName);

            string fromPath = getPath(from);
            string toPath = getPath(to);

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string RemoveSurrounding(string source, string left = "\"", string right = "\"")
        {
            if (string.IsNullOrEmpty(source))
                return source;

            if (source.StartsWith(left) && source.EndsWith(right))
            {
                if (source.Length > left.Length + right.Length)
                    return source.Substring(left.Length, source.Length - (left.Length + right.Length));

                return "";
            }

            return source;
        }

        public static string SanitizedIdentifier(string identifier) => s_keywords.Contains(identifier) ? $"@{identifier}" : identifier;

        public static string ToStringLiteral(string input)
        {
            input = RemoveSurrounding(RemoveSurrounding(input ?? ""), "`", "`");

            using (StringWriter writer = new StringWriter())
            {
                s_provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, s_generatorOptions);
                return writer.ToString();
            }
        }
    }
}
