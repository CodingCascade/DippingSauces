import {
  AfterViewInit, Component, ElementRef, Input,
  OnChanges, OnDestroy, ViewChild
} from '@angular/core';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-chart', standalone: true,
  template: `<div class="chart-box" [class.ring]="kind === 'doughnut'">
    <canvas #canvas role="img" [attr.aria-label]="description">
      {{ description }}
    </canvas>
  </div>`,
  styles: [`
    :host { display: block; min-width: 0; }
    .chart-box { position: relative; height: 80px; }
    .ring { height: 230px; }
  `]
})
export class DashboardChart implements AfterViewInit, OnChanges, OnDestroy {
  @Input() kind: 'line' | 'doughnut' = 'line';
  @Input() labels: string[] = [];
  @Input() values: number[] = [];
  @Input() colors: string[] = ['#009da4'];
  @Input() description = '';
  @ViewChild('canvas') canvas?: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;
  ngAfterViewInit() { this.draw(); }
  ngOnChanges() { this.draw(); }
  ngOnDestroy() { this.chart?.destroy(); }
  private draw() {
    if (!this.canvas) return;
    this.chart?.destroy();
    const ring = this.kind === 'doughnut';
    this.chart = new Chart(this.canvas.nativeElement, {
      type: this.kind,
      data: {
        labels: this.labels,
        datasets: [{
          data: this.values,
          backgroundColor: ring ? this.colors : this.colors[0] + '18',
          borderColor: ring ? '#ffffff' : this.colors[0],
          borderWidth: ring ? 2 : 2.5,
          fill: !ring, tension: 0.35, pointRadius: 0
        }]
      },
      options: {
        responsive: true, maintainAspectRatio: false, animation: false,
        ...(ring ? { cutout: '64%' } : {}),
        plugins: { legend: { display: false }, tooltip: { enabled: true } },
        ...(ring ? {} : {
          scales: { x: { display: false }, y: { display: false } }
        })
      }
    });
  }
}

