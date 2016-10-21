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
            Console.WriteLine("6. BATCH MODE (deprecated)");
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
                Console.Write("\n\tWhat type of account? Checking(C), Savings(S) or Go Back(Q)? ");
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

            } catch (Exception)
            {
                Console.WriteLine("Error finding customer.");
                return;
            }

            if (customer == null) Console.WriteLine("That account doesn't exist.");
            else
            {
                Console.Write("Enter amount $");
                try
                {
                    double deposit = Convert.ToDouble(Console.ReadLine());
                    customer.checkingAccount.Amount += deposit;
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input.");
                    return;
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
                Console.WriteLine("Error finding customer.");
                return;
            }

            if(customer == null) Console.WriteLine("The customer does not exist");
            else
            {
                Console.Write("Enter amount $");
                try
                {
                    double withdrawal = Convert.ToDouble(Console.ReadLine());
                    customer.checkingAccount.Amount -= withdrawal;
                } catch (Exception)
                {
                    Console.WriteLine("Invalid input.");
                    return;
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
        }

        private Customer FindCustomerByName(string[] cname)
        {
            //var lookup = HsCustomers.ToLookup(c => c.firstName);
            var look = HsCustomers.GetEnumerator();

            while (look.Current.firstName != cname[0] && look.Current.lastName != cname[1])
            {
                try
                {
                    look.MoveNext();
                }
                catch (Exception)
                {
                    Console.WriteLine("Customer not found.");
                    return null;
                }
            }
            return look.Current;
        }

        private static Customer FindCustomer(uint AccountNumber)
        {
            var lookup = HsCustomers.GetEnumerator();
            if (lookup.MoveNext() == false) return null;

            do
            {
                Console.WriteLine("In FindCustomer dowhile: {0}", lookup.Current.firstName);
                if (lookup.Current.checkingAccount.AccountNumber.Equals(AccountNumber) ||
                    lookup.Current.savingsAccount.AccountNumber.Equals(AccountNumber))
                    return lookup.Current;
            } while (lookup.MoveNext());
            /*
            var lookup = HsCustomers.ToLookup(c => c.checkingAccount.AccountNumber);
            foreach (var actnum in lookup)
            {
                if (actnum.Key == AccountNumber) return actnum.GetEnumerator().Current;
                // return lookup[3].GetEnumerator().Current; //
            }
            */
            return null;
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

            potentialCustomer = new Customer( new string[][]{ name, address }, checking: true);

            // Check if the customer already exists
            CustomerComparer predicate = new CustomerComparer();
            if (HsCustomers.Contains(potentialCustomer, predicate))
            {
                Console.WriteLine("That customer already exists.");
                potentialCustomer = null;
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
        public static HashSet<Customer> HsCustomers;
        #endregion
    } // class Program


    class SavingsAccount
    {
        public SavingsAccount(double INTERESTRATE = 1.5)
        {
            interestRate = INTERESTRATE;
            AccountNumber = ++totalAccounts;
            amount = 0;
        }

        public double interestRate { get; set; }
        public uint AccountNumber { get; private set; }
        private static uint totalAccounts;
        private double amount;
        public double Amount
        {
            get { return amount; }
            set
            {
                if (value < 0)
                {
                    Console.WriteLine("Insufficient funds");
                }
                else
                {
                    amount = value;
                }
            }
        }
         
    } // class SavingsAccount

    class CheckingAccount
    {
        public CheckingAccount()
        {
            AccountNumber = ++totalAccounts;
            //AccountNumber.Add(++totalCheckingAccounts);
        }

        public uint AccountNumber;
        private static uint totalAccounts;
        public double Amount { get; set; }
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

        public Customer(string[][] details, bool checking = false, bool savings = false)
        {
            firstName = details[0][0].ToString(); 
            lastName = details[0][1].ToString(); 
            City = details[1][0].ToString();
            State = details[1][1].ToString(); 
            Zipcode = Convert.ToUInt32(details[1][2].ToString()); 
            if (checking == true)
                checkingAccount = new CheckingAccount();
            if (savings)
                savingsAccount = new SavingsAccount();
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
