import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

export interface ChatRequest {
  message: string;
  characterId?: number;
  gameSessionId?: number;
  conversationHistory: ChatMessage[];
}

export interface ChatResponse {
  response: string;
  gameState?: GameStateDto;
}

export interface GameStateDto {
  character?: any;
  gameSession?: any;
  currentEnemy?: any;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class GameChatService {
  private baseUrl = `${environment.apiBaseUrl}/api/GameMcp`;
  private maxHistoryLength = 10;

  // Observables für Echtzeit-Updates
  private conversationHistorySubject = new BehaviorSubject<ChatMessage[]>([]);
  public conversationHistory$ = this.conversationHistorySubject.asObservable();

  private gameStateSubject = new BehaviorSubject<GameStateDto | null>(null);
  public gameState$ = this.gameStateSubject.asObservable();

  private characterIdSubject = new BehaviorSubject<number | null>(null);
  private gameSessionIdSubject = new BehaviorSubject<number | null>(null);

  constructor(private http: HttpClient) {}

  /**
   * Sendet User-Nachricht an Chat-Endpoint
   */
  sendMessage(message: string): Observable<ApiResponse<ChatResponse>> {
    console.log('=== SENDING CHAT MESSAGE ===');
    console.log('Message:', message);
    console.log('Current Character ID:', this.characterIdSubject.value);
    console.log('Current Session ID:', this.gameSessionIdSubject.value);

    const userMessage: ChatMessage = {
      role: 'user',
      content: message,
      timestamp: new Date()
    };

    // User-Nachricht zur History hinzufügen
    this.addMessageToHistory(userMessage);

    const request: ChatRequest = {
      message: message,
      characterId: this.characterIdSubject.value ?? undefined,
      gameSessionId: this.gameSessionIdSubject.value ?? undefined,
      conversationHistory: this.conversationHistorySubject.value.map(msg => ({
        role: msg.role,
        content: msg.content,
        timestamp: msg.timestamp
      }))
    };

    console.log('Chat Request:', request);

    return this.http.post<ApiResponse<ChatResponse>>(
      `${this.baseUrl}/chat`,
      request
    ).pipe(
      tap(response => {
        console.log('=== SERVICE RECEIVED RESPONSE ===');
        console.log('Full Response:', JSON.stringify(response, null, 2));
        console.log('Response.success:', response.success);
        console.log('Response.data:', response.data);
        console.log('Response.data.response TYPE:', typeof response.data?.response);
        console.log('Response.data.response VALUE:', response.data?.response);
        console.log('Response.data.gameState:', response.data?.gameState);
        
        if (response.success && response.data) {
          // Prüfe ob response ein String oder ein Objekt ist
          let messageContent: string;
          if (typeof response.data.response === 'string') {
            messageContent = response.data.response;
          } else if (typeof response.data.response === 'object') {
            console.warn('WARNING: response.data.response is an object, converting to JSON string');
            messageContent = JSON.stringify(response.data.response, null, 2);
          } else {
            console.error('ERROR: response.data.response has unexpected type:', typeof response.data.response);
            messageContent = 'Unerwartete Response-Struktur';
          }
          
          console.log('Final message content to add:', messageContent);
          
          // AI-Antwort zur History hinzufügen
          const aiMessage: ChatMessage = {
            role: 'assistant',
            content: messageContent,
            timestamp: new Date()
          };
          this.addMessageToHistory(aiMessage);

          // Game State aktualisieren
          if (response.data.gameState) {
            console.log('Updating Game State:', response.data.gameState);
            this.gameStateSubject.next(response.data.gameState);

            // Character ID & Session ID extrahieren
            if (response.data.gameState.character) {
              const charId = response.data.gameState.character.id;
              console.log('Setting Character ID:', charId);
              this.characterIdSubject.next(charId);
            }
            if (response.data.gameState.gameSession) {
              const sessId = response.data.gameState.gameSession.id;
              console.log('Setting Session ID:', sessId);
              this.gameSessionIdSubject.next(sessId);
            }
          }
        }
      })
    );
  }

  /**
   * Fügt Nachricht zur Conversation History hinzu (max 10 Nachrichten)
   */
  private addMessageToHistory(message: ChatMessage): void {
    const currentHistory = this.conversationHistorySubject.value;
    const updatedHistory = [...currentHistory, message];

    // Nur die letzten 10 Nachrichten behalten
    if (updatedHistory.length > this.maxHistoryLength) {
      updatedHistory.shift();
    }

    this.conversationHistorySubject.next(updatedHistory);
  }

  /**
   * Setzt den Chat zurück (für neues Spiel)
   */
  resetChat(): void {
    console.log('=== RESET CHAT ===');
    this.conversationHistorySubject.next([]);
    this.gameStateSubject.next(null);
    this.characterIdSubject.next(null);
    this.gameSessionIdSubject.next(null);
  }

  /**
   * Gibt aktuelle Character-ID zurück
   */
  getCurrentCharacterId(): number | null {
    return this.characterIdSubject.value;
  }

  /**
   * Gibt aktuelle Session-ID zurück
   */
  getCurrentSessionId(): number | null {
    return this.gameSessionIdSubject.value;
  }

  /**
   * Gibt aktuelle Character ID zurück (Alias)
   */
  getCharacterId(): number | null {
    return this.characterIdSubject.value;
  }

  /**
   * Gibt aktuelle Session ID zurück (Alias)
   */
  getSessionId(): number | null {
    return this.gameSessionIdSubject.value;
  }

  /**
   * Setzt Character ID manuell
   */
  setCharacterId(characterId: number): void {
    console.log('=== SETTING CHARACTER ID ===', characterId);
    this.characterIdSubject.next(characterId);
  }

  /**
   * Lädt existierenden Character und Session mit ChatMessages
   */
  loadExistingCharacter(characterId: number, sessionId?: number): void {
    console.log('=== LOAD EXISTING CHARACTER ===');
    console.log('Character ID:', characterId);
    console.log('Session ID:', sessionId);
    
    this.characterIdSubject.next(characterId);
    
    if (sessionId) {
      this.gameSessionIdSubject.next(sessionId);
      // Lade Session mit ChatMessages vom Backend
      this.loadSessionWithChat(sessionId);
    }
  }

  /**
   * Lädt Session mit ChatMessages, Character und Enemy vom Backend
   */
  private loadSessionWithChat(sessionId: number): void {
    console.log('=== LOADING SESSION WITH CHAT ===');
    console.log('Session ID:', sessionId);

    // Import GameService dynamisch, um zirkuläre Abhängigkeit zu vermeiden
    const gameServiceUrl = `${environment.apiBaseUrl}/api/GameSessions/${sessionId}/with-chat`;
    
    this.http.get<any>(gameServiceUrl).subscribe({
      next: (response) => {
        console.log('=== SESSION WITH CHAT LOADED ===');
        console.log('Response:', response);

        // ChatMessages wiederherstellen
        if (response.chatMessages && Array.isArray(response.chatMessages)) {
          const chatHistory: ChatMessage[] = response.chatMessages.map((msg: any) => ({
            role: msg.role === 'User' || msg.role === 'user' ? 'user' : 'assistant',
            content: msg.content,
            timestamp: new Date(msg.timestamp)
          }));
          
          console.log('Restored Chat History:', chatHistory);
          this.conversationHistorySubject.next(chatHistory);
        }

        // GameState wiederherstellen
        const gameState: GameStateDto = {
          character: response.character,
          gameSession: {
            id: response.id,
            characterId: response.characterId,
            difficulty: response.difficulty,
            currentFloor: response.currentFloor,
            maxFloor: response.maxFloor,
            isCompleted: response.isCompleted,
            startedAt: response.startedAt,
            completedAt: response.completedAt
          },
          currentEnemy: response.currentEnemyState ? JSON.parse(response.currentEnemyState) : null
        };

        console.log('Restored Game State:', gameState);
        this.gameStateSubject.next(gameState);

        // Character ID & Session ID setzen
        if (gameState.character?.id) {
          this.characterIdSubject.next(gameState.character.id);
        }
        if (gameState.gameSession?.id) {
          this.gameSessionIdSubject.next(gameState.gameSession.id);
        }
      },
      error: (error) => {
        console.error('=== ERROR LOADING SESSION WITH CHAT ===');
        console.error('Error:', error);
      }
    });
  }
}
