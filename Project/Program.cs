using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Security;

namespace Program
{
    public enum currency { UAH = 980, USD = 840, EUR = 978, PLN = 985 }

    delegate void AccountHandler(string message);

    public interface ICredit
    {
        void Credit(double sum, currency c);
    }

    public class Account
    {
        private int ID;
        private string Name;
        private double Balance;
        private currency C;
        static int AccNum = 0;


        public int id
        {
            get { return ID; }
            set { ID = value; }
        }
        public string name
        {
            get { return Name; }
            set { Name = value; }
        }
        public double balance
        {
            get { return Balance; }
            set { Balance = value; }
        }

        public currency c
        {
            get { return C; }
            set { C = value; }
        }

        public Account() : this(0, "", 0, currency.UAH)
        {

        }

        public Account(int id, string name, double balance, currency c)
        {
            ID = id;
            Name = name;
            Balance = balance;
            C = c;
            AccNum++;
        }
    }

    public class CurrentCounter : Account, ICredit
    {
        event AccountHandler Notify;


        public CurrentCounter() : base(0, "", 0, currency.UAH)
        {
        }

        public CurrentCounter(int ID, string Name, double Balance, currency C) : base(ID, Name, Balance, C)
        {
            id = ID;
            name = Name;
            balance = Balance;
            c = C;
        }


        public void Credit(double amount, currency e)
        {
            try
            {
                amount = Exchange(amount, c, e);

                if (c == e)
                {
                    if (balance >= amount)
                    {
                        balance -= amount;
                        Notify?.Invoke($"Списання з рахунку: {amount}, {e}");
                        History registor = new History($"Списання з рахунку: {amount}, {e}");
                    }
                    else
                    {
                        Notify?.Invoke("Недостатньо коштів");
                        History registor = new History("Недостатньо коштiв");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Debit(double amount, currency e)
        {
            try
            {
                amount = Exchange(amount, c, e);
                balance += amount;
                Notify?.Invoke($"Поповнення рахунку: {amount}, {e}");
                History registor = new History($"Поповнення рахунку: {amount}, {e}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static string GetXMLAsString(XmlDocument myxml)
        {
            return myxml.OuterXml;
        }

        public double Exchange(double amount, currency с, currency e)
        {
            switch(e)
            {
                case currency.USD:
                    {
                        switch (c)
                        {
                            case currency.UAH:
                                {
                                    amount *= 26.6068;
                                    break;
                                }

                            case currency.EUR:
                                {
                                    amount *= 0.8521;
                                    break;
                                }

                            case currency.PLN:
                                {
                                    amount *= 3.9345;
                                    break;
                                }
                        }
                        break;
                    }

                case currency.UAH:
                    {
                        switch (c)
                        {
                            case currency.USD:
                                {
                                    amount *= 0.0376;
                                    break;
                                }

                            case currency.EUR:
                                {
                                    amount *= 0.0320;
                                    break;
                                }

                            case currency.PLN:
                                {
                                    amount *= 0.1479;
                                    break;
                                }
                        }
                        break;
                    }

                case currency.EUR:
                    {
                        switch (c)
                        {
                            case currency.USD:
                                {
                                    amount *= 1.1736;
                                    break;
                                }

                            case currency.UAH:
                                {
                                    amount *= 31.2258;
                                    break;
                                }

                            case currency.PLN:
                                {
                                    amount *= 4.6175;
                                    break;
                                }
                        }
                        break;
                    }

                case currency.PLN:
                    {
                        switch (e)
                        {
                            case currency.USD:
                                {
                                    amount *= 0.2542;
                                    break;
                                }

                            case currency.UAH:
                                {
                                    amount *= 6.7625;
                                    break;
                                }

                            case currency.EUR:
                                {
                                    amount *= 0.2166;
                                    break;
                                }
                        }
                        break;
                    }
            }
            return amount;
        }

        public static string UnescapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            return xmlString.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
        }

        public void Show()
        {
            Console.WriteLine(balance);
        }
    }

    class History
    {
        public List<string> history = new List<string>();
        string record;

        public History(string register)
        {
            File.AppendAllText(@"History.txt", register + "\n");
            history.Add(register);
        }

        public void Show()
        {
            for (int i = 0; i < history.Count; i++)
                Console.WriteLine(history[i]);
        }
    }

    public static class StringExt
    {
        public static Account ToAccount(this string value)
        {
            var arr = value.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return new Account()
            {
                id = Convert.ToInt32(arr[0]),
                name = arr[1],
                balance = Convert.ToDouble(arr[2]),
                c = (currency)Enum.Parse(typeof(currency), arr[3]),
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            File.Delete(@"History.txt");
            List<Account> client = File.ReadAllLines("test.txt").Select(x => x.ToAccount()).ToList();

            CurrentCounter acc_0 = new CurrentCounter(client[0].id, client[0].name, client[0].balance, client[0].c);
            acc_0.Debit(200, currency.EUR);

            CurrentCounter acc_1 = new CurrentCounter(client[1].id, client[1].name, client[1].balance, client[1].c);
            acc_1.Credit(740, currency.UAH);
            //
            CurrentCounter acc_2 = new CurrentCounter(client[2].id, client[2].name, client[2].balance, client[2].c);
            acc_2.Credit(423, currency.EUR);
            //
            CurrentCounter acc_3 = new CurrentCounter(client[3].id, client[3].name, client[3].balance, client[3].c);
            acc_3.Debit(500, currency.USD);
            //
            CurrentCounter acc_4 = new CurrentCounter(client[4].id, client[4].name, client[4].balance, client[4].c);
            acc_4.Debit(245, currency.EUR);
            //
            CurrentCounter acc_5 = new CurrentCounter(client[5].id, client[5].name, client[5].balance, client[5].c);
            acc_5.Debit(2852, currency.UAH);
            //
            CurrentCounter acc_6 = new CurrentCounter(client[6].id, client[6].name, client[6].balance, client[6].c);
            acc_6.Credit(500, currency.PLN);

            CurrentCounter acc_7 = new CurrentCounter(client[7].id, client[7].name, client[7].balance, client[7].c);
            acc_7.Debit(400, currency.EUR);

            CurrentCounter acc_8 = new CurrentCounter(client[8].id, client[8].name, client[8].balance, client[8].c);
            acc_8.Credit(250, currency.USD);
            //
            CurrentCounter acc_9 = new CurrentCounter(client[9].id, client[9].name, client[9].balance, client[9].c);
            acc_9.Credit(2000, currency.UAH);


            History show = new History("");
            show.Show();
        }
    }
}