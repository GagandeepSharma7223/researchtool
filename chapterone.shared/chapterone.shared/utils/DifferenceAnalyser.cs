using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chapterone.shared.utils
{
    /// <summary>
    /// Models additions and removals when comparing two lists
    /// </summary>
    public class Difference<T>
    {
        /// <summary>
        /// List of added objects
        /// </summary>
        public IEnumerable<T> Added { get; set; } = new List<T>();


        /// <summary>
        /// List of removed objects
        /// </summary>
        public IEnumerable<T> Removed { get; set; } = new List<T>();
    }


    /// <summary>
    /// Class for analysing the differences between two lists
    /// </summary>
    public class DifferenceAnalyser<T>
    {
        public static Difference<T> ProcessChanges(IEnumerable<T> newIds, IEnumerable<T> currentIds)
        {
            var processor = new DifferenceAnalyser<T>(newIds, currentIds);

            return processor.Process();
        }

        private IEnumerable<T> _listA;
        private IEnumerable<T> _listB;


        /// <summary>
        /// Constructor
        /// </summary>
        private DifferenceAnalyser(IEnumerable<T> listA, IEnumerable<T> listB)
        {
            _listA = listA ?? new List<T>();
            _listB = listB ?? new List<T>();
        }


        /// <summary>
        /// Process the friend lists
        /// </summary>
        public Difference<T> Process()
        {
            return new Difference<T>()
            {
                Added = _listA.Except(_listB).ToList(),
                Removed = _listB.Except(_listA).ToList()
            };
        }
    }
}
