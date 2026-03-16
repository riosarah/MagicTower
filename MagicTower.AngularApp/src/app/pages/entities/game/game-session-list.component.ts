//@CustomCode
import { IdType, IdDefault } from '@app/models/i-key-model';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { IQueryParams } from '@app/models/base/i-query-params';
import { IGameSession } from '@app-models/entities/game/i-game-session';
import { GameSessionBaseListComponent }from '@app/components/entities/game/game-session-base-list.component';
import { GameSessionEditComponent }from '@app/components/entities/game/game-session-edit.component';
@Component({
  standalone: true,
  selector:'app-game-session-list',
  imports: [ CommonModule, FormsModule, TranslateModule, RouterModule ],
  templateUrl: './game-session-list.component.html',
  styleUrl: './game-session-list.component.css'
})
export class GameSessionListComponent extends GameSessionBaseListComponent {
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
    queryParams.filter = '';
  }
  protected override getItemKey(item: IGameSession): IdType {
    return item?.id || IdDefault;
  }
  override get pageTitle(): string {
    return 'GameSessions';
  }
  override getEditComponent() {
    return GameSessionEditComponent;
  }
}
