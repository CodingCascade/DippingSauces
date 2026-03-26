using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace PayStubMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Staff> myStaff = new List<Staff>();
            FileReader fr = new FileReader();
            int month = 0;
            int year = 0;

            while (year == 0)
            {
                Console.Write("\nPlease enter the year: ");

                try
                {
                    year = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " Please try again.");
                }
            }

            while (month == 0)
            {
                Console.Write("\nPlease enter the month: ");

                try
                {
                    month = Convert.ToInt32(Console.ReadLine());

                    if (month < 1 || month > 12)
                    {
                        Console.WriteLine("Month must be from 1 to 12. Please try again.");
                        month = 0;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " Please try again.");
                }
            }

            myStaff = fr.ReadFile();

            for (int i = 0; i < myStaff.Count; i++)
            {
                try
                {
                    Console.Write("\nEnter hours worked for {0}: ", myStaff[i].NameOfStaff);
                    myStaff[i].HoursWorked = Convert.ToInt32(Console.ReadLine());
                    myStaff[i].CalculatePay();
                    Console.WriteLine(myStaff[i].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    i--;
                }
            }

            PaySlip ps = new PaySlip(month, year);
            ps.GeneratePaySlip(myStaff);
            ps.GenerateSummary(myStaff);

            Console.Read();
        }
    }

    class Staff
    {
        private float hourlyRate;
        private int hoursWorked;

        public float TotalPay { get; protected set; }
        public float BasicPay { get; private set; }
        public string NameOfStaff { get; private set; }

        public int HoursWorked
        {
            get
            {
                return hoursWorked;
            }
            set
            {
                if (value > 0)
                {
                    hoursWorked = value;
                }
                else
                {
                    hoursWorked = 0;
                }
            }
        }

        public Staff(string name, float rate)
        {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        public virtual void CalculatePay()
        {
            Console.WriteLine("Calculating Pay...");

            BasicPay = hoursWorked * hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString()
        {

            return "\nNameOfStaff = " + NameOfStaff + "\nhourlyRate = " + hourlyRate + "\n\nTotalPay = " + hoursWorked + "\nBasicPay = " + BasicPay + "\n\nTotalPay = " + TotalPay;

        }
    }

    class Manager : Staff
    {
        private const float managerHourlyRate = 50.0f;
        public int Allowance { get; private set; }
        public Manager(string name) : base(name, managerHourlyRate) { }

        public override void CalculatePay()
        {
            base.CalculatePay();
            Allowance = 0;

            if (HoursWorked > 160)
            {
                Allowance = 1000;
                TotalPay = BasicPay + Allowance;
            }
        }

        public override string ToString()
        {
            return "\nNameOfStaff = " + NameOfStaff + "\nmanagerHourlyRate = " + managerHourlyRate + "\nHoursWorked = " + HoursWorked + "\nBasicPay = " + BasicPay + "\nAllowance = " + Allowance + "\n\nTotalPay = " + TotalPay;
        }
    }
    
    class Admin : Staff
    {
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30f;

        public float Overtime { get; private set; }

        public Admin(string name) : base(name, adminHourlyRate) { }

        public override void CalculatePay()
        {
            base.CalculatePay();

            if (HoursWorked > 160)
            {
                Overtime = overtimeRate * (HoursWorked - 160);
            }
        }

        public override string ToString()
        {
            return "\nNameOfStaff = " + NameOfStaff + "\nadminHourlyRate = " + adminHourlyRate + "\nHoursWorked = " + HoursWorked + "\nBasicPay = " + BasicPay + "\nOvertime = " + Overtime + "\n\nTotalPay = " + TotalPay;
        }
    }
    
    class FileReader
    {
       public List<Staff> ReadFile()
       {
           List<Staff> myStaff = new List<Staff>();
           string[] result = new string[2];
           string path = "staff.txt";
           string[] separator = { ", " };

           if (File.Exists(path))
           {
               using (StreamReader sr = new StreamReader(path))
               {
                   while (!sr.EndOfStream)
                   {
                       result = sr.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                       
                        if (result[1] == "Manager")
                       {
                          myStaff.Add(new Manager(result[0]));
                       }
                       else if (result[1] == "Admin")
                       {
                          myStaff.Add(new Admin(result[0]));
                       }
                   }
                   
                    sr.Close();
               }
           }
           else 
           {
               Console.WriteLine("Error: File does not exist!");
           }

           return myStaff;
            
       }
    }

    class PaySlip
    {
        private int month;
        private int year;

        enum MonthsOfYear
        {
           January = 1,
           February = 2,
           March,
           April,
           May,
           June,
           July,
           August,
           September,
           October,
           November,
           December
        }

        public PaySlip(int payMonth, int payYear)
        {
           month = payMonth;
           year = payYear;
        }


        public void GeneratePaySlip(List<Staff> myStaff)
        {
            string path;

            foreach (Staff f in myStaff)
            {
                path = f.NameOfStaff + ".txt";

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("Payslip for {0} {1}", (MonthsOfYear)month, year);
                    sw.WriteLine("===========================");
                    sw.WriteLine("Name of Staff: {0}", f.NameOfStaff);
                    sw.WriteLine("Hours Worked: {0}", f.HoursWorked);
                    sw.WriteLine("");
                    sw.WriteLine("Basic Pay: {0:C]", f.BasicPay);

                    if (f.GetType() == typeof(Manager))
                    {
                        sw.WriteLine("Allowance: {0:C}", ((Manager)f).Allowance);
                    }
                    else if (f.GetType() == typeof(Admin))
                    {
                        sw.WriteLine("Overtime: {0:C}", ((Admin)f).Overtime);
                    }

                    sw.WriteLine("");
                    sw.WriteLine("============================");
                    sw.WriteLine("Total Pay: {0:C}", f.TotalPay);
                    sw.WriteLine("============================");

                    sw.Close();

                }
            }
        }
        
        public void GenerateSummary(List<Staff> myStaff)
        {
            var result = from f in myStaff
                         where f.HoursWorked < 10
                         orderby f.NameOfStaff ascending
                         select new { f.NameOfStaff, f.HoursWorked };

            string path = "summary.txt";

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("Staff with less than 10 working hours.");

                foreach (var f in result)
                {
                    sw.WriteLine("Name of Staff: {0}, Hours Worked: {1}", f.NameOfStaff, f.HoursWorked);    
                }

                sw.Close();
            }
        }

        public override string ToString()
        {
            return "month = " + month + "year = " + year;
        }
            
    }
}

