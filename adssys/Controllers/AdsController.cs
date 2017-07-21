using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using adssys.Models;

using User = adssys.Models.User;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace adssys.Controllers
{
    [Route("ads")]
    public class AdsController:Controller
    {

        [HttpGet("/")]
        public Object Index()
        {
            if (Helpers.SecretKey == null)
            {

                return View();
            }
            return RedirectToAction("Index", "Dashboard", new { uid = Helpers.Username });
        }

        [HttpPost("v1/register")]
        public void register()
        {
            User user = new User
            {
                Username = "admin",
                Password = "Pine0908"
            };
            
            if (Helpers.isValidRegister(user))
            {
                AdsSystemDb.Register(user).Wait();
            }
        }

        [HttpPost("status")]
        public Object statusSystem()
        {
            var obj = AdsSystemDb.GetStatus();
            if(obj == null)
            {
                return new
                {
                    error = "connection failed."
                };
            }
            return obj;
        }

        [HttpPost("v1/authentication")]
        public ActionResult authentication()
        {
            var pwd = Request.Form["password"];
            var uid = Request.Form["username"];
            dynamic uObj = Helpers.checkExistUser(uid);
            
            if (uObj != null && uObj.Username == uid)
            {
                var pwdShadow = Helpers.Decrypt(uObj.Password, "username:" + uid + ".");
                if(pwdShadow == pwd)
                {
                    if (Helpers.SecretKey == null)
                    {
                        Helpers.SecretKey = Helpers.Encrypt(pwd, "username:" + uid + ".");
                        Helpers.Username = uid;
                    }
                    return RedirectToAction("Index", "Dashboard", new { uid = Helpers.Username });
                }
            }
            return RedirectToAction("Index", "Ads");
        }
#region cryptTest
        private static string mCode;
        [HttpPost("encode")]
        public string encode()
        {
            var pwd = Request.Form["password"];
            var uid = Request.Form["username"];
            var code = Helpers.Encrypt(pwd, "username:" + uid + ".");
            mCode = code;
            return code;
        }
        [HttpPost("decode")]
        public string decode()
        {
            var code = Helpers.Decrypt(mCode, Helpers.DecryptKey);
            return code;
        }
#endregion
        [HttpPost("v1/delivery")]
        public Object delivery()
        {
            dynamic objAdsInfo = null;
            var deliver = new Delivery
            {
                providerId = Request.Form["provider-id"],
                deviceId = Request.Form["device-id"],
                salt = Request.Form["salt"],
                sign = Request.Form["sign"]
            };
            var dbSecretKey = AdsSystemDb.getSecretKey(Request.Form["provider-id"]);
            var dbAdsId = AdsSystemDb.getAdsId(Request.Form["provider-id"]);
            var hash = Helpers.GenerateSHA256String("provider-id:" + deliver.providerId + ".device-id:" + deliver.deviceId + "." + deliver.salt + "." + dbSecretKey);

            if (hash == deliver.sign)
            {
                objAdsInfo = AdsSystemDb.getObjAds(dbAdsId);
            }
            return objAdsInfo;
        }

        [HttpPost("v1/metadata")]
        public string metadata()
        {
            return "metadata";
        }

        [HttpPost("addAds")]
        public async Task<IActionResult> addAds(List<IFormFile> files)
        {
            string blobPath = null;
            Ads ads = new Ads
            {
                Id = Helpers.GetUID().ToString(),
                Title = Request.Form["title"],
                Link = Request.Form["link"],
                Description = Request.Form["description"]
            };
            
            Object _ads = AdsSystemDb.FindAds(ads);
   
            long size = files.Sum(f => f.Length);

            var filePath = Path.GetTempFileName();

            foreach(var formFile in files)
            {
                if(formFile.Length > 0)
                {
                    using(var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            if (_ads == null && files[0].Length > 0)
            {
                Array s;
                s = files[0].FileName.Split('.');
                string fileName = Helpers.GetUID().ToString() + "." + s.GetValue(1).ToString();
                blobPath = AdsSystemDb.UploadIcon(filePath, fileName);
                ads.Icon = blobPath;
                AdsSystemDb.AddAds(ads).Wait();
               
            }
            return RedirectToAction("Index", "Dashboard", new { uid = Helpers.Username }); 
        }

        [HttpPost("addProvider")]
        public Object addProvider(Provider provider)
        {
            foreach (var prop in provider.GetType().GetProperties())
            {
                if (prop.GetValue(provider) == null)
                {
                    //prop.SetValue(provider, "");
                }
            }
            provider.Id = Helpers.GetUID().ToString();
            AdsSystemDb.AddProvider(provider).Wait();
            string adsTitle = AdsSystemDb.getAds(provider.AdsId);
            return new { provider, adsTitle };
        }

        [HttpPost("updateProvider")]
        public string updateProvider(Provider provider)
        {
            AdsSystemDb.UpdateProvider(provider).Wait();
            string adsTitle = AdsSystemDb.getAds(provider.AdsId);
            return adsTitle;
        }

        [HttpPost("changedPassword")]
        public ActionResult changedPassword()
        {
            var uid = Request.Form["username"];
            var curPwd = Request.Form["currentPassword"];
            var newPwd = Request.Form["newPassword"];
            var user = new User
            {
                Username = uid,
                Password = curPwd
            };
            dynamic uObj = Helpers.checkExistUser(uid);
            
            if (uObj != null )
            {
                var pwdShadow = Helpers.Decrypt(uObj.Password, "username:" + uid.ToString().ToLower() + ".");
                if(pwdShadow == curPwd.ToString().ToLower())
                {
                    AdsSystemDb.UpdatePasswordAdmin(user, newPwd).Wait();
                    return RedirectToAction("Index", "Ads");
                }
            }
            return RedirectToAction("Index", "ChangePassWord", new { uid = Helpers.Username });
        }

        
    }
}
