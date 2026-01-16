# AI Game Master Guide für MagicTower MCP Integration

## Übersicht

Dieses Dokument beschreibt, wie die KI als Game Master fungiert und den gesamten Spielprozess über MCP Tools steuert.

## System Prompt für Gemini AI

```
Du bist der Game Master für das MagicTower Rollenspiel. Deine Aufgabe ist es, ein spannendes und immersives Spielerlebnis zu schaffen.

### Deine Verantwortlichkeiten:
1. **Charakter Management**: Erstelle und verwalte Charaktere basierend auf Spielerwünschen
2. **Spiel-Steuerung**: Starte und verwalte Spielsessions mit passender Schwierigkeit
3. **Kampf-Koordination**: Generiere Gegner und führe Kampfrunden durch
4. **Story-Telling**: Erzähle eine fesselnde Geschichte mit atmosphärischen Beschreibungen
5. **State Tracking**: Verfolge IMMER den aktuellen Spielzustand und gib ihn strukturiert zurück

### Verfügbare Tools:
- `create_character`: Erstellt einen neuen Charakter (Warrior/Archer/Druid)
- `start_game_session`: Startet eine neue Session mit Schwierigkeit (10/20/30)
- `generate_enemy`: Generiert einen Gegner für die aktuelle Etage
- `execute_combat_action`: Führt eine Kampfaktion aus
- `heal_character`: Heilt den Charakter gegen Gold
- `buy_special_attacks`: Kauft Spezialattacken gegen Gold

### WICHTIG: Game State Tracking

Nach JEDER Tool-Verwendung MUSST du den aktuellen Spielzustand strukturiert zurückgeben:

```json
{
  "character": {
    "id": null,  // wird von Backend gesetzt
    "name": "Charaktername",
    "characterClass": 1,  // 1=Warrior, 2=Archer, 3=Druid
    "level": 1,
    "maxHealth": 120,
    "currentHealth": 120,
    "attackPower": 15,
    "gold": 0,
    "specialAttackLevel": 1
  },
  "gameSession": {
    "id": null,  // wird von Backend gesetzt
    "characterId": null,  // wird von Backend gesetzt
    "difficulty": 10,  // 10=Easy, 20=Medium, 30=Hard
    "currentFloor": 1,
    "isCompleted": false
  },
  "currentEnemy": {
    "type": "Ritter",
    "race": "Ork",
    "level": 1,
    "health": 150,
    "maxHealth": 150,
    "attackPower": 8,
    "weapon": "Schwert",
    "isBoss": false,
    "hasSpecialAttack": false
  },
  "actionsPerformed": ["create_character", "start_game_session"]
}
```

### Spielablauf:

1. **Charaktererstellung**
   ```
   Spieler: "Ich möchte einen Krieger namens Thorin erstellen"
   Du: Rufe create_character("Thorin", 1) auf
   Du: Gib den Character State zurück und begrüße den Spieler
   ```

2. **Session Start**
   ```
   Spieler: "Lass uns auf mittlerer Schwierigkeit spielen"
   Du: Rufe start_game_session(20, "Thorin", 1) auf
   Du: Gib den Game Session State zurück
   ```

3. **Kampf**
   ```
   Du: Rufe generate_enemy(charLevel, currentFloor) auf
   Du: Beschreibe den Gegner atmosphärisch
   Du: Gib den Enemy State zurück
   
   Spieler: "Ich greife an!"
   Du: Rufe execute_combat_action(...) auf mit allen Parametern
   Du: Beschreibe den Kampf spannend
   Du: Gib den aktualisierten State zurück (Health, Gold, Level, Floor)
   ```

4. **Nach Sieg**
   ```
   Du: Aktualisiere currentFloor +1
   Du: Setze currentEnemy auf null
   Du: Frage ob weiter gespielt werden soll
   ```

### Beispiel-Konversation:

**Spieler**: "Ich möchte ein Spiel starten"

**Du** (intern):
1. Tool: create_character("Hero", 1)
2. Tool: start_game_session(10, "Hero", 1)
3. Tool: generate_enemy(1, 1)

**Du** (Antwort):
```
?? Willkommen in MagicTower! ??

Dein Krieger "Hero" wurde erschaffen!
?? Level 1 | ?? 120/120 HP | ?? 15 ATK | ?? 0 Gold

Die Reise beginnt auf Schwierigkeit Easy (10 Etagen).

**Etage 1**
Du betrittst einen dunklen Raum. Plötzlich springt ein Ork Ritter mit seinem Schwert aus dem Schatten!

?? Ork Ritter (Level 1)
?? 150 HP | ?? 8 ATK | ??? Schwert

Was möchtest du tun?
- Angreifen
- Spezialattacke (3 verfügbar)
- Heilen (kostet 20 Gold)
```

**State zurückgeben**:
```json
{
  "character": {
    "name": "Hero",
    "characterClass": 1,
    "level": 1,
    "maxHealth": 120,
    "currentHealth": 120,
    "attackPower": 15,
    "gold": 0,
    "specialAttackLevel": 1
  },
  "gameSession": {
    "difficulty": 10,
    "currentFloor": 1,
    "isCompleted": false
  },
  "currentEnemy": {
    "type": "Ritter",
    "race": "Ork",
    "level": 1,
    "health": 150,
    "maxHealth": 150,
    "attackPower": 8,
    "weapon": "Schwert",
    "isBoss": false,
    "hasSpecialAttack": false
  },
  "actionsPerformed": ["create_character", "start_game_session", "generate_enemy"]
}
```

### Regeln:
1. ? IMMER den vollständigen Game State nach Tool-Nutzung zurückgeben
2. ? Berechne Health, Gold, Level korrekt nach jedem Kampf
3. ? Erhöhe currentFloor nach jedem Sieg
4. ? Setze currentEnemy auf null wenn kein Kampf aktiv
5. ? Beschreibe Kämpfe atmosphärisch und spannend
6. ? Boss-Kämpfe (alle 5 Etagen) besonders hervorheben
7. ? Spieler-Entscheidungen respektieren
8. ? NIEMALS Spielwerte erfinden - nutze nur Tool-Ergebnisse
9. ? NIEMALS den State vergessen zurückzugeben

### Schwierigkeitsgrade:
- **10 (Easy)**: 10 Etagen, für Anfänger
- **20 (Medium)**: 20 Etagen, ausgewogen
- **30 (Hard)**: 30 Etagen, für Experten

### Boss-Kämpfe:
- Jede 5. Etage (5, 10, 15, 20, 25, 30)
- 1.5x stärkere Stats
- Hat IMMER Spezialattacke
- Höhere Belohnungen

### Belohnungen nach Sieg:
- **Gold**: 10-50 (normal), 50-150 (Boss)
- **Level Up**: 40% Chance ? +10 HP, +2 ATK
- **Heilung**: +3 Spezialattacken
- **Waffen**: 20% (normal), 60% (Boss)

Sei kreativ, spannend und fair! Viel Erfolg als Game Master! ??
```

## n8n Workflow Struktur

### Node 1: Webhook (Eingang)
- Empfängt Chat-Anfrage vom Frontend
- Payload: `{ message, characterId?, gameSessionId?, conversationHistory }`

### Node 2: Load Game State (Code Node)
```javascript
// Lade aktuellen Game State aus Datenbank wenn IDs vorhanden
const characterId = $json.characterId;
const gameSessionId = $json.gameSessionId;

let gameState = null;

if (characterId) {
  // HTTP Request an WebApi: GET /api/GameMcp/character/{characterId}
  // HTTP Request an WebApi: GET /api/GameMcp/session/{gameSessionId}
  // Kombiniere zu gameState
}

return {
  message: $json.message,
  conversationHistory: $json.conversationHistory || [],
  currentGameState: gameState
};
```

### Node 3: Gemini AI (MCP Tools aktiviert)
- Model: gemini-1.5-pro
- System Prompt: (siehe oben)
- Input: message + currentGameState + conversationHistory
- MCP Server URL: `http://localhost:5292/mcp`
- Output: AI Response + gameStateUpdate

### Node 4: Parse AI Response (Code Node)
```javascript
// Extrahiere Game State Update aus AI Response
const aiResponse = $json.response;

// Suche nach JSON-Block in der Antwort
const jsonMatch = aiResponse.match(/```json\n([\s\S]*?)\n```/);
let gameStateUpdate = null;

if (jsonMatch) {
  try {
    gameStateUpdate = JSON.parse(jsonMatch[1]);
  } catch (e) {
    console.error('Failed to parse game state:', e);
  }
}

return {
  aiResponseText: aiResponse.replace(/```json[\s\S]*?```/g, '').trim(),
  gameStateUpdate: gameStateUpdate
};
```

### Node 5: Persist Game State (Code Node)
```javascript
const gameStateUpdate = $json.gameStateUpdate;
const characterId = $input.first().json.characterId;
const gameSessionId = $input.first().json.gameSessionId;

let persistedData = {
  characterId: characterId,
  gameSessionId: gameSessionId
};

if (gameStateUpdate) {
  // Wenn Character Update vorhanden
  if (gameStateUpdate.character) {
    if (!characterId) {
      // POST /api/GameMcp/character/create
      const createResponse = await $http.post('/api/GameMcp/character/create', {
        name: gameStateUpdate.character.name,
        characterClass: gameStateUpdate.character.characterClass
      });
      persistedData.characterId = createResponse.data.id;
    } else {
      // PUT /api/GameMcp/character/{characterId}/update
      await $http.put(`/api/GameMcp/character/${characterId}/update`, 
        gameStateUpdate.character
      );
    }
  }
  
  // Wenn Session Update vorhanden
  if (gameStateUpdate.gameSession) {
    if (!gameSessionId) {
      // POST /api/GameMcp/session/start
      const sessionResponse = await $http.post('/api/GameMcp/session/start', {
        characterId: persistedData.characterId,
        difficulty: gameStateUpdate.gameSession.difficulty
      });
      persistedData.gameSessionId = sessionResponse.data.id;
    } else {
      // PUT /api/GameMcp/session/{gameSessionId}/update
      await $http.put(`/api/GameMcp/session/${gameSessionId}/update`,
        gameStateUpdate.gameSession
      );
    }
  }
}

return persistedData;
```

### Node 6: Response (Webhook Response)
```javascript
return {
  response: $json.aiResponseText,
  characterId: $json.characterId,
  gameSessionId: $json.gameSessionId,
  gameState: $json.gameStateUpdate
};
```

## Backend: Update Endpoints

Die WebApi benötigt zusätzliche Update-Endpoints:

### PUT /api/GameMcp/character/{id}/update
```csharp
[HttpPut("character/{id}/update")]
public async Task<ActionResult<ApiResponse<CharacterDto>>> UpdateCharacter(
    int id, 
    [FromBody] CharacterUpdateDto update)
{
    // Update nur die Felder die nicht null sind
    var character = await _characterSet.GetByIdAsync(id);
    
    if (update.CurrentHealth.HasValue) 
        character.CurrentHealth = update.CurrentHealth.Value;
    if (update.Level.HasValue) 
        character.Level = update.Level.Value;
    if (update.Gold.HasValue) 
        character.Gold = update.Gold.Value;
    // ... weitere Felder
    
    await _characterSet.UpdateAsync(character);
    return Ok(...);
}
```

### PUT /api/GameMcp/session/{id}/update
```csharp
[HttpPut("session/{id}/update")]
public async Task<ActionResult<ApiResponse<GameSessionDto>>> UpdateGameSession(
    int id,
    [FromBody] GameSessionUpdateDto update)
{
    var session = await _sessionSet.GetByIdAsync(id);
    
    if (update.CurrentFloor.HasValue)
        session.CurrentFloor = update.CurrentFloor.Value;
    if (update.IsCompleted.HasValue)
        session.IsCompleted = update.IsCompleted.Value;
    
    await _sessionSet.UpdateAsync(session);
    return Ok(...);
}
```

## Testing

### Test 1: Charakter erstellen
```bash
curl -X POST http://localhost:5000/api/GameMcp/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Erstelle einen Krieger namens Thor"}'
```

### Test 2: Spiel starten
```bash
curl -X POST http://localhost:5000/api/GameMcp/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Starte ein Spiel auf mittlerer Schwierigkeit",
    "characterId": 1
  }'
```

### Test 3: Kampf
```bash
curl -X POST http://localhost:5000/api/GameMcp/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Ich greife an!",
    "characterId": 1,
    "gameSessionId": 1
  }'
```

## Troubleshooting

### Problem: AI gibt keinen State zurück
- **Lösung**: System Prompt überprüfen und betonen dass State IMMER zurückgegeben werden muss

### Problem: State wird nicht persistiert
- **Lösung**: Code Node Logs prüfen, HTTP Requests validieren

### Problem: Charaktere werden dupliziert
- **Lösung**: Prüfen ob characterId korrekt weitergegeben wird

## Zusammenfassung

1. ? KI steuert den gesamten Spielprozess
2. ? Alle Werte werden strukturiert hin und her gesendet
3. ? Backend persistiert State-Changes automatisch
4. ? Frontend erhält immer aktuellen Game State
5. ? Kein manuelles State-Management im Frontend nötig
