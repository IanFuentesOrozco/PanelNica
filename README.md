# PanelNica

Aplicación académica en Visual Basic .NET 10, Windows Forms y MariaDB.
Un proyecto, cuatro carpetas de trabajo. No contiene commits ni autorías simuladas.
Cada integrante debe revisar, ejecutar y explicar su módulo antes de integrarlo.
La guía original propone equipos de tres; esta organización de cuatro debe concordar
con las indicaciones del docente.

## Qué descargar

- `PanelNica-completo.zip`: el proyecto integrado para abrir y ejecutar.
- `Integrante-1.zip` a `Integrante-4.zip`: partes para distribuir e integrar.
- Las carpetas de los integrantes NO son cuatro proyectos independientes.
- Ningún paquete contiene `bin`, `obj`, contraseñas, una base de datos de prueba ni datos de tasas redistribuidos.

## Abrir en Visual Studio (menús en inglés)

1. Extrae el ZIP completo: clic derecho → **Extract All…**.
2. En Visual Studio: **File → Open → Project/Solution…**.
3. Selecciona `PanelNica.slnx` (también puedes abrir `PanelNica.vbproj`).
4. Espera a que se restaure **MySqlConnector 2.4.0**. La primera vez necesitas internet.
5. Ejecuta **Build → Build Solution**. El proyecto requiere .NET SDK 10 y
   la carga **.NET desktop development** de Visual Studio Installer.
6. Pulsa **F5**. Se abre el menú principal.

Cada pantalla secundaria tiene **Volver al menú**, en la esquina superior derecha.
Si hay una consulta en curso, volver cancela esa operación y regresa al menú existente.

Los formularios se crean en código. Usa **View Code** para editarlos.
No hay que arrastrar controles ni agregar archivos `.Designer.vb` o `.resx`.
No copies encima los formularios de ClimaNica: este es un proyecto separado.

## Preparar MariaDB

1. Inicia tu servidor MariaDB habitual. No se instala ni modifica ningún servicio automáticamente.
2. Abre HeidiSQL u otro cliente SQL con tu usuario y contraseña habituales.
3. Abre y ejecuta `Integrante 2/SQL/base_datos.sql`. Crea `panelnica` y sus tres tablas.
4. En PanelNica abre **Configurar MariaDB**.
5. Escribe servidor (normalmente `localhost`), puerto (normalmente `3306`),
   base de datos `panelnica`, tu usuario real y su contraseña.
6. Pulsa **Probar conexión**, después **Crear tablas y ciudades iniciales**.
7. **Guardar configuración** recuerda los campos salvo la contraseña. La contraseña
   solo permanece durante esa ejecución; vuelve a introducirla al reiniciar.

La cuenta de MariaDB debe tener permisos para usar y crear las tablas de esa base.
No compartas contraseñas en GitHub. El archivo de configuración sin contraseña se
guarda en `%LOCALAPPDATA%/PanelNica/conexion.json`, fuera del repositorio.

## Probar los módulos

### Clima

- **Clima y pronóstico** → selecciona una ciudad → **Consultar clima**.
- Muestra temperatura, humedad y viento actuales y cinco días de mínimas, máximas y lluvia.
- Alerta solamente cuando una probabilidad es estrictamente mayor a 60 %.
- Los valores nulos se muestran vacíos o como «Sin dato», nunca como ceros inventados.
- Las horas del clima se interpretan en America/Managua.
- Si MariaDB no está disponible se ofrecen cinco ciudades iniciales, con aviso explícito.
  Esto no sustituye el CRUD ni el respaldo en la base.

### Ciudades

- **Administrar ciudades** → **Nueva** → nombre y coordenadas → **Guardar**.
- Selecciona una fila para editarla; **Eliminar** solicita confirmación.
- En Clima pulsa **Recargar ciudades** para ver los cambios.

### Tipo de cambio

- **Consultar tasas** obtiene NIO/USD de ExchangeRate-API y la tasa oficial del BCN por SOAP/XML.
- El conversor funciona en ambas direcciones. El historial guarda las consultas nuevas
  recibidas de internet, con fecha de consulta UTC y fecha de la fuente.
- Se muestran diferencia absoluta y porcentual cuando ambas tasas están disponibles.
- ExchangeRate-API es una referencia agregada: no representa una cotización de compra o
  venta de un banco comercial ni una tasa garantizada para una transacción.
- Si BCN falla y hay respaldo de la misma fecha, usa ese respaldo. Si no lo hay,
  muestra el XML del BCE como referencia **EUR/USD**. El BCE no publica NIO en ese feed:
  no se presenta su tasa como oficial NIO/USD ni se calcula una comparación falsa.
- La tasa BCN se solicita para la fecha actual de Nicaragua (UTC−6).

### Centroamérica

- Consulta países y monedas. Las tasas son unidades de cada moneda por 1 USD.
- Incluye los siete países, con una fila por moneda cuando un país tiene más de una.
- Las tasas que no ofrece el proveedor quedan vacías.
- La versión REST Countries v3.1 de la guía fue retirada. El programa intenta la URL de
  la guía y, si no entrega una lista válida, utiliza el catálogo público
  `mledoze/countries`, indicando la fuente alternativa en pantalla y en Estado de servicios.

### Respaldo sin conexión

1. Con internet y MariaDB funcionando, consulta clima, tasas y países al menos una vez.
2. Vuelve al menú y marca **Usar solamente el respaldo de MariaDB**.
3. Abre los módulos y repite las consultas. Muestran la fecha original de lo guardado.
4. MariaDB debe seguir funcionando: el respaldo está en esa base, no en archivos de ejemplo.
5. Una consulta sin respaldo previo muestra un mensaje; no se inventan datos.
6. Desmarca la casilla para volver a consultar internet.

La caché de clima distingue coordenadas. La del BCN distingue la fecha solicitada:
no usa la tasa de ayer como si fuera oficial de hoy. Para las tasas agregadas se
reutiliza la caché una hora; para países, un día; para XML del BCE y BCN, doce horas.
Si falla internet se puede recuperar una copia más antigua, siempre con su fecha visible.

### Estado de servicios

Muestra la última operación por servicio durante la sesión: fecha UTC, HTTP,
milisegundos, origen y detalle. «Sin solicitud HTTP» significa que se leyó la caché.
Un HTTP 200 con JSON inválido queda registrado como error de contenido.
La ventana **Actualizar estado** no envía nuevas solicitudes a las APIs.

## Reparto y subida al repositorio

| Carpeta | Responsabilidad |
|---|---|
| Integrante 1 | Clima actual, cinco días, alertas |
| Integrante 2 | MariaDB, ciudades, historial, caché y configuración |
| Integrante 3 | Tasas, BCN, BCE, conversor e historial en pantalla |
| Integrante 4 | Menú, países, estados, modelos, HTTP, interfaz común y pruebas |

Para reunir los paquetes, extraigan **los cuatro ZIP en la misma carpeta**, sin cambiar
el nombre ni la estructura de sus carpetas. El ZIP del integrante 4 incluye los archivos
de raíz (`PanelNica.vbproj`, solución, README y `.gitignore`). Los otros tres agregan
sus carpetas. Después abran la solución y compilen.

Si suben por separado a un repositorio, conserven esa misma estructura. La compilación
completa requiere los cuatro módulos. El proyecto incluye los `.vb` automáticamente:
no deben editar `PanelNica.vbproj` para agregar archivos a las carpetas.

Cada integrante revisa el README de su carpeta, prueba su sección y sube el aporte
que efectivamente realizó. Los paquetes no atribuyen autoría ni crean contribuciones
ficticias. No suban ZIP, `bin`, `obj`, contraseñas ni archivos de la base de datos al repo.

## Validación y pruebas

Ver `VALIDACION.md` para los resultados y límites de las comprobaciones realizadas.
Las pruebas incluidas requieren una base exclusiva cuyo nombre termine en `_pruebas`.
Consulta `Integrante 4/Pruebas/LEEME.md` antes de ejecutarlas.

## Fuentes y cambios respecto a la guía

- Open-Meteo: https://open-meteo.com/en/docs
- Rates By Exchange Rate API: https://www.exchangerate-api.com/docs/free
- BCN y guía SOAP: https://www.bcn.gob.ni/descripcion-del-servicio
- BCE: https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html
- Retiro de REST Countries v3.1: https://restcountries.com/docs/countries/legacy-api-deprecation
- Catálogo alternativo y licencia ODbL 1.0: https://github.com/mledoze/countries

El código consulta las fuentes en ejecución. No incluye ni redistribuye respuestas
reales de ExchangeRate-API dentro de los paquetes.
