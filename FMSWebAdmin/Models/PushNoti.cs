using FMSWebAdmin.Models.Dataset;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models
{
    public class PushNoti : IJob
    {
        private UtilModel util = new UtilModel();
        private UtilConstant uConstant = new UtilConstant();

        public PushNoti() {
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            string token = context.JobDetail.JobDataMap.GetString("tokens");

            string[] tokens = token.Split("//");

           // string[] tokens = { "eWvonZUuTSc:APA91bGB4mMs6wAxOFj4H4rTn0uwPonwsDBW6904QiYuKH0zmhP0-T8519ZktWmjuo73livxkt_ZgVBJwbLLDPVjvi0QOnIcFTAfc8ZluLRmAJXnfDOsP-olvZQUKYS4lrqhhqvM8sy6" };
            var data = new { action = uConstant.FIREBASE_RELOAD_ACTION, click_action = "FLUTTER_NOTIFICATION_CLICK" };
            await util.SendPushNoti(tokens, "SERVER_REQUEST", "TaskDefault" + "", data);

        }
    }
}
