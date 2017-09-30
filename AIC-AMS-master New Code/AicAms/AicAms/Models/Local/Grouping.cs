using System.Collections.Generic;
using System.Collections.ObjectModel;
using AicAms.Extensions;

namespace AicAms.Models.Local
{
    public class Grouping<T, K> : ObservableCollection<T>
    {
        public K Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping{T,K}"/> class.
        /// </summary>
        /// <param name="items">A collection of items of type T</param>
        /// <param name="key">A key of type K.</param>
        public Grouping(IEnumerable<T> items, K key)
        {
            Key = key;
            Items.AddRange(items);
        }
    }
}
