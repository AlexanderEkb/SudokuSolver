namespace Exercize
{
using System.Diagnostics;
  enum Result {OK, WRONG};
  enum MatrixState {SOLVED, DEAD_END, INCOMPLETE_SOLUTION};
  class Constraint
  {
    public int constraint;
    public int p1;
    public int p2;
  }
  class FieldCell
  {
    public int row;
    public int col;
    public int num;
  }
  class Sudoku : ITask
  {
    const int PUZZLE_SIZE           = 9;
    const int CONSTRAINT_COUNT      = 4;
    const int CONSTRAINT_ROW_COL    = 0;
    const int CONSTRAINT_ROW_NUMBER = 1;
    const int CONSTRAINT_COL_NUMBER = 2;
    const int CONSTRAINT_BOX_NUMBER = 3;
    private int[,] field;
    private bool[,] rowColFlags;
    private bool[,] rowNumFlags;
    private bool[,] colNumFlags;
    private bool[,] boxNumFlags;
    private StreamWriter writer;
    int recursionDepth;
    int maxRecursionDepth;
    const int NUMBERS = PUZZLE_SIZE + 1; /* 0..9 */
    IncidenceMatrix matrix;
    public Sudoku()
    {
      recursionDepth = 0;
      maxRecursionDepth = 0;
      field = new int[PUZZLE_SIZE, PUZZLE_SIZE];
      rowNumFlags = new bool[PUZZLE_SIZE, PUZZLE_SIZE];
      colNumFlags = new bool[PUZZLE_SIZE, PUZZLE_SIZE];
      boxNumFlags = new bool[PUZZLE_SIZE, PUZZLE_SIZE];
      rowColFlags = new bool[PUZZLE_SIZE, PUZZLE_SIZE];
      writer = new StreamWriter(Console.OpenStandardOutput());
      matrix = new IncidenceMatrix(new StreamWriter(Console.OpenStandardOutput()));
    }
    Node ChooseColumn()
    {
      CountSort sorter = new CountSort(writer);
      Row headers = matrix.columns;
      int headerCount = headers.GetLength();
      Node[] headerArray = new Node[headerCount];
      Node header = headers.GetRoot().r;
      for(int i=0; header != headers.GetRoot(); i++)
      {
        headerArray[i] = header;
        header = header.r;
      }
      int[] index = sorter.Sort(headerArray);
      int col;
      for(col=0;headerArray[col].data<2;col++);
      return headerArray[col];
    }
    public void Clear()
    {
      int count = PUZZLE_SIZE;
      for(int row=0; row<count; row++)
      {
        for(int col=0; col<count; col++)
        {
          field[row, col] = 0;
        }
      }

      for(int i=0; i<PUZZLE_SIZE; i++)
      {
        for(int num=0; num<PUZZLE_SIZE; num++)
        {
          rowNumFlags[i, num] = true;
          colNumFlags[i, num] = true;
          boxNumFlags[i, num] = true;
          rowColFlags[i, num] = true;
        }
      }
    }
    Constraint DecodeConstraintId(int id)
    {
      Constraint result = new Constraint();
      result.constraint = id / 81;
      result.p1         = (id % 81) / 9;
      result.p2         = id % 9;

      return result;
    }
    FieldCell DecodeRowId(long id)
    {
      FieldCell result = new FieldCell();
      const int count = 4;
      Constraint[] constraints = new Constraint[count];
      int divisor = 324;
      long rem = id;
      for(int i=0;i<count;i++)
      {
        int colId = (int)(rem % divisor);
        constraints[i] = DecodeConstraintId(colId);
        rem /= divisor;
      }
      for(int i=0;i<count;i++)
      {
        if(constraints[i].constraint == CONSTRAINT_ROW_COL)
        {
          result.row = constraints[i].p1;
          result.col = constraints[i].p2;
        }
        else if(constraints[i].constraint == CONSTRAINT_BOX_NUMBER)
        {
          result.num = constraints[i].p2 + 1;
        }
      }
      return result;
    }
    void DecodeSolution()
    {
      Row dummy = matrix.columns;
      Row row = dummy.d;
      int counter = 0;
      writer.WriteLine($"{matrix.GetRowCount()} rows remained in the matrix. Now decoding.");
      while(row != dummy)
      {
        counter++;
        FieldCell cell = DecodeRowId(row.GetId());
        field[cell.row, cell.col] = cell.num;
        row = row.d;
      }
      writer.WriteLine(@"Solution:");
      DrawField();
      writer.WriteLine($"Max recursion depth: {maxRecursionDepth}");
    }
    void DrawField()
    {
      for(int row=0; row<PUZZLE_SIZE; row++)
      {
        for(int col=0; col<PUZZLE_SIZE; col++)
        {
          int num = field[row, col];
          if(num == 0)
          {
            writer.Write(@"∙ ");
          }
          else
          {
            writer.Write($"{num} ");
          }
        }
        writer.WriteLine();
      }

      writer.WriteLine();
    }
    int EncodeConstraintId(int constraint, int p1, int p2)
    {
      /*
       * constraint is one of the values below:
       * CONSTRAINT_ROW_COL
       * CONSTRAINT_ROW_NUMBER
       * CONSTRAINT_COL_NUMBER
       * CONSTRAINT_BOX_NUMBER
       * 
       * 'p1' and 'p2' are, say, some generalized parameters, which meaning depends on 'constraint'.
       * For CONSTRAINT_ROW_COL these values must be treated as row and column respectively;
       * For CONSTRAINT_ROW_NUMBER - as row and number
       * For CONSTRAINT_COL_NUMBER - as column and number
       * For CONSTRAINT_BOX_NUMBER - as a 'box index' and number.
       */
      return constraint*PUZZLE_SIZE*PUZZLE_SIZE + p1*PUZZLE_SIZE + p2;
    }

    void InitializeMatrix()
    {
      matrix = new IncidenceMatrix(writer);
      for(int r=0; r<PUZZLE_SIZE; r++)
      {
        for(int c=0; c<PUZZLE_SIZE; c++)
        {
          int boxRow = r / 3;
          int boxCol = c / 3;
          int box = (boxRow * 3) + boxCol;

          for(int n=0;n<PUZZLE_SIZE; n++)
          {
            Row newRow = new Row();
            bool addRow = (rowNumFlags[r, n]) && (colNumFlags[c, n]) && (boxNumFlags[box, n]) && (rowColFlags[r, c]);
            if(addRow)
            {
              newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_ROW_COL, r, c), 0));
              newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_ROW_NUMBER, r, n), 0));
              newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_COL_NUMBER, c, n), 0));
              newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_BOX_NUMBER, box, n), 0));
              matrix.Add(newRow);
            }
          }
        }
      }
    }
    MatrixState IsSolved()
    {
      if(matrix.GetRowCount() > 0)
      {
        Node root = matrix.columns.GetRoot();
        Node node = root.r;
        while(node != root)
        {
          int nodesInColumn = node.data;
          if(nodesInColumn == 0)
          {
            return MatrixState.DEAD_END;
          }
          else if(nodesInColumn > 1)
          {
            return MatrixState.INCOMPLETE_SOLUTION;
          }
          node = node.r;
        }
      }
      return MatrixState.SOLVED;
    }
    Result Iterate()
    {
      recursionDepth++;
      if(recursionDepth > maxRecursionDepth)
      {
        maxRecursionDepth = recursionDepth;
      }
      Result result = Result.WRONG;
      MatrixState state = IsSolved();
      switch(state)
      {
        case MatrixState.SOLVED:
          writer.WriteLine(@"Solved!");
          result = Result.OK;
          break;
        case MatrixState.DEAD_END:
          break;
        default:
          Node colHeader = ChooseColumn();
          Node node = colHeader.d;
          while(node != colHeader)  /* The loop iterates over the rows that include nodes in selected column */
          {
            Row row = node.row;
            Row[] retiringRows = matrix.RemoveIntersections(row);
            if(retiringRows.Length > 0)
            {
              result = Iterate();
              if(result == Result.OK)
              {
                return Result.OK;
              }
            }
            if(retiringRows.Length > 0)
            {
              matrix.Insert(retiringRows);
            }
            node = node.d;
          }
          break;
      }
      recursionDepth--;
      return result;
    }
    Result Load(string[] data)
    {
      writer.WriteLine("Loading");
      Clear();
      for(int row=0; row<PUZZLE_SIZE; row++)
      {
        string str = data[row];
        while(str.Length < PUZZLE_SIZE)
        {
          str.Append('*');
        }
        for(int col=0; col<PUZZLE_SIZE; col++)
        {
          int boxRow = row / 3;
          int boxCol = col / 3;
          int box = boxRow * 3 + boxCol;

          bool rowOk = true;
          bool colOk = true;
          bool boxOk = true;
          bool numOk = true;

          string c = str[col].ToString();
          int num = 0;
          if(c.Equals("*"))
          {
            writer.Write(@"∙ ");
          }
          else
          {
            num = Convert.ToInt32(c);
            writer.Write($"{num} ");
            rowOk = rowNumFlags[row, num-1] == true;
            colOk = colNumFlags[col, num-1] == true;
            boxOk = boxNumFlags[box, num-1] == true;
            numOk = rowColFlags[row, col] == true;
          }
          bool allOk = rowOk && colOk && boxOk && numOk;
          if(allOk)
          {
            field[row, col] = num;
            if (num != 0) rowNumFlags[row, num-1] = false;
            if (num != 0) colNumFlags[col, num-1] = false;
            if (num != 0) boxNumFlags[box, num-1] = false;
            if (num != 0) rowColFlags[row, col] = false;
          }
          else
          {
            writer.WriteLine(@"<<ERROR!");
            return Result.WRONG;
          }
        }
        writer.WriteLine();
      }
      writer.WriteLine();
      writer.WriteLine("Loaded");
      return Result.OK;
    }
    public string Run(string[] data, string solutionFile)
    {
      writer = new StreamWriter(solutionFile);
      writer.AutoFlush = true;
      writer.WriteLine("Ok, let's go!");
      if(Load(data) != Result.OK)
      {
        string res = "ERROR - initial position is wrong";
        writer.WriteLine(res);
        return res;
      }

      if(Solve() != Result.OK)
      {
        string res = "FAIL - can't solve";
        writer.WriteLine(res);
        return res;
      }
      string result = "SUCCESS";
      writer.WriteLine(result);
      writer.Dispose();
      return result;
    }

    Result Solve()
    {
      Result result = Result.WRONG;
      writer.WriteLine(@"Solving...");
      recursionDepth = 0;
      maxRecursionDepth = 0;
      Stopwatch sw = new Stopwatch();
      sw.Start();
      InitializeMatrix();
      writer.WriteLine($"Incidence matrix of {matrix.GetRowCount()} rows has been built.");
      Result solutionFound = Iterate();
      if(solutionFound == Result.OK)
      {
        DecodeSolution();
        result = Result.OK;
      }
      else
      {
        writer.WriteLine(@"No solution");
        result = Result.WRONG;
      }
      sw.Stop();
      string timeElapsed = sw.ElapsedMilliseconds.ToString();
      writer.WriteLine($"Time elapsed: {timeElapsed} ms.");
      return result;
    }
  }
}