using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Payee
    {
        private String payee;
        private List<Tuple<String, String>> categories;

        public Payee(String _payee)
        {
            payee = _payee;
            categories = new List<Tuple<String, String>>();
        }

        public Payee(String _payee, String category, String subcategory)
        {
            payee = _payee;
            categories = new List<Tuple<String, String>>();
            categories.Add(new Tuple<String, String>(category, subcategory));
        }
    }
}
