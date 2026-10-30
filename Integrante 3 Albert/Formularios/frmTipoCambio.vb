Imports System.Globalization

Public Class frmTipoCambio
    Inherits FormularioBase

    Private ReadOnly consultar As Button = Boton("Consultar tasas")
    Private ReadOnly convertir As Button = Boton("Convertir")
    Private ReadOnly monto As New NumericUpDown With {
        .DecimalPlaces = 2, .Maximum = 1000000000D, .Width = 160, .ThousandsSeparator = True}
    Private ReadOnly sentido As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 150}
    Private ReadOnly mercado As New Label With {.AutoSize = True}
    Private ReadOnly oficial As New Label With {.AutoSize = True}
    Private ReadOnly diferencia As New Label With {.AutoSize = True}
    Private ReadOnly referencia As New Label With {.AutoSize = True}
    Private ReadOnly resultado As New Label With {
        .AutoSize = True, .Font = New Font("Segoe UI", 12, FontStyle.Bold)}
    Private ReadOnly historial As DataGridView = Tabla()
    Private tasaMercado As DatoTipoCambio
    Private tasaBCN As DatoTipoCambio

    Public Sub New()
        MyBase.New("Tipo de cambio de Nicaragua")
        sentido.Items.AddRange(New Object() {"USD a NIO", "NIO a USD"})
        sentido.SelectedIndex = 0
        AgregarFila(Barra(consultar, Cancelar))
        AgregarFila(mercado)
        AgregarFila(oficial)
        AgregarFila(diferencia)
        AgregarFila(referencia)
        AgregarFila(Barra(New Label With {.Text = "Monto:", .AutoSize = True},
                         monto, sentido, convertir))
        AgregarFila(resultado)
        AgregarFila(New Label With {.Text = "Historial guardado en MariaDB:", .AutoSize = True})
        AgregarFila(historial, True)
        AgregarFila(Estado)
        AgregarFila(New Label With {
            .Text = "ExchangeRate-API: referencia agregada; BCN: tasa oficial. BCE: EUR/USD, no NIO/USD.",
            .AutoSize = True})
        convertir.Enabled = False
        mercado.Text = "ExchangeRate-API: aún sin consultar"
        oficial.Text = "BCN: aún sin consultar"
        diferencia.Text = "Diferencia: sin datos comparables"
        referencia.Text = "BCE: no consultado"
        AddHandler consultar.Click, Async Sub(sender, e) Await ConsultarAsync()
        AddHandler convertir.Click, Sub(sender, e) Convertir()
    End Sub

    Private Async Function ConsultarAsync() As Task
        Await EjecutarAsync(Async Function(ct)
            tasaMercado = Nothing
            tasaBCN = Nothing
            Dim errores As New List(Of String)
            Try
                tasaMercado = Await New ServicioTipoCambio().ConsultarAsync(ct)
                mercado.Text = $"ExchangeRate-API: 1 USD = {Numero(tasaMercado.Tasa)} NIO · {tasaMercado.FechaFuente}"
            Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                mercado.Text = "ExchangeRate-API: no disponible"
                errores.Add("Mercado: " & ex.Message)
            End Try

            Try
                tasaBCN = Await New ServicioBCN().ConsultarAsync(ct)
                oficial.Text = $"BCN: 1 USD = {Numero(tasaBCN.Tasa)} NIO · {tasaBCN.FechaFuente}"
                referencia.Text = "BCE: no necesario (BCN disponible)"
            Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                oficial.Text = "BCN: no disponible"
                errores.Add("BCN: " & ex.Message)
                Try
                    Dim euro = Await New ServicioBCE().ConsultarAsync(ct)
                    referencia.Text = $"BCE (referencia distinta): 1 EUR = {Numero(euro.Tasa)} USD · {euro.FechaFuente}"
                Catch exBce As Exception When Not TypeOf exBce Is OperationCanceledException
                    referencia.Text = "BCE: no disponible"
                    errores.Add("BCE: " & exBce.Message)
                End Try
            End Try

            If tasaMercado IsNot Nothing AndAlso tasaBCN IsNot Nothing Then
                Dim delta = tasaMercado.Tasa - tasaBCN.Tasa
                Dim porcentaje = delta / tasaBCN.Tasa * 100D
                diferencia.Text = $"Diferencia mercado - BCN: {Numero(delta)} NIO ({porcentaje:+0.00;-0.00;0.00} %)"
            Else
                diferencia.Text = "Sin dos tasas NIO/USD disponibles para comparar."
            End If
            convertir.Enabled = tasaMercado IsNot Nothing OrElse tasaBCN IsNot Nothing
            Convertir()

            Try
                Dim repo As New Repositorio()
                If Not ConexionBD.SoloRespaldo Then
                    If tasaMercado IsNot Nothing Then
                        Await repo.GuardarTasaAsync("ExchangeRate-API", tasaMercado.Tasa,
                            tasaMercado.FechaFuente, DateTime.UtcNow, ct)
                    End If
                    If tasaBCN IsNot Nothing Then
                        Await repo.GuardarTasaAsync("BCN", tasaBCN.Tasa,
                            tasaBCN.FechaFuente, DateTime.UtcNow, ct)
                    End If
                End If
                historial.DataSource = Await repo.HistorialAsync(ct)
            Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                errores.Add("MariaDB: " & ex.Message)
            End Try
            Estado.Text = If(errores.Count = 0,
                "Consulta finalizada; revisa fechas y fuentes.", String.Join(" | ", errores))
        End Function, consultar, convertir)
    End Function

    Private Sub Convertir()
        Dim tasa = If(tasaMercado, tasaBCN)
        If tasa Is Nothing OrElse tasa.Tasa <= 0D Then
            resultado.Text = "Conversión pendiente: consulta una tasa NIO/USD."
            Return
        End If
        Dim valor = If(sentido.SelectedIndex = 0,
                       monto.Value * tasa.Tasa, monto.Value / tasa.Tasa)
        Dim moneda = If(sentido.SelectedIndex = 0, "NIO", "USD")
        resultado.Text = $"Resultado: {valor.ToString("N2", CultureInfo.GetCultureInfo("es-NI"))} {moneda}"
    End Sub

    Private Shared Function Numero(n As Decimal) As String
        Return n.ToString("N4", CultureInfo.GetCultureInfo("es-NI"))
    End Function
End Class
