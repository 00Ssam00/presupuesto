namespace Quincena.Application.Panel;

public interface IServicioPanel
{
    Task<ResultadoPanel?> ObtenerPanelAsync(Guid periodId);
}
