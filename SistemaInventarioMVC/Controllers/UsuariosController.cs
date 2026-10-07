using SistemaInventarioMVC.Helpers;
using SistemaInventarioMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaInventarioMVC.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private SistemaInventarioDBEntities db = new SistemaInventarioDBEntities();

        // GET: /Usuarios  -> lista de usuarios (requiere sesión)
        public ActionResult Index()
        {
            var usuarios = db.Usuarios.OrderBy(u => u.Nombre).ToList();
            return View(usuarios);
        }

        // GET: /Usuarios/Create -> muestra el formulario
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Usuarios/Create -> procesa el formulario
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string correo = model.Correo.Trim();

            // El enunciado pide correo único: lo verificamos antes de guardar
            bool existe = db.Usuarios.Any(u => u.Correo == correo);
            if (existe)
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado");
                return View(model);
            }

            var usuario = new Usuarios
            {
                Nombre = model.Nombre.Trim(),
                Correo = correo,
                PasswordHash = PasswordHelper.Hash(model.Password),
                Activo = true
            };

            db.Usuarios.Add(usuario);
            db.SaveChanges();

            TempData["Mensaje"] = "Usuario creado correctamente";

            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}