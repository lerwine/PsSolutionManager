using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public class RootProjectElement : ProjectElement<ProjectDocument>
    {
        internal RootProjectElement(ProjectDocument parent)
            : base(parent, parent.Data.DocumentElement)
        {
            if (parent.RootElement != null || parent.Data.OwnerDocument == null)
                throw new InvalidOperationException();
        }
    }
}