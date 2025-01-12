//******************************************************************************************************
//  Arguments.cs - Gbtc
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

using CommandLine;

namespace go2cs;

public class Arguments
{
    private readonly ParserResult<Options> m_parserResult;

    private Arguments(ParserResult<Options> parserResult) => m_parserResult = parserResult;

    public Options ParsedOptions => (m_parserResult as Parsed<Options>)?.Value;

    public bool ParseSuccess => m_parserResult.Tag == ParserResultType.Parsed;

    public static Arguments Parse(string[] args) => new(Parser.Default.ParseArguments<Options>(args));
}