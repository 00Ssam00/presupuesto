using Quincena.Domain.Entidades;

namespace Quincena.Application.PagosProximos;

// Port de backend/src/controllers/upcomingPaymentsController.ts.
public class ServicioPagosProximos : IServicioPagosProximos
{
    private readonly IRepositorioPagosProximos _repository;

    public ServicioPagosProximos(IRepositorioPagosProximos repository)
    {
        _repository = repository;
    }

    public Task<List<PagoProximoListaItem>> ListarAsync(Guid? periodoId, string? estado) =>
        _repository.ListarConCategoriaAsync(periodoId, estado);

    public Task<PagoProximo> CrearAsync(CrearPagoProximoPeticion request)
    {
        var payment = new PagoProximo
        {
            PeriodoId = request.PeriodoId,
            CategoriaId = request.CategoriaId,
            Nombre = request.Nombre,
            Monto = request.Monto,
            FechaVencimiento = request.FechaVencimiento,
            EsRecurrente = request.EsRecurrente ?? false,
            IntervaloRecurrencia = request.IntervaloRecurrencia,
        };
        return _repository.AgregarAsync(payment);
    }

    public Task<PagoProximo?> ActualizarAsync(Guid id, ActualizarPagoProximoPeticion request) =>
        _repository.ActualizarAsync(id, payment =>
        {
            payment.CategoriaId = request.CategoriaId;
            payment.Nombre = request.Nombre;
            payment.Monto = request.Monto;
            payment.FechaVencimiento = request.FechaVencimiento;
            payment.Estado = request.Estado;
            payment.EsRecurrente = request.EsRecurrente;
            payment.IntervaloRecurrencia = request.IntervaloRecurrencia;
        });

    public Task EliminarAsync(Guid id) => _repository.EliminarAsync(id);
}
