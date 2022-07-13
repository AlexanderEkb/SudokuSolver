namespace Exercize
{
  class CountSort
  {
    StreamWriter writer;
    const int itemsToSort = 10;
    public CountSort(StreamWriter writer)
    {
      this.writer = writer;
    }
    public int[] Sort(ISortable[] Array)
    {
      int length = Array.Length;
      int[] counters = new int[itemsToSort];
      int[,] buckets = new int[length, itemsToSort];
      int[] result = new int[length];

      for(int i=0; i<itemsToSort; i++)
      {
        counters[i] = 0;
      }

      for(int i=0; i<length; i++)
      {
        int value = Array[i].GetValue();
        int index = counters[value];
        buckets[index, value] = i;
        counters[value]++;
      }

      int ptr=0;
      for(int i=0; i<itemsToSort; i++)
      {
        int count = counters[i];
        for(int p=0; p<count; p++)
        {
          result[ptr] = buckets[p, i];
          ptr++;
        }
      }
      return result;
    }
  }
}
