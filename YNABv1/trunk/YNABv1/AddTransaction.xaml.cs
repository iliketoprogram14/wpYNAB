using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using YNABv1.Model;
using YNABv1.Helpers;
using System.Diagnostics;
using GoogleAds;

namespace YNABv1
{
    public partial class AddTransaction : PhoneApplicationPage
    {
        private const string CURRENT_TRANS_KEY = "CurrentTransaction";
        private const string HAS_UNSAVED_CHANGES_KEY = "HasUnsavedChanges";
        private Transaction currentTransaction;
        private bool hasUnsavedChanges;
        private TextBox textboxWithFocus;
        private RadioButton buttonWithFocus;
        private Transaction transactionToEdit = null;
        private IDictionary<string, object> phoneState = PhoneApplicationService.Current.State;

        /// <summary>
        ///
        /// </summary>
        public AddTransaction()
        {
            InitializeComponent();
            GotFocus += AddTransactionPage_GotFocus;
        }

        /// <summary>
        ///
        /// </summary>
        private void InitializePageState()
        {
            if (State.ContainsKey(CURRENT_TRANS_KEY))
                currentTransaction = State[CURRENT_TRANS_KEY] as Transaction;
            else if (transactionToEdit != null)
                currentTransaction = transactionToEdit.DeepCopy();
            else
                currentTransaction = new Transaction();

            DataContext = currentTransaction;
            hasUnsavedChanges = State.ContainsKey(HAS_UNSAVED_CHANGES_KEY) && (bool)State[HAS_UNSAVED_CHANGES_KEY];

            if (Datastore.Accounts.Count == 0) {
                AccountTextBox.Visibility = Visibility.Visible;
                AccountListPicker.Visibility = Visibility.Collapsed;
            } else {
                List<String> accounts = new List<String>(Datastore.Accounts);
                accounts.Sort();
                accounts.Add("New...");
                AccountListPicker.ItemsSource = accounts;
                AccountListPicker.Visibility = Visibility.Visible;
                AccountTextBox.Visibility = Visibility.Collapsed;
                if (transactionToEdit != null)
                    AccountListPicker.SelectedItem = currentTransaction.Account;
            }

            List<String> masterCats = Datastore.MasterCategories();
            if (Datastore.MasterCategories().Count == 0) {
                CategoryTextBox.Visibility = Visibility.Visible;
                SubCategoryTextBox.Visibility = Visibility.Visible;
                SubCategoryTextBlock.Visibility = Visibility.Visible;
                CategoryListPicker.Visibility = Visibility.Collapsed;
            } else {
                List<Category> cats = Datastore.TieredCategories();
                cats.Add(new Category("New..."));
                CategoryListPicker.ItemsSource = cats;
                CategoryListPicker.Visibility = Visibility.Visible;
                CategoryTextBox.Visibility = Visibility.Collapsed;
                SubCategoryTextBox.Visibility = Visibility.Collapsed;
                SubCategoryTextBlock.Visibility = Visibility.Collapsed;

                if (transactionToEdit != null) {
                    Category c = transactionToEdit.CategoryObj;
                    CategoryListPicker.SelectedIndex = cats.IndexOf(c);
                }
            }

            Ad.Refresh();
        }

        #region Navigation Events

        /// <summary>
        /// Called when navigating to this page; loads the car data from storage
        /// and then initializes the page state.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (PhoneApplicationService.Current.State.ContainsKey(Constants.NAV_PARAM_TRANSACTION)) {
                transactionToEdit = phoneState[Constants.NAV_PARAM_TRANSACTION] as Transaction;
                phoneState.Remove(Constants.NAV_PARAM_TRANSACTION);
            } 

            // Initialize the page state only if it is not already initialized,
            // and not when the application was deactivated but not tombstoned
            // (returning from being dormant).
            if (DataContext == null) {
                InitializePageState();
                UpdateAd();
            }

            // Delete temporary storage to avoid unnecessary storage costs.
            State.Clear();
        }

        /// <summary>
        /// Called when navigating away from this page; stores the fill-up data
        /// values and a value that indicates whether there are unsaved changes.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Do not cache the page state when navigating backward
            // or when there are no unsaved changes.
            if (e.Uri.OriginalString.Equals("//MainPage.xaml") || !hasUnsavedChanges)
                return;

            CommitItemWithFocus();
            State[CURRENT_TRANS_KEY] = currentTransaction;
            State[HAS_UNSAVED_CHANGES_KEY] = hasUnsavedChanges;
        }

        /// <summary>
        /// Displays a warning dialog box if the user presses the back button
        /// and there are unsaved changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (!hasUnsavedChanges)
                return;

            var result = MessageBox.Show("You are about to discard your changes. Continue?", "Warning", MessageBoxButton.OKCancel);
            e.Cancel = (result == MessageBoxResult.Cancel);
        }
        #endregion Navigation Events

        /// <summary>
        /// Ensures that any changes to text box values are committed.
        /// </summary>
        private void CommitItemWithFocus()
        {
            if (textboxWithFocus == null)
                return;

            hasUnsavedChanges = true;

            BindingExpression expression = textboxWithFocus.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateSource();

            if (buttonWithFocus == null)
                return;

            expression = buttonWithFocus.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (expression != null)
                expression.UpdateSource();
        }

        private bool CheckParams()
        {
            if (AccountTextBox.Visibility == Visibility.Visible) {
                if (string.IsNullOrWhiteSpace(AccountTextBox.Text)) {
                    MessageBox.Show("The account is required.");
                    return false;
                }
            } else {
                if (((String)AccountListPicker.SelectedItem).Equals("")) {
                    MessageBox.Show("The account is required.");
                    return false;
                }
                currentTransaction.Account = (String)AccountListPicker.SelectedItem;
            }

            if (string.IsNullOrWhiteSpace(PayeeTextBox.Text)) {
                MessageBox.Show("The payee is required.");
                return false;
            }

            if (CategoryTextBox.Visibility == Visibility.Visible) {
                if (string.IsNullOrWhiteSpace(CategoryTextBox.Text)) {
                    MessageBox.Show("The category is required.");
                    return false;
                }
            } else {
                if (((Category)CategoryListPicker.SelectedItem).MasterCategory.Equals("")) {
                    MessageBox.Show("The category is required.");
                    return false;
                }
                currentTransaction.CategoryObj = (Category)CategoryListPicker.SelectedItem;
            }

            bool outflow = (bool)OutflowButton.IsChecked;
            bool inflow = (bool)InflowButton.IsChecked;
            if ((!outflow && !inflow) || (outflow && inflow)) {
                MessageBox.Show("The transaction must be an outflow or an inflow.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(AmountTextBox.Text)) {
                MessageBox.Show("The amount is required.");
                return false;
            }

            double amountVal;
            if (!double.TryParse(AmountTextBox.Text, out amountVal)) {
                MessageBox.Show("The amount could not be parsed.");
                return false;
            }

            return true;
        }

        #region UI events
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Commit any uncommitted changes. Changes in a bound text box are
            // normally committed to the data source only when the text box
            // loses focus. However, application bar buttons do not receive or
            // change focus because they are not Silverlight controls.
            CommitItemWithFocus();

            if (!CheckParams())
                return;

            // Save the account, category, and/or subcategory if necessary
            if (AccountTextBox.Visibility == Visibility.Visible)
                if (!Datastore.Accounts.Contains(AccountTextBox.Text))
                    Datastore.Accounts.Add(AccountTextBox.Text);

            if (CategoryTextBox.Visibility == Visibility.Visible)
                if (!Datastore.MasterCategories().Contains(CategoryTextBox.Text))
                    Datastore.AddCategory(CategoryTextBox.Text);

            if (SubCategoryTextBox.Visibility == Visibility.Visible) {
                String cat = CategoryTextBox.Text;
                if (CategoryListPicker.Visibility == Visibility.Visible)
                    cat = (String)CategoryListPicker.SelectedItem;
                if (!Datastore.SubCategories(cat).Contains(SubCategoryTextBox.Text))
                    Datastore.AddSubcategory(cat, SubCategoryTextBox.Text);
            }

            SaveResult result = new SaveResult();
            if (transactionToEdit != null) {
                result = Datastore.DeleteTransaction(transactionToEdit, delegate { MessageBox.Show(Constants.MSG_DELETE); });
                if (!result.SaveSuccessful)
                    goto Failure;
            }

            result = Datastore.AddTransaction(currentTransaction, delegate { MessageBox.Show(Constants.MSG_NO_SPACE); });
            if (result.SaveSuccessful) {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State[Constants.SAVED_KEY_TRANSACTIONS] = true;
                NavigationService.GoBack();
            }

        Failure:
            if (!result.SaveSuccessful) {
                string errorMessages = String.Join(Environment.NewLine + Environment.NewLine, result.ErrorMessages.ToArray());
                if (!String.IsNullOrEmpty(errorMessages))
                    MessageBox.Show(errorMessages, "Warning: Invalid Values", MessageBoxButton.OK);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!hasUnsavedChanges) {
                var result = MessageBox.Show("You are about to discard your changes. Continue?", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    NavigationService.GoBack();
            } else
                NavigationService.GoBack();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDown_Typed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.PlatformKeyCode != 0x0A)
                return;
            switch (textboxWithFocus.Name) {
                case "AccountTextBox":
                    PayeeTextBox.Focus();
                    break;
                case "PayeeTextBox":
                    if (CategoryTextBox.Visibility == Visibility.Visible)
                        CategoryTextBox.Focus();
                    else
                        CategoryListPicker.Focus();
                    break;
                case "CategoryTextBox":
                    MemoTextBox.Focus();
                    break;
                case "MemoTextBox":
                    OutflowButton.Focus();
                    break;
                case "AmountTextBox":
                    AmountTextBox.Focus();
                    break;
            }
            CommitItemWithFocus();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Checked_Event(object sender, RoutedEventArgs e)
        {
            AmountTextBox.Focus();
            if (AmountTextBox.Text == "0")
                AmountTextBox.Text = "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTransactionPage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(e.OriginalSource.GetType(), PayeeTextBox.GetType()))
                textboxWithFocus = e.OriginalSource as TextBox;
            else if (Object.ReferenceEquals(e.OriginalSource.GetType(), OutflowButton.GetType()))
                buttonWithFocus = e.OriginalSource as RadioButton;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0) {
                if (AccountListPicker.SelectedItem != null) {
                    Category item = (Category)CategoryListPicker.SelectedItem;
                    if (item.MasterCategory == "New...") {
                        CategoryListPicker.Visibility = Visibility.Collapsed;
                        CategoryTextBox.Visibility = Visibility.Visible;
                        SubCategoryTextBlock.Visibility = Visibility.Visible;
                        SubCategoryTextBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AccountListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0) {
                if (AccountListPicker.SelectedItem != null) {
                    String item = (String)AccountListPicker.SelectedItem;
                    if (item == "New...") {
                        AccountListPicker.Visibility = Visibility.Collapsed;
                        AccountTextBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        #endregion UI events

        #region Ads
        /// <summary>
        /// 
        /// </summary>
        public void UpdateAd()
        {
            Ad.Visibility = Utils.ShowAds ? Visibility.Visible : Visibility.Collapsed;
            Ad.IsAutoRefreshEnabled = false;
        }

        private void Ad_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            String msg = e.Error.Message;
            if (msg.Equals("No ad available.")) {
                Ad.Visibility = Visibility.Collapsed;
                Ad.IsAutoRefreshEnabled = false;
                GoogleAd.Visibility = Visibility.Visible;
                AdRequest adRequest = new AdRequest();
#if DEBUG
                adRequest.ForceTesting = true;
#endif
                GoogleAd.LoadAd(adRequest);
            }
        }

        private void AdView_FailedToReceiveAd(object sender, GoogleAds.AdErrorEventArgs e)
        {
            adDuplexAd.Visibility = Visibility.Visible;
#if DEBUG
            adDuplexAd.IsTest = true;
#endif
            GoogleAd.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}