import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideStore } from '@ngrx/store';
import { provideState } from '@ngrx/store';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { CreateProjectComponent } from './create-project.component';

describe('CreateProjectComponent', () => {
  async function createComponent() {
    await TestBed.configureTestingModule({
      imports: [CreateProjectComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideStore(),
        provideState(authFeature),
        provideAnimationsAsync(),
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(CreateProjectComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('name is required', async () => {
    const fixture = await createComponent();
    const nameControl = fixture.componentInstance['form'].get('name')!;

    nameControl.setValue('');

    expect(nameControl.hasError('required')).toBe(true);
  });

  it('name exceeding the max length is invalid', async () => {
    const fixture = await createComponent();
    const nameControl = fixture.componentInstance['form'].get('name')!;

    nameControl.setValue('a'.repeat(201));

    expect(nameControl.hasError('maxlength')).toBe(true);
  });

  it('an end date before the start date fails the cross-field date-order validator', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    form.patchValue({ startDate: '2026-08-01', endDate: '2026-01-01' });

    expect(form.hasError('dateOrder')).toBe(true);
  });

  it('an end date on or after the start date passes the date-order validator', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    form.patchValue({ startDate: '2026-08-01', endDate: '2026-11-30' });

    expect(form.hasError('dateOrder')).toBe(false);
  });

  it('an omitted end date passes the date-order validator (open-ended project)', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    form.patchValue({ startDate: '2026-08-01', endDate: '' });

    expect(form.hasError('dateOrder')).toBe(false);
  });

  it('start date is required', async () => {
    const fixture = await createComponent();
    const startDateControl = fixture.componentInstance['form'].get('startDate')!;

    startDateControl.setValue('');

    expect(startDateControl.hasError('required')).toBe(true);
  });
});
