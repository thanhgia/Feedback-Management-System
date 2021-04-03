using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FMSWebAdmin.Models;
using FMSWebAdmin.Models.Dataset;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;
using QRCoder;

namespace FMSWebAdmin.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly IEquipmentRepository equipment;
        private readonly IEquipmentGroup equipmentGroup;
        private UtilModel util = new UtilModel();
        private UtilConstant uConstant = new UtilConstant();

        public EquipmentController(IEquipmentRepository equipment, IEquipmentGroup equipmentGroup)
        {
            this.equipmentGroup = equipmentGroup;
            this.equipment = equipment;
        }


        public IActionResult ShowAll()
        {
            string brandId = HttpContext.Session.GetString("brandId");
            List<EquipmentGroup> list = equipmentGroup.GetEquipmentGroup(brandId).ToList();
            List<Equipment> listEquip = new List<Equipment>();
            foreach (EquipmentGroup group in list)
            {
                IQueryable<Equipment> tmp = equipment.GetEquipmentByGroupId(group.EquipmentGroupId);
                if (tmp != null)
                {
                    listEquip.AddRange(tmp.ToList());
                }
            }
            ViewBag.listEquip = listEquip;
            ViewBag.listEquipGroup = list;
            ViewBag.listJson = CreateListJson(listEquip);

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        private string CreateListJson(List<Equipment> list)
        {
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var obj = JsonConvert.SerializeObject(list, Formatting.Indented, serializerSettings);
            return obj;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNew(Equipment equip)
        {
            equip.EquipmentId = util.RandomString(8);
            Equipment check;
            do
            {
                check = null;
                check = equipment.Get(equip.EquipmentId);
            }
            while (check != null);
            equip.EquipmentSercurityCode = util.RandomString(10);
            equip.CreateDate = DateTime.Now;
            equip.Username = HttpContext.Session.GetString("adminName");

            equipment.Add(equip);
            await equipmentGroup.SaveChangesAsync();

            if (equip.Type == 2)
            {
                string url = "https://feedbackwebqr.azurewebsites.net/" + "Home/GetFeedback?equipmentId=" + equip.EquipmentId;
                return RedirectToAction("ShowQR", new { qrText = url });
            }
            return RedirectToAction("ShowAll");
        }

        public IActionResult ShowQR(string qrText)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q); //QRCodeGenerator.ECCLevel.Q là mức chịu lỗi 25%; .L là 7%; .M là 15% và .H là trên 25%
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, false); // draw quiet zones (co ve~ vien` hay ko)
            ViewBag.Url = JsonConvert.SerializeObject(new { r = BitmapToBytesCode(qrCodeImage) });

            return View();
        }

        private static Byte[] BitmapToBytesCode(Bitmap image) // chuyen doi hinh anh bitmap thanh byte[]
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png); // luu anh vao memorystream
                return stream.ToArray(); // tra ve mang byte
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateEquipment(Equipment equip)
        {
            equip.CreateDate = DateTime.Now;
            equip.Username = HttpContext.Session.GetString("adminName");
            equipment.Update(equip.EquipmentId, equip);
            if (await equipment.SaveChangesAsync() >= 0)
            {
                return RedirectToAction("ShowAll");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout(Equipment equip)
        {
            string token = await equipment.GetFCMTokenById(equip.EquipmentId);
            if (token != null)
            {
                string[] tokens = { token };
                var data = new { action = uConstant.FIREBASE_LOGOUT_ACTION, click_action = "FLUTTER_NOTIFICATION_CLICK" };
                await SendPushNoti(tokens, "SERVER_REQUEST", "REQUEST_BODY", data);

                await HttpContext.Session.LoadAsync();
                string email = HttpContext.Session.GetString("email");
                
                return RedirectToAction("ShowAll");
            }
            return View();
        }

        public async Task<bool> SendPushNoti(string[] deviceToken, string title, string body, object data)
        {
            bool sent = false;
            var messageInformation = new FMSMessage()
            {
                notification = new FMSNoti()
                {
                    title = title,
                    text = body
                },
                data = data,
                registration_ids = deviceToken
            };
            //Object to JSON STRUCTURE => using Newtonsoft.Json;
            string jsonMessage = JsonConvert.SerializeObject(messageInformation);

            var request = new HttpRequestMessage(HttpMethod.Post, uConstant.FIREBASE_URL);
            request.Headers.TryAddWithoutValidation("Authorization", "Key =" + uConstant.FIREBASE_KEY);
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                result = await client.SendAsync(request);
                sent = sent && result.IsSuccessStatusCode;
            }
            return sent;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEquipment(Equipment delEquipment)
        {
            Equipment equip = equipment.Get(delEquipment.EquipmentId);
            if (equip.Active == 1)
            {
                ViewBag.isLoginError = true;
                return View();
            }
            equipment.UpdateActiveToRemove(delEquipment.EquipmentId);
            if (await equipment.SaveChangesAsync() >= 0)
            {
                await HttpContext.Session.LoadAsync();
                string email = HttpContext.Session.GetString("email");
                string brandId = HttpContext.Session.GetString("brandId");
                string emailEncode = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
                await HttpContext.Session.CommitAsync();
                return RedirectToAction("ShowAll", new { email = emailEncode});
            }
            return View();
        }
    }
}
