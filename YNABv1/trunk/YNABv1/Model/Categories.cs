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
    public class Categories : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public Categories()
        {
            CategoryObjList = new Dictionary<String, Category>();
            CategoryList = new List<string>();
        }

        #region Get/Set
        public List<String> CategoryList
        {
            get; set;
        }

        public Dictionary<String, Category> CategoryObjList
        {
            get; set;
        }
        #endregion

        #region Public Interface
        public List<String> SubCategories(String category) 
        { 
            return CategoryObjList[category].SubCategories; 
        }

        public bool ContainsCategory(String category)
        {
            return CategoryList.Contains(category);
        }

        public bool ContainsFullCategory(String category, String subCategory)
        {
            return ContainsCategory(category) && CategoryObjList[category].HasSubCategory(subCategory);
        }

        public void AddCategory(String category)
        {
            CategoryObjList[category] = new Category(category);
            CategoryList.Add(category);
            Sort();
        }

        public void AddFullCategory(String category, String subcategory)
        {
            if (!ContainsCategory(category))
                AddCategory(category);

            Category c = CategoryObjList[category];
            if (!c.HasSubCategory(subcategory)) {
                c.AddSubCategory(subcategory);
                c.SubCategories.Sort();
                CategoryObjList[category] = c;
            }

            Sort();
        }

        public void Sort()
        {
            CategoryList.Sort();
        }
        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
