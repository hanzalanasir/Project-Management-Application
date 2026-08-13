import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideNativeDateAdapter } from '@angular/material/core';
import { ReportPickerComponent } from './report-picker.component';

@Component({ template: '' })
class BlankComponent {}

// T030 — the picker must build its form FROM the catalog descriptors at runtime, not hard-code one
// form per report type: selecting a descriptor with different parameters produces a different set
// of form controls, with no per-report branch in this component.
describe('ReportPickerComponent', () => {
  const catalog = [
    {
      type: 'ProjectProgress',
      title: 'Project Progress',
      note: null,
      parameters: [
        { name: 'from', type: 'date', required: true },
        { name: 'to', type: 'date', required: true },
        { name: 'projectScope', type: 'all|projectIds', required: false },
      ],
      formats: ['json', 'pdf', 'csv'],
    },
    {
      type: 'TeamPerformance',
      title: 'Team Performance',
      note: 'self only',
      parameters: [
        { name: 'from', type: 'date', required: true },
        { name: 'to', type: 'date', required: true },
        { name: 'userId', type: 'uuid', required: false },
      ],
      formats: ['json', 'pdf', 'csv'],
    },
  ];

  async function createComponent() {
    await TestBed.configureTestingModule({
      imports: [ReportPickerComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'reports/project-progress', component: BlankComponent }]),
        provideAnimationsAsync(),
        provideNativeDateAdapter(),
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ReportPickerComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    httpMock.expectOne('/api/reports/catalog').flush(catalog);
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('renders one card per catalog descriptor', async () => {
    const { fixture, httpMock } = await createComponent();
    const cards = fixture.debugElement.queryAll(By.css('.descriptor'));
    expect(cards.length).toBe(2);
    httpMock.verify();
  });

  it("builds a form with exactly the selected descriptor's parameters, and it changes per descriptor", async () => {
    const { fixture, httpMock } = await createComponent();
    const component = fixture.componentInstance;

    component['selectReport'](catalog[0] as never);
    fixture.detectChanges();
    expect(Object.keys(component['form']!.controls)).toEqual(['from', 'to', 'projectScope']);

    component['selectReport'](catalog[1] as never);
    fixture.detectChanges();
    expect(Object.keys(component['form']!.controls)).toEqual(['from', 'to', 'userId']);

    httpMock.verify();
  });

  it('navigates to the report route with the form values as query params on submit', async () => {
    const { fixture, httpMock } = await createComponent();
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);

    component['selectReport'](catalog[0] as never);
    fixture.detectChanges();
    component['form']!.setValue({ from: new Date(2026, 6, 1), to: new Date(2026, 6, 31), projectScope: '' });
    component['submit']();
    await fixture.whenStable();

    expect(router.url).toBe('/reports/project-progress?from=2026-07-01&to=2026-07-31');
    httpMock.verify();
  });
});
