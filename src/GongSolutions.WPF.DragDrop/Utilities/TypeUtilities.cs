using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace GongSolutions.Wpf.DragDrop.Utilities
{
    public static class TypeUtilities
    {
        public static IEnumerable CreateDynamicallyTypedList(IEnumerable source)
        {
            var type = GetCommonBaseClass(source);

            var listType = typeof(List<>).MakeGenericType(type);
            var addMethod = listType.GetMethod("Add");

            var list = listType.GetConstructor(Type.EmptyTypes)?.Invoke(null);

            foreach (var o in source)
            {
                addMethod.Invoke(list, new[] { o });
            }

            return list as IEnumerable;
        }

        public static Type GetCommonBaseClass(IEnumerable e)
        {
            var types = e.Cast<object>().Select(o => o.GetType()).ToArray();
            return GetCommonBaseClass(types);
        }

        public static Type GetCommonBaseClass(Type[] types)
        {
            if (types.Length == 0)
            {
                return typeof(object);
            }

            var ret = types[0];

            for (var i = 1; i < types.Length; ++i)
            {
                if (types[i].IsAssignableFrom(ret))
                {
                    ret = types[i];
                }
                else
                {
                    // This will always terminate when ret == typeof(object)
                    while (ret is { } && !ret.IsAssignableFrom(types[i]))
                    {
                        ret = ret.BaseType;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the enumerable as list.
        /// If enumerable is an ICollectionView then it returns the SourceCollection as list.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>Returns a list.</returns>
        public static IList TryGetList(this IEnumerable enumerable)
        {
            if (enumerable is ICollectionView collectionView)
            {
                return collectionView.SourceCollection as IList;
            }

            if (enumerable is IList list)
            {
                return list;
            }

            return enumerable?.OfType<object>().ToList();
        }

        /// <summary>
        /// Checks if the given collection is a ObservableCollection&lt;&gt;
        /// </summary>
        /// <param name="collection">The collection to test.</param>
        /// <returns>True if the collection is a ObservableCollection&lt;&gt;</returns>
        public static bool IsObservableCollection([CanBeNull] this IList collection)
        {
            return collection != null && IsObservableCollectionType(collection.GetType());
        }

        private static bool IsObservableCollectionType([CanBeNull] Type type)
        {
            if (type is null || !typeof(IList).IsAssignableFrom(type))
            {
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {
                return true;
            }

            return IsObservableCollectionType(type.BaseType);
        }

        /// <summary>
        /// Checks if both collections are the same ObservableCollection&lt;&gt;
        /// </summary>
        /// <param name="collection1">The first collection to test.</param>
        /// <param name="collection2">The second collection to test.</param>
        /// <returns>True if both collections are the same ObservableCollection&lt;&gt;</returns>
        public static bool IsSameObservableCollection(this IList collection1, IList collection2)
        {
            return collection1 != null
                   && ReferenceEquals(collection1, collection2);
                   //&& collection1.IsObservableCollection();
        }

        /// <summary>
        /// Tries to get an ordered list from the given enumerable.
        /// If the enumerable is an ICollectionView, it returns the default view of the collection as a list of type T.
        /// If the enumerable is already an IList, it returns the same list.
        /// Otherwise, it converts the enumerable to a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="enumerable">The enumerable to convert to a list.</param>
        /// <returns>A list of type T.</returns>
        public static IList TryGetDefaultViewList<T>(this IEnumerable enumerable)
        {
            if (enumerable is ICollectionView collectionView)
            {
                return CollectionViewSource.GetDefaultView(collectionView).Cast<T>().ToList();
            }

            if (enumerable is IList list)
            {
                return list;
            }

            return enumerable?.OfType<object>().ToList();
        }

        /// <summary>
        /// Checks if both collections are of the same type.
        /// </summary>
        /// <param name="collection1">The first collection to compare.</param>
        /// <param name="collection2">The second collection to compare.</param>
        /// <returns>True if both collections are of the same type and have the same count.</returns>
        public static bool IsSameCollectionType(this IList collection1, IList collection2)
        {
            return collection1 != null
                   && collection1.GetType() == collection2.GetType()
                   && collection1.Count == collection2.Count;
        }
    }
}