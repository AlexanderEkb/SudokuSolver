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
    static int recursionDepth;
    const int NUMBERS = PUZZLE_SIZE + 1; /* 0..9 */
    IncidenceMatrix matrix;
    public Sudoku()
    {
      recursionDepth = 0;
      field = new int[PUZZLE_SIZE, PUZZLE_SIZE];
      writer = new StreamWriter(Console.OpenStandardOutput());
      matrix = new IncidenceMatrix(0, new StreamWriter(Console.OpenStandardOutput()));
    }
    Result CheckCol(int Col)
    {
      int[] flags = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

      for(int row=0; row<PUZZLE_SIZE; row++)
      {
        int n = field[row, Col];
        if(flags[n] == 0)
          flags[n] = 1;
        else if(n == 0)
          flags[n]++;
        else
          return Result.WRONG;
      }
      return Result.OK;
    }
    Result CheckRow(int row)
    {
      int[] flags = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

      for(int col=0; col<PUZZLE_SIZE; col++)
      {
        int n = field[row, col];
        if(flags[n] == 0)
          flags[n] = 1;
        else if(n == 0)
          flags[n]++;
        else
          return Result.WRONG;
      }
      return Result.OK;
    }
    int ChooseColumn()
    {
      int[] index = CreateIndex();
      int col;
      for(col=0;matrix.colHeaders[col].data<2;col++);

      return col;
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
    }
    int[] CreateIndex()
    {
      CountSort sorter = new CountSort(writer);
      int[] index = sorter.Sort(matrix.colHeaders);

      return index;
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
          result.num = constraints[i].p2;
        }
      }
      return result;
    }
    void DecodeSolution()
    {
      Row headers = matrix.colHeaders[0].row;
      Row row = headers;
      int counter = 0;
      writer.WriteLine($"{matrix.GetRowCount()} rows remained in the matrix. Now decoding.");
      while(row != headers)
      {
        row = row.d;
        counter++;
        writer.WriteLine($"decoding row {counter} - {row.DecodeId()}");
        FieldCell cell = DecodeRowId(row.GetId());
        field[cell.row, cell.col] = cell.num;
      }
      writer.WriteLine(@"Solution:");
      DrawField();
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
      for(int r=0; r<PUZZLE_SIZE; r++)
      {
        for(int c=0; c<PUZZLE_SIZE; c++)
        {
          int boxRow = r / 3;
          int boxCol = c / 3;
          int box = (boxRow * 3) + boxCol;

          for(int n=0;n<PUZZLE_SIZE; n++)
          {
            bool isPredefined = (field[r, c] == (n+1)); 
            Row newRow = new Row(isPredefined);
            newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_ROW_COL, r, c)));
            newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_ROW_NUMBER, r, n)));
            newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_COL_NUMBER, c, n)));
            newRow.Insert(new Node(newRow, EncodeConstraintId(CONSTRAINT_BOX_NUMBER, box, n)));
            matrix.Add(newRow);
          }
        }
      }

      // for(int i=0; i<columnCount; i++)
      // {
      //   for(int j=i+1; j<columnCount; j++)
      //   {
      //     if(matrix.colHeaders[i].data == matrix.colHeaders[j].data)
      //     {
      //       writer.WriteLine($"ALARM! Cols {i} and {j} have the same restriction code: {matrix.colHeaders[i].data}");
      //     }
      //   }
      // }
    }
    MatrixState IsSolved()
    {
      for(int i=0; i<columnCount; i++)
      {
        int nodesInColumn = matrix.colHeaders[i].data;
        if(nodesInColumn == 0)
        {
          return MatrixState.DEAD_END;
        }
        else if(nodesInColumn > 1)
        {
          return MatrixState.INCOMPLETE_SOLUTION;
        }
      }
      return MatrixState.SOLVED;
    }
    Result Iterate()
    {
      recursionDepth++;
      writer.WriteLine($"Iterate() entry ---------- Recursion depth: {recursionDepth}");
      Result result = Result.WRONG;
      MatrixState state = IsSolved();
      switch(state)
      {
        case MatrixState.SOLVED:
          writer.WriteLine(@"Solved!");
          result = Result.OK;
          break;
        case MatrixState.DEAD_END:
          writer.WriteLine(@"Roll back");
          break;
        default:
          int col = ChooseColumn();
          Node colHeader = matrix.colHeaders[col];
          Node node = colHeader.d;
          while(node != colHeader)  /* The loop iterates over the rows that include nodes in selected column */
          {
            Row row = node.row;
            Row[] retiringRows = matrix.RemoveIntersections(row);
            if(retiringRows.Length > 0)
            {
              writer.WriteLine($"Rows left: {matrix.GetRowCount()}, gone: {retiringRows.Length}");
              result = Iterate();
              if(result == Result.OK)
              {
                return Result.OK;
              }
            }
            else
            {
              writer.WriteLine(@"No rows could be removed. Skip.");
            }
            if(retiringRows.Length > 0)
            {
              matrix.Insert(retiringRows);
              writer.WriteLine($"Insert. Rows become: {matrix.GetRowCount()}, added: {retiringRows.Length}");
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
          string c = str[col].ToString();
          int num = 0;
          if(!c.Equals("*"))
          {
            num = Convert.ToInt32(c);
          }
          if(num == 0)
          {
            writer.Write(@"∙ ");
          }
          else
          {
            writer.Write($"{num} ");
          }
          Result result = Put(row, col, num);
          if(result != Exercize.Result.OK)
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
    public Result Put(int row, int col, int num)
    {
      int prev = field[row, col];
      field[row, col] = num;
      Result rowOk = CheckRow(row);
      Result colOk = CheckCol(col);
      Result boxOk = CheckBox(row, col);
      bool somethingWentWrong =
      (rowOk != Result.OK) ||
      (colOk != Result.OK) ||
      (boxOk != Result.OK);
      
      if (somethingWentWrong)
      {
        field[row, col] = prev;
        return Result.WRONG;
      }
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
      Stopwatch sw = new Stopwatch();
      sw.Start();
      columnCount = CONSTRAINT_COUNT * PUZZLE_SIZE * PUZZLE_SIZE; /* Total count of all applicable constraints */
      matrix = new IncidenceMatrix(columnCount, writer);
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
    Result CheckBox(int row, int col)
    {
      int[] flags = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
      int boxRow = row / 3;
      int fromRow = boxRow * 3;
      int toRow = fromRow + (PUZZLE_SIZE / 3);
      int boxCol = col / 3;
      int fromCol = boxCol * 3;
      int toCol = fromCol + (PUZZLE_SIZE / 3);
      for(int nCol=fromCol; nCol<toCol; nCol++)
      {
        for(int nRow=fromRow; nRow<toRow; nRow++)
        {
          int n = field[nRow, nCol];
          if(flags[n] == 0)
            flags[n] = 1;
          else if(n == 0)
            flags[n]++;
          else
            return Result.WRONG;
        }
      }
      return Result.OK;
    }
    private int[,] field;
    private int columnCount;
    private StreamWriter writer;
    const int PUZZLE_SIZE           = 9;
    const int CONSTRAINT_COUNT      = 4;
    const int CONSTRAINT_ROW_COL    = 0;
    const int CONSTRAINT_ROW_NUMBER = 1;
    const int CONSTRAINT_COL_NUMBER = 2;
    const int CONSTRAINT_BOX_NUMBER = 3;

  }
}