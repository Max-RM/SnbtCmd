using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fNbt
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public abstract class NbtContainerTag : NbtTag, IEnumerable<NbtTag>
    {
        public delegate void ChildrenChangedEvent(NbtContainerTag changed);
        public event ChildrenChangedEvent ChildrenChanged;

        public abstract IEnumerable<NbtTag> Tags { get; }
        public abstract int Count { get; }
        public abstract int IndexOf(NbtTag item);
        public abstract bool Contains(NbtTag item);
        protected abstract void DoInsert(int index, NbtTag item);
        protected abstract void DoAdd(NbtTag item);
        protected abstract NbtTag DoGet(int index);
        protected abstract void DoSet(int index, NbtTag item);
        protected abstract bool DoRemove(NbtTag item);
        protected abstract void DoRemoveAt(int index);
        protected abstract void DoClear();
        public IEnumerator<NbtTag> GetEnumerator() => Tags.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool CanAdd(IEnumerable<NbtTag> tags)
        {
            try { ThrowIfCantAdd(tags); }
            catch { return false; }
            return true;
        }

        public abstract bool CanAddType(NbtTagType type);

        public virtual void ThrowIfCantAdd(IEnumerable<NbtTag> tags)
        {
            if (tags.Any(x => x is null))
                throw new ArgumentNullException();
            if (tags.Any(x => x.Parent is not null))
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            if (tags.OfType<NbtContainerTag>().Any(x => x.IsAncestor(this)))
                throw new ArgumentException("A tag cannot be added to its own descendant.");
        }
        public void ThrowIfCantAdd(NbtTag tag) => ThrowIfCantAdd(new[] { tag });

        private void FireEvents()
        {
            ChildrenChanged?.Invoke(this);
            CascadeChanges();
        }

        public void Insert(int index, NbtTag item)
        {
            ThrowIfCantAdd(item);
            DoInsert(index, item);
            item.Parent = this;
            FireEvents();
        }
        public void Add(NbtTag item)
        {
            ThrowIfCantAdd(item);
            DoAdd(item);
            item.Parent = this;
            FireEvents();
        }
        public void AddRange(IEnumerable<NbtTag> items)
        {
            ThrowIfCantAdd(items);
            foreach (var item in items)
            {
                DoAdd(item);
                item.Parent = this;
            }
            FireEvents();
        }
        public void Clear()
        {
            foreach (var tag in Tags)
            {
                tag.Parent = null;
            }
            DoClear();
            FireEvents();
        }
        public bool Remove(NbtTag item)
        {
            if (DoRemove(item))
            {
                item.Parent = null;
                FireEvents();
                return true;
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            DoGet(index).Parent = null;
            DoRemoveAt(index);
            FireEvents();
        }
        public override NbtTag this[int tagIndex]
        {
            get { return DoGet(tagIndex); }
            set
            {
                ThrowIfCantAdd(value);
                DoGet(tagIndex).Parent = null;
                DoSet(tagIndex, value);
                value.Parent = this;
                FireEvents();
            }
        }

    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
