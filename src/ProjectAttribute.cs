using System;
using System.Linq;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public class ProjectAttribute<TParent> : ProjectNode<XmlAttribute, TParent>, IProjectAttribute
        where TParent : ProjectNode, IProjectParent
    {
        IProjectParent IProjectChild.Parent => base.Parent;
        
        public ProjectAttribute(TParent parent, XmlAttribute attribute)
            : base(parent, attribute)
        {
            if (!(parent.Data.HasChildNodes && parent.Data.Attributes.OfType<XmlAttribute>().Any(a => ReferenceEquals(a, attribute))) ||
                    parent.Attributes.Any(e => ReferenceEquals(e.Data, attribute)))
                throw new InvalidOperationException();
        }
    }
}