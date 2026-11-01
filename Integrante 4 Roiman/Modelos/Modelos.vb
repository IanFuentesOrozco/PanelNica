Imports System
Imports System.Collections.Generic

' Tipos comunes utilizados por los cuatro módulos de PanelNica.
Public Class Ciudad
    Public Property Id As Integer
    Public Property Nombre As String = ""
    Public Property Latitud As Double
    Public Property Longitud As Double
    Public Overrides Function ToString() As String
        Return Nombre
    End Function
End Class

Public Class DiaPronostico
    Public Property Fecha As String = ""
    Public Property Minima As Double?
    Public Property Maxima As Double?
    Public Property Lluvia As Double?
End Class

Public Class Pronostico
    Public Property Hora As String = ""
    Public Property Temperatura As Double?
    Public Property Humedad As Double?
    Public Property Viento As Double?
    Public Property Dias As New List(Of DiaPronostico)
    Public Property Documento As DocumentoServicio
End Class

Public Class EntradaCache
    Public Property Texto As String = ""
    Public Property FechaUtc As DateTime
End Class

Public Class DocumentoServicio
    Public Property Texto As String = ""
    Public Property FechaUtc As DateTime
    Public Property Descripcion As String = ""
    Public Property Origen As String = ""
End Class

Public Class EstadoServicio
    Public Property Servicio As String = ""
    Public Property FechaUtc As String = ""
    Public Property CodigoHTTP As String = ""
    Public Property Milisegundos As Long
    Public Property Origen As String = ""
    Public Property Detalle As String = ""
End Class

Public Class PaisMoneda
    Public Property Pais As String = ""
    Public Property CodigoPais As String = ""
    Public Property Moneda As String = ""
    Public Property CodigoMoneda As String = ""
    Public Property UnidadesPorUSD As Decimal?
End Class
