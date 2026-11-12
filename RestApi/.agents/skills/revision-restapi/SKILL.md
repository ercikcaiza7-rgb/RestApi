---
name: revision-restapi
description: Revisa el código de la RestApi antes de hacer commit verificando arquitectura en capas, seguridad SQL, convenciones y limpieza de archivos.
---

# Revisión de Código Pre-Commit (RestApi)

Cuando el usuario pida revisar el código antes del commit, inspecciona todo el proyecto `RestApi` y evalúa cada uno de los siguientes puntos contra las reglas de `AGENTS.md`:

1. **Arquitectura en capas:** Verificar que `Web/` solo tenga endpoints (`MapGroup`), `Negocio/` solo lógica y validaciones, y `Datos/` sea el único lugar con consultas SQL.
2. **Seguridad SQL:** Comprobar que todas las consultas en `Datos/` usen parámetros (`@param`) y no exista concatenación de cadenas en SQL.
3. **Convenciones asíncronas:** Verificar que todos los métodos asíncronos terminen con el sufijo `Async`.
4. **Borrado lógico:** Asegurar que en `Platos` no exista `DELETE` físico, sino actualización de `Disponible = 0` y filtro `Disponible = 1`.
5. **Manejo de excepciones y respuestas:** Verificar que se utilice `Respuesta.cs` y las excepciones estándar (`ArgumentException`, `KeyNotFoundException`, `InvalidOperationException`).
6. **Archivos sensibles y temporales:** Revisar `git status` y el directorio para confirmar que no existan archivos `.env` ni carpetas `bin/` u `obj/` sin ignorar.

## Formato de salida obligatorio

Presenta el resultado como una lista de verificación clara:
- Usa **`✓`** para cada criterio que se cumple correctamente.
- Usa **`✗`** para cada hallazgo o incumplimiento detectado, indicando el archivo exacto, la línea o método afectado y cómo corregirlo.
