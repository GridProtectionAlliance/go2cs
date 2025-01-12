//******************************************************************************************************
//  ExpressionInfo.cs - Gbtc
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
//  07/12/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace go2cs.Metadata
{
    // expression is fundamentally one of:
    //     unaryExpr
    //     expression operator expression
    //     primaryExpr

    // For unaryExpr, result type matches sub-expression

    // For expression operator expression
    //     if operator is comparison, result type is boolean
    //     otherwise, result type matches sub-expression

    // primaryExpr is more complex, it breaks down to one of:
    //     operand
    //     conversion
    //     primaryExpr operation

    // When primaryExpr is operand, result types are as follows:
    //     literal, result is literal type
    //     operandName, result is operand type
    //     methodExpr, result is a method expression
    //     (expression), result type matched sub-expression

    // When primaryExpr is conversion,
    //     result type is specified target type

    // When primary is expression has an operation, options are:
    //     DOT IDENIFIER, result type matches identifier
    //     index, result type matches sub-primaryExpr
    //     slice, result type matches sub-primaryExpr
    //     typeAssersion, result type matches specified target type
    //     arguments, result type matches sub-primaryExpr

    [Serializable]
    public class ExpressionInfo
    {
        public string Text;
        public TypeInfo Type;
        public override string ToString() => Text;
    }
}
