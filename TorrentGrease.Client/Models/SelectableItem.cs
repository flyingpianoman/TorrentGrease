using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentGrease.Client.Models
{
    public class SelectableItem<T>
    {
        public bool IsSelected { get; set; }
        public T Item { get; set; }
        public SelectableItem(T item)
        {
            Item = item;
        }
    }
}
