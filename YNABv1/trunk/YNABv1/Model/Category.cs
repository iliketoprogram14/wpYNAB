﻿using System;
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
    public class Category : INotifyPropertyChanged, IEquatable<Category>
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public Category()
        {
            MasterCategory = "";
            SubCategories = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        public Category(String category)
        {
            MasterCategory = category;
            SubCategories = new List<String>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
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
