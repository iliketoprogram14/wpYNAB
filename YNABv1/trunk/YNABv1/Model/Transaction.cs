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
        private String memo;
        private float amount;
        private String account;
        private Boolean transfer;

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

        public float Amount
        {
            get { return amount; }
            set
            {
                if (amount == value) return;
                amount = value;
                NotifyPropertyChanged("Amount");
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
