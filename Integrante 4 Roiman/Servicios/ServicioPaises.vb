Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks

Public Class ResultadoPaises
    Public Property Filas As New List(Of PaisMoneda)
    Public Property Fuente As String = ""
    Public Property Origen As String = ""
End Class

' Países y monedas de Centroamérica, sin inventar cotizaciones ausentes.
Public Class ServicioPaises
    Private Const ApiPrincipal As String = "https://restcountries.com/v3.1/region/americas?fields=name,cca2,currencies"
    Private Const ApiAlternativa As String = "https://raw.githubusercontent.com/mledoze/countries/master/dist/countries.json"
    Private Const ApiTasas As String = "https://open.er-api.com/v6/latest/USD"

    Private Shared ReadOnly Nombres As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
        {"BZ", "Belice"}, {"GT", "Guatemala"}, {"SV", "El Salvador"},
        {"HN", "Honduras"}, {"NI", "Nicaragua"}, {"CR", "Costa Rica"},
        {"PA", "Panamá"}}

    Public Async Function ConsultarAsync(ct As CancellationToken) As Task(Of ResultadoPaises)
        Dim resultado As New ResultadoPaises()
        Dim documento As DocumentoServicio = Nothing
        Dim usarAlternativa As Boolean = False
        Try
            documento = Await ServicioHttp.ObtenerAsync("Paises", ApiPrincipal,
                Function() New HttpRequestMessage(HttpMethod.Get, ApiPrincipal),
                Sub(texto) ValidarPaises(texto), ct)
            resultado.Fuente = "REST Countries"
        Catch ex As OperationCanceledException When ct.IsCancellationRequested
            Throw
        Catch ex As Exception
            ' VB.NET no permite Await dentro de Catch: resolver fuera del bloque.
            usarAlternativa = True
        End Try

        If usarAlternativa Then
            documento = Await ServicioHttp.ObtenerAsync("PaisesAlternativa", ApiAlternativa,
                Function() New HttpRequestMessage(HttpMethod.Get, ApiAlternativa),
                Sub(texto) ValidarPaises(texto), ct)
            resultado.Fuente = "Catálogo público mledoze/countries (alternativa a REST Countries)"
        End If

        resultado.Origen = documento.Descripcion
        resultado.Filas = ParsearPaises(documento.Texto)

        ' Una tasa sin referencia USD queda vacía; no crear valores ficticios.
        Try
            Dim tasasDocumento = Await ServicioHttp.ObtenerAsync("ExchangeRate-API", ApiTasas,
                Function() New HttpRequestMessage(HttpMethod.Get, ApiTasas),
                Sub(texto) ValidarTasas(texto), ct)
            Using json = JsonDocument.Parse(tasasDocumento.Texto)
                Dim tasas = json.RootElement.GetProperty("rates")
                For Each fila In resultado.Filas
                    Dim dato As JsonElement
                    If tasas.TryGetProperty(fila.CodigoMoneda, dato) AndAlso dato.ValueKind = JsonValueKind.Number Then
                        Dim valor As Decimal
                        If dato.TryGetDecimal(valor) AndAlso valor > 0D Then fila.UnidadesPorUSD = valor
                    End If
                Next
            End Using
        Catch ex As OperationCanceledException When ct.IsCancellationRequested
            Throw
        Catch ex As Exception
            resultado.Origen &= "; tasas no disponibles: " & ex.Message
        End Try
        Return resultado
    End Function

    Private Shared Sub ValidarTasas(texto As String)
        Using documento = JsonDocument.Parse(texto)
            Dim raiz = documento.RootElement
            If raiz.ValueKind <> JsonValueKind.Object OrElse
               raiz.GetProperty("result").GetString() <> "success" OrElse
               raiz.GetProperty("base_code").GetString() <> "USD" OrElse
               raiz.GetProperty("rates").ValueKind <> JsonValueKind.Object Then
                Throw New FormatException("Respuesta inválida de tasas con base USD.")
            End If
        End Using
    End Sub

    Private Shared Sub ValidarPaises(texto As String)
        Dim filas = ParsearPaises(texto)
        If filas.Select(Function(f) f.CodigoPais).Distinct().Count() <> 7 Then
            Throw New FormatException("El catálogo no contiene los siete países centroamericanos.")
        End If
    End Sub

    Public Shared Function ParsearPaises(texto As String) As List(Of PaisMoneda)
        Dim filas As New List(Of PaisMoneda)()
        Using documento = JsonDocument.Parse(texto)
            Dim raiz = documento.RootElement
            If raiz.ValueKind <> JsonValueKind.Array Then
                Throw New FormatException("Se esperaba una lista de países; el proveedor devolvió otro formato.")
            End If
            For Each pais In raiz.EnumerateArray()
                If pais.ValueKind <> JsonValueKind.Object Then Continue For
                Dim codigoEl As JsonElement
                If Not pais.TryGetProperty("cca2", codigoEl) OrElse codigoEl.ValueKind <> JsonValueKind.String Then Continue For
                Dim codigo = codigoEl.GetString()
                If String.IsNullOrEmpty(codigo) OrElse Not Nombres.ContainsKey(codigo) Then Continue For
                Dim monedas As JsonElement
                If Not pais.TryGetProperty("currencies", monedas) Then Continue For
                Dim codigos As New List(Of String)()
                If monedas.ValueKind = JsonValueKind.Object Then
                    For Each moneda In monedas.EnumerateObject()
                        codigos.Add(moneda.Name.ToUpperInvariant())
                    Next
                ElseIf monedas.ValueKind = JsonValueKind.Array Then
                    For Each moneda In monedas.EnumerateArray()
                        If moneda.ValueKind = JsonValueKind.String Then
                            codigos.Add(moneda.GetString().ToUpperInvariant())
                        End If
                    Next
                End If
                For Each moneda In codigos.Distinct()
                    filas.Add(New PaisMoneda With {
                        .Pais = Nombres(codigo), .CodigoPais = codigo,
                        .Moneda = NombreMoneda(moneda), .CodigoMoneda = moneda})
                Next
            Next
        End Using
        Return filas.OrderBy(Function(x) x.Pais).ThenBy(Function(x) x.CodigoMoneda).ToList()
    End Function

    Private Shared Function NombreMoneda(codigo As String) As String
        Select Case codigo
            Case "BZD" : Return "Dólar beliceño"
            Case "GTQ" : Return "Quetzal"
            Case "USD" : Return "Dólar estadounidense"
            Case "SVC" : Return "Colón salvadoreño"
            Case "HNL" : Return "Lempira"
            Case "NIO" : Return "Córdoba"
            Case "CRC" : Return "Colón costarricense"
            Case "PAB" : Return "Balboa"
            Case Else : Return codigo
        End Select
    End Function
End Class
