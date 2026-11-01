Imports System
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class frmCentroamerica
    Inherits FormularioBase

    Private ReadOnly consultar As Button = Boton("Consultar países y monedas")
    Private ReadOnly fuente As New Label With {.AutoSize = True, .Text = "Sin consultar."}
    Private ReadOnly aviso As New Label With {.AutoSize = True,
        .Text = "Las tasas representan unidades de moneda por 1 USD. Un valor vacío indica que no se dispone de tasa."}
    Private ReadOnly grillaCentroamerica As DataGridView = Tabla()

    Public Sub New()
        MyBase.New("Centroamérica · países, monedas y tasas")
        AgregarFila(Barra(consultar, Cancelar))
        AgregarFila(fuente)
        AgregarFila(aviso)
        AgregarFila(grillaCentroamerica, True)
        AgregarFila(Estado)
        AddHandler consultar.Click, Async Sub(sender, e) Await ConsultarAsync()
    End Sub

    Private Async Function ConsultarAsync() As Task
        Await EjecutarAsync(Async Function(ct)
            Dim datos = Await New ServicioPaises().ConsultarAsync(ct)
            If IsDisposed Then Return
            grillaCentroamerica.DataSource = datos.Filas
            fuente.Text = "Fuente de países: " & datos.Fuente
            Estado.Text = $"{datos.Filas.Count} registros para los siete países. {datos.Origen}"
        End Function, consultar)
    End Function
End Class
