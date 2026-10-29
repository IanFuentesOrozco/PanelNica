# Integrante 1 — Clima

Esta carpeta pertenece al proyecto compartido PanelNica, no es una aplicación independiente.
Mantén el nombre `Integrante 1` y reúne las otras tres carpetas junto al `.vbproj`.

## Archivos

- `Servicios/ServicioClima.vb`: URL, lectura JSON y modelo del pronóstico.
- `Formularios/frmClima.vb`: ciudad, datos actuales, tabla de cinco días y avisos.
- Depende de modelos/HTTP/interfaz del integrante 4 y del repositorio del integrante 2.

## Revisar y probar antes de integrar

1. Abre el proyecto completo y ejecuta con F5.
2. Consulta Estelí y otra ciudad; comprueba que las coordenadas de ambas se distinguen.
3. Comprueba cinco filas, unidades y fecha de la fuente.
4. Agrega una ciudad desde Ciudades y pulsa Recargar ciudades.
5. Consulta una ciudad, activa el modo de respaldo y consulta la misma: debe mantener
   los datos y su fecha. Una ciudad nunca consultada no debe inventar un respaldo.
6. Explica por qué se usa InvariantCulture para la URL y Await para la red.

Las pruebas automatizadas incluyen casos 60 %, 61 % y datos nulos: no dependen de que
el pronóstico real del día tenga lluvia para verificar el umbral.

Registra en un commit las revisiones, pruebas o cambios que realmente hayas realizado.
