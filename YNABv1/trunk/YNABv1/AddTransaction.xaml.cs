using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using YNABv1.Model;
using System.Windows.Data;
using System.Windows.Input;

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

        public AddTransaction()
        {
            InitializeComponent();
            GotFocus += AddTransactionPage_GotFocus;
        }

        void AddTransactionPage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(e.OriginalSource.GetType(), PayeeTextBox.GetType()))
                textboxWithFocus = e.OriginalSource as TextBox;
            else if (Object.ReferenceEquals(e.OriginalSource.GetType(), OutflowButton.GetType()))
                buttonWithFocus = e.OriginalSource as RadioButton;
        }

        private void InitializePageState()
        {
            if (State.ContainsKey(CURRENT_TRANS_KEY))
                currentTransaction = State[CURRENT_TRANS_KEY] as Transaction;
            else if (transactionToEdit != null)
                currentTransaction = transactionToEdit.DeepCopy();
            else
                currentTransaction = new Transaction { Date = DateTime.Now };
            DataContext = currentTransaction;
            hasUnsavedChanges = State.ContainsKey(HAS_UNSAVED_CHANGES_KEY) && (bool)State[HAS_UNSAVED_CHANGES_KEY];

            List<String> masterCats = Datastore.MasterCategories();
            if (Datastore.MasterCategories().Count == 0) {
                CategoryTextBox.Visibility = Visibility.Visible;
                SubCategoryTextBox.Visibility = Visibility.Visible;
                CategoryListPicker.Visibility = Visibility.Collapsed;
                SubCategoryListPicker.Visibility = Visibility.Collapsed;
            } else {
                CategoryListPicker.ItemsSource = Datastore.MasterCategories();
                CategoryListPicker.Visibility = Visibility.Visible;
                SubCategoryListPicker.Visibility = Visibility.Visible;
                CategoryTextBox.Visibility = Visibility.Collapsed;
                SubCategoryTextBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Called when navigating to this page; loads the car data from storage 
        /// and then initializes the page state.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (PhoneApplicationService.Current.State.ContainsKey("AddTransactionParam")) {
                transactionToEdit = PhoneApplicationService.Current.State["AddTransactionParam"] as Transaction;
                PhoneApplicationService.Current.State.Remove("AddTransactionParam");
            } else 
                transactionToEdit = null;

            // Initialize the page state only if it is not already initialized,
            // and not when the application was deactivated but not tombstoned (returning from being dormant).
            if (DataContext == null) {
                InitializePageState();
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

            var result = MessageBox.Show("You are about to discard your " +
                "changes. Continue?", "Warning", MessageBoxButton.OKCancel);
            e.Cancel = (result == MessageBoxResult.Cancel);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Commit any uncommitted changes. Changes in a bound text box are 
            // normally committed to the data source only when the text box 
            // loses focus. However, application bar buttons do not receive or 
            // change focus because they are not Silverlight controls. 
            CommitItemWithFocus();

            if (string.IsNullOrWhiteSpace(AccountTextBox.Text)) {
                MessageBox.Show("The account is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(PayeeTextBox.Text)) {
                MessageBox.Show("The payee is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(CategoryTextBox.Text)) {
                MessageBox.Show("The category is required.");
                return;
            }
            
            bool outflow = (bool)OutflowButton.IsChecked;
            bool inflow = (bool)InflowButton.IsChecked;
            if ((!outflow && !inflow) || (outflow && inflow)) {
                MessageBox.Show("The transaction must be an outflow or an inflow.");
                return;
            }
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text)) {
                MessageBox.Show("The amount is required.");
                return;
            }
            double amountVal;
            if (!double.TryParse(AmountTextBox.Text, out amountVal)) {
                MessageBox.Show("The amount could not be parsed.");
                return;
            }

            SaveResult result = new SaveResult();
            if (transactionToEdit != null) {
                result = Datastore.DeleteTransaction(transactionToEdit, delegate { MessageBox.Show(Constants.DELETE_MSG); });
                if (!result.SaveSuccessful)
                    goto Failure;
            }

            result = Datastore.AddTransaction(currentTransaction, delegate { MessageBox.Show(Constants.NO_SPACE_MSG); });
            if (result.SaveSuccessful) {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State[Constants.TRANSACTION_SAVED_KEY] = true;
                NavigationService.GoBack();
            }

Failure:
            if (!result.SaveSuccessful) {
                string errorMessages = String.Join(
                    Environment.NewLine + Environment.NewLine,
                    result.ErrorMessages.ToArray());
                if (!String.IsNullOrEmpty(errorMessages)) {
                    MessageBox.Show(errorMessages,
                        "Warning: Invalid Values", MessageBoxButton.OK);
                }
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("You are about to discard your " +
                "changes. Continue?", "Warning", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
                NavigationService.GoBack();
        }

        /// <summary>
        /// Ensures that any changes to text box values are committed. 
        /// </summary>
        private void CommitItemWithFocus()
        {
            if (textboxWithFocus == null)
                return;

            BindingExpression expression = textboxWithFocus.GetBindingExpression(TextBox.TextProperty);
            if (expression != null) 
                expression.UpdateSource();

            if (buttonWithFocus == null)
                return;

            expression = buttonWithFocus.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (expression != null)
                expression.UpdateSource();
        }

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

        private void Checked_Event(object sender, RoutedEventArgs e)
        {
            AmountTextBox.Focus();
            if (AmountTextBox.Text == "0")
                AmountTextBox.Text = "";
        }

        private void Payee_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CategoryListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String item = (String)CategoryListPicker.SelectedItem;
            if (item != "")
                SubCategoryListPicker.ItemsSource = Datastore.SubCategories(item);
        }
    }
}