import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { GameChatService } from '../../services/game-chat.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: false
})
export class HomeComponent implements OnInit {
  sessions: any[] = [];
  loading = false;

  constructor(
    private router: Router,
    private gameService: GameService,
    private chatService: GameChatService
  ) {}

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions(): void {
    this.loading = true;
    console.log('=== LOADING SESSIONS ===');
    console.log('Calling GET /api/GameSessions');
    
    this.gameService.getAllSessions().subscribe({
      next: (response) => {
        console.log('=== SESSIONS RESPONSE ===');
        console.log('Full Response:', response);
        console.log('Response is Array:', Array.isArray(response));
        
        // Backend gibt direkt das Array zurück, nicht ein ApiResponse Wrapper
        if (Array.isArray(response)) {
          this.sessions = response;
          console.log('Sessions assigned:', this.sessions);
          console.log('Sessions length:', this.sessions.length);
          
          // Lade Charakterinformationen für jede Session
          if (this.sessions.length > 0) {
            this.loadCharacterNames();
          } else {
            this.loading = false;
          }
        } else {
          console.warn('Response is not an array:', response);
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('=== ERROR LOADING SESSIONS ===');
        console.error('Error:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Lädt die Charakternamen für alle Sessions
   */
  loadCharacterNames(): void {
    // Erstelle Array von Character-API-Aufrufen
    const characterRequests = this.sessions.map(session => 
      this.gameService.getCharacter(session.characterId)
    );

    // Führe alle Requests parallel aus
    forkJoin(characterRequests).subscribe({
      next: (responses) => {
        // Füge Charakternamen zu den Sessions hinzu
        responses.forEach((response, index) => {
          if (response.success && response.data) {
            this.sessions[index].characterName = response.data.name;
            this.sessions[index].characterClass = response.data.characterClass;
          } else {
            this.sessions[index].characterName = 'Unbekannt';
          }
        });
        console.log('Sessions with character names:', this.sessions);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading character names:', error);
        this.loading = false;
      }
    });
  }

  startNewGame(): void {
    this.router.navigate(['/character-creation']);
  }

  continueGame(character: any): void {
    // Navigate to game session selection or directly to combat
    this.router.navigate(['/combat'], { queryParams: { characterId: character.id } });
  }

  /**
   * Startet Chat Gameplay mit bestehender Session
   */
  startChatGameplay(session: any): void {
    console.log('=== STARTING CHAT GAMEPLAY WITH EXISTING SESSION ===');
    console.log('Session:', session);

    // Lade existierende Session mit Character ID und Session ID
    this.chatService.loadExistingCharacter(session.characterId, session.id);
    
    // Navigiere zum Chat - Session wird fortgesetzt
    this.router.navigate(['/game-chat']);
  }

  /**
   * Startet neuen Chat ohne Charakter - KI erstellt alles
   */
  startNewChatGame(): void {
    console.log('=== STARTING NEW CHAT GAME (RESET) ===');
    
    // Setze Chat komplett zurück für neue Session
    this.chatService.resetChat();
    
    // Navigiere zum Chat - KI erstellt neuen Charakter und neue Session
    this.router.navigate(['/game-chat']);
  }

  getClassDisplayName(characterClass: number): string {
    switch (characterClass) {
      case 1:
        return 'Warrior';
      case 2:
        return 'Archer';
      case 3:
        return 'Druid';
      default:
        return 'Unknown';
    }
  }
}
