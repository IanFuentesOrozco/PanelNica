Imports MySqlConnector

Public Class Repositorio
    Public Shared Function CiudadesIniciales() As List(Of Ciudad)
        Return New List(Of Ciudad) From {
            New Ciudad With {.Id = 1, .Nombre = "Managua", .Latitud = 12.1364, .Longitud = -86.2514},
            New Ciudad With {.Id = 2, .Nombre = "León", .Latitud = 12.4379, .Longitud = -86.878},
            New Ciudad With {.Id = 3, .Nombre = "Estelí", .Latitud = 13.0919, .Longitud = -86.3538},
            New Ciudad With {.Id = 4, .Nombre = "Matagalpa", .Latitud = 12.9256, .Longitud = -85.9175},
            New Ciudad With {.Id = 5, .Nombre = "Bluefields", .Latitud = 12.0137, .Longitud = -83.7635}}
    End Function

    Public Async Function PrepararAsync(ct As CancellationToken) As Task
        Dim sentencias = {
            "CREATE TABLE IF NOT EXISTS ciudades (id INT AUTO_INCREMENT PRIMARY KEY,nombre VARCHAR(100) NOT NULL UNIQUE,latitud DOUBLE NOT NULL,longitud DOUBLE NOT NULL)",
            "CREATE TABLE IF NOT EXISTS cache_respuestas (clave CHAR(64) PRIMARY KEY,servicio VARCHAR(100) NOT NULL,respuesta LONGTEXT NOT NULL,fecha_utc DATETIME(6) NOT NULL)",
            "CREATE TABLE IF NOT EXISTS tipo_cambio (id BIGINT AUTO_INCREMENT PRIMARY KEY,fecha_utc DATETIME(6) NOT NULL,fuente VARCHAR(50) NOT NULL,tasa DECIMAL(18,6) NOT NULL,fecha_fuente VARCHAR(100) NOT NULL)"}
        Using cn = Await ConexionBD.AbrirAsync(ct)
            For Each sql In sentencias
                Using cmd As New MySqlCommand(sql, cn)
                    Await cmd.ExecuteNonQueryAsync(ct)
                End Using
            Next
            Using cmd As New MySqlCommand("SELECT COUNT(*) FROM ciudades", cn)
                If Convert.ToInt32(Await cmd.ExecuteScalarAsync(ct)) = 0 Then
                    For Each ciudad In CiudadesIniciales()
                        Using insertar As New MySqlCommand("INSERT INTO ciudades(nombre,latitud,longitud) VALUES(@n,@a,@o)", cn)
                            insertar.Parameters.AddWithValue("@n", ciudad.Nombre)
                            insertar.Parameters.AddWithValue("@a", ciudad.Latitud)
                            insertar.Parameters.AddWithValue("@o", ciudad.Longitud)
                            Await insertar.ExecuteNonQueryAsync(ct)
                        End Using
                    Next
                End If
            End Using
        End Using
    End Function

    Public Async Function ListarCiudadesAsync(ct As CancellationToken) As Task(Of List(Of Ciudad))
        Dim resultado As New List(Of Ciudad)
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("SELECT id,nombre,latitud,longitud FROM ciudades ORDER BY nombre", cn)
                Using lector = Await cmd.ExecuteReaderAsync(ct)
                    While Await lector.ReadAsync(ct)
                        resultado.Add(New Ciudad With {.Id = lector.GetInt32(0), .Nombre = lector.GetString(1), .Latitud = lector.GetDouble(2), .Longitud = lector.GetDouble(3)})
                    End While
                End Using
            End Using
        End Using
        Return resultado
    End Function

    Public Shared Sub ValidarCiudad(c As Ciudad)
        If String.IsNullOrWhiteSpace(c.Nombre) OrElse c.Nombre.Trim().Length > 100 Then Throw New ArgumentException("El nombre debe tener entre 1 y 100 caracteres.")
        If Not Double.IsFinite(c.Latitud) OrElse Not Double.IsFinite(c.Longitud) OrElse c.Latitud < -90 OrElse c.Latitud > 90 OrElse c.Longitud < -180 OrElse c.Longitud > 180 Then Throw New ArgumentException("Coordenadas fuera de rango.")
    End Sub

    Public Async Function GuardarCiudadAsync(c As Ciudad, ct As CancellationToken) As Task
        ValidarCiudad(c)
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Dim sql = If(c.Id = 0, "INSERT INTO ciudades(nombre,latitud,longitud) VALUES(@n,@a,@o)", "UPDATE ciudades SET nombre=@n,latitud=@a,longitud=@o WHERE id=@id")
            Using cmd As New MySqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@n", c.Nombre.Trim())
                cmd.Parameters.AddWithValue("@a", c.Latitud)
                cmd.Parameters.AddWithValue("@o", c.Longitud)
                cmd.Parameters.AddWithValue("@id", c.Id)
                Await cmd.ExecuteNonQueryAsync(ct)
            End Using
        End Using
    End Function

    Public Async Function EliminarCiudadAsync(id As Integer, ct As CancellationToken) As Task
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("DELETE FROM ciudades WHERE id=@id", cn)
                cmd.Parameters.AddWithValue("@id", id)
                Await cmd.ExecuteNonQueryAsync(ct)
            End Using
        End Using
    End Function

    Public Async Function LeerCacheAsync(clave As String, ct As CancellationToken) As Task(Of EntradaCache)
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("SELECT respuesta,fecha_utc FROM cache_respuestas WHERE clave=@k", cn)
                cmd.Parameters.AddWithValue("@k", clave)
                Using lector = Await cmd.ExecuteReaderAsync(ct)
                    If Await lector.ReadAsync(ct) Then Return New EntradaCache With {.Texto = lector.GetString(0), .FechaUtc = DateTime.SpecifyKind(lector.GetDateTime(1), DateTimeKind.Utc)}
                End Using
            End Using
        End Using
        Return Nothing
    End Function

    Public Async Function GuardarCacheAsync(clave As String, servicio As String, texto As String, fecha As DateTime, ct As CancellationToken) As Task
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("INSERT INTO cache_respuestas(clave,servicio,respuesta,fecha_utc) VALUES(@k,@s,@r,@f) ON DUPLICATE KEY UPDATE respuesta=@r,fecha_utc=@f", cn)
                cmd.Parameters.AddWithValue("@k", clave)
                cmd.Parameters.AddWithValue("@s", servicio)
                cmd.Parameters.AddWithValue("@r", texto)
                cmd.Parameters.AddWithValue("@f", fecha)
                Await cmd.ExecuteNonQueryAsync(ct)
            End Using
        End Using
    End Function

    Public Async Function GuardarTasaAsync(fuente As String, tasa As Decimal, fechaFuente As String, fechaConsulta As DateTime, ct As CancellationToken) As Task
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("INSERT INTO tipo_cambio(fecha_utc,fuente,tasa,fecha_fuente) VALUES(@f,@o,@t,@d)", cn)
                cmd.Parameters.AddWithValue("@f", fechaConsulta)
                cmd.Parameters.AddWithValue("@o", fuente)
                cmd.Parameters.AddWithValue("@t", tasa)
                cmd.Parameters.AddWithValue("@d", fechaFuente)
                Await cmd.ExecuteNonQueryAsync(ct)
            End Using
        End Using
    End Function

    Public Async Function HistorialAsync(ct As CancellationToken) As Task(Of Data.DataTable)
        Dim tabla As New Data.DataTable
        Using cn = Await ConexionBD.AbrirAsync(ct)
            Using cmd As New MySqlCommand("SELECT fecha_utc AS FechaUTC,fuente AS Fuente,tasa AS NIOporUSD,fecha_fuente AS FechaFuente FROM tipo_cambio ORDER BY id DESC LIMIT 200", cn)
                Using lector = Await cmd.ExecuteReaderAsync(ct)
                    ' Llenado manual: no usar DataAdapter.Fill síncrono en la interfaz.
                    For i = 0 To lector.FieldCount - 1
                        tabla.Columns.Add(lector.GetName(i), lector.GetFieldType(i))
                    Next
                    While Await lector.ReadAsync(ct)
                        Dim valores(lector.FieldCount - 1) As Object
                        lector.GetValues(valores)
                        tabla.Rows.Add(valores)
                    End While
                End Using
            End Using
        End Using
        Return tabla
    End Function
End Class
