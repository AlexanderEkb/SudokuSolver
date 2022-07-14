namespace Exercize
{
  class Node : ISortable
  {
    public Node l;
    public Node r;
    public Node u;
    public Node d;
    public Row row;
    public int data;
    public int id;
    public string DecodeId()
    {
      return DecodeId(id);
    }
    public static string DecodeId(int id)
    {
      string result = "";
      string[] restrictionsP1 = {"R", "R", "C", "B"};
      string[] restrictionsP2 = {"C", "N", "N", "N"};

      int restrictionId = id / 81;
      int p1 = (id % 81) / 9;
      int p2 = id % 9;

      result = String.Concat(result, restrictionsP1[restrictionId]);
      result = String.Concat(result, (p1+1).ToString());
      result = String.Concat(result, restrictionsP2[restrictionId]);
      result = String.Concat(result, (p2+1).ToString());
      return result;
    }
    public Node(Row row, int id, int data)
    {
      l = this;
      r = this;
      u = this;
      d = this;
      
      this.id = id;
      this.data = data;
      this.row  = row;
    }

    public int GetValue()
    {
      return data;
    }
  }

  class Row
  {
    const int IS_ROOT = -1;
    public Row()
    {
      root = new Node(this, 0, IS_ROOT);
      u = this;
      d = this;
      length = 0;
      id = 0;
    }
    public string DecodeId()
    {
      string result = "";
      int divisor = 324;
      long rem = id;
      for(int i=0;i<4;i++)
      {
        int colId = (int)(rem % divisor);

        result = String.Concat(result, Node.DecodeId(colId));
        result = String.Concat(result, " ");

        rem /= divisor;
      }
      return result;
    }
    public long GetId()
    {
      return id;
    }
    public int GetLength()
    {
      return length;
    }
    public Node GetRoot()
    {
      return root;
    }
    public void Insert(Node node)
    {
      node.l    = root.l;
      node.r    = root;
      root.l.r  = node;
      root.l    = node;
      length++;
      id = (id*324) + node.id;
    }
    public void Remove(Node node)
    {
      node.l.r = node.r;
      node.r.l = node.l;
      length--;
    }
    Node root;
    public Row u;
    public Row d;
    int length;
    long id;
  }

  class IncidenceMatrix
  {
    public IncidenceMatrix(StreamWriter writer)
    {
      this.writer = writer;
      columns = new Row();
    }
    public int GetColCount()
    {
      return columns.GetLength();
    }
    Node GetColHeader(int id)
    {
      Node node = columns.GetRoot();
      int count = columns.GetLength();

      for(int i=0; i<count; i++)
      {
        node = node.r;
        if(node.id == id)
        {
          return node;
        }
      }
      node = new Node(columns, id, 0);
      columns.Insert(node);
      return node;
    }
    public int GetRowCount()
    {
      Row row = columns.d;
      int count = 0;
      for(;row != columns;count++)
      {
        row = row.d;
      }
      return count;
    }
    public void Add(Row row)
    {
      Node root = row.GetRoot();
      Node node = root.r;
      while(node != root)
      {
        Node header = GetColHeader(node.id);
        node.u      = header.u;
        node.d      = header;
        header.u.d  = node;
        header.u    = node;
        header.data++;
        node = node.r;
      }
      row.u = columns.u;
      row.d = columns;
      columns.u.d = row;
      columns.u = row;
    }
    public void Insert(Row[] rows)
    {
      int count = rows.Length;
      for(int i=0; i<count; i++)
      {
        Row row = rows[i];
        Node root = row.GetRoot();
        Node node = root.r;
        for(int n=0; node != root; n++) /* 'Less or equal' to have the root processed as well */
        {
          node.u.d = node;
          node.d.u = node;
          Node header = GetColHeader(node.id);
          header.data++;
          node = node.r;
        }
        row.u.d = row;
        row.d.u = row;
      }
    }
    public Row[] RemoveIntersections(Row row)
    {
      Row[] scratchpad = new Row[36];
      int counter = 0;
      Node root = row.GetRoot();
      Node node = root.r;                                         /* First node after the root */
      
      while(node != root)                                         /* Outer loop iterates horizontally over the nodes in selected row */
      {
        Node columnHeader = GetColHeader(node.id);
        Node cross = columnHeader.d;

        while(cross != columnHeader)                              /* Inner loop iterates vertically over the intersecting rows */
        {
          Node next = cross.d;
          bool mayBeRemoved = (cross != node);
          if(mayBeRemoved)
          {
            Row r = RemoveRow(cross);
            scratchpad[counter] = r;
            counter++;
          }
          cross = next;
        }
        node = node.r;
      }

      Row[] result = new Row[counter];
      for(int i=0; i<counter; i++)
      {
        result[i] = scratchpad[i];
      }
      return result;
    }
    Row RemoveRow(Node anyNode)
    {
      Row row = anyNode.row;

      Node root = row.GetRoot();
      Node node = root.r;
      while(node != root)
      {
        Node header = GetColHeader(node.id);
        header.data--;
        node.u.d  = node.d;
        node.d.u  = node.u;
        node = node.r;
      }
      row.u.d = row.d;
      row.d.u = row.u;
      return row;
    }
    public Row columns;
    StreamWriter writer;
  }
}