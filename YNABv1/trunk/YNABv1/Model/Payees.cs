using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Payees : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public Payees()
        {
            PayeeList = new List<Payee>();
        }

        #region Public Interface
        public List<Payee> PayeeList
        {
            get; set;
        }

        public int Count() { return PayeeList.Count; }

        public bool ContainsPayee(string payee)
        {
            return PayeeList.Contains(new Payee(payee));
        }

        public void AddPayee(string payee)
        {
            if (!ContainsPayee(payee))
                PayeeList.Add(new Payee(payee));
        }

        public void AddFullCategory(string payee, string category, string subcategory)
        {
            if (!ContainsPayee(payee))
                PayeeList.Add(new Payee(payee, category, subcategory));
            else {
                Payee p = new Payee(payee);
                int i = PayeeList.IndexOf(p);
                p = PayeeList.ElementAt(i);
                PayeeList.RemoveAt(i);
                p.AddFullCategory(category, subcategory);
                PayeeList.Add(p);
            }
        }

        public void Sort()
        {
            PayeeList.Sort();
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
