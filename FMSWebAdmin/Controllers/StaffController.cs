using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSWebAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;

namespace FMSWebAdmin.Controllers
{
    public class StaffController : Controller
    {
        private readonly IEquipmentRepository equipmentRepository;
        private readonly IAccountRepository accountRepository;
        public StaffController(IAccountRepository accountRepository, IEquipmentRepository equipmentRepository)
        {
            this.accountRepository = accountRepository;
            this.equipmentRepository = equipmentRepository;
        }

        public IActionResult ShowResult(string email)
        {
            string emailDecode = Encoding.UTF8.GetString(Convert.FromBase64String(email));
            string groupId = accountRepository.CheckEmail(emailDecode);
            
            List<Equipment> listEquipment = equipmentRepository.GetEquipmentByGroupId(groupId).ToList<Equipment>();

            ViewBag.token = TempData["token"];
            ViewBag.listEquipment = listEquipment;
            ViewBag.email = emailDecode;

            return View();
        }
    }
}
