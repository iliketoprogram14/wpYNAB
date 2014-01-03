using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Payee : IEquatable<Payee>
    {
        private String payee;
        private Categories categories;

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
    }
}
