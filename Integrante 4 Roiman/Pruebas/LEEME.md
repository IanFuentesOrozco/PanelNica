# Validación pendiente: Integrante 4

Las pruebas NO se declaran ejecutadas hasta que se corran en Windows con las cuatro carpetas.

1. `dotnet --info`: confirmar SDK .NET 10.
2. `dotnet build PanelNica.slnx`: registrar errores, advertencias y fecha.
3. Abrir con `dotnet run --project PanelNica.vbproj` y comprobar apertura del menú.
4. Entrar en cada ventana y usar «Volver al menú» sin abrir instancias adicionales.
5. Probar clima y caché con Estelí y dos coordenadas distintas.
6. Probar alta, edición y eliminación de ciudades con MariaDB de prueba.
7. Probar tipo de cambio USD ↔ NIO, valores decimales y BCN; verificar fechas.
8. Comprobar siete países, monedas y tasas por USD; sin inventar tasas ausentes.
9. Comprobar «Estado de servicios» y que «Actualizar» no haga HTTP.
10. Activar modo sin conexión, desconectar Internet y verificar que la fuente sea MariaDB.
11. Sin respuesta cacheada, verificar que se explique el dato ausente.
12. Repetir consultas y verificar que se respetan los TTL de cada servicio.
13. Cancelar una solicitud en curso y volver al menú.

**No ejecutar pruebas destructivas sobre la base principal.** Si se crean pruebas de escritura automatizadas, utilizar una base independiente terminada en `_pruebas`.
