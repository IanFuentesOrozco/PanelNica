Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmPrincipal
    Inherits Form

    Private ReadOnly modoRespaldo As New CheckBox With {
        .Text = "Usar solamente el respaldo de MariaDB",
        .AutoSize = True, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}
    Private ReadOnly estado As New Label With {.AutoSize = True,
        .Text = "Seleccioná un módulo para comenzar."}

    Public Sub New()
        Text = "PanelNica · Tablero de servicios web"
        StartPosition = FormStartPosition.CenterScreen
        MinimumSize = New Size(850, 560)
        Size = New Size(1060, 700)
        Font = New Font("Segoe UI", 10)
        BackColor = Color.White

        Dim cabecera As New Panel With {.Dock = DockStyle.Top, .Height = 115,
            .BackColor = Color.FromArgb(10, 58, 122)}
        Dim titulo As New Label With {.Text = "PanelNica",
            .Font = New Font("Segoe UI", 27, FontStyle.Bold), .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(30, 13)}
        Dim subtitulo As New Label With {
            .Text = "Clima, tipo de cambio, ciudades y Centroamérica",
            .Font = New Font("Segoe UI", 12), .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(33, 70)}
        cabecera.Controls.AddRange(New Control() {titulo, subtitulo})

        Dim cuerpo As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill, .Padding = New Padding(32),
            .FlowDirection = FlowDirection.TopDown, .WrapContents = False,
            .AutoScroll = True, .BackColor = Color.White}
        modoRespaldo.Checked = ConexionBD.SoloRespaldo
        AddHandler modoRespaldo.CheckedChanged,
            Sub() ConexionBD.SoloRespaldo = modoRespaldo.Checked
        cuerpo.Controls.Add(modoRespaldo)
        cuerpo.Controls.Add(New Label With {.Height = 8})

        Dim botones As New TableLayoutPanel With {
            .ColumnCount = 2, .RowCount = 3,
            .Size = New Size(790, 330),
            .Margin = New Padding(0, 6, 0, 18)}
        botones.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        botones.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        For i = 1 To 3
            botones.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / 3.0F))
        Next
        AgregarBoton(botones, "🌤  Clima y pronóstico", 0, 0, Sub() Abrir(New frmClima()))
        AgregarBoton(botones, "🏙  Administrar ciudades", 1, 0, Sub() Abrir(New frmCiudades()))
        AgregarBoton(botones, "💱  Tipo de cambio", 0, 1, Sub() Abrir(New frmTipoCambio()))
        AgregarBoton(botones, "🌎  Centroamérica", 1, 1, Sub() Abrir(New frmCentroamerica()))
        AgregarBoton(botones, "📡  Estado de servicios", 0, 2, Sub() Abrir(New frmEstadoServicios()))
        AgregarBoton(botones, "⚙  Configurar MariaDB", 1, 2, Sub() Abrir(New frmConfiguracion()))
        cuerpo.Controls.Add(botones)
        cuerpo.Controls.Add(estado)
        Controls.Add(cuerpo)
        Controls.Add(cabecera)
    End Sub

    Private Shared Sub AgregarBoton(tabla As TableLayoutPanel, texto As String,
        columna As Integer, fila As Integer, accion As Action)
        Dim boton As New Button With {
            .Text = texto, .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = Color.FromArgb(232, 240, 251),
            .ForeColor = Color.FromArgb(10, 58, 122),
            .FlatStyle = FlatStyle.Flat,
            .Margin = New Padding(5)}
        AddHandler boton.Click, Sub(sender, e) accion()
        tabla.Controls.Add(boton, columna, fila)
    End Sub

    Private Sub Abrir(ventana As Form)
        Using ventana
            ventana.ShowDialog(Me)
        End Using
        ' Si el modo se cambia desde Clima, volver a sincronizar la casilla.
        modoRespaldo.Checked = ConexionBD.SoloRespaldo
    End Sub
End Class
