using Quincena.Domain.Entidades;

namespace Quincena.Application.Categorias;

// Port de backend/src/controllers/categoriesController.ts.
public class ServicioCategorias : IServicioCategorias
{
    private readonly IRepositorioCategorias _repository;

    public ServicioCategorias(IRepositorioCategorias repository)
    {
        _repository = repository;
    }

    public Task<List<Categoria>> ListarAsync() => _repository.ObtenerActivosOrdenadosAsync();

    public Task<Categoria> CrearAsync(CrearCategoriaPeticion request)
    {
        var category = new Categoria
        {
            Nombre = request.Nombre,
            Color = string.IsNullOrEmpty(request.Color) ? "#01696f" : request.Color,
            Icono = string.IsNullOrEmpty(request.Icono) ? null : request.Icono,
            Orden = request.Orden ?? 0,
        };
        return _repository.AgregarAsync(category);
    }

    public Task<Categoria?> ActualizarAsync(Guid id, ActualizarCategoriaPeticion request) =>
        _repository.ActualizarAsync(id, category =>
        {
            category.Nombre = request.Nombre;
            category.Color = request.Color;
            // COALESCE: solo se pisa si el campo vino en el body (no null); si no
            // vino, se conserva el valor actual — así una edición que solo manda
            // name/color no borra icon/is_active/sort_order.
            if (request.Icono != null) category.Icono = request.Icono;
            if (request.EstaActivo.HasValue) category.EstaActivo = request.EstaActivo.Value;
            if (request.Orden.HasValue) category.Orden = request.Orden.Value;
        });

    public Task EliminarAsync(Guid id) => _repository.DesactivarAsync(id);
}
