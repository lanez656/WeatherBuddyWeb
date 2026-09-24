(function () {
  function init() {
    const canvas = document.getElementById('sunsetCanvas');
    if (!canvas) return; // Guard clause prevents crashes if canvas isn't present

    const ctx = canvas.getContext('2d');

    function resize() {
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width;
    canvas.height = window.innerHeight - rect.top; // Trækker navbarens højde fra
    draw();
    }

    // Helper function to draw rounded pill shapes
    function drawPill(x, y, width, height, color) {
      const radius = height / 2;
      ctx.beginPath();
      ctx.fillStyle = color;
      ctx.roundRect(x - width / 2, y, width, height, radius);
      ctx.fill();
    }

    function draw() {
      const width = canvas.width;
      const height = canvas.height;
      const horizon = height * 0.52; // Horizon position

      ctx.clearRect(0, 0, width, height);

      // 1. Background Gradient
      const bgGradient = ctx.createLinearGradient(0, 0, 0, height);
      bgGradient.addColorStop(0, '#f47040');   
      bgGradient.addColorStop(0.55, '#FF8C00'); 
      bgGradient.addColorStop(1, '#FFC107');   
      ctx.fillStyle = bgGradient;
      ctx.fillRect(0, 0, width, height);

      const sunX = width / 2;
      const sunRadius = Math.min(width, height) * 0.17;

      // 2. Main Sun (Clipped cleanly above the horizon line)
      ctx.save();
      ctx.beginPath();
      ctx.rect(0, 0, width, horizon); 
      ctx.clip();

      ctx.beginPath();
      ctx.arc(sunX, horizon + 10, sunRadius, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(255, 248, 230, 0.95)';
      ctx.fill();

      ctx.restore();

      // 3. Horizon Line
      ctx.beginPath();
      ctx.moveTo(0, horizon);
      ctx.lineTo(width, horizon);
      ctx.strokeStyle = 'rgba(255, 248, 230, 0.95)';
      ctx.lineWidth = 3;
      ctx.stroke();

      // 4. Static Icon-Style Reflection Bars (Pills)
      const sunColor = 'rgba(255, 248, 230, 0.95)';
      const bars = [
        { yOffset: 8,   w: sunRadius * 2.1, h: 14, xShift: 15 },
        { yOffset: 28,  w: sunRadius * 1.6, h: 12, xShift: -25 },
        { yOffset: 46,  w: sunRadius * 1.2, h: 10, xShift: 10 },
        { yOffset: 62,  w: sunRadius * 1.4, h: 8,  xShift: -12 },
        { yOffset: 76,  w: sunRadius * 0.9, h: 6,  xShift: 5 },
        { yOffset: 88,  w: sunRadius * 0.5, h: 5,  xShift: 0 }
      ];

      bars.forEach(bar => {
        drawPill(
          sunX + bar.xShift, 
          horizon + bar.yOffset, 
          bar.w, 
          bar.h, 
          sunColor
        );
      });
    }

    window.addEventListener('resize', resize);
    resize(); // Initial draw call
  }

  // Ensures element exists before trying to draw
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();