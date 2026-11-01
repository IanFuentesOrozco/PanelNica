Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

' Cliente HTTP compartido. Los formularios nunca crean HttpClient ni consultan SQL directamente.
Public NotInheritable Class ServicioHttp
    Private Sub New()
    End Sub

    Private Shared ReadOnly Cliente As New HttpClient With {.Timeout = TimeSpan.FromSeconds(18)}
    Private Shared ReadOnly Estados As New ConcurrentDictionary(Of String, EstadoServicio)(StringComparer.OrdinalIgnoreCase)

    Public Shared Function UltimosEstados() As List(Of EstadoServicio)
        Return Estados.Values.OrderBy(Function(x) x.Servicio).ToList()
    End Function

    Private Shared Function ClaveSHA256(texto As String) As String
        Dim bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto))
        Return Convert.ToHexString(bytes).ToLowerInvariant()
    End Function

    Private Shared Function Vigencia(servicio As String) As TimeSpan
        Select Case servicio
            Case "ExchangeRate-API" : Return TimeSpan.FromHours(1)
            Case "BCN", "BCE" : Return TimeSpan.FromHours(12)
            Case "Paises", "PaisesAlternativa" : Return TimeSpan.FromDays(1)
            Case "Open-Meteo" : Return TimeSpan.FromMinutes(20)
            Case Else : Return TimeSpan.FromMinutes(30)
        End Select
    End Function

    Private Shared Sub Registrar(servicio As String, codigo As String, tiempo As Long, origen As String, detalle As String)
        Estados(servicio) = New EstadoServicio With {
            .Servicio = servicio,
            .FechaUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            .CodigoHTTP = codigo,
            .Milisegundos = tiempo,
            .Origen = origen,
            .Detalle = detalle}
    End Sub

    Private Shared Function Documento(texto As String, fecha As DateTime, origen As String) As DocumentoServicio
        Return New DocumentoServicio With {
            .Texto = texto,
            .FechaUtc = fecha,
            .Origen = origen,
            .Descripcion = $"{origen}; respaldo de {fecha:yyyy-MM-dd HH:mm:ss} UTC"}
    End Function

    Public Shared Async Function ObtenerAsync(
        servicio As String,
        identificador As String,
        solicitud As Func(Of HttpRequestMessage),
        validar As Action(Of String),
        ct As CancellationToken) As Task(Of DocumentoServicio)

        ct.ThrowIfCancellationRequested()
        Dim clave = ClaveSHA256(identificador)
        Dim cache As EntradaCache = Nothing
        Dim repo As New Repositorio()
        Dim reloj = Stopwatch.StartNew()

        ' El respaldo depende de MariaDB. Una falla de base no impide consultar Internet.
        Try
            cache = Await repo.LeerCacheAsync(clave, ct)
        Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
            cache = Nothing
        End Try

        If ConexionBD.SoloRespaldo Then
            If cache Is Nothing Then
                Registrar(servicio, "Sin solicitud HTTP", reloj.ElapsedMilliseconds, "Sin respaldo", "No hay datos guardados en MariaDB.")
                Throw New InvalidOperationException("Modo sin conexión: no hay una respuesta guardada para esta consulta en MariaDB.")
            End If
            validar(cache.Texto)
            Registrar(servicio, "Sin solicitud HTTP", reloj.ElapsedMilliseconds, "MariaDB", "Datos locales; no se consultó Internet.")
            Return Documento(cache.Texto, cache.FechaUtc, "Respaldo MariaDB (modo sin conexión)")
        End If

        If cache IsNot Nothing AndAlso DateTime.UtcNow - cache.FechaUtc < Vigencia(servicio) Then
            Try
                validar(cache.Texto)
                Registrar(servicio, "Sin solicitud HTTP", reloj.ElapsedMilliseconds, "MariaDB", "Caché vigente.")
                Return Documento(cache.Texto, cache.FechaUtc, "Caché MariaDB")
            Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                ' Si la caché es inválida, ir a Internet y no reutilizarla.
                cache = Nothing
            End Try
        End If

        Dim codigo As String = "Sin respuesta"
        Try
            Using peticion = solicitud()
                Using respuesta = Await Cliente.SendAsync(peticion, HttpCompletionOption.ResponseHeadersRead, ct)
                    codigo = CInt(respuesta.StatusCode).ToString()
                    respuesta.EnsureSuccessStatusCode()
                    Dim contenido = Await respuesta.Content.ReadAsStringAsync(ct)
                    validar(contenido)   ' Un HTTP 200 con JSON/XML inválido no sobrescribe caché.
                    Dim ahora = DateTime.UtcNow
                    Try
                        Await repo.GuardarCacheAsync(clave, servicio, contenido, ahora, ct)
                    Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                        ' El dato en línea sigue disponible aunque no se pueda guardar.
                    End Try
                    Registrar(servicio, codigo, reloj.ElapsedMilliseconds, "Internet", "Respuesta válida.")
                    Return Documento(contenido, ahora, "Internet")
                End Using
            End Using
        Catch ex As OperationCanceledException When ct.IsCancellationRequested
            Registrar(servicio, codigo, reloj.ElapsedMilliseconds, "Cancelado", "Solicitud cancelada por el usuario.")
            Throw
        Catch ex As Exception
            ' Ante un fallo se reutiliza un respaldo antiguo, identificándolo expresamente.
            If cache IsNot Nothing Then
                Try
                    validar(cache.Texto)
                    Registrar(servicio, codigo, reloj.ElapsedMilliseconds, "MariaDB", "Falló Internet: " & ex.Message)
                    Return Documento(cache.Texto, cache.FechaUtc, "Respaldo antiguo (Internet falló)")
                Catch exCache As Exception
                    ' Caché inválida: se reporta el fallo original.
                End Try
            End If
            Registrar(servicio, codigo, reloj.ElapsedMilliseconds, "Error", ex.Message)
            Throw
        End Try
    End Function
End Class
