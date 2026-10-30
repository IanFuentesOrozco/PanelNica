Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Xml.Linq

Public Class ServicioBCN
    Private Const Url As String = "https://servicios.bcn.gob.ni/Tc_Servicio/ServicioTC.asmx"
    Private Const Accion As String = "http://servicios.bcn.gob.ni/RecuperaTC_Dia"

    Public Async Function ConsultarAsync(ct As CancellationToken) As Task(Of DatoTipoCambio)
        ' Nicaragua mantiene UTC-6. La fecha del BCN debe ser la fecha local.
        Dim fecha = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-6)).Date
        Dim clave = Url & "?fecha=" & fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        Dim doc = Await ServicioHttp.ObtenerAsync(
            "BCN", clave,
            Function() CrearSolicitud(fecha),
            Sub(texto) Interpretar(texto), ct)
        Dim dato = Interpretar(doc.Texto)
        dato.Documento = doc
        dato.FechaFuente = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        Return dato
    End Function

    Private Shared Function CrearSolicitud(fecha As Date) As HttpRequestMessage
        Dim xml = $"<?xml version=""1.0"" encoding=""utf-8""?>" &
                  $"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://servicios.bcn.gob.ni/"">" &
                  $"<soap:Body><ser:RecuperaTC_Dia><ser:Año>{fecha.Year}</ser:Año>" &
                  $"<ser:Mes>{fecha.Month}</ser:Mes><ser:Dia>{fecha.Day}</ser:Dia>" &
                  "</ser:RecuperaTC_Dia></soap:Body></soap:Envelope>"
        Dim req As New HttpRequestMessage(HttpMethod.Post, Url)
        req.Headers.TryAddWithoutValidation("SOAPAction", """" & Accion & """")
        req.Content = New StringContent(xml, Encoding.UTF8, "text/xml")
        Return req
    End Function

    Public Shared Function Interpretar(texto As String) As DatoTipoCambio
        Dim xml = XDocument.Parse(texto)
        If xml.Descendants().Any(Function(x) x.Name.LocalName = "Fault") Then
            Throw New FormatException("El BCN devolvió un error SOAP.")
        End If
        Dim nodo = xml.Descendants().FirstOrDefault(
            Function(x) x.Name.LocalName = "RecuperaTC_DiaResult")
        Dim tasa As Decimal
        If nodo Is Nothing OrElse Not Decimal.TryParse(nodo.Value.Trim(),
            NumberStyles.Number, CultureInfo.InvariantCulture, tasa) OrElse tasa <= 0D Then
            Throw New FormatException("El BCN no devolvió una tasa NIO por USD válida.")
        End If
        Return New DatoTipoCambio With {.Tasa = tasa}
    End Function
End Class
