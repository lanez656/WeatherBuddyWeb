// 1. Gruppering af Open-Meteo koder
function getPreset(code, isNight) {
    // Solrigt / Næsten klart (0, 1)
    if ([0, 1].includes(code)) {
        return isNight
            ? { mode: 'CLEAR_NIGHT', cloudCount: 0, showSun: false, showMoon: true, showStars: true, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'CLEAR_DAY', cloudCount: 0, showSun: true, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #6374d2 0%, #178ed8 100%)' };
    }
    // Delvist skyet (2)
    if (code === 2) {
        return isNight
            ? { mode: 'PARTLY_CLOUDY_NIGHT', cloudCount: 20, showSun: false, showMoon: true, showStars: true, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'PARTLY_CLOUDY_DAY', cloudCount: 20, showSun: true, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #6374d2 0%, #178ed8 100%)' };
    }
    // Overskyet / Tåge (3, 45, 48)
    if ([3, 45, 48].includes(code)) {
        return isNight
            ? { mode: 'OVERCAST_NIGHT', cloudCount: 25, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'OVERCAST_DAY', cloudCount: 25, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #4B515D 0%, #A8B2C1 100%)' };
    }
    // Sne (71, 73, 75, 77, 85, 86)
    if ([71, 73, 75, 77, 85, 86].includes(code)) {
        return isNight
            ? { mode: 'SNOW_NIGHT', cloudCount: 55, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #101a35 0%, #26395a 100%)' }
            : { mode: 'SNOW_DAY', cloudCount: 55, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #2b5876 0%, #4e4376 100%)' };
    }
    // Regn / Støvregn / Torden (51-67, 80-82, 95-99)
    return isNight
        ? { mode: 'RAIN_NIGHT', cloudCount: 25, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #050b18 0%, #12232e 50%, #1c3544 100%)' }
        : { mode: 'RAIN_DAY', cloudCount: 25, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #0f2027 0%, #203a43 50%, #2c5364 100%)' };
}

function initWeatherBackground(weatherCode, isNight) {
    const canvas = document.getElementById('weatherCanvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    let animationFrameId;
    let particles = [];
    let clouds = [];
    let stars = [];

    function resize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }
    window.addEventListener('resize', resize);
    resize();

    const config = getPreset(weatherCode, isNight);
    canvas.style.background = config.bg;

    // Generer stjerner hvis stjerneklart om natten
    stars = [];
    if (config.showStars) {
        const starColors = ['#ffffff', '#e0f7fa', '#fff9c4', '#e8eaf6'];
        const starCount = 100;
        for (let i = 0; i < starCount; i++) {
            stars.push({
                x: Math.random() * canvas.width,
                y: Math.random() * (canvas.height * 0.7), // Spredt over øvre 70%
                radius: Math.random() * 1.8 + 0.4,
                baseOpacity: Math.random() * 0.6 + 0.2,
                twinkleSpeed: Math.random() * 0.04 + 0.01,
                phase: Math.random() * Math.PI * 2,
                color: starColors[Math.floor(Math.random() * starColors.length)]
            });
        }
    }

    // 2. Generer rolige skyer i toppen
    clouds = [];
    for (let i = 0; i < config.cloudCount; i++) {
        clouds.push({
            x: Math.random() * canvas.width,
            y: Math.random() * (canvas.height * 0.25), // Placeres i øverste 25% af skærmen
            radius: Math.random() * 40 + 50,
            speed: Math.random() * 0.15 + 0.05, // Meget rolig bevægelse
            opacity: config.mode.includes('OVERCAST') ? 0.85 : 0.6
        });
    }

    // 3. Generer regn- eller sne-partikler
    particles = [];
    let particleCount = 0;
    if (config.mode.includes('RAIN')) {
        particleCount = 100;
    } else if (config.mode.includes('SNOW')) {
        particleCount = 80;
    }

    for (let i = 0; i < particleCount; i++) {
        particles.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            length: Math.random() * 15 + 10,
            radius: Math.random() * 3 + 1,
            speedY: config.mode.includes('RAIN') ? Math.random() * 8 + 8 : Math.random() * 1.2 + 0.5,
            speedX: config.mode.includes('SNOW') ? (Math.random() - 0.5) * 0.5 : 0,
            opacity: Math.random() * 0.4 + 0.3
        });
    }

    // Hjælpefunktion til at tegne en blød sky af cirkler
    function drawCloud(cloud) {
        ctx.fillStyle = config.mode.includes('OVERCAST')
            ? `rgba(200, 205, 215, ${cloud.opacity})` 
            : `rgba(255, 255, 255, ${cloud.opacity})`;

        ctx.beginPath();
        ctx.arc(cloud.x, cloud.y, cloud.radius, 0, Math.PI * 2);
        ctx.arc(cloud.x + cloud.radius * 0.6, cloud.y - cloud.radius * 0.2, cloud.radius * 0.7, 0, Math.PI * 2);
        ctx.arc(cloud.x - cloud.radius * 0.6, cloud.y - cloud.radius * 0.1, cloud.radius * 0.6, 0, Math.PI * 2);
        ctx.fill();
    }

    // Hjælpefunktion til at tegne en måneskinssigdl / full måne med glød
    function drawMoon() {
        const moonX = canvas.width * 0.75;
        const moonY = canvas.height * 0.2;
        const radius = 35;

        ctx.save();
        
        // Måneglød (outer glow)
        const glowGradient = ctx.createRadialGradient(moonX, moonY, radius * 0.8, moonX, moonY, radius * 2.5);
        glowGradient.addColorStop(0, 'rgba(255, 255, 230, 0.3)');
        glowGradient.addColorStop(1, 'rgba(255, 255, 230, 0)');
        ctx.fillStyle = glowGradient;
        ctx.beginPath();
        ctx.arc(moonX, moonY, radius * 2.5, 0, Math.PI * 2);
        ctx.fill();

        // Måne-krop
        ctx.beginPath();
        ctx.arc(moonX, moonY, radius, 0, Math.PI * 2);
        ctx.fillStyle = '#fffde8';
        ctx.fill();

        ctx.restore();
    }

    // Tegn funklende stjerner
    function drawStars() {
        stars.forEach(star => {
            star.phase += star.twinkleSpeed;
            const opacity = star.baseOpacity + Math.sin(star.phase) * 0.3;

            ctx.save();
            ctx.beginPath();
            ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
            ctx.fillStyle = star.color;
            ctx.globalAlpha = Math.max(0.1, Math.min(1, opacity));
            ctx.shadowBlur = star.radius > 1.2 ? 6 : 0; // Giv de største stjerner en svag glød
            ctx.shadowColor = star.color;
            ctx.fill();
            ctx.restore();
        });
    }

    // Håndter og tegn stjerneskud
    function updateAndDrawShootingStars() {
        // Tilføj et nyt stjerneskud med tilfældige mellemrum
        if (Math.random() < 0.008 && shootingStars.length < 2) {
            shootingStars.push({
                x: Math.random() * canvas.width * 0.7,
                y: Math.random() * (canvas.height * 0.3),
                length: Math.random() * 90 + 50,
                speed: Math.random() * 10 + 6,
                angle: Math.PI / 4 + (Math.random() - 0.5) * 0.2, // Skråt nedad
                opacity: 1
            });
        }

        shootingStars.forEach((ss, index) => {
            ctx.save();
            ctx.strokeStyle = `rgba(255, 255, 255, ${ss.opacity})`;
            ctx.lineWidth = 1.8;
            ctx.lineCap = 'round';
            
            // Tegn halen af stjerneskuddet
            ctx.beginPath();
            ctx.moveTo(ss.x, ss.y);
            ctx.lineTo(ss.x - Math.cos(ss.angle) * ss.length, ss.y - Math.sin(ss.angle) * ss.length);
            ctx.stroke();
            ctx.restore();

            // Flyt stjerneskuddet
            ss.x += Math.cos(ss.angle) * ss.speed;
            ss.y += Math.sin(ss.angle) * ss.speed;
            ss.opacity -= 0.025; // Fad ud

            // Fjern når det er væk
            if (ss.opacity <= 0) {
                shootingStars.splice(index, 1);
            }
        });
    }

    // 4. Hoved-animationsloop
    function animate() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // A1. Tegn Stjerner (hvis relevant)
        if (config.showStars) {
            drawStars();
        }

        // A2. Tegn Sol eller Måne (hvis relevant)
        if (config.showSun) {
            ctx.beginPath();
            ctx.arc(canvas.width * 0.75, canvas.height * 0.2, 50, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(255, 230, 40, 0.9)';
            ctx.fill();
        } else if (config.showMoon) {
            drawMoon();
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
        if (config.mode.includes('RAIN')) {
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
        else if (config.mode.includes('SNOW')) {
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