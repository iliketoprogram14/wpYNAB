using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace YNABv1.Model
{
    public class Transactions : INotifyPropertyChanged
    {
        private ObservableCollection<Transaction> transactions;

        public Transactions() {
            transactions = new ObservableCollection<Transaction>();
        }

        #region Get/Set
        public ObservableCollection<Transaction> TransactionHistory
        {
            get { return transactions; }
            set {
                if (transactions == value)
                    return;
                transactions = value;
                NotifyPropertyChanged("TransactionHistory");
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public void Add(Transaction t)
        {
            transactions.Add(t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public void Remove(Transaction t)
        {
            transactions.Remove(t);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveAll()
        {
            transactions.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SortByDate()
        {
            List<Transaction> transactionList = transactions.ToList<Transaction>();
            transactionList.Sort();
            TransactionHistory = new ObservableCollection<Transaction>(transactionList);
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
