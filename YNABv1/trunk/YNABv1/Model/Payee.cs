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
        #region Constructors
        public Payee()
        {
            PayeeName = "";
            CategoryList = new Categories();
        }

        public Payee(String _payee)
        {
            PayeeName = _payee;
            CategoryList = new Categories();
        }

        public Payee(String _payee, String category, String subcategory)
        {
            PayeeName = _payee;
            CategoryList = new Categories();
            CategoryList.AddFullCategory(category, subcategory);
        }
        #endregion

        #region Get/Set
        public String PayeeName
        {
            get; set;
        }

        public Categories CategoryList
        {
            get; set;
        }
        #endregion

        public void AddFullCategory(String category, String subcategory)
        {
            if (!CategoryList.ContainsFullCategory(category, subcategory))
                CategoryList.AddFullCategory(category, subcategory);
        }

        public bool Equals(Payee p2)
        {
            if (p2 == null)
                return false;
            return PayeeName.Equals(p2.PayeeName);
        }

        int IComparable.CompareTo(object obj)
        {
            Payee p = (Payee)obj;
            return String.Compare(PayeeName, p.PayeeName);
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
