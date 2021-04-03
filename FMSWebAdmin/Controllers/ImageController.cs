using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using FMSWebAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;

namespace FMSWebAdmin.Controllers
{
    public class ImageController : Controller
    {
        private readonly IImageRepository imageRepository;
        private UtilModel util = new UtilModel();
        private static string apiKey = "AIzaSyBaM-mhp86Lv_-ksCsJU9YE1uGZKkleFIU"; // AIzaSyATSz1be4QOBGg8H_N7Ur949WeZMjYZAZ0
        private static string bucket = "feedbacksystem-282204.appspot.com"; // feedbacksystem-85eec.appspot.com
        private static string authEmail = "mrhiep314@gmail.com"; //feedback.fms.vn@gmail.com
        private static string authPassword = "ZZNGUYENDUCHIEPZZ"; // 123456@123456

        public ImageController(IImageRepository imageRepository)
        {
            this.imageRepository = imageRepository;
        }
        public async Task<IActionResult> ShowAll()
        {
            await HttpContext.Session.LoadAsync();
            string brandId = HttpContext.Session.GetString("brandId");
            List<Image> listImage = await imageRepository.GetImageByBrandId(brandId);
            ViewBag.listImage = JsonConvert.SerializeObject(listImage);

            ViewBag.email = HttpContext.Session.GetString("email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNew([FromForm] FileInputModel model)
        {
            await HttpContext.Session.LoadAsync();
            string brandId = HttpContext.Session.GetString("brandId");
            string username = HttpContext.Session.GetString("adminName");
            string imageUrl = await PostImage(model.FileToUpload, brandId);
            Image image = new Image();

            image.BrandId = brandId;
            image.ImageId = util.RandomString(5);
            image.ImageName = model.Name;
            image.ImageLink = imageUrl;
            image.Username = username;
            image.CreateDate = DateTime.Now;

            imageRepository.Add(image);
            if (await imageRepository.SaveChangesAsync() >= 0)
            {
                return RedirectToAction("ShowAll");
            }

            return View();
        }

        public async Task<string> PostImage(IFormFile fileUpload, string brandId)
        {
            //var fileUpload = file.File;
            Stream ms = fileUpload.OpenReadStream();
            if (fileUpload.Length > 0)
            {
                // fs = new FileStream(Path.Combine(path, fileUpload.FileName), FileMode.Open);
                var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPassword);
                var cancellation = new CancellationTokenSource();
                Task<string> b = Task.FromResult(a.FirebaseToken);
                var task = new FirebaseStorage(
                     bucket,
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                         ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                     })
                     .Child("BrandImage")
                     .Child($"{brandId}")
                     .Child($"{fileUpload.FileName}")
                     .PutAsync(ms, cancellation.Token);

                try
                {
                    string link = await task;
                    return link;

                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }
}
