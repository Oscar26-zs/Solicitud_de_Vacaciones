namespace Vacations.Web.Helpers;

public static class AvatarHelper
{
    public static string Iniciales(string nombre)
    {
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", partes.Select(p => p[0]).Take(2));
    }

    public static string Color(string nombre)
    {
        var colores = new[] { "f43f5e", "0ea5e9", "8b5cf6", "14b8a6", "f97316", "6366f1", "c026d3" };
        var hash = nombre.Aggregate(0, (acc, c) => acc + c);
        return colores[Math.Abs(hash) % colores.Length];
    }
}
