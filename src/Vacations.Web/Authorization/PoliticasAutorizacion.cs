namespace Vacations.Web.Authorization;

public static class Roles
{
    public const string Empleado = "Empleado";
    public const string Aprobador = "Aprobador";
    public const string RRHH = "RRHH";
}

public static class Politicas
{
    public const string RequiereEmpleado = "RequiereEmpleado";
    public const string RequiereAprobador = "RequiereAprobador";
    public const string RequiereRRHH = "RequiereRRHH";
    public const string RequiereAprobadorActivo = "RequiereAprobadorActivo";
}
