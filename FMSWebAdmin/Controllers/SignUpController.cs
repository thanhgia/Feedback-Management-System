using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using ModelsFeedbackSystem.Models;
using ModelsFeedbackSystem.Repository;

namespace FMSWebAdmin.Controllers
{
    public class SignUpController : Controller
    {
        private readonly IConfiguration _config;
        IAccountRepository _accountRepository;

        public SignUpController(IConfiguration config, IAccountRepository accountRepository)
        {
            _config = config;
            _accountRepository = accountRepository;
        }

        public IActionResult SignUp()
        {
            return View();
        }
        /// <summary>
        /// Add new user into database
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateNewUser()
        {
            int checkAddAccount = 0;
            string userName = Request.Form["username"];
            string fullName = Request.Form["fullname"];
            string phone = Request.Form["phone"];
            string confirmPass = Request.Form["confirm_password"];
            string email = Request.Form["email"];
            int status = 0;
            int role = 0;
            string brandId = "B000001";
            DateTime createDate = DateTime.Now;

            Account account = new Account()
            {
                Username = userName,
                Fullname = fullName,
                Phone = phone,
                Email = email,
                Password = confirmPass,
                Status = status,
                Role = role,
                CreateDate = createDate,
                BrandId = brandId
            };
            _accountRepository.Add(account);
            checkAddAccount = await _accountRepository.SaveChangesAsync();
            if (checkAddAccount > 0)
            {
                return RedirectToAction("Login", "Account");
                    }
            return View("Error");

        }
    }
}
