//@CustomCode
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { WeaponBaseEditComponent }from '@app/components/entities/game/weapon-base-edit.component';
@Component({
  standalone: true,
  selector:'app-weapon-edit',
  imports: [ CommonModule, FormsModule, TranslateModule],
  templateUrl: './weapon-edit.component.html',
  styleUrl: './weapon-edit.component.css'
})
export class WeaponEditComponent extends WeaponBaseEditComponent {
}
