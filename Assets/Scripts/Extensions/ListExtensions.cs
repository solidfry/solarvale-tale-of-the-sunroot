using System;
using System.Collections.Generic;
namespace Extensions
{
    /// <summary>
    ///   Extension methods for List
    /// 
    /// </summary>
    public static class ListExtensions
    {
        private static Random _rng;
        
        /// <summary>
        /// Shuffles the elements in the list using Durstenfeld's algorithm of the Fisher-Yates shuffle algorithm.
        /// This method modifies the original list in place, ensuring that the elements are shuffled and each permutation is equally likely
        /// and returns the shuffled list for method chaining.
        /// </summary>
        /// <param name="list">The list to be shuffled.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <returns>The shuffled list.</returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
           if (_rng == null)
               _rng = new Random();
           int count = list.Count;
           while (count > 1)
           {
               --count;
               int index = _rng.Next(count + 1);
               (list[index], list[count]) = (list[count], list[index]);
           }

           return list;
        }
    }
}