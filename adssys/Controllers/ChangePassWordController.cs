using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace adssys.Controllers
{
    public class ChangePasswordController:Controller
    {
        [HttpGet("changepassword")]
        public ActionResult Index(string uid)
        {
            if(Helpers.Username != null)
            {
                ViewBag.uid = uid;
                return View();
            }
            
            return RedirectToAction("Index", "Ads");
        }
    }
}
