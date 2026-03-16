# GameSessions API Endpoints

Dokumentation für alle verfügbaren GameSessions Endpoints.

## Base URL
```
/api/GameSessions
```

---

## ?? Standard CRUD Endpoints

### 1. Get All Sessions (mit Debug-Logging)
```http
GET /api/GameSessions
```

**Beschreibung:** Holt alle GameSessions aus der Datenbank mit ausführlichem Console-Logging für Debugging.

**Response:**
```json
[
  {
    "id": 1,
    "characterId": 1,
    "difficulty": 20,
    "currentFloor": 5,
    "maxFloor": 20,
    "isCompleted": false,
    "startedAt": "2024-01-15T10:30:00Z",
    "completedAt": null,
    "character": { ... },
    "defeatedEnemies": [ ... ]
  }
]
```

**Console Output:**
```
=== GameSessions DEBUG GET START ===
Retrieved X entities from database
Converted to X models
Total time: Xms
=== First Session Details ===
ID: 1
CharacterId: 1
...
```

---

## ?? Paged Endpoint (Empfohlen für Frontend)

### 2. Get Paged Sessions
```http
GET /api/GameSessions/paged
```

**Query Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|-----|---------|--------------|
| `pageNumber` | int | 1 | Seitennummer (1-basiert) |
| `pageSize` | int | 10 | Anzahl Items pro Seite (max: 100) |
| `characterId` | int? | null | Optional: Filter nach Charakter-ID |
| `isCompleted` | bool? | null | Optional: Filter nach Abschluss-Status |
| `sortBy` | string | "StartedAt" | Sortierfeld: StartedAt, CurrentFloor, Difficulty, CompletedAt |
| `sortOrder` | string | "desc" | Sortierung: asc oder desc |

**Beispiel-Requests:**

```bash
# Erste Seite mit 10 Items (Standard)
GET /api/GameSessions/paged

# Seite 2 mit 20 Items
GET /api/GameSessions/paged?pageNumber=2&pageSize=20

# Nur Sessions für Charakter 5
GET /api/GameSessions/paged?characterId=5

# Nur abgeschlossene Sessions
GET /api/GameSessions/paged?isCompleted=true

# Sortiert nach CurrentFloor aufsteigend
GET /api/GameSessions/paged?sortBy=CurrentFloor&sortOrder=asc

# Kombiniert: Charakter 3, nicht abgeschlossen, sortiert nach StartedAt
GET /api/GameSessions/paged?characterId=3&isCompleted=false&sortBy=StartedAt&sortOrder=desc
```

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "characterId": 1,
      "difficulty": 20,
      "currentFloor": 5,
      "maxFloor": 20,
      "isCompleted": false,
      "startedAt": "2024-01-15T10:30:00Z",
      "completedAt": null
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Frontend Usage (TypeScript/Angular):**
```typescript
interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Service Method
getPagedSessions(
  pageNumber: number = 1, 
  pageSize: number = 10,
  characterId?: number,
  isCompleted?: boolean
): Observable<PagedResult<GameSession>> {
  let params = new HttpParams()
    .set('pageNumber', pageNumber.toString())
    .set('pageSize', pageSize.toString());
  
  if (characterId) params = params.set('characterId', characterId.toString());
  if (isCompleted !== undefined) params = params.set('isCompleted', isCompleted.toString());
  
  return this.http.get<PagedResult<GameSession>>(
    `${this.apiUrl}/GameSessions/paged`, 
    { params }
  );
}

// Component Usage
loadSessions(page: number = 1) {
  this.gameService.getPagedSessions(page, 20, this.characterId)
    .subscribe(result => {
      this.sessions = result.items;
      this.totalPages = result.totalPages;
      this.currentPage = result.pageNumber;
      this.hasNext = result.hasNextPage;
      this.hasPrevious = result.hasPreviousPage;
    });
}
```

---

## ?? Debug-spezifische Endpoints

### 3. Get Sessions by Character (Debug)
```http
GET /api/GameSessions/character/{characterId}/debug
```

**Beschreibung:** Holt alle Sessions für einen bestimmten Charakter mit ausführlichem Logging.

**Beispiel:**
```bash
GET /api/GameSessions/character/1/debug
```

**Console Output:**
```
=== GameSessions DEBUG GET BY CHARACTER 1 START ===
Total entities in database: 50
Sessions for character 1: 12
Converted and sorted 12 models
=== All 12 Sessions for Character 1 ===
  Session 15: Floor 10/20, Started: 2024-01-15 10:30:00, Completed: No
  Session 14: Floor 20/20, Started: 2024-01-14 15:20:00, Completed: Yes
  ...
```

---

### 4. Create Test Session
```http
POST /api/GameSessions/test/{characterId}
```

**Beschreibung:** Erstellt eine Test-Session für Debugging-Zwecke.

**Beispiel:**
```bash
POST /api/GameSessions/test/1
```

**Response:**
```json
{
  "id": 99,
  "characterId": 1,
  "difficulty": 20,
  "currentFloor": 1,
  "maxFloor": 20,
  "isCompleted": false,
  "startedAt": "2024-01-15T12:00:00Z",
  "completedAt": null
}
```

---

### 5. Get Database Info
```http
GET /api/GameSessions/dbinfo
```

**Beschreibung:** Zeigt Datenbank-Verbindungsinformationen für Debugging.

**Response:**
```json
{
  "contextType": "MagicTowerDbContext",
  "entitySetType": "EntitySet",
  "timestamp": "2024-01-15T12:00:00Z",
  "message": "Database connection is working"
}
```

---

## ?? Weitere Standard-CRUD Endpoints

### 6. Get by ID
```http
GET /api/GameSessions/{id}
```

### 7. Create Session
```http
POST /api/GameSessions
Content-Type: application/json

{
  "characterId": 1,
  "difficulty": 20,
  "currentFloor": 1,
  "maxFloor": 20,
  "isCompleted": false,
  "startedAt": "2024-01-15T10:00:00Z"
}
```

### 8. Update Session
```http
PUT /api/GameSessions/{id}
Content-Type: application/json

{
  "id": 1,
  "characterId": 1,
  "difficulty": 20,
  "currentFloor": 5,
  "maxFloor": 20,
  "isCompleted": false,
  "startedAt": "2024-01-15T10:00:00Z"
}
```

### 9. Delete Session
```http
DELETE /api/GameSessions/{id}
```

### 10. Count Sessions
```http
GET /api/GameSessions/count
```

**Response:**
```json
45
```

---

## ?? Empfohlene Verwendung im Frontend

### Für Session-Liste mit Pagination:
```typescript
// Verwende den Paged Endpoint
GET /api/GameSessions/paged?pageNumber=1&pageSize=20&characterId=5&sortBy=StartedAt&sortOrder=desc
```

**Vorteile:**
- ? Effizient bei vielen Sessions
- ? Reduziert Datenübertragung
- ? Bessere Performance
- ? Flexibles Filtern und Sortieren
- ? Eingebaute Pagination-Metadaten

### Für einzelne Session-Details:
```typescript
GET /api/GameSessions/{id}
```

### Für Debugging:
```typescript
// Console-Logs im Backend anschauen
GET /api/GameSessions              // Alle mit Logging
GET /api/GameSessions/character/5/debug  // Spezifisch mit Logging
GET /api/GameSessions/dbinfo       // Verbindungstest
```

---

## ?? Performance-Tipps

1. **Verwende immer den Paged Endpoint** für Listen im Frontend
2. **Setze sinnvolle Page Sizes**: 10-50 Items pro Seite
3. **Nutze Filter**: Reduziere Datenmenge mit `characterId` oder `isCompleted`
4. **Cache Results**: Speichere bereits geladene Seiten im Frontend
5. **Lazy Loading**: Lade weitere Seiten erst bei Bedarf

---

## ?? Debugging

### Problem: Keine Sessions werden zurückgegeben

**1. Prüfe Console-Logs:**
```bash
GET /api/GameSessions
```
Schau im Backend-Console nach:
```
!!! NO SESSIONS FOUND IN DATABASE !!!
```

**2. Prüfe Datenbank-Verbindung:**
```bash
GET /api/GameSessions/dbinfo
```

**3. Erstelle Test-Session:**
```bash
POST /api/GameSessions/test/1
```

### Problem: Frontend erhält keine Daten

**1. CORS-Problem?**
- Prüfe Browser Console auf CORS-Fehler
- Prüfe `Program.cs` für CORS-Konfiguration

**2. Falscher Endpoint?**
- Verwende `/api/GameSessions/paged` statt `/api/GameSessions`
- Prüfe Base URL

**3. Serialisierung?**
- Prüfe ob Navigation Properties (Character, DefeatedEnemies) zirkuläre Referenzen verursachen

---

## ?? Notizen

- Alle Endpoints haben ausführliches Console-Logging für Debugging
- Der Paged Endpoint ist **optimal für das Frontend**
- Standard GET ohne Paging sollte nur für Debugging verwendet werden
- Test-Sessions haben immer Difficulty=Medium (20 floors)
- Sorting funktioniert case-insensitive

---

## ?? Siehe auch

- [GameMcpController API](../GameMcpController.cs) - Alternative API für AI-Chat
- [GameSession Model](../../Models/Game/GameSession.cs) - Model Definition
- [GameSession Entity](../../../Logic/Entities/Game/GameSession.cs) - Datenbank Entity
