using System;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public interface IProjectAttribute : IProjectNode, IProjectChild
    {
        new XmlAttribute Data { get; }
    }
}