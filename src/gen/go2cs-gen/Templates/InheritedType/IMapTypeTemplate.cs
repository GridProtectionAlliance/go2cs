using System;

namespace go2cs.Templates.InheritedType;

internal static class IMapTypeTemplate
{
    public static string Generate(string structName, string keyTypeName, string valueTypeName) =>
        $$"""
        
                public nint Length => ((IMap)m_value).Length;
                
                public bool IsNil => ((IMap)m_value).IsNil;
                
                /// <summary>ISupportMake factory — a made named map wraps a made concrete map.</summary>
                public static {{structName}} Make(nint p1, nint p2) => new {{structName}}(map<{{keyTypeName}}, {{valueTypeName}}>.Make(p1, p2));
                
                public int Count => m_value.Count;
                
                public {{valueTypeName}} this[{{keyTypeName}} key]
                {
                    get => m_value[key];
                    set => m_value[key] = value;
                }
                
                public ({{valueTypeName}}, bool) this[{{keyTypeName}} key, bool _] => m_value[key, _];
                
                public void Add({{keyTypeName}} key, {{valueTypeName}} value) => m_value.Add(key, value);
                
                public bool Remove({{keyTypeName}} key) => m_value.Remove(key);
                
                public void Clear() => m_value.Clear();
                
                public bool TryGetValue({{keyTypeName}} key, out {{valueTypeName}} value) => m_value.TryGetValue(key, out value);
                
                public bool ContainsKey({{keyTypeName}} key) => m_value.ContainsKey(key);
                
                public global::System.Collections.Generic.ICollection<{{keyTypeName}}> Keys => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>)m_value).Keys;
                
                public global::System.Collections.Generic.ICollection<{{valueTypeName}}> Values => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>)m_value).Values;
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Add(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Add(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Contains(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Contains(item);
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.CopyTo(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>[] array, int arrayIndex) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).CopyTo(array, arrayIndex);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Remove(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Remove(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.IsReadOnly => false;
                
                public global::System.Collections.Generic.IEnumerator<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>> GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).GetEnumerator();
                
                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        """;
}
