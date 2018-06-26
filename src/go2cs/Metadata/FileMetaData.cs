//******************************************************************************************************
//  FileInfo.cs - Gbtc
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
//  06/18/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace go2cs.Metadata
{
    [Serializable]
    public class FileMetadata
    {
        public string Package;
        public string PackageImport;
        public string SourceFileName;
        public string TargetFileName;
        public Dictionary<string, (string targetImport, string targetUsing)> ImportAliases;
        public Dictionary<string, InterfaceInfo> Interfaces;
        public Dictionary<string, StructInfo> Structs;
        public Dictionary<string, FunctionInfo> Functions;
        public DateTime LastUpdate;
    }
}
