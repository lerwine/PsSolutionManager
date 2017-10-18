using System;
using System.IO;
using System.Management.Automation;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Erwine.Leonard.T.PsSolutionManager
{
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ValidateXmlSourceAttribute : ValidateEnumeratedArgumentsAttribute
    {
        public bool AllowNull { get; set; }

        public bool AllowEmptyString { get; set; }

        public ValidateXmlSourceAttribute() { }

        public static XmlReader AsXmlReader(object value)
        {
            if (value == null)
                return null;
            
            object baseObject = value;
            if (value is PSObject)
                baseObject = (value as PSObject).BaseObject;
            
            if (baseObject is XmlReader)
            {
                XmlReader xmlReader = value as XmlReader;
                XmlReaderSettings settings = xmlReader.Settings.Clone();
                settings.CloseInput = false;
                return XmlReader.Create(value as XmlReader, settings);
            }
            
            Func<XmlReaderSettings> getSettings = () =>
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.CheckCharacters = false;
                settings.DtdProcessing = DtdProcessing.Ignore;
                settings.ValidationType = ValidationType.None;
                settings.CloseInput = false;
                return settings;
            };

            if (baseObject is Stream)
                return XmlReader.Create((Stream)baseObject, getSettings());
            
            if (baseObject is TextReader)
                return XmlReader.Create((TextReader)baseObject, getSettings());

            return (baseObject is XmlDocument) ? new DocumentXmlReader((XmlDocument)baseObject) : new DocumentXmlReader((baseObject is string) ? (string)baseObject : value.ToString());
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                if (AllowNull)
                    return;
                throw new ValidationMetadataException("Parameter cannot be null.");
            }

            object baseObject = element;
            if (element is PSObject)
                baseObject = (element as PSObject).BaseObject;
            
            if (baseObject is string)
            {
                if (!AllowEmptyString && ((string)baseObject).Trim().Length == 0)
                    throw new ValidationMetadataException("Parameter cannot be empty.");
                return;
            }

            if (!(baseObject is XmlReader || baseObject is Stream || baseObject is TextReader || baseObject is XmlDocument))
                throw new ValidationMetadataException("Parameter cannot parsed as XML.");
        }

        class DocumentXmlReader : XmlReader
        {
            XmlReader _innerReader;
            IDisposable _innerDisposable;

            public override string this[string name] { get { return _innerReader[name]; } }
            public override string this[int i] { get { return _innerReader[i]; } }
            public override string this[string name, string namespaceURI] { get { return _innerReader[name, namespaceURI]; } }
            public override bool HasAttributes { get { return _innerReader.HasAttributes; } }
            public override string XmlLang { get { return _innerReader.XmlLang; } }
            public override XmlSpace XmlSpace { get { return _innerReader.XmlSpace; } }
            public override char QuoteChar { get { return _innerReader.QuoteChar; } }
            public override bool IsDefault { get { return _innerReader.IsDefault; } }
            public override bool IsEmptyElement { get { return _innerReader.IsEmptyElement; } }
            public override string BaseURI { get { return _innerReader.BaseURI; } }
            public override string Value { get { return _innerReader.Value; } }
            public override bool HasValue { get { return _innerReader.HasValue; } }
            public override string Prefix { get { return _innerReader.Prefix; } }
            public override XmlReaderSettings Settings { get { return _innerReader.Settings; } }
            public override XmlNodeType NodeType { get { return _innerReader.NodeType; } }
            public override string Name { get { return _innerReader.Name; } }
            public override int Depth { get { return _innerReader.Depth; } }
            public override IXmlSchemaInfo SchemaInfo { get { return _innerReader.SchemaInfo; } }
            public override Type ValueType { get { return _innerReader.ValueType; } }
            public override int AttributeCount { get { return _innerReader.AttributeCount; } }
            public override string LocalName { get { return _innerReader.LocalName; } }
            public override bool CanReadValueChunk { get { return _innerReader.CanReadValueChunk; } }
            public override bool CanReadBinaryContent { get { return _innerReader.CanReadBinaryContent; } }
            public override string NamespaceURI { get { return _innerReader.NamespaceURI; } }
            public override XmlNameTable NameTable { get { return _innerReader.NameTable; } }
            public override ReadState ReadState { get { return _innerReader.ReadState; } }
            public override bool EOF { get { return _innerReader.EOF; } }
            public override bool CanResolveEntity { get { return _innerReader.CanResolveEntity; } }
            internal DocumentXmlReader(XmlDocument xmlDocument)
            {
                MemoryStream memoryStream = new MemoryStream();
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.CheckCharacters = false;
                ws.CloseOutput = false;
                using (XmlWriter writer = XmlWriter.Create(memoryStream, ws))
                {
                    xmlDocument.WriteTo(writer);
                    writer.Flush();
                }
                try
                {
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    XmlReaderSettings settings  = new XmlReaderSettings();
                    settings.CheckCharacters = false;
                    settings.DtdProcessing = DtdProcessing.Ignore;
                    settings.ValidationType = ValidationType.None;
                    settings.CloseInput = true;
                    _innerReader = XmlReader.Create(memoryStream, settings);
                }
                catch
                {
                    memoryStream.Dispose();
                    throw;
                }
                _innerDisposable = memoryStream;
            }
            internal DocumentXmlReader(string text)
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.CheckCharacters = false;
                settings.DtdProcessing = DtdProcessing.Ignore;
                settings.ValidationType = ValidationType.None;
                settings.CloseInput = true;
                StringReader reader = new StringReader(text);
                try { _innerReader = XmlReader.Create(reader, settings); }
                catch
                {
                    reader.Dispose();
                    throw;
                }
                _innerDisposable = reader;
            }
            public override void Close() { _innerReader.Close(); }
            public override string GetAttribute(string name, string namespaceURI) { return _innerReader.GetAttribute(name, namespaceURI); }
            public override string GetAttribute(string name) { return _innerReader.GetAttribute(name); }
            public override string GetAttribute(int i) { return _innerReader.GetAttribute(i); }
            public override bool IsStartElement(string localname, string ns) { return _innerReader.IsStartElement(localname, ns); }
            public override bool IsStartElement(string name) { return _innerReader.IsStartElement(name); }
            public override bool IsStartElement() { return _innerReader.IsStartElement(); }
            public override string LookupNamespace(string prefix) { return _innerReader.LookupNamespace(prefix); }
            public override bool MoveToAttribute(string name) { return _innerReader.MoveToAttribute(name); }
            public override void MoveToAttribute(int i) { _innerReader.MoveToAttribute(i); }
            public override bool MoveToAttribute(string name, string ns) { return _innerReader.MoveToAttribute(name, ns); }
            public override XmlNodeType MoveToContent() { return _innerReader.MoveToContent(); }
            public override bool MoveToElement() { return _innerReader.MoveToElement(); }
            public override bool MoveToFirstAttribute() { return _innerReader.MoveToFirstAttribute(); }
            public override bool MoveToNextAttribute() { return _innerReader.MoveToNextAttribute(); }
            public override bool Read() { return _innerReader.Read(); }
            public override bool ReadAttributeValue() { return _innerReader.ReadAttributeValue(); }
            public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) { return _innerReader.ReadContentAs(returnType, namespaceResolver); }
            public override int ReadContentAsBase64(byte[] buffer, int index, int count) { return _innerReader.ReadContentAsBase64(buffer, index, count); }
            public override int ReadContentAsBinHex(byte[] buffer, int index, int count) { return _innerReader.ReadContentAsBinHex(buffer, index, count); }
            public override bool ReadContentAsBoolean() { return _innerReader.ReadContentAsBoolean(); }
            public override DateTime ReadContentAsDateTime() { return _innerReader.ReadContentAsDateTime(); }
            public override DateTimeOffset ReadContentAsDateTimeOffset() { return _innerReader.ReadContentAsDateTimeOffset(); }
            public override decimal ReadContentAsDecimal() { return _innerReader.ReadContentAsDecimal(); }
            public override double ReadContentAsDouble() { return _innerReader.ReadContentAsDouble(); }
            public override float ReadContentAsFloat() { return _innerReader.ReadContentAsFloat(); }
            public override int ReadContentAsInt() { return _innerReader.ReadContentAsInt(); }
            public override long ReadContentAsLong() { return _innerReader.ReadContentAsLong(); }
            public override object ReadContentAsObject() { return _innerReader.ReadContentAsObject(); }
            public override string ReadContentAsString() { return _innerReader.ReadContentAsString(); }
            public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) { return _innerReader.ReadElementContentAs(returnType, namespaceResolver); }
            public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI) { return _innerReader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI); }
            public override int ReadElementContentAsBase64(byte[] buffer, int index, int count) { return _innerReader.ReadElementContentAsBase64(buffer, index, count); }
            public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count) { return _innerReader.ReadElementContentAsBinHex(buffer, index, count); }
            public override bool ReadElementContentAsBoolean() { return _innerReader.ReadElementContentAsBoolean(); }
            public override bool ReadElementContentAsBoolean(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsBoolean(localName, namespaceURI); }
            public override DateTime ReadElementContentAsDateTime() { return _innerReader.ReadElementContentAsDateTime(); }
            public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsDateTime(localName, namespaceURI); }
            public override decimal ReadElementContentAsDecimal(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsDecimal(localName, namespaceURI); }
            public override decimal ReadElementContentAsDecimal() { return _innerReader.ReadElementContentAsDecimal(); }
            public override double ReadElementContentAsDouble(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsDouble(localName, namespaceURI); }
            public override double ReadElementContentAsDouble() { return _innerReader.ReadElementContentAsDouble(); }
            public override float ReadElementContentAsFloat() { return _innerReader.ReadElementContentAsFloat(); }
            public override float ReadElementContentAsFloat(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsFloat(localName, namespaceURI); }
            public override int ReadElementContentAsInt(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsInt(localName, namespaceURI); }
            public override int ReadElementContentAsInt() { return _innerReader.ReadElementContentAsInt(); }
            public override long ReadElementContentAsLong() { return _innerReader.ReadElementContentAsLong(); }
            public override long ReadElementContentAsLong(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsLong(localName, namespaceURI); }
            public override object ReadElementContentAsObject() { return _innerReader.ReadElementContentAsObject(); }
            public override object ReadElementContentAsObject(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsObject(localName, namespaceURI); }
            public override string ReadElementContentAsString(string localName, string namespaceURI) { return _innerReader.ReadElementContentAsString(localName, namespaceURI); }
            public override string ReadElementContentAsString() { return _innerReader.ReadElementContentAsString(); }
            public override string ReadElementString(string localname, string ns) { return _innerReader.ReadElementString(localname, ns); }
            public override string ReadElementString() { return _innerReader.ReadElementString(); }
            public override string ReadElementString(string name) { return _innerReader.ReadElementString(name); }
            public override void ReadEndElement() { _innerReader.ReadEndElement(); }
            public override string ReadInnerXml() { return _innerReader.ReadInnerXml(); }
            public override string ReadOuterXml() { return _innerReader.ReadOuterXml(); }
            public override void ReadStartElement(string name) { _innerReader.ReadStartElement(name); }
            public override void ReadStartElement() { _innerReader.ReadStartElement(); }
            public override void ReadStartElement(string localname, string ns) { _innerReader.ReadStartElement(localname, ns); }
            public override string ReadString() { return _innerReader.ReadString(); }
            public override XmlReader ReadSubtree() { return _innerReader.ReadSubtree(); }
            public override bool ReadToDescendant(string localName, string namespaceURI) { return _innerReader.ReadToDescendant(localName, namespaceURI); }
            public override bool ReadToDescendant(string name) { return _innerReader.ReadToDescendant(name); }
            public override bool ReadToFollowing(string name) { return _innerReader.ReadToFollowing(name); }
            public override bool ReadToFollowing(string localName, string namespaceURI) { return _innerReader.ReadToFollowing(localName, namespaceURI); }
            public override bool ReadToNextSibling(string name) { return _innerReader.ReadToNextSibling(name); }
            public override bool ReadToNextSibling(string localName, string namespaceURI) { return _innerReader.ReadToNextSibling(localName, namespaceURI); }
            public override int ReadValueChunk(char[] buffer, int index, int count) { return _innerReader.ReadValueChunk(buffer, index, count); }
            public override void ResolveEntity() { _innerReader.ResolveEntity(); }
            public override void Skip() { _innerReader.Skip(); }
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    _innerReader.Dispose();       
                    _innerDisposable.Dispose();
                }
            }
        }
    }
}