![Magic Tower Screenshot](MagicTower_Screenshot.png)

# MagicTower

MagicTower ist ein textbasiertes Fantasy-Abenteuer, das sich wie eine Reise durch einen alten, lebendigen Turm anfühlt. Hinter jeder Tür warten neue Gegner, Beute, Entscheidungen und kurze erzählerische Momente, während ein KI-gestützter Game Master das Abenteuer begleitet. Der Spieler führt einen Helden Stockwerk für Stockwerk nach oben, überlebt Kämpfe, verbessert Ausrüstung und versucht, die Spitze des Turms zu erreichen.

## Spielidee

Im Zentrum steht ein leichtgewichtiges RPG-System mit drei Charakterklassen, mehreren Schwierigkeitsgraden und einem rundenbasierten Kampffluss. Das Spiel verbindet klassische Tower-Crawler-Elemente mit moderner Webtechnik und einer KI-Integration für Chat- und Story-Interaktionen.

Wichtige Gameplay-Bausteine:

- Drei Klassen: Warrior, Archer, Druid
- Turm-Fortschritt über mehrere Etagen je nach Schwierigkeit
- Rundenbasierte Kämpfe mit normalen und Spezialangriffen
- Bosskämpfe in regelmäßigen Abständen
- Gold, Waffen, Upgrades und Fortschritt pro Session
- Chat-basierte Interaktion über Web API, MCP Tool und n8n

## Technischer Überblick

Ein zentrales Architekturmerkmal von MagicTower ist die Trennung zwischen generativer Interaktion und deterministischer Systemausführung. Das Large Language Model übernimmt die narrative Führung, Dialoggestaltung und kontextbezogene Interpretation von Spieleraktionen. Die tatsächliche Ausführung spielrelevanter Operationen erfolgt jedoch nicht frei durch das Modell selbst, sondern kontrolliert über das MCP Tool. Dadurch werden generative Fähigkeiten in klar definierte, reproduzierbare und technisch überprüfbare Spielaktionen überführt. Das MCP Tool fungiert damit als Ausführungsschicht, die konsistente Regeln, stabile Zustandsübergänge und nachvollziehbare Spiellogik sicherstellt.

Das Repository besteht aus mehreren Projekten, die gemeinsam das MagicTower-System bilden:

- `MagicTower.AngularApp`
  Angular-Frontend für die Benutzeroberfläche und die Kommunikation mit der Web API.
- `MagicTower.WebApi`
  ASP.NET Core Web API für Spiellogik-Zugriffe, Persistenz, Chat-Endpunkte und Frontend-Anbindung.
- `MagicTower.McpTool`
  Separater MCP-Server für Tool-Aufrufe rund um den Game-Master-Flow.
- `MagicTower.Logic`
  Domänenlogik, Entity Framework Core, Datenkontext und Spielmodule.
- `MagicTower.Common`
  Gemeinsame Contracts, Modelle und Hilfstypen.
- `MagicTower.ConApp`
  Konsolenanwendung für Initialisierung, Datenbank-Setup und Hilfsfunktionen.
- `TemplateTools.ConApp`, `MagicTower.CodeGenApp`, `TemplateTools.Logic`
  Werkzeuge für Code-Generierung und Template-basierte Entwicklung.

## Technische Daten

- Backend: .NET 8 für Web API, Logic, Common und ConApp
- MCP Tool: .NET 10
- Frontend: Angular 19
- UI-Basis: Bootstrap und Bootstrap Icons
- Datenzugriff: Entity Framework Core
- Standard-Datenbank in der aktuellen Konfiguration: SQLite
- KI-Integration: n8n-Workflow mit externem LLM/Game-Master-Flow

Standard-Ports in der aktuellen Entwicklungskonfiguration:

- Angular App: `http://127.0.0.1:54091`
- Web API: `http://localhost:5096` und `https://localhost:7074`
- MCP Tool: `http://localhost:5087` und `https://localhost:7076`
- n8n: `http://localhost:5678`

## Voraussetzungen

Bevor das Projekt lokal gestartet wird, sollten folgende Werkzeuge installiert sein:

- .NET SDK 8
- .NET SDK 10
- Node.js und npm
- Angular CLI optional, da `npm run start` die lokale CLI aus `node_modules` verwendet
- n8n
- Git

## Repository lokal einrichten

### 1. Repository klonen

```powershell
git clone <REPOSITORY-URL>
cd MagicTower
```

### 2. .NET-Abhängigkeiten wiederherstellen und Lösung bauen

```powershell
dotnet restore .\MagicTower.sln
dotnet build .\MagicTower.sln
```

### 3. Frontend-Abhängigkeiten installieren

```powershell
Set-Location .\MagicTower.AngularApp
npm install
Set-Location ..
```

### 4. Datenbank initialisieren

Die Standardkonfiguration verwendet SQLite. Die Datenbank kann über die Konsolenanwendung initialisiert werden:

```powershell
dotnet run --project .\MagicTower.ConApp\MagicTower.ConApp.csproj
```

Beim ersten Start wird die Datenbank initialisiert. Die SQLite-Datei wird lokal im Projektkontext angelegt.

## Entwicklungsstart

Für einen vollständigen lokalen Start werden in der Regel vier Prozesse benötigt:

1. Web API
2. MCP Tool
3. Angular Frontend
4. n8n

### Web API starten

```powershell
dotnet run --project .\MagicTower.WebApi\MagicTower.WebApi.csproj
```

### MCP Tool starten

```powershell
dotnet run --project .\MagicTower.McpTool\MagicTower.McpTool.csproj
```

### Angular App starten

```powershell
Set-Location .\MagicTower.AngularApp
npm run start
Set-Location ..
```

### n8n starten

```powershell
n8n start
```

## Start in VS Code

In der Datei `.vscode/launch.json` ist bereits ein Compound für den parallelen Debug-Start von Web API und MCP Tool vorhanden:

- `WebApi + McpTool`

Dieses Compound kann in VS Code über Run and Debug ausgewählt und mit `F5` gestartet werden.

## Empfohlene Startreihenfolge

1. `dotnet build .\MagicTower.sln`
2. `dotnet run --project .\MagicTower.ConApp\MagicTower.ConApp.csproj`
3. `dotnet run --project .\MagicTower.WebApi\MagicTower.WebApi.csproj`
4. `dotnet run --project .\MagicTower.McpTool\MagicTower.McpTool.csproj`
5. `Set-Location .\MagicTower.AngularApp; npm run start`
6. `n8n start`

Danach ist die Anwendung in der Regel unter diesen URLs erreichbar:

- Frontend: `http://127.0.0.1:54091`
- API: `http://localhost:5096/api`
- n8n Editor: `http://localhost:5678`

## Projektstruktur auf einen Blick

```text
MagicTower/
|- MagicTower.AngularApp/
|- MagicTower.WebApi/
|- MagicTower.McpTool/
|- MagicTower.Logic/
|- MagicTower.Common/
|- MagicTower.ConApp/
|- TemplateTools.ConApp/
|- TemplateTools.Logic/
|- MagicTower.CodeGenApp/
|- MagicTower.sln
```

## Hinweise für neue Entwickler

- Die Lösung kombiniert manuell geschriebenen und generierten Code.
- Änderungen an Entitäten und generierten Bereichen sollten im Kontext der vorhandenen Template- und Generator-Tools erfolgen.
- Für den Spiel- und Chat-Flow sind Web API, MCP Tool und n8n gemeinsam relevant.
- Für Frontend-Entwicklung reicht häufig Web API plus Angular App; für den vollständigen KI-gestützten Flow wird zusätzlich n8n benötigt.

## Nützliche Befehle

```powershell
dotnet build .\MagicTower.sln
dotnet run --project .\MagicTower.ConApp\MagicTower.ConApp.csproj
dotnet run --project .\MagicTower.WebApi\MagicTower.WebApi.csproj
dotnet run --project .\MagicTower.McpTool\MagicTower.McpTool.csproj
Set-Location .\MagicTower.AngularApp; npm install
Set-Location .\MagicTower.AngularApp; npm run start
n8n start
```

## Status

MagicTower ist als kombinierte Spiel-, API- und Tooling-Lösung aufgebaut. Das Repository enthält sowohl das eigentliche Spielsystem als auch Generator- und Support-Projekte für die weitere Entwicklung.