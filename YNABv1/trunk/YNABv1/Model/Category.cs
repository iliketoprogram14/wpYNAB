using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Model
{
    public class Category : INotifyPropertyChanged, IEquatable<Category>
    {
        #region Constructors
        public Category()
        {
            MasterCategory = "";
            SubCategories = new List<string>();
        }

        public Category(String category)
        {
            MasterCategory = category;
            SubCategories = new List<String>();
        }

        public Category(String category, String subCategory)
        {
            MasterCategory = category;
            SubCategories = new List<String>();
            SubCategories.Add(subCategory);
        }
        #endregion

        #region Get/Set
        public String MasterCategory { get; set; }

        public List<String> SubCategories { get; set;  }
        #endregion

        #region Public Interface
        public bool HasSubCategory(String subCategory) 
        { 
            return SubCategories.Contains(subCategory);
        }

        public void AddSubCategory(String subCategory) 
        {
            SubCategories.Add(subCategory);
        }

        public void RemoveSubCategory(String subCategory)
        {
            SubCategories.Remove(subCategory);
        }
        #endregion

        public Boolean Equals(Category c)
        {
            return MasterCategory.Equals(c.MasterCategory);
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
