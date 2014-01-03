using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{ 
    public class Payee : INotifyPropertyChanged, IEquatable<Payee>, IComparable
    {
        private String payee;
        private Categories categories;

        public Payee()
        {
            payee = "";
            categories = new Categories();
        }

        public Payee(String _payee)
        {
            payee = _payee;
            categories = new Categories();
        }

        public Payee(String _payee, String category, String subcategory)
        {
            payee = _payee;
            categories = new Categories();
            categories.AddFullCategory(category, subcategory);
        }

        public String ThePayee
        {
            get { return payee; }
            set {
                if (payee == value)
                    return;
                payee = value;
                NotifyPropertyChanged("ThePayee");
            }
        }

        public Categories CategoryList
        {
            get { return categories; }
            set {
                if (categories == value)
                    return;
                categories = value;
                NotifyPropertyChanged("CategoryList");
            }
        }

        public void AddFullCategory(String category, String subcategory)
        {
            if (!categories.ContainsFullCategory(category, subcategory))
                categories.AddFullCategory(category, subcategory);
        }

        public bool Equals(Payee p2)
        {
            if (p2 == null)
                return false;
            return payee.Equals(p2.payee);
        }

        int IComparable.CompareTo(object obj)
        {
            Payee p = (Payee)obj;
            return String.Compare(payee, p.payee);
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
