using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FMSWebAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;
using Newtonsoft.Json;


namespace FMSWebAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository accountRepository;
        private readonly IConfiguration config;

        public AccountController(IAccountRepository accountRepository, IConfiguration config)
        {
            this.accountRepository = accountRepository;
            this.config = config;
        }

        public IActionResult Logout()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> LoginByGoogle(string token)
        {
            const string GoogleApiTokenInfoUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";
            var httpClient = new HttpClient();
            var requestUri = new Uri(string.Format(GoogleApiTokenInfoUrl, token));
            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = httpClient.GetAsync(requestUri).Result;
            }
            catch (Exception)
            {
                return RedirectToAction("Logout");
            }
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return RedirectToAction("Logout");
            }

            string response = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var googleApiTokenInfo = JsonConvert.DeserializeObject<GoogleApiTokenInfo>(response);

            Account account = await accountRepository.GetAccountByEmail(googleApiTokenInfo.email);
            if (account == null)
            {
                return RedirectToAction("Logout");
            }
            else
            {
                UtilModel utilModel = new UtilModel(config);

                string email = googleApiTokenInfo.email; 
                string emailEncode = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
                int role = account.Role;

                TempData["token"] = utilModel.CreateToken(email);

                if (role == 0)
                {
                    return RedirectToAction("ShowResult", "Staff", new { email = emailEncode });
                } else
                {
                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetString("brandId", account.BrandId.ToString());
                    HttpContext.Session.SetString("date", DateTime.Now.ToString());
                    HttpContext.Session.SetString("adminName", account.Username);
                    HttpContext.Session.SetString("email", account.Email);
                    await HttpContext.Session.CommitAsync();

                    return RedirectToAction("Show", "Admin");
                }
            } 
        }


        public async Task<IActionResult> CheckToken(string email, string token)
        {
            UtilModel utilModel = new UtilModel(config);
            
            if (utilModel.ValidateToken(token))
            {
                string emailDecode = Encoding.UTF8.GetString(Convert.FromBase64String(email));
                Account account = await accountRepository.GetAccountByEmail(emailDecode);
                int role = account.Role;

                if (role == 0)
                {
                    return RedirectToAction("ShowResult", "Staff", new { email = email });
                }
                else
                {
                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetString("brandId", account.BrandId.ToString());
                    HttpContext.Session.SetString("date", DateTime.Now.ToString());
                    HttpContext.Session.SetString("adminName", account.Username);
                    HttpContext.Session.SetString("email", account.Email);
                    await HttpContext.Session.CommitAsync();

                    return RedirectToAction("Show", "Admin");
                }
            }
            return RedirectToAction("Logout");
        }

        [HttpPost]
        public async Task<IActionResult> LoginByAccount()
        {
            string userName = Request.Form["username"];
            string passWord = Request.Form["pass"];
            Account resultAccount = accountRepository.Login(userName, passWord);
            if (resultAccount != null)
            {
                UtilModel util = new UtilModel(config);
                string emailEncode = Convert.ToBase64String(Encoding.UTF8.GetBytes(resultAccount.Email));
                //TempData["token"] = util.CreateToken(resultEmail);
                ViewBag.token = util.CreateToken(resultAccount.Email);
                ViewBag.brandid = resultAccount.BrandId;
                ViewBag.email = resultAccount.Email;
                //use to showAccountByAdminName on ManageAccount Page (Sub-admin was create by username-Admin)
                ViewBag.adminName = resultAccount.Username;

                //Admin ko dc tạo bởi admin khác nên adminName = null  Mai
                await HttpContext.Session.LoadAsync();
                if (String.IsNullOrEmpty(resultAccount.AdminName))
                {
                    HttpContext.Session.SetString("brandId", resultAccount.BrandId.ToString());
                    HttpContext.Session.SetString("date", DateTime.Now.ToString());
                    HttpContext.Session.SetString("adminName", resultAccount.Username);
                }
                HttpContext.Session.SetString("email", resultAccount.Email);
                await HttpContext.Session.CommitAsync();
                
                /*return View("Admin");*/
                return RedirectToAction("Show", "Admin");
            }
            return View("Login");
        }


        public async Task<IActionResult> DisplayAccountByAdminName()
        {
           
            List<Account> resultAccount = await getListSubAddmin();
            ViewBag.listAccount = resultAccount;
            ViewBag.email = HttpContext.Session.GetString("email");
            /*List<Account> resultAccount = await accountRepository.GetSubAccountByAdminName(adminName);*/
          /*  ViewBag.listAccount = resultAccount;*/
            SaveDataAccountInViewBag();

            return View("ManageAccount");
        }

        public async Task<IActionResult> CreateNewAccount(Account account)
        {
            accountRepository.Add(account);
            await accountRepository.SaveChangesAsync();
            //luu data trong viewbag de add new lan tiep theo 
            SaveDataAccountInViewBag();
            // load lai data tu csdl
            /*List<Account> resultAccount = await accountRepository.GetSubAccountByAdminName(HttpContext.Session.GetString("adminName"));*/
            List<Account> resultAccount = await getListSubAddmin();
            ViewBag.listAccount = resultAccount;
            return View("ManageAccount");
        }

        public async Task<IActionResult> DeleteAccount(Account account)
        {
            accountRepository.RemoveAccountByUpdate(account.Username);
            await accountRepository.SaveChangesAsync();
            List<Account> resultAccount = await getListSubAddmin();
            ViewBag.listAccount = resultAccount;
            return View("ManageAccount");
        }


        public void SaveDataAccountInViewBag()
        {
            ViewBag.sessionBrandId = HttpContext.Session.GetString("brandId");
            ViewBag.sessionDate = HttpContext.Session.GetString("date");
            ViewBag.sessionAdminName = HttpContext.Session.GetString("adminName");
            ViewBag.email = HttpContext.Session.GetString("email");
        }

        public async Task<List<Account>> getListSubAddmin()
        {
            string t = HttpContext.Session.GetString("adminName");
            List<Account> resultAccount = new List<Account>();
            List<Account> listSubAccount = await accountRepository.GetSubAccountByAdminName(HttpContext.Session.GetString("adminName"));
            foreach (Account acc in listSubAccount)
            {
                if (acc.Status != 0)
                {
                    resultAccount.Add(acc);
                }
            }
            if (resultAccount != null)
            {
                return resultAccount;
            }
            return null;
        }
    }
}
