# Integrante 2 — Ciudades y persistencia

Parte del único proyecto PanelNica. Requiere los otros módulos para compilar.

## Archivos

- `Datos/ConexionBD.vb`: conexión asíncrona, configuración y contraseña solo en memoria.
- `Datos/Repositorio.vb`: CRUD, caché e historial con consultas parametrizadas.
- `Formularios/frmCiudades.vb`: agregar, editar, eliminar y listar.
- `Formularios/frmConfiguracion.vb`: conexión y preparación de tablas.
- `SQL/base_datos.sql`: base y tres tablas sin borrar datos existentes.

## Revisar y probar

1. Sigue la configuración de MariaDB del README principal.
2. Agrega una ciudad, edítala, elimínala y verifica su desaparición de la lista.
3. Prueba nombre vacío, nombre duplicado y límites de coordenadas.
4. Comprueba que un nombre con apóstrofe se guarda como texto mediante parámetros.
5. Consulta las APIs y revisa que cache_respuestas e historial reciban filas.
6. Prueba el modo de respaldo con MariaDB encendida y sin internet.
7. Explica las claves de caché, las fechas UTC y por qué no se guarda la contraseña.

No compartas carpetas de datos de MariaDB ni credenciales en el repositorio.
Los demás integrantes usan este repositorio desde sus servicios; los formularios
no construyen SQL ni crean HttpClient por su cuenta.
