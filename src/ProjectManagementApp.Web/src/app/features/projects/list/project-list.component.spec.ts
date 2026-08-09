import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideStore } from '@ngrx/store';
import { provideState } from '@ngrx/store';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { AuthActions } from '../../../core/store/auth/auth.actions';
import { Store } from '@ngrx/store';
import { ProjectListComponent } from './project-list.component';
import { ProjectsService } from '../../../core/services/projects.service';

describe('ProjectListComponent', () => {
  async function createComponent(role: 'Admin' | 'ProjectManager' | 'TeamMember') {
    await TestBed.configureTestingModule({
      imports: [ProjectListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideStore(),
        provideState(authFeature),
        provideAnimationsAsync(),
      ],
    }).compileComponents();

    const store = TestBed.inject(Store);
    store.dispatch(
      AuthActions.loginSuccess({
        accessToken: 'token',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        user: { id: 'u1', fullName: 'Test User', email: 'test@example.com', role },
      })
    );

    const fixture = TestBed.createComponent(ProjectListComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne(request => request.url === '/api/projects');
    req.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('does not render "New Project" for a TeamMember', async () => {
    const { fixture, httpMock } = await createComponent('TeamMember');

    const link = fixture.debugElement.query(By.css('a[routerLink="/projects/new"]'));
    expect(link).toBeNull();

    httpMock.verify();
  });

  it('renders "New Project" for a ProjectManager', async () => {
    const { fixture, httpMock } = await createComponent('ProjectManager');

    const link = fixture.debugElement.query(By.css('a[routerLink="/projects/new"]'));
    expect(link).not.toBeNull();

    httpMock.verify();
  });

  it('hiding the button is UX only — a forced create call still surfaces the API 403, not a silent success', async () => {
    const { httpMock } = await createComponent('TeamMember');
    const projectsService = TestBed.inject(ProjectsService);

    let observedStatus: number | null = null;
    projectsService
      .create({ name: 'Forced', startDate: '2026-08-01' })
      .subscribe({ error: err => (observedStatus = err.status) });

    const req = httpMock.expectOne('/api/projects');
    req.flush({ title: 'Forbidden', status: 403 }, { status: 403, statusText: 'Forbidden' });

    expect(observedStatus).toBe(403);
    httpMock.verify();
  });
});
