Public Class frmClima
    Inherits FormularioBase
    Private ReadOnly ciudades As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 200}
    Private ReadOnly cargar As Button = Boton("Recargar ciudades")
    Private ReadOnly consultar As Button = Boton("Consultar clima")
    Private ReadOnly consultarEnLinea As Button = Boton("Activar Internet y consultar")
    Private ReadOnly modo As New Label With {.AutoSize = True, .MaximumSize = New Size(950, 0)}
    Private ReadOnly actual As New Label With {.AutoSize = True, .Font = New Font("Segoe UI", 14), .Padding = New Padding(0, 10, 0, 10)}
    Private ReadOnly aviso As New Label With {.AutoSize = True, .ForeColor = Color.Firebrick, .Padding = New Padding(0, 8, 0, 8)}
    Private ReadOnly grid As DataGridView = Tabla()
    Private ReadOnly listarCiudades As Func(Of CancellationToken, Task(Of List(Of Ciudad)))
    Private ReadOnly obtenerClima As Func(Of Ciudad, CancellationToken, Task(Of Pronostico))

    Public Sub New()
        Me.New(Function(ct) New Repositorio().ListarCiudadesAsync(ct), Function(ciudad, ct) New ServicioClima().ConsultarAsync(ciudad, ct))
    End Sub

    Friend Sub New(listar As Func(Of CancellationToken, Task(Of List(Of Ciudad))), obtener As Func(Of Ciudad, CancellationToken, Task(Of Pronostico)))
        MyBase.New("Clima de Nicaragua")
        listarCiudades = listar
        obtenerClima = obtener
        AgregarFila(Barra(New Label With {.Text = "Ciudad:", .AutoSize = True, .Padding = New Padding(0, 8, 0, 0)}, ciudades, cargar, consultar, Cancelar))
        AgregarFila(Barra(modo, consultarEnLinea))
        MostrarModo()
        AgregarFila(actual)
        AgregarFila(aviso)
        AgregarFila(grid, True)
        AgregarFila(Estado)
        AgregarFila(New Label With {.AutoSize = True, .Text = "Fuente: Open-Meteo · Horas del lugar (America/Managua). El respaldo conserva su fecha original."})
        grid.DataSource = New List(Of DiaPronostico)()
        actual.Text = "Selecciona una ciudad y pulsa Consultar clima para ver el pronóstico."
        AddHandler Shown, Async Sub(sender, e) Await CargarCiudadesAsync()
        AddHandler cargar.Click, Async Sub(sender, e) Await CargarCiudadesAsync()
        AddHandler consultar.Click, Async Sub(sender, e) Await ConsultarAsync()
        AddHandler consultarEnLinea.Click, Async Sub(sender, e)
                                                  ConexionBD.SoloRespaldo = False
                                                  MostrarModo()
                                                  Await ConsultarAsync()
                                              End Sub
    End Sub

    Private Sub MostrarModo()
        modo.Text = If(ConexionBD.SoloRespaldo,
            "Modo sin conexión: solo se muestran datos guardados; no se consulta Internet.",
            "Modo en línea: se consulta Open-Meteo y se intenta guardar un respaldo en MariaDB.")
        modo.ForeColor = If(ConexionBD.SoloRespaldo, Color.Firebrick, Color.FromArgb(45, 62, 84))
        consultarEnLinea.Visible = ConexionBD.SoloRespaldo
    End Sub

    Private Async Function CargarCiudadesAsync() As Task
        Await EjecutarAsync(Async Function(ct)
            Try
                Dim lista = Await listarCiudades(ct)
                If IsDisposed Then Return
                MostrarCiudades(lista)
            Catch ex As MySqlConnector.MySqlException
                If IsDisposed Then Return
                MostrarCiudades(New List(Of Ciudad)())
                Estado.Text = "MariaDB no disponible. Se usan cinco ciudades iniciales; no son registros leídos de la base de datos."
            End Try

        End Function, cargar, consultar, ciudades, consultarEnLinea)
    End Function

    Friend Sub MostrarCiudades(lista As List(Of Ciudad))
        Dim seleccion = TryCast(ciudades.SelectedItem, Ciudad)
        Dim vacia = lista.Count = 0
        If vacia Then lista = Repositorio.CiudadesIniciales()
        ciudades.DataSource = Nothing
        ciudades.DisplayMember = NameOf(Ciudad.Nombre)
        ciudades.ValueMember = NameOf(Ciudad.Id)
        ciudades.DataSource = lista
        ciudades.SelectedIndex = 0
        If seleccion IsNot Nothing AndAlso lista.Any(Function(c) c.Id = seleccion.Id) Then ciudades.SelectedValue = seleccion.Id
        Estado.Text = If(vacia,
            "MariaDB no contiene ciudades. Se usan cinco ciudades iniciales para consultar el clima; puedes guardar ciudades desde Administrar ciudades.",
            $"{lista.Count} ciudades cargadas desde MariaDB. Selecciona una y pulsa Consultar clima.")
    End Sub

    Private Async Function ConsultarAsync() As Task
        Dim ciudad = TryCast(ciudades.SelectedItem, Ciudad)
        If ciudad Is Nothing Then
            Estado.Text = "Agrega o selecciona una ciudad."
            Return
        End If
        Await EjecutarAsync(Async Function(ct)
            grid.DataSource = New List(Of DiaPronostico)()
            actual.Text = ""
            aviso.Text = ""
            Dim clima = Await obtenerClima(ciudad, ct)
            If IsDisposed Then Return
            actual.Text = $"{ciudad.Nombre} · {clima.Hora} · {Medida(clima.Temperatura)} °C · Humedad {Medida(clima.Humedad)} % · Viento {Medida(clima.Viento)} km/h"
            grid.DataSource = clima.Dias
            aviso.Text = If(clima.Dias.Any(Function(d) d.Lluvia.HasValue AndAlso d.Lluvia.Value > 60), "Aviso: hay días con probabilidad de lluvia superior al 60 %.", "Sin avisos de lluvia superior al 60 % en los datos disponibles.")
            Estado.Text = clima.Documento.Descripcion
        End Function, cargar, consultar, ciudades, consultarEnLinea)
    End Function

    Private Shared Function Medida(valor As Double?) As String
        Return If(valor.HasValue, valor.Value.ToString("0.0"), "Sin dato")
    End Function
End Class

