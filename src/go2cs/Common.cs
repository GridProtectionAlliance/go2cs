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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace go2cs
{
    public static class Common
    {
        public const string RootNamespace = "go2cs";

        public static readonly Assembly EntryAssembly;

        public static string GoUtilSharedProject { get; private set; }

        static Common()
        {
            EntryAssembly = Assembly.GetEntryAssembly();
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
        }

        // Use this function to preserve existing shared project Guid when overwriting
        public static string GetSharedProjectGuid(string sharedProjectFileName)
        {
            if (File.Exists(sharedProjectFileName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(sharedProjectFileName);

                XmlNodeList sharedGuidList = xmlDoc.GetElementsByTagName("SharedGUID");

                if (sharedGuidList.Count > 0)
                    return sharedGuidList[0].InnerText;
            }

            return Guid.NewGuid().ToString();
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return GetMD5HashFromStream(stream);
        }

        private static string GetMD5HashFromStream(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
