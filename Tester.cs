using System.Globalization;

namespace Exercize

{
  internal class Tester
  {
    private ITask task;
    private string path;
    private string[] data = {};
    // private string expect = "";
    // private string actual = "";
    public Tester(ITask task, string path)
    {
      this.task = task;
      this.path = path;
    }
    public void RunAllTests()
    {
      int count = 0;
      while(true)
      {
        bool result = RunTest(count);
        if(!result)
        {
          break;
        }
        count++;
      }
    }

    public bool RunTest(int count)
    {
      string inFile = $"{path}test.{count}.in";
      string outFile = $"{path}test.{count}.out";
      string solFile = $"{path}test.{count}.sol";
      if(!File.Exists(inFile) || !File.Exists(outFile))
      {
        return false;
      }
      if(File.Exists(solFile))
      {
        File.Delete(solFile);
      }
      RunTest(inFile, outFile, solFile, count);
      return true;
    }
    bool RunTest(string inFile, string outFile, string solutionFile, int count)
    {
      try
      {
        Console.Write($"Test #{count} - ");
        string[] data = File.ReadAllLines(inFile);
        string expect = File.ReadAllText(outFile);
        string actual = task.Run(data, solutionFile);
        bool result = actual.Equals(expect);
        if(result)
        {
          Console.WriteLine($"PASS");
        }
        else
        {
          Console.WriteLine($"FAIL ({expect} != {actual})");
        }
        return actual == expect;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return false;
      }
    }
  }
}