Public Class frmCiudades
    Inherits FormularioBase
    Private ReadOnly grid As DataGridView = Tabla()
    Private ReadOnly nombre As New TextBox With {.Width = 220, .MaxLength = 100, .PlaceholderText = "Nombre de la ciudad"}
    Private ReadOnly latitud As New NumericUpDown With {.Minimum = -90, .Maximum = 90, .DecimalPlaces = 6, .Width = 130}
    Private ReadOnly longitud As New NumericUpDown With {.Minimum = -180, .Maximum = 180, .DecimalPlaces = 6, .Width = 130}
    Private ReadOnly nuevo As Button = Boton("Nueva")
    Private ReadOnly guardar As Button = Boton("Guardar")
    Private ReadOnly eliminar As Button = Boton("Eliminar")
    Private ReadOnly recargar As Button = Boton("Actualizar lista")
    Private id As Integer
    Private ReadOnly repositorio As New Repositorio

    Public Sub New()
        MyBase.New("Administrar ciudades")
        AgregarFila(Barra(nuevo, guardar, eliminar, recargar, Cancelar))
        AgregarFila(Barra(New Label With {.Text = "Nombre", .AutoSize = True}, nombre, New Label With {.Text = "Latitud", .AutoSize = True}, latitud, New Label With {.Text = "Longitud", .AutoSize = True}, longitud))
        AgregarFila(grid, True)
        AgregarFila(Estado)
        AddHandler Shown, Async Sub(sender, e) Await RecargarAsync()
        AddHandler recargar.Click, Async Sub(sender, e) Await RecargarAsync()
        AddHandler nuevo.Click, Sub() Limpiar()
        AddHandler grid.SelectionChanged, Sub() Seleccionar()
        AddHandler guardar.Click, Async Sub(sender, e) Await GuardarAsync()
        AddHandler eliminar.Click, Async Sub(sender, e) Await EliminarAsync()
    End Sub

    Private Sub Limpiar()
        id = 0
        grid.ClearSelection()
        nombre.Clear()
        latitud.Value = 0
        longitud.Value = 0
        Estado.Text = "Nueva ciudad. Escribe nombre y coordenadas."
    End Sub

    Private Sub Seleccionar()
        If grid.SelectedRows.Count = 0 Then Return
        Dim ciudad = TryCast(grid.SelectedRows(0).DataBoundItem, Ciudad)
        If ciudad Is Nothing Then Return
        id = ciudad.Id
        nombre.Text = ciudad.Nombre
        latitud.Value = CDec(ciudad.Latitud)
        longitud.Value = CDec(ciudad.Longitud)
    End Sub

    Private Async Function RecargarAsync() As Task
        Await EjecutarAsync(Async Function(ct)
            grid.DataSource = Await repositorio.ListarCiudadesAsync(ct)
            Estado.Text = "Ciudades guardadas en MariaDB."
        End Function, nuevo, guardar, eliminar, recargar, nombre, latitud, longitud, grid)
    End Function

    Private Async Function GuardarAsync() As Task
        Dim ciudad As New Ciudad With {.Id = id, .Nombre = nombre.Text, .Latitud = CDbl(latitud.Value), .Longitud = CDbl(longitud.Value)}
        Await EjecutarAsync(Async Function(ct)
            Await repositorio.GuardarCiudadAsync(ciudad, ct)
            grid.DataSource = Await repositorio.ListarCiudadesAsync(ct)
            Estado.Text = "Ciudad guardada. En Clima, pulsa Recargar ciudades."
        End Function, nuevo, guardar, eliminar, recargar, nombre, latitud, longitud, grid)
    End Function

    Private Async Function EliminarAsync() As Task
        If id = 0 Then Return
        If MessageBox.Show(Me, "¿Eliminar la ciudad seleccionada?", "Eliminar ciudad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Dim elegido = id
        Await EjecutarAsync(Async Function(ct)
            Await repositorio.EliminarCiudadAsync(elegido, ct)
            grid.DataSource = Await repositorio.ListarCiudadesAsync(ct)
            Limpiar()
            Estado.Text = "Ciudad eliminada."
        End Function, nuevo, guardar, eliminar, recargar, nombre, latitud, longitud, grid)
    End Function
End Class
