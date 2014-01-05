using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Transaction : INotifyPropertyChanged, IEquatable<Transaction>, IComparable
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

        public String FullCategory
        {
            get { return (category == "") ? "" : category + ": " + subcategory; }
            set {
                String fullCategory = category + ": " + subcategory;
                if (fullCategory.Equals(value))
                    return;
                if (value == "") {
                    category = "";
                    subcategory = "";
                } else {
                    String[] splitString = value.Split(':');
                    category = splitString[0].Trim(' ');
                    subcategory = splitString[1].Trim(' ');
                }
                NotifyPropertyChanged("Category");
                NotifyPropertyChanged("SubCategory");
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

        public double SignedAmount
        {
            get { return (dir == DIRECTION.OUT ? -1 : 1) * amount; }
            set {
                if (((dir == DIRECTION.OUT ? -1 : 1) * amount) == value)
                    return;
                amount = (value < 0) ? value * -1 : value;
                dir = (value < 0) ? DIRECTION.OUT : DIRECTION.IN;
                NotifyPropertyChanged("Amount");
                NotifyPropertyChanged("Direction");
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
            set {
                if (value && dir == DIRECTION.OUT)
                    return;
                dir = (value) ? DIRECTION.OUT : DIRECTION.IN;
                NotifyPropertyChanged("Outflow");
                NotifyPropertyChanged("Inflow");
            }
        }

        public Boolean Inflow
        {
            get { return (dir == DIRECTION.IN); }
            set { Outflow = !value; }
        }

        public Boolean Transfer
        {
            get { return transfer; }
            set
            {
                if (transfer == value)
                    return;
                transfer = value;
                NotifyPropertyChanged("Transfer");
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Returns a deep copy of this transaction.
        /// </summary>
        /// <returns>A deep copy of this transaxtion.</returns>
        public Transaction DeepCopy()
        {
            Transaction t = new Transaction();
            t.dir = dir;
            t.date = date;
            t.payee = payee;
            t.category = category;
            t.subcategory = subcategory;
            t.memo = memo;
            t.amount = amount;
            t.account = account;
            t.transfer = transfer;
            return t;
        }

        /// <summary>
        /// If this is a transfer, returns the inverse transaction of this 
        /// transaction.  For example, for a transfer from A to B of an 
        /// outflow of $50, a transfer from B to A of an inflow of $50 is 
        /// returned.  For transactions that aren't transfer, this returns an
        /// unmodified deep copy.
        /// </summary>
        /// <returns>The inverse transaction of this object</returns>
        public Transaction InverseTransfer()
        {
            Transaction inverse = DeepCopy();
            if (!transfer)
                return inverse;

            String fromAccount = inverse.Account;
            String toAccount = inverse.Account.Replace("Transfer : ", "");
            inverse.Account = toAccount;
            inverse.Payee = "Transfer : " + fromAccount;
            inverse.Outflow = !Outflow;

            return inverse;
        }

        /// <summary>
        /// Return the csv equivalent of the transaction.
        /// </summary>
        /// <returns>A csv string representation of the transaction</returns>
        public String GetCsv()
        {
            // Structure: Date,Payee,Category,Memo,Outflow,Inflow
            String dateStr = String.Format("{0:d}", date.ToString());
            String fullCategory = this.FullCategory;
            String outflowStr = this.Outflow ? amount.ToString() : "";
            String inflowStr = this.Inflow ? amount.ToString() : "";
            return dateStr + "," + payee + "," + FullCategory + "," + memo + "," +
                outflowStr + "," + inflowStr + "\n";
        }
        #endregion

        public Boolean Equals(Transaction t2)
        {
            return (Outflow == t2.Outflow) && (date.Equals(t2.Date)) && (payee.Equals(t2.Payee)) &&
                (category.Equals(t2.Category)) && (subcategory.Equals(t2.subcategory)) &&
                (memo.Equals(t2.Memo)) && (amount == t2.Amount) && (account.Equals(t2.Account)) &&
                (transfer == t2.Transfer);
        }

        int IComparable.CompareTo(object obj)
        {
            Transaction t2 = obj as Transaction;
            return date.CompareTo(t2.date);
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
