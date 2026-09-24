function initWeatherBackground(weatherCode) {
    const canvas = document.getElementById('weatherCanvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    let animationFrameId;
    let particles = [];
    let clouds = [];

    function resize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }
    window.addEventListener('resize', resize);
    resize();

    // 1. Gruppering af Open-Meteo koder
    function getPreset(code) {
        // Solrigt / Næsten klart (0, 1)
        if ([0, 1].includes(code)) {
            return { mode: 'CLEAR', cloudCount: 0, showSun: true, bg: 'linear-gradient(180deg, #6374d2 0%, #17b2d8 100%)' };
        }
        // Delvist skyet (2)
        if (code === 2) {
            return { mode: 'PARTLY_CLOUDY', cloudCount: 10, showSun: true, bg: 'linear-gradient(180deg, #6374d2, #2297d1 0% 100%)' };
        }
        // Overskyet / Tåge (3, 45, 48)
        if ([3, 45, 48].includes(code)) {
            return { mode: 'OVERCAST', cloudCount: 25, showSun: false, bg: 'linear-gradient(180deg, #4B515D 0%, #A8B2C1 100%)' };
        }
        // Sne (71, 73, 75, 77, 85, 86)
        if ([71, 73, 75, 77, 85, 86].includes(code)) {
            return { mode: 'SNOW', cloudCount: 25, showSun: false, bg: 'linear-gradient(180deg, #2b5876 0%, #4e4376 100%)' };
        }
        // Regn / Støvregn / Torden (51-67, 80-82, 95-99)
        return { mode: 'RAIN', cloudCount: 25, showSun: false, bg: 'linear-gradient(180deg, #0f2027 0%, #203a43 50%, #2c5364 100%)' };
    }

    const config = getPreset(weatherCode);
    canvas.style.background = config.bg;

    // 2. Generer rolige skyer i toppen
    clouds = [];
    for (let i = 0; i < config.cloudCount; i++) {
        clouds.push({
            x: Math.random() * canvas.width,
            y: Math.random() * (canvas.height * 0.25), // Placeres i øverste 25% af skærmen
            radius: Math.random() * 40 + 50,
            speed: Math.random() * 0.15 + 0.05, // Meget rolig bevægelse
            opacity: config.mode === 'OVERCAST' ? 0.85 : 0.6
        });
    }

    // 3. Generer regn- eller sne-partikler
    particles = [];
    const particleCount = config.mode === 'RAIN' ? 100 : (config.mode === 'SNOW' ? 60 : 0);
    for (let i = 0; i < particleCount; i++) {
        particles.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            length: Math.random() * 15 + 10,
            radius: Math.random() * 3 + 1,
            speedY: config.mode === 'RAIN' ? Math.random() * 8 + 8 : Math.random() * 1.2 + 0.5,
            speedX: config.mode === 'SNOW' ? Math.sin(Math.random()) * 0.5 : 0,
            opacity: Math.random() * 0.4 + 0.3
        });
    }

    // Hjælpefunktion til at tegne en blød sky af cirkler
    function drawCloud(cloud) {
        ctx.fillStyle = config.mode === 'OVERCAST' 
            ? `rgba(200, 205, 215, ${cloud.opacity})` 
            : `rgba(255, 255, 255, ${cloud.opacity})`;

        ctx.beginPath();
        ctx.arc(cloud.x, cloud.y, cloud.radius, 0, Math.PI * 2);
        ctx.arc(cloud.x + cloud.radius * 0.6, cloud.y - cloud.radius * 0.2, cloud.radius * 0.7, 0, Math.PI * 2);
        ctx.arc(cloud.x - cloud.radius * 0.6, cloud.y - cloud.radius * 0.1, cloud.radius * 0.6, 0, Math.PI * 2);
        ctx.fill();
    }

    // 4. Hoved-animationsloop
    function animate() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // A. Tegn Sol (hvis relevant)
        if (config.showSun) {
            ctx.beginPath();
            ctx.arc(canvas.width * 0.75, canvas.height * 0.2, 50, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(255, 230, 40, 0.9)';
            ctx.fill();
        }

        // B. Tegn og flyt skyer
        clouds.forEach(c => {
            drawCloud(c);
            c.x += c.speed;
            // Loop skyen rundt, når den driver ud af skærmen
            if (c.x - c.radius * 2 > canvas.width) {
                c.x = -c.radius * 2;
                c.y = Math.random() * (canvas.height * 0.25);
            }
        });

        // C. Tegn og flyt regn / sne
        if (config.mode === 'RAIN') {
            ctx.strokeStyle = 'rgba(200, 225, 255, 0.6)';
            ctx.lineWidth = 1.2;
            particles.forEach(p => {
                ctx.beginPath();
                ctx.moveTo(p.x, p.y);
                ctx.lineTo(p.x, p.y + p.length);
                ctx.stroke();

                p.y += p.speedY;
                if (p.y > canvas.height) {
                    p.y = -p.length;
                    p.x = Math.random() * canvas.width;
                }
            });
        } 
        else if (config.mode === 'SNOW') {
            particles.forEach(p => {
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(255, 255, 255, ${p.opacity})`;
                ctx.fill();

                p.y += p.speedY;
                p.x += p.speedX;

                if (p.y > canvas.height) {
                    p.y = -p.radius;
                    p.x = Math.random() * canvas.width;
                }
            });
        }

        animationFrameId = requestAnimationFrame(animate);
    }

    animate();
}