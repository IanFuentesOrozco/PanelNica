# Validación realizada

Fecha: 26 de octubre de 2026 (America/Denver).

## Entorno

- Windows, .NET SDK 10.0.402, destino `net10.0-windows`.
- MySqlConnector 2.4.0.
- Instancia temporal separada de MariaDB 12.3 en 127.0.0.1:33177.
- Base exclusiva `panelnica_pruebas`; no se utilizaron bases existentes del usuario.
- La instancia temporal se apagó al terminar las comprobaciones.

## Compilación

`dotnet build --configuration Release`: correcta, cero errores y cero advertencias.
La restauración se realizó usando paquetes disponibles localmente. En otra máquina
Visual Studio restaurará MySqlConnector desde NuGet con internet.

También se extrajeron los cuatro ZIP en una carpeta nueva, se verificó que reunieran
exactamente los mismos archivos sin duplicados y se compiló esa reconstrucción:
cero errores y cero advertencias.

## Pruebas automáticas: 23 comprobaciones correctas

1. Parseo de clima actual y cinco días.
2. Umbral de lluvia: 60 % no avisa; 61 % sí.
3. Valores nulos conservados como datos ausentes.
4. Parseo de tasas por moneda.
5. Conversión reversible USD/NIO.
6. Rechazo de tasa cero.
7. SOAP del BCN con espacios de nombres.
8. XML del BCE: USD/EUR y fecha.
9. Siete países y monedas.
10. Creación de tablas y ciudades iniciales en MariaDB.
11. Inserción parametrizada de nombre con apóstrofe.
12. Edición de ciudad persistida.
13. Eliminación de ciudad persistida.
14. Rechazo de coordenadas fuera de rango.
15. Registro y lectura de historial.
16. Solicitud HTTP contra un servidor local de prueba.
17. Guardado de respuesta válida en MariaDB.
18. Recuperación de caché ante HTTP 503, conservando fecha.
19. Conservación del código HTTP de fallo en el estado.
20. Una respuesta JSON inválida no sobrescribe la copia válida.
21. El modo sin conexión no envía solicitudes al servidor.
22. La caché vigente evita consultas repetidas de tasas.
23. Cancelación de una solicitud HTTP en curso.

## Servicios públicos: 6 comprobaciones correctas

- Open-Meteo: clima actual y cinco días para Estelí.
- ExchangeRate-API: tasas con base USD y NIO positivo.
- BCN: petición SOAP real y tasa positiva.
- BCE: XML real con referencia USD/EUR.
- Países: siete países recibidos desde el catálogo alternativo público.
- Recuperación posterior de los cinco servicios desde la caché de MariaDB con el modo
  sin conexión activado.

REST Countries v3.1 devolvió HTTP 200 con un objeto de error de versión retirada,
en lugar de una lista de países. La validación detectó que el contenido era inválido
y usó `mledoze/countries`, con una advertencia explícita. No se inventó una respuesta
exitosa de REST Countries.

Las pruebas distinguen solicitudes al servidor local con datos de prueba de las
solicitudes reales a proveedores. No se distribuyen las respuestas de tasas recibidas.

## Revisión visual

Se renderizaron y revisaron las siete pantallas: menú, clima, ciudades, cambio,
Centroamérica, estado de servicios y configuración. Se comprobaron títulos,
distribución, botones y campos a tamaño inicial de 1040 × 680 de área cliente.
Esto es revisión de renderizado; no equivale a una prueba manual completa de todos
los flujos desde la interfaz ni de todas las escalas y resoluciones de Windows.

Actualización de interfaz: se retiraron las indicaciones extensas de configuración
y los pasos numerados del menú, se acortó el aviso de modo sin conexión y se agregó
Volver al menú a las seis pantallas secundarias. Se recompiló sin errores ni
advertencias y se volvieron a renderizar las siete pantallas.

## Límites y configuración pendiente en cada equipo

- Se debe instalar/iniciar MariaDB y configurar una cuenta propia. No se suministran credenciales.
- El servidor y los permisos del equipo de cada integrante pueden ser diferentes.
- Las APIs pueden cambiar o estar temporalmente fuera de servicio.
- La guía de clase utiliza REST Countries v3.1. Su retirada se documenta y se implementa
  una fuente alternativa sin clave; conviene explicar esa adaptación al presentar el proyecto.
- El XML del BCE no puede reemplazar una tasa oficial NIO/USD: se muestra EUR/USD
  como referencia separada si no hay datos BCN utilizables.
- El estado de servicios se conserva durante la sesión; historial y caché sí persisten en MariaDB.
- El proyecto integra las ocho funciones del resumen de PanelNica de la guía recibida.
  No se dispuso de una rúbrica adicional de las sesiones 2 y 3.
