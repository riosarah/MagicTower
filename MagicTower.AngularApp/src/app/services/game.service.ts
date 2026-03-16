import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  error?: string;
}

export interface Character {
  id: number;
  name: string;
  characterClass: number;
  level: number;
  health: number;
  maxHealth: number;
  attackPower: number;
  gold: number;
  specialAttackMultiplier: number;
  specialAttacksAvailable: number;
}

export interface GameSession {
  id: number;
  characterId: number;
  difficulty: number;
  currentFloor: number;
  maxFloor: number;
  isCompleted: boolean;
  startedAt: string;
  completedAt?: string;
}

export interface Enemy {
  type: string;
  race: string;
  level: number;
  health: number;
  maxHealth: number;
  attackPower: number;
  weapon: string;
  isBoss: boolean;
  hasSpecialAttack: boolean;
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
  rewards?: CombatRewards;
  floorAdvanced: boolean;
  gameCompleted: boolean;
}

export interface CombatRewards {
  goldEarned: number;
  leveledUp: boolean;
  newLevel?: number;
  newWeapon?: Weapon;
  specialAttackUpgraded: boolean;
  newSpecialMultiplier?: number;
}

export interface Weapon {
  id: number;
  name: string;
  damageBonus: number;
  upgradeLevel: number;
  goldValue: number;
  suitableForClass: number;
  isEquipped: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private apiUrl = `${environment.apiBaseUrl}/api/GameMcp`;

  constructor(private http: HttpClient) {}

  // Character Management
  createCharacter(name: string, characterClass: number): Observable<ApiResponse<Character>> {
    return this.http.post<ApiResponse<Character>>(`${this.apiUrl}/character/create`, {
      name,
      characterClass
    });
  }

  getCharacter(characterId: number): Observable<ApiResponse<Character>> {
    return this.http.get<ApiResponse<Character>>(`${this.apiUrl}/character/${characterId}`);
  }

  getAllCharacters(): Observable<ApiResponse<Character[]>> {
    return this.http.get<ApiResponse<Character[]>>(`${this.apiUrl}/character/list`);
  }

  // Session Management
  getAllSessions(): Observable<GameSession[]> {
    return this.http.get<GameSession[]>(`${environment.apiBaseUrl}/api/GameSessions`);
  }

  getSessionById(sessionId: number): Observable<GameSession> {
    return this.http.get<GameSession>(`${environment.apiBaseUrl}/api/GameSessions/${sessionId}`);
  }

  getSessionsByCharacter(characterId: number): Observable<GameSession[]> {
    return this.http.get<GameSession[]>(`${this.apiUrl}/session/character/${characterId}`);
  }

  getSessionsCount(): Observable<number> {
    return this.http.get<number>(`${environment.apiBaseUrl}/api/GameSessions/count`);
  }

  startSession(characterId: number, difficulty: number): Observable<ApiResponse<GameSession>> {
    return this.http.post<ApiResponse<GameSession>>(`${this.apiUrl}/session/start`, {
      characterId,
      difficulty
    });
  }

  getSession(sessionId: number): Observable<GameSession> {
    return this.http.get<GameSession>(`${environment.apiBaseUrl}/api/GameSessions/${sessionId}`);
  }

  getSessionWithChat(sessionId: number): Observable<any> {
    return this.http.get<any>(`${environment.apiBaseUrl}/api/GameSessions/${sessionId}/with-chat`);
  }

  // Combat
  startFight(gameSessionId: number, characterId: number): Observable<ApiResponse<Enemy>> {
    return this.http.post<ApiResponse<Enemy>>(`${this.apiUrl}/fight/start`, {
      gameSessionId,
      characterId
    });
  }

  executeAction(
    gameSessionId: number,
    characterId: number,
    useSpecialAttack: boolean,
    enemy: Enemy
  ): Observable<ApiResponse<CombatResult>> {
    return this.http.post<ApiResponse<CombatResult>>(`${this.apiUrl}/fight/action`, {
      gameSessionId,
      characterId,
      useSpecialAttack,
      enemy
    });
  }

  // Weapons
  getWeapons(characterId: number): Observable<ApiResponse<Weapon[]>> {
    return this.http.get<ApiResponse<Weapon[]>>(`${this.apiUrl}/weapon/character/${characterId}`);
  }

  upgradeWeapon(weaponId: number, characterId: number): Observable<ApiResponse<Weapon>> {
    return this.http.post<ApiResponse<Weapon>>(`${this.apiUrl}/weapon/upgrade`, {
      weaponId,
      characterId
    });
  }

  sellWeapon(weaponId: number, characterId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/weapon/sell`, {
      weaponId,
      characterId
    });
  }
}
