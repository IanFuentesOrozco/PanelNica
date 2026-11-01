Imports System
Imports System.Windows.Forms

Public Class frmEstadoServicios
    Inherits FormularioBase

    Private ReadOnly refrescar As Button = Boton("Actualizar estado")
    Private ReadOnly grillaEstados As DataGridView = Tabla()

    Public Sub New()
        MyBase.New("Estado de servicios web")
        AgregarFila(Barra(refrescar))
        AgregarFila(New Label With {.AutoSize = True,
            .Text = "Última operación de cada servicio durante la sesión. Actualizar no realiza nuevas solicitudes HTTP."})
        AgregarFila(grillaEstados, True)
        AgregarFila(Estado)
        AddHandler refrescar.Click, Sub() Actualizar()
        AddHandler Shown, Sub() Actualizar()
    End Sub

    Private Sub Actualizar()
        Dim estados = ServicioHttp.UltimosEstados()
        grillaEstados.DataSource = Nothing
        grillaEstados.DataSource = estados
        Estado.Text = If(estados.Count = 0, "Todavía no se han realizado consultas.",
            $"Último estado disponible para {estados.Count} servicios. Sin solicitudes nuevas.")
    End Sub
End Class
