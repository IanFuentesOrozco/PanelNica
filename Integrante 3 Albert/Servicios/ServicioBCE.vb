Imports System.Globalization
Imports System.Net.Http
Imports System.Xml.Linq

' BCE: EUR/USD (solo referencia, NO reemplaza la tasa oficial NIO/USD).
Public Class ServicioBCE
    Private Const Url As String = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"

    Public Async Function ConsultarAsync(ct As CancellationToken) As Task(Of DatoTipoCambio)
        Dim doc = Await ServicioHttp.ObtenerAsync(
            "BCE", Url,
            Function() New HttpRequestMessage(HttpMethod.Get, Url),
            Sub(texto) Interpretar(texto), ct)
        Dim dato = Interpretar(doc.Texto)
        dato.Documento = doc
        Return dato
    End Function

    Public Shared Function Interpretar(texto As String) As DatoTipoCambio
        Dim xml = XDocument.Parse(texto)
        Dim nodo = xml.Descendants().FirstOrDefault(
            Function(x) x.Name.LocalName = "Cube" AndAlso
                CStr(x.Attribute("currency")) = "USD")
        Dim tasa As Decimal
        If nodo Is Nothing OrElse Not Decimal.TryParse(CStr(nodo.Attribute("rate")),
            NumberStyles.Number, CultureInfo.InvariantCulture, tasa) OrElse tasa <= 0D Then
            Throw New FormatException("No se encontró la referencia EUR/USD del BCE.")
        End If
        Dim fechaNodo = nodo.Parent
        Dim fecha = If(fechaNodo IsNot Nothing, CStr(fechaNodo.Attribute("time")), "Sin fecha")
        Return New DatoTipoCambio With {.Tasa = tasa, .FechaFuente = fecha}
    End Function
End Class
