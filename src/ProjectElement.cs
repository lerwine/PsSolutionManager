using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public class ProjectElement<TParent> : ProjectNode<XmlElement, TParent>, IProjectElement
        where TParent : ProjectNode, IProjectParent
    {
        private ProjectAttributeCollection<TParent> _attributes;
        private List<IProjectElement> _elements = new List<IProjectElement>();
        private IProjectElement _preceding = null;
        private IProjectElement _following = null;

        public IProjectElement this[int index] { get => _elements[index]; }

        IProjectElement IList<IProjectElement>.this[int index]
        {
            get => _elements[index];
            set => throw new NotImplementedException();
        }

        public IProjectElement Preceding => _preceding;

        public IProjectElement Following => _following;

        public ProjectAttributeCollection<TParent> Attributes => _attributes;

        public int Count => _elements.Count;

        bool ICollection<IProjectElement>.IsReadOnly => false;

        //IProjectParent IProjectChild.Parent => base.Parent;

        //XmlElement IProjectElement.Data => throw new NotImplementedException();

        public ProjectElement(TParent parent, XmlElement element)
            : base(parent, element)
        {
            if (!(parent.Data.HasChildNodes && parent.Data.ChildNodes.OfType<XmlElement>().Any(e => ReferenceEquals(e, element))) ||
                    parent.Any(e => ReferenceEquals(e.Data, element)))
                throw new InvalidOperationException();
            _attributes = new ProjectAttributeCollection<TParent>(this);
        }

        public void Add(IProjectElement item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IProjectElement item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IProjectElement[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IProjectElement> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(IProjectElement item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IProjectElement item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IProjectElement item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IProjectParent.OnAddingChild(XmlNode node)
        {
            throw new NotImplementedException();
        }

        void IProjectParent.OnAddedChild(XmlNode node)
        {
            throw new NotImplementedException();
        }

        void IProjectParent.OnRemovingChild(IProjectChild child)
        {
            throw new NotImplementedException();
        }

        void IProjectParent.OnRemovedChild(IProjectChild child)
        {
            throw new NotImplementedException();
        }
    }
}