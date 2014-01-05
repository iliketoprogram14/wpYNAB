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
        private static Payees payees;
        private static Categories categories;
        private static List<String> accounts;

        public static event EventHandler TransactionsUpdated;
        public static event EventHandler PayeesUpdated;
        public static event EventHandler CategoriesUpdated;

        #region Public Interface
        public static void Init()
        {
            transactions =
                (appSettings.Contains(TRANSACTION_KEY)) ?
                (Transactions)appSettings[TRANSACTION_KEY] :
                new Transactions();
            payees =
                (appSettings.Contains(PAYEE_KEY)) ?
                (Payees)appSettings[PAYEE_KEY] :
                new Payees();
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
            get {
                if (transactions == null)
                    transactions = (appSettings.Contains(TRANSACTION_KEY)) ?
                        (Transactions)appSettings[TRANSACTION_KEY] : new Transactions();
                return transactions;
            }
            set {
                transactions = value;
                NotifyTransactionsUpdated();
            }
        }

        public static List<String> MasterCategories() { return categories.CategoryList; }

        public static List<String> SubCategories(String category) {  return categories.SubCategories(category); }

        public static void ClearAllTransactions()
        {
            transactions.RemoveAll();
            SaveTransactions();
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
                    payees.AddFullCategory(payee, category, subcategory);

                    if (!accounts.Contains(account))
                        accounts.Add(account);
                }

                categories.Sort();
                accounts.Sort();
                payees.Sort();

                appSettings[ACCOUNTS_KEY] = accounts;
                appSettings[CATEGORIES_KEY] = categories;
                appSettings[PAYEE_KEY] = payees;
                appSettings.Save();

                NotifyCategoriesUpdated();
                NotifyPayeesUpdated();
                
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Import of register successful!");
                });
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
        #endregion

        private static void SaveTransactions(Action errorCallback=null)
        {
            try {
                appSettings[TRANSACTION_KEY] = transactions;
                appSettings.Save();
                NotifyTransactionsUpdated();
            } catch (IsolatedStorageException) {
                if (errorCallback != null)
                    errorCallback();
                else
                    MessageBox.Show(Constants.MSG_DELETE);
            }
        }

        #region Notification Functions
        private static void NotifyTransactionsUpdated()
        {
            var handler = TransactionsUpdated;
            if (handler != null) 
                handler(null, null);
        }

        private static void NotifyPayeesUpdated()
        {
            var handler = PayeesUpdated;
            if (handler != null)
                handler(null, null);
        }
        
        private static void NotifyCategoriesUpdated()
        {
            var handler = CategoriesUpdated;
            if (handler != null)
                handler(null, null);
        }
        #endregion
    }
}
