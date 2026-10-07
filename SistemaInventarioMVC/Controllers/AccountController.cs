using SistemaInventarioMVC.Helpers;
using SistemaInventarioMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace SistemaInventarioMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SistemaInventarioDBEntities db = new SistemaInventarioDBEntities();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hash = PasswordHelper.Hash(model.Password);

            var usuario = db.Usuarios
                .FirstOrDefault(u => u.Correo == model.Correo && u.Activo);

            if (usuario == null ||
                !string.Equals(usuario.PasswordHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(usuario.Correo, false);

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

    }
}