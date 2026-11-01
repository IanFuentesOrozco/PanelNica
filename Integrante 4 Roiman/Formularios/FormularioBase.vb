Imports System
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Interfaz uniforme y control de cancelación para los módulos de Ian, Uriel y Albert.
Public Class FormularioBase
    Inherits Form

    Private ReadOnly contenedor As New TableLayoutPanel With {
        .Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 0,
        .AutoScroll = True, .Padding = New Padding(20), .BackColor = Color.White}
    Private ReadOnly regresar As Button = Boton("Volver al menú")
    Protected ReadOnly Cancelar As Button = Boton("Cancelar consulta")
    Protected ReadOnly Estado As New Label With {
        .AutoSize = True, .MaximumSize = New Size(960, 0),
        .ForeColor = Color.FromArgb(40, 65, 95), .Text = "Listo."}
    Private ctsActual As CancellationTokenSource
    Private trabajando As Boolean

    Protected Sub New(titulo As String)
        Me.Text = "PanelNica · " & titulo
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(780, 540)
        Me.Size = New Size(1060, 740)
        Me.BackColor = Color.White
        Me.Font = New Font("Segoe UI", 10)
        Dim cabecera As New Panel With {.Dock = DockStyle.Top, .Height = 62, .BackColor = Color.FromArgb(10, 58, 122)}
        Dim tituloLabel As New Label With {
            .Text = titulo, .ForeColor = Color.White, .Font = New Font("Segoe UI", 17, FontStyle.Bold),
            .AutoSize = True, .Location = New Point(20, 16)}
        regresar.Anchor = AnchorStyles.Right Or AnchorStyles.Top
        regresar.Location = New Point(830, 14)
        regresar.Size = New Size(170, 34)
        AddHandler cabecera.Resize, Sub() regresar.Left = Math.Max(20, cabecera.ClientSize.Width - regresar.Width - 16)
        AddHandler regresar.Click, Sub() Me.Close()
        AddHandler Cancelar.Click, Sub() If ctsActual IsNot Nothing Then ctsActual.Cancel()
        Cancelar.Enabled = False
        cabecera.Controls.Add(tituloLabel)
        cabecera.Controls.Add(regresar)
        Controls.Add(contenedor)
        Controls.Add(cabecera)
    End Sub

    Protected Shared Function Boton(texto As String) As Button
        Return New Button With {
            .Text = texto, .AutoSize = True, .Height = 36,
            .BackColor = Color.FromArgb(232, 240, 251),
            .ForeColor = Color.FromArgb(10, 58, 122),
            .FlatStyle = FlatStyle.Flat,
            .Padding = New Padding(8, 2, 8, 2),
            .Margin = New Padding(0, 3, 7, 3)}
    End Function

    Protected Shared Function Tabla() As DataGridView
        Return New DataGridView With {
            .Dock = DockStyle.Fill, .ReadOnly = True,
            .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False, .BackgroundColor = Color.White,
            .RowHeadersVisible = False, .MinimumSize = New Size(0, 180)}
    End Function

    Protected Shared Function Barra(ParamArray elementos As Control()) As FlowLayoutPanel
        Dim fila As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill, .AutoSize = True, .WrapContents = True,
            .FlowDirection = FlowDirection.LeftToRight,
            .Margin = New Padding(0, 5, 0, 8)}
        fila.Controls.AddRange(elementos)
        Return fila
    End Function

    Protected Sub AgregarFila(elemento As Control, Optional expandir As Boolean = False)
        Dim i = contenedor.RowCount
        contenedor.RowCount += 1
        contenedor.RowStyles.Add(New RowStyle(If(expandir, SizeType.Percent, SizeType.AutoSize), If(expandir, 100.0F, 0.0F)))
        elemento.Dock = DockStyle.Fill
        contenedor.Controls.Add(elemento, 0, i)
        If expandir Then elemento.MinimumSize = New Size(0, 160)
    End Sub

    Protected Async Function EjecutarAsync(operacion As Func(Of CancellationToken, Task), ParamArray deshabilitar As Control()) As Task
        If trabajando OrElse IsDisposed Then Return
        trabajando = True
        Dim localCts As New CancellationTokenSource()
        ctsActual = localCts
        For Each elemento In deshabilitar
            If elemento IsNot Nothing AndAlso Not elemento.IsDisposed Then elemento.Enabled = False
        Next
        If Not Cancelar.IsDisposed Then Cancelar.Enabled = True
        Try
            Await operacion(localCts.Token)
        Catch ex As OperationCanceledException When localCts.IsCancellationRequested
            If Not IsDisposed Then Estado.Text = "Operación cancelada."
        Catch ex As Exception
            If Not IsDisposed Then Estado.Text = "Error: " & ex.Message
        Finally
            trabajando = False
            If Not IsDisposed Then
                For Each elemento In deshabilitar
                    If elemento IsNot Nothing AndAlso Not elemento.IsDisposed Then elemento.Enabled = True
                Next
                Cancelar.Enabled = False
            End If
            If Object.ReferenceEquals(ctsActual, localCts) Then ctsActual = Nothing
            localCts.Dispose()
        End Try
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If ctsActual IsNot Nothing Then ctsActual.Cancel()
        MyBase.OnFormClosing(e)
    End Sub
End Class
