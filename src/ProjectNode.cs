using System;
using System.Linq;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public abstract class ProjectNode : IProjectNode
    {
        protected abstract ProjectDocument __OwnerDocument { get; }

        ProjectDocument IProjectNode.OwnerDocument { get { return __OwnerDocument; } }
    }

    public abstract class ProjectNode<TData> : ProjectNode
        where TData : XmlNode
    {
        protected abstract override ProjectDocument __OwnerDocument { get; }

        public abstract TData Data { get; }
    }

    public abstract class ProjectNode<TData, TParent> : ProjectNode<TData>, IProjectChild
        where TData : XmlNode
        where TParent : ProjectNode, IProjectParent
    {
        private TData _data;
        private TParent _parent;

        public override TData Data => _data;
        
        public TParent Parent => _parent;
        
        public ProjectDocument OwnerDocument { get { return __OwnerDocument; }}
        
        protected override ProjectDocument __OwnerDocument => (_parent == null) ? null : _parent.OwnerDocument;

        IProjectParent IProjectChild.Parent => _parent;

        protected ProjectNode(TParent parent, TData data)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            if (data == null)
                throw new ArgumentNullException("data");

            _parent = parent;
            _data = data;
        }

        void IProjectChild.OnValueChanging(XmlNode node, string oldValue, string newValue)
        {
            throw new NotImplementedException();
        }

        void IProjectChild.OnValueChanged(XmlNode node, string oldValue, string newValue)
        {
            throw new NotImplementedException();
        }
    }
}