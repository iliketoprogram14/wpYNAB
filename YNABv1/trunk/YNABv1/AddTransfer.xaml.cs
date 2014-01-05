﻿using System;
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
            if (State.ContainsKey(CURRENT_TRANSFER_KEY))
                currentTransfer = State[CURRENT_TRANSFER_KEY] as Transaction;
            else if (transferToEdit != null)
                currentTransfer = transferToEdit.DeepCopy();
            else
                currentTransfer = new Transaction(true);
            DataContext = currentTransfer;
            hasUnsavedChanges = State.ContainsKey(HAS_UNSAVED_CHANGES_KEY) && (bool)State[HAS_UNSAVED_CHANGES_KEY];
        }

        #region Navigation Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (PhoneApplicationService.Current.State.ContainsKey("AddTransferParam")) {
                transferToEdit = PhoneApplicationService.Current.State["AddTransferParam"] as Transaction;
                PhoneApplicationService.Current.State.Remove("AddTransferParam");
            } else
                transferToEdit = null;

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

            var result = MessageBox.Show("You are about to discard your " +
                "changes. Continue?", "Warning", MessageBoxButton.OKCancel);
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

            if (string.IsNullOrWhiteSpace(AccountTextBox.Text)) {
                MessageBox.Show("The account is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(PayeeTextBox.Text)) {
                MessageBox.Show("The payee is required.");
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

            // Now that everything has cleared, modify the payee field
            String toAccount = currentTransfer.Payee;
            currentTransfer.Payee = "Transfer : " + toAccount;

            SaveResult result = new SaveResult();
            if (transferToEdit != null) {
                result = Datastore.DeleteTransaction(transferToEdit, delegate { MessageBox.Show(Constants.MSG_DELETE); });
                if (!result.SaveSuccessful)
                    goto Failure;
            }

            result = Datastore.AddTransaction(currentTransfer, delegate { MessageBox.Show(Constants.MSG_NO_SPACE); });
            if (result.SaveSuccessful) {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State[Constants.SAVED_KEY_TRANSACTIONS] = true;
            }

            // Perform the same functions for the inverse of this transfer
            if (transferToEdit != null) {
                // delete the transaction here
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
                string errorMessages = String.Join(
                    Environment.NewLine + Environment.NewLine,
                    result.ErrorMessages.ToArray());
                if (!String.IsNullOrEmpty(errorMessages)) {
                    MessageBox.Show(errorMessages,
                        "Warning: Invalid Values", MessageBoxButton.OK);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("You are about to discard your " +
                "changes. Continue?", "Warning", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
                NavigationService.GoBack();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AddTransferPage_GotFocus(object sender, RoutedEventArgs e)
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
        #endregion
    }
}