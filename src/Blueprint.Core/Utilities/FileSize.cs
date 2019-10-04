using System;
using System.Globalization;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Represents the size of a file in bytes, providing the necessary formatting methods to allow
    /// converting to an appropriate string representation of a given unit size (e.g. as Bytes, Megabytes,
    /// Gigabytes etc.).
    /// </summary>
    public struct FileSize : IFormattable
    {
        private const string FileSystemFormatSpecifier = "FS";

        private static readonly UnitSize ByteUnit = new UnitSize { UnitValue = Math.Pow(2, 0), Suffix = "B", FormatModifier = "N0" };
        private static readonly UnitSize KilobyteUnit = new UnitSize { UnitValue = Math.Pow(2, 10), Suffix = "KB", FormatModifier = "N0" };
        private static readonly UnitSize MegabyteUnit = new UnitSize { UnitValue = Math.Pow(2, 20), Suffix = "MB", FormatModifier = "N2" };
        private static readonly UnitSize GigabyteUnit = new UnitSize { UnitValue = Math.Pow(2, 30), Suffix = "GB", FormatModifier = "N2" };
        private static readonly UnitSize TerabyteUnit = new UnitSize { UnitValue = Math.Pow(2, 40), Suffix = "TB", FormatModifier = "N2" };
        private static readonly UnitSize PetabyteUnit = new UnitSize { UnitValue = Math.Pow(2, 50), Suffix = "PB", FormatModifier = "N2" };

        private static readonly UnitSize[] Units =
        {
            ByteUnit,
            KilobyteUnit,
            MegabyteUnit,
            GigabyteUnit,
            TerabyteUnit,
            PetabyteUnit,
        };

        private readonly UnitSize unitSize;
        private readonly double units;

        private FileSize(UnitSize unitSize, double units)
        {
            Guard.NotNull(nameof(unitSize), unitSize);
            Guard.GreaterThanOrEqual("units", units, 0);

            this.unitSize = unitSize;
            this.units = units;
        }

        /// <summary>
        /// Gets the number of bytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Bytes => unitSize.ConvertTo(ByteUnit, units);

        /// <summary>
        /// Gets the number of kilobytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Kilobytes => unitSize.ConvertTo(KilobyteUnit, units);

        /// <summary>
        /// Gets the number of megabytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Megabytes => unitSize.ConvertTo(MegabyteUnit, units);

        /// <summary>
        /// Gets the number of gigabytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Gigabytes => unitSize.ConvertTo(GigabyteUnit, units);

        /// <summary>
        /// Gets the number of terabytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Terabytes => unitSize.ConvertTo(TerabyteUnit, units);

        /// <summary>
        /// Gets the number of petabytes this <see cref="FileSize" /> represents.
        /// </summary>
        public double Petabytes => unitSize.ConvertTo(PetabyteUnit, units);

        /// <summary>
        /// Compares the two objects to determine if they contain the same number of bytes.
        /// </summary>
        /// <param name="leftTerm">
        /// The object to compare to.
        /// </param>
        /// <param name="rightTerm">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// True if the two values are the same.
        /// </returns>
        public static bool operator ==(FileSize leftTerm, FileSize rightTerm)
        {
            // Return true if the fields match:
            return leftTerm.Bytes == rightTerm.Bytes;
        }

        /// <summary>
        /// Compares the two objects to determine if they contain a different number of bytes.
        /// </summary>
        /// <param name="leftTerm">
        /// The object to compare to.
        /// </param>
        /// <param name="rightTerm">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// True if the two values are different.
        /// </returns>
        public static bool operator !=(FileSize leftTerm, FileSize rightTerm)
        {
            return !(leftTerm == rightTerm);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of bytes.
        /// </summary>
        /// <param name="bytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromBytes(double bytes)
        {
            Guard.GreaterThanOrEqual("bytes", bytes, 0);

            return new FileSize(ByteUnit, bytes);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of kilobytes.
        /// </summary>
        /// <param name="kilobytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromKilobytes(double kilobytes)
        {
            Guard.GreaterThanOrEqual("kilobytes", kilobytes, 0);

            return new FileSize(KilobyteUnit, kilobytes);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of megabytes.
        /// </summary>
        /// <param name="megabytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromMegabytes(double megabytes)
        {
            Guard.GreaterThanOrEqual("megabytes", megabytes, 0);

            return new FileSize(MegabyteUnit, megabytes);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of gigabytes.
        /// </summary>
        /// <param name="gigabytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromGigabytes(double gigabytes)
        {
            Guard.GreaterThanOrEqual("gigabytes", gigabytes, 0);

            return new FileSize(GigabyteUnit, gigabytes);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of petabytes.
        /// </summary>
        /// <param name="petabytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromPetabytes(double petabytes)
        {
            Guard.GreaterThanOrEqual("petabytes", petabytes, 0);

            return new FileSize(PetabyteUnit, petabytes);
        }

        /// <summary>
        /// Creates a new <see cref="FileSize"/> instance that represents the given number
        /// of terabytes.
        /// </summary>
        /// <param name="terabytes">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new <see cref="FileSize"/> instance.
        /// </returns>
        public static FileSize FromTerabytes(double terabytes)
        {
            Guard.GreaterThanOrEqual("terabytes", terabytes, 0);

            return new FileSize(TerabyteUnit, terabytes);
        }

        /// <summary>
        /// Determines if the specified object contains the same number of bytes as the current object.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current instance.
        /// </param>
        /// <returns>
        /// True if the two values are the same.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is FileSize size))
            {
                return false;
            }

            // Return true if the fields match:
            return size.Bytes == Bytes;
        }

        /// <summary>
        /// Returns the HashCode.
        /// </summary>
        /// <returns>The hash code of this file size.</returns>
        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this file size, using the 'File System'
        /// format that will return a representation in the largest unit size
        /// appropriate (e.g. 1023B -&gt; "1023B", 1024B -&gt; "1KB").
        /// </summary>
        /// <returns>
        /// A string representation of this file size.
        /// </returns>
        public override string ToString()
        {
            return ToString(FileSystemFormatSpecifier, null);
        }

        /// <summary>
        /// Returns a string representation of this file size, using the provided format modifier
        /// and provider.
        /// </summary>
        /// <remarks>
        /// The format modifier either specifies only a unit size (e.g. 'B', 'KB', 'MB', or the special 'FS') or
        /// a number formatter plus unit size, separated by a colon (e.g. 'N2:GB' for a gigabyte representation
        /// to 2 decimal places).
        /// </remarks>
        /// <param name="format">The format to use.</param>
        /// <param name="formatProvider">A format provider which may contain a custom formatter to use instead
        /// of the default provided by this method.</param>
        /// <returns>A string representation of this file size.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider?.GetFormat(typeof(FileSize)) is ICustomFormatter formatter)
            {
                return formatter.Format(format, this, formatProvider);
            }

            return GetUnitSizeForFormat(format, Bytes).Format(Bytes);
        }

        private static UnitSize GetUnitSizeForFormat(string format, double sizeInBytes)
        {
            // Can specify format with format modifier plus a unit (e.g. 'N4:FS')
            if (format != null && format.Contains(":"))
            {
                var parts = format.Split(':');
                var unit = GetDefaultUnitSizeForFormat(parts[1], sizeInBytes);

                return new UnitSize { FormatModifier = parts[0], Suffix = unit.Suffix, UnitValue = unit.UnitValue };
            }

            return GetDefaultUnitSizeForFormat(format, sizeInBytes);
        }

        private static UnitSize GetDefaultUnitSizeForFormat(string unitFormat, double bytes)
        {
            unitFormat = (unitFormat ?? FileSystemFormatSpecifier).ToUpperInvariant();

            if (unitFormat.Equals(FileSystemFormatSpecifier) || Units.All(u => u.Suffix != unitFormat))
            {
                return Units.Last(m => bytes >= m.UnitValue);
            }

            return GetUnitSize(unitFormat);
        }

        private static UnitSize GetUnitSize(string unitFormat)
        {
            return Units.Single(m => m.Suffix == unitFormat);
        }

        private class UnitSize
        {
            public string FormatModifier { get; set; }

            public string Suffix { get; set; }

            public double UnitValue { get; set; }

            public string Format(double bytes)
            {
                return string.Format(
                                     CultureInfo.CurrentCulture,
                                     "{0:" + FormatModifier + "}{1}",
                                     bytes / UnitValue,
                                     Suffix);
            }

            public double ConvertTo(UnitSize yours, double units)
            {
                if (yours.UnitValue > UnitValue)
                {
                    return units / (yours.UnitValue / UnitValue);
                }

                if (yours.UnitValue < UnitValue)
                {
                    return units * (UnitValue / yours.UnitValue);
                }

                return units;
            }
        }
    }
}
