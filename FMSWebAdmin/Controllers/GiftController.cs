using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;

namespace FMSWebAdmin.Controllers
{
    public class GiftController : Controller
    {
        private IConfiguration _config;
        private IGiftRepository _gift;

        public GiftController(IConfiguration config, IGiftRepository gift)
        {
            _config = config;
            _gift = gift;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShowGift()
        {
            getGiftByBrandId();
            ViewBag.email = HttpContext.Session.GetString("email");
            return View("ManageGift");
        }

        public async Task<IActionResult> CreateNewGift(Gift gift)
        {
            CreateDefaultPropertiesForGift(gift);
            _gift.Add(gift);
            await _gift.SaveChangesAsync();
            getGiftByBrandId();
            return View("ManageGift");
        }

        public async Task<IActionResult> UpdateGift(Gift gift)
        {
            CreateDefaultPropertiesForGift(gift);
            _gift.Update(gift.GiftId, gift);
            await _gift.SaveChangesAsync();
            getGiftByBrandId();
            return View("ManageGift");
        }

        public IActionResult DeleteGift(Gift gift)
        {
            int giftStatus = _gift.GetStatus(gift.GiftId);
            if (giftStatus != 2)
            {
                _gift.RemoveGiftByUpdateStatus(gift.GiftId);
            }
            else
            {
                ViewBag.GiftIsUsing = "Quà tặng đang được áp dụng, không thể xóa.";
            }
            getGiftByBrandId();
            return View("ManageGift");
        }
        public void CreateDefaultPropertiesForGift(Gift gift)
        {
            string brandId = HttpContext.Session.GetString("brandId");
            string userNameToCreateGift = HttpContext.Session.GetString("adminName");
            DateTime currentDate = DateTime.Now;
            gift.BrandId = brandId;
            gift.Username = userNameToCreateGift;
            gift.CreateDate = currentDate;
            gift.Status = 0;
        }

        public void getGiftByBrandId()
        {
            List<Gift> list = _gift.GetAll().ToList<Gift>();
            List<Gift> listGetByBrandId = new List<Gift>();
            foreach (Gift gift in list)
            {
                if (gift.BrandId.Equals(HttpContext.Session.GetString("brandId")) && gift.Status != 1)
                {
                    listGetByBrandId.Add(gift);
                }
            }
            ViewBag.listGifts = listGetByBrandId;
            ViewBag.email = HttpContext.Session.GetString("email");
        }
    }
}
