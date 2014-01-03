using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Categories : INotifyPropertyChanged
    {
        private Dictionary<String, Category> categoryObjList;
        private List<String> categoryList;

        public Categories()
        {
            categoryObjList = new Dictionary<String, Category>();
            categoryList = new List<string>();
        }

        public List<String> CategoryList { 
            get { return categoryList; }
            set {
                if (categoryList == value)
                    return;
                categoryList = value;
                NotifyPropertyChanged("CategoryList");
            } 
        }

        public Dictionary<String, Category> CategoryObjList {
            get { return categoryObjList; }
            set {
                if (categoryObjList == value)
                    return;
                categoryObjList = value;
                NotifyPropertyChanged("CategoryObjList");
            } 
        }

        public List<String> MasterCategories() { return categoryList; }

        public List<String> SubCategories(String category) { return categoryObjList[category].SubCategories(); }

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

        public void Sort()
        {
            categoryList.Sort();
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
