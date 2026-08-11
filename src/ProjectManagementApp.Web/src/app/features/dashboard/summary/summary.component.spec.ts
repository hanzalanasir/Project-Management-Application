import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { SummaryComponent } from './summary.component';

// T048: the "My Work" panel renders its zero state ("You have no tasks assigned.") for a caller
// with no assignments — not an error, and not a blank/undefined render.
describe('SummaryComponent', () => {
  async function createComponent(personalTasks: { assignedTotal: number; byStatus: Record<string, number>; overdueCount: number }) {
    await TestBed.configureTestingModule({
      imports: [SummaryComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), provideAnimationsAsync()],
    }).compileComponents();

    const fixture = TestBed.createComponent(SummaryComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne('/api/dashboard/summary');
    req.flush({
      generatedAt: new Date().toISOString(),
      scope: 'ProjectManager',
      visibleProjectCount: 1,
      projectsByStatus: { Planning: 1, Active: 0, OnHold: 0, Completed: 0, Cancelled: 0 },
      tasksByStatus: { ToDo: 0, InProgress: 0, InReview: 0, Done: 0, Blocked: 0 },
      overdueTaskCount: 0,
      completionRate: 0,
      blockedTaskCount: 0,
      visibleTeamMemberCount: 0,
      personalTasks,
    });
    fixture.detectChanges();

    // SummaryComponent embeds ActivityFeedComponent (T039), which fires its own request on init.
    const activityReq = httpMock.expectOne(r => r.url === '/api/dashboard/activity');
    activityReq.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('renders the zero state for a caller with no assigned tasks, not an error', async () => {
    const { fixture, httpMock } = await createComponent({
      assignedTotal: 0,
      byStatus: { ToDo: 0, InProgress: 0, InReview: 0, Done: 0, Blocked: 0 },
      overdueCount: 0,
    });

    const myWork = fixture.debugElement.query(By.css('.my-work'));
    expect(myWork.nativeElement.textContent).toContain('You have no tasks assigned.');
    expect(fixture.debugElement.query(By.css('.error'))).toBeNull();

    httpMock.verify();
  });

  it('renders the assigned count when the caller has tasks', async () => {
    const { fixture, httpMock } = await createComponent({
      assignedTotal: 3,
      byStatus: { ToDo: 2, InProgress: 1, InReview: 0, Done: 0, Blocked: 0 },
      overdueCount: 1,
    });

    const myWork = fixture.debugElement.query(By.css('.my-work'));
    expect(myWork.nativeElement.textContent).not.toContain('You have no tasks assigned.');
    expect(myWork.nativeElement.textContent).toContain('3');

    httpMock.verify();
  });
});
