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

namespace YNABv1
{
    public partial class AddTransaction : PhoneApplicationPage
    {
        private const string CURRENT_TRANS_KEY = "CurrentTransaction";
        private const string HAS_UNSAVED_CHANGES_KEY = "HasUnsavedChanges";
        private Transaction currentTransaction;
        private bool hasUnsavedChanges;
        private TextBox textboxWithFocus;

        public AddTransaction()
        {
            InitializeComponent();
            GotFocus += AddTransactionPage_GotFocus;
        }

        void AddTransactionPage_GotFocus(object sender, RoutedEventArgs e)
        {
            textboxWithFocus = e.OriginalSource as TextBox;
        }

        private void InitializePageState()
        {
            DataContext = currentTransaction =
                State.ContainsKey(CURRENT_TRANS_KEY) ?
                (Transaction)State[CURRENT_TRANS_KEY] :
                new Transaction { Date = DateTime.Now };
            hasUnsavedChanges = State.ContainsKey(HAS_UNSAVED_CHANGES_KEY) && (bool)State[HAS_UNSAVED_CHANGES_KEY];
        }

        /// <summary>
        /// Called when navigating to this page; loads the car data from storage 
        /// and then initializes the page state.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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

            CommitTextBoxWithFocus();
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
            CommitTextBoxWithFocus();

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
            if (string.IsNullOrWhiteSpace(SubCategoryTextBox.Text)) {
                MessageBox.Show("The subcategory is required.");
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

            SaveResult result = Datastore.AddTransaction(currentTransaction,
                delegate {
                    MessageBox.Show("There is not enough space on your phone to " +
                        "save your fill-up data. Free some space and try again.");
                });
            if (result.SaveSuccessful) {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State[Constants.TRANSACTION_SAVED_KEY] = true;
                NavigationService.GoBack();
            } else {
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
        private void CommitTextBoxWithFocus()
        {
            if (textboxWithFocus == null) return;

            BindingExpression expression = textboxWithFocus.GetBindingExpression(TextBox.TextProperty);
            if (expression != null) 
                expression.UpdateSource();
        }
    }
}