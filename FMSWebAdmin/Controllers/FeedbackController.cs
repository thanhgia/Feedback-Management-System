using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSWebAdmin.Models;
using FMSWebAdmin.Models.TaskScheduler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;
using Quartz;

namespace FMSWebAdmin.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackRepository feedback;
        private readonly IThanksRepository thanksRepository;
        private readonly IGiftRepository giftRepository;
        private readonly IImageRepository imageRepository;
        private readonly IEquipmentRepository equipment;
        private readonly IEquipmentGroup equipmentGroup;
        private readonly IQuestionRepository question;
        private UtilModel util = new UtilModel();
        private UtilConstant uConstant = new UtilConstant();
        private readonly IScheduler _scheduler;

        public FeedbackController(IFeedbackRepository feedback, IThanksRepository thanksRepository, IGiftRepository giftRepository, IImageRepository imageRepository, IEquipmentRepository equipment, IEquipmentGroup equipmentGroup, IQuestionRepository questionRepository, IScheduler factory)
        {
            this.feedback = feedback;
            this.thanksRepository = thanksRepository;
            this.giftRepository = giftRepository;
            this.imageRepository = imageRepository;
            this.equipmentGroup = equipmentGroup;
            this.equipment = equipment;
            this.question = questionRepository;
            _scheduler = factory;
        }

        private string CreateListJson<T>(IEnumerable<T> list)
        {
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var obj = JsonConvert.SerializeObject(list, Formatting.Indented, serializerSettings);
            return obj;
        }

        public async Task<IActionResult> ShowAll()
        {
            List<Feedback> listFeedback;
            List<Thanks> thanks;
            List<Gift> gifts;
            List<Image> images;

            await HttpContext.Session.LoadAsync();
            string brandId = HttpContext.Session.GetString("brandId");
            string listJson;
            IEnumerable<Feedback> list = await feedback.GetFeedbackByBrandId(brandId);
            if (list == null)
            {
                listFeedback = new List<Feedback>();
            }
            else
            {
                listFeedback = list.ToList();
            }
            listJson = CreateListJson<Feedback>(list);

            IEnumerable<Thanks> thanksEnum = await thanksRepository.GetThanksByBrandId(brandId);
            if (thanksEnum == null)
            {
                thanks = new List<Thanks>();
            }
            else
            {
                thanks = thanksEnum.ToList();
            }

            IEnumerable<Gift> giftsEnum = await giftRepository.GetGiftByBrandId(brandId);
            if (giftsEnum == null)
            {
                gifts = new List<Gift>();
            }
            else
            {
                gifts = giftsEnum.ToList();
            }

            IEnumerable<Image> imagesEnum = await imageRepository.GetImageByBrandId(brandId);
            if (imagesEnum == null)
            {
                images = new List<Image>();
            }
            else
            {
                images = imagesEnum.ToList();
            }
            await HttpContext.Session.CommitAsync();
            
            IEnumerable<EquipmentGroup> equipGroup = equipmentGroup.GetEquipmentGroup(brandId);
            List<EquipmentGroup> listGroup = new List<EquipmentGroup>();
            List<Equipment> listEquip = new List<Equipment>();
            if (equipGroup != null)
            {
                listGroup = equipGroup.ToList();
                foreach (EquipmentGroup group in listGroup)
                {
                    IQueryable<Equipment> tmp = equipment.GetEquipmentByGroupId(group.EquipmentGroupId);
                    if (tmp != null)
                    {
                        listEquip.AddRange(tmp.ToList());
                    }
                }
            }
            
            ViewBag.listFeedback = listFeedback;
            ViewBag.listEquip = listEquip;
            ViewBag.listGroup = listGroup;
            ViewBag.listThanks = thanks;
            ViewBag.listGift = gifts;
            ViewBag.listImage = JsonConvert.SerializeObject(images);
            ViewBag.listJson = listJson;

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        public IActionResult CreateDetail(Feedback feedback)
        {
            Thanks thanks = thanksRepository.Get(feedback.ThanksSentenceId);
            Image image = imageRepository.Get(feedback.ImageId);
            string linkImageBrand = "";
            if (image != null)
            {
                linkImageBrand = image.ImageLink;
            }
            ViewBag.feedback = JsonConvert.SerializeObject(feedback);
            ViewBag.thanks = JsonConvert.SerializeObject(thanks.ContentThanks);
            ViewBag.ImageBrand = linkImageBrand;

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        public async Task<IActionResult> CreateNew (SurveyModel survey)
        {
            Feedback feedbackCheck;
            Feedback fb = JsonConvert.DeserializeObject<Feedback>(survey.Feedback);
            do
            {
                fb.FeedbackId = util.RandomString(8);
                feedbackCheck = null;
                feedbackCheck = feedback.Get(fb.FeedbackId);
            } while (feedbackCheck != null);
            feedback.Add(fb);
            await feedback.SaveChangesAsync();

            List<Question> listQuestion = JsonConvert.DeserializeObject<List<Question>>(survey.ListQuestion);
            foreach (Question ques in listQuestion)
            {
                ques.FeedbackId = fb.FeedbackId;
                ques.Username = fb.Username;
                ques.Status = 1;
                Question questionCheck;
                do
                {
                    ques.QuestionId = util.RandomString(7);
                    questionCheck = null;
                    questionCheck = question.Get(ques.QuestionId);
                } while (questionCheck != null);
                question.Add(ques);
                await question.SaveChangesAsync();
            }

            var equipId = fb.EquipmentIds.Split(",");
            string tokens = "";
            for (int i = 0; i < equipId.Length; i++)
            {
                if (equipId[i].Trim() != "")
                {
                    string token = await equipment.GetFCMTokenById(equipId[i].Trim());
                    tokens += token + "//";
                }
                
            }
            await _scheduler.Start();
            ITrigger trigger;

            var startDate = DateTime.Parse(fb.StartTime + "").ToUniversalTime();
            var endDate = DateTime.Parse(fb.EndTime + "").ToUniversalTime();
            if (startDate <= DateTime.UtcNow && DateTime.UtcNow < endDate)
            {
                trigger = TriggerBuilder.Create()
             .WithIdentity($"TriggerBegin-{DateTime.Now}")
             .StartNow()
             .WithPriority(1)
             .Build();
            }   
            else
            {
                trigger = TriggerBuilder.Create()
             .WithIdentity($"TriggerBegin-{DateTime.Now}")
             .StartAt(startDate)
             .WithPriority(1)
             .Build();
            }

            IJobDetail job = JobBuilder.Create<TaskBegin>()
                        .WithIdentity($"Job:{DateTime.Now}")
                        .Build();
            job.JobDataMap.Put("tokens", tokens); 
            job.JobDataMap.Put("thanks", fb.ThanksSentenceId);
            job.JobDataMap.Put("gift", fb.GiftId);
            await _scheduler.ScheduleJob(job, trigger);

            ITrigger triggerEnd = TriggerBuilder.Create()
             .WithIdentity($"TriggerEnd-{DateTime.Now}")
             .StartAt(endDate)
             .WithPriority(1)
             .Build();

            IJobDetail jobEnd = JobBuilder.Create<TaskEnd>()
                        .WithIdentity($"JobEnd:{DateTime.Now}")
                        .Build();
            jobEnd.JobDataMap.Put("tokens", tokens);
            jobEnd.JobDataMap.Put("thanks", fb.ThanksSentenceId);
            jobEnd.JobDataMap.Put("gift", fb.GiftId);
            jobEnd.JobDataMap.Put("feedback", fb.FeedbackId);

            await _scheduler.ScheduleJob(jobEnd, triggerEnd);

            return RedirectToAction("ShowAll");
        }

        public IActionResult ViewDetail(string feedbackId)
        {
            Feedback fb = feedback.Get(feedbackId);
            Thanks thanks = thanksRepository.Get(fb.ThanksSentenceId);
            Image image = imageRepository.Get(fb.ImageId);
            string linkImageBrand = "";
            if (image != null)
            {
                linkImageBrand = image.ImageLink;
            }
            List<Question> listQuestion = question.GetQuestionByFeedbackId(feedbackId);
            string questionJson = JsonConvert.SerializeObject(listQuestion);
           
            ViewBag.feedback = JsonConvert.SerializeObject(fb);
            ViewBag.listQuestion = questionJson;
            ViewBag.thanks = JsonConvert.SerializeObject(thanks.ContentThanks);
            ViewBag.ImageBrand = linkImageBrand;

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        public async Task<IActionResult> UpdateDetail (SurveyModel survey, string listQuestionInsert, string listQuestionDelete)
        {
            Feedback fb = JsonConvert.DeserializeObject<Feedback>(survey.Feedback);
            List<Question> listInsert = JsonConvert.DeserializeObject<List<Question>>(listQuestionInsert);
            List<Question> listDelete = JsonConvert.DeserializeObject<List<Question>>(listQuestionDelete);

            foreach (Question ques in listInsert)
            {
                ques.FeedbackId = fb.FeedbackId;
                ques.Username = fb.Username;
                ques.Status = 0;
                Question questionCheck;
                do
                {
                    ques.QuestionId = util.RandomString(7);
                    questionCheck = null;
                    questionCheck = question.Get(ques.QuestionId);
                } while (questionCheck != null);
                question.Add(ques);
                await question.SaveChangesAsync();
            }

            foreach (Question ques in listDelete)
            {
                ques.Status = 1;
                question.UpdateQuestionStatus(ques.QuestionId);
                await question.SaveChangesAsync();
            }

            var equipId = fb.EquipmentIds.Split(",");
            string tokens = "";
            for (int i = 0; i < equipId.Length; i++)
            {
                if (equipId[i].Trim() != "")
                {
                    string token = await equipment.GetFCMTokenById(equipId[i].Trim());
                    tokens += token + "//";
                }

            }

            await _scheduler.Start();
            ITrigger trigger;

            var startDate = DateTime.Parse(fb.StartTime + "").ToUniversalTime();
            var endDate = DateTime.Parse(fb.EndTime + "").ToUniversalTime();
            if (startDate <= DateTime.UtcNow && DateTime.UtcNow < endDate)
            {
                trigger = TriggerBuilder.Create()
                 .WithIdentity($"Trigger Update-{DateTime.Now}")
                 .StartNow()
                 .WithPriority(1)
                 .Build();

                IJobDetail job = JobBuilder.Create<PushNoti>()
                        .WithIdentity($"Job Update:{DateTime.Now}")
                        .Build();
                job.JobDataMap.Put("tokens", tokens);

                await _scheduler.ScheduleJob(job, trigger);
            }

            return RedirectToAction("ShowAll");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(Feedback delFeedback)
        {
            Feedback fb = feedback.Get(delFeedback.FeedbackId);
            feedback.UpdateStatusOfFeedback(delFeedback.FeedbackId);
            if (await feedback.SaveChangesAsync() >= 0)
            {
                var equipId = fb.EquipmentIds.Split(",");
                string tokens = "";
                for (int i = 0; i < equipId.Length; i++)
                {
                    if (equipId[i].Trim() != "")
                    {
                        string token = await equipment.GetFCMTokenById(equipId[i].Trim());
                        tokens += token + "//";
                    }

                }
                await _scheduler.Start();
                ITrigger trigger = TriggerBuilder.Create()
                 .WithIdentity($"Trigger Delete-{DateTime.Now}")
                 .StartNow()
                 .WithPriority(1)
                 .Build();

                IJobDetail job = JobBuilder.Create<TaskEnd>()
                        .WithIdentity($"Job Delete:{DateTime.Now}")
                        .Build();
                job.JobDataMap.Put("tokens", tokens);
                job.JobDataMap.Put("thanks", fb.ThanksSentenceId);
                job.JobDataMap.Put("gift", fb.GiftId);
                job.JobDataMap.Put("feedback", fb.FeedbackId);

                await _scheduler.ScheduleJob(job, trigger);

                return RedirectToAction("ShowAll");
            }
            return View();
        }
    }
}
