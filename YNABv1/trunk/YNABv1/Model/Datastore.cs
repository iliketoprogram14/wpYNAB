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
    /// <summary>
    /// 
    /// </summary>
    public static class Datastore
    {
        private const string TRANSACTION_KEY = "YNABcompanion.Transactions";
        private const string PAYEE_KEY = "YNABcompanion.Payees";
        private const string CATEGORIES_KEY = "YNABcompanion.Categories";
        private const string ACCOUNTS_KEY = "YNABcompanion.Accounts";
        private static readonly IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        private static Transactions transactions;
        private static Payees payees;
        private static Categories categories;

        public static event EventHandler TransactionsUpdated;
        public static event EventHandler PayeesUpdated;
        public static event EventHandler CategoriesUpdated;

        #region Public Interface
        public static void Init()
        {
            transactions =
                (AppSettings.Contains(TRANSACTION_KEY)) ?
                (Transactions)AppSettings[TRANSACTION_KEY] :
                new Transactions();
            payees =
                (AppSettings.Contains(PAYEE_KEY)) ?
                (Payees)AppSettings[PAYEE_KEY] :
                new Payees();
            categories =
                (AppSettings.Contains(CATEGORIES_KEY)) ?
                (Categories)AppSettings[CATEGORIES_KEY] :
                new Categories();
            Accounts =
                (AppSettings.Contains(ACCOUNTS_KEY)) ?
                (List<String>)AppSettings[ACCOUNTS_KEY] :
                new List<String>();
        }

        public static Transactions Transactions
        {
            get {
                if (transactions == null)
                    transactions = (AppSettings.Contains(TRANSACTION_KEY)) ?
                        (Transactions)AppSettings[TRANSACTION_KEY] : new Transactions();
                return transactions;
            }
            set {
                transactions = value;
                NotifyTransactionsUpdated();
            }
        }

        public static List<String> Accounts
        {
            get; set;
        }

        public static List<String> MasterCategories() {
            return categories.CategoryList;
        }

        public static List<String> SubCategories(String category) {
            return categories.SubCategories(category);
        }

        public static List<Category> TieredCategories()
        {
            List<Category> catList = new List<Category>();
            foreach (String cat in categories.CategoryList) {
                catList.Add(new Category(cat));
                List<String> subcats = categories.SubCategories(cat);

                foreach (String subcat in subcats)
                    catList.Add(new Category(cat, subcat));
            }
            return catList;
        }

        public static void AddCategory(String category)
        {
            categories.AddCategory(category);
            SaveCategories();
        }

        public static void AddSubcategory(String category, String subcategory)
        {
            categories.AddFullCategory(category, subcategory);
            SaveCategories();
        }

        public static void ClearAllTransactions()
        {
            transactions.RemoveAll();
            SaveTransactions();
        }

        public static void Parse(String csvRegister)
        {
            ThreadPool.QueueUserWorkItem(context => {
                CsvReader reader = new CsvReader(new StringReader(csvRegister));

                reader.Read();
                List<String> headers = new List<String>(reader.FieldHeaders);
                bool isBudget = true; // false if isRegister
                bool hasAccount = headers.Contains("Account");
                bool hasMasterCategory = headers.Contains("Master Category");
                bool hasAllBudgetHeaders =
                    headers.Contains("Month") &&
                    headers.Contains("Category") &&
                    headers.Contains("Master Category") &&
                    headers.Contains("Sub Category") &&
                    headers.Contains("Budgeted") &&
                    headers.Contains("Outflows") &&
                    headers.Contains("Category Balance");
                bool hasAllRegisterHeaders = 
                    headers.Contains("Account") &&
                    headers.Contains("Flag") &&
                    headers.Contains("Check Number") &&
                    headers.Contains("Date") &&
                    headers.Contains("Payee") &&
                    headers.Contains("Category") &&
                    headers.Contains("Master Category") &&
                    headers.Contains("Sub Category") &&
                    headers.Contains("Memo") &&
                    headers.Contains("Outflow") &&
                    headers.Contains("Inflow") &&
                    headers.Contains("Cleared") &&
                    headers.Contains("Running Balance");
                if (hasAllRegisterHeaders) {
                    ParseRegister(reader);
                    isBudget = false;
                } else if (hasAllBudgetHeaders) {
                    ParseBudget(reader);
                    isBudget = true;
                } else {
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("Import not successful.  Please import either an exported YNAB budget or register.");
                    });
                    return;
                }

                categories.Sort();
                Accounts.Sort();
                payees.Sort();

                AppSettings[ACCOUNTS_KEY] = Accounts;
                AppSettings[CATEGORIES_KEY] = categories;
                AppSettings[PAYEE_KEY] = payees;
                AppSettings.Save();

                NotifyCategoriesUpdated();
                NotifyPayeesUpdated();
                
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Import of YNAB " + ((isBudget) ? "budget" : "register") +
                        " was successful! Consider also importing the " + ((!isBudget) ? "budget" : "register") + " as well.");
                });
            });
        }

        public static SaveResult AddTransaction(Transaction t, Action errorCallback)
        {
            var result = new SaveResult();
            transactions.Add(t);
            result.SaveSuccessful = true;
            SaveTransactions(errorCallback);
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

        private static void ParseRegister(CsvReader reader)
        {
            do {
                var account = reader.GetField("Account");
                var category = reader.GetField("Master Category");
                var subcategory = reader.GetField("Sub Category");
                var payee = reader.GetField("Payee");

                if (category != "")
                    categories.AddFullCategory(category, subcategory);
                if (payee != "")
                    payees.AddFullCategory(payee, category, subcategory);

                if (account != "" && !Accounts.Contains(account))
                    Accounts.Add(account);
            } while (reader.Read());
        }

        private static void ParseBudget(CsvReader reader)
        {
            do {
                var category = reader.GetField("Master Category");
                var subcategory = reader.GetField("Sub Category");

                if (category != "")
                    categories.AddFullCategory(category, subcategory);
            } while (reader.Read());
        }

        private static void SaveTransactions(Action errorCallback = null)
        {
            try {
                AppSettings[TRANSACTION_KEY] = transactions;
                AppSettings.Save();
                NotifyTransactionsUpdated();
            } catch (IsolatedStorageException) {
                if (errorCallback != null)
                    errorCallback();
                else
                    MessageBox.Show(Constants.MSG_DELETE);
            }
        }

        private static void SaveCategories(Action errorCallback = null)
        {
            try {
                AppSettings[CATEGORIES_KEY] = categories;
                AppSettings.Save();
                NotifyCategoriesUpdated();
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
