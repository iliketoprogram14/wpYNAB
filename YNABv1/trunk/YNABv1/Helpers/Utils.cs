using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YNABv1.Helpers
{
    public class Utils
    {
        public static bool ShowAds { get; set; }
        public static void UpdateInAppPurchases()
        {
            ShowAds = true;
            var allLicenses = Windows.ApplicationModel.Store.CurrentApp.LicenseInformation.ProductLicenses;
            if (allLicenses.ContainsKey("NoAds")) {
                var license = allLicenses["NoAds"];
                if (license.IsActive)
                    ShowAds = false;
            }
        }
    }
}
