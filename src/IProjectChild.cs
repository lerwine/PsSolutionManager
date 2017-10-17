using System;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public interface IProjectChild : IProjectNode
    {
        IProjectParent Parent { get; }
        void OnValueChanging(XmlNode node, string oldValue, string newValue);
        void OnValueChanged(XmlNode node, string oldValue, string newValue);
    }
}