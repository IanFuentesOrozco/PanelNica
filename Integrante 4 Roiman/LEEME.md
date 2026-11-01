# Integrante 4 — Roiman / Royman · Integración e interfaz

Esta carpeta forma parte de UN proyecto Windows Forms .NET 10, no es una aplicación independiente.

## Entrega
- `Modelos/Modelos.vb`: tipos compartidos por clima, ciudades, caché y estados.
- `Servicios/ServicioHttp.vb`: HttpClient único, cancelación, errores, datos validados y caché MariaDB.
- `Servicios/ServicioPaises.vb`: siete países centroamericanos, monedas y cotizaciones por USD.
- `Formularios/FormularioBase.vb`: controles visuales reutilizables, navegación y cancelación.
- `Formularios/frmPrincipal.vb`: menú y activación global del respaldo sin conexión.
- `Formularios/frmCentroamerica.vb`: listado de países/monedas.
- `Formularios/frmEstadoServicios.vb`: última operación HTTP (sin solicitar de nuevo).
- `Program.vb`: punto de entrada definido en `PanelNica.vbproj`.
- `Pruebas/LEEME.md`: lista de verificaciones a ejecutar.

## Dependencias y límites
- No editar las carpetas de Ian, Uriel ni Albert para atribuir trabajo a Roiman.
- Reutiliza `Repositorio` y `ConexionBD` aportados por Uriel.
- Requiere `frmTipoCambio` del PR #3 de Albert (rama `albert-cambio`), todavía Draft.
- No fusionar el PR de Albert ni el de Royman a `main` antes de probar ambos módulos juntos.

## Prueba temporal sin fusionar PRs

Desde `roiman-integracion`, con archivos guardados y commit local:

```bash
git fetch origin
git switch -c verificacion-panelnica
git merge --no-ff --no-edit origin/albert-cambio
dotnet build PanelNica.slnx
dotnet run --project PanelNica.vbproj
```

La rama `verificacion-panelnica` es **local y temporal**: no subirla ni abrir un PR desde ella. El PR de Royman debe apuntar desde `roiman-integracion` a `main` y contener exclusivamente su carpeta. Consultar `Pruebas/LEEME.md` para pruebas reales.

**Estado**: código de primera integración para revisar y probar. No se afirma compilación ni ejecución satisfactoria hasta que se obtenga el resultado real del SDK en Windows.
