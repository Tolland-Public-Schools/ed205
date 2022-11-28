/*
Copyright © 2022 Tolland Public Schools
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;

// File MUST be tab delimited
// Do not include headers
// TXT Format:
// 0 = S_CT_STU_HT_C.N_InstuctionalSuppService
// 1 = S_CT_STU_HT_C.N_TitleIProgramType
// 2 = S_CT_STU_HT_C.N_TitleIStudent
// 3 = S_CT_STU_HT_C.Transaction_Date
// 4 = STUDENTS.Student_Number
// 5 = STUDENTS.Last_Name
// 6 = STUDENTS.First_Name
// 7 = STUDENTS.Grade_Level
// 8 = STUDENTS.State_StudentNumber


namespace ed205
{
    public class Batch
    {
        private string filePath = "";
        // If students are missing a Title 1 Program Type (this happens somtimes) what should be used?
        private const string _defaultTitle1ProgramType = "1";
        
        public Batch(string filePath)
        {
            this.filePath = filePath;
        }

        // rawStudents includes each transacption exported from PowerSchool.
        // There is will to be more than one rawStudent line for students whos services chagned.
        List<RawStudent> rawStudents = new List<RawStudent>();

        // students includes one entry for each student that received at least one service
        List<Student> students = new List<Student>();

        // Counters for total students receiving services
        int bgpReading = 0;
        int bgpMath = 0;
        int tisReading = 0;
        int tisMath = 0;
        int tmsReading = 0;
        int tmsMath = 0;
        int thsReading = 0;
        int thsMath = 0;

        // Grade cutoffs to determine which school a student should be counted for
        int _tisGradeCutoff = 3;
        int _tmsGradeCutoff = 6;
        int _thsGradeCutoff = 9;
        int _graduatedCutoff = 12;

        public void Run()
        {

            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Running ED205");

            Console.Write("Earliest Date (dd/mm/YYYY) [leave blank for {0}]: ", "7/1/" + (DateTime.Now.Year - 1).ToString());
            string earlyString = Console.ReadLine() ?? "";
            if (earlyString == "")
            {
                earlyString = "7/1/" + (DateTime.Now.Year - 1).ToString();
                Console.WriteLine("Earliest date left blank, using July of last year: {0}", earlyString);
            }

            DateOnly earliest = DateOnly.Parse(earlyString);
            Console.Write("Latest Date (dd/mm/YYYY) [leave blank for {0}]: ","6/30/" + (DateTime.Now.Year).ToString());
            string lateString = Console.ReadLine() ?? "";
            if (lateString == "")
            {
                lateString = "6/30/" + (DateTime.Now.Year).ToString();
                Console.WriteLine("Latest date left blank, using June of this year: {0}", lateString);
            }
            DateOnly latest = DateOnly.Parse(lateString);

            Console.WriteLine("Using cutoff dates: {0} - {1}", earliest, latest);

            Console.WriteLine("Y = keep grade levels the way they are");
            Console.WriteLine("N = subtract 1 from all grade levels");
            Console.Write("Run for this year? (Y/N): ");
            string thisYearString = (Console.ReadLine() ?? "").ToUpper();
            bool thisYear = true;
            if (thisYearString == "N") thisYear = false;

            Console.WriteLine("Running for this year: {0}", thisYear);



            // Read in the tab-delimited file
            try
            {
                // Create a "raw student" for each line. 
                // A "real" student may have multiple "raw student" entries
                foreach (string line in File.ReadLines(filePath))
                {
                    // Console.WriteLine(line);
                    char[] delimiterChars = { '\t' }; ;
                    string[] data = line.Split(delimiterChars);
                    RawStudent rs = new RawStudent();
                    rs.studentNumber = data[4];
                    rs.services = data[0];
                    rs.grade = data[7];
                    rs.date = DateOnly.Parse(data[3]);
                    rs.stateStudentNumber = data[8];
                    rs.title1ProgramType = data[1] ?? "";
                    rs.firstName = data[6];
                    rs.lastName = data[5];
                    rawStudents.Add(rs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Data import exception {0}", e);
            }


            // Create one real student for the raw student entries
            // The "real" student will have "true" for math or reading of any if its raw entries did
            foreach (RawStudent rs in rawStudents)
            {
                // If the transaction date is not within the date range, skip the record

                if (rs.date >= earliest && rs.date <= latest)
                {
                    // Console.WriteLine("{0}: Date in range, using", rs.date);
                }
                else
                {
                    // Console.WriteLine("{0}: Date not in range, skipping", rs.date);
                    continue;
                }


                bool found = false;

                // Update an existing student
                foreach (Student student in students)
                {
                    if (student.studentNumber == rs.studentNumber)
                    {
                        string[] services = rs.services.Split(",");
                        if (services.Contains("1")) student.reading = true;
                        if (services.Contains("2")) student.math = true;
                        if (rs.title1ProgramType != "" && rs.title1ProgramType != null) student.title1ProgramType = rs.title1ProgramType;
                        found = true;
                        break;
                    }
                }
                // Create a new student
                if (!found)
                {
                    Student student = new Student();
                    student.studentNumber = rs.studentNumber;
                    student.stateStudentNumber = rs.stateStudentNumber;
                    student.firstName = rs.firstName;
                    student.lastName = rs.lastName;
                    
                    if (rs.title1ProgramType != "" && rs.title1ProgramType != null) student.title1ProgramType = rs.title1ProgramType;
                    
                    // If we are running the report for last year, subtract 1 from the grade
                    // Does not take retained students into account
                    if (thisYear)
                    {
                        student.grade = int.Parse(rs.grade);
                    }
                    else
                    {
                        student.grade = int.Parse(rs.grade) - 1;
                    }

                    string[] services = rs.services.Split(",");
                    if (services.Contains("1")) student.reading = true;
                    if (services.Contains("2")) student.math = true;
                    // If the student has received any services, they are a title 1 student
                    if (services.Count() > 0) student.title1Student = "Y";
                    students.Add(student);
                }
            }

            // Count the reading and math students for the appropriate school based on grade
            foreach (Student student in students)
            {
                // Console.WriteLine("{0} Grade {3} Reading {1} Math {2}", student.studentNumber, student.reading, student.math, student.grade);
                if (student.grade < _tisGradeCutoff)
                {
                    countBGP(student);
                }
                else if (student.grade >= _tisGradeCutoff && student.grade < _tmsGradeCutoff)
                {
                    countTIS(student);
                }
                else if (student.grade >= _tmsGradeCutoff && student.grade < _thsGradeCutoff)
                {
                    countTMS(student);
                }
                else if (student.grade >= _thsGradeCutoff && student.grade <= _graduatedCutoff)
                {
                    countTHS(student);
                }
            }



            List<string> ed205 = new List<string>();
            ed205.Add("ED205 Report for School Year " + earliest.ToShortDateString() + " - " + latest.ToShortDateString());
            ed205.Add("BGP");
            ed205.Add("Reading: " + bgpReading);
            ed205.Add("Math: " + bgpMath);
            ed205.Add("TIS");
            ed205.Add("Reading: " + tisReading);
            ed205.Add("Math: " + tisMath);
            ed205.Add("TMS");
            ed205.Add("Reading: " + tmsReading);
            ed205.Add("Math: " + tmsMath);

            foreach (string line in ed205)
            {
                Console.WriteLine(line);
            }

            string folder = Path.GetDirectoryName(filePath) ?? "";
            Console.WriteLine("Output folder will be: {0}", folder);

            string summaryFile = Path.Combine(folder,"ed205-summary.txt");

            Console.WriteLine("Creating {0} from current data.", summaryFile);          
            File.Delete(summaryFile);
            File.WriteAllLines(summaryFile, ed205);

            // Create a tab-delimited export of only students that received Title 1 services.            

            List<string> outputLines = new List<string>();
            string headers = "S_CT_STU_HT_C.N_InstuctionalSuppService\t" +
            "S_CT_STU_HT_C.N_TitleIProgramType\t" +
            "S_CT_STU_HT_C.N_TitleIStudent\t" +
            "STUDENTS.Grade_Level\t" +
            "STUDENTS.Student_Number\t" +
            "STUDENTS.State_StudentNumber";
            outputLines.Add(headers);

            foreach (Student student in students)
            {
                if (student.reading == false && student.math == false)
                {
                    continue;
                }

                // If we have a blank in Title 1 Program Type, set to 1, since that's what all of our current students should be
                if (student.title1ProgramType == "")
                {
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: {0} {1} {2} does not have a Title 1 Program Type in any of their entries, using {3}",
                        student.firstName, student.lastName, student.studentNumber, "1");
                    student.title1ProgramType = _defaultTitle1ProgramType;
                    Console.ResetColor();
                } 

                string services = "";

                if (student.reading) services += "1,";
                if (student.math) services += "2,";

                string studentLine = services + "\t" +
                    student.title1ProgramType + "\t" +
                    student.title1Student + "\t" +
                    student.grade + "\t" +
                    student.studentNumber + "\t" +
                    student.stateStudentNumber + "\t" +
                    student.lastName + "\t" +
                    student.firstName;
                outputLines.Add(studentLine);

            }

            string exportFile = Path.Combine(folder,"ed205-export.txt");

            Console.WriteLine("Creating {0} from current data.", exportFile);

            // Delete the existing export if it exists
            File.Delete(exportFile);

            // Create the new export file
            File.WriteAllLines(exportFile, outputLines);
        }

        void countBGP(Student student)
        {
            if (student.reading) bgpReading++;
            if (student.math) bgpMath++;
        }

        void countTIS(Student student)
        {
            if (student.reading) tisReading++;
            if (student.math) tisMath++;
        }

        void countTMS(Student student)
        {
            if (student.reading) tmsReading++;
            if (student.math) tmsMath++;
        }

        // NOTE: CountTHS is not currently accurate for last year since can't just subract 1 from 99, we need to take
        // graduation dates into account. Will code if needed.
        void countTHS(Student student)
        {
            if (student.reading) thsReading++;
            if (student.math) thsMath++;
        }
    }
}
