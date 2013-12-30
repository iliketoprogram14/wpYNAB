using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;

namespace YNABv1.Model
{
    public static class Datastore
    {
        private const string TRANSACTION_KEY = "YNABcompanion.Transactions";
        private const string PAYEE_KEY = "YNABcompanion.Payees";
        private const string CATEGORIES_KEY = "YNABcompanion.Categories";
        private const string ACCOUNTS_KEY = "YNABcompanion.Accounts";
        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        private static List<Transaction> transactions;
        private static List<Payee> payees;
        private static List<Category> categories;
        private static List<String> accounts;

        private static event EventHandler TransactionsUpdated;

        private static void SaveTransactions(Action errorCallback)
        {
            try
            {
                appSettings[TRANSACTION_KEY] = transactions;
                appSettings.Save();
                NotifyTransactionsUpdated();
            }
            catch (IsolatedStorageException)
            {
                errorCallback();
            }
        }

        public static SaveResult AddTransaction(Transaction t, Action errorCallback)
        {
            var result = new SaveResult();
            transactions.Add(t);
            result.SaveSuccessful = true;
            // after save, look for new payees, categories, and accounts
            // save if necessary
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
