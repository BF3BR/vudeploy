using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Controllers
{
    public class MatchController : Controller
    {
        // GET: MatchController
        public ActionResult Index()
        {
            return View();
        }

        // GET: MatchController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MatchController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MatchController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MatchController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MatchController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MatchController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MatchController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
