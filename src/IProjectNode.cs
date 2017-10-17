using System;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public interface IProjectNode
    {
        ProjectDocument OwnerDocument { get; }
        XmlNode Data { get; }
    }
}