using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Presupuestos;
using Quincena.Application.Tarjetas;
using Quincena.Application.Categorias;
using Quincena.Application.Panel;
using Quincena.Application.MetodosPago;
using Quincena.Application.Periodos;
using Quincena.Application.Arrastre;
using Quincena.Application.Transacciones;
using Quincena.Infrastructure.Repositorios;

namespace Quincena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<QuincenaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IRepositorioPeriodos, RepositorioPeriodos>();
        services.AddScoped<IFuenteDatosArrastre, FuenteDatosArrastre>();
        services.AddScoped<IRepositorioCategorias, RepositorioCategorias>();
        services.AddScoped<IRepositorioTarjetas, RepositorioTarjetas>();
        services.AddScoped<IRepositorioMetodosPago, RepositorioMetodosPago>();
        services.AddScoped<IRepositorioTransacciones, RepositorioTransacciones>();
        services.AddScoped<IRepositorioPresupuestos, RepositorioPresupuestos>();
        services.AddScoped<IRepositorioPanel, RepositorioPanel>();
        return services;
    }
}
