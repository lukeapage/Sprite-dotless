using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless
{
    public static class Utilities
    {
        /// <summary>
        /// Sorts the specified <see cref="IList{T}"/> using the insertion
        /// sort algorithm and the specified <see cref="Comparison{T}"/>
        /// method.</summary>
        /// <typeparam name="T">
        /// The type of all elements in the specified .
        /// </typeparam>
        /// 
        /// The <see cref="IList{T}"/> to sort.
        /// 
        /// The <see cref="Comparison{T}"/> method that defines the sorting
        /// order.
        /// <exception cref="ArgumentNullException">
        ///  or  is a null
        /// reference.</exception>
        /// <remarks>
        /// <b>InsertionSort</b> sorts the specified 
        /// using the insertion sort algorithm. Unlike the Quicksort
        /// algorithms provided by the standard library, this is a stable
        /// algorithm. That is, the relative order of any two elements for
        /// which the specified  method returns
        /// zero remains unchanged.</remarks>
        public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            for (int j = 1; j < list.Count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                    list[i + 1] = list[i];

                list[i + 1] = key;
            }
        }
    }
}
