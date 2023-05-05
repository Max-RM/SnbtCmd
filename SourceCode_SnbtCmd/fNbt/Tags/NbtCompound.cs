using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
    /// <summary> A tag containing a set of other named tags. Order is not guaranteed. </summary>
    public sealed class NbtCompound : NbtContainerTag
    {
        /// <summary> Type of this tag (Compound). </summary>
        public override NbtTagType TagType => NbtTagType.Compound;

        private readonly OrderedDictionary<string, NbtTag> TagDict = new();

        #region constructors
        /// <summary> Creates an empty unnamed NbtCompound tag. </summary>
        public NbtCompound() { }

        /// <summary> Creates an empty NbtCompound tag with the given name. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        public NbtCompound([CanBeNull] string tagName)
        {
            name = tagName;
        }

        /// <summary> Creates an unnamed NbtCompound tag, containing the given tags. </summary>
        /// <param name="tags"> Collection of tags to assign to this tag's Value. May not be null </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>, or one of the tags is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If some of the given tags were not named, or two tags with the same name were given. </exception>
        public NbtCompound([NotNull] IEnumerable<NbtTag> tags)
            : this(null, tags) { }

        /// <summary> Creates an NbtCompound tag with the given name, containing the given tags. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="tags"> Collection of tags to assign to this tag's Value. May not be null </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>, or one of the tags is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If some of the given tags were not named, or two tags with the same name were given. </exception>
        public NbtCompound([CanBeNull] string tagName, [NotNull] IEnumerable<NbtTag> tags)
        {
            if (tags == null) throw new ArgumentNullException("tags");
            name = tagName;
            AddRange(tags);
        }

        /// <summary> Creates a deep copy of given NbtCompound. </summary>
        /// <param name="other"> An existing NbtCompound to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtCompound([NotNull] NbtCompound other)
        {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            AddRange(other.Tags.Select(x => (NbtTag)x.Clone()));
        }
        #endregion

        /// <summary> Gets or sets the tag with the specified name. May return <c>null</c>. </summary>
        /// <returns> The tag with the specified key. Null if tag with the given name was not found. </returns>
        /// <param name="tagName"> The name of the tag to get or set. Must match tag's actual name. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> is <c>null</c>; or if trying to assign null value. </exception>
        /// <exception cref="ArgumentException"> <paramref name="tagName"/> does not match the given tag's actual name;
        /// or given tag already has a Parent. </exception>
        public override NbtTag this[[NotNull] string tagName]
        {
            [CanBeNull]
            get { return Get<NbtTag>(tagName); }
            set
            {
                if (tagName == null)
                {
                    throw new ArgumentNullException("tagName");
                }
                else if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                else if (value.Name != tagName)
                {
                    throw new ArgumentException("Given tag name must match tag's actual name.");
                }
                else if (value.Parent != null)
                {
                    throw new ArgumentException("A tag may only be added to one compound/list at a time.");
                }
                else if (value == this)
                {
                    throw new ArgumentException("Cannot add tag to itself");
                }
                TagDict[tagName] = value;
                value.Parent = this;
            }
        }

        /// <summary> Gets the tag with the specified name. May return <c>null</c>. </summary>
        /// <param name="tagName"> The name of the tag to get. </param>
        /// <typeparam name="T"> Type to cast the result to. Must derive from NbtTag. </typeparam>
        /// <returns> The tag with the specified key. Null if tag with the given name was not found. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> is <c>null</c>. </exception>
        /// <exception cref="InvalidCastException"> If tag could not be cast to the desired tag. </exception>
        [CanBeNull]
        public T Get<T>([NotNull] string tagName) where T : NbtTag
        {
            if (tagName == null) throw new ArgumentNullException("tagName");
            if (TagDict.ContainsKey(tagName))
            {
                return (T)TagDict[tagName];
            }
            return null;
        }

        /// <summary> Gets the tag with the specified name. May return <c>null</c>. </summary>
        /// <param name="tagName"> The name of the tag to get. </param>
        /// <returns> The tag with the specified key. Null if tag with the given name was not found. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> is <c>null</c>. </exception>
        /// <exception cref="InvalidCastException"> If tag could not be cast to the desired tag. </exception>
        [CanBeNull]
        public NbtTag Get([NotNull] string tagName)
        {
            if (tagName == null) throw new ArgumentNullException("tagName");
            if (TagDict.ContainsKey(tagName))
            {
                return TagDict[tagName];
            }
            return null;
        }

        /// <summary> Gets the tag with the specified name. </summary>
        /// <param name="tagName"> The name of the tag to get. </param>
        /// <param name="result"> When this method returns, contains the tag associated with the specified name, if the tag is found;
        /// otherwise, null. This parameter is passed uninitialized. </param>
        /// <typeparam name="T"> Type to cast the result to. Must derive from NbtTag. </typeparam>
        /// <returns> true if the NbtCompound contains a tag with the specified name; otherwise, false. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> is <c>null</c>. </exception>
        /// <exception cref="InvalidCastException"> If tag could not be cast to the desired tag. </exception>
        public bool TryGet<T>([NotNull] string tagName, out T result) where T : NbtTag
        {
            if (tagName == null) throw new ArgumentNullException("tagName");
            if (TagDict.ContainsKey(tagName))
            {
                result = (T)TagDict[tagName];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary> Gets the tag with the specified name. </summary>
        /// <param name="tagName"> The name of the tag to get. </param>
        /// <param name="result"> When this method returns, contains the tag associated with the specified name, if the tag is found;
        /// otherwise, null. This parameter is passed uninitialized. </param>
        /// <returns> true if the NbtCompound contains a tag with the specified name; otherwise, false. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> is <c>null</c>. </exception>
        /// <exception cref="InvalidCastException"> If tag could not be cast to the desired tag. </exception>
        public bool TryGet([NotNull] string tagName, out NbtTag result)
        {
            if (tagName == null) throw new ArgumentNullException("tagName");
            if (TagDict.ContainsKey(tagName))
            {
                result = TagDict[tagName];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        #region sorting
        public void Sort(IComparer<NbtTag> sorter, bool recursive)
        {
            var tags = Tags.OrderBy(x => x, sorter).ToList();
            if (recursive)
            {
                foreach (var tag in tags)
                {
                    if (tag is NbtCompound sub)
                        sub.Sort(sorter, true);
                    else if (tag is NbtList list)
                        SortListChildren(list, sorter);
                }
            }
            Clear();
            AddRange(tags);
        }

        private void UnsortRecursive(NbtCompound reference)
        {
            var order = Tags.OrderBy(x => reference.IndexOf(x.Name)).ToList();
            foreach (var tag in order)
            {
                if (tag is NbtCompound sub)
                    sub.UnsortRecursive((NbtCompound)reference[tag.Name]);
                else if (tag is NbtList list)
                    UnsortListChildren(list, (NbtList)reference[tag.Name]);
            }
            UnsortRoot(order);
        }

        private static void UnsortListChildren(NbtList list, NbtList reference)
        {
            if (list.ListType == NbtTagType.Compound)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ((NbtCompound)list[i]).UnsortRecursive((NbtCompound)reference[i]);
                }
            }
            else if (list.ListType == NbtTagType.List)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    UnsortListChildren((NbtList)list[i], (NbtList)reference[i]);
                }
            }
        }

        private void UnsortRoot(List<NbtTag> order)
        {
            Clear();
            AddRange(order);
        }

        private static void SortListChildren(NbtList list, IComparer<NbtTag> sorter)
        {
            if (list.ListType == NbtTagType.Compound)
            {
                foreach (NbtCompound item in list.Tags)
                {
                    item.Sort(sorter, true);
                }
            }
            else if (list.ListType == NbtTagType.List)
            {
                foreach (NbtList item in list.Tags)
                {
                    SortListChildren(item, sorter);
                }
            }
        }
        #endregion

        internal void RenameTag([NotNull] string oldName, [NotNull] string newName)
        {
            Debug.Assert(oldName != null);
            Debug.Assert(newName != null);
            Debug.Assert(newName != oldName);
            if (TagDict.ContainsKey(newName))
            {
                throw new ArgumentException("Cannot rename: a tag with the name already exists in this compound.");
            }
            if (!TagDict.ContainsKey(oldName))
            {
                throw new ArgumentException("Cannot rename: no tag found to rename.");
            }
            var tag = TagDict[oldName];
            var index = IndexOf(tag);
            TagDict.Remove(oldName);
            TagDict.Insert(index, newName, tag);
        }

        /// <summary> Gets a collection containing all tag names in this NbtCompound. </summary>
        [NotNull]
        public IEnumerable<string> Names
        {
            get { return TagDict.Keys; }
        }

        /// <summary> Gets a collection containing all tags in this NbtCompound. </summary>
        [NotNull]
        public override IEnumerable<NbtTag> Tags
        {
            get { return TagDict.Values; }
        }

        public int IndexOf(string name)
        {
            return TagDict.IndexOf(name);
        }

        public bool Contains(string name)
        {
            return TagDict.ContainsKey(name);
        }

        public bool Remove(string name)
        {
            if (TagDict.TryGetValue(name, out var result))
                return this.Remove(result);
            return false;
        }

        #region Reading / Writing

        internal static NbtTag CreateTag(NbtTagType type)
        {
            switch (type)
            {
                case NbtTagType.Byte:
                    return new NbtByte();
                case NbtTagType.Short:
                    return new NbtShort();
                case NbtTagType.Int:
                    return new NbtInt();
                case NbtTagType.Long:
                    return new NbtLong();
                case NbtTagType.Float:
                    return new NbtFloat();
                case NbtTagType.Double:
                    return new NbtDouble();
                case NbtTagType.ByteArray:
                    return new NbtByteArray();
                case NbtTagType.String:
                    return new NbtString();
                case NbtTagType.List:
                    return new NbtList();
                case NbtTagType.Compound:
                    return new NbtCompound();
                case NbtTagType.IntArray:
                    return new NbtIntArray();
                case NbtTagType.LongArray:
                    return new NbtLongArray();
                default:
                    throw new NbtFormatException("Unsupported tag type found: " + type);
            }
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            if (Parent != null && readStream.Selector != null && !readStream.Selector(this))
            {
                SkipTag(readStream);
                return false;
            }

            while (true)
            {
                NbtTagType nextTag = readStream.ReadTagType();
                if (nextTag == NbtTagType.End)
                    return true;

                NbtTag newTag = CreateTag(nextTag);
                newTag.Parent = this;
                newTag.Name = readStream.ReadString();
                if (newTag.ReadTag(readStream))
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    // newTag.Name is never null
                    TagDict.Add(newTag.Name, newTag);
                    // ReSharper restore AssignNullToNotNullAttribute
                }
            }
        }


        internal override void SkipTag(NbtBinaryReader readStream)
        {
            while (true)
            {
                NbtTagType nextTag = readStream.ReadTagType();
                NbtTag newTag;
                switch (nextTag)
                {
                    case NbtTagType.End:
                        return;

                    case NbtTagType.Byte:
                        newTag = new NbtByte();
                        break;

                    case NbtTagType.Short:
                        newTag = new NbtShort();
                        break;

                    case NbtTagType.Int:
                        newTag = new NbtInt();
                        break;

                    case NbtTagType.Long:
                        newTag = new NbtLong();
                        break;

                    case NbtTagType.Float:
                        newTag = new NbtFloat();
                        break;

                    case NbtTagType.Double:
                        newTag = new NbtDouble();
                        break;

                    case NbtTagType.ByteArray:
                        newTag = new NbtByteArray();
                        break;

                    case NbtTagType.String:
                        newTag = new NbtString();
                        break;

                    case NbtTagType.List:
                        newTag = new NbtList();
                        break;

                    case NbtTagType.Compound:
                        newTag = new NbtCompound();
                        break;

                    case NbtTagType.IntArray:
                        newTag = new NbtIntArray();
                        break;

                    case NbtTagType.LongArray:
                        newTag = new NbtLongArray();
                        break;

                    default:
                        throw new NbtFormatException("Unsupported tag type found in NBT_Compound: " + nextTag);
                }
                readStream.SkipString();
                newTag.SkipTag(readStream);
            }
        }


        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.Compound);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            WriteData(writeStream);
        }


        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            foreach (NbtTag tag in TagDict.Values)
            {
                tag.WriteTag(writeStream);
            }
            writeStream.Write(NbtTagType.End);
        }

        #endregion


        #region container implementation
        public override void ThrowIfCantAdd(IEnumerable<NbtTag> tags)
        {
            if (!tags.Any())
                return;
            base.ThrowIfCantAdd(tags);

            if (tags.Any(x => x.Name is null))
                throw new ArgumentException("Unnamed tag given. A compound may only contain named tags.");
        }
        public override bool CanAddType(NbtTagType type) => true;

        public override int Count => TagDict.Count;
        public override int IndexOf(NbtTag item)
        {
            if (TagDict.TryGetValue(item.Name, out var result) && result == item)
                return TagDict.IndexOf(item.Name);
            return -1;
        }
        public override bool Contains(NbtTag item) => IndexOf(item) != -1;
        protected override void DoInsert(int index, NbtTag item) => TagDict.Insert(index, item.Name, item);
        protected override void DoAdd(NbtTag item) => TagDict.Add(item.Name, item);
        protected override bool DoRemove(NbtTag item)
        {
            if (TagDict.TryGetValue(item.Name, out var result) && result == item)
                return TagDict.Remove(item.Name);
            return false;
        }
        protected override NbtTag DoGet(int index) => TagDict[index];
        protected override void DoSet(int index, NbtTag item) => TagDict[index] = item;
        protected override void DoRemoveAt(int index) => TagDict.RemoveAt(index);
        protected override void DoClear() => TagDict.Clear();
        #endregion

        /// <inheritdoc />
        public override object Clone()
        {
            return new NbtCompound(this);
        }
    }
}
