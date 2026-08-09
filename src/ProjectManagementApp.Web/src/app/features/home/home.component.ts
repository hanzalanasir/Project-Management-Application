import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { authFeature } from '../../core/store/auth/auth.feature';

// Placeholder authenticated landing route — the real Dashboard is feature 005, out of scope
// for 001. This exists only so the auth guard (T087) has a real destination to protect.
@Component({
  selector: 'app-home',
  template: `<p>Signed in as {{ user()?.fullName }} ({{ user()?.role }})</p>`,
})
export class HomeComponent {
  protected readonly user = inject(Store).selectSignal(authFeature.selectUser);
}
