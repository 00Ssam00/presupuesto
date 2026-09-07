namespace Quincena.Domain.Entidades;

// AppSetting en Node. Se llama "Ajuste" y no "Configuracion" para no chocar
// con el namespace Quincena.Infrastructure.Configuraciones (las clases
// IEntityTypeConfiguration de EF Core) ni con la página "Configuración" del
// frontend, que en realidad gestiona Categorías/Tarjetas/Quincenas.
public class Ajuste
{
    public string Clave { get; set; } = null!;
    public string Valor { get; set; } = null!;
    public DateTime ActualizadoEn { get; set; }
}
