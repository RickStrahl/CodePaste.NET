using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace CodePasteMvc.Controllers
{
    public class HomeController : baseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Developer()
        {
            return View();
        }

        public ActionResult WhatsNew()
        {
            return View();
        }
    }
}
