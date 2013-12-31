namespace YNABv1
{
    public static class Constants
    {
        public const string TRANSACTION_SAVED_KEY = "SavedTransactions";
        public const string PAYEE_SAVED_KEY = "SavedPayees";
        public const string CATEGORY_SAVED_KEY = "SavedCategories";
        public const string ACCOUNT_SAVED_KEY = "SavedAccounts";

        public const string DELETE_MSG = "Something went wrong :( Please close the application and try again.";
        public const string NO_SPACE_MSG = "There is not enough space on your phone to save your fill-up data. Free some space and try again.";

        public static string DROPBOX_KEY = DropboxApi.KEY;
        public static string DROPBOX_SECRET = DropboxApi.SECRET;

        public const string DROPBOX_ACCESS_TOKEN = "DropboxAccessToken";
        public const string DROPBOX_UID = "DropboxUID";
    }
}
