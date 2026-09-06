namespace Quincena.Application.Cards;

public class CreateCardRequest
{
    public string Name { get; set; } = null!;
    public string? LastFour { get; set; }
    public string? Bank { get; set; }
}

// name/last_four/bank siempre se sobrescriben (igual que Node, sin COALESCE —
// omitir last_four/bank en un PUT los deja en null); is_active se preserva si
// no viene en el body — ver CardService.UpdateAsync.
public class UpdateCardRequest
{
    public string Name { get; set; } = null!;
    public string? LastFour { get; set; }
    public string? Bank { get; set; }
    public bool? IsActive { get; set; }
}
