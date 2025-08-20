# 📝 Post-it Digital

Una aplicación de notas adhesivas (Post-it) para Windows desarrollada en C# con WinForms.

## ✨ Características

- **🎨 Colores personalizables**: Amarillo, azul, verde, rosa y más colores personalizados
- **👻 Control de opacidad**: Ajusta la transparencia de las notas (30-100%)
- **💾 Auto-guardado**: Las notas se guardan automáticamente cada 2 segundos
- **📍 Posición persistente**: Las notas mantienen su ubicación al reiniciar
- **🎯 Sin interferencias**: Los Post-its no se mantienen siempre encima de otras aplicaciones
- **🖱️ Arrastrar y soltar**: Mueve las notas libremente por el escritorio
- **🍕 Bandeja del sistema**: La aplicación se ejecuta minimizada en la barra de notificaciones
- **⌨️ Atajos de teclado**: 
  - `Ctrl+S`: Guardar nota
  - `Ctrl+Del`: Eliminar nota
- **🎨 Diseño moderno**: Bordes redondeados y apariencia limpia

## 🚀 Uso

1. **Crear nueva nota**: Haz clic en el icono de la bandeja del sistema y selecciona "Nueva Nota"
2. **Editar**: Haz clic en los campos de título y texto para editarlos
3. **Cambiar color**: Clic derecho en la nota → Selecciona un color predefinido o "Más colores..."
4. **Ajustar opacidad**: Clic derecho → "Cambiar opacidad..."
5. **Mover**: Haz clic y arrastra la nota a cualquier parte del escritorio
6. **Eliminar**: Clic derecho → "Eliminar"

## 🛠️ Tecnologías

- **C# .NET 9.0**
- **Windows Forms**
- **System.Text.Json** para persistencia de datos

## 📦 Estructura del proyecto

```
Post-it/
├── Form1.cs              # Ventana principal y gestión de la bandeja del sistema
├── Form1.Designer.cs     # Diseño de la ventana principal
├── StickyNoteForm.cs     # Formulario individual de cada Post-it
├── StickyNoteData.cs     # Modelo de datos para las notas
├── Program.cs            # Punto de entrada de la aplicación
├── Post-it.csproj        # Configuración del proyecto
└── Post-it.sln           # Solución de Visual Studio
```

## 🎯 Funcionalidades técnicas

- **Serialización JSON** para guardar y cargar notas
- **NotifyIcon** para funcionamiento en la bandeja del sistema
- **Menús contextuales** para interacción rápida
- **Bordes redondeados** usando GraphicsPath
- **Sistema de arrastre personalizado** para movimiento fluido
- **Auto-redimensionamiento** basado en el contenido del texto

## 🏃‍♂️ Ejecución

La aplicación se ejecuta minimizada en la bandeja del sistema. Para acceder:
- **Doble clic** en el icono de la bandeja para mostrar/ocultar la ventana principal
- **Clic derecho** en el icono para acceder al menú contextual

## 💡 Características avanzadas

- Las notas se guardan automáticamente en `sticky_notes.json`
- Soporte para múltiples notas simultáneas
- Interfaz limpia sin barras de desplazamiento
- Confirmación antes de eliminar notas
- Vista previa en tiempo real de cambios de opacidad

---

Desarrollado con ❤️ en C# y WinForms
