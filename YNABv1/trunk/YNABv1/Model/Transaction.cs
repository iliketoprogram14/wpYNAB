using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Transaction : INotifyPropertyChanged
    {
        private enum DIRECTION { OUT, IN };

        private DIRECTION dir;
        private DateTime date;
        private String payee;
        private String category;
        private String subcategory;
        private String memo;
        private double amount;
        private String account;
        private Boolean transfer;

        public Transaction()
        {
            date = new DateTime();
            payee = "";
            category = "";
            subcategory = "";
            memo = "";
            account = "";
            amount = 0.0;
            dir = DIRECTION.OUT;
            transfer = false;
        }

        #region Get/Set
        public DateTime Date
        {
            get { return date; }
            set
            {
                if (date.Equals(value)) return;
                date = value;
                NotifyPropertyChanged("Date");
            }
        }

        public String Payee
        {
            get { return payee; }
            set
            {
                if (payee.Equals(value)) return;
                payee = value;
                NotifyPropertyChanged("Payee");
            }
        }

        public String Category
        {
            get { return category; }
            set
            {
                if (category.Equals(value)) return;
                category = value;
                NotifyPropertyChanged("Category");
            }
        }

        public String Subcategory
        {
            get { return subcategory; }
            set
            {
                if (subcategory.Equals(value)) return;
                subcategory = value;
                NotifyPropertyChanged("Subcategory");
            }
        }

        public String Memo
        {
            get { return memo; }
            set
            {
                if (memo.Equals(value)) return;
                memo  = value;
                NotifyPropertyChanged("Memo");
            }
        }

        public double Amount
        {
            get { return amount; }
            set
            {
                if (amount == value) return;
                amount = value;
                NotifyPropertyChanged("Amount");
            }
        }

        public String Account
        {
            get { return account; }
            set {
                if (account.Equals(value))
                    return;
                account = value;
                NotifyPropertyChanged("Account");
            }
        }

        public Boolean Outflow
        {
            get { return (dir == DIRECTION.OUT); }
            set
            {
                if (dir == DIRECTION.OUT) return;
                dir = DIRECTION.OUT;
                NotifyPropertyChanged("Outflow");
            }
        }

        public Boolean Inflow
        {
            get { return (dir == DIRECTION.IN); }
            set
            {
                if (dir == DIRECTION.IN) return;
                dir = DIRECTION.IN;
                NotifyPropertyChanged("Inflow");
            }
        }

        public Boolean Transfer
        {
            get { return transfer; }
            set
            {
                if (transfer == value) return;
                transfer = value;
                NotifyPropertyChanged("Transfer");
            }
        }
        #endregion

        public Boolean Equals(Transaction t2)
        {
            return (Outflow == t2.Outflow) && (date.Equals(t2.Date)) && (payee.Equals(t2.Payee)) &&
                (category.Equals(t2.Category)) && (subcategory.Equals(t2.subcategory)) &&
                (memo.Equals(t2.Memo)) && (amount == t2.Amount) && (account.Equals(t2.Account)) &&
                (transfer == t2.Transfer);
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
