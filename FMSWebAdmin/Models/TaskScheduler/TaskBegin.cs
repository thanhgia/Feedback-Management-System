using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Quartz;
using System;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models.TaskScheduler
{
    public class TaskBegin : IJob
    {
        private UtilModel util = new UtilModel();
        private UtilConstant uConstant = new UtilConstant();
        private readonly IThanksRepository thanks = new ThanksRepository(new FeedbackSystemDBContext());
        private readonly IGiftRepository gift = new GiftRepository(new FeedbackSystemDBContext());

        public TaskBegin()
        {
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string token = context.JobDetail.JobDataMap.GetString("tokens");
            string thanksId = context.JobDetail.JobDataMap.GetString("thanks");
            string giftId = context.JobDetail.JobDataMap.GetString("gift");

            string[] tokens = token.Split("//");

            // string[] tokens = { "eWvonZUuTSc:APA91bGB4mMs6wAxOFj4H4rTn0uwPonwsDBW6904QiYuKH0zmhP0-T8519ZktWmjuo73livxkt_ZgVBJwbLLDPVjvi0QOnIcFTAfc8ZluLRmAJXnfDOsP-olvZQUKYS4lrqhhqvM8sy6" };
            var data = new { action = uConstant.FIREBASE_RELOAD_ACTION, click_action = "FLUTTER_NOTIFICATION_CLICK" };
            await util.SendPushNoti(tokens, "SERVER_REQUEST", "TaskBegin" + DateTime.Now + "", data);

            thanks.UpdateStatusActiveThanks(thanksId);
            await thanks.SaveChangesAsync();

            if (!string.IsNullOrEmpty(giftId))
            {
                gift.UpdateStatusActiveGift(giftId);
                await gift.SaveChangesAsync();
            }

        }
    }
}
