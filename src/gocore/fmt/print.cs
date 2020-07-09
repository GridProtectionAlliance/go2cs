//******************************************************************************************************
//  print.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  06/30/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace go
{
    public static partial class fmt_package
    {
        // Stringer is implemented by any value that has a String method,
        // which defines the ``native'' format for that value.
        // The String method is used to print values passed as an operand
        // to any format that accepts a string or to an unformatted printer
        // such as Print.
        public partial interface Stringer  {
            @string String();
        }
    }
}
