//@CustomCode
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DefeatedEnemyBaseEditComponent }from '@app/components/entities/game/defeated-enemy-base-edit.component';
@Component({
  standalone: true,
  selector:'app-defeated-enemy-edit',
  imports: [ CommonModule, FormsModule, TranslateModule],
  templateUrl: './defeated-enemy-edit.component.html',
  styleUrl: './defeated-enemy-edit.component.css'
})
export class DefeatedEnemyEditComponent extends DefeatedEnemyBaseEditComponent {
}
