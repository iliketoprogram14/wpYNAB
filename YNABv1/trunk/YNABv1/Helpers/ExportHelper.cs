using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YNABv1.Model;

namespace YNABv1.Helpers
{
    public static class ExportHelper
    {
        public static Dictionary<String, String> GetCsvStrings()
        {
            Dictionary<String, String> dict = new Dictionary<String, String>();
            ObservableCollection<Transaction> transactions = Datastore.Transactions.TransactionHistory;
            List<Transaction> transList = transactions.ToList();

            foreach (Transaction t in transList)
                dict[t.Account] = (dict.ContainsKey(t.Account)) ? 
                    dict[t.Account] + t.GetCsv() : "Date,Payee,Category,Memo,Outflow,Inflow\n" + t.GetCsv();
            return dict;
        }

    }
}
