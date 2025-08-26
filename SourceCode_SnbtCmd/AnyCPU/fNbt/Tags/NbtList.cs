using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
    /// <summary> A tag containing a list of unnamed tags, all of the same kind. </summary>
    public sealed class NbtList : NbtContainerTag
    {
        /// <summary> Type of this tag (List). </summary>
        public override NbtTagType TagType => NbtTagType.List;

        private readonly List<NbtTag> TagList = new List<NbtTag>();

        /// <summary> Gets or sets the tag type of this list. All tags in this NbtTag must be of the same type. </summary>
        /// <exception cref="ArgumentException"> If the given NbtTagType does not match the type of existing list items (for non-empty lists). </exception>
        /// <exception cref="ArgumentOutOfRangeException"> If the given NbtTagType is a recognized tag type. </exception>
        public NbtTagType ListType
        {
            get
            {
                if (TagList.Count == 0)
                    return NbtTagType.End;
                return TagList[0].TagType;
            }
        }

        #region constructors
        /// <summary> Creates an unnamed NbtList with empty contents and undefined ListType. </summary>
        public NbtList() : this(null, null) { }

        /// <summary> Creates an NbtList with given name, empty contents, and undefined ListType. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        public NbtList([CanBeNull] string tagName) : this(tagName, null) { }

        /// <summary> Creates an unnamed NbtList with the given contents, and inferred ListType. 
        /// If given tag array is empty, NbtTagType remains Unknown. </summary>
        /// <param name="tags"> Collection of tags to insert into the list. All tags are expected to be of the same type.
        /// ListType is inferred from the first tag. List may be empty, but may not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If given tags are of mixed types. </exception>
        public NbtList([NotNull] IEnumerable<NbtTag> tags) : this(null, tags)
        {
            // the base constructor will allow null "tags," but we don't want that in this constructor
            if (tags == null) throw new ArgumentNullException("tags");
        }

        /// <summary> Creates an NbtList with the given name and contents, and an explicitly specified ListType. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="tags"> Collection of tags to insert into the list.
        /// All tags are expected to be of the same type. May be empty or <c>null</c>. </param>
        public NbtList([CanBeNull] string tagName, [CanBeNull] IEnumerable<NbtTag> tags)
        {
            name = tagName;
            if (tags == null) return;
            AddRange(tags);
        }


        /// <summary> Creates a deep copy of given NbtList. </summary>
        /// <param name="other"> An existing NbtList to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtList([NotNull] NbtList other)
        {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            AddRange(other.TagList.Select(x => (NbtTag)x.Clone()));
        }
        #endregion

        /// <summary> Copies all tags in this NbtList to an array. </summary>
        /// <returns> Array of NbtTags. </returns>
        [NotNull]
        [Pure]
        public NbtTag[] ToArray() => TagList.ToArray();

        #region Reading / Writing
        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            if (readStream.Selector != null && !readStream.Selector(this))
            {
                SkipTag(readStream);
                return false;
            }

            var list_type = readStream.ReadTagType();

            int length = readStream.ReadInt32();
            if (length < 0)
            {
                throw new NbtFormatException("Negative list size given.");
            }

            for (int i = 0; i < length; i++)
            {
                NbtTag newTag;
                switch (list_type)
                {
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
                        // should never happen, since ListType is checked beforehand
                        throw new NbtFormatException("Unsupported tag type found in a list: " + ListType);
                }
                newTag.Parent = this;
                if (newTag.ReadTag(readStream))
                {
                    TagList.Add(newTag);
                }
            }
            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            // read list type, and make sure it's defined
            var list_type = readStream.ReadTagType();

            int length = readStream.ReadInt32();
            if (length < 0)
            {
                throw new NbtFormatException("Negative list size given.");
            }

            switch (list_type)
            {
                case NbtTagType.Byte:
                    readStream.Skip(length);
                    break;
                case NbtTagType.Short:
                    readStream.Skip(length * sizeof(short));
                    break;
                case NbtTagType.Int:
                    readStream.Skip(length * sizeof(int));
                    break;
                case NbtTagType.Long:
                    readStream.Skip(length * sizeof(long));
                    break;
                case NbtTagType.Float:
                    readStream.Skip(length * sizeof(float));
                    break;
                case NbtTagType.Double:
                    readStream.Skip(length * sizeof(double));
                    break;
                default:
                    for (int i = 0; i < length; i++)
                    {
                        switch (list_type)
                        {
                            case NbtTagType.ByteArray:
                                new NbtByteArray().SkipTag(readStream);
                                break;
                            case NbtTagType.String:
                                readStream.SkipString();
                                break;
                            case NbtTagType.List:
                                new NbtList().SkipTag(readStream);
                                break;
                            case NbtTagType.Compound:
                                new NbtCompound().SkipTag(readStream);
                                break;
                            case NbtTagType.IntArray:
                                new NbtIntArray().SkipTag(readStream);
                                break;
                        }
                    }
                    break;
            }
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.List);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(ListType);
            writeStream.Write(TagList.Count);
            foreach (NbtTag tag in TagList)
            {
                tag.WriteData(writeStream);
            }
        }
        #endregion

        #region container implementation
        public override void ThrowIfCantAdd(IEnumerable<NbtTag> tags)
        {
            if (!tags.Any())
                return;
            base.ThrowIfCantAdd(tags);

            if (tags.Any(x => x.Name != null))
                throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
            var tag_types = tags.Select(x => x.TagType).Distinct();
            if (tag_types.Count() > 1)
                throw new ArgumentException("Items must all be the same type");
            if (TagList.Count > 0 && tag_types.Single() != ListType)
                throw new ArgumentException($"Items must be of type {ListType}");
        }
        public override bool CanAddType(NbtTagType type) => TagList.Count == 0 || type == ListType;

        public override int Count => TagList.Count;
        public override IEnumerable<NbtTag> Tags => TagList;
        public override int IndexOf(NbtTag item) => TagList.IndexOf(item);
        public override bool Contains(NbtTag item) => TagList.Contains(item);
        protected override void DoInsert(int index, NbtTag item) => TagList.Insert(index, item);
        protected override void DoAdd(NbtTag item) => TagList.Add(item);
        protected override bool DoRemove(NbtTag item) => TagList.Remove(item);
        protected override NbtTag DoGet(int index) => TagList[index];
        protected override void DoSet(int index, NbtTag item) => TagList[index] = item;
        protected override void DoRemoveAt(int index) => TagList.RemoveAt(index);
        protected override void DoClear() => TagList.Clear();
        #endregion

        /// <inheritdoc />
        public override object Clone()
        {
            return new NbtList(this);
        }

    }
}
