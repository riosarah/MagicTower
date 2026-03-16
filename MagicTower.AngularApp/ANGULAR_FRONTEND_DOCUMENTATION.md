# MagicTower - Angular Frontend Dokumentation

## Projektbeschreibung

**MagicTower** ist ein textbasiertes Adventure-Rollenspiel, bei dem Spieler einen Charakter durch einen magischen Turm führen. Das Spiel kombiniert klassische RPG-Elemente mit einem KI-gesteuerten Game Master (Gemini API via n8n).

### Kernfeatures
- 🎭 **3 Charakterklassen**: Warrior, Archer, Druid
- 🏰 **Turm-System**: 10/20/30 Stockwerke je nach Schwierigkeitsgrad
- ⚔️ **Rundenbasiertes Kampfsystem**: Normal- und Spezialangriffe
- 🏆 **Boss-Kämpfe**: Alle 5 Stockwerke
- 💎 **Belohnungssystem**: Level-Ups, Gold, Waffen
- 🗡️ **Waffen-Management**: Sammeln, Aufwerten (+1 bis +5), Verkaufen
- 💬 **KI Game Master**: Narrative Combat-Beschreibungen via Gemini API

### Technologie-Stack
- **Backend**: .NET 8.0 Web API (Port 5000)
- **Database**: PostgreSQL
- **Frontend**: Angular (geplant)
- **AI Integration**: Gemini API via n8n Webhook
- **MCP Tool**: HTTP Proxy (Port 5678)

---

## API Endpoints

**Base URL**: `http://localhost:5000/api/GameMcp`

Alle Endpoints geben folgendes Response-Format zurück:
```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  error?: string;
}
```

---

### 1. Charakter erstellen

**POST** `/api/GameMcp/create-character`

**Request Body**:
```json
{
  "name": "Aragorn",
  "characterClass": 1
}
```

**Character Classes**:
- `1` = Warrior (120 HP, 15 ATK)
- `2` = Archer (100 HP, 12 ATK)
- `3` = Druid (110 HP, 10 ATK)

**Response**:
```json
{
  "success": true,
  "message": "Charakter erfolgreich erstellt",
  "data": {
    "id": 1,
    "name": "Aragorn",
    "characterClass": 1,
    "level": 1,
    "health": 120,
    "maxHealth": 120,
    "attackPower": 15,
    "gold": 0,
    "specialAttackMultiplier": 2.0,
    "specialAttacksAvailable": 1
  }
}
```

---

### 2. Charakter abrufen

**GET** `/api/GameMcp/character/{characterId}`

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Aragorn",
    "characterClass": 1,
    "level": 5,
    "health": 160,
    "maxHealth": 160,
    "attackPower": 35,
    "gold": 250,
    "specialAttackMultiplier": 2.5,
    "specialAttacksAvailable": 1
  }
}
```

---

### 3. Alle Charaktere abrufen

**GET** `/api/GameMcp/characters`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Aragorn",
      "characterClass": 1,
      "level": 5,
      "health": 160,
      "maxHealth": 160,
      "attackPower": 35,
      "gold": 250,
      "specialAttackMultiplier": 2.5,
      "specialAttacksAvailable": 1
    }
  ]
}
```

---

### 4. Spiel-Session starten

**POST** `/api/GameMcp/start-session`

**Request Body**:
```json
{
  "characterId": 1,
  "difficulty": 10
}
```

**Difficulty Levels**:
- `10` = Easy (10 Stockwerke)
- `20` = Medium (20 Stockwerke)
- `30` = Hard (30 Stockwerke)

**Response**:
```json
{
  "success": true,
  "message": "Spiel-Session gestartet",
  "data": {
    "id": 1,
    "characterId": 1,
    "difficulty": 10,
    "currentFloor": 1,
    "maxFloor": 10,
    "isCompleted": false,
    "startedAt": "2026-01-14T10:30:00Z"
  }
}
```

---

### 5. Spiel-Session abrufen

**GET** `/api/GameMcp/session/{sessionId}`

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "characterId": 1,
    "difficulty": 10,
    "currentFloor": 5,
    "maxFloor": 10,
    "isCompleted": false,
    "startedAt": "2026-01-14T10:30:00Z",
    "completedAt": null
  }
}
```

---

### 6. Kampf starten (Gegner generieren)

**POST** `/api/GameMcp/start-fight`

**Request Body**:
```json
{
  "gameSessionId": 1,
  "characterId": 1
}
```

**Response**:
```json
{
  "success": true,
  "message": "Neuer Gegner generiert",
  "data": {
    "type": "Ork",
    "race": "Ritter",
    "level": 5,
    "health": 45,
    "maxHealth": 45,
    "attackPower": 32,
    "weapon": "Schwert",
    "isBoss": true,
    "hasSpecialAttack": true
  }
}
```

**Enemy Types**: Ork, Goblin, Troll, Untoter, Dämon, Drache, Riese, Vampir  
**Races**: Krieger, Ritter, Magier, Schamane, König, Herrscher, Titan, Fürst

---

### 7. Kampfaktion ausführen

**POST** `/api/GameMcp/execute-action`

**Request Body**:
```json
{
  "gameSessionId": 1,
  "characterId": 1,
  "useSpecialAttack": false,
  "enemy": {
    "type": "Ork",
    "race": "Ritter",
    "level": 5,
    "health": 45,
    "attackPower": 32,
    "weapon": "Schwert",
    "isBoss": true,
    "hasSpecialAttack": true
  }
}
```

**Response**:
```json
{
  "success": true,
  "message": "Kampf erfolgreich durchgeführt",
  "data": {
    "playerDamage": 35,
    "enemyDamage": 32,
    "playerUsedSpecialAttack": false,
    "enemyUsedSpecialAttack": false,
    "enemyDefeated": false,
    "playerDefeated": false,
    "updatedPlayerHealth": 128,
    "updatedEnemyHealth": 10,
    "rewards": null,
    "floorAdvanced": false,
    "gameCompleted": false
  }
}
```

**Bei Enemy-Defeat**:
```json
{
  "success": true,
  "message": "Gegner besiegt!",
  "data": {
    "playerDamage": 35,
    "enemyDamage": 0,
    "playerUsedSpecialAttack": false,
    "enemyUsedSpecialAttack": false,
    "enemyDefeated": true,
    "playerDefeated": false,
    "updatedPlayerHealth": 160,
    "updatedEnemyHealth": 0,
    "rewards": {
      "goldEarned": 50,
      "leveledUp": true,
      "newLevel": 6,
      "newWeapon": {
        "id": 5,
        "name": "Legendäres Schwert",
        "damageBonus": 15,
        "upgradeLevel": 0,
        "goldValue": 100,
        "suitableForClass": 1
      },
      "specialAttackUpgraded": true,
      "newSpecialMultiplier": 2.5
    },
    "floorAdvanced": true,
    "gameCompleted": false
  }
}
```

---

### 8. Waffen abrufen

**GET** `/api/GameMcp/weapons/{characterId}`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Schwert",
      "damageBonus": 10,
      "upgradeLevel": 2,
      "goldValue": 200,
      "suitableForClass": 1,
      "isEquipped": true
    },
    {
      "id": 2,
      "name": "Axt",
      "damageBonus": 8,
      "upgradeLevel": 0,
      "goldValue": 80,
      "suitableForClass": 1,
      "isEquipped": false
    }
  ]
}
```

---

### 9. Waffe aufwerten

**POST** `/api/GameMcp/upgrade-weapon`

**Request Body**:
```json
{
  "weaponId": 1,
  "characterId": 1
}
```

**Upgrade-Kosten**:
- Level 0 → 1: 50 Gold
- Level 1 → 2: 100 Gold
- Level 2 → 3: 200 Gold
- Level 3 → 4: 400 Gold
- Level 4 → 5: 800 Gold
- **Max Level**: 5

**Response**:
```json
{
  "success": true,
  "message": "Waffe erfolgreich aufgewertet auf Level 3",
  "data": {
    "id": 1,
    "name": "Schwert +3",
    "damageBonus": 13,
    "upgradeLevel": 3,
    "goldValue": 380,
    "suitableForClass": 1,
    "isEquipped": true
  }
}
```

---

### 10. Waffe verkaufen

**POST** `/api/GameMcp/sell-weapon`

**Request Body**:
```json
{
  "weaponId": 2,
  "characterId": 1
}
```

**Response**:
```json
{
  "success": true,
  "message": "Waffe für 80 Gold verkauft",
  "data": {
    "goldReceived": 80,
    "newGoldTotal": 330
  }
}
```

---

## Datenmodelle (TypeScript)

```typescript
export interface Character {
  id: number;
  name: string;
  characterClass: CharacterClass;
  level: number;
  health: number;
  maxHealth: number;
  attackPower: number;
  gold: number;
  specialAttackMultiplier: number;
  specialAttacksAvailable: number;
}

export enum CharacterClass {
  Warrior = 1,
  Archer = 2,
  Druid = 3
}

export interface GameSession {
  id: number;
  characterId: number;
  difficulty: Difficulty;
  currentFloor: number;
  maxFloor: number;
  isCompleted: boolean;
  startedAt: string;
  completedAt?: string;
}

export enum Difficulty {
  Easy = 10,
  Medium = 20,
  Hard = 30
}

export interface Enemy {
  type: string;
  race: string;
  level: number;
  health: number;
  maxHealth?: number;
  attackPower: number;
  weapon: string;
  isBoss: boolean;
  hasSpecialAttack: boolean;
}

export interface Weapon {
  id: number;
  name: string;
  damageBonus: number;
  upgradeLevel: number;
  goldValue: number;
  suitableForClass: CharacterClass;
  isEquipped: boolean;
}

export interface CombatResult {
  playerDamage: number;
  enemyDamage: number;
  playerUsedSpecialAttack: boolean;
  enemyUsedSpecialAttack: boolean;
  enemyDefeated: boolean;
  playerDefeated: boolean;
  updatedPlayerHealth: number;
  updatedEnemyHealth: number;
  rewards?: Rewards;
  floorAdvanced: boolean;
  gameCompleted: boolean;
}

export interface Rewards {
  goldEarned: number;
  leveledUp: boolean;
  newLevel: number;
  newWeapon?: Weapon;
  specialAttackUpgraded: boolean;
  newSpecialMultiplier: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  error?: string;
}
```

---

## Spielablauf (Workflow)

### 1. Charakter-Erstellung
```
User erstellt Charakter
    ↓
POST /create-character
    ↓
Charakter wird in DB gespeichert
    ↓
Character-ID wird zurückgegeben
```

### 2. Session-Start
```
User wählt Schwierigkeitsgrad
    ↓
POST /start-session
    ↓
Session wird erstellt (currentFloor = 1)
    ↓
Session-ID wird zurückgegeben
```

### 3. Kampf-Loop
```
POST /start-fight (generiert Gegner)
    ↓
Frontend zeigt Gegner-Stats
    ↓
User wählt Aktion (Normal/Special Attack)
    ↓
POST /execute-action
    ↓
Backend berechnet Kampfround
    ↓
Gegner noch alive?
    ├── JA → User wählt nächste Aktion
    └── NEIN → Belohnungen werden vergeben
              ↓
              Level-Up + Gold + ggf. Waffe
              ↓
              currentFloor + 1
              ↓
              Nächster Kampf oder Game Complete
```

### 4. Waffen-Management
```
GET /weapons/{characterId} (Liste aller Waffen)
    ↓
User kann:
    ├── POST /upgrade-weapon (kostet Gold)
    └── POST /sell-weapon (gibt Gold)
```

---

## UI-Komponenten (Empfehlung)

### Komponenten-Struktur
```
src/app/
├── components/
│   ├── character/
│   │   ├── character-creation/
│   │   ├── character-selection/
│   │   └── character-stats/
│   ├── combat/
│   │   ├── enemy-display/
│   │   ├── combat-actions/
│   │   └── combat-log/
│   ├── inventory/
│   │   ├── weapon-list/
│   │   └── weapon-card/
│   └── game-session/
│       ├── difficulty-selection/
│       └── floor-progress/
├── services/
│   ├── game-api.service.ts
│   ├── combat.service.ts
│   └── character.service.ts
├── models/
│   └── game.models.ts
└── pages/
    ├── home/
    ├── character-creation/
    ├── game/
    └── inventory/
```

### Key Features pro Komponente

**CharacterCreationComponent**
- Input: Name + Class-Auswahl
- POST /create-character
- Navigation zu Difficulty-Selection

**DifficultySelectionComponent**
- Radio Buttons: Easy/Medium/Hard
- POST /start-session
- Navigation zu Game-View

**CombatComponent**
- Enemy-Display mit Stats
- Action-Buttons (Attack/Special)
- Health-Bars (Player + Enemy)
- Combat-Log (Damage-Numbers)

**WeaponInventoryComponent**
- Liste aller Waffen
- Upgrade-Button (mit Kosten)
- Sell-Button
- Equipped-Indicator

**CharacterStatsComponent**
- Live-Update von HP, Level, Gold
- Spezialangriff-Status
- Progress-Bar zum nächsten Boss

---

## API Service (TypeScript)

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GameApiService {
  private baseUrl = 'http://localhost:5000/api/GameMcp';

  constructor(private http: HttpClient) {}

  // Character
  createCharacter(name: string, characterClass: CharacterClass): Observable<ApiResponse<Character>> {
    return this.http.post<ApiResponse<Character>>(`${this.baseUrl}/create-character`, {
      name, characterClass
    });
  }

  getCharacter(characterId: number): Observable<ApiResponse<Character>> {
    return this.http.get<ApiResponse<Character>>(`${this.baseUrl}/character/${characterId}`);
  }

  getCharacters(): Observable<ApiResponse<Character[]>> {
    return this.http.get<ApiResponse<Character[]>>(`${this.baseUrl}/characters`);
  }

  // Session
  startSession(characterId: number, difficulty: Difficulty): Observable<ApiResponse<GameSession>> {
    return this.http.post<ApiResponse<GameSession>>(`${this.baseUrl}/start-session`, {
      characterId, difficulty
    });
  }

  getSession(sessionId: number): Observable<ApiResponse<GameSession>> {
    return this.http.get<ApiResponse<GameSession>>(`${this.baseUrl}/session/${sessionId}`);
  }

  // Combat
  startFight(gameSessionId: number, characterId: number): Observable<ApiResponse<Enemy>> {
    return this.http.post<ApiResponse<Enemy>>(`${this.baseUrl}/start-fight`, {
      gameSessionId, characterId
    });
  }

  executeCombatAction(
    gameSessionId: number,
    characterId: number,
    useSpecialAttack: boolean,
    enemy: Enemy
  ): Observable<ApiResponse<CombatResult>> {
    return this.http.post<ApiResponse<CombatResult>>(`${this.baseUrl}/execute-action`, {
      gameSessionId, characterId, useSpecialAttack, enemy
    });
  }

  // Weapons
  getWeapons(characterId: number): Observable<ApiResponse<Weapon[]>> {
    return this.http.get<ApiResponse<Weapon[]>>(`${this.baseUrl}/weapons/${characterId}`);
  }

  upgradeWeapon(weaponId: number, characterId: number): Observable<ApiResponse<Weapon>> {
    return this.http.post<ApiResponse<Weapon>>(`${this.baseUrl}/upgrade-weapon`, {
      weaponId, characterId
    });
  }

  sellWeapon(weaponId: number, characterId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/sell-weapon`, {
      weaponId, characterId
    });
  }
}
```

---

## Assets benötigt

### Bilder
- **Character Classes**: warrior.png, archer.png, druid.png
- **Enemy Types**: ork.png, goblin.png, troll.png, untoter.png, daemon.png, drache.png, riese.png, vampir.png
- **Weapons**: schwert.png, axt.png, bogen.png, stab.png, etc.
- **Icons**: health.svg, attack.svg, gold.svg, level.svg

### Sounds (optional)
- combat_hit.mp3
- level_up.mp3
- boss_battle.mp3
- victory.mp3
- defeat.mp3

---

## CORS Configuration

⚠️ **Wichtig**: Das Backend muss CORS für `http://localhost:4200` aktiviert haben!

In `MagicTower.WebApi/Program.cs` sollte stehen:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("AllowAngular");
```

---

## Testing

### Postman Collection
Eine Postman-Collection mit allen Endpoints ist empfohlen für Backend-Testing vor Integration.

### Angular Testing
```typescript
// Beispiel: Combat Component Test
it('should execute attack and update enemy health', fakeAsync(() => {
  const mockEnemy: Enemy = { ... };
  const mockResult: CombatResult = { ... };
  
  spyOn(gameApi, 'executeCombatAction').and.returnValue(of({
    success: true,
    data: mockResult
  }));
  
  component.attack(false);
  tick();
  
  expect(component.enemy.health).toBe(mockResult.updatedEnemyHealth);
}));
```

---

## Deployment

### Development
```bash
# Backend starten
cd MagicTower.WebApi
dotnet run

# Frontend starten
cd MagicTower.AngularApp
npm start
```

### Production
- Backend: IIS / Azure App Service
- Frontend: Azure Static Web Apps / Vercel
- Database: Azure PostgreSQL

---

## Support

Bei Fragen oder Problemen:
- Backend-API: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- MCP Tool: [http://localhost:5678/swagger](http://localhost:5678/swagger)
- Project Repository: [GitHub Link]
