using System;
using System.Linq;

namespace G9ConfigManagement.DataType
{
    /// <summary>
    ///     A data type for specifying the version of config
    ///     <para />
    ///     More explain: <see href="https://en.wikipedia.org/wiki/Software_versioning" />
    /// </summary>
    public class G9DtConfigVersion : IEquatable<G9DtConfigVersion>
    {
        #region Methods

        /// <summary>
        ///     Constructor
        ///     <para />
        ///     Initialize the requirements
        /// </summary>
        /// <param name="majorVersion">Specifies the major version in version structure 'X.0.0.0'.</param>
        /// <param name="minorVersion">Specifies the minor version in version structure '0.X.0.0'.</param>
        /// <param name="patchSetRelease">Specifies the patch set release in version structure '0.0.X.0'.</param>
        /// <param name="patchSetUpdate">Specifies the patch set update in version structure '0.0.0.X'.</param>
        /// <exception cref="Exception">All number parts in the version structure can't be zero.</exception>
        public G9DtConfigVersion(ushort majorVersion, ushort minorVersion, ushort patchSetRelease,
            ushort patchSetUpdate)
        {
            if (majorVersion == 0 && minorVersion == 0 && patchSetRelease == 0 && patchSetUpdate == 0)
                throw new Exception("All number parts in the version structure can't be zero.");

            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PatchSetRelease = patchSetRelease;
            PatchSetUpdate = patchSetUpdate;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{PatchSetRelease}.{PatchSetUpdate}";
        }

        /// <summary>
        ///     Method to parse a string version to object config version.
        /// </summary>
        /// <param name="version">Specifies a string version.</param>
        /// <returns>Parsed config version object.</returns>
        public static G9DtConfigVersion Parse(string version)
        {
            var splitVersion = version.Split('.').Select(ushort.Parse).ToArray();
            return new G9DtConfigVersion(splitVersion[0], splitVersion[1], splitVersion[2], splitVersion[3]);
        }

        /// <inheritdoc />
        public bool Equals(G9DtConfigVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MajorVersion == other.MajorVersion && MinorVersion == other.MinorVersion &&
                   PatchSetRelease == other.PatchSetRelease && PatchSetUpdate == other.PatchSetUpdate;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((G9DtConfigVersion)obj);
        }

#if !NETSTANDARD2_1
        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = MajorVersion;
                hashCode = (hashCode * 397) ^ MinorVersion;
                hashCode = (hashCode * 397) ^ PatchSetRelease;
                hashCode = (hashCode * 397) ^ PatchSetUpdate;
                return hashCode;
            }
        }
#else
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(MajorVersion, MinorVersion, PatchSetRelease, PatchSetUpdate);
        }
#endif

        /// Override equal operator 
        public static bool operator ==(G9DtConfigVersion obj1, G9DtConfigVersion obj2)
        {
            if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2))
                return true;
            if (ReferenceEquals(null, obj1)) return false;
            if (ReferenceEquals(null, obj2)) return false;
            return obj1.MajorVersion == obj2.MajorVersion && obj1.MinorVersion == obj2.MinorVersion &&
                   obj1.PatchSetRelease == obj2.PatchSetRelease && obj1.PatchSetUpdate == obj2.PatchSetUpdate;
        }

        /// Override not equal operator 
        public static bool operator !=(G9DtConfigVersion obj1, G9DtConfigVersion obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Specifies the major version in version structure 'X.0.0.0'.
        /// </summary>
        public readonly ushort MajorVersion;

        /// <summary>
        ///     Specifies the minor version in version structure '0.X.0.0'.
        /// </summary>
        public readonly ushort MinorVersion;

        /// <summary>
        ///     Specifies the patch set release in version structure '0.0.X.0'.
        /// </summary>
        public readonly ushort PatchSetRelease;

        /// <summary>
        ///     Specifies the patch set update in version structure '0.0.0.X'.
        /// </summary>
        public readonly ushort PatchSetUpdate;

        #endregion
    }
}