﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace go2cs.Templates
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\Projects\go2cs\src\go2cs.Templates\IChannelImplementation.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class IChannelImplementation : TemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            
            #line 1 "D:\Projects\go2cs\src\go2cs.Templates\IChannelImplementation.tt"
 // This template creates an inherited type, e.g., type MyFloat float64 in a <PackageName>_<StructName>StructOf(<GoTypeName>).cs file 
            
            #line default
            #line hidden
            this.Write(@"
            public nint Capacity => m_value.Capacity;

            public nint Length => m_value.Length;

            public bool SendIsReady => m_value.SendIsReady;

            public bool ReceiveIsReady => m_value.ReceiveIsReady;

            void Send(object value) => m_value.Send(value);

            object Receive() => m_value.Receive();

            bool Sent(object value) => m_value.Sent(value);

            bool Received(out object value) => m_values.Received(out value);

            void Close() => m_value.Close();

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();
");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 24 "D:\Projects\go2cs\src\go2cs.Templates\IChannelImplementation.tt"

// Template Parameters
public string TypeName;

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
}
