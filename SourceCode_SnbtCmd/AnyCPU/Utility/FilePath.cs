using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryashtarUtils.Utility
{
    // file path that is safe for comparisons/dictionary entries
    // (i.e. so partial paths aren't considered separate from their full names etc)
    public class FilePath : IEquatable<FilePath>
    {
        public readonly string Location;
        public readonly bool IsRelative;
        private readonly string ComparisonLocation;
        private readonly string[] Pieces;
        public FilePath(string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
                Location = uri.LocalPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            else
                Location = uri.OriginalString;
            IsRelative = Path.IsPathRooted(Location);
            ComparisonLocation = Location.ToLowerInvariant();
            Pieces = Location.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        public FilePath(params string[] path) : this(String.Join(Path.DirectorySeparatorChar.ToString(), path))
        { }

        public FilePath()
        {
            Location = "";
            IsRelative = true;
            ComparisonLocation = "";
            Pieces = new string[0];
        }

        public string FileName => Pieces[Pieces.Length - 1];

        public FilePath Rooted()
        {
            return new FilePath(Path.GetFullPath(this.Location));
        }

        public FilePath CombineWith(FilePath other)
        {
            return new FilePath(Path.Combine(this.Location, other.Location));
        }

        public FilePath CombineWith(params string[] deeper)
        {
            var path = new FilePath(deeper);
            return CombineWith(path);
        }

        public FilePath ParentDirectory()
        {
            return new FilePath(Path.GetDirectoryName(this.Location));
        }

        public bool StartsWith(FilePath prefix)
        {
            if (prefix.Pieces.Length > this.Pieces.Length)
                return false;
            for (int i = 0; i < prefix.Pieces.Length; i++)
            {
                if (!String.Equals(Pieces[i], prefix.Pieces[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        public bool Equals(FilePath obj)
        {
            if (obj == null)
                return false;
            return ComparisonLocation == obj.ComparisonLocation;
        }

        public override bool Equals(object obj)
        {
            if (obj is string s)
                return Equals(new FilePath(s));
            if (obj is FilePath f)
                return Equals(f);
            return false;
        }

        public override int GetHashCode()
        {
            return ComparisonLocation.GetHashCode();
        }

        public override string ToString() => Location;

        public static implicit operator FilePath(string input)
        {
            return new FilePath(input);
        }

        public static bool operator ==(FilePath p1, FilePath p2)
        {
            if ((object)p1 == null)
                return (object)p2 == null;
            return p1.Equals(p2);
        }
        public static bool operator !=(FilePath p1, FilePath p2) => !(p1 == p2);
    }
}
