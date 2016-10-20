/*
    'readonly' variables must be assigned a value before the constructor exits.
    'const' variables must be defined at declaration & are implicitly static. It is also baked into
    the assembly IL. So if another assembly references it, and the const value was changed, it the other 
    assembly would only get that new value after recompiling the const variable's assembly.

    ____Assumptions about the data____
    There can be multiple customers with the same name. 
    A customer can have multiple checking/ savings accounts.
    No checking or savings account can have the same number.
    

    ____Member data____
    public 

    ___Ideas___
    Use an immutable data structure to store customers since customers can have the same name.

    the code can be a bit more DRY.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AccountManagement
{
    class Program
    {
        public static void Menu()
        {
            Console.WriteLine("\n1. Create a new account");
            Console.WriteLine("2. View Account(s)");
            Console.WriteLine("3. Deposit");
            Console.WriteLine("4. Withdrawal");
            Console.WriteLine("5. Delete an account");
            Console.WriteLine("6. BATCH MODE");
            Console.WriteLine("7. Search customer by name");
            Console.WriteLine("8. List All Customers (ordered by join date)");
            Console.WriteLine("9. Bank's Total Funds");
            Console.WriteLine("Q Exit");
        }

        public static bool ProcessMenuSelection(char sel)
        {
            switch (sel)
            {
                case '1':
                    CreateAccount();
                    break;
                case '2':
                    AccountSummary();
                    break;
                case '3':
                    Deposit();
                    break;
                case '4':
                    Withdrawal();
                    break;
                case '5':
                    RemoveAccount();
                    break;
                case '8':
                    ListCustomers();
                    break;
                case '9':
                    Console.WriteLine("$ {0}", TotalFunds());
                    break;
                case 'q':
                case 'Q':
                    return false;
                default:
                    Console.WriteLine("Invalid selection\n");
                    break;
            }
            return true;
        }

        public static void CreateAccount()
        {
            do
            {
                Console.Write("\n\tWhat type of account? Checking(C), Savings(S), Both(B), or Go Back(Q)? ");
                char acctype = Console.ReadKey(false).KeyChar;

                if (acctype == 'c' || acctype == 'C')
                {
                    Customer newCustomer;
                    GetInfo(out newCustomer);
                    if (newCustomer != null)
                        AddNewCustomer(ref newCustomer);
                }
                else if (acctype == 's' || acctype == 'S')
                {
                    Customer newCustomer;
                    GetInfo(out newCustomer);
                    if (newCustomer != null)
                        AddNewCustomer(ref newCustomer);
                }
                else if (acctype == 'b' || acctype == 'B')
                {
                    Console.WriteLine("Setting up both checking & savings");
                }
                else if (acctype == 'q' || acctype == 'Q')
                {
                    break;
                }
                else
                {
                    Console.Write("Invalid input. \n");
                }
            } while (true);


        }


        public static void Deposit()
        {

        }

        public static void Withdrawal()
        {

        }

        public static void AccountSummary()
        {

        }

        public static void RemoveAccount()
        {

        }

        private string[] FindCustomer(string[] cname)
        {
            return new string[1];
        }

        public static bool CustomerExists(string cname)
        {
            //Program.HsCustomers.Contains();
            return false;
        }

        private static bool AddNewCustomer(ref Customer newCustomer)
        {
            if (HsCustomers.Add(newCustomer))
            {
                Console.WriteLine(@"'{0}' added successfully", newCustomer.ToString());
            }
            else
            {
                Console.WriteLine("Customer could not be added");
            }

            return true;
        }

        public static void ListCustomers()
        {
            foreach (var c in HsCustomers)
            {
                Console.WriteLine(c);
            }
        }

        private static void GetInfo(out Customer potentialCustomer)
        {
            string[] name;
            string[] address;
            try
            {
                Console.Write("\tCustomer Name: ");
                name = Console.ReadLine().Split(' ');
                Console.Write("\tAddress: <city> <state> <zip>: ");
                address = Console.ReadLine().Split(' ');
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetInfo(). \n {0}", e.Message);
                return;
            }
            finally
            {
                potentialCustomer = null;
            }

            if (name.Length < 2)
            {
                Console.WriteLine("Must input first and last name");
                return;
            }

            potentialCustomer = new Customer(name, address);
            CustomerComparer predicate = new CustomerComparer();
            if (HsCustomers.Contains(potentialCustomer, predicate))
            {
                Console.WriteLine("That customer already exists.");
                potentialCustomer = null;
                return;
            }
        }


        public static double TotalFunds()
        {
            if (HsCustomers.Count < 1) return 0.0;
            else
            {
                double total = 0;
                foreach (var cust in HsCustomers)
                {
                    total += cust.checkingAccount.Amount + cust.savingsAccount.Amount;
                }
                return total;
            }
        }

        static void Main(string[] args)
        {
            HsCustomers = new HashSet<Customer>();
            // Program loop
            do
            {
                Menu();
            } while ( ProcessMenuSelection(Console.ReadKey().KeyChar) ); 
        } // Main()

        #region Program member data
        public Customer customer;
        public static HashSet<Customer> HsCustomers;
        #endregion
    } // class Program


    class SavingsAccount
    {
        public SavingsAccount(double INTERESTRATE = 1.5)
        {
            interestRate = INTERESTRATE;
            ACCOUNT_NUMBER = ++totalSavingsAccount;
        }

        public double interestRate { get; set; }
        public uint ACCOUNT_NUMBER { get; private set; }
        private static uint totalSavingsAccount;
        public int Amount { get; set; }
    } // class SavingsAccount

    class CheckingAccount
    {
        public CheckingAccount()
        {
            AccountNumber = ++totalCheckingAccounts;
            //AccountNumber.Add(++totalCheckingAccounts);
        }

        public uint AccountNumber;
        private static uint totalCheckingAccounts;
        public int Amount { get; set; }
    } // class CheckingAccount

    class Customer
    {
        #region Customer member data
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public uint Zipcode { get; set; }
        public CheckingAccount checkingAccount;
        public SavingsAccount savingsAccount;
        #endregion

        public Customer(bool checking = false, bool savings = false)
        {
            if (checking == true)
            { 
                checkingAccount = new CheckingAccount();
            }

            if (savings == true)
            {
                savingsAccount = new SavingsAccount();
            }
        }

        public Customer(params object[] details)//string [] name, string city, string state, uint zip)
        {
            firstName = details[0].ToString(); //name[0];
            lastName = details[1].ToString(); //name[1];
            City = details[2].ToString();//city;
            State = details[3].ToString(); //state;
            Zipcode = Convert.ToUInt32(details[4].ToString()); //zip;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("{0} {1}\n {2}, {3}  {4}\n {5}\n{6}\n", firstName, lastName, City, State, Zipcode, checkingAccount, savingsAccount);
            return str.ToString();
        }
    } // Class Customer

    class CustomerComparer : IEqualityComparer<Customer>
    {
        public bool Equals(Customer a, Customer b)
        {
            return a.firstName == b.firstName && a.lastName == b.lastName && a.checkingAccount == b.checkingAccount;
        }

        public int GetHashCode(Customer a)
        {
            return a.GetHashCode();
        }
    }
}// namespace 


/*


            public static void BatchMode(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c" + command);
                //{Arguments = "cmd.exe", Domain = command};
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;

            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
        }



                bool BATCH = false;
            if (args.Length > 0) BATCH = true;
            TextReader istr = Console.In;
            FileStream ifs;
            if (BATCH)
            {
                try
                {
                    ifs = File.OpenRead(args[0]);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("\tCould not open file \"{0}\" \n Ending program.\n", e.FileName);
                    Environment.Exit(-1);
                }
                catch (Exception)
                {
                    Console.WriteLine("\tSome error occured when trying to open file.\n");
                    Environment.Exit(-1);
                }
            }

    */
