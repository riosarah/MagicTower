# Chat History Implementation

## Überblick

Der Chatverlauf wird jetzt in einer **separaten `ChatMessage` Entity** gespeichert, nicht mehr als JSON in der `GameSession`.

## Datenbankstruktur

### ChatMessage Entity

```csharp
public class ChatMessage
{
    public int Id { get; set; }
    public int GameSessionId { get; set; }
    public string Message { get; set; }
    public bool IsAiMessage { get; set; }    // true = AI, false = User
    public DateTime SentAt { get; set; }
    public int SequenceNumber { get; set; }
    
    // Navigation
    public GameSession GameSession { get; set; }
}
```

### Vorteile dieser Lösung

? **Normalisierte Datenbank** - Keine JSON-Blobs  
? **Bessere Performance** - Effiziente Abfragen möglich  
? **Einfache Filterung** - Z.B. nur AI- oder User-Nachrichten  
? **Historische Analysen** - Auswertungen über alle Sessions hinweg  
? **Skalierbar** - Pagination möglich  

## Verwendung

### 1. Chat-Nachricht hinzufügen

```csharp
using MagicTower.WebApi.Extensions;

// Im Controller oder Service
var session = await context.GameSessionSet.GetByIdAsync(sessionId);

// User-Nachricht speichern
await session.AddChatMessageAsync(context, "Ich greife den Ork an!", isAiMessage: false);

// AI-Nachricht speichern
await session.AddChatMessageAsync(context, "Du verursachst 25 Schaden!", isAiMessage: true);
```

### 2. Chatverlauf laden

```csharp
// Gesamten Chatverlauf laden
var chatHistory = await session.GetChatHistoryAsync(context);

// Nur die letzten 10 Nachrichten laden
var recentHistory = await session.GetRecentChatHistoryAsync(context, count: 10);

// Verwendung
foreach (var message in chatHistory)
{
    Console.WriteLine($"{message.Role}: {message.Content}");
}
```

### 3. Im GameMcpController verwenden

```csharp
[HttpPost("chat")]
public async Task<ActionResult> Chat([FromBody] ChatRequest request)
{
    using var context = _contextAccessor.GetContext();
    
    // Lade GameSession
    var session = await context.GameSessionSet.GetByIdAsync(request.GameSessionId);
    
    // Speichere User-Nachricht
    await session.AddChatMessageAsync(context, request.Message, isAiMessage: false);
    
    // Lade bisherigen Chatverlauf für AI-Kontext
    var conversationHistory = await session.GetRecentChatHistoryAsync(context, count: 10);
    
    // Sende an n8n
    var n8nRequest = new N8nRequestDto
    {
        Message = request.Message,
        ConversationHistory = conversationHistory,
        // ...
    };
    
    var n8nResponse = await SendToN8n(n8nRequest);
    
    // Speichere AI-Antwort
    await session.AddChatMessageAsync(context, n8nResponse.Response, isAiMessage: true);
    
    return Ok(n8nResponse);
}
```

## Migration erstellen

Nach dem Hinzufügen der neuen Entity muss eine Migration erstellt werden:

```bash
# In MagicTower.Logic Verzeichnis
dotnet ef migrations add AddChatMessageEntity --project ../MagicTower.Logic --startup-project ../MagicTower.WebApi

# Migration anwenden
dotnet ef database update --project ../MagicTower.Logic --startup-project ../MagicTower.WebApi
```

## API-Endpoints

### Chat-Historie abrufen

```http
GET /api/GameMcp/session/{sessionId}/chat-history
```

**Response:**
```json
[
  {
    "role": "user",
    "content": "Ich greife den Ork an!"
  },
  {
    "role": "assistant",
    "content": "Du verursachst 25 Schaden! Der Ork hat noch 50 HP."
  }
]
```

### Chat-Historie exportieren

```http
GET /api/GameMcp/session/{sessionId}/chat-history/export
```

Lädt die komplette Chat-Historie als JSON-Datei herunter.

## Frontend-Integration

### TypeScript Interface

```typescript
interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

interface GameSession {
  id: number;
  characterId: number;
  currentFloor: number;
  chatHistory?: ChatMessage[];  // Wird vom Backend geladen
}
```

### Service Method

```typescript
class GameService {
  async loadSession(sessionId: number): Promise<GameSession> {
    // Session laden
    const session = await this.http.get<GameSession>(
      `/api/GameSessions/${sessionId}`
    ).toPromise();
    
    // Chat-Historie laden
    session.chatHistory = await this.http.get<ChatMessage[]>(
      `/api/GameMcp/session/${sessionId}/chat-history`
    ).toPromise();
    
    return session;
  }
  
  async sendMessage(sessionId: number, message: string): Promise<ChatResponse> {
    return this.http.post<ChatResponse>('/api/GameMcp/chat', {
      message,
      gameSessionId: sessionId
      // ConversationHistory wird vom Backend automatisch geladen!
    }).toPromise();
  }
}
```

## Datenmigration (Optional)

Falls Sie alte Sessions mit JSON-ChatHistory haben:

```sql
-- Migriere alte JSON-ChatHistory in ChatMessage-Tabelle
-- (Manuelles SQL-Script nach Bedarf)
```

## Performance-Überlegungen

### Pagination

Bei sehr langen Gesprächen (>100 Nachrichten):

```csharp
public static async Task<PagedResult<ChatMessage>> GetChatHistoryPagedAsync(
    this GameSession session,
    IContext context,
    int pageNumber = 1,
    int pageSize = 20)
{
    var chatMessageSet = context.GetEntitySet<ChatMessage>();
    var allMessages = await chatMessageSet.GetAllAsync();
    
    var sessionMessages = allMessages
        .Where(m => m.GameSessionId == session.Id)
        .OrderBy(m => m.SequenceNumber);
    
    var totalCount = sessionMessages.Count();
    var items = sessionMessages
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    
    return new PagedResult<ChatMessage>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

### Caching

Für häufig geladene Chat-Historien:

```csharp
private static Dictionary<int, List<ChatMessage>> _chatCache = new();

public static async Task<List<ChatMessage>> GetChatHistoryCachedAsync(
    this GameSession session,
    IContext context,
    bool forceRefresh = false)
{
    if (!forceRefresh && _chatCache.ContainsKey(session.Id))
    {
        return _chatCache[session.Id];
    }
    
    var history = await session.GetChatHistoryAsync(context);
    _chatCache[session.Id] = history;
    
    return history;
}
```

## Zusammenfassung

### Vorher (JSON-basiert):
```csharp
session.ChatHistory = "[{\"role\":\"user\",\"content\":\"...\"}, ...]"
```

### Nachher (Entity-basiert):
```csharp
session.ChatMessages = [
    new ChatMessage { Message = "...", IsAiMessage = false },
    new ChatMessage { Message = "...", IsAiMessage = true }
]
```

### Vorteile:
- ? Sauber strukturiert
- ? Einfach zu querien
- ? Bessere Performance
- ? Einfacher zu erweitern (z.B. Timestamps, Metadata)
- ? Normalisierte Datenbank
