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
        private Transaction transactionToEdit;
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
                accounts.Add("New...");
                AccountListPicker.ItemsSource = accounts;
                AccountListPicker.Visibility = Visibility.Visible;
                AccountTextBox.Visibility = Visibility.Collapsed;
            }

            List<String> masterCats = Datastore.MasterCategories();
            if (Datastore.MasterCategories().Count == 0) {
                CategoryTextBox.Visibility = Visibility.Visible;
                SubCategoryTextBox.Visibility = Visibility.Visible;
                CategoryListPicker.Visibility = Visibility.Collapsed;
                SubCategoryListPicker.Visibility = Visibility.Collapsed;
            } else {
                List<String> masterCategories = new List<String>(Datastore.MasterCategories());
                masterCategories.Add("New...");
                CategoryListPicker.ItemsSource = masterCategories;
                CategoryListPicker.Visibility = Visibility.Visible;
                SubCategoryListPicker.Visibility = Visibility.Visible;
                CategoryTextBox.Visibility = Visibility.Collapsed;
                SubCategoryTextBox.Visibility = Visibility.Collapsed;
            }
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
            } else
                transactionToEdit = null;

            // Initialize the page state only if it is not already initialized,
            // and not when the application was deactivated but not tombstoned
            // (returning from being dormant).
            if (DataContext == null)
                InitializePageState();

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
                if (((String)CategoryListPicker.SelectedItem).Equals("")) {
                    MessageBox.Show("The category is required.");
                    return false;
                }
                currentTransaction.Category = (String)CategoryListPicker.SelectedItem;
                currentTransaction.Subcategory = (String)SubCategoryListPicker.SelectedItem;
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
            var result = MessageBox.Show("You are about to discard your changes. Continue?", "Warning", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
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
                    CategoryTextBox.Focus();
                    break;

                case "CategoryTextBox":
                    SubCategoryTextBox.Focus();
                    break;

                case "SubCategoryTextBox":
                    MemoTextBox.Focus();
                    break;

                case "MemoTextBox":
                    OutflowButton.Focus();
                    ScrollViewerGrid.ScrollToVerticalOffset(ScrollViewerGrid.Height - 75);
                    break;

                case "AmountTextBox":
                    AmountTextBox.Focus();
                    break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Checked_Event(object sender, RoutedEventArgs e)
        {
            AmountTextBox.Focus();
            ScrollViewerGrid.ScrollToVerticalOffset(ScrollViewerGrid.Height - 40);
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
            String item = (String)CategoryListPicker.SelectedItem;
            if (item == "New...") {
                CategoryListPicker.Visibility = Visibility.Collapsed;
                CategoryTextBox.Visibility = Visibility.Visible;
                SubCategoryListPicker.Visibility = Visibility.Collapsed;
                SubCategoryTextBox.Visibility = Visibility.Visible;
            } else if (item != "") {
                List<String> subcats = new List<String>(Datastore.SubCategories(item));
                subcats.Add("New...");
                SubCategoryListPicker.ItemsSource = subcats;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AccountListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String item = (String)AccountListPicker.SelectedItem;
            if (item == "New...") {
                AccountListPicker.Visibility = Visibility.Collapsed;
                AccountTextBox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubCategoryListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String item = (String)SubCategoryListPicker.SelectedItem;
            if (item == "New...") {
                SubCategoryListPicker.Visibility = Visibility.Collapsed;
                SubCategoryTextBox.Visibility = Visibility.Visible;
            }
        }
        #endregion UI events
    }
}