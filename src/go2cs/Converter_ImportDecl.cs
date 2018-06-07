//******************************************************************************************************
//  Converter_ImportDecl.cs - Gbtc
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
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public override void EnterImportSpec(GolangParser.ImportSpecContext context)
        {
            // Base class parses package path
            base.EnterImportSpec(context);

            string alternateName = SanitizedIdentifier(context.IDENTIFIER()?.GetText());
            bool useStatic = (object)alternateName == null && context.ChildCount > 1 && context.GetChild(0).GetText().Equals(".");

            int lastSlash = PackagePath.LastIndexOf('/');
            string packageName = SanitizedIdentifier(lastSlash > -1 ? PackagePath.Substring(lastSlash + 1) : PackagePath);

            string targetUsing = $"{RootNamespace}.{string.Join(".", PackagePath.Split('/').Select(SanitizedIdentifier))}{ClassSuffix}";

            if (useStatic)
                m_targetFile.Append($"using static {targetUsing};");
            else if (alternateName?.Equals("_") ?? false)
                m_targetFile.Append($"using _{packageName}_ = {targetUsing};");
            else
                m_targetFile.Append($"using {alternateName ?? packageName} = {targetUsing};");

            m_targetFile.Append(CheckForCommentsRight(context));

            if (!m_wroteCommentWithLineFeed)
                m_targetFile.AppendLine();
        }
    }
}