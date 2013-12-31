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
        
        public ObservableCollection<Transaction> TransactionHistory
        {
            get { return transactions; }
            set
            {
                if (transactions == value)
                    return;
                transactions = value;
                /*if (transactions != null) {
                    transactions.CollectionChanged += delegate { };
                }*/
                NotifyPropertyChanged("TransactionHistory");
            }
        }

        public void Add(Transaction t)
        {
            transactions.Add(t);
        }

        public void Remove(Transaction t)
        {
            transactions.Remove(t);
        }

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
