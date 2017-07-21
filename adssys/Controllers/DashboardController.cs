using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using adssys.Models;


namespace adssys.Controllers
{
    public class DashboardController:Controller
    {

        [HttpGet("/dashboard")]
        public ActionResult Index(string uid)
        {
            if(Helpers.Username != null)
            {
                ViewBag.uid = uid;
                var data = new List<AdsSystem> { };
                data.Add(new AdsSystem
                {
                    ads = new List<Ads> { },
                    provider = new List<Provider> { }
                });

                dynamic adsList = AdsSystemDb.LoadAds();
                dynamic providerList = AdsSystemDb.LoadProvider();
                for (var aid = 0; aid < adsList.Length; aid++)
                {
                    data.First().ads.Add(
                        new Ads
                        {
                            Id = adsList[aid].c.id,
                            Title = adsList[aid].c.title,
                            Link = adsList[aid].c.link,
                            Description = adsList[aid].c.description,
                            Icon = adsList[aid].c.icon
                        }
                        );
                }

                for (var pid = 0; pid < providerList.Length; pid++)
                {
                    data.First().provider.Add(new Provider
                    {
                        Id = providerList[pid].c.id,
                        Title = providerList[pid].c.title,
                        SecretKey = providerList[pid].c["secret-key"],
                        AdsId = providerList[pid].c["ads-id"]
                    });

                }

                return View(data);
            }
            return RedirectToAction("Index", "Ads");
        }
    }
}
