Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json

Public Class ServicioClima
    Public Async Function ConsultarAsync(ciudad As Ciudad, ct As CancellationToken) As Task(Of Pronostico)
        Repositorio.ValidarCiudad(ciudad)
        Dim lat = ciudad.Latitud.ToString(CultureInfo.InvariantCulture)
        Dim lon = ciudad.Longitud.ToString(CultureInfo.InvariantCulture)
        Dim url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,wind_speed_10m&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=America%2FManagua&forecast_days=5"
        Dim doc = Await ServicioHttp.ObtenerAsync("Open-Meteo", url, Function() New HttpRequestMessage(HttpMethod.Get, url), Sub(s) Interpretar(s), ct)
        Dim resultado = Interpretar(doc.Texto)
        resultado.Documento = doc
        Return resultado
    End Function

    Public Shared Function Interpretar(texto As String) As Pronostico
        Using doc = JsonDocument.Parse(texto)
            Dim raiz = doc.RootElement
            Dim actual As JsonElement
            Dim diario As JsonElement
            If Not raiz.TryGetProperty("current", actual) OrElse Not raiz.TryGetProperty("daily", diario) Then Throw New FormatException("La respuesta no contiene clima actual y diario.")
            Dim resultado As New Pronostico With {
                .Temperatura = Numero(actual.GetProperty("temperature_2m")),
                .Humedad = Numero(actual.GetProperty("relative_humidity_2m")),
                .Viento = Numero(actual.GetProperty("wind_speed_10m")),
                .Hora = actual.GetProperty("time").GetString()}
            Dim fechas = diario.GetProperty("time")
            Dim minimas = diario.GetProperty("temperature_2m_min")
            Dim maximas = diario.GetProperty("temperature_2m_max")
            Dim lluvia = diario.GetProperty("precipitation_probability_max")
            If fechas.GetArrayLength() <> 5 OrElse minimas.GetArrayLength() <> 5 OrElse maximas.GetArrayLength() <> 5 OrElse lluvia.GetArrayLength() <> 5 Then Throw New FormatException("Se esperaban cinco días de pronóstico.")
            For i = 0 To 4
                resultado.Dias.Add(New DiaPronostico With {.Fecha = fechas(i).GetString(), .Minima = Numero(minimas(i)), .Maxima = Numero(maximas(i)), .Lluvia = Numero(lluvia(i))})
            Next
            Return resultado
        End Using
    End Function

    Private Shared Function Numero(elemento As JsonElement) As Double?
        If elemento.ValueKind = JsonValueKind.Null Then Return Nothing
        Return elemento.GetDouble()
    End Function
End Class
