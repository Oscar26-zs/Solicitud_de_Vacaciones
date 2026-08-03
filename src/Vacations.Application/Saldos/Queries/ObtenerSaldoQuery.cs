namespace Vacations.Application.Saldos.Queries;

/// <summary>Query para consultar el saldo de un empleado (CU-02).</summary>
public sealed record ObtenerSaldoQuery
{
    /// <summary>Id del empleado cuyo saldo se consulta.</summary>
    public Guid EmpleadoId { get; init; }

    /// <summary>Id del empleado autenticado (null si es RRHH consultando a otro).</summary>
    public Guid? EmpleadoSolicitanteId { get; init; }

    public bool EsRRHH { get; init; }
}