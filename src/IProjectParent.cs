using System;
using System.Collections.Generic;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public interface IProjectParent : IList<IProjectElement>, IProjectNode
    {
        void OnAddingChild(XmlNode node);
        void OnAddedChild(XmlNode node);
        void OnRemovingChild(IProjectChild child);
        void OnRemovedChild(IProjectChild child);
    }
}