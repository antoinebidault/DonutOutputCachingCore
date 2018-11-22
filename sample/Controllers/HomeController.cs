﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using DonutOutputCachingCore;
using Microsoft.Net.Http.Headers;

namespace Sample.Controllers
{
    public class HomeController : Controller
    {
        [DonutOutputCache(Duration = 600)]
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Api()
        {
            HttpContext.EnableOutputCaching(TimeSpan.FromMinutes(1));
            return View("Index");
        }

        [DonutOutputCache(Duration = 600, VaryByParam = "foo")]
        public IActionResult Query()
        {
            return View("Index");
        }

        [DonutOutputCache(Profile = "default")]
        public IActionResult Profile()
        {
            return View("Index");
        }

        public IActionResult Redirect()
        {
            Response.Headers.Add(HeaderNames.CacheControl, "no-store, no-cache, must-revalidate");
            return RedirectToActionPermanent("Index");
        }

        public IActionResult NoCache()
        {
            Response.Headers.Add(HeaderNames.CacheControl, "no-store, no-cache, must-revalidate");
            return View("Index");
        }
    }
}
