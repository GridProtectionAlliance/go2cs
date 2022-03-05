//******************************************************************************************************
//  ScannerBase_Signature.cs - Gbtc
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
using System.Collections.Generic;

namespace go2cs;

public partial class ScannerBase
{
    // Stack handlers:
    //  functionDecl (optional)
    //  function (required)
    //  methodDecl (optional)
    //  methodSpec (optional)
    //  functionType (required)
    protected readonly ParseTreeValues<Signature> Signatures = new ParseTreeValues<Signature>();
    protected List<ParameterInfo> Result;

    public override void EnterSignature(GoParser.SignatureContext context)
    {
        Result = new(new[]
        {
            new ParameterInfo
            {
                Name = string.Empty,
                Type = TypeInfo.VoidType,
                IsVariadic = false
            }
        });
    }

    public override void ExitSignature(GoParser.SignatureContext context)
    {
        Parameters.TryGetValue(context.parameters(), out List<ParameterInfo> parameters);
        Signatures[context] = new()
        {
            Parameters = parameters?.ToArray() ?? System.Array.Empty<ParameterInfo>(),
            Result = Result?.ToArray() ?? System.Array.Empty<ParameterInfo>()
        };
    }

    public override void ExitResult(GoParser.ResultContext context)
    {
        //result
        //  : parameters
        //  | type
        if (!Parameters.TryGetValue(context.parameters(), out Result))
        {
            if (Types.TryGetValue(context.type_(), out TypeInfo typeInfo))
            {
                Result = new(new[] { new ParameterInfo
                {
                    Name = string.Empty,
                    Type = typeInfo,
                    IsVariadic = false
                }});
            }
        }
    }
}
