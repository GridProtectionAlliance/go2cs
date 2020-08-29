// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xml -- go2cs converted at 2020 August 29 08:36:00 UTC
// import "encoding/xml" ==> using xml = go.encoding.xml_package
// Original source: C:\Go\src\encoding\xml\marshal.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using encoding = go.encoding_package;
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class xml_package
    {
 
        // Header is a generic XML header suitable for use with the output of Marshal.
        // This is not automatically added to any output of this package,
        // it is provided as a convenience.
        public static readonly @string Header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "\n";

        // Marshal returns the XML encoding of v.
        //
        // Marshal handles an array or slice by marshaling each of the elements.
        // Marshal handles a pointer by marshaling the value it points at or, if the
        // pointer is nil, by writing nothing. Marshal handles an interface value by
        // marshaling the value it contains or, if the interface value is nil, by
        // writing nothing. Marshal handles all other data by writing one or more XML
        // elements containing the data.
        //
        // The name for the XML elements is taken from, in order of preference:
        //     - the tag on the XMLName field, if the data is a struct
        //     - the value of the XMLName field of type Name
        //     - the tag of the struct field used to obtain the data
        //     - the name of the struct field used to obtain the data
        //     - the name of the marshaled type
        //
        // The XML element for a struct contains marshaled elements for each of the
        // exported fields of the struct, with these exceptions:
        //     - the XMLName field, described above, is omitted.
        //     - a field with tag "-" is omitted.
        //     - a field with tag "name,attr" becomes an attribute with
        //       the given name in the XML element.
        //     - a field with tag ",attr" becomes an attribute with the
        //       field name in the XML element.
        //     - a field with tag ",chardata" is written as character data,
        //       not as an XML element.
        //     - a field with tag ",cdata" is written as character data
        //       wrapped in one or more <![CDATA[ ... ]]> tags, not as an XML element.
        //     - a field with tag ",innerxml" is written verbatim, not subject
        //       to the usual marshaling procedure.
        //     - a field with tag ",comment" is written as an XML comment, not
        //       subject to the usual marshaling procedure. It must not contain
        //       the "--" string within it.
        //     - a field with a tag including the "omitempty" option is omitted
        //       if the field value is empty. The empty values are false, 0, any
        //       nil pointer or interface value, and any array, slice, map, or
        //       string of length zero.
        //     - an anonymous struct field is handled as if the fields of its
        //       value were part of the outer struct.
        //
        // If a field uses a tag "a>b>c", then the element c will be nested inside
        // parent elements a and b. Fields that appear next to each other that name
        // the same parent will be enclosed in one XML element.
        //
        // If the XML name for a struct field is defined by both the field tag and the
        // struct's XMLName field, the names must match.
        //
        // See MarshalIndent for an example.
        //
        // Marshal will return an error if asked to marshal a channel, function, or map.
        public static (slice<byte>, error) Marshal(object v)
        {
            bytes.Buffer b = default;
            {
                var err = NewEncoder(ref b).Encode(v);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (b.Bytes(), null);
        }

        // Marshaler is the interface implemented by objects that can marshal
        // themselves into valid XML elements.
        //
        // MarshalXML encodes the receiver as zero or more XML elements.
        // By convention, arrays or slices are typically encoded as a sequence
        // of elements, one per entry.
        // Using start as the element tag is not required, but doing so
        // will enable Unmarshal to match the XML elements to the correct
        // struct field.
        // One common implementation strategy is to construct a separate
        // value with a layout corresponding to the desired XML and then
        // to encode it using e.EncodeElement.
        // Another common strategy is to use repeated calls to e.EncodeToken
        // to generate the XML output one token at a time.
        // The sequence of encoded tokens must make up zero or more valid
        // XML elements.
        public partial interface Marshaler
        {
            error MarshalXML(ref Encoder e, StartElement start);
        }

        // MarshalerAttr is the interface implemented by objects that can marshal
        // themselves into valid XML attributes.
        //
        // MarshalXMLAttr returns an XML attribute with the encoded value of the receiver.
        // Using name as the attribute name is not required, but doing so
        // will enable Unmarshal to match the attribute to the correct
        // struct field.
        // If MarshalXMLAttr returns the zero attribute Attr{}, no attribute
        // will be generated in the output.
        // MarshalXMLAttr is used only for struct fields with the
        // "attr" option in the field tag.
        public partial interface MarshalerAttr
        {
            (Attr, error) MarshalXMLAttr(Name name);
        }

        // MarshalIndent works like Marshal, but each XML element begins on a new
        // indented line that starts with prefix and is followed by one or more
        // copies of indent according to the nesting depth.
        public static (slice<byte>, error) MarshalIndent(object v, @string prefix, @string indent)
        {
            bytes.Buffer b = default;
            var enc = NewEncoder(ref b);
            enc.Indent(prefix, indent);
            {
                var err = enc.Encode(v);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (b.Bytes(), null);
        }

        // An Encoder writes XML data to an output stream.
        public partial struct Encoder
        {
            public printer p;
        }

        // NewEncoder returns a new encoder that writes to w.
        public static ref Encoder NewEncoder(io.Writer w)
        {
            Encoder e = ref new Encoder(printer{Writer:bufio.NewWriter(w)});
            e.p.encoder = e;
            return e;
        }

        // Indent sets the encoder to generate XML in which each element
        // begins on a new indented line that starts with prefix and is followed by
        // one or more copies of indent according to the nesting depth.
        private static void Indent(this ref Encoder enc, @string prefix, @string indent)
        {
            enc.p.prefix = prefix;
            enc.p.indent = indent;
        }

        // Encode writes the XML encoding of v to the stream.
        //
        // See the documentation for Marshal for details about the conversion
        // of Go values to XML.
        //
        // Encode calls Flush before returning.
        private static error Encode(this ref Encoder enc, object v)
        {
            var err = enc.p.marshalValue(reflect.ValueOf(v), null, null);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(enc.p.Flush());
        }

        // EncodeElement writes the XML encoding of v to the stream,
        // using start as the outermost tag in the encoding.
        //
        // See the documentation for Marshal for details about the conversion
        // of Go values to XML.
        //
        // EncodeElement calls Flush before returning.
        private static error EncodeElement(this ref Encoder enc, object v, StartElement start)
        {
            var err = enc.p.marshalValue(reflect.ValueOf(v), null, ref start);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(enc.p.Flush());
        }

        private static slice<byte> begComment = (slice<byte>)"<!--";        private static slice<byte> endComment = (slice<byte>)"-->";        private static slice<byte> endProcInst = (slice<byte>)"?>";

        // EncodeToken writes the given XML token to the stream.
        // It returns an error if StartElement and EndElement tokens are not properly matched.
        //
        // EncodeToken does not call Flush, because usually it is part of a larger operation
        // such as Encode or EncodeElement (or a custom Marshaler's MarshalXML invoked
        // during those), and those will call Flush when finished.
        // Callers that create an Encoder and then invoke EncodeToken directly, without
        // using Encode or EncodeElement, need to call Flush when finished to ensure
        // that the XML is written to the underlying writer.
        //
        // EncodeToken allows writing a ProcInst with Target set to "xml" only as the first token
        // in the stream.
        private static error EncodeToken(this ref Encoder enc, Token t)
        {
            var p = ref enc.p;
            switch (t.type())
            {
                case StartElement t:
                    {
                        var err__prev1 = err;

                        var err = p.writeStart(ref t);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    break;
                case EndElement t:
                    {
                        var err__prev1 = err;

                        err = p.writeEnd(t.Name);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    break;
                case CharData t:
                    escapeText(p, t, false);
                    break;
                case Comment t:
                    if (bytes.Contains(t, endComment))
                    {
                        return error.As(fmt.Errorf("xml: EncodeToken of Comment containing --> marker"));
                    }
                    p.WriteString("<!--");
                    p.Write(t);
                    p.WriteString("-->");
                    return error.As(p.cachedWriteError());
                    break;
                case ProcInst t:
                    if (t.Target == "xml" && p.Buffered() != 0L)
                    {
                        return error.As(fmt.Errorf("xml: EncodeToken of ProcInst xml target only valid for xml declaration, first token encoded"));
                    }
                    if (!isNameString(t.Target))
                    {
                        return error.As(fmt.Errorf("xml: EncodeToken of ProcInst with invalid Target"));
                    }
                    if (bytes.Contains(t.Inst, endProcInst))
                    {
                        return error.As(fmt.Errorf("xml: EncodeToken of ProcInst containing ?> marker"));
                    }
                    p.WriteString("<?");
                    p.WriteString(t.Target);
                    if (len(t.Inst) > 0L)
                    {
                        p.WriteByte(' ');
                        p.Write(t.Inst);
                    }
                    p.WriteString("?>");
                    break;
                case Directive t:
                    if (!isValidDirective(t))
                    {
                        return error.As(fmt.Errorf("xml: EncodeToken of Directive containing wrong < or > markers"));
                    }
                    p.WriteString("<!");
                    p.Write(t);
                    p.WriteString(">");
                    break;
                default:
                {
                    var t = t.type();
                    return error.As(fmt.Errorf("xml: EncodeToken of invalid token type"));
                    break;
                }
            }
            return error.As(p.cachedWriteError());
        }

        // isValidDirective reports whether dir is a valid directive text,
        // meaning angle brackets are matched, ignoring comments and strings.
        private static bool isValidDirective(Directive dir)
        {
            long depth = default;            byte inquote = default;            bool incomment = default;
            foreach (var (i, c) in dir)
            {

                if (incomment) 
                    if (c == '>')
                    {
                        {
                            long n = 1L + i - len(endComment);

                            if (n >= 0L && bytes.Equal(dir[n..i + 1L], endComment))
                            {
                                incomment = false;
                            }

                        }
                    } 
                    // Just ignore anything in comment
                else if (inquote != 0L) 
                    if (c == inquote)
                    {
                        inquote = 0L;
                    } 
                    // Just ignore anything within quotes
                else if (c == '\'' || c == '"') 
                    inquote = c;
                else if (c == '<') 
                    if (i + len(begComment) < len(dir) && bytes.Equal(dir[i..i + len(begComment)], begComment))
                    {
                        incomment = true;
                    }
                    else
                    {
                        depth++;
                    }
                else if (c == '>') 
                    if (depth == 0L)
                    {
                        return false;
                    }
                    depth--;
                            }
            return depth == 0L && inquote == 0L && !incomment;
        }

        // Flush flushes any buffered XML to the underlying writer.
        // See the EncodeToken documentation for details about when it is necessary.
        private static error Flush(this ref Encoder enc)
        {
            return error.As(enc.p.Flush());
        }

        private partial struct printer
        {
            public ref bufio.Writer Writer => ref Writer_ptr;
            public ptr<Encoder> encoder;
            public long seq;
            public @string indent;
            public @string prefix;
            public long depth;
            public bool indentedIn;
            public bool putNewline;
            public map<@string, @string> attrNS; // map prefix -> name space
            public map<@string, @string> attrPrefix; // map name space -> prefix
            public slice<@string> prefixes;
            public slice<Name> tags;
        }

        // createAttrPrefix finds the name space prefix attribute to use for the given name space,
        // defining a new prefix if necessary. It returns the prefix.
        private static @string createAttrPrefix(this ref printer p, @string url)
        {
            {
                var prefix__prev1 = prefix;

                var prefix = p.attrPrefix[url];

                if (prefix != "")
                {
                    return prefix;
                } 

                // The "http://www.w3.org/XML/1998/namespace" name space is predefined as "xml"
                // and must be referred to that way.
                // (The "http://www.w3.org/2000/xmlns/" name space is also predefined as "xmlns",
                // but users should not be trying to use that one directly - that's our job.)

                prefix = prefix__prev1;

            } 

            // The "http://www.w3.org/XML/1998/namespace" name space is predefined as "xml"
            // and must be referred to that way.
            // (The "http://www.w3.org/2000/xmlns/" name space is also predefined as "xmlns",
            // but users should not be trying to use that one directly - that's our job.)
            if (url == xmlURL)
            {
                return xmlPrefix;
            } 

            // Need to define a new name space.
            if (p.attrPrefix == null)
            {
                p.attrPrefix = make_map<@string, @string>();
                p.attrNS = make_map<@string, @string>();
            } 

            // Pick a name. We try to use the final element of the path
            // but fall back to _.
            prefix = strings.TrimRight(url, "/");
            {
                var i = strings.LastIndex(prefix, "/");

                if (i >= 0L)
                {
                    prefix = prefix[i + 1L..];
                }

            }
            if (prefix == "" || !isName((slice<byte>)prefix) || strings.Contains(prefix, ":"))
            {
                prefix = "_";
            }
            if (strings.HasPrefix(prefix, "xml"))
            { 
                // xmlanything is reserved.
                prefix = "_" + prefix;
            }
            if (p.attrNS[prefix] != "")
            { 
                // Name is taken. Find a better one.
                p.seq++;

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    {
                        var id = prefix + "_" + strconv.Itoa(p.seq);

                        if (p.attrNS[id] == "")
                        {
                            prefix = id;
                            break;
                    p.seq++;
                        }

                    }
                }

            }
            p.attrPrefix[url] = prefix;
            p.attrNS[prefix] = url;

            p.WriteString("xmlns:");
            p.WriteString(prefix);
            p.WriteString("=\"");
            EscapeText(p, (slice<byte>)url);
            p.WriteString("\" ");

            p.prefixes = append(p.prefixes, prefix);

            return prefix;
        }

        // deleteAttrPrefix removes an attribute name space prefix.
        private static void deleteAttrPrefix(this ref printer p, @string prefix)
        {
            delete(p.attrPrefix, p.attrNS[prefix]);
            delete(p.attrNS, prefix);
        }

        private static void markPrefix(this ref printer p)
        {
            p.prefixes = append(p.prefixes, "");
        }

        private static void popPrefix(this ref printer p)
        {
            while (len(p.prefixes) > 0L)
            {
                var prefix = p.prefixes[len(p.prefixes) - 1L];
                p.prefixes = p.prefixes[..len(p.prefixes) - 1L];
                if (prefix == "")
                {
                    break;
                }
                p.deleteAttrPrefix(prefix);
            }

        }

        private static var marshalerType = reflect.TypeOf((Marshaler.Value)(null)).Elem();        private static var marshalerAttrType = reflect.TypeOf((MarshalerAttr.Value)(null)).Elem();        private static var textMarshalerType = reflect.TypeOf((encoding.TextMarshaler.Value)(null)).Elem();

        // marshalValue writes one or more XML elements representing val.
        // If val was obtained from a struct field, finfo must have its details.
        private static error marshalValue(this ref printer p, reflect.Value val, ref fieldInfo finfo, ref StartElement startTemplate)
        {
            if (startTemplate != null && startTemplate.Name.Local == "")
            {
                return error.As(fmt.Errorf("xml: EncodeElement of StartElement with missing name"));
            }
            if (!val.IsValid())
            {
                return error.As(null);
            }
            if (finfo != null && finfo.flags & fOmitEmpty != 0L && isEmptyValue(val))
            {
                return error.As(null);
            } 

            // Drill into interfaces and pointers.
            // This can turn into an infinite loop given a cyclic chain,
            // but it matches the Go 1 behavior.
            while (val.Kind() == reflect.Interface || val.Kind() == reflect.Ptr)
            {
                if (val.IsNil())
                {
                    return error.As(null);
                }
                val = val.Elem();
            }


            var kind = val.Kind();
            var typ = val.Type(); 

            // Check for marshaler.
            if (val.CanInterface() && typ.Implements(marshalerType))
            {
                return error.As(p.marshalInterface(val.Interface()._<Marshaler>(), defaultStart(typ, finfo, startTemplate)));
            }
            if (val.CanAddr())
            {
                var pv = val.Addr();
                if (pv.CanInterface() && pv.Type().Implements(marshalerType))
                {
                    return error.As(p.marshalInterface(pv.Interface()._<Marshaler>(), defaultStart(pv.Type(), finfo, startTemplate)));
                }
            } 

            // Check for text marshaler.
            if (val.CanInterface() && typ.Implements(textMarshalerType))
            {
                return error.As(p.marshalTextInterface(val.Interface()._<encoding.TextMarshaler>(), defaultStart(typ, finfo, startTemplate)));
            }
            if (val.CanAddr())
            {
                pv = val.Addr();
                if (pv.CanInterface() && pv.Type().Implements(textMarshalerType))
                {
                    return error.As(p.marshalTextInterface(pv.Interface()._<encoding.TextMarshaler>(), defaultStart(pv.Type(), finfo, startTemplate)));
                }
            } 

            // Slices and arrays iterate over the elements. They do not have an enclosing tag.
            if ((kind == reflect.Slice || kind == reflect.Array) && typ.Elem().Kind() != reflect.Uint8)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L;
                    var n = val.Len(); i < n; i++)
                    {
                        {
                            var err__prev2 = err;

                            var err = p.marshalValue(val.Index(i), finfo, startTemplate);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                    }


                    i = i__prev1;
                }
                return error.As(null);
            }
            var (tinfo, err) = getTypeInfo(typ);
            if (err != null)
            {
                return error.As(err);
            } 

            // Create start element.
            // Precedence for the XML element name is:
            // 0. startTemplate
            // 1. XMLName field in underlying struct;
            // 2. field name/tag in the struct field; and
            // 3. type name
            StartElement start = default;

            if (startTemplate != null)
            {
                start.Name = startTemplate.Name;
                start.Attr = append(start.Attr, startTemplate.Attr);
            }
            else if (tinfo.xmlname != null)
            {
                var xmlname = tinfo.xmlname;
                if (xmlname.name != "")
                {
                    start.Name.Space = xmlname.xmlns;
                    start.Name.Local = xmlname.name;
                }                {
                    Name (v, ok) = xmlname.value(val).Interface()._<Name>();


                    else if (ok && v.Local != "")
                    {
                        start.Name = v;
                    }

                }
            }
            if (start.Name.Local == "" && finfo != null)
            {
                start.Name.Space = finfo.xmlns;
                start.Name.Local = finfo.name;
            }
            if (start.Name.Local == "")
            {
                var name = typ.Name();
                if (name == "")
                {
                    return error.As(ref new UnsupportedTypeError(typ));
                }
                start.Name.Local = name;
            } 

            // Attributes
            {
                long i__prev1 = i;

                foreach (var (__i) in tinfo.fields)
                {
                    i = __i;
                    var finfo = ref tinfo.fields[i];
                    if (finfo.flags & fAttr == 0L)
                    {
                        continue;
                    }
                    var fv = finfo.value(val);

                    if (finfo.flags & fOmitEmpty != 0L && isEmptyValue(fv))
                    {
                        continue;
                    }
                    if (fv.Kind() == reflect.Interface && fv.IsNil())
                    {
                        continue;
                    }
                    name = new Name(Space:finfo.xmlns,Local:finfo.name);
                    {
                        var err__prev1 = err;

                        err = p.marshalAttr(ref start, name, fv);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                }

                i = i__prev1;
            }

            {
                var err__prev1 = err;

                err = p.writeStart(ref start);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            if (val.Kind() == reflect.Struct)
            {
                err = p.marshalStruct(tinfo, val);
            }
            else
            {
                var (s, b, err1) = p.marshalSimple(typ, val);
                if (err1 != null)
                {
                    err = err1;
                }
                else if (b != null)
                {
                    EscapeText(p, b);
                }
                else
                {
                    p.EscapeString(s);
                }
            }
            if (err != null)
            {
                return error.As(err);
            }
            {
                var err__prev1 = err;

                err = p.writeEnd(start.Name);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            return error.As(p.cachedWriteError());
        }

        // marshalAttr marshals an attribute with the given name and value, adding to start.Attr.
        private static error marshalAttr(this ref printer p, ref StartElement start, Name name, reflect.Value val)
        {
            if (val.CanInterface() && val.Type().Implements(marshalerAttrType))
            {
                MarshalerAttr (attr, err) = val.Interface()._<MarshalerAttr>().MarshalXMLAttr(name);
                if (err != null)
                {
                    return error.As(err);
                }
                if (attr.Name.Local != "")
                {
                    start.Attr = append(start.Attr, attr);
                }
                return error.As(null);
            }
            if (val.CanAddr())
            {
                var pv = val.Addr();
                if (pv.CanInterface() && pv.Type().Implements(marshalerAttrType))
                {
                    (attr, err) = pv.Interface()._<MarshalerAttr>().MarshalXMLAttr(name);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    if (attr.Name.Local != "")
                    {
                        start.Attr = append(start.Attr, attr);
                    }
                    return error.As(null);
                }
            }
            if (val.CanInterface() && val.Type().Implements(textMarshalerType))
            {
                encoding.TextMarshaler (text, err) = val.Interface()._<encoding.TextMarshaler>().MarshalText();
                if (err != null)
                {
                    return error.As(err);
                }
                start.Attr = append(start.Attr, new Attr(name,string(text)));
                return error.As(null);
            }
            if (val.CanAddr())
            {
                pv = val.Addr();
                if (pv.CanInterface() && pv.Type().Implements(textMarshalerType))
                {
                    (text, err) = pv.Interface()._<encoding.TextMarshaler>().MarshalText();
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    start.Attr = append(start.Attr, new Attr(name,string(text)));
                    return error.As(null);
                }
            } 

            // Dereference or skip nil pointer, interface values.

            if (val.Kind() == reflect.Ptr || val.Kind() == reflect.Interface) 
                if (val.IsNil())
                {
                    return error.As(null);
                }
                val = val.Elem();
            // Walk slices.
            if (val.Kind() == reflect.Slice && val.Type().Elem().Kind() != reflect.Uint8)
            {
                var n = val.Len();
                for (long i = 0L; i < n; i++)
                {
                    {
                        var err = p.marshalAttr(start, name, val.Index(i));

                        if (err != null)
                        {
                            return error.As(err);
                        }

                    }
                }

                return error.As(null);
            }
            if (val.Type() == attrType)
            {
                start.Attr = append(start.Attr, val.Interface()._<Attr>());
                return error.As(null);
            }
            var (s, b, err) = p.marshalSimple(val.Type(), val);
            if (err != null)
            {
                return error.As(err);
            }
            if (b != null)
            {
                s = string(b);
            }
            start.Attr = append(start.Attr, new Attr(name,s));
            return error.As(null);
        }

        // defaultStart returns the default start element to use,
        // given the reflect type, field info, and start template.
        private static StartElement defaultStart(reflect.Type typ, ref fieldInfo finfo, ref StartElement startTemplate)
        {
            StartElement start = default; 
            // Precedence for the XML element name is as above,
            // except that we do not look inside structs for the first field.
            if (startTemplate != null)
            {
                start.Name = startTemplate.Name;
                start.Attr = append(start.Attr, startTemplate.Attr);
            }
            else if (finfo != null && finfo.name != "")
            {
                start.Name.Local = finfo.name;
                start.Name.Space = finfo.xmlns;
            }
            else if (typ.Name() != "")
            {
                start.Name.Local = typ.Name();
            }
            else
            { 
                // Must be a pointer to a named type,
                // since it has the Marshaler methods.
                start.Name.Local = typ.Elem().Name();
            }
            return start;
        }

        // marshalInterface marshals a Marshaler interface value.
        private static error marshalInterface(this ref printer p, Marshaler val, StartElement start)
        { 
            // Push a marker onto the tag stack so that MarshalXML
            // cannot close the XML tags that it did not open.
            p.tags = append(p.tags, new Name());
            var n = len(p.tags);

            var err = val.MarshalXML(p.encoder, start);
            if (err != null)
            {
                return error.As(err);
            } 

            // Make sure MarshalXML closed all its tags. p.tags[n-1] is the mark.
            if (len(p.tags) > n)
            {
                return error.As(fmt.Errorf("xml: %s.MarshalXML wrote invalid XML: <%s> not closed", receiverType(val), p.tags[len(p.tags) - 1L].Local));
            }
            p.tags = p.tags[..n - 1L];
            return error.As(null);
        }

        // marshalTextInterface marshals a TextMarshaler interface value.
        private static error marshalTextInterface(this ref printer p, encoding.TextMarshaler val, StartElement start)
        {
            {
                var err = p.writeStart(ref start);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (text, err) = val.MarshalText();
            if (err != null)
            {
                return error.As(err);
            }
            EscapeText(p, text);
            return error.As(p.writeEnd(start.Name));
        }

        // writeStart writes the given start element.
        private static error writeStart(this ref printer p, ref StartElement start)
        {
            if (start.Name.Local == "")
            {
                return error.As(fmt.Errorf("xml: start tag with no name"));
            }
            p.tags = append(p.tags, start.Name);
            p.markPrefix();

            p.writeIndent(1L);
            p.WriteByte('<');
            p.WriteString(start.Name.Local);

            if (start.Name.Space != "")
            {
                p.WriteString(" xmlns=\"");
                p.EscapeString(start.Name.Space);
                p.WriteByte('"');
            } 

            // Attributes
            foreach (var (_, attr) in start.Attr)
            {
                var name = attr.Name;
                if (name.Local == "")
                {
                    continue;
                }
                p.WriteByte(' ');
                if (name.Space != "")
                {
                    p.WriteString(p.createAttrPrefix(name.Space));
                    p.WriteByte(':');
                }
                p.WriteString(name.Local);
                p.WriteString("=\"");
                p.EscapeString(attr.Value);
                p.WriteByte('"');
            }
            p.WriteByte('>');
            return error.As(null);
        }

        private static error writeEnd(this ref printer p, Name name)
        {
            if (name.Local == "")
            {
                return error.As(fmt.Errorf("xml: end tag with no name"));
            }
            if (len(p.tags) == 0L || p.tags[len(p.tags) - 1L].Local == "")
            {
                return error.As(fmt.Errorf("xml: end tag </%s> without start tag", name.Local));
            }
            {
                var top = p.tags[len(p.tags) - 1L];

                if (top != name)
                {
                    if (top.Local != name.Local)
                    {
                        return error.As(fmt.Errorf("xml: end tag </%s> does not match start tag <%s>", name.Local, top.Local));
                    }
                    return error.As(fmt.Errorf("xml: end tag </%s> in namespace %s does not match start tag <%s> in namespace %s", name.Local, name.Space, top.Local, top.Space));
                }

            }
            p.tags = p.tags[..len(p.tags) - 1L];

            p.writeIndent(-1L);
            p.WriteByte('<');
            p.WriteByte('/');
            p.WriteString(name.Local);
            p.WriteByte('>');
            p.popPrefix();
            return error.As(null);
        }

        private static (@string, slice<byte>, error) marshalSimple(this ref printer p, reflect.Type typ, reflect.Value val)
        {

            if (val.Kind() == reflect.Int || val.Kind() == reflect.Int8 || val.Kind() == reflect.Int16 || val.Kind() == reflect.Int32 || val.Kind() == reflect.Int64) 
                return (strconv.FormatInt(val.Int(), 10L), null, null);
            else if (val.Kind() == reflect.Uint || val.Kind() == reflect.Uint8 || val.Kind() == reflect.Uint16 || val.Kind() == reflect.Uint32 || val.Kind() == reflect.Uint64 || val.Kind() == reflect.Uintptr) 
                return (strconv.FormatUint(val.Uint(), 10L), null, null);
            else if (val.Kind() == reflect.Float32 || val.Kind() == reflect.Float64) 
                return (strconv.FormatFloat(val.Float(), 'g', -1L, val.Type().Bits()), null, null);
            else if (val.Kind() == reflect.String) 
                return (val.String(), null, null);
            else if (val.Kind() == reflect.Bool) 
                return (strconv.FormatBool(val.Bool()), null, null);
            else if (val.Kind() == reflect.Array) 
                if (typ.Elem().Kind() != reflect.Uint8)
                {
                    break;
                } 
                // [...]byte
                slice<byte> bytes = default;
                if (val.CanAddr())
                {
                    bytes = val.Slice(0L, val.Len()).Bytes();
                }
                else
                {
                    bytes = make_slice<byte>(val.Len());
                    reflect.Copy(reflect.ValueOf(bytes), val);
                }
                return ("", bytes, null);
            else if (val.Kind() == reflect.Slice) 
                if (typ.Elem().Kind() != reflect.Uint8)
                {
                    break;
                } 
                // []byte
                return ("", val.Bytes(), null);
                        return ("", null, ref new UnsupportedTypeError(typ));
        }

        private static slice<byte> ddBytes = (slice<byte>)"--";

        // indirect drills into interfaces and pointers, returning the pointed-at value.
        // If it encounters a nil interface or pointer, indirect returns that nil value.
        // This can turn into an infinite loop given a cyclic chain,
        // but it matches the Go 1 behavior.
        private static reflect.Value indirect(reflect.Value vf)
        {
            while (vf.Kind() == reflect.Interface || vf.Kind() == reflect.Ptr)
            {
                if (vf.IsNil())
                {
                    return vf;
                }
                vf = vf.Elem();
            }

            return vf;
        }

        private static error marshalStruct(this ref printer _p, ref typeInfo _tinfo, reflect.Value val) => func(_p, _tinfo, (ref printer p, ref typeInfo tinfo, Defer _, Panic panic, Recover __) =>
        {
            parentStack s = new parentStack(p:p);
            foreach (var (i) in tinfo.fields)
            {
                var finfo = ref tinfo.fields[i];
                if (finfo.flags & fAttr != 0L)
                {
                    continue;
                }
                var vf = finfo.value(val);


                if (finfo.flags & fMode == fCDATA || finfo.flags & fMode == fCharData) 
                    var emit = EscapeText;
                    if (finfo.flags & fMode == fCDATA)
                    {
                        emit = emitCDATA;
                    }
                    {
                        var err__prev1 = err;

                        var err = s.trim(finfo.parents);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    if (vf.CanInterface() && vf.Type().Implements(textMarshalerType))
                    {
                        encoding.TextMarshaler (data, err) = vf.Interface()._<encoding.TextMarshaler>().MarshalText();
                        if (err != null)
                        {
                            return error.As(err);
                        }
                        {
                            var err__prev2 = err;

                            err = emit(p, data);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                        continue;
                    }
                    if (vf.CanAddr())
                    {
                        var pv = vf.Addr();
                        if (pv.CanInterface() && pv.Type().Implements(textMarshalerType))
                        {
                            (data, err) = pv.Interface()._<encoding.TextMarshaler>().MarshalText();
                            if (err != null)
                            {
                                return error.As(err);
                            }
                            {
                                var err__prev3 = err;

                                err = emit(p, data);

                                if (err != null)
                                {
                                    return error.As(err);
                                }

                                err = err__prev3;

                            }
                            continue;
                        }
                    }
                    array<byte> scratch = new array<byte>(64L);
                    vf = indirect(vf);

                    if (vf.Kind() == reflect.Int || vf.Kind() == reflect.Int8 || vf.Kind() == reflect.Int16 || vf.Kind() == reflect.Int32 || vf.Kind() == reflect.Int64) 
                        {
                            var err__prev1 = err;

                            err = emit(p, strconv.AppendInt(scratch[..0L], vf.Int(), 10L));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (vf.Kind() == reflect.Uint || vf.Kind() == reflect.Uint8 || vf.Kind() == reflect.Uint16 || vf.Kind() == reflect.Uint32 || vf.Kind() == reflect.Uint64 || vf.Kind() == reflect.Uintptr) 
                        {
                            var err__prev1 = err;

                            err = emit(p, strconv.AppendUint(scratch[..0L], vf.Uint(), 10L));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (vf.Kind() == reflect.Float32 || vf.Kind() == reflect.Float64) 
                        {
                            var err__prev1 = err;

                            err = emit(p, strconv.AppendFloat(scratch[..0L], vf.Float(), 'g', -1L, vf.Type().Bits()));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (vf.Kind() == reflect.Bool) 
                        {
                            var err__prev1 = err;

                            err = emit(p, strconv.AppendBool(scratch[..0L], vf.Bool()));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (vf.Kind() == reflect.String) 
                        {
                            var err__prev1 = err;

                            err = emit(p, (slice<byte>)vf.String());

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (vf.Kind() == reflect.Slice) 
                        {
                            slice<byte> (elem, ok) = vf.Interface()._<slice<byte>>();

                            if (ok)
                            {
                                {
                                    var err__prev2 = err;

                                    err = emit(p, elem);

                                    if (err != null)
                                    {
                                        return error.As(err);
                                    }

                                    err = err__prev2;

                                }
                            }

                        }
                                        continue;
                else if (finfo.flags & fMode == fComment) 
                    {
                        var err__prev1 = err;

                        err = s.trim(finfo.parents);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    vf = indirect(vf);
                    var k = vf.Kind();
                    if (!(k == reflect.String || k == reflect.Slice && vf.Type().Elem().Kind() == reflect.Uint8))
                    {
                        return error.As(fmt.Errorf("xml: bad type for comment field of %s", val.Type()));
                    }
                    if (vf.Len() == 0L)
                    {
                        continue;
                    }
                    p.writeIndent(0L);
                    p.WriteString("<!--");
                    var dashDash = false;
                    var dashLast = false;

                    if (k == reflect.String) 
                        s = vf.String();
                        dashDash = strings.Contains(s, "--");
                        dashLast = s[len(s) - 1L] == '-';
                        if (!dashDash)
                        {
                            p.WriteString(s);
                        }
                    else if (k == reflect.Slice) 
                        var b = vf.Bytes();
                        dashDash = bytes.Contains(b, ddBytes);
                        dashLast = b[len(b) - 1L] == '-';
                        if (!dashDash)
                        {
                            p.Write(b);
                        }
                    else 
                        panic("can't happen");
                                        if (dashDash)
                    {
                        return error.As(fmt.Errorf("xml: comments must not contain \"--\""));
                    }
                    if (dashLast)
                    { 
                        // "--->" is invalid grammar. Make it "- -->"
                        p.WriteByte(' ');
                    }
                    p.WriteString("-->");
                    continue;
                else if (finfo.flags & fMode == fInnerXml) 
                    vf = indirect(vf);
                    var iface = vf.Interface();
                    switch (iface.type())
                    {
                        case slice<byte> raw:
                            p.Write(raw);
                            continue;
                            break;
                        case @string raw:
                            p.WriteString(raw);
                            continue;
                            break;

                    }
                else if (finfo.flags & fMode == fElement || finfo.flags & fMode == fElement | fAny) 
                    {
                        var err__prev1 = err;

                        err = s.trim(finfo.parents);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    if (len(finfo.parents) > len(s.stack))
                    {
                        if (vf.Kind() != reflect.Ptr && vf.Kind() != reflect.Interface || !vf.IsNil())
                        {
                            {
                                var err__prev3 = err;

                                err = s.push(finfo.parents[len(s.stack)..]);

                                if (err != null)
                                {
                                    return error.As(err);
                                }

                                err = err__prev3;

                            }
                        }
                    }
                                {
                    var err__prev1 = err;

                    err = p.marshalValue(vf, finfo, null);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }
            s.trim(null);
            return error.As(p.cachedWriteError());
        });

        // return the bufio Writer's cached write error
        private static error cachedWriteError(this ref printer p)
        {
            var (_, err) = p.Write(null);
            return error.As(err);
        }

        private static void writeIndent(this ref printer p, long depthDelta)
        {
            if (len(p.prefix) == 0L && len(p.indent) == 0L)
            {
                return;
            }
            if (depthDelta < 0L)
            {
                p.depth--;
                if (p.indentedIn)
                {
                    p.indentedIn = false;
                    return;
                }
                p.indentedIn = false;
            }
            if (p.putNewline)
            {
                p.WriteByte('\n');
            }
            else
            {
                p.putNewline = true;
            }
            if (len(p.prefix) > 0L)
            {
                p.WriteString(p.prefix);
            }
            if (len(p.indent) > 0L)
            {
                for (long i = 0L; i < p.depth; i++)
                {
                    p.WriteString(p.indent);
                }

            }
            if (depthDelta > 0L)
            {
                p.depth++;
                p.indentedIn = true;
            }
        }

        private partial struct parentStack
        {
            public ptr<printer> p;
            public slice<@string> stack;
        }

        // trim updates the XML context to match the longest common prefix of the stack
        // and the given parents. A closing tag will be written for every parent
        // popped. Passing a zero slice or nil will close all the elements.
        private static error trim(this ref parentStack s, slice<@string> parents)
        {
            long split = 0L;
            while (split < len(parents) && split < len(s.stack))
            {
                if (parents[split] != s.stack[split])
                {
                    break;
                split++;
                }
            }

            for (var i = len(s.stack) - 1L; i >= split; i--)
            {
                {
                    var err = s.p.writeEnd(new Name(Local:s.stack[i]));

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }

            s.stack = s.stack[..split];
            return error.As(null);
        }

        // push adds parent elements to the stack and writes open tags.
        private static error push(this ref parentStack s, slice<@string> parents)
        {
            for (long i = 0L; i < len(parents); i++)
            {
                {
                    var err = s.p.writeStart(ref new StartElement(Name:Name{Local:parents[i]}));

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }

            s.stack = append(s.stack, parents);
            return error.As(null);
        }

        // UnsupportedTypeError is returned when Marshal encounters a type
        // that cannot be converted into XML.
        public partial struct UnsupportedTypeError
        {
            public reflect.Type Type;
        }

        private static @string Error(this ref UnsupportedTypeError e)
        {
            return "xml: unsupported type: " + e.Type.String();
        }

        private static bool isEmptyValue(reflect.Value v)
        {

            if (v.Kind() == reflect.Array || v.Kind() == reflect.Map || v.Kind() == reflect.Slice || v.Kind() == reflect.String) 
                return v.Len() == 0L;
            else if (v.Kind() == reflect.Bool) 
                return !v.Bool();
            else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                return v.Int() == 0L;
            else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                return v.Uint() == 0L;
            else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                return v.Float() == 0L;
            else if (v.Kind() == reflect.Interface || v.Kind() == reflect.Ptr) 
                return v.IsNil();
                        return false;
        }
    }
}}
