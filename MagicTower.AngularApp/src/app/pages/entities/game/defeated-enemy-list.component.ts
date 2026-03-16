//@CustomCode
import { IdType, IdDefault } from '@app/models/i-key-model';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { IQueryParams } from '@app/models/base/i-query-params';
import { IDefeatedEnemy } from '@app-models/entities/game/i-defeated-enemy';
import { DefeatedEnemyBaseListComponent }from '@app/components/entities/game/defeated-enemy-base-list.component';
import { DefeatedEnemyEditComponent }from '@app/components/entities/game/defeated-enemy-edit.component';
@Component({
  standalone: true,
  selector:'app-defeated-enemy-list',
  imports: [ CommonModule, FormsModule, TranslateModule, RouterModule ],
  templateUrl: './defeated-enemy-list.component.html',
  styleUrl: './defeated-enemy-list.component.css'
})
export class DefeatedEnemyListComponent extends DefeatedEnemyBaseListComponent {
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
    queryParams.filter = 'enemyType.ToLower().Contains(@0) OR enemyRace.ToLower().Contains(@0) OR enemyWeapon.ToLower().Contains(@0)';
  }
  protected override getItemKey(item: IDefeatedEnemy): IdType {
    return item?.id || IdDefault;
  }
  override get pageTitle(): string {
    return 'DefeatedEnemies';
  }
  override getEditComponent() {
    return DefeatedEnemyEditComponent;
  }
}
