# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

```bash
# Build the solution
dotnet build TaskRecruiting.sln

# Run tests (requires local API server at localhost:5000)
dotnet test src/TR.Connector.Tests/TR.Connector.Tests.csproj

# Run a single test
dotnet test src/TR.Connector.Tests/TR.Connector.Tests.csproj --filter "FullyQualifiedName~TestMethodName"
```

## Architecture Overview

This is a .NET 8 connector library for a technical support service. The connector acts as an integration layer between the product and the managed system via REST API.

### Project Structure

- **TR.Connectors.Api** - API contracts (interfaces and entities). Defines `IConnector` interface and entity classes (`Permission`, `Property`, `UserProperty`, `UserToCreate`).
- **TR.Connector** - Implementation of `IConnector`. References TR.Connectors.Api via DLL (`libs/TR.Connectors.Api.dll`).
- **TR.Connector.Tests** - xUnit integration tests. Requires a running API server.

### Key Interface: IConnector

The `IConnector` interface (`src/TR.Connectors.Api/Interfaces/IConnector.cs`) defines all connector operations:
- User management: `CreateUser`, `IsUserExists`, `GetUserProperties`, `UpdateUserProperties`
- Permission management: `GetAllPermissions`, `GetUserPermissions`, `AddUserPermissions`, `RemoveUserPermissions`
- Property definitions: `GetAllProperties`
- Configuration: `StartUp(connectionString)` - must be called before other methods

### Configuration

Connector configuration via `StartUp` method with connection string format:
```
url=http://localhost:5000;login=login;password=password
```

**Important**: The `Connector` class requires an empty constructor and `ILogger` must be set before calling `StartUp`.

### Permission ID Format

Permissions use composite IDs: `{Type},{NumericId}`
- IT Roles: `ItRole,{id}` (e.g., `ItRole,5`)
- Request Rights: `RequestRight,{id}` (e.g., `RequestRight,5`)

### User Properties

Fixed set of user properties: `lastName`, `firstName`, `middleName`, `telephoneNumber`, `isLead`, `status`. Login is the unique identifier, not a property.

### API Authentication

Bearer token authentication via `/api/v1/login` endpoint. Token is obtained during `StartUp` and stored internally.

### User Lock Status

Users can be locked (`status == "Lock"`) or unlocked (`status == "Unlock"`). Locked users cannot have permissions modified or properties retrieved.