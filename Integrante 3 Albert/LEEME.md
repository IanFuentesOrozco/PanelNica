# Integrante 3 — Tipo de cambio (Albert)

Este módulo es parte del único proyecto PanelNica.

- `ServicioTipoCambio.vb`: ExchangeRate-API, JSON, moneda base USD y tasa NIO/USD.
- `ServicioBCN.vb`: consulta SOAP al BCN, fecha de Nicaragua y respuesta XML.
- `ServicioBCE.vb`: referencia EUR/USD como alternativa *informativa*, nunca oficial NIO/USD.
- `frmTipoCambio.vb`: consulta asíncrona, comparación, conversor y tabla de historial.

## Dependencias

Usa `ServicioHttp`, `DocumentoServicio` y `FormularioBase` del módulo de integración
(Integrante 4), así como `Repositorio` y `ConexionBD` del Integrante 2.
No edites `PanelNica.vbproj`: .NET incluye automáticamente los `.vb` del proyecto.

## Antes de integrar

1. Reunir las cuatro carpetas, pues antes faltan clases compartidas y no compila completo.
2. Compilar con `dotnet build PanelNica.slnx` usando .NET SDK 10 en Windows.
3. Configurar MariaDB y crear tablas con la interfaz; no guardar contraseñas en GitHub.
4. Consultar las tasas con internet, verificar referencias y dirección del conversor.
5. Desconectar internet (con MariaDB encendida) y probar solo respaldo.
6. Validar errores de servicio y que un XML del BCE nunca se muestre como NIO/USD.
7. Revisar que las consultas repetidas no dupliquen registros indebidamente; el
   guardado actual almacena cada consulta de la sesión cuando no está en modo respaldo.

> Código inicial para revisión y aprendizaje; aún requiere compilación y pruebas.
