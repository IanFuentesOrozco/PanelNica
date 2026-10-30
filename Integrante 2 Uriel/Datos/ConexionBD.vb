Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports MySqlConnector

Public Class ConfiguracionBD
    Public Property Servidor As String = "localhost"
    Public Property Puerto As UInteger = 3306
    Public Property BaseDatos As String = "panelnica"
    Public Property Usuario As String = "root"
    <JsonIgnore>
    Public Property Contrasena As String = ""
End Class

Public Module ConexionBD
    Public Property Configuracion As ConfiguracionBD = LeerConfiguracion()
    Public Property SoloRespaldo As Boolean
    Private ReadOnly Property Ruta As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PanelNica", "conexion.json")
        End Get
    End Property

    Private Function LeerConfiguracion() As ConfiguracionBD
        Try
            If File.Exists(Ruta) Then
                Dim resultado = JsonSerializer.Deserialize(Of ConfiguracionBD)(File.ReadAllText(Ruta))
                If resultado IsNot Nothing Then Return resultado
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is JsonException OrElse TypeOf ex Is UnauthorizedAccessException
            ' La ventana de configuración permite recuperar un archivo dañado.
        End Try
        Return New ConfiguracionBD()
    End Function

    Public Sub GuardarConfiguracion()
        Directory.CreateDirectory(Path.GetDirectoryName(Ruta))
        File.WriteAllText(Ruta, JsonSerializer.Serialize(Configuracion, New JsonSerializerOptions With {.WriteIndented = True}))
        ' La contraseña permanece solo en memoria; nunca se escribe al archivo.
    End Sub

    Public Async Function AbrirAsync(ct As CancellationToken) As Task(Of MySqlConnection)
        Dim cadena = Environment.GetEnvironmentVariable("PANELNICA_DB")
        If String.IsNullOrWhiteSpace(cadena) Then
            cadena = New MySqlConnectionStringBuilder With {
                .Server = Configuracion.Servidor, .Port = Configuracion.Puerto,
                .Database = Configuracion.BaseDatos, .UserID = Configuracion.Usuario,
                .Password = Configuracion.Contrasena, .ConnectionTimeout = 4,
                .DefaultCommandTimeout = 10, .DateTimeKind = MySqlDateTimeKind.Utc
            }.ConnectionString
        End If
        Dim conexion As New MySqlConnection(cadena)
        Try
            Await conexion.OpenAsync(ct)
            Return conexion
        Catch
            conexion.Dispose()
            Throw
        End Try
    End Function
End Module
