using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using YNABv1.Model;
using YNABv1.Helpers;
using GoogleAds;

namespace YNABv1
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AddTransfer : PhoneApplicationPage
    {
        private const string CURRENT_TRANSFER_KEY = "CurrentTransfer";
        private const string HAS_UNSAVED_CHANGES_KEY = "HasUnsavedChanges";
        private Transaction currentTransfer;
        private bool hasUnsavedChanges;
        private TextBox textboxWithFocus;
        private RadioButton buttonWithFocus;
        private Transaction transferToEdit;
        private IDictionary<string, object> phoneState = PhoneApplicationService.Current.State;

        /// <summary>
        /// 
        /// </summary>
        public AddTransfer()
        {
            InitializeComponent();
            GotFocus += AddTransferPage_GotFocus;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializePageState() 
        {
            Ad.Refresh();
            
            if (State.ContainsKey(CURRENT_TRANSFER_KEY))
                currentTransfer = State[CURRENT_TRANSFER_KEY] as Transaction;
            else if (transferToEdit != null)
                currentTransfer = transferToEdit.DeepCopy();
            else
                currentTransfer = new Transaction(true);
            DataContext = currentTransfer;
            hasUnsavedChanges = State.ContainsKey(HAS_UNSAVED_CHANGES_KEY) && (bool)State[HAS_UNSAVED_CHANGES_KEY];

            if (Datastore.Accounts.Count == 0) {
                AccountTextBox.Visibility = Visibility.Visible;
                PayeeTextBox.Visibility = Visibility.Visible;
                AccountListPicker.Visibility = Visibility.Collapsed;
                PayeeListPicker.Visibility = Visibility.Collapsed;
            } else {
                List<String> accts = new List<String>(Datastore.Accounts);
                accts.Sort();
                accts.Add("New...");
                AccountListPicker.ItemsSource = accts;
                PayeeListPicker.ItemsSource = accts;
                if (Datastore.Accounts.Count > 1)
                    PayeeListPicker.SelectedItem = Datastore.Accounts.ElementAt(1);
                AccountListPicker.Visibility = Visibility.Visible;
                PayeeListPicker.Visibility = Visibility.Visible;
                AccountTextBox.Visibility = Visibility.Collapsed;
                PayeeTextBox.Visibility = Visibility.Collapsed;

                if (transferToEdit != null) {
                    AccountListPicker.SelectedItem = currentTransfer.Account;
                    PayeeListPicker.SelectedItem = currentTransfer.Payee.Replace("Transfer : ", "");
                }
            }
        }

        #region Navigation Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateAd();

            if (PhoneApplicationService.Current.State.ContainsKey("AddTransferParam")) {
                transferToEdit = PhoneApplicationService.Current.State["AddTransferParam"] as Transaction;
                PhoneApplicationService.Current.State.Remove("AddTransferParam");
            } 

            if (DataContext == null)
                InitializePageState();

            State.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Do not cache the page state when navigating backward 
            // or when there are no unsaved changes.
            if (e.Uri.OriginalString.Equals("//MainPage.xaml") || !hasUnsavedChanges)
                return;

            CommitItemWithFocus();
            State[CURRENT_TRANSFER_KEY] = currentTransfer;
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
        #endregion

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

            if (buttonWithFocus != null) {
                expression = buttonWithFocus.GetBindingExpression(RadioButton.IsCheckedProperty);
                if (expression != null)
                    expression.UpdateSource();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
                currentTransfer.Account = (String)AccountListPicker.SelectedItem;
            }

            if (PayeeTextBox.Visibility == Visibility.Visible) {
                if (string.IsNullOrWhiteSpace(PayeeTextBox.Text)) {
                    MessageBox.Show("The payee is required.");
                    return false;
                }
            } else {
                if (((String)PayeeListPicker.SelectedItem).Equals("")) {
                    MessageBox.Show("The payee is required.");
                    return false;
                }
                currentTransfer.Payee = (String)PayeeListPicker.SelectedItem;
            }

            bool outflow = (bool)OutflowButton.IsChecked;
            bool inflow = (bool)InflowButton.IsChecked;
            if ((!outflow && !inflow) || (outflow && inflow)) {
                MessageBox.Show("The transaction must be an outflow or an inflow.");
                return false;
            }

            double amountVal;
            if (!double.TryParse(AmountTextBox.Text, out amountVal)) {
                MessageBox.Show("The amount could not be parsed.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateAd()
        {
            Ad.Visibility = Utils.ShowAds ? Visibility.Visible : Visibility.Collapsed;
            Ad.IsAutoRefreshEnabled = false;
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

            // Save the account(s) if necessary
            if (AccountTextBox.Visibility == Visibility.Visible && (!Datastore.Accounts.Contains(AccountTextBox.Text)))
                Datastore.Accounts.Add(AccountTextBox.Text);

            if (PayeeTextBox.Visibility == Visibility.Visible && (!Datastore.Accounts.Contains(PayeeTextBox.Text)))
                Datastore.Accounts.Add(PayeeTextBox.Text);

            // Now that everything has cleared, it's safe to modify the payee field
            currentTransfer.Payee = "Transfer : " + currentTransfer.Payee;

            SaveResult result = new SaveResult();
            if (transferToEdit != null) {
                result = Datastore.DeleteTransaction(transferToEdit, delegate { MessageBox.Show(Constants.MSG_DELETE); });
                if (!result.SaveSuccessful)
                    goto Failure;
            }

            result = Datastore.AddTransaction(currentTransfer, delegate { MessageBox.Show(Constants.MSG_NO_SPACE); });
            if (result.SaveSuccessful)
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State[Constants.SAVED_KEY_TRANSACTIONS] = true;

            // Perform the same functions for the inverse of this transfer
            if (transferToEdit != null) {
                Transaction inverseTransferToEdit = transferToEdit.InverseTransfer();
                result = Datastore.DeleteTransaction(inverseTransferToEdit, delegate { MessageBox.Show(Constants.MSG_DELETE); });
                if (!result.SaveSuccessful)
                    goto Failure;
            }

            Transaction inverseTransfer = currentTransfer.InverseTransfer();
            result = Datastore.AddTransaction(inverseTransfer, delegate { MessageBox.Show(Constants.MSG_NO_SPACE); });
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
        private void AddTransferPage_GotFocus(object sender, RoutedEventArgs e)
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
        private void KeyDown_Typed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.PlatformKeyCode != 0x0A)
                return;
            switch (textboxWithFocus.Name) {
                case "AccountTextBox":
                    PayeeTextBox.Focus();
                    break;
                case "PayeeTextBox":
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
        private void PayeeListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String item = (String)PayeeListPicker.SelectedItem;
            if (item == "New...") {
                PayeeListPicker.Visibility = Visibility.Collapsed;
                PayeeTextBox.Visibility = Visibility.Visible;
            }
        }
        #endregion

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
    }
}