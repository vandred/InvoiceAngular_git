using InvoiceAngular.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InvoiceAngular.Controllers
{
    public class MainController : Controller
    {
        /// <summary>
        /// This maps to the Main/Index.cshtml file.  This file is the main view for the application.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            return View();
        }
    }
}