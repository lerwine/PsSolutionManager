using System;
using System.Collections.Generic;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public interface IProjectElement : IProjectNode, IProjectParent, IProjectChild
    {
        ICollection<IProjectAttribute> Attributes { get; }
        IProjectElement Preceding { get; }
        IProjectElement Following { get; }
        new XmlElement Data { get; }
    }
}