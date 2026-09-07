namespace Quincena.Application.Ajustes;

// Port de backend/src/controllers/settingsController.ts.
public class ServicioAjustes : IServicioAjustes
{
    private readonly IRepositorioAjustes _repository;

    public ServicioAjustes(IRepositorioAjustes repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<string, string>> ObtenerTodosAsync()
    {
        var ajustes = await _repository.ObtenerTodosAsync();
        return ajustes.ToDictionary(a => a.Clave, a => a.Valor);
    }

    public Task ActualizarAsync(string clave, string valor) => _repository.GuardarAsync(clave, valor);
}
