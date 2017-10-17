using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Erwine.Leonard.T.PsSolutionManager
{
    public class ProjectAttributeCollection<TParent> : IList<IProjectAttribute>
        where TParent : ProjectNode, IProjectParent
    {
        private List<IProjectAttribute> _attributes = new List<IProjectAttribute>();

        public IProjectAttribute this[int index] { get => _attributes[index]; }

        IProjectAttribute IList<IProjectAttribute>.this[int index]
        {
            get => _attributes[index];
            set => throw new NotImplementedException();
        }

        public int Count => _attributes.Count;

        bool ICollection<IProjectAttribute>.IsReadOnly => false;

        public ProjectAttributeCollection(ProjectElement<TParent> projectElement)
        {
            throw new NotImplementedException();
        }

        public void Add(IProjectAttribute item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IProjectAttribute item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IProjectAttribute[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IProjectAttribute> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(IProjectAttribute item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IProjectAttribute item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IProjectAttribute item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}