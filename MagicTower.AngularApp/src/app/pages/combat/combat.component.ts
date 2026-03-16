import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GameService } from '../../services/game.service';

@Component({
  selector: 'app-combat',
  templateUrl: './combat.component.html',
  styleUrls: ['./combat.component.css'],
  standalone: false
})
export class CombatComponent implements OnInit {
  characterId!: number;
  sessionId!: number;
  character: any = null;
  enemy: any = null;
  session: any = null;
  weapons: any[] = [];
  
  loading = false;
  combatInProgress = false;
  combatLog: string[] = [];
  showRewards = false;
  rewards: any = null;
  gameOver = false;
  victory = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.characterId = +params['characterId'];
      this.sessionId = +params['sessionId'];
      
      if (!this.characterId) {
        this.router.navigate(['/home']);
        return;
      }
      
      this.loadGameData();
    });
  }

  loadGameData(): void {
    console.log('=== LOAD GAME DATA START ===');
    console.log('Character ID:', this.characterId);
    console.log('Session ID:', this.sessionId);
    this.loading = true;
    
    // Load character
    this.gameService.getCharacter(this.characterId).subscribe({
      next: (response) => {
        console.log('Character Response:', response);
        if (response.success && response.data) {
          this.character = response.data;
          console.log('Character loaded:', this.character);
        } else {
          console.error('Character loading failed:', response.message);
        }
      },
      error: (error) => {
        console.error('Fehler beim Laden des Charakters:', error);
        console.error('Error details:', error);
      }
    });

    // Load session if available
    if (this.sessionId) {
      console.log('Loading session with ID:', this.sessionId);
      this.gameService.getSession(this.sessionId).subscribe({
        next: (session) => {
          console.log('Session Response:', session);
          this.session = session;
          console.log('Session loaded:', this.session);
          
          // Check if game is completed
          if (this.session.isCompleted) {
            console.log('Game is completed!');
            this.victory = true;
            this.gameOver = true;
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Fehler beim Laden der Session:', error);
          console.error('Error details:', error);
          this.loading = false;
        }
      });
    } else {
      console.log('No session ID, skipping session load');
      this.loading = false;
    }

    // Load weapons
    this.loadWeapons();
  }

  loadWeapons(): void {
    this.gameService.getWeapons(this.characterId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.weapons = response.data;
        }
      },
      error: (error) => console.error('Fehler beim Laden der Waffen:', error)
    });
  }

  startFight(): void {
    console.log('=== START FIGHT ===');
    console.log('Session ID exists?', !!this.sessionId);
    console.log('Session ID:', this.sessionId);
    console.log('Character ID:', this.characterId);
    
    if (!this.sessionId) {
      console.log('No session ID, creating new session...');
      // Start a new session first
      this.gameService.startSession(this.characterId, 10).subscribe({
        next: (response) => {
          console.log('New session response:', response);
          if (response.success && response.data) {
            this.sessionId = response.data.id;
            this.session = response.data;
            console.log('New session created:', this.session);
            this.generateEnemy();
          } else {
            console.error('Session creation failed:', response.message);
          }
        },
        error: (error) => {
          console.error('Fehler beim Starten der Session:', error);
          console.error('Error details:', error);
        }
      });
    } else {
      console.log('Session exists, generating enemy...');
      this.generateEnemy();
    }
  }

  generateEnemy(): void {
    console.log('=== GENERATE ENEMY ===');
    console.log('Session ID:', this.sessionId);
    console.log('Character ID:', this.characterId);
    
    this.loading = true;
    this.combatLog = [];
    this.showRewards = false;
    
    this.gameService.startFight(this.sessionId, this.characterId).subscribe({
      next: (response) => {
        console.log('Enemy generation response:', response);
        if (response.success && response.data) {
          this.enemy = response.data;
          console.log('Enemy generated:', this.enemy);
          this.addToCombatLog(`Ein wilder ${this.enemy.race} ${this.enemy.type} erscheint!`);
          if (this.enemy.isBoss) {
            this.addToCombatLog('⚠️ Dies ist ein BOSS-Kampf!');
          }
        } else {
          console.error('Enemy generation failed:', response.message);
          this.addToCombatLog(`Fehler: ${response.message}`);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Fehler beim Generieren des Gegners:', error);
        console.error('Error details:', error);
        this.addToCombatLog(`Fehler beim Generieren des Gegners: ${error.message || error}`);
        this.loading = false;
      }
    });
  }

  attack(useSpecialAttack: boolean = false): void {
    console.log('=== ATTACK ===');
    console.log('Use special attack:', useSpecialAttack);
    console.log('Combat in progress:', this.combatInProgress);
    console.log('Enemy exists:', !!this.enemy);
    console.log('Session ID:', this.sessionId);
    console.log('Character ID:', this.characterId);
    console.log('Enemy:', this.enemy);
    
    if (this.combatInProgress || !this.enemy) {
      console.log('Combat blocked - in progress or no enemy');
      return;
    }
    
    this.combatInProgress = true;
    
    const actionRequest = {
      gameSessionId: this.sessionId,
      characterId: this.characterId,
      useSpecialAttack: useSpecialAttack,
      enemy: this.enemy
    };
    console.log('Action Request:', actionRequest);
    
    this.gameService.executeAction(
      this.sessionId,
      this.characterId,
      useSpecialAttack,
      this.enemy
    ).subscribe({
      next: (response) => {
        console.log('Combat Response:', response);
        if (response.success && response.data) {
          const result = response.data;
          console.log('Combat Result:', result);
          
          // Log combat actions
          if (result.playerUsedSpecialAttack) {
            this.addToCombatLog(`⚡ ${this.character.name} nutzt SPEZIALANGRIFF! (${result.playerDamage} Schaden)`);
          } else {
            this.addToCombatLog(`⚔️ ${this.character.name} greift an! (${result.playerDamage} Schaden)`);
          }
          
          if (!result.enemyDefeated) {
            if (result.enemyUsedSpecialAttack) {
              this.addToCombatLog(`💥 ${this.enemy.race} ${this.enemy.type} nutzt SPEZIALANGRIFF! (${result.enemyDamage} Schaden)`);
            } else {
              this.addToCombatLog(`🗡️ ${this.enemy.race} ${this.enemy.type} greift an! (${result.enemyDamage} Schaden)`);
            }
          }
          
          // Update health
          this.character.health = result.updatedPlayerHealth;
          this.enemy.health = result.updatedEnemyHealth;
          
          // Check results
          if (result.playerDefeated) {
            this.addToCombatLog('💀 Du wurdest besiegt!');
            this.gameOver = true;
          } else if (result.enemyDefeated) {
            this.addToCombatLog(`✅ ${this.enemy.race} ${this.enemy.type} wurde besiegt!`);
            this.enemy = null;
            
            if (result.rewards) {
              this.rewards = result.rewards;
              this.showRewards = true;
              this.processRewards(result.rewards);
            }
            
            if (result.floorAdvanced) {
              this.session.currentFloor++;
              this.addToCombatLog(`🏆 Du erreichst Stockwerk ${this.session.currentFloor}!`);
            }
            
            if (result.gameCompleted) {
              this.addToCombatLog('🎉 GLÜCKWUNSCH! Du hast den Turm bezwungen!');
              this.victory = true;
              this.gameOver = true;
            }
          }
          
          // Reload character data
          console.log('Reloading character data...');
          this.loadGameData();
        } else {
          console.error('Combat action failed:', response.message);
          this.addToCombatLog(`Fehler: ${response.message}`);
        }
        this.combatInProgress = false;
        console.log('Combat finished, combatInProgress = false');
      },
      error: (error) => {
        console.error('Fehler beim Ausführen der Aktion:', error);
        console.error('Error details:', error);
        this.addToCombatLog(`Fehler beim Ausführen der Aktion: ${error.message || error}`);
        this.combatInProgress = false;
      }
    });
  }

  processRewards(rewards: any): void {
    if (rewards.goldEarned) {
      this.addToCombatLog(`💰 +${rewards.goldEarned} Gold`);
    }
    
    if (rewards.leveledUp) {
      this.addToCombatLog(`⬆️ Level Up! Neues Level: ${rewards.newLevel}`);
    }
    
    if (rewards.newWeapon) {
      this.addToCombatLog(`🗡️ Neue Waffe erhalten: ${rewards.newWeapon.name}`);
    }
    
    if (rewards.specialAttackUpgraded) {
      this.addToCombatLog(`✨ Spezialangriff verbessert! Multiplikator: ${rewards.newSpecialMultiplier}x`);
    }
  }

  addToCombatLog(message: string): void {
    this.combatLog.push(message);
    // Auto scroll to bottom
    setTimeout(() => {
      const logElement = document.querySelector('.combat-log-content');
      if (logElement) {
        logElement.scrollTop = logElement.scrollHeight;
      }
    }, 100);
  }

  continueAdventure(): void {
    this.showRewards = false;
    this.rewards = null;
    this.startFight();
  }

  goHome(): void {
    this.router.navigate(['/home']);
  }

  getHealthPercentage(current: number, max: number): number {
    return (current / max) * 100;
  }

  getHealthBarClass(percentage: number): string {
    if (percentage > 60) return 'bg-success';
    if (percentage > 30) return 'bg-warning';
    return 'bg-danger';
  }

  getClassDisplayName(): string {
    if (!this.character) return '';
    switch (this.character.characterClass) {
      case 1: return 'Warrior';
      case 2: return 'Archer';
      case 3: return 'Druid';
      default: return 'Unknown';
    }
  }

  upgradeWeapon(weaponId: number): void {
    this.gameService.upgradeWeapon(weaponId, this.characterId).subscribe({
      next: (response) => {
        if (response.success) {
          this.addToCombatLog(`✨ ${response.message}`);
          this.loadWeapons();
          this.loadGameData();
        }
      },
      error: (error) => console.error('Fehler beim Aufwerten der Waffe:', error)
    });
  }

  sellWeapon(weaponId: number): void {
    this.gameService.sellWeapon(weaponId, this.characterId).subscribe({
      next: (response) => {
        if (response.success) {
          this.addToCombatLog(`💰 ${response.message}`);
          this.loadWeapons();
          this.loadGameData();
        }
      },
      error: (error) => console.error('Fehler beim Verkaufen der Waffe:', error)
    });
  }
}
