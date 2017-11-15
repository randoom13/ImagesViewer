using System.Linq;
using System.Collections.Generic;
namespace ImagesViewer.Controls
{
    public class Range
    {
        public int Index { get; private set; }
        public int Count { get; private set; }

        public Range(int index, int count)
        {
            Count = count;
            Index = index;
        }

        private static int GetComputeHashCode(params object[] items)
        {
          return items.Select(item =>  item.GetHashCode())
          .Aggregate(23, (hash, itemHash) => hash * 31 + itemHash);
        }

        public override int GetHashCode()
        {
            return GetComputeHashCode(new []{ Index, Count });
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Range))
                return false;
            var range = (Range)obj;
            return range.Index == Index && range.Count == Count;
        }
    }
}
