//@CustomCode
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { GameSessionBaseEditComponent }from '@app/components/entities/game/game-session-base-edit.component';
@Component({
  standalone: true,
  selector:'app-game-session-edit',
  imports: [ CommonModule, FormsModule, TranslateModule],
  templateUrl: './game-session-edit.component.html',
  styleUrl: './game-session-edit.component.css'
})
export class GameSessionEditComponent extends GameSessionBaseEditComponent {
}
