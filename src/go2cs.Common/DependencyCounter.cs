//******************************************************************************************************
//  DependencyCounter.cs - Gbtc
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
//  07/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace go2cs
{
    /// <summary>
    /// Represents a list of dependencies as tracked by their counts.
    /// </summary>
    public class DependencyCounter : IEnumerable<string>
    {
        private readonly Dictionary<string, int> m_dependencyCounts;

        public DependencyCounter()
        {
            m_dependencyCounts = new(StringComparer.Ordinal);
        }

        public int Count => m_dependencyCounts.Count;

        public int Add(string dependency)
        {
            int count = m_dependencyCounts.GetOrAdd(dependency, 0) + 1;
            m_dependencyCounts[dependency] = count;
            return count;
        }

        public int Decrement(string dependency)
        {
            if (!m_dependencyCounts.TryGetValue(dependency, out int count))
                return 0;

            if (count > 1)
            {
                count--;
                m_dependencyCounts[dependency] = count;
                return count;
            }
            
            m_dependencyCounts.Remove(dependency);
            return 0;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return m_dependencyCounts.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
