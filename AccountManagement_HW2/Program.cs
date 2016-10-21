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
            Console.WriteLine("6. List Customers");
            Console.WriteLine("7. Bank's Total Funds");
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
                    AllAccounts();
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
                case '6':
                    ListCustomers();
                    break;
                case '7':
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
                Console.Write("\n\tWhat type of account? Checking(C), Savings(S) or Go Back(Q)? ");
                char acctype = Console.ReadKey(false).KeyChar;

                if (acctype == 'c' || acctype == 'C')
                {
                    Customer newCustomer;
                    AddCustomerInfo(out newCustomer, checking: true);
                    if (newCustomer != null)
                        AddNewCustomer(ref newCustomer);
                }
                else if (acctype == 's' || acctype == 'S')
                {
                    Customer newCustomer;
                    AddCustomerInfo(out newCustomer, savings: true);
                    if (newCustomer != null)
                        AddNewCustomer(ref newCustomer);
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
            Console.Write("\nType the customer's account number: ");
            Customer customer;
            try
            {
                customer = FindCustomer(Convert.ToUInt32(Console.ReadLine()));
            }
            catch (Exception)
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error finding customer.");
                Console.ForegroundColor = originalForeground;
                return;
            }

            if (customer == null) Console.WriteLine("That account doesn't exist.");
            else
            {
                Console.Write("Enter amount $");
                try
                {
                    customer.Deposit(Convert.ToDouble(Console.ReadLine()));
                }
                catch (FormatException)
                {
                    var originalForeground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input.");
                    Console.ForegroundColor = originalForeground;
                }
            }
        }

        public static void Withdrawal()
        {
            Console.Write("\nType the customer's account number: ");
            Customer customer;
            try
            {
                customer = FindCustomer(Convert.ToUInt32(Console.ReadLine()));
            }
            catch (Exception)
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error finding customer.");
                Console.ForegroundColor = originalForeground;
                return;
            }

            if (customer == null) Console.WriteLine("The customer does not exist");
            else
            {
                Console.Write("Enter amount $");
                try
                {
                    customer.Withdrawal(Convert.ToDouble(Console.ReadLine()));
                }
                catch (FormatException)
                {
                    var originalForeground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Format Exception");
                    Console.ForegroundColor = originalForeground;
                }
            }
        }

        public static void AllAccounts()
        {
            if (HsCustomers.Count < 1)
            {
                Console.WriteLine("No customers available.");
                return;
            }
            foreach (var cust in HsCustomers)
            {
                Console.WriteLine(cust);
            }
        }

        public static void RemoveAccount()
        {
            Console.Write("Type the customer's account number: ");
            uint acctnum = 0;
            try
            {
                acctnum = Convert.ToUInt32(Console.ReadLine());
            }
            catch (Exception)
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red; 
                Console.WriteLine("Invalid input.\n");
                Console.ForegroundColor = originalForeground;
            }
            
            if (HsCustomers.Remove(FindCustomer(acctnum)))
                Console.WriteLine("Customer was successfully removed");
            else
                Console.WriteLine("Customer was not found");

        }

        public static Customer FindCustomerByName(string[] cname)
        {
            using (var look = HsCustomers.GetEnumerator())
            {
                while (look.MoveNext()) 
                {
                    if (look.Current.FirstName != cname[0] && look.Current.LastName != cname[1])
                        return look.Current;
                }
            }
            var originalForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Customer not found. \n{0}");
            Console.ForegroundColor = originalForeground;
            return null;
        }

        private static Customer FindCustomer(uint accountNumber)
        {
            using (var lookup = HsCustomers.GetEnumerator())
            {
                if (lookup.MoveNext() == false) return null;
                do
                {
                    if (lookup.Current.CheckingAccountNumber.Equals(accountNumber) ||
                        lookup.Current.SavingsAccountNumber.Equals(accountNumber))
                        return lookup.Current;
                } while (lookup.MoveNext());
            }
            Console.WriteLine("Customer not found.\n");
            return null;
        }

        private static bool AddNewCustomer(ref Customer newCustomer)
        {
            if (HsCustomers.Add(newCustomer))
            {
                Console.WriteLine(@"'{0}' added successfully", newCustomer);
            }
            else
            {
                Console.WriteLine("Customer could not be added");
                return false;
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

        private static void AddCustomerInfo(out Customer potentialCustomer, bool checking = false, bool savings = false)
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
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in AddCustomerInfo(). \n {0}", e.Message);
                Console.ForegroundColor = originalForeground;
                return;
            }
            finally
            {
                potentialCustomer = null;
            }

            if (name.Length < 2)
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Must input first and last name");
                Console.ForegroundColor = originalForeground;
                return;
            }

            if (address.Length < 3)
            {
                Console.WriteLine("Address must contain the city state and zip code.", ConsoleColor.Red);
            }
            potentialCustomer = new Customer( new string[][]{ name, address }, checking: checking, savings: savings);
        }


        public static double TotalFunds()
        {
            if (HsCustomers.Count < 1) return 0.0;
            double total = 0;
            foreach (var cust in HsCustomers)
            {
              total += cust.CheckingBalance + cust.SavingsBalance;
            }
            return total;
        }

        public static void Main(string[] args)
        {
            HsCustomers = new HashSet<Customer>();
            do
            {
                Menu();
            } while ( ProcessMenuSelection(Console.ReadKey().KeyChar) ); 
        } // Main()

        #region Program member data
        public static HashSet<Customer> HsCustomers;
        #endregion
    } // class Program




    public class Customer
    {
        #region private embedded classes
        private class SavingsAccount
        {
            public SavingsAccount(double interestRate = 1.5)
            {
                _interestRate = interestRate;
                AccountNumber = ++_totalAccounts;
                _balance = 0;
            }

            public readonly double _interestRate;
            public double InterestRate => _interestRate;
            public readonly uint? AccountNumber; // { get; private set; }
            private static uint _totalAccounts;
            private double? _balance;
            public double? Balance
            {
                get { return _balance; }
                set
                {
                    if (value < 0)
                        Console.WriteLine("Insufficient funds. Transaction Aborted");
                    else
                        _balance = value;
                }
            }

        } // class SavingsAccount

        private class CheckingAccount
        {
            public CheckingAccount()
            {
                AccountNumber = ++_totalAccounts;
                _balance = 0;
            }

            public readonly uint? AccountNumber; 
            private static uint _totalAccounts;
            private double? _balance;
            public double? Balance
            {
                get { return _balance; }
                set
                {
                    if (value < 0)
                    {
                        var originalForeground = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Attempting to set Checking Balance below 0. Transaction aborted.");
                        Console.ForegroundColor = originalForeground;
                    }
                    else
                        _balance = value;
                }
            }
        } // class CheckingAccount
        #endregion

        #region Customer member data
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public uint Zipcode { get; set; }
        private readonly CheckingAccount _checkingAccount;
        private readonly SavingsAccount _savingsAccount;
        
        public double CheckingBalance => _checkingAccount?.Balance ?? 0.0;
        public double SavingsBalance => _savingsAccount?.Balance ??  0.0;

        public uint CheckingAccountNumber => _checkingAccount?.AccountNumber ?? 0;
        public uint SavingsAccountNumber => _savingsAccount?.AccountNumber ?? 0;
        #endregion

        public Customer(bool checking = false, bool savings = false)
        {
            if (checking)
            { 
                _checkingAccount = new CheckingAccount();
            }

            if (savings)
            {
                _savingsAccount = new SavingsAccount();
            }
        }

        public Customer(string[][] details, bool checking = false, bool savings = false)
        {
            FirstName = details[0][0].ToString(); 
            LastName = details[0][1].ToString(); 
            City = details[1][0].ToString();
            State = details[1][1].ToString(); 
            Zipcode = Convert.ToUInt32(details[1][2].ToString()); 
            if (checking)
                _checkingAccount = new CheckingAccount();
            if (savings)
                _savingsAccount = new SavingsAccount();
        }

        public void Withdrawal(double amount, bool checking = false, bool savings = false)
        { 
            if(checking)   this._checkingAccount.Balance -= amount;
            else if (savings)  this._savingsAccount.Balance -= amount;
        }

        public void Deposit(double amount, bool checking = false, bool savings = false)
        {
            if (checking)   this._checkingAccount.Balance += amount;
            else if (savings)   this._savingsAccount.Balance += amount;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("{0} {1}\n {2}, {3}  {4}\nChecking Account #: {5}\nChecking Balance: ${6}\nSavings Account #: {7}\nSavings Balance: ${8}", 
                FirstName, LastName, City, State, Zipcode, CheckingAccountNumber, CheckingBalance, SavingsAccountNumber, SavingsBalance);
            return str.ToString();
        }
    } // Class Customer

    public class CustomerComparer : IEqualityComparer<Customer>
    {
        public bool Equals(Customer a, Customer b)
        {
            return a.CheckingAccountNumber == b.CheckingAccountNumber;
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
