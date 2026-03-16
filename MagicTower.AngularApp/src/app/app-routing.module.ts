import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { LoginComponent } from './pages/auth/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { HomeComponent } from './pages/home/home.component';
import { CharacterCreationComponent } from './pages/character-creation/character-creation.component';
import { CombatComponent } from './pages/combat/combat.component';
import { GameChatComponent } from './pages/game-chat/game-chat.component';

const routes: Routes = [
  // Öffentlicher Login-Bereich
  { path: 'auth/login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  
  // Game Routes
  { path: 'home', component: HomeComponent },
  { path: 'character-creation', component: CharacterCreationComponent },
  { path: 'combat', component: CombatComponent },
  { path: 'game-chat', component: GameChatComponent },

  // Geschützter Bereich mit Dashboard und Unterseiten
  //{ path: 'protected', component: ProtectedListComponent, canActivate: [AuthGuard] },

  // Redirect von leerem Pfad auf Home
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  // Fallback bei ungültiger URL
  { path: '**', redirectTo: '/home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
