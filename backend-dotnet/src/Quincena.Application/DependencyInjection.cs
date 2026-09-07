using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Presupuestos;
using Quincena.Application.Tarjetas;
using Quincena.Application.Categorias;
using Quincena.Application.Panel;
using Quincena.Application.MetodosPago;
using Quincena.Application.Periodos;
using Quincena.Application.Arrastre;
using Quincena.Application.Transacciones;
using Quincena.Application.Metas;
using Quincena.Application.PagosProximos;
using Quincena.Application.Ajustes;

namespace Quincena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IServicioArrastre, ServicioArrastre>();
        services.AddScoped<IServicioPeriodos, ServicioPeriodos>();
        services.AddScoped<IServicioCategorias, ServicioCategorias>();
        services.AddScoped<IServicioTarjetas, ServicioTarjetas>();
        services.AddScoped<IServicioMetodosPago, ServicioMetodosPago>();
        services.AddScoped<IServicioTransacciones, ServicioTransacciones>();
        services.AddScoped<IServicioPresupuestos, ServicioPresupuestos>();
        services.AddScoped<IServicioPanel, ServicioPanel>();
        services.AddScoped<IServicioMetas, ServicioMetas>();
        services.AddScoped<IServicioPagosProximos, ServicioPagosProximos>();
        services.AddScoped<IServicioAjustes, ServicioAjustes>();
        return services;
    }
}
