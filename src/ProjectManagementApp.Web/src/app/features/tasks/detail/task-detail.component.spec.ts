import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { Store, provideStore, provideState } from '@ngrx/store';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { AuthActions } from '../../../core/store/auth/auth.actions';
import { TaskDetailComponent } from './task-detail.component';

describe('TaskDetailComponent', () => {
  const taskId = '11111111-1111-1111-1111-111111111111';

  const taskDetailBody = {
    id: taskId,
    projectId: '22222222-2222-2222-2222-222222222222',
    title: 'Spec Task',
    description: null,
    status: 'ToDo',
    priority: 'Medium',
    dueDate: null,
    assignee: null,
    closedAt: null,
    createdAt: '2026-08-01T00:00:00Z',
    updatedAt: '2026-08-01T00:00:00Z',
  };

  async function createComponent(role: 'Admin' | 'ProjectManager' | 'TeamMember') {
    await TestBed.configureTestingModule({
      imports: [TaskDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideStore(),
        provideState(authFeature),
        provideAnimationsAsync(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: new Map([['id', taskId]]) } },
        },
      ],
    }).compileComponents();

    const store = TestBed.inject(Store);
    store.dispatch(
      AuthActions.loginSuccess({
        accessToken: 'token',
        expiresAt: new Date(Date.now() + 900_000).toISOString(),
        user: { id: 'caller-id', fullName: 'Caller', email: 'caller@example.com', role },
      })
    );

    const fixture = TestBed.createComponent(TaskDetailComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne(`/api/tasks/${taskId}`);
    req.flush(taskDetailBody, { headers: { ETag: '"1"' } });
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('for a TeamMember, the status control is present and the Edit link is absent', async () => {
    const { fixture, httpMock } = await createComponent('TeamMember');

    const statusControl = fixture.nativeElement.querySelector('[data-testid="status-control"]');
    const editLink = fixture.nativeElement.querySelector('[data-testid="edit-link"]');

    expect(statusControl).not.toBeNull();
    expect(editLink).toBeNull();
    httpMock.verify();
  });

  it('for a ProjectManager, both the status control and the Edit link are present', async () => {
    const { fixture, httpMock } = await createComponent('ProjectManager');

    const statusControl = fixture.nativeElement.querySelector('[data-testid="status-control"]');
    const editLink = fixture.nativeElement.querySelector('[data-testid="edit-link"]');

    expect(statusControl).not.toBeNull();
    expect(editLink).not.toBeNull();
    httpMock.verify();
  });

  it('for an Admin, both the status control and the Edit link are present', async () => {
    const { fixture, httpMock } = await createComponent('Admin');

    const statusControl = fixture.nativeElement.querySelector('[data-testid="status-control"]');
    const editLink = fixture.nativeElement.querySelector('[data-testid="edit-link"]');

    expect(statusControl).not.toBeNull();
    expect(editLink).not.toBeNull();
    httpMock.verify();
  });
});
