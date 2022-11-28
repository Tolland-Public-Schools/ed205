/*
Copyright © 2022 Tolland Public Schools
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// Calculate ED205 Title 1 Ready and Math students

using System;
using System.Collections.Generic;
using System.IO;

namespace ed205
{
    class Program
    {
        static void Main(string[] args)
        {
            if (
                args.Length < 1 ||
                args[0] == "--help" ||
                args[0] == "-h" ||
                args[0] == "/?" ||
                args[0] == "/help" ||
                args[0] == "/h"
                ) 
            {
                Console.WriteLine("Usage: ed205.exe [student data file]");
                Console.WriteLine("or drag and drop the student data file onto the app or shortcut.");
                Console.WriteLine("Export file usage format is as follows:");
                Console.WriteLine("File MUST be tab delimited\n" + 
                "Do not include headers\n" + 
                "Columns (in this order):\n" + 
                "S_CT_STU_HT_C.N_InstuctionalSuppService\n" + 
                "S_CT_STU_HT_C.N_TitleIProgramType\n" + 
                "S_CT_STU_HT_C.N_TitleIStudent\n" + 
                "S_CT_STU_HT_C.Transaction_Date\n" + 
                "STUDENTS.Student_Number\n" + 
                "STUDENTS.Last_Name\n" + 
                "STUDENTS.First_Name\n" + 
                "STUDENTS.Grade_Level\n" + 
                "STUDENTS.State_StudentNumber");
                Environment.Exit(0);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found {0}", args[0]);
                Environment.Exit(0);
            }
            
            // Start and run the ed205 report
            Batch b = new Batch(args[0]);
            b.Run();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}