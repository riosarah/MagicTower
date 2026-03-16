import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { Router } from '@angular/router';
import { GameChatService, ChatMessage, GameStateDto } from '../../services/game-chat.service';

@Component({
  selector: 'app-game-chat',
  templateUrl: './game-chat.component.html',
  styleUrls: ['./game-chat.component.css'],
  standalone: false
})
export class GameChatComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('chatMessages') private chatMessagesContainer!: ElementRef;
  
  conversationHistory: ChatMessage[] = [];
  gameState: GameStateDto | null = null;
  userInput: string = '';
  currentMessage: string = '';
  isLoading: boolean = false;
  private shouldScroll = false;

  // Text-to-Speech Properties
  private speechSynthesis: SpeechSynthesis;
  private currentUtterance: SpeechSynthesisUtterance | null = null;
  speakingMessageIndex: number = -1; // Index der aktuell vorgelesenen Nachricht

  // Retro Game Design Properties
  showZorroPopup: boolean = true; // Zorro beim Start zeigen
  zorroMessage: string = 'Ich bin Zorro, dein magischer Begleiter!'; // Aktuelle Zorro Nachricht
  backgroundImage: string = '';
  sessionTowerBackground: string = ''; // Turm-Hintergrund für die gesamte Session
  currentFloor: number = 0; // Aktuelle Etage
  sessionDungeonBackgrounds: Map<number, string> = new Map(); // Dungeon-Hintergründe pro Etage
  characterImage: string = '';
  enemyImage: string = '';
  zorroImage: string = 'assets/images/Zorro.png';

  // Quick Actions
  quickActions = [
    { label: 'Angreifen', message: 'Ich greife an!' },
    { label: 'Spezialangriff', message: 'Ich nutze meinen Spezialangriff!' },
    { label: 'Status', message: 'Zeige mir meinen aktuellen Status' },
    { label: 'Nächstes Stockwerk', message: 'Ich gehe zum nächsten Stockwerk' }
  ];

  constructor(
    private chatService: GameChatService,
    private router: Router
  ) {
    this.speechSynthesis = window.speechSynthesis;
  }

  ngOnInit(): void {
    // Initialer random Tower Hintergrund
    this.setRandomTowerBackground();

    // Conversation History abonnieren
    this.chatService.conversationHistory$.subscribe(history => {
      this.conversationHistory = history;
      this.shouldScroll = true;
      
      // Zorro popup bei neuer AI-Nachricht mit aktueller Nachricht
      if (history.length > 0 && history[history.length - 1].role === 'assistant') {
        const lastMessage = history[history.length - 1].content;
        this.zorroMessage = lastMessage;
        this.showZorroPopupTemporary();
      }
    });

    // Game State abonnieren
    this.chatService.gameState$.subscribe(state => {
      this.gameState = state;
      console.log('Game State Updated:', state);
      
      // Character Image aktualisieren
      if (state?.character) {
        this.updateCharacterImage(state.character.className);
      }
      
      // Enemy Image aktualisieren
      if (state?.currentEnemy) {
        this.updateEnemyImage(state.currentEnemy);
        // Dungeon Hintergrund wenn Feind vorhanden
        // Prüfe ob sich die Etage geändert hat
        const newFloor = state.gameSession?.currentFloor || 1;
        if (this.currentFloor !== newFloor) {
          this.currentFloor = newFloor;
          this.setRandomDungeonBackground(newFloor);
        } else {
          // Etage ist gleich, setze existierenden Dungeon-Hintergrund
          this.setExistingDungeonBackground(newFloor);
        }
      } else {
        this.enemyImage = '';
        // Tower Hintergrund wenn kein Feind
        this.setRandomTowerBackground();
      }
    });

    // Prüfen ob Character & Session vorhanden sind
    this.checkAndInitializeSession();
  }

  /**
   * Prüft ob Charakter vorhanden ist und zeigt passende Willkommensnachricht
   */
  private checkAndInitializeSession(): void {
    const characterId = this.chatService.getCharacterId();
    const sessionId = this.chatService.getSessionId();

    console.log('=== CHECKING SESSION ===');
    console.log('Character ID:', characterId);
    console.log('Session ID:', sessionId);

    if (this.conversationHistory.length === 0) {
      if (characterId) {
        // Charakter vorhanden - KI wird Session starten
        this.addSystemMessage(
          `Willkommen zurück!\n\n` +
          `Charakter ID ${characterId} geladen.\n\n` +
          `Schreibe z.B.: "Starte ein Abenteuer auf mittlerer Schwierigkeit" oder "Zeige meinen Status"`
        );
      } else {
        // Kein Charakter - KI erstellt alles
        this.addSystemMessage(
          `Willkommen beim magischen Turm! Ich bin Zorro dein magischer Begleiter.\n` +
          `Ich werde dir helfen dich durch die zahlreichen Ebenen zu kämpfen und Ruhm und Sieg zu erlangen.\n` +
          `Was für ein Abenteurer bist du?\n` + 
          `Ein Krieger, ein Schütze oder gar Druide?\n` +
          `Stell dich schnell vor, denn ich höre schon Schritte.\n` 
        );
      }
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  sendMessage(message?: string): void {
    const messageToSend = message || this.currentMessage.trim();
    if (!messageToSend) return;

    this.isLoading = true;
    this.chatService.sendMessage(messageToSend).subscribe({
      next: (response) => {
        console.log('=== COMPONENT RECEIVED RESPONSE ===');
        console.log('Full Response:', response);
        console.log('Success:', response.success);
        console.log('Data:', response.data);
        
        if (!response.success) {
          console.error('Response not successful:', response.message || response.error);
          this.addSystemMessage(`Fehler: ${response.message || response.error || 'Unbekannter Fehler'}`);
        } else {
          console.log('Response successful - AI message should be in conversation history now');
          console.log('Game State from response:', response.data?.gameState);
        }
        
        this.currentMessage = '';
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Chat error:', error);
        this.addSystemMessage('Verbindungsfehler zum Server. Bitte versuche es erneut.');
        this.isLoading = false;
      }
    });
  }

  startNewGame(): void {
    this.chatService.resetChat();
    this.addSystemMessage(
      'Neues Abenteuer gestartet!\n\n' +
      'Stell mir deinen Charakter vor!\n\n' +
      'Nenne mir deinen Namen und deine Klasse (Krieger, Schütze, Druide)'
    );
  }

  useQuickAction(action: any): void {
    this.sendMessage(action.message);
  }

  goToHome(): void {
    this.router.navigate(['/home']);
  }

  private addSystemMessage(content: string): void {
    const systemMessage: ChatMessage = {
      role: 'assistant',
      content: content,
      timestamp: new Date()
    };
    this.conversationHistory = [...this.conversationHistory, systemMessage];
    this.shouldScroll = true;
  }

  private scrollToBottom(): void {
    try {
      if (this.chatMessagesContainer) {
        this.chatMessagesContainer.nativeElement.scrollTop = 
          this.chatMessagesContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Scroll error:', err);
    }
  }

  getHealthPercentage(current: number, max: number): number {
    return (current / max) * 100;
  }

  getHealthBarClass(current: number, max: number): string {
    const percentage = this.getHealthPercentage(current, max);
    if (percentage > 60) return 'bg-success';
    if (percentage > 30) return 'bg-warning';
    return 'bg-danger';
  }

  getDifficultyLabel(difficulty: number | undefined): string {
    if (difficulty === 10) return 'Einfach';
    if (difficulty === 20) return 'Mittel';
    return 'Schwer';
  }

  getClassNameInGerman(className: string): string {
    const translations: { [key: string]: string } = {
      'Warrior': 'Krieger',
      'Druid': 'Druide',
      'Archer': 'Bogenschütze'
    };
    return translations[className] || className;
  }

  getTowerProgress(): number {
    if (!this.gameState?.gameSession) return 0;
    return (this.gameState.gameSession.currentFloor / this.gameState.gameSession.maxFloor) * 100;
  }

  // === RETRO GAME DESIGN METHODS ===

  /**
   * Zeigt Zorro-Popup für 2 Sekunden
   */
  private showZorroPopupTemporary(): void {
    this.showZorroPopup = true;
    setTimeout(() => {
      this.showZorroPopup = false;
    }, 8000);
  }

  /**
   * Versteckt Zorro-Popup (z.B. nach erster User-Nachricht)
   */
  hideZorroOnFirstUserMessage(): void {
    if (this.conversationHistory.some(msg => msg.role === 'user')) {
      this.showZorroPopup = false;
    }
  }

  /**
   * Aktualisiert Character Image basierend auf className
   */
  private updateCharacterImage(className: string): void {
    const imageMap: { [key: string]: string } = {
      'Warrior': 'assets/images/Warrior.png',
      'Archer': 'assets/images/Archer.png',
      'Druid': 'assets/images/Druid.png'
    };
    this.characterImage = imageMap[className] || '';
  }

  /**
   * Aktualisiert Enemy Image basierend auf imageKey/type/race
   */
  private updateEnemyImage(enemy: any): void {
    const imageMap: { [key: string]: string } = {
      'Ork': 'assets/images/enemies/Ork.png',
      'Goblin': 'assets/images/enemies/Goblin.png',
      'Troll': 'assets/images/enemies/Troll.png',
      'Wolf': 'assets/images/enemies/Wolf.png',
      'Drache': 'assets/images/enemies/Dragon.png',
      'Riese': 'assets/images/enemies/Giant.png',
      'Hydra': 'assets/images/enemies/Hydra.png'
    };

    const normalize = (value: any): string =>
      String(value ?? '').trim().toLowerCase();

    const imageKey = normalize(enemy?.imageKey);
    const type = normalize(enemy?.type);
    const race = normalize(enemy?.race);

    const imageByKeyTokenMap: { [key: string]: string } = {
      wolf: imageMap['Wolf'],
      troll: imageMap['Troll'],
      goblin: imageMap['Goblin'],
      ork: imageMap['Ork'],
      orc: imageMap['Ork'],
      drache: imageMap['Drache'],
      dragon: imageMap['Drache'],
      riese: imageMap['Riese'],
      giant: imageMap['Riese'],
      hydra: imageMap['Hydra']
    };

    let resolvedImage = '';

    if (imageKey) {
      const keyTokens = imageKey.split(/[_\s-]+/);
      const matchedToken = keyTokens.find(token => imageByKeyTokenMap[token]);
      if (matchedToken) {
        resolvedImage = imageByKeyTokenMap[matchedToken];
      }
    }

    if (!resolvedImage) {
      resolvedImage = imageByKeyTokenMap[type] || imageByKeyTokenMap[race] || '';
    }

    this.enemyImage = resolvedImage;
  }

  /**
   * Setzt random Tower Hintergrund (nur einmal pro Session)
   */
  private setRandomTowerBackground(): void {
    // Wenn bereits ein Tower-Hintergrund für diese Session gewählt wurde, verwende ihn
    if (!this.sessionTowerBackground) {
      const towerCount = 3;
      const randomTower = Math.floor(Math.random() * towerCount) + 1;
      this.sessionTowerBackground = `assets/images/towers/tower${randomTower}.jpg`;
    }
    
    this.backgroundImage = this.sessionTowerBackground;
    // Setze Hintergrund für die gesamte Seite (Body)
    document.body.style.backgroundImage = `url('${this.backgroundImage}')`;
    document.body.style.backgroundSize = 'cover';
    document.body.style.backgroundPosition = 'center';
    document.body.style.backgroundRepeat = 'no-repeat';
    document.body.style.backgroundAttachment = 'fixed';
  }

  /**
   * Setzt random Dungeon Hintergrund (nur wenn Etage neu ist)
   */
  private setRandomDungeonBackground(floor: number): void {
    // Wenn für diese Etage bereits ein Hintergrund gewählt wurde, verwende ihn
    if (this.sessionDungeonBackgrounds.has(floor)) {
      this.backgroundImage = this.sessionDungeonBackgrounds.get(floor)!;
    } else {
      // Wähle neuen zufälligen Dungeon-Hintergrund für diese Etage
      const dungeonCount = 15;
      const randomDungeon = Math.floor(Math.random() * dungeonCount) + 1;
      this.backgroundImage = `assets/images/dungeons/dungeon${randomDungeon}.jpg`;
      // Speichere Hintergrund für diese Etage
      this.sessionDungeonBackgrounds.set(floor, this.backgroundImage);
    }
    
    // Setze Hintergrund für die gesamte Seite (Body)
    document.body.style.backgroundImage = `url('${this.backgroundImage}')`;
    document.body.style.backgroundSize = 'cover';
    document.body.style.backgroundPosition = 'center';
    document.body.style.backgroundRepeat = 'no-repeat';
    document.body.style.backgroundAttachment = 'fixed';
  }

  /**
   * Setzt existierenden Dungeon Hintergrund ohne neue Auswahl
   */
  private setExistingDungeonBackground(floor: number): void {
    if (this.sessionDungeonBackgrounds.has(floor)) {
      this.backgroundImage = this.sessionDungeonBackgrounds.get(floor)!;
      // Setze Hintergrund für die gesamte Seite (Body)
      document.body.style.backgroundImage = `url('${this.backgroundImage}')`;
      document.body.style.backgroundSize = 'cover';
      document.body.style.backgroundPosition = 'center';
      document.body.style.backgroundRepeat = 'no-repeat';
      document.body.style.backgroundAttachment = 'fixed';
    }
  }

  /**
   * Cleanup beim Verlassen der Komponente
   */
  ngOnDestroy(): void {
    // Stoppe Text-to-Speech
    this.stopSpeaking();
    
    // Entferne Body-Hintergrund beim Verlassen der Komponente
    document.body.style.backgroundImage = '';
    document.body.style.backgroundSize = '';
    document.body.style.backgroundPosition = '';
    document.body.style.backgroundRepeat = '';
    document.body.style.backgroundAttachment = '';
  }

  /**
   * Liest eine Nachricht mit Text-to-Speech vor
   */
  speakMessage(message: string, messageIndex: number): void {
    // Wenn bereits diese Nachricht vorgelesen wird, stoppe sie
    if (this.speakingMessageIndex === messageIndex) {
      this.stopSpeaking();
      return;
    }

    // Stoppe vorherige Wiedergabe
    this.stopSpeaking();

    // Entferne HTML-Tags aus der Nachricht
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = message;
    const textContent = tempDiv.textContent || tempDiv.innerText || '';

    // Erstelle neue Utterance
    this.currentUtterance = new SpeechSynthesisUtterance(textContent);
    this.currentUtterance.lang = 'de-DE'; // Deutsche Stimme
    this.currentUtterance.rate = 1.0; // Normale Geschwindigkeit
    this.currentUtterance.pitch = 1.0; // Normale Tonhöhe

    // Wähle spezifische Stimme: Microsoft Michael (primär), Microsoft Stefan (fallback)
    const voices = this.speechSynthesis.getVoices();
    
    // Suche nach Microsoft Michael (bevorzugt)
    let selectedVoice = voices.find(voice => 
      voice.name.includes('Microsoft Michael')
    );
    
    // Fallback: Microsoft Stefan
    if (!selectedVoice) {
      selectedVoice = voices.find(voice => 
        voice.name.includes('Microsoft Stefan')
      );
    }
    
    // Fallback: Irgendeine deutsche Stimme
    if (!selectedVoice) {
      selectedVoice = voices.find(voice => voice.lang.startsWith('de'));
    }
    
    if (selectedVoice) {
      this.currentUtterance.voice = selectedVoice;
      console.log('Gewählte Stimme:', selectedVoice.name);
    } else {
      console.log('Keine passende Stimme gefunden, verwende Browser-Standard');
    }

    // Event-Listener für Ende der Wiedergabe
    this.currentUtterance.onend = () => {
      this.speakingMessageIndex = -1;
      this.currentUtterance = null;
    };

    // Event-Listener für Fehler
    this.currentUtterance.onerror = (event) => {
      console.error('Speech synthesis error:', event);
      this.speakingMessageIndex = -1;
      this.currentUtterance = null;
    };

    // Setze aktuellen Index
    this.speakingMessageIndex = messageIndex;

    // Starte Wiedergabe
    this.speechSynthesis.speak(this.currentUtterance);
  }

  /**
   * Stoppt die aktuelle Text-to-Speech Wiedergabe
   */
  stopSpeaking(): void {
    if (this.speechSynthesis.speaking) {
      this.speechSynthesis.cancel();
    }
    this.speakingMessageIndex = -1;
    this.currentUtterance = null;
  }

  /**
   * Prüft ob eine bestimmte Nachricht gerade vorgelesen wird
   */
  isSpeaking(messageIndex: number): boolean {
    return this.speakingMessageIndex === messageIndex;
  }
}
