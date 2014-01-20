using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YNABv1.Model;

namespace YNABv1.Helpers
{
    public static class Ynab4Helper
    {
        private static IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        public async static void ImportBudget(String ynab4Path)
        {
            String jsonMetadata = await DropboxHelper.GetMetaData(ynab4Path);
            Metadata meta = new Metadata(jsonMetadata);

            String dataFolderName = "";
            foreach (Metadata m in meta.Contents) {
                if (m.IsDir && m.Name.Contains("data")) {
                    dataFolderName = m.Name;
                    break;
                }
            }
            if (dataFolderName == "") {
                // SOMETHING WENT HORRIBLY WRONG
                return;
            }

            AppSettings[Constants.YNAB_DATA_PATH] = ynab4Path + "/" + dataFolderName;
            AppSettings.Save();

            String budget = GetLatestBudget();

            // parse the budget file for the accounts and categories and payees and their associated IDs
            // check return value like in ImportCsv above
        }

        private static String GetLatestBudget()
        {
            // grab the metadata for <data_folder>/devices
            // check cache to see if anything has changed
            // if nothing has changed, return what's in the cache

            // download the latest (non-conflicted) device file
            // cache the device GUID and the shortDeviceId in a dictionary (if it's not already in there)
            // cache the "knowledge" value (and take note of the different devices
            // download <data_folder>/<deviceGUID>/Budget.yfull and return it
            return "";
        }
    }
}
