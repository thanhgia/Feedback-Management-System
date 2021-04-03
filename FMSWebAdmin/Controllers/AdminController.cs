using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FMSWebAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;

namespace FMSWebAdmin.Controllers
{
    public class AdminController : Controller
    {
        private IConfiguration _config;
        private IFeedbackRepository _feedbackRepository;
        private IQuestionRepository _questionRepository;
        private IResponseRepository _responseRepository;
        private IEquipmentRepository _equipmentRepository;
        private IEquipmentGroup _equipmentGroup;
        /*private IElasticClient _elasticClient;*/

        public AdminController(IConfiguration config, IFeedbackRepository feedbackRepository, IQuestionRepository questionRepository,
            IResponseRepository responseRepository, IEquipmentRepository equipmentRepository, IEquipmentGroup equipmentGroup)
        {
            _config = config;
            _feedbackRepository = feedbackRepository;
            _questionRepository = questionRepository;
            _responseRepository = responseRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentGroup = equipmentGroup;
            /*  _elasticClient = elasticClient;*/
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Show()
        {
            getFeedbackIDByBrandId();
            getEquipmentGroupId();
            ViewBag.email = HttpContext.Session.GetString("email");
            return View("Admin");
        }

        public void getFeedbackIDByBrandId()
        {
            string t = HttpContext.Session.GetString("brandId");
            List<Feedback> listFeedback = _feedbackRepository.GetAll().ToList<Feedback>();
            /*List<string> listFeedbackId = new List<string>();*/
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (Feedback fb in listFeedback)
            {
                if (fb.BrandId.Equals(HttpContext.Session.GetString("brandId")))
                {
                    /*listFeedbackId.Add(fb.FeedbackId.ToString());*/
                    SelectListItem item = new SelectListItem(fb.FeedbackId, fb.FeedbackId);
                    listItems.Add(item);
                }
            }
            ViewBag.listResultFeedbackId = listItems;
        }
        /// <summary>
        /// getEquipmentGroupId by brandId of User 
        /// </summary>
        public void getEquipmentGroupId()
        {
            List<EquipmentGroup> listequipmentGroups = _equipmentGroup.GetAll().ToList<EquipmentGroup>();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (EquipmentGroup eGroup in listequipmentGroups)
            {
                if (eGroup.BrandId.Equals(HttpContext.Session.GetString("brandId")))
                {
                    if (eGroup.EquipmentGroupId.Equals(HttpContext.Session.GetString("equipmentGroupId")))
                    {
                        SelectListItem itemSelected = new SelectListItem(eGroup.NameOfGroup, eGroup.EquipmentGroupId, true);
                        listItems.Add(itemSelected);
                        /* HttpContext.Session.Remove("equipmentGroupId");*/
                    }
                    else
                    {
                        SelectListItem item = new SelectListItem(eGroup.NameOfGroup, eGroup.EquipmentGroupId);
                        listItems.Add(item);
                    }
                }
            }
            ViewBag.listEquipmentGroupIdByBrandId = listItems;
            //get email to display on view
            ViewBag.email = HttpContext.Session.GetString("email");
        }

        public async Task<IActionResult> GetListFeedbackByEquipmentGroupId(EquipmentGroup eGroup)
        {
            // get list equipment by equipGroupId
            //1. get equipmentId from nameOfGroup
            //List<EquipmentGroup> listequipmentGroups = _equipmentGroup.GetAll().ToList<EquipmentGroup>();
            List<Feedback> listTmp = new List<Feedback>();
            List<Feedback> listFeedback = new List<Feedback>();
            /*string equipGroupId = null;
            foreach (EquipmentGroup group in listequipmentGroups)
            {
                if (group.NameOfGroup.Equals(eGroup.NameOfGroup))
                {
                    equipGroupId = group.EquipmentGroupId;
                }
            }*/
            //2. get list equipment
            if (_equipmentRepository.GetEquipmentByGroupId(eGroup.EquipmentGroupId) != null)
            {
                List<Equipment> listEquip = _equipmentRepository.GetEquipmentByGroupId(eGroup.EquipmentGroupId).ToList<Equipment>();

                //get list feedback by list equipment
                foreach (Equipment e in listEquip)
                {
                    listTmp = await _feedbackRepository.GetFeebackByEquipmentId(e.EquipmentId);
                    foreach (Feedback fb in listTmp)
                    {
                        listFeedback.Add(fb);
                    }
                }

                List<SelectListItem> listItems = new List<SelectListItem>();
                foreach (Feedback fb in listFeedback)
                {
                    if (fb.FeedbackId.Equals(HttpContext.Session.GetString("feedbackId")))
                    {
                        SelectListItem itemSelected = new SelectListItem(fb.Title, fb.FeedbackId, true);
                        listItems.Add(itemSelected);
                        /*HttpContext.Session.Remove("feedbackId");*/
                    }
                    else
                    {
                        SelectListItem item = new SelectListItem(fb.Title, fb.FeedbackId);
                        listItems.Add(item);
                    }
                }

                ViewBag.listFeedback = listItems;
                ViewBag.EQuipId = eGroup.EquipmentGroupId;
                await HttpContext.Session.LoadAsync();
                HttpContext.Session.SetString("equipmentGroupId", eGroup.EquipmentGroupId);
                await HttpContext.Session.CommitAsync();
                getEquipmentGroupId();
            }
            else
            {
                getEquipmentGroupId();
                ViewBag.checkListEquipmentNotNull = 1;
            }
            return View("Admin");
        }

        public List<string> getResponseDetail(string res)
        {
            List<string> detail = new List<string>();
            do
            {
                int subCount = res.IndexOf("//");
                string tmp = res.Substring(0, subCount);
                res = res.Remove(0, subCount + 2);
                detail.Add(tmp);
            } while (res.Contains("//"));
            return detail;
        }

        
        public async Task<IActionResult> GetResponseByFeedbackId(string feedbackId)
        {
            /*_equipmentGroup.*/
            //get questionId from feedbackId
            List<Question> listQuestion = _questionRepository.GetAll().ToList<Question>();
            List<Question> listQuestionForFb = new List<Question>();
            List<string> listQuestionId = new List<string>();
            List<ResponseModel> listResponse = new List<ResponseModel>();
            List<Response> listTmp = new List<Response>();
            List<ResponseModel> listResForOneQues = new List<ResponseModel>();
            List<List<ResponseModel>> listResult = new List<List<ResponseModel>>();
            foreach (Question que in listQuestion)
            {
                if (que.FeedbackId.Equals(feedbackId))
                {
                    listQuestionId.Add(que.QuestionId);
                    listQuestionForFb.Add(que);
                }
            }
            //get all response from QuestionID
            for (int questionIndex = 0; questionIndex < listQuestionId.Count; questionIndex++)
            {
                listTmp = _responseRepository.GetResponseByQuestionId(listQuestionId[questionIndex]).ToList();
                foreach (Response r in listTmp)
                {
                    String tmpResponseDetail = r.ResponseDetail;
                    List<string> listResponseDetail = new List<string>();
                    //TypeOfQuestion 1: Emotion
                    if (r.Question.TypeOfQuestion == 1)
                    {
                        switch (tmpResponseDetail)
                        {
                            case "1":
                                tmpResponseDetail = "Thất vọng";
                                break;
                            case "2":
                                tmpResponseDetail = "Tạm ổn";
                                break;
                            case "3":
                                tmpResponseDetail = "Tốt";
                                break;
                            case "4":
                                tmpResponseDetail = "Tuyệt vời";
                                break;
                            case "5":
                                tmpResponseDetail = "Yêu thích";
                                break;
                        }
                    }
                    //TypeOfQuestion 2: Rating star
                    else if (r.Question.TypeOfQuestion == 2)
                    {
                        tmpResponseDetail = r.ResponseDetail.Substring(0, 1) + " sao";

                    }
                    else if(r.Question.TypeOfQuestion == 3)
                    {
                       listResponseDetail = getResponseDetail(r.ResponseDetail);
                       foreach(string detail in listResponseDetail)
                        {
                            ResponseModel resModelForMultichoice = new ResponseModel
                            {
                                equipmentId = r.EquipmentId,
                                questionId = questionIndex + "",
                                responseDetail = detail,
                                typeOfQuestion = r.Question.TypeOfQuestion + "",
                                responseTime = r.ResponseTime

                            };
                            listResponse.Add(resModelForMultichoice);
                        }
                    }
                    //TypeOfQuestion 4: one choice
                    else if (r.Question.TypeOfQuestion == 4)
                    {
                        tmpResponseDetail = r.ResponseDetail;
                    }

                    if (r.Question.TypeOfQuestion != 3)
                    {
                        ResponseModel resModel = new ResponseModel
                        {
                            equipmentId = r.EquipmentId,
                            questionId = questionIndex + "",
                            responseDetail = tmpResponseDetail,
                            typeOfQuestion = r.Question.TypeOfQuestion + "",
                            responseTime = r.ResponseTime

                        };
                        listResponse.Add(resModel);
                        if (questionIndex == 0)
                        {
                            listResForOneQues.Add(resModel);
                        }
                    }
                }
                listResult.Add(listResponse);

            }
            
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"))
                .RequestTimeout(TimeSpan.FromMinutes(2));
            var lowlevelClient = new ElasticLowLevelClient(settings);
            //push data on elastic for count response of feedback
            int i = 0;
            int checkIdOfIndexExist = 0;
            if (HttpContext.Session.GetInt32("TotalIdIndexOfCountRes") != null)
            {
                checkIdOfIndexExist = (int)HttpContext.Session.GetInt32("TotalIdIndexOfCountRes");
                if (checkIdOfIndexExist != 0)
                {
                    //Clear all data in index 
                    for (int j = 1; j <= checkIdOfIndexExist; j++)
                    {
                        lowlevelClient.Delete<StringResponse>("countresponseforfb", Convert.ToString(j));
                    }
                    HttpContext.Session.Remove("TotalIdIndexOfCountRes");
                }
            }
            foreach (ResponseModel res in listResForOneQues)
            {
                lowlevelClient.Index<StringResponse>("countresponseforfb", Convert.ToString(++i), PostData.Serializable(res));
            }
            HttpContext.Session.SetInt32("TotalIdIndexOfCountRes", i);


            //Id cho tung object trong index
            #region push response on elastic 
            i = 0;
            checkIdOfIndexExist = 0;
            if (HttpContext.Session.GetInt32("TotalIdIndex") != null)
            {
                checkIdOfIndexExist = (int)HttpContext.Session.GetInt32("TotalIdIndex");
                if (checkIdOfIndexExist != 0)
                {
                    //Clear all data in index 
                    for (int j = 1; j <= checkIdOfIndexExist; j++)
                    {
                        lowlevelClient.Delete<StringResponse>("testresponse", Convert.ToString(j));
                    }
                    HttpContext.Session.Remove("TotalIdIndex");
                }
            }

            foreach (ResponseModel res in listResponse)
            {
                lowlevelClient.Index<StringResponse>("testresponse", Convert.ToString(++i), PostData.Serializable(res));
            }

            HttpContext.Session.SetInt32("TotalIdIndex", i);
            #endregion

            ViewBag.countResponse = listResForOneQues.Count;
            ViewBag.listQuestion = listQuestionForFb;
            ViewBag.feedbackId = feedbackId;
            getFeedbackIDByBrandId();
            getEquipmentGroupId();

            await HttpContext.Session.LoadAsync();
            HttpContext.Session.SetString("feedbackId", feedbackId);
            await HttpContext.Session.CommitAsync();
            EquipmentGroup e = new EquipmentGroup { EquipmentGroupId = HttpContext.Session.GetString("equipmentGroupId") };
            await GetListFeedbackByEquipmentGroupId(e);
            return View("Admin");
        }



    }
}
