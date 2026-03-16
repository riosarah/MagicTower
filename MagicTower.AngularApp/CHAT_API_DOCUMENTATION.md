# MagicTower Chat API - Frontend Integration Guide

## Übersicht

Das Frontend kommuniziert mit **einem einzigen Endpoint**: `/api/GameMcp/chat`. Die Game Session dient gleichzeitig als:
1. **Spielsession** (für das eigentliche Spiel mit Etagen, Kämpfen etc.)
2. **Chat-Session** (für den n8n Konversationsverlauf)

---

## Haupt-Endpoint

### **POST** `/api/GameMcp/chat`

**Wichtig:** Die `gameSessionId` wird automatisch initialisiert, wenn ein Charakter vorhanden ist.

### Request Body:
```json
{
  "message": "Hallo! Ich möchte als Elfenkriegerin spielen",
  "characterId": null,
  "gameSessionId": null,
  "conversationHistory": []
}
```

### Response:
```json
{
  "success": true,
  "data": {
    "response": "?? Willkommen bei MagicTower!...",
    "gameState": {
      "character": {
        "id": 1,
        "name": "Elfenkriegerin",
        "characterClass": 2,
        "level": 1,
        "currentHealth": 100,
        "maxHealth": 100,
        "attackPower": 12,
        "gold": 0
      },
      "gameSession": {
        "id": 1,
        "characterId": 1,
        "difficulty": 20,
        "currentFloor": 1,
        "maxFloor": 20
      },
      "currentEnemy": null
    }
  }
}
```

---

## Frontend Implementierung

### 1. Angular Service

```typescript
export class GameService {
  private apiUrl = 'http://localhost:5000/api/GameMcp';
  private conversationHistory: ChatMessage[] = [];
  
  // Diese IDs aus der Response speichern
  private characterId: number | null = null;
  private gameSessionId: number | null = null;

  constructor(private http: HttpClient) {}

  sendMessage(userMessage: string): Observable<ChatResponse> {
    this.conversationHistory.push({
      role: 'user',
      content: userMessage
    });

    const request = {
      message: userMessage,
      characterId: this.characterId,
      gameSessionId: this.gameSessionId,  // Wird automatisch erstellt wenn null
      conversationHistory: this.conversationHistory
    };

    return this.http.post<ApiResponse<ChatResponse>>(`${this.apiUrl}/chat`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            // AI Antwort zur History hinzufügen
            this.conversationHistory.push({
              role: 'assistant',
              content: response.data.response
            });

            // IDs aus Response speichern
            if (response.data.gameState?.character?.id) {
              this.characterId = response.data.gameState.character.id;
            }
            if (response.data.gameState?.gameSession?.id) {
              this.gameSessionId = response.data.gameState.gameSession.id;
            }

            return response.data;
          }
          throw new Error(response.message);
        })
      );
  }
}
```

### 2. Component Beispiel

```typescript
export class GameComponent implements OnInit {
  gameState: GameState | null = null;
  chatMessages: Array<{sender: 'user' | 'ai', text: string}> = [];

  constructor(private gameService: GameService) {}

  onSendMessage(message: string) {
    // User Message zum Chat hinzufügen
    this.chatMessages.push({
      sender: 'user',
      text: message
    });

    // An Backend senden
    this.gameService.sendMessage(message).subscribe({
      next: (response) => {
        // AI Response zum Chat hinzufügen
        this.chatMessages.push({
          sender: 'ai',
          text: response.response
        });

        // Game State aktualisieren
        if (response.gameState) {
          this.gameState = response.gameState;
          this.updateUI();
        }
      },
      error: (error) => {
        console.error('Fehler:', error);
      }
    });
  }

  updateUI() {
    if (!this.gameState) return;

    // Character UI
    if (this.gameState.character) {
      this.characterName = this.gameState.character.name;
      this.characterLevel = this.gameState.character.level;
      this.characterHealth = this.gameState.character.currentHealth;
      this.characterMaxHealth = this.gameState.character.maxHealth;
      this.characterGold = this.gameState.character.gold;
    }

    // Session UI (Etage anzeigen)
    if (this.gameState.gameSession) {
      this.currentFloor = this.gameState.gameSession.currentFloor;
      this.maxFloor = this.gameState.gameSession.maxFloor;
    }

    // Enemy UI
    if (this.gameState.currentEnemy) {
      this.showEnemy(this.gameState.currentEnemy);
    } else {
      this.hideEnemy();
    }
  }
}
```

---

## Spielablauf

### **Schritt 1: App Start**
```typescript
// Erste Nachricht
this.gameService.sendMessage("Hallo");
```

**Response:**
```json
{
  "response": "Willkommen bei MagicTower! Möchtest du ein neues Abenteuer starten?",
  "gameState": null
}
```

---

### **Schritt 2: Charakter erstellen**
```typescript
this.gameService.sendMessage("Ja, ich möchte als Elfenkriegerin mit Bogen spielen");
```

**Was passiert im Backend:**
1. AI ruft MCP Tool `create_character("Elfenkriegerin", 2)` auf (2 = Archer)
2. Charakter wird in DB erstellt mit ID 1
3. **Game Session wird automatisch erstellt** mit ID 1 und Medium Difficulty
4. Response enthält `characterId: 1` und `gameSessionId: 1`

**Response:**
```json
{
  "response": "? Deine Elfenkriegerin wurde erschaffen!...",
  "gameState": {
    "character": {
      "id": 1,
      "name": "Elfenkriegerin",
      "characterClass": 2,
      "className": "Archer"
    },
    "gameSession": {
      "id": 1,
      "characterId": 1,
      "difficulty": 20,
      "currentFloor": 1,
      "maxFloor": 20
    }
  }
}
```

---

### **Schritt 3: Schwierigkeit ändern (optional)**
```typescript
this.gameService.sendMessage("Ich möchte auf Hard spielen");
```

**Was passiert:**
- AI aktualisiert die Game Session auf Difficulty = 30 (Hard)
- `maxFloor` wird zu 30

---

### **Schritt 4: Erster Kampf**
```typescript
this.gameService.sendMessage("Ich bin bereit für den ersten Kampf!");
```

**Response:**
```json
{
  "response": "**Etage 1**\n\nDu betrittst einen dunklen Raum...\n\n?? Ork Ritter\n?? 150/150 HP",
  "gameState": {
    "character": { ... },
    "gameSession": {
      "id": 1,
      "currentFloor": 1
    },
    "currentEnemy": {
      "type": "Ritter",
      "race": "Ork",
      "level": 1,
      "health": 150,
      "maxHealth": 150,
      "attackPower": 8,
      "weapon": "Schwert",
      "isBoss": false
    }
  }
}
```

---

### **Schritt 5: Angreifen**
```typescript
this.gameService.sendMessage("Ich greife an!");
```

**Response:**
```json
{
  "response": "?? Elfenkriegerin verursacht 78 Schaden!\n?? Ork Ritter verursacht 45 Schaden!",
  "gameState": {
    "character": {
      "currentHealth": 55
    },
    "currentEnemy": {
      "health": 72
    }
  }
}
```

---

### **Schritt 6: Sieg & Level Up**
```typescript
// Nach weiteren Angriffen
this.gameService.sendMessage("Nochmal angreifen!");
```

**Response bei Sieg:**
```json
{
  "response": "?? SIEG!\n?? +10 Gold\n? LEVEL UP! 1 ? 2\n\n?? Weiter zur nächsten Etage: 2",
  "gameState": {
    "character": {
      "level": 2,
      "currentHealth": 110,
      "maxHealth": 110,
      "attackPower": 14,
      "gold": 10
    },
    "gameSession": {
      "currentFloor": 2
    },
    "currentEnemy": null
  }
}
```

---

## Wichtige State-Verwaltung

### LocalStorage für Persistenz (optional)

```typescript
export class GameService {
  private readonly STORAGE_KEY = 'magicTower_gameState';

  // Speichern nach jedem Response
  private saveState() {
    const state = {
      characterId: this.characterId,
      gameSessionId: this.gameSessionId,
      conversationHistory: this.conversationHistory.slice(-10) // Nur letzte 10
    };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
  }

  // Laden beim App-Start
  loadState() {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      const state = JSON.parse(saved);
      this.characterId = state.characterId;
      this.gameSessionId = state.gameSessionId;
      this.conversationHistory = state.conversationHistory || [];
    }
  }

  // Neues Spiel starten
  resetGame() {
    this.characterId = null;
    this.gameSessionId = null;
    this.conversationHistory = [];
    localStorage.removeItem(this.STORAGE_KEY);
  }
}
```

---

## Backend-Ablauf (zur Info)

```
???????????????????????????????????????????????
?  FRONTEND                                   ?
?  POST /api/GameMcp/chat                     ?
?  { message, characterId, gameSessionId }    ?
???????????????????????????????????????????????
                    ?
                    ?
???????????????????????????????????????????????
?  BACKEND (GameMcpController)                ?
?                                             ?
?  1. Wenn characterId vorhanden aber KEINE   ?
?     gameSessionId:                          ?
?     ? Erstelle GameSession (Medium)         ?
?     ? Setze gameSessionId                   ?
?                                             ?
?  2. Sende Request an n8n mit:               ?
?     - sessionId = gameSessionId.ToString()  ?
?     - message                               ?
?     - gameState                             ?
???????????????????????????????????????????????
                    ?
                    ?
???????????????????????????????????????????????
?  N8N WORKFLOW                               ?
?  - Verwendet sessionId für Chat-History     ?
?  - Ruft Gemini AI auf                       ?
?  - AI nutzt MCP Tools                       ?
???????????????????????????????????????????????
                    ?
                    ?
???????????????????????????????????????????????
?  MCP SERVER                                 ?
?  - create_character                         ?
?  - generate_enemy                           ?
?  - execute_combat_action                    ?
?  - etc.                                     ?
???????????????????????????????????????????????
                    ?
                    ?
???????????????????????????????????????????????
?  BACKEND                                    ?
?  - Persistiert State-Changes in DB          ?
?  - Returned aktuellen gameState             ?
???????????????????????????????????????????????
                    ?
                    ?
???????????????????????????????????????????????
?  FRONTEND                                   ?
?  - Zeigt AI Response                        ?
?  - Updated UI mit gameState                 ?
?  - Speichert IDs für nächsten Request       ?
???????????????????????????????????????????????
```

---

## Vorteile dieser Lösung

? **Eine Session für alles**
- GameSession = Spiel-Zustand
- GameSession.Id = n8n Chat-History Session
- Keine separaten Session-IDs nötig

? **Automatische Initialisierung**
- Backend erstellt GameSession automatisch
- Frontend muss sich nur IDs merken

? **Persistenz in DB**
- Alle Game-Daten werden gespeichert
- Chat-History in n8n verknüpft mit GameSession.Id
- Nach Neustart kann weitergespielt werden

? **Einfaches Frontend**
- Nur 2 IDs tracken: `characterId` + `gameSessionId`
- Beide kommen aus der Response
- Keine komplexe State-Verwaltung

---

## Zusammenfassung

**Frontend braucht nur:**
1. Request an `/api/GameMcp/chat` mit `{ message, characterId?, gameSessionId? }`
2. IDs aus Response speichern (`gameState.character.id`, `gameState.gameSession.id`)
3. Bei jedem weiteren Request diese IDs mitsenden
4. UI mit `gameState` aus Response aktualisieren

**Das Backend kümmert sich um:**
- Automatische GameSession-Erstellung
- Persistierung aller Änderungen
- Kommunikation mit n8n/AI/MCP
- State-Management in DB

**Super simpel!** ??
