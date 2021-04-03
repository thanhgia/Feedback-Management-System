using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models
{
    public class UtilConstant
    {
        public readonly Uri FIREBASE_URL = new Uri("https://fcm.googleapis.com/fcm/send");
        public readonly String FIREBASE_KEY = "AAAAYo5zNbo:APA91bF4N4jtX-OdtJPnjkq8T05z50LS62SBZAdTLFABHyBlC13H723ZFydjuOpDFmdd9K_S-ooyOkQ4XwKcMkhaFhcEl_Fs61ytn255038QgNLQwGveRHdIeOFhqPpGZnuDpZrX0xd2";
        public readonly String FIREBASE_LOGOUT_ACTION = "ACTION_SIGNOUT";
        public readonly String FIREBASE_RELOAD_ACTION = "ACTION_RELOAD";

    }
}
