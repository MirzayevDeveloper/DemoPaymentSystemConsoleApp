using DemoPaymentSystem.Domains;
using DemoPaymentSystem.States;
using System.Text.RegularExpressions;

namespace DemoPaymentSystem
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool isActive = true;
            int choice;

            while (isActive)
            {
                Console.Clear();
                Console.Write("1.Sign in\n2.Sign up\nchoose option: ");
                string? choose = Console.ReadLine();
                int.TryParse(choose, out choice);

                switch (choice)
                {
                    case 1:
                        string? login = SignIn();
                        if (login == null) ConsoleReadKey();
                        else
                        {
                            SecondMenu(login);
                        }
                        break;
                    case 2:
                        string? phone = SignUp();
                        if (phone == null) ConsoleReadKey();
                        else
                        {
                            SecondMenu(phone);
                        }
                        break;
                }
            }
        }
        static string? SignIn()
        {
            Console.Write("enter your phone: +(998) ");
            string? phone = "+(998)" + Console.ReadLine();

            bool isTrue = IsValidNumber(ref phone);
            if (!isTrue) { Console.WriteLine("Invalid number!"); return null; }

            if (!Account.HasLogin(phone))
            {
                Console.WriteLine("We have not like this phone number account\nPlease try again!");
                return null;
            }

            string? password = string.Empty;
            int count = -1;
            do
            {
                if (count == 2)
                {
                    Console.WriteLine("You enter many times, please try again later!");
                    return null;
                }
                Console.Write("enter your password: ");
                password = Console.ReadLine();
                count++;
            }
            while (!Account.IsTruePassword(phone, password));

            return phone;
        }
        static string? SignUp()
        {
            Console.Write("enter your phone number: +(998) ");
            string? phone = "+(998)" + Console.ReadLine();

            bool isTrue = IsValidNumber(ref phone);
            if (!isTrue) { Console.WriteLine("Invalid phone number!"); return null; }

            if (Account.HasLogin(phone))
            {
                Console.WriteLine("This number already our user, Please try again later!");
                return null;
            }

            Console.Write("enter your card number: ");
            string? cardNumber = Console.ReadLine();
            isTrue = IsValidCardNumber(ref cardNumber);
            if (!isTrue) { Console.WriteLine("Invalid card number!"); return null; }
            if (Account.CheckCard(cardNumber) != null)
            {
                Console.WriteLine("This card is already registered!"); return null;
            }

            Console.Write("enter your name: ");
            string? name = Console.ReadLine();

            bool isValid = false;
            string? password;
            do
            {
                Console.Write("create password: ");
                password = Console.ReadLine();
                isValid = Regex.IsMatch(password, @"(\d|\w){8,}");

            } while (!isValid);

            var newUser = new User(cardNumber, null)
            {
                Owner = name,
                Balance = 100,
                Phone = phone,
                Password = password,
                AccountStatus = UserStatus.Active
            };
            Account.AddUser(newUser);
            return phone;
        }
        static void SecondMenu(string login)
        {
            bool isTrue = true;
            int selection;
            while (isTrue)
            {
                Console.Clear();
                Console.Write($"Current balance: {Account.GetUser(login)?.Balance}\n" +
                    $"1.Transaction\n2.My Monitoring\n3.Get debt from system\n4.Log out\n" +
                    $"5.Delete my account\nchoose option: ");
                string? chosen = Console.ReadLine();
                int.TryParse(chosen, out selection);

                switch (selection)
                {
                    case 0: isTrue = false; break;
                    case 1:
                        if (Transaction(Account.GetUser(login))) Console.WriteLine("Successfully...");
                        ConsoleReadKey();
                        break;
                    case 2: GetMonitoring(login); ConsoleReadKey(); break;
                    case 3:
                        if (GetDebt(Account.GetUser(login))) Console.WriteLine("Successfully");
                        ConsoleReadKey();
                        break;
                    case 4: isTrue = false; break;
                    case 5: break;
                }
            }
        }
        static bool GetDebt(User? user)
        {
            decimal amount = Account.GetDebt(user.Id);
            Console.WriteLine($"Your current debt: {amount}");
            if (amount > 0)
            {
                Console.WriteLine("Press 'y' to pay your debt! ");
            }
            Console.Write("enter debt amount: ");
            string? amunt = Console.ReadLine();
            if(amunt == "y")
                return Account.PayDebt(user);
            else
            {
                decimal _amount;
                decimal.TryParse(amunt, out _amount);
                return Account.GetDebtFromSystem(user, _amount);
            }
        }
        static void GetMonitoring(string? login)
        {
            var list = Account.GetMonitoring(login);

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }
        static bool Transaction(User? user)
        {
            Console.Write("enter receiver card: ");
            string? receiverCard = Console.ReadLine();

            bool isTrue = IsValidCardNumber(ref receiverCard);
            if (!isTrue) { Console.WriteLine("Invalid card numeber!"); return false; }

            if (Account.CheckCard(receiverCard) == null)
            { Console.WriteLine("not found like this card user"); return false; }

            else if (receiverCard == user?.CardNumber)
            { Console.WriteLine("Are you ill, why did enter your card number for transaction?"); return false; }

            Console.Write("enter Amount: ");
            string? amount = Console.ReadLine();
            decimal _amount;
            decimal.TryParse(amount, out _amount);

            if (!Account.CheckBalance(user, _amount))
            {
                Console.WriteLine("You haven't enough money!\n" +
                    "If you want debt, you can choose from main menu \"3.Get debt\" option!");
                return false;
            }

            Account.Transaction(user, receiverCard, _amount);

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(1000);
                Console.Beep();
            }

            return true;
        }
        static bool IsValidCardNumber(ref string? cardNumber)
        {
            bool isValid = Regex.IsMatch(cardNumber, @"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}\s?$");
            List<char> list = new();
            for (int i = 0; i < cardNumber.Length; i++)
            {
                if (cardNumber[i] == ' ') continue;
                list.Add(cardNumber[i]);
            }
            cardNumber = string.Join("", list);
            return isValid;
        }
        static bool IsValidNumber(ref string? number)
        {
            bool isTrue = Regex.IsMatch(number, @"^\+\(\d{3}\)\d{2}(\s|-)?\d{3}(\s|-)?\d{2}(\s|-)?\d{2}$");
            if (!isTrue) return false;

            for (int i = 1; i < number.Length; i++)
            {
                if (number[i] == '-' || number[i] == ' ')
                {
                    number = number.Remove(i, 1);
                    i--;
                }
            }
            return true;
        }
        static void ConsoleReadKey()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}