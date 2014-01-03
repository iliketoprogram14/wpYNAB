using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Payees : INotifyPropertyChanged
    {
        List<Payee> payeeList;

        public Payees()
        {
            payeeList = new List<Payee>();
        }

        public int Count() { return payeeList.Count; }

        public List<Payee> PayeeList
        {
            get { return payeeList; }
            set {
                if (payeeList == value)
                    return;
                payeeList = value;
                NotifyPropertyChanged("PayeeList");
            }
        }

        public bool ContainsPayee(string payee)
        {
            Payee p = new Payee(payee);
            return payeeList.Contains(p);
        }

        public void AddPayee(string payee)
        {
            if (!ContainsPayee(payee))
                payeeList.Add(new Payee(payee));
        }

        public void AddFullCategory(string payee, string category, string subcategory)
        {
            if (!ContainsPayee(payee))
                payeeList.Add(new Payee(payee, category, subcategory));
            else {
                Payee p = new Payee(payee);
                int i = payeeList.IndexOf(p);
                p = payeeList.ElementAt(i);
                payeeList.RemoveAt(i);
                p.AddFullCategory(category, subcategory);
                payeeList.Add(p);
            }
        }

        public void Sort()
        {
            payeeList.Sort();
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
