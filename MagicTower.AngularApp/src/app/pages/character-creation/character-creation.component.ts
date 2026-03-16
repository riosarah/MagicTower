import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { GameService } from '../../services/game.service';

interface CharacterClass {
  id: number;
  name: string;
  description: string;
  health: number;
  attack: number;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-character-creation',
  templateUrl: './character-creation.component.html',
  styleUrls: ['./character-creation.component.css'],
  standalone: false
})
export class CharacterCreationComponent {
  characterName = '';
  selectedClass: number | null = null;
  difficulty: number = 10;
  loading = false;
  errorMessage = '';

  characterClasses: CharacterClass[] = [
    {
      id: 1,
      name: 'Warrior',
      description: 'Ein starker Kämpfer mit hoher Gesundheit und Nahkampfstärke',
      health: 120,
      attack: 15,
      icon: 'bi-shield-fill-check',
      color: '#dc3545'
    },
    {
      id: 2,
      name: 'Archer',
      description: 'Ein geschickter Fernkämpfer mit präzisen Angriffen',
      health: 100,
      attack: 12,
      icon: 'bi-bullseye',
      color: '#28a745'
    },
    {
      id: 3,
      name: 'Druid',
      description: 'Ein mystischer Magier mit ausgewogenen Fähigkeiten',
      health: 110,
      attack: 10,
      icon: 'bi-stars',
      color: '#6f42c1'
    }
  ];

  difficultyLevels = [
    { value: 10, label: 'Einfach (10 Stockwerke)', icon: 'bi-circle', color: '#28a745' },
    { value: 20, label: 'Mittel (20 Stockwerke)', icon: 'bi-circle-half', color: '#ffc107' },
    { value: 30, label: 'Schwer (30 Stockwerke)', icon: 'bi-circle-fill', color: '#dc3545' }
  ];

  constructor(
    private router: Router,
    private gameService: GameService
  ) {}

  selectClass(classId: number): void {
    this.selectedClass = classId;
  }

  getSelectedClassInfo(): CharacterClass | undefined {
    return this.characterClasses.find(c => c.id === this.selectedClass);
  }

  createCharacter(): void {
    if (!this.characterName.trim()) {
      this.errorMessage = 'Bitte geben Sie einen Charakternamen ein.';
      return;
    }

    if (this.selectedClass === null) {
      this.errorMessage = 'Bitte wählen Sie eine Charakterklasse aus.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.gameService.createCharacter(this.characterName, this.selectedClass).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Start a new game session with the created character
          this.startGameSession(response.data.id);
        } else {
          this.errorMessage = response.message || 'Fehler beim Erstellen des Charakters';
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Fehler beim Erstellen des Charakters:', error);
        this.errorMessage = 'Ein Fehler ist aufgetreten. Bitte versuchen Sie es erneut.';
        this.loading = false;
      }
    });
  }

  startGameSession(characterId: number): void {
    this.gameService.startSession(characterId, this.difficulty).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          // Navigate to combat with the session data
          this.router.navigate(['/combat'], {
            queryParams: {
              characterId: characterId,
              sessionId: response.data.id
            }
          });
        } else {
          this.errorMessage = response.message || 'Fehler beim Starten der Session';
        }
      },
      error: (error) => {
        console.error('Fehler beim Starten der Session:', error);
        this.errorMessage = 'Ein Fehler ist aufgetreten. Bitte versuchen Sie es erneut.';
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/home']);
  }
}
