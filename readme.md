Sudoku solver
=============
This program has been created as a course project. It uses an algorithm of 'dancing links' to solve Sudoku puzzles.
It is quite a subject to be optimized, but it works. On Core i7-7700K@4.2GHz all included tests are succeeding in ca. 5 minutes.

Tests
-----
There are some tests in, yes, 'tests' folder. Each test requires two files to be run - test.N.in and test.N.out, 
where N is a decimal number. Test numbers are zero-based and must be assigned increasingly, one by one. If program can't find the next file it terminates.
'test.N.in' file must contain something like this:

'6*253***4  '
'5**8*7***  '
'*31******  '
'**5*6***8  '
'*28***5*1  '
'7********  '
'3*****7**  '
'***4*****  '
'*8***3*96  '

where unknown numbers are replaced by stars. 'test.N.out' must contain one of the strings from the list below:

ERROR - initial position is wrong
FAIL - can't solve
SUCCESS

depending on what you expect from the program on the corresponding .in file.
After each test a 'test.N.sol' file is generated. It contains the result of a test run and solution, if found.
