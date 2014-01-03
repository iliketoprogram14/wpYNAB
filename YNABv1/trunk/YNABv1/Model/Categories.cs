using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Categories
    {
        private Dictionary<String, Category> categoryObjList;
        private List<String> categoryList;

        public Categories()
        {
            categoryObjList = new Dictionary<String, Category>();
            categoryList = new List<string>();
        }

        public bool ContainsCategory(String category)
        {
            return categoryList.Contains(category);
        }

        public bool ContainsFullCategory(String category, String subCategory)
        {
            return ContainsCategory(category) && categoryObjList[category].HasSubCategory(subCategory);
        }

        public void AddCategory(String category)
        {
            categoryObjList[category] = new Category(category);
            categoryList.Add(category);
        }

        public void AddFullCategory(String category, String subcategory)
        {
            if (!ContainsCategory(category))
                AddCategory(category);
            Category c = categoryObjList[category];
            if (!c.HasSubCategory(subcategory)) {
                c.AddSubCategory(subcategory);
                categoryObjList[category] = c;
            }
        }
    }
}
