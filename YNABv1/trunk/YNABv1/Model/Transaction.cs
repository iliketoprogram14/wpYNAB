using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Transaction : INotifyPropertyChanged, IEquatable<Transaction>, IComparable
    {
        private enum DIRECTION { OUT, IN }

        private DIRECTION dir;

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public Transaction()
        {
            Date = DateTime.Now;
            Payee = "";
            CategoryObj = new Category();
            Memo = "";
            Account = "";
            Amount = 0.0;
            dir = DIRECTION.OUT;
            Transfer = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transfer"></param>
        public Transaction(bool transfer)
        {
            Date = DateTime.Now;
            Payee = "";
            CategoryObj = new Category();
            Memo = "";
            Account = "";
            Amount = 0.0;
            dir = DIRECTION.OUT;
            Transfer = transfer;
        }
        #endregion

        #region Get/Set
        public DateTime Date
        {
            get; set;
        }

        public String Payee
        {
            get; set;
        }

        public Category CategoryObj
        {
            get; set;
        }

        public String FullCategory
        {
            get {
                return (CategoryObj.MasterCategory == "") ? "" :
                    CategoryObj.MasterCategory + ": " + CategoryObj.SubCategory;
            }
            set {
                String fullCategory = CategoryObj.MasterCategory + ": " + CategoryObj.SubCategory;
                if (fullCategory.Equals(value))
                    return;
                if (value == "")
                    CategoryObj = new Category();
                else {
                    String[] splitString = value.Split(':');
                    String category = splitString[0].Trim(' ');
                    String subcategory = splitString[1].Trim(' ');
                    CategoryObj = new Category(category, subcategory);
                }
                NotifyPropertyChanged("CategoryObj");
            }
        }

        public String Memo
        {
            get; set;
        }

        public double Amount
        {
            get; set;
        }

        public double SignedAmount
        {
            get { return (dir == DIRECTION.OUT ? -1 : 1) * Amount; }
            set {
                if (((dir == DIRECTION.OUT ? -1 : 1) * Amount) == value)
                    return;
                Amount = (value < 0) ? value * -1 : value;
                dir = (value < 0) ? DIRECTION.OUT : DIRECTION.IN;
                NotifyPropertyChanged("Amount");
                NotifyPropertyChanged("Direction");
            }
        }

        public String Account
        {
            get; set;
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
            get; set;
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
            t.Date = Date;
            t.Payee = Payee;
            t.CategoryObj = CategoryObj.DeepCopy();
            t.Memo = Memo;
            t.Amount = Amount;
            t.Account = Account;
            t.Transfer = Transfer;
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
            if (!Transfer)
                return inverse;

            String fromAccount = inverse.Account;
            String toAccount = inverse.Payee.Replace("Transfer : ", "");
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
            String dateStr = String.Format("{0:d}", Date);
            String fullCategory = this.FullCategory;
            String outflowStr = this.Outflow ? Amount.ToString() : "";
            String inflowStr = this.Inflow ? Amount.ToString() : "";
            return dateStr + "," + Payee + "," + FullCategory + "," + Memo + "," +
                outflowStr + "," + inflowStr + "\n";
        }
        #endregion

        public Boolean Equals(Transaction t2)
        {
            return (Outflow == t2.Outflow) && (Date.Equals(t2.Date)) && (Payee.Equals(t2.Payee)) &&
                CategoryObj.Equals(t2.CategoryObj) &&
                (Memo.Equals(t2.Memo)) && (Amount == t2.Amount) && (Account.Equals(t2.Account)) &&
                (Transfer == t2.Transfer);
        }

        int IComparable.CompareTo(object obj)
        {
            Transaction t2 = obj as Transaction;
            return Date.CompareTo(t2.Date);
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
