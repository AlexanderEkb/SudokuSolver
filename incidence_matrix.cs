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
    public static string DecodeId(int id)
    {
      string result = "";
      string[] restrictionsP1 = {"R", "R", "C", "B"};
      string[] restrictionsP2 = {"C", "N", "N", "N"};

      int restrictionId = id / 81;
      int p1 = (id % 81) / 9;
      int p2 = id % 9;

      result = String.Concat(result, restrictionsP1[restrictionId]);
      result = String.Concat(result, p1.ToString());
      result = String.Concat(result, restrictionsP2[restrictionId]);
      result = String.Concat(result, p2.ToString());
      return result;
    }
    public Node(Row row, int data)
    {
      l = this;
      r = this;
      u = this;
      d = this;

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
    public Row(bool isPredefined)
    {
      root = new Node(this, IS_ROOT);
      u = this;
      d = this;
      this.isPredefined = isPredefined;
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
    public int GetNodeData(int index)
    {
      Node wanted = root;
      for(int i=0; i<=index; i++)
      {
        wanted = wanted.r;
        if(wanted == root)
        {
          return -1;
        }
      }
      return wanted.data;
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
      id = (id*324) + node.data;
    }
    public bool IsPredefined()
    {
      return isPredefined;
    }
    Node root;
    bool isPredefined;
    public Row u;
    public Row d;
    int length;
    long id;
  }

  class IncidenceMatrix
  {
    public IncidenceMatrix(int columnCount, StreamWriter writer)
    {
      this.columnCount = columnCount;
      this.rowCount = 0;
      this.writer = writer;
      dummyRow = new Row(true);
      colHeaders = new Node[columnCount];
      for(int i=0; i<columnCount; i++)
      {
        colHeaders[i] = new Node(dummyRow, 0);
      }
    }
    public void Add(Row row)
    {
      Node root = row.GetRoot();
      Node node = root.r;
      while(node != root)
      {
        Node header = colHeaders[node.data];
        node.u      = header.u;
        node.d      = header;
        header.u.d  = node;
        header.u    = node;
        header.data++;
        node = node.r;
      }
      row.u = dummyRow.u;
      row.d = dummyRow;
      dummyRow.u.d = row;
      dummyRow.u = row;
      rowCount++;
    }
    public int GetRowCount()
    {
      return rowCount;
    }
    public void Insert(Row[] rows)
    {
      int count = rows.Length;
      for(int i=0; i<count; i++)
      {
        Row row = rows[i];
        int nodeCount = row.GetLength();
        Node node = row.GetRoot();
        for(int n=0; n<=nodeCount; n++) /* 'Less or equal' to have the root processed as well */
        {
          node = node.r;
          node.u.d = node;
          node.d.u = node;
        }
        row.u.d = row;
        row.d.u = row;
        rowCount++;
      }
    }
    public Row[] RemoveIntersections(Row row)
    {
      writer.WriteLine($"RemoveIntersections, row #{row.DecodeId()}");
      Row[] scratchpad = new Row[36];
      int counter = 0;
      Node root = row.GetRoot();
      Node node = root.r;                                         /* First node after the root */
      
      while(node != root)                                         /* Outer loop iterates horizontally over the nodes in selected row */
      {
        Node columnHeader = colHeaders[node.data];
        Node cross = columnHeader.d;

        Node foo = columnHeader.d;
        writer.Write($" Column {node.data} {{");
        int cnt = 0;
        while (foo != columnHeader)
        {
          writer.Write($"({foo.row.DecodeId()}) ");
          cnt++;
          foo = foo.d;
        }
        writer.WriteLine($"}} {cnt} entries found.");
        while(cross != columnHeader)                              /* Inner loop iterates vertically over the intersecting rows */
        {
          Node next = cross.d;
          writer.Write($"  Row #{cross.row.DecodeId()} ");
          bool isPredefined = cross.row.IsPredefined();
          bool isChosen = (cross == node);
          if(isPredefined)
          {
            writer.WriteLine(@"is predefined. Can't remove.");
          }
          else if(isChosen)
          {
            writer.WriteLine(@"is chosen one.");
          }
          else
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
      writer.WriteLine(@"RemoveIntersections finished.");
      return result;
    }
    Row RemoveRow(Node anyNode)
    {
      Row row = anyNode.row;

      Node root = row.GetRoot();
      Node node = root;
      while(true)
      {
        node = node.r;
        node.u.d  = node.d;
        node.d.u  = node.u;
        if(node != root)
        {
          int col = node.data;
          colHeaders[col].data--;
        }
        else
        { /* Root is processed separately */
          rowCount--;
          writer.WriteLine($" removed");
          return root.row;
        }
      }
    }
    public Node[] colHeaders;
    int columnCount;
    Row dummyRow;
    int rowCount;
    StreamWriter writer;
  }
}