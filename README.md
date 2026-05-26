# ProjectFlow

**Desarrollado por xops7vargas**

Sistema de gestión de flujos de trabajo con frontend en React y backend en ASP.NET Core.

## Licencia

Este proyecto está licenciado bajo la **Apache License 2.0** - ver el archivo [LICENSE](LICENSE) para más detalles.

## Estructura del Proyecto

```
experimento2/
├── client/                    # Frontend React + TypeScript + Vite
│   ├── src/                   # Código fuente del cliente
│   └── package.json          # Dependencias y scripts de npm
│
└── src/                       # Backend .NET
    ├── ProjectFlow.API/       # API ASP.NET Core
    ├── ProjectFlow.Application/  # Capa de aplicación
    ├── ProjectFlow.Domain/    # Capa de dominio
    └── ProjectFlow.Infrastructure/  # Capa de infraestructura
```

## Requisitos

- **.NET 8.0** o superior
- **Node.js 18+** y **npm**
- **SQL Server** (configurable en appsettings.json)

## Instalación

### Backend

```bash
cd src/ProjectFlow.API
dotnet restore
dotnet run
```

### Frontend

```bash
cd client
npm install
npm run dev
```

## Scripts Disponibles

### Frontend (client/)

| Comando        | Descripción                    |
|----------------|--------------------------------|
| `npm run dev`  | Inicia el servidor de desarrollo |
| `npm run build`| Compila para producción         |
| `npm run lint` | Ejecuta el linter               |
| `npm run preview` | Vista previa de producción    |

## Tecnologías

### Frontend
- React 18
- TypeScript
- Vite
- Material UI
- Redux Toolkit
- React Router
- Recharts
- i18next

### Backend
- ASP.NET Core 8.0
- Entity Framework Core
- SignalR
- JWT Authentication

## Configuración

El backend utiliza `appsettings.json` para la configuración de la base de datos y otros parámetros de la aplicación.