//******************************************************************************************************
//  PreScanner_ImportDecl.cs - Gbtc
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

using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class PreScanner
    {
        public override void EnterImportSpec(GoParser.ImportSpecContext context)
        {
            // Base class parses current import package path
            base.EnterImportSpec(context);

            string alternateName = SanitizedIdentifier(context.IDENTIFIER()?.GetText());
            bool useStatic = (object)alternateName == null && context.ChildCount > 1 && context.GetChild(0).GetText().Equals(".");

            int lastSlash = CurrentImportPath.LastIndexOf('/');
            string packageName = SanitizedIdentifier(lastSlash > -1 ? CurrentImportPath.Substring(lastSlash + 1) : CurrentImportPath);

            string targetUsing = $"{RootNamespace}.{string.Join(".", CurrentImportPath.Split('/').Select(SanitizedIdentifier))}{ClassSuffix}";

            string alias;

            if (useStatic)
                alias = $"static {targetUsing}";
            else if (alternateName?.Equals("_") ?? false)
                alias = $"_{packageName}_";
            else
                alias = alternateName ?? packageName;

            m_importAliases[alias] = (CurrentImportPath, targetUsing);
        }
    }
}