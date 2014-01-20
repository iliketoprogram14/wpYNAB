namespace YNABv1
{
    /// <summary>
    /// 
    /// </summary>
    public static class Constants
    {
        // For current phone state
        public const string SAVED_KEY_TRANSACTIONS = "SavedTransactions";
        public const string SAVED_KEY_PAYEES = "SavedPayees";
        public const string SAVED_KEY_CATEGORIES = "SavedCategories";
        public const string SAVED_KEY_ACCOUNTS = "SavedAccounts";

        public const string MSG_DELETE = "Something went wrong :( Please close the application and try again.";
        public const string MSG_NO_SPACE = "There is not enough space on your phone to save your fill-up data. Free some space and try again.";

        public const string TUTORIAL_KEY = "TutorialHasBeenShown";

        public const string DROPBOX_KEY = DropboxApi.KEY;
        public const string DROPBOX_SECRET = DropboxApi.SECRET;
        public const string DROPBOX_ACCESS_TOKEN = "DropboxAccessToken";
        public const string DROPBOX_UID = "DropboxUID";

        public const string SYNC_KEY = "YnabCompanion.SyncKey";
        public const string YNAB_DATA_PATH = "YnabCompanion.DataPathKey";

        // For passing params while navigating
        public const string NAV_PARAM_TRANSACTION = "AddTransactionParam";
        public const string NAV_PARAM_TRANSFER = "AddTransferParam";
    }
}
