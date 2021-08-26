using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Controllers
{
    public class StatsController : Controller
    {
        // GET: StatsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: StatsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StatsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StatsController/Create
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

        // GET: StatsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StatsController/Edit/5
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

        // GET: StatsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StatsController/Delete/5
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
