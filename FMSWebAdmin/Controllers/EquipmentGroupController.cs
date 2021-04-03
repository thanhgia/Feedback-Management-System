using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMSWebAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;

namespace FMSWebAdmin.Controllers
{
    public class EquipmentGroupController : Controller
    {
        private readonly IEquipmentGroup equipmentGroup;
        private UtilModel util = new UtilModel();

        public EquipmentGroupController(IEquipmentGroup equipmentGroup)
        {
            this.equipmentGroup = equipmentGroup;
        }

        public IActionResult ShowAll()
        {
            string brandId = HttpContext.Session.GetString("brandId");
            IQueryable<EquipmentGroup> list = equipmentGroup.GetEquipmentGroup(brandId);
            if (list != null)
            {
                List<EquipmentGroup> listGroup = list.ToList<EquipmentGroup>();
                ViewBag.listEquipGroup = listGroup;
            }

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNew(EquipmentGroup equipGroup)
        {
            equipGroup.EquipmentGroupId = util.RandomString(8);
            EquipmentGroup group;
            do
            {
                group = null;
                group = equipmentGroup.Get(equipGroup.EquipmentGroupId);
            }
            while (group != null);
            await HttpContext.Session.LoadAsync();
            equipGroup.CreateDate = DateTime.Now;
            equipGroup.Username = HttpContext.Session.GetString("adminName");
            equipmentGroup.Add(equipGroup);
            return RedirectToAction("ShowAll");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroupEquipment(EquipmentGroup equipGroup)
        {
            equipGroup.Username = HttpContext.Session.GetString("adminName");
            equipGroup.CreateDate = DateTime.Now;
            equipmentGroup.Update(equipGroup.EquipmentGroupId, equipGroup);
            if (await equipmentGroup.SaveChangesAsync() >= 0)
            {
                return RedirectToAction("ShowAll");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEquipGroup(EquipmentGroup delEquipGroup)
        {
            equipmentGroup.UpdateStatusById(delEquipGroup.EquipmentGroupId);
            if (await equipmentGroup.SaveChangesAsync() >= 0)
            {
                return RedirectToAction("ShowAll");
            }
            return View();
        }
    }
}
