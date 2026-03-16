//@CustomCode
import { IdType, IdDefault } from '@app/models/i-key-model';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { IQueryParams } from '@app/models/base/i-query-params';
import { IWeapon } from '@app-models/entities/game/i-weapon';
import { WeaponBaseListComponent }from '@app/components/entities/game/weapon-base-list.component';
import { WeaponEditComponent }from '@app/components/entities/game/weapon-edit.component';
@Component({
  standalone: true,
  selector:'app-weapon-list',
  imports: [ CommonModule, FormsModule, TranslateModule, RouterModule ],
  templateUrl: './weapon-list.component.html',
  styleUrl: './weapon-list.component.css'
})
export class WeaponListComponent extends WeaponBaseListComponent {
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
    queryParams.filter = 'name.ToLower().Contains(@0) OR type.ToLower().Contains(@0)';
  }
  protected override getItemKey(item: IWeapon): IdType {
    return item?.id || IdDefault;
  }
  override get pageTitle(): string {
    return 'Weapons';
  }
  override getEditComponent() {
    return WeaponEditComponent;
  }
}
