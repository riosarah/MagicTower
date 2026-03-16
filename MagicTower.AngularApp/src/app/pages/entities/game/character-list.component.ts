//@CustomCode
import { IdType, IdDefault } from '@app/models/i-key-model';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { IQueryParams } from '@app/models/base/i-query-params';
import { ICharacter } from '@app-models/entities/game/i-character';
import { CharacterBaseListComponent }from '@app/components/entities/game/character-base-list.component';
import { CharacterEditComponent }from '@app/components/entities/game/character-edit.component';
@Component({
  standalone: true,
  selector:'app-character-list',
  imports: [ CommonModule, FormsModule, TranslateModule, RouterModule ],
  templateUrl: './character-list.component.html',
  styleUrl: './character-list.component.css'
})
export class CharacterListComponent extends CharacterBaseListComponent {
  constructor()
  {
    super();
  }
  override ngOnInit(): void {
    super.ngOnInit();
    this.reloadData();
  }
  override prepareQueryParams(queryParams: IQueryParams): void {
    super.prepareQueryParams(queryParams);
    queryParams.filter = 'name.ToLower().Contains(@0)';
  }
  protected override getItemKey(item: ICharacter): IdType {
    return item?.id || IdDefault;
  }
  override get pageTitle(): string {
    return 'Characters';
  }
  override getEditComponent() {
    return CharacterEditComponent;
  }
}
