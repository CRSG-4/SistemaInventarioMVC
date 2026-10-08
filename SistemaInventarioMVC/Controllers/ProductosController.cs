using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SistemaInventarioMVC.Models;

namespace SistemaInventarioMVC.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private SistemaInventarioDBEntities db = new SistemaInventarioDBEntities();

        private void CargarCategorias(int? seleccionada = null)
        {
            ViewBag.CategoriaId = new SelectList(
                db.Categoria
                  .Where(c => c.Activo || c.Id == seleccionada)
                  .OrderBy(c => c.Nombre),
                "Id", "Nombre", seleccionada);
        }

        // GET: /Productos
        public ActionResult Index()
        {
            var productos = db.Producto.Include(p => p.Categoria).OrderBy(p => p.Codigo);
            return View(productos.ToList());
        }

        // GET: /Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Producto.Include(p => p.Categoria).FirstOrDefault(p => p.Id == id);
            if (producto == null)
                return HttpNotFound();

            return View(producto);
        }

        // GET: /Productos/Create
        [HttpGet]
        public ActionResult Create()
        {
            CargarCategorias();
            return View(new Producto { Activo = true });
        }

        // POST: /Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Codigo,Nombre,Descripcion,Precio,Existencia,CategoriaId")] Producto producto)
        {
            // Código único
            if (db.Producto.Any(p => p.Codigo == producto.Codigo))
                ModelState.AddModelError("Codigo", "Ya existe un producto con ese código");

            if (ModelState.IsValid)
            {
                producto.Activo = true;
                db.Producto.Add(producto);
                db.SaveChanges();
                TempData["Mensaje"] = "Producto creado correctamente";
                return RedirectToAction("Index");
            }

            CargarCategorias(producto.CategoriaId);   // IMPORTANTE: volver a llenar el dropdown
            return View(producto);
        }

        // GET: /Productos/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = db.Producto.Find(id);
            if (producto == null)
                return HttpNotFound();

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // POST: /Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Codigo,Nombre,Descripcion,Precio,Existencia,CategoriaId,Activo")] Producto producto)
        {
            // Código único, ignorando al propio producto
            if (db.Producto.Any(p => p.Codigo == producto.Codigo && p.Id != producto.Id))
                ModelState.AddModelError("Codigo", "Ya existe otro producto con ese código");

            if (ModelState.IsValid)
            {
                db.Entry(producto).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Mensaje"] = "Producto actualizado correctamente";
                return RedirectToAction("Index");
            }

            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        // POST: /Productos/CambiarEstado/5  (desactivar o activar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id)
        {
            var producto = db.Producto.Find(id);
            if (producto == null)
                return HttpNotFound();

            producto.Activo = !producto.Activo;
            db.SaveChanges();

            TempData["Mensaje"] = producto.Activo ? "Producto activado" : "Producto desactivado";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

    }
}
