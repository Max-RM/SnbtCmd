﻿using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> A tag containing a single string. String is stored in UTF-8 encoding. </summary>
    public sealed class NbtString : NbtTag {
        /// <summary> Type of this tag (String). </summary>
        public override NbtTagType TagType {
            get { return NbtTagType.String; }
        }

        /// <summary> Value/payload of this tag (a single string). May not be <c>null</c>. </summary>
        [NotNull]
        public string Value {
            get => stringVal;
            set {
                stringVal = value;
                CascadeChanges();
            }
        }

        private void SetValue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            stringVal = value;
        }

        [NotNull]
        string stringVal = "";


        /// <summary> Creates an unnamed NbtString tag with the default value (empty string). </summary>
        public NbtString() {}


        /// <summary> Creates an unnamed NbtString tag with the given value. </summary>
        /// <param name="value"> String value to assign to this tag. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is <c>null</c>. </exception>
        public NbtString([NotNull] string value)
            : this(null, value) {}


        /// <summary> Creates an NbtString tag with the given name and value. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="value"> String value to assign to this tag. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is <c>null</c>. </exception>
        public NbtString([CanBeNull] string tagName, [NotNull] string value) {
            if (value == null) throw new ArgumentNullException("value");
            name = tagName;
            stringVal = value;
        }


        /// <summary> Creates a copy of given NbtString tag. </summary>
        /// <param name="other"> Tag to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtString([NotNull] NbtString other) {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            stringVal = other.Value;
        }


        #region Reading / Writing

        internal override bool ReadTag(NbtBinaryReader readStream) {
            if (readStream.Selector != null && !readStream.Selector(this)) {
                readStream.SkipString();
                return false;
            }
            stringVal = readStream.ReadString();
            return true;
        }


        internal override void SkipTag(NbtBinaryReader readStream) {
            readStream.SkipString();
        }


        internal override void WriteTag(NbtBinaryWriter writeStream) {
            writeStream.Write(NbtTagType.String);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            writeStream.Write(Value);
        }


        internal override void WriteData(NbtBinaryWriter writeStream) {
            writeStream.Write(Value);
        }

        #endregion


        /// <inheritdoc />
        public override object Clone() {
            return new NbtString(this);
        }
    }
}
