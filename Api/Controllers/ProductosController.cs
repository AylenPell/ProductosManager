using Api.Contracts;
using Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private static List<Producto> productList = new List<Producto>()
        {
            new Producto(1, "Libro A", 25000, 1),
            new Producto(2, "Libro B", 32000, 2),
            new Producto(3, "Libro C", 12000, 3)
        };

        [HttpGet]
        public ActionResult<List<Producto>> GetAll()
        {
            var listaProductos = productList.ToList();

            if (listaProductos.Count < 2)
            {
                return Conflict("El numero de productos es menor a 2");
            }

            return Ok(listaProductos);
        }

        [HttpGet("total")]
        public ActionResult<int> GetTotal()
        {
            return Ok(productList.Count());
        }

        [HttpGet("{id}")]
        public ActionResult<List<Producto>> GetById([FromRoute] int id)
        {
            var producto = productList.FirstOrDefault(x => x.Id == id);

            if (producto == null)
            {
                return NotFound("Producto no encontrado");
            }

            return Ok(producto);
        }

        [HttpGet("price/{value}")]
        public ActionResult<List<ProductoResponse>> GetByPrice([FromRoute] decimal value)
        {
            var productos = productList.Where(x => x.Precio >= value).ToList();
            if (!productos.Any())
            {
                return NotFound("No se encontraron productos");
            }

            return Ok(productos);
        }

        [HttpGet("search")]
        public ActionResult<List<ProductoResponse>> GetByName([FromQuery] string name)
        {
            var productos = productList.Where(x => x.Nombre.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!productos.Any())
            {
                return NotFound("No se encontraron productos");
            }

            return Ok(productos);
        }

        [HttpPost]
        public ActionResult<ProductoResponse> Create([FromBody] ProductoRequest producto)
        {
            if (string.IsNullOrEmpty(producto.Nombre) || producto.Precio <= 0 || producto.Stock < 0)
            {
                return BadRequest("Datos inconsistentes, completa bien");
            }

            if (productList.Any())
            {
                producto.Id = productList.Max(x => x.Id) + 1;
            }
            else
            {
                producto.Id = 1;
            }

            int stock = producto.Stock == null ? 10 : producto.Stock.Value;

            var newProducto = new Producto(producto.Id, producto.Nombre, producto.Precio, stock);

            productList.Add(newProducto);

            var returnProducto = new ProductoResponse()
            {
                Id = newProducto.Id,
                Nombre = newProducto.Nombre,
                Precio = newProducto.Precio,
                Stock = newProducto.Stock
            };

            return CreatedAtAction(nameof(GetById), new { id = returnProducto.Id }, returnProducto);
        }

        [HttpPut("{id}")]
        public ActionResult Update([FromRoute] int id, [FromBody] ProductoRequest producto)
        {
            var productoExistente = productList.FirstOrDefault(x => x.Id == id);

            if (productoExistente == null)
            {
                return NotFound("Producto no encontrado");
            }

            if (string.IsNullOrEmpty(producto.Nombre) || producto.Precio <= 0)
            {
                return BadRequest("nombre vacio y/o precio igual a 0.");
            }

            int stock = producto.Stock == null ? 10 : producto.Stock.Value;

            productoExistente.Nombre = producto.Nombre;
            productoExistente.Precio = producto.Precio;
            productoExistente.Stock = stock;

            return NoContent();
        }

        [HttpPatch("{id}")]
        public ActionResult PartialUpdate([FromRoute] int id, [FromBody] ProductoParcialUpdateRequest producto)
        {
            var productoExistente = productList.FirstOrDefault(x => x.Id == id);

            if (productoExistente == null)
            {
                return NotFound("Producto no encontrado");
            }

            if (producto.Stock < 0 || producto.Precio <= 0)
            {
                return BadRequest("Datos inconsistentes");
            }

            productoExistente.Precio = producto.Precio ?? productoExistente.Precio;
            productoExistente.Stock = producto.Stock ?? productoExistente.Stock;

            return NoContent();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] int id)
        {
            var productoExistente = productList.FirstOrDefault(x => x.Id == id);

            if (productoExistente == null)
            {
                return NotFound("Producto no encontrado");
            }

            productList.Remove(productoExistente);

            return NoContent();
        }

        

    }
}
