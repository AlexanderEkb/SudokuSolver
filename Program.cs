using System.Diagnostics;

namespace Exercize
{
      class Program
    {
        static void Main(string[] args)
        {
          ITask sudoku = new Sudoku();
          Tester tester = new Tester(sudoku, @".\tests\");
          tester.RunAllTests();
          // tester.RunTest(0);
        }
    }
}
