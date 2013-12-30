using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Category
    {
        private String masterCategory;
        private List<String> subCategories;

        public Category(String category)
        {
            masterCategory = category;
            subCategories = new List<String>();
        }

        public Category(String category, String subCategory)
        {
            masterCategory = category;
            subCategories = new List<String>();
            subCategories.Add(subCategory);
        }
    }
}
