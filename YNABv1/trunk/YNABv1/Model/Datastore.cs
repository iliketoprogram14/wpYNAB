using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Threading;
using CsvHelper;

namespace YNABv1.Model
{
    public static class Datastore
    {
        private const string TRANSACTION_KEY = "YNABcompanion.Transactions";
        private const string PAYEE_KEY = "YNABcompanion.Payees";
        private const string CATEGORIES_KEY = "YNABcompanion.Categories";
        private const string ACCOUNTS_KEY = "YNABcompanion.Accounts";
        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        private static Transactions transactions;
        private static List<Payee> payees;
        private static Categories categories;
        private static List<String> accounts;

        public static event EventHandler TransactionsUpdated;

        public static void Init()
        {
            transactions =
                (appSettings.Contains(TRANSACTION_KEY)) ?
                (Transactions)appSettings[TRANSACTION_KEY] :
                new Transactions();
            payees =
                (appSettings.Contains(PAYEE_KEY)) ?
                (List<Payee>)appSettings[PAYEE_KEY] :
                new List<Payee>();
            categories =
                (appSettings.Contains(CATEGORIES_KEY)) ?
                (Categories)appSettings[CATEGORIES_KEY] :
                new Categories();
            accounts =
                (appSettings.Contains(ACCOUNTS_KEY)) ?
                (List<String>)appSettings[ACCOUNTS_KEY] :
                new List<String>();
        }

        public static Transactions Transactions
        {
            get
            {
                if (transactions == null) {
                    if (appSettings.Contains(TRANSACTION_KEY)) {
                        transactions = (Transactions)appSettings[TRANSACTION_KEY];
                    } else {
                        transactions = new Transactions();
                    }
                }
                return transactions;
            }
            set
            {
                transactions = value;
                NotifyTransactionsUpdated();
            }
        }

        public static void ClearAllTransactions()
        {
            transactions.RemoveAll();
            SaveTransactions(null);
        }

        private static void SaveTransactions(Action errorCallback)
        {
            try {
                appSettings[TRANSACTION_KEY] = transactions;
                appSettings.Save();
                NotifyTransactionsUpdated();
            } catch (IsolatedStorageException) {
                if (errorCallback != null)
                    errorCallback();
                else
                    MessageBox.Show(Constants.DELETE_MSG);
            }
        }

        public static void ParseRegister(String csvRegister)
        {
            ThreadPool.QueueUserWorkItem(context => {
                CsvReader reader = new CsvReader(new StringReader(csvRegister));
                while (reader.Read()) {
                    var account = reader.GetField("Account");
                    var category = reader.GetField("Master Category");
                    var subcategory = reader.GetField("Sub Category");
                    var payee = reader.GetField("Payee");

                    categories.AddFullCategory(category, subcategory);

                    Payee p = new Payee(payee);
                    if (payees.Contains(p)) {
                        int i = payees.IndexOf(p);
                        p = payees.ElementAt(i);
                        payees.RemoveAt(i);
                        p.AddFullCategory(category, subcategory);
                        payees.Add(p);
                    } else
                        payees.Add(new Payee(payee, category, subcategory));

                    if (!accounts.Contains(account))
                        accounts.Add(account);
                }
                appSettings[PAYEE_KEY] = payees;
                appSettings[CATEGORIES_KEY] = categories;
                appSettings[ACCOUNTS_KEY] = accounts;
                appSettings.Save();
            });
        }

        public static SaveResult AddTransaction(Transaction t, Action errorCallback)
        {
            var result = new SaveResult();
            transactions.Add(t);
            result.SaveSuccessful = true;
            SaveTransactions(errorCallback);
            // after save, look for new payees, categories, and accounts
            // save if necessary
            return result;
        }

        public static SaveResult DeleteTransaction(Transaction t, Action errorCallback)
        {
            var result = new SaveResult();
            transactions.Remove(t);
            result.SaveSuccessful = true;
            SaveTransactions(errorCallback);
            return result;
        }

        private static void NotifyTransactionsUpdated()
        {
            var handler = TransactionsUpdated;
            if (handler != null) 
                handler(null, null);
        }
    }
}
