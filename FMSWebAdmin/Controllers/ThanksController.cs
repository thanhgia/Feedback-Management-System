using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;

namespace FMSWebAdmin.Controllers
{
    public class ThanksController : Controller
    {
        private IConfiguration _config;
        private IThanksRepository _thanksRepository;
        private IFeedbackRepository _feedbackRepository;

        public ThanksController(IConfiguration connfig, IThanksRepository thanksRepository, IFeedbackRepository feedbackRepository)
        {
            _config = connfig;
            _thanksRepository = thanksRepository;
            _feedbackRepository = feedbackRepository;
        }
        public IActionResult Index()
        { 
            return View();
        }

        public IActionResult ShowThanks()
        {
            //string username = HttpContext.Session.GetString("adminName");
            getFeedbackID();               
            getThanksByBranIdCreate();
            ViewBag.email = HttpContext.Session.GetString("email");
            return View("ManageThanks");
        }

        public async Task<IActionResult> CreateNewThank(Thanks thank)
        {
            string brandId = HttpContext.Session.GetString("brandId");
            string userNameToCreateThank = HttpContext.Session.GetString("adminName");
            DateTime currentDate = DateTime.Now;
            thank.BrandId = brandId;
            thank.Username = userNameToCreateThank;
            thank.CreateDate = currentDate;
            thank.Status = 0;

            _thanksRepository.Add(thank);
            await _thanksRepository.SaveChangesAsync();
            getThanksByBranIdCreate();
            getFeedbackID();
            return View("ManageThanks");
        }

        public IActionResult DeleteThankSentence(Thanks thank)
        {
            int thankStatus = _thanksRepository.GetStatus(thank.ThanksSentenceId);
            if (thankStatus != 2)
            {
                _thanksRepository.RemoveThanksByUpdateStatus(thank.ThanksSentenceId);
                _thanksRepository.SaveChanges();
            }
            else
            {
                ViewBag.ThankIsUsing = "Lời cảm ơn đang được áp dụng, không thể xóa.";
            }
            getThanksByBranIdCreate();
            getFeedbackID();
            return View("ManageThanks");
        }

        public async Task<IActionResult> UpdateThankSentence(Thanks thank)
        {
            string brandId = HttpContext.Session.GetString("brandId");
            string userNameToCreateThank = HttpContext.Session.GetString("adminName");
            DateTime currentDate = DateTime.Now;
            thank.BrandId = brandId;
            thank.Username = userNameToCreateThank;
            thank.CreateDate = currentDate;
            thank.Status = 0;
            _thanksRepository.Update(thank.ThanksSentenceId, thank);
            await _thanksRepository.SaveChangesAsync();
            getThanksByBranIdCreate();
            getFeedbackID();
            return View("ManageThanks");
        }

        public void getThanksByBranIdCreate()
        {
            List<Thanks> listAll = _thanksRepository.GetAll().ToList<Thanks>();
            List<Thanks> listResult = new List<Thanks>();
            foreach (Thanks thank in listAll)
            {
                if (thank.BrandId.Equals(HttpContext.Session.GetString("brandId")) && thank.Status != 1)
                {
                    listResult.Add(thank);
                }
            }
            ViewBag.listThanks = listResult;
            ViewBag.email = HttpContext.Session.GetString("email");


        }

        //Get feedbackID to use create new thanks by dropdownlist for user
        public void getFeedbackID()
        {
            List<Feedback> listFeedback = _feedbackRepository.GetAll().ToList<Feedback>();
            /*List<string> listFeedbackId = new List<string>();*/
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (Feedback fb in listFeedback)
            {
                /*listFeedbackId.Add(fb.FeedbackId.ToString());*/
                SelectListItem item = new SelectListItem(fb.FeedbackId, fb.FeedbackId);
                listItems.Add(item);
            }
           
           
            ViewBag.listResultFeedbackId = listItems;
        }
            
    }
}
