Public Class frmConfiguracion
    Inherits FormularioBase
    Private ReadOnly servidor As New TextBox With {.Width = 240}
    Private ReadOnly puerto As New NumericUpDown With {.Minimum = 1, .Maximum = 65535, .Width = 120}
    Private ReadOnly baseDatos As New TextBox With {.Width = 240}
    Private ReadOnly usuario As New TextBox With {.Width = 240}
    Private ReadOnly contrasena As New TextBox With {.Width = 240, .UseSystemPasswordChar = True}
    Private ReadOnly probar As Button = Boton("Probar conexión")
    Private ReadOnly preparar As Button = Boton("Crear tablas y ciudades iniciales")
    Private ReadOnly guardar As Button = Boton("Guardar configuración")

    Public Sub New()
        MyBase.New("Configurar MariaDB")
        servidor.Text = ConexionBD.Configuracion.Servidor
        puerto.Value = ConexionBD.Configuracion.Puerto
        baseDatos.Text = ConexionBD.Configuracion.BaseDatos
        usuario.Text = ConexionBD.Configuracion.Usuario
        contrasena.Text = ConexionBD.Configuracion.Contrasena
        AgregarFila(Barra(New Label With {.Text = "Servidor", .Width = 150}, servidor))
        AgregarFila(Barra(New Label With {.Text = "Puerto", .Width = 150}, puerto))
        AgregarFila(Barra(New Label With {.Text = "Base de datos", .Width = 150}, baseDatos))
        AgregarFila(Barra(New Label With {.Text = "Usuario", .Width = 150}, usuario))
        AgregarFila(Barra(New Label With {.Text = "Contraseña", .Width = 150}, contrasena))
        AgregarFila(Barra(guardar, probar, preparar, Cancelar))
        AgregarFila(Estado, True)
        AddHandler guardar.Click, Sub() GuardarOpciones()
        AddHandler probar.Click, Async Sub(sender, e) Await ProbarAsync(False)
        AddHandler preparar.Click, Async Sub(sender, e) Await ProbarAsync(True)
    End Sub

    Private Sub Aplicar()
        If String.IsNullOrWhiteSpace(servidor.Text) OrElse String.IsNullOrWhiteSpace(baseDatos.Text) OrElse String.IsNullOrWhiteSpace(usuario.Text) Then Throw New ArgumentException("Completa servidor, base de datos y usuario.")
        ConexionBD.Configuracion = New ConfiguracionBD With {.Servidor = servidor.Text.Trim(), .Puerto = CUInt(puerto.Value), .BaseDatos = baseDatos.Text.Trim(), .Usuario = usuario.Text.Trim(), .Contrasena = contrasena.Text}
    End Sub

    Private Sub GuardarOpciones()
        Try
            Aplicar()
            ConexionBD.GuardarConfiguracion()
            Estado.Text = "Configuración guardada sin contraseña."
        Catch ex As Exception
            Estado.Text = ex.Message
        End Try
    End Sub

    Private Async Function ProbarAsync(crearTablas As Boolean) As Task
        Await EjecutarAsync(Async Function(ct)
            Aplicar()
            If crearTablas Then
                Await New Repositorio().PrepararAsync(ct)
                Estado.Text = "Tablas preparadas. Las ciudades iniciales se agregan únicamente si la tabla está vacía."
            Else
                Using cn = Await ConexionBD.AbrirAsync(ct)
                    Estado.Text = "Conexión correcta con MariaDB."
                End Using
            End If
        End Function, guardar, probar, preparar, servidor, puerto, baseDatos, usuario, contrasena)
    End Function
End Class
