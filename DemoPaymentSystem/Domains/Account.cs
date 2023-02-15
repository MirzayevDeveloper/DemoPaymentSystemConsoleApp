using DemoPaymentSystem.States;
using Newtonsoft.Json;

namespace DemoPaymentSystem.Domains
{
    public class Account
    {
        private static string path = Directory.GetCurrentDirectory();
        private static DirectoryInfo? directoryInfo = new DirectoryInfo(path).Parent?.Parent?.Parent;

        private static Func<List<DebtModel>> getListOfDebts = () =>
        {
            string path2 = directoryInfo?.FullName + "\\DataBase\\debt.json";

            if (!File.Exists(path2)) return new List<DebtModel> { };

            string json = File.ReadAllText(path2);

            return JsonConvert.DeserializeObject<List<DebtModel>>(json);
        };
        public static decimal GetDebt(Guid guid)
        {
            var debts = getListOfDebts();
            var user = debts.FirstOrDefault(x => x.UserId == guid);
            if (user != null) return user.debtAmount;
            return 0;
        }
        public static bool GetDebtFromSystem(User? user, decimal amount)
        {
            if (amount < 0) return false;

            var debts = getListOfDebts();

            var debt = debts.FirstOrDefault(x => x.UserId == user.Id);

            if (debt != null)
            {
                debts.Remove(debt);
                debt.debtAmount += amount;
                debts.Add(debt);
            }
            else
            {
                debts.Add(new DebtModel
                {
                    UserId = user.Id,
                    UserName = user.Owner,
                    debtAmount = amount,
                });
            }
            user.Balance += amount;
            ChangeUserInfo(user);
            SaveAction.Invoke(debts);
            return true;
        }

        public static Action<List<DebtModel>> SaveAction = debt =>
        {
            string path2 = directoryInfo?.FullName + "\\DataBase\\debt.json";

            var json = JsonConvert.SerializeObject(debt, Formatting.Indented);
            File.WriteAllText(path2, json);
        };

        public static bool PayDebt(User? user)
        {
            var debts = getListOfDebts();
            var debt = debts.FirstOrDefault(x => x.UserId == user.Id);
            DebtModel? model = new();
            model = debt;

            if(model != null)
            {
                if(user.Balance >= model.debtAmount)
                {
                    user.Balance -= model.debtAmount;
                    debts.Remove(debt);
                }
                else
                {
                    model.debtAmount -= user.Balance;
                    user.Balance = 0;
                    debts.Remove(debt);
                    debts.Add(model);
                }
                SaveAction.Invoke(debts);
                ChangeUserInfo(user);
                return true;
            }
            return false;
        }

        public static void RemoveUserFromDebtList(Guid userId)
        {
            var debt = getListOfDebts();
            var user = debt.FirstOrDefault(x => x.UserId == userId);
            if (user != null) { debt.Remove(user); }
            
        }

        public static List<string> GetMonitoring(string? login)
        {
            string path1 = directoryInfo.FullName + "\\DataBase\\Monitoring.txt";

            var userOfMonitoring = GetUser(login);

            List<string> list = File.ReadAllLines(path1).ToList();

            string id = "Id: " + userOfMonitoring.Id.ToString();

            List<string> monitoring = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == id)
                {
                    int j = i;
                    while (list[j] != "")
                    {
                        monitoring.Add(list[j]);
                        j++;
                    }
                    monitoring.Add("");
                }
            }
            return monitoring;
        }

        public static bool Transaction(User? sender, string? card, decimal amount)
        {
            var list = FuncGetAllUser();
            var _sender = list?.Find(x => x.CardNumber == sender?.CardNumber);
            var _reveiver = list?.Find(x => x.CardNumber == card);

            if (_sender == null || _reveiver == null) return false;

            decimal price = amount / 100 / 2;

            if (_sender.Balance - amount < price) amount -= price;
            else _sender.Balance -= price;

            ActionMonitoring(new HistoryModel() { Sender = _sender, Receiver = _reveiver, Amount = amount, ServicePrice = price });

            _sender.Balance -= amount;
            _reveiver.Balance += amount;
            ChangeUserInfo(_sender);
            ChangeUserInfo(_reveiver);

            return true;
        }

        private static Action<HistoryModel> ActionMonitoring = HistoryModel =>
        {
            string path1 = directoryInfo.FullName + "\\DataBase\\Monitoring.txt";

            if (!File.Exists(path1)) File.Create(path1).Close();

            using (StreamWriter sw = File.AppendText(path1))
            {
                sw.WriteLine(HistoryModel);
            };
        };

        public static bool CheckBalance(User user, decimal amount)
        {
            return user.Balance >= amount;
        }

        public static void AddUser(User user)
        {
            string path1 = directoryInfo?.FullName + "\\DataBase";
            if (!Directory.Exists(path1))
            {
                Directory.CreateDirectory(path1);
                File.Create(path1 + "\\Data.txt").Close();
            }
            DirectoryInfo directory = new DirectoryInfo(path1);

            using (StreamWriter writer = File.AppendText(directory.FullName + "\\Data.txt"))
            {
                writer.WriteLine(user);
            }
        }

        public static bool HasLogin(string? phone)
        {
            var list = FuncGetAllUser();
            if (list != null) return list.Find(a => a.Phone == phone) != null;
            return false;
        }

        public static bool IsTruePassword(string? phone, string? password)
        {
            var list = FuncGetAllUser();
            var user = list?.Find(a => a.Phone == phone);
            return user?.Password == password;
        }

        public static void RemoveUser(User user)
        {
            var list = FuncGetAllUser.Invoke();

            var foundUser = list.Find(u => u.Id == user.Id);

            if (foundUser != null)
            {
                list.Remove(foundUser);

                ActionWrite.Invoke(list);
            }
        }

        public static User? CheckCard(string? card)
        {
            var list = FuncGetAllUser();
            if (list != null) return list.Find(x => x.CardNumber == card);
            return null;
        }

        public static User? GetUser(string? phone)
        {
            var list = FuncGetAllUser();
            if (list != null) return list.Find(x => x.Phone == phone);
            return null;
        }

        private static Action<List<User>> ActionWrite = list =>
        {
            string path1 = directoryInfo?.FullName + "\\DataBase";
            DirectoryInfo? directory = new(path1);

            using (StreamWriter writer = File.CreateText(directory.FullName + "\\Data.txt"))
            {
                foreach (var item in list)
                {
                    writer.WriteLine(item);
                }
            }
        };

        public static Action<User> ChangeUserInfo = user =>
        {
            var list = FuncGetAllUser();
            int index = list.IndexOf(list.Find(x => x.CardNumber == user.CardNumber));
            list[index] = user;
            ActionWrite(list);
        };

        private static Func<List<User>?> FuncGetAllUser = () =>
        {
            string path1 = directoryInfo?.FullName + "\\DataBase";
            DirectoryInfo? directory = directoryInfo;

            if (!File.Exists(path1 + "\\Data.txt")) return null;

            if (!Directory.Exists(directoryInfo?.FullName + "\\DataBase"))
            {
                path1 = directoryInfo?.FullName + "\\DataBase";
                directory = new DirectoryInfo(path1);
            }

            List<string> list = File.ReadAllLines(path1 + "\\Data.txt").ToList();

            List<User> users = new List<User>();
            User currentUser = new();

            Guid? currentId = new Guid();
            string? currentCardNumber = string.Empty;
            int index;

            for (int i = 0; i < list.Count; i++)
            {
                index = list[i].IndexOf(' ') + 1;
                int size = list[i].Length - index;

                if (list[i].Contains("Id:"))
                    currentId = Guid.Parse(list[i].Substring(index, size));

                else if (list[i].Contains("CardNumber:"))
                    currentCardNumber = list[i].Substring(index, size);

                else if (list[i].Contains("Balance:"))
                    currentUser.Balance = decimal.Parse(list[i].Substring(index, size));

                else if (list[i].Contains("Owner:"))
                    currentUser.Owner = list[i].Substring(index, size);

                else if (list[i].Contains("Phone:"))
                    currentUser.Phone = list[i].Substring(index, size);

                else if (list[i].Contains("Password:"))
                    currentUser.Password = list[i].Substring(index, size);

                else if (list[i].Contains("AccountStatus:"))
                {
                    UserStatus status;
                    Enum.TryParse(list[i].Substring(index, size), out status);
                    currentUser.AccountStatus = status;
                }
                else
                {
                    currentUser = new User(currentCardNumber, currentId)
                    {
                        AccountStatus = currentUser.AccountStatus,
                        Balance = currentUser.Balance,
                        Owner = currentUser.Owner,
                        Password = currentUser.Password,
                        Phone = currentUser.Phone,
                    };

                    users.Add(currentUser);
                    currentUser = new User();
                }
            }
            return users;
        };
    }
}
