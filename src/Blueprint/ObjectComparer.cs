using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Utilities;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace Blueprint
{
    public static class ObjectComparer
    {
        private static readonly Dictionary<string, object> _noChangesDictionary = new Dictionary<string, object>();

        private static readonly CompareLogic _compareLogic = new CompareLogic(new ComparisonConfig
        {
            MaxDifferences = int.MaxValue,

            CustomComparers = new List<BaseTypeComparer>
            {
                new DateTimeOffsetComparer(RootComparerFactory.GetRootComparer()),
            },

            CompareChildren = true,

            AttributesToIgnore = new List<Type> { typeof(DoNotCompareAttribute) },
        });

        /// <summary>
        /// Given an 'old' object and a 'new' one gets a dictionary of property changes, indicating values that
        /// have been added / updated in the new object that are different from the old.
        /// </summary>
        /// <remarks>
        /// In cases where certain properties should be excluded the `newPropertyPredicate` function should return `false`
        /// to indicate a property should not be included in the comparison.
        /// </remarks>
        /// <param name="oldObject">The 'old' object to check.</param>
        /// <param name="newObject">The 'new' object to check.</param>
        /// <returns>A dictionary of changes between the old and new objects.</returns>
        public static Dictionary<string, object> GetPreviousValues(object oldObject, object newObject)
        {
            if (oldObject == null || newObject == null)
            {
                return _noChangesDictionary;
            }

            var comparison = _compareLogic.Compare(oldObject, newObject);

            if (comparison.AreEqual)
            {
                return _noChangesDictionary;
            }

            return comparison.Differences
                .ToDictionary(d => d.PropertyName.TrimStart('.'), d => d.Object1);
        }

        private class DateTimeOffsetComparer : BaseTypeComparer
        {
            /// <summary>
            /// Constructor that takes a root comparer.
            /// </summary>
            /// <param name="rootComparer"></param>
            public DateTimeOffsetComparer(RootComparer rootComparer)
                : base(rootComparer)
            {
            }

            /// <summary>
            /// Returns true if the type is a simple type.
            /// </summary>
            /// <param name="type1">The type of the first object.</param>
            /// <param name="type2">The type of the second object.</param>
            /// <returns></returns>
            public override bool IsTypeMatch(Type type1, Type type2)
            {
                return ReflectionUtilities.GetUnderlyingTypeIfNullable(type1) == typeof(DateTimeOffset) &&
                       ReflectionUtilities.GetUnderlyingTypeIfNullable(type2) == typeof(DateTimeOffset);
            }

            /// <summary>
            /// Compare two simple types.
            /// </summary>
            public override void CompareType(CompareParms parms)
            {
                if (!Equals(parms.Object1, parms.Object2))
                {
                    this.AddDifference(parms);
                }
            }
        }
    }
}
