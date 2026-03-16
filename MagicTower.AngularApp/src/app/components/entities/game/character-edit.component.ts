//@CustomCode
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CharacterBaseEditComponent }from '@app/components/entities/game/character-base-edit.component';
@Component({
  standalone: true,
  selector:'app-character-edit',
  imports: [ CommonModule, FormsModule, TranslateModule],
  templateUrl: './character-edit.component.html',
  styleUrl: './character-edit.component.css'
})
export class CharacterEditComponent extends CharacterBaseEditComponent {
}
