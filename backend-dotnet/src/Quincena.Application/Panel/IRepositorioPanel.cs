using Quincena.Domain.Entidades;

namespace Quincena.Application.Panel;

public interface IRepositorioPanel
{
    Task<Periodo?> ObtenerPeriodoAsync(Guid periodId);
    Task<(decimal TotalGastos, decimal TotalIngresoExtra)> ObtenerTotalesAsync(Guid periodId);
    Task<List<GastoPorCategoriaItem>> ObtenerGastosPorCategoriaAsync(Guid periodId);
    Task<List<PagoProximoConCategoria>> ObtenerPagosProximosAsync(Guid periodId);
}
