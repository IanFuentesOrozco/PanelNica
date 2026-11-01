Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json

' Resultado compartido por las tres fuentes; siempre indica la moneda base.
Public Class DatoTipoCambio
    Public Property Tasa As Decimal
    Public Property FechaFuente As String = "Sin fecha"
    Public Property Documento As DocumentoServicio
End Class

Public Class ServicioTipoCambio
    Private Const Url As String = "https://open.er-api.com/v6/latest/USD"

    Public Async Function ConsultarAsync(ct As CancellationToken) As Task(Of DatoTipoCambio)
        Dim doc = Await ServicioHttp.ObtenerAsync(
            "ExchangeRate-API", Url,
            Function() New HttpRequestMessage(HttpMethod.Get, Url),
            Sub(texto) Interpretar(texto), ct)
        Dim dato = Interpretar(doc.Texto)
        dato.Documento = doc
        Return dato
    End Function

    Public Shared Function Interpretar(texto As String) As DatoTipoCambio
        Using json = JsonDocument.Parse(texto)
            Dim raiz = json.RootElement
            If raiz.GetProperty("result").GetString() <> "success" OrElse
               raiz.GetProperty("base_code").GetString() <> "USD" Then
                Throw New FormatException("La respuesta no contiene tasas válidas con base USD.")
            End If
            Dim tasa = raiz.GetProperty("rates").GetProperty("NIO").GetDecimal()
            If tasa <= 0D Then Throw New FormatException("La tasa NIO/USD debe ser positiva.")
            Dim fecha As JsonElement
            Dim fechaTexto = If(raiz.TryGetProperty("time_last_update_utc", fecha),
                                fecha.GetString(), "Sin fecha")
            Return New DatoTipoCambio With {.Tasa = tasa, .FechaFuente = fechaTexto}
        End Using
    End Function
End Class
