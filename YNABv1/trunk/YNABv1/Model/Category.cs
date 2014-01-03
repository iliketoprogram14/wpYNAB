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
        private String masterCategory;
        private List<String> subCategories;

        public Category()
        {
            masterCategory = "";
            subCategories = new List<string>();
        }

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

        public String TheMasterCategory { 
            get { return masterCategory; } 
            set {
                if (masterCategory == value)
                    return;
                masterCategory = value;
                NotifyPropertyChanged("TheMasterCategory");
            }
        }

        public List<String> SubCategoryList { 
            get { return subCategories; } 
            set {
                if (subCategories == value)
                    return;
                subCategories = value;
                NotifyPropertyChanged("SubCategoryList");
            } 
        }

        public List<String> SubCategories() { return subCategories; }
            
        public string MainCategory() { return masterCategory; }

        public bool HasSubCategory(String subCategory) { return subCategories.Contains(subCategory); }

        public void AddSubCategory(String subCategory) 
        {
            subCategories.Add(subCategory);
        }

        public Boolean Equals(Category c)
        {
            return masterCategory.Equals(c.masterCategory);
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
