using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public class ProjectDocument : ProjectNode<XmlDocument>, IProjectParent
    {
        public const string XmlnsMsb = "http://schemas.microsoft.com/developer/msbuild/2003";
        public const string _MsbPrefix = "msb";
        public const string MsbPrefix = "msb:";
        public const string ToolsVersion = "14.0";
        public const string DefaultTargets = "Build";

        private object _syncRoot = new object();
        private XmlDocument _xmlDocument;
        private XmlNamespaceManager _nsmgr;
        private RootProjectElement _rootElement = null;

        protected override ProjectDocument __OwnerDocument => this;

        public override XmlDocument Data => _xmlDocument;
        
        ProjectAttributeCollection IProjectParent.Attributes => throw new NotImplementedException();

        int ICollection<IProjectElement>.Count => (_rootElement == null) ? 0 : 1;

        bool ICollection<IProjectElement>.IsReadOnly => true;

        public RootProjectElement RootElement => _rootElement;

        IProjectElement IList<IProjectElement>.this[int index]
        {
            get
            {
                RootProjectElement rootElement = _rootElement;
                if (rootElement == null || index != 0)
                    throw new IndexOutOfRangeException();
                return rootElement;
            }
            set => throw new NotImplementedException();
        }

        public ProjectDocument()
        {
            _xmlDocument = new XmlDocument();
            _nsmgr = new XmlNamespaceManager(_xmlDocument.NameTable);
            _nsmgr.AddNamespace(_MsbPrefix, XmlnsMsb);
            _xmlDocument.NodeChanging += NodeChanging;
            _xmlDocument.NodeChanged += NodeChanged;
            _xmlDocument.NodeInserting += NodeInserting;
            _xmlDocument.NodeInserted += NodeInserted;
            _xmlDocument.NodeRemoving += NodeRemoving;
            _xmlDocument.NodeRemoved += NodeRemoved;
        }

        ~ProjectDocument()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                _xmlDocument.NodeChanging -= NodeChanging;
                _xmlDocument.NodeChanged -= NodeChanged;
                _xmlDocument.NodeInserting -= NodeInserting;
                _xmlDocument.NodeInserted -= NodeInserted;
                _xmlDocument.NodeRemoving -= NodeRemoving;
                _xmlDocument.NodeRemoved -= NodeRemoved;
                _xmlDocument.NodeChanging += NodeChanging;
                _xmlDocument.NodeChanged += NodeChanged;
                _xmlDocument.NodeInserting += NodeInserting;
                _xmlDocument.NodeInserted += NodeInserted;
                _xmlDocument.NodeRemoving += NodeRemoving;
                _xmlDocument.NodeRemoved += NodeRemoved;
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        public void Load(string uri)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(uri);
            XmlElement projectElement = xmlDocument.DocumentElement;
            if (projectElement == null)
                throw new Exception("No XML data loaded.");
            projectElement = (XmlElement)(_xmlDocument.ImportNode(projectElement, true));
            if (projectElement.NamespaceURI != XmlnsMsb || projectElement.LocalName != ElementNames.Project)
                throw new Exception("XML data does not represent a valid project.");

            Monitor.Enter(_syncRoot);
            try
            {
                if (_xmlDocument.DocumentElement == null)
                    _xmlDocument.AppendChild(projectElement);
                else
                    _xmlDocument.ReplaceChild(projectElement, _xmlDocument.DocumentElement);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        public IProjectParent GetCorrespondingParent(XmlNode node)
        {
            if (node is XmlDocument)
                return (ReferenceEquals(node, _xmlDocument)) ? this : null;
            
            if (node is XmlElement && node.ParentNode != null)
            {
                IProjectParent parent = GetCorrespondingParent(node.ParentNode);
                return (parent == null) ? null : parent.FirstOrDefault(e => ReferenceEquals(e.Data, node));
            }
            
            return null;
        }

        public IProjectChild GetCorrespondingChild(XmlNode node)
        {
            if (node is XmlDocument)
                return null;
            if (node is XmlElement && node.ParentNode != null)
            {
                IProjectParent parent = GetCorrespondingParent(node.ParentNode);
                return (parent == null) ? null : parent.FirstOrDefault(e => ReferenceEquals(e.Data, node));
            }
            if (node is XmlAttribute)
            {
                IProjectParent parent = GetCorrespondingParent(node.ParentNode);
                return (parent == null) ? null : parent.Attributes.FirstOrDefault(a => ReferenceEquals(a.Data, node));
            }
            return null;
        }

        private void NodeChanging(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                IProjectChild target = GetCorrespondingChild(e.NewParent ?? e.OldParent);
                if (target != null)
                    target.OnValueChanging(e.Node, e.OldValue, e.NewValue);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        private void NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                IProjectChild target = GetCorrespondingChild(e.NewParent ?? e.OldParent);
                if (target != null)
                    target.OnValueChanged(e.Node, e.OldValue, e.NewValue);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        private void NodeInserting(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (e.Node is XmlElement || e.Node is XmlAttribute)
                {
                    IProjectParent target = GetCorrespondingParent(e.NewParent);
                    if (target != null)
                        target.OnAddingChild(e.Node);
                }
                else if (e.Node is XmlCharacterData)
                {
                    IProjectChild target = GetCorrespondingChild(e.NewParent);
                    if (target != null)
                        target.OnValueChanging(e.Node, null, e.NewValue);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        private void NodeInserted(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (e.Node is XmlElement || e.Node is XmlAttribute)
                {
                    IProjectParent target = GetCorrespondingParent(e.NewParent);
                    if (target != null)
                        target.OnAddedChild(e.Node);
                }
                else if (e.Node is XmlCharacterData)
                {
                    IProjectChild target = GetCorrespondingChild(e.NewParent);
                    if (target != null)
                        target.OnValueChanged(e.Node, null, e.NewValue);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        private void NodeRemoving(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (e.Node is XmlElement || e.Node is XmlAttribute)
                {
                    IProjectParent parent = GetCorrespondingParent(e.OldParent);
                    if (parent == null)
                        return;
                    IProjectChild child = GetCorrespondingChild(e.Node);
                    if (child != null)
                        parent.OnRemovingChild(child);
                }
                else if (e.Node is XmlCharacterData)
                {
                    IProjectChild target = GetCorrespondingChild(e.NewParent);
                    if (target != null)
                        target.OnValueChanging(e.Node, e.OldValue, null);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }
        
        private void NodeRemoved(object sender, XmlNodeChangedEventArgs e)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (e.Node is XmlElement || e.Node is XmlAttribute)
                {
                    IProjectParent parent = GetCorrespondingParent(e.OldParent);
                    if (parent == null)
                        return;
                    IProjectChild child = GetCorrespondingChild(e.Node);
                    if (child != null)
                        parent.OnRemovedChild(child);
                }
                else if (e.Node is XmlCharacterData)
                {
                    IProjectChild target = GetCorrespondingChild(e.NewParent);
                    if (target != null)
                        target.OnValueChanged(e.Node, e.OldValue, null);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void IProjectParent.OnAddingChild(XmlNode node)
        {
        }

        void IProjectParent.OnAddedChild(XmlNode node)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (node != null && _xmlDocument.DocumentElement != null && ReferenceEquals(node, _xmlDocument.DocumentElement))
                    _rootElement = new RootProjectElement(this);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void IProjectParent.OnRemovingChild(IProjectChild child)
        {
        }

        void IProjectParent.OnRemovedChild(IProjectChild child)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (_xmlDocument.DocumentElement == null)
                    _rootElement = null;
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        int IList<IProjectElement>.IndexOf(IProjectElement item)
        {
            RootProjectElement rootElement = _rootElement;
            return (rootElement != null && ReferenceEquals(item, rootElement)) ? 0 : -1;
        }

        void IList<IProjectElement>.Insert(int index, IProjectElement item) => throw new NotSupportedException();

        void IList<IProjectElement>.RemoveAt(int index)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (index != 0 || _xmlDocument.DocumentElement == null)
                    throw new ArgumentOutOfRangeException("index");
                _xmlDocument.RemoveChild(_xmlDocument.DocumentElement);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<IProjectElement>.Add(IProjectElement item) => throw new NotSupportedException();

        void ICollection<IProjectElement>.Clear()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (_xmlDocument.DocumentElement != null)
                    _xmlDocument.RemoveChild(_xmlDocument.DocumentElement);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        bool ICollection<IProjectElement>.Contains(IProjectElement item)
        {
            if (item == null)
                return false;
            Monitor.Enter(_syncRoot);
            try
            {
                return (_rootElement != null && ReferenceEquals(item, _rootElement));
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<IProjectElement>.CopyTo(IProjectElement[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { ((_xmlDocument.DocumentElement == null) ? new IProjectElement[0] : new IProjectElement[] { _rootElement }).CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
            
        }

        bool ICollection<IProjectElement>.Remove(IProjectElement item)
        {
            if (item == null)
                return false;
            Monitor.Enter(_syncRoot);
            try
            {
                if (_rootElement != null && ReferenceEquals(item, _rootElement))
                {
                    _xmlDocument.RemoveChild(_xmlDocument.DocumentElement);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }

            return false;
        }

        public IEnumerator<IProjectElement> GetEnumerator()
        {
            return (new List<IProjectElement>((_xmlDocument.DocumentElement == null) ? new IProjectElement[0] : new IProjectElement[] { _rootElement })).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((_xmlDocument.DocumentElement == null) ? new IProjectElement[0] : new IProjectElement[] { _rootElement }).GetEnumerator();
        }
    }
}