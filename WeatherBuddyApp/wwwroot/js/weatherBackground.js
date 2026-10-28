// 1. Gruppering af Open-Meteo koder
function getPreset(code, isNight) {
    // Solrigt / Næsten klart (0, 1)
    if ([0, 1].includes(code)) {
        return isNight
            ? { mode: 'CLEAR_NIGHT', cloudCount: 0, showSun: false, showMoon: true, showStars: true, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'CLEAR_DAY', cloudCount: 0, showSun: true, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #527bd4 0%, #54b4e8 80%, #81d8f2 100%)' };
    }
    // Delvist skyet (2)
    if (code === 2) {
        return isNight
            ? { mode: 'PARTLY_CLOUDY_NIGHT', cloudCount: 20, showSun: false, showMoon: true, showStars: true, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'PARTLY_CLOUDY_DAY', cloudCount: 15, showSun: true, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #527bd4 0%, #54b4e8 80%, #81d8f2 100%)' };
    }
    // Overskyet / Tåge (3, 45, 48)
    if ([3, 45, 48].includes(code)) {
        return isNight
            ? { mode: 'OVERCAST_NIGHT', cloudCount: 50, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #020722 0%, #002136 100%)' }
            : { mode: 'OVERCAST_DAY', cloudCount: 50, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #4B515D 0%, #A8B2C1 100%)' };
    }
    // Sne (71, 73, 75, 77, 85, 86)
    if ([71, 73, 75, 77, 85, 86].includes(code)) {
        return isNight
            ? { mode: 'SNOW_NIGHT', cloudCount: 55, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #101a35 0%, #26395a 100%)' }
            : { mode: 'SNOW_DAY', cloudCount: 55, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #2b5876 0%, #4e4376 100%)' };
    }
    // Regn / Støvregn / Torden (51-67, 80-82, 95-99)
    return isNight
        ? { mode: 'RAIN_NIGHT', cloudCount: 50, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #050b18 0%, #12232e 50%, #1c3544 100%)' }
        : { mode: 'RAIN_DAY', cloudCount: 50, showSun: false, showMoon: false, showStars: false, bg: 'linear-gradient(180deg, #0f2027 0%, #203a43 50%, #2c5364 100%)' };
}

function initWeatherBackground(weatherCode, isNight) {
    const canvas = document.getElementById('weatherCanvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    let animationFrameId;
    let particles = [];
    let clouds = [];
    let stars = [];
    let shootingStars = [];
    let sceneInitialized = false;
    let lastFrameTime = 0;
    let rainCanvas;
    let rainCtx;
    let rainAnimationFrameId;
    let lastRainFrameTime = 0;
    let cloudCanvas;
    let cloudCtx;
    const frameInterval = 1000 / 30;

    function resize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        if (rainCanvas) {
            rainCanvas.width = canvas.width;
            rainCanvas.height = canvas.height;
            cloudCanvas.width = canvas.width;
            cloudCanvas.height = canvas.height;
        }
        if (sceneInitialized) {
            lastFrameTime = 0;
            if (animationFrameId !== undefined) {
                cancelAnimationFrame(animationFrameId);
            }
            if (!document.hidden) {
                animationFrameId = requestAnimationFrame(animate);
            }
            if (rainCanvas && !document.hidden) {
                lastRainFrameTime = 0;
                if (rainAnimationFrameId !== undefined) {
                    cancelAnimationFrame(rainAnimationFrameId);
                }
                rainAnimationFrameId = requestAnimationFrame(animateRain);
            }
        }
    }
    window.addEventListener('resize', resize);
    resize();

    const config = getPreset(weatherCode, isNight);
    canvas.style.background = config.bg;
    if (config.mode.includes('RAIN')) {
        rainCanvas = document.createElement('canvas');
        rainCanvas.setAttribute('aria-hidden', 'true');
        rainCanvas.style.position = 'fixed';
        rainCanvas.style.top = '0';
        rainCanvas.style.left = '0';
        rainCanvas.style.width = '100vw';
        rainCanvas.style.height = '100vh';
        rainCanvas.style.zIndex = '-1';
        rainCanvas.style.pointerEvents = 'none';
        canvas.insertAdjacentElement('afterend', rainCanvas);
        rainCanvas.width = canvas.width;
        rainCanvas.height = canvas.height;
        rainCtx = rainCanvas.getContext('2d');

        cloudCanvas = document.createElement('canvas');
        cloudCanvas.setAttribute('aria-hidden', 'true');
        cloudCanvas.style.position = 'fixed';
        cloudCanvas.style.top = '0';
        cloudCanvas.style.left = '0';
        cloudCanvas.style.width = '100vw';
        cloudCanvas.style.height = '100vh';
        cloudCanvas.style.zIndex = '-1';
        cloudCanvas.style.pointerEvents = 'none';
        rainCanvas.insertAdjacentElement('afterend', cloudCanvas);
        cloudCanvas.width = canvas.width;
        cloudCanvas.height = canvas.height;
        cloudCtx = cloudCanvas.getContext('2d');
    }

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
            radius: config.mode === 'PARTLY_CLOUDY_DAY'
                ? Math.random() * 24 + 34
                : Math.random() * 40 + 50,
            speed: Math.random() * 0.15 + 0.05, // Meget rolig bevægelse
            opacity: config.mode.includes('OVERCAST') ? 0.85 : 0.6,
            baseY: 0,
            phase: Math.random() * Math.PI * 2,
            sprite: null
        });
        clouds[clouds.length - 1].baseY = clouds[clouds.length - 1].y;
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

    // Draw softly shaded clouds with colors matched to the current sky.
    function drawCloud(cloud, targetCtx = ctx) {
        if (cloud.sprite) {
            targetCtx.drawImage(
                cloud.sprite,
                cloud.x - cloud.spriteOffsetX,
                cloud.y - cloud.spriteOffsetY
            );
            return;
        }

        const r = cloud.radius;
        const padding = 24;
        const originX = r + padding;
        const originY = r * 1.05 + padding;
        let highlight = [255, 255, 255];
        let midtone = [248, 252, 255];
        let shadow = [191, 216, 233];
        let shadowColor = 'rgba(42, 91, 135, 0.18)';

        if (config.mode.includes('OVERCAST')) {
            highlight = [214, 218, 224];
            midtone = [169, 176, 185];
            shadow = [102, 113, 127];
            shadowColor = 'rgba(25, 34, 48, 0.24)';
        } else if (config.mode.includes('RAIN')) {
            highlight = [196, 209, 220];
            midtone = [143, 159, 174];
            shadow = [75, 91, 108];
            shadowColor = 'rgba(8, 20, 32, 0.3)';
        } else if (config.mode.includes('SNOW')) {
            highlight = [250, 253, 255];
            midtone = [224, 235, 245];
            shadow = [159, 181, 201];
            shadowColor = 'rgba(35, 57, 84, 0.2)';
        } else if (config.mode.includes('NIGHT')) {
            highlight = [218, 229, 242];
            midtone = [158, 178, 201];
            shadow = [87, 108, 136];
            shadowColor = 'rgba(0, 5, 20, 0.35)';
        }

        cloud.sprite = document.createElement('canvas');
        cloud.sprite.width = Math.ceil(r * 2.85 + padding * 2);
        cloud.sprite.height = Math.ceil(r * 1.77 + padding * 2);
        cloud.spriteOffsetX = originX;
        cloud.spriteOffsetY = originY;
        const cloudCtx = cloud.sprite.getContext('2d');
        const color = (rgb, opacity) => `rgba(${rgb.join(', ')}, ${opacity})`;
        const cloudGradient = cloudCtx.createLinearGradient(0, originY - r, 0, originY + r * 0.8);
        cloudGradient.addColorStop(0, color(highlight, cloud.opacity));
        cloudGradient.addColorStop(0.55, color(midtone, cloud.opacity));
        cloudGradient.addColorStop(1, color(shadow, cloud.opacity * 0.9));

        cloudCtx.save();
        cloudCtx.fillStyle = cloudGradient;
        cloudCtx.shadowColor = shadowColor;
        cloudCtx.shadowBlur = 18;
        cloudCtx.shadowOffsetY = 8;
        cloudCtx.beginPath();
        cloudCtx.moveTo(originX - r * 0.75, originY + r * 0.45);
        cloudCtx.quadraticCurveTo(originX - r, originY + r * 0.45, originX - r * 0.95, originY + r * 0.15);
        cloudCtx.quadraticCurveTo(originX - r * 0.95, originY - r * 0.2, originX - r * 0.55, originY - r * 0.3);
        cloudCtx.quadraticCurveTo(originX - r * 0.45, originY - r * 0.8, originX + r * 0.05, originY - r * 0.65);
        cloudCtx.quadraticCurveTo(originX + r * 0.35, originY - r * 1.05, originX + r * 0.75, originY - r * 0.72);
        cloudCtx.quadraticCurveTo(originX + r * 1.25, originY - r * 0.75, originX + r * 1.3, originY - r * 0.2);
        cloudCtx.quadraticCurveTo(originX + r * 1.85, originY - r * 0.25, originX + r * 1.8, originY + r * 0.3);
        cloudCtx.quadraticCurveTo(originX + r * 1.8, originY + r * 0.65, originX + r * 1.45, originY + r * 0.68);
        cloudCtx.lineTo(originX - r * 0.55, originY + r * 0.68);
        cloudCtx.quadraticCurveTo(originX - r * 0.85, originY + r * 0.68, originX - r * 0.75, originY + r * 0.45);
        cloudCtx.closePath();
        cloudCtx.fill();
        cloudCtx.restore();

        targetCtx.drawImage(cloud.sprite, cloud.x - originX, cloud.y - originY);
    }

    function drawDayAtmosphere() {
        const glow = ctx.createRadialGradient(
            canvas.width * 0.75,
            canvas.height * 0.2,
            20,
            canvas.width * 0.75,
            canvas.height * 0.2,
            canvas.width * 0.65
        );
        glow.addColorStop(0, 'rgba(255, 241, 190, 0.16)');
        glow.addColorStop(1, 'rgba(255, 241, 190, 0)');
        ctx.fillStyle = glow;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        const horizonHaze = ctx.createLinearGradient(0, canvas.height * 0.58, 0, canvas.height);
        horizonHaze.addColorStop(0, 'rgba(224, 246, 250, 0)');
        horizonHaze.addColorStop(1, 'rgba(224, 246, 250, 0.2)');
        ctx.fillStyle = horizonHaze;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    }

    // Hjælpefunktion til at tegne en måneskinssigdl / full måne med glød
    function drawMoon() {
        const moonX = canvas.width * 0.75;
        const moonY = canvas.height * 0.2;
        const radius = 35;

        ctx.save();
        
        const glowGradient = ctx.createRadialGradient(moonX, moonY, radius * 0.8, moonX, moonY, radius * 2.5);
        glowGradient.addColorStop(0, 'rgba(220, 228, 240, 0.22)');
        glowGradient.addColorStop(1, 'rgba(220, 228, 240, 0)');
        ctx.fillStyle = glowGradient;
        ctx.beginPath();
        ctx.arc(moonX, moonY, radius * 2.5, 0, Math.PI * 2);
        ctx.fill();

        ctx.beginPath();
        ctx.arc(moonX, moonY, radius, 0, Math.PI * 2);
        ctx.clip();

        const surfaceGradient = ctx.createRadialGradient(
            moonX - radius * 0.35,
            moonY - radius * 0.4,
            radius * 0.1,
            moonX,
            moonY,
            radius * 1.3
        );
        surfaceGradient.addColorStop(0, '#d8d9d6');
        surfaceGradient.addColorStop(0.65, '#aeb1b2');
        surfaceGradient.addColorStop(1, '#666d75');
        ctx.fillStyle = surfaceGradient;
        ctx.beginPath();
        ctx.arc(moonX, moonY, radius, 0, Math.PI * 2);
        ctx.fill();

        const maria = [
            [-0.38, -0.3, 0.28, 0.2],
            [0.2, -0.43, 0.34, 0.22],
            [0.36, 0.08, 0.32, 0.25],
            [-0.12, 0.3, 0.38, 0.23]
        ];
        ctx.fillStyle = 'rgba(75, 80, 84, 0.14)';
        maria.forEach(([x, y, width, height]) => {
            ctx.beginPath();
            ctx.ellipse(
                moonX + x * radius,
                moonY + y * radius,
                width * radius,
                height * radius,
                -0.35,
                0,
                Math.PI * 2
            );
            ctx.fill();
        });

        const craters = [
            [-0.48, 0.02, 0.12],
            [-0.12, -0.52, 0.1],
            [0.2, -0.12, 0.15],
            [0.48, 0.39, 0.1],
            [-0.27, 0.52, 0.09],
            [-0.51, -0.48, 0.07]
        ];
        craters.forEach(([x, y, size]) => {
            const craterX = moonX + x * radius;
            const craterY = moonY + y * radius;
            const craterRadius = size * radius;
            const craterGradient = ctx.createRadialGradient(
                craterX - craterRadius * 0.3,
                craterY - craterRadius * 0.35,
                craterRadius * 0.1,
                craterX,
                craterY,
                craterRadius
            );
            craterGradient.addColorStop(0, 'rgba(70, 76, 80, 0.45)');
            craterGradient.addColorStop(0.72, 'rgba(90, 96, 100, 0.24)');
            craterGradient.addColorStop(1, 'rgba(235, 237, 232, 0.45)');
            ctx.beginPath();
            ctx.arc(craterX, craterY, craterRadius, 0, Math.PI * 2);
            ctx.fillStyle = craterGradient;
            ctx.fill();
        });

        ctx.restore();
    }

    function drawSun() {
        const sunX = canvas.width * 0.75;
        const sunY = canvas.height * 0.2;
        const radius = 35;

        ctx.save();

        const glowGradient = ctx.createRadialGradient(sunX, sunY, radius * 0.5, sunX, sunY, radius * 3);
        glowGradient.addColorStop(0, 'rgba(255, 209, 70, 0.87)');
        glowGradient.addColorStop(1, 'rgba(255, 170, 30, 0)');
        ctx.fillStyle = glowGradient;
        ctx.beginPath();
        ctx.arc(sunX, sunY, radius * 3, 0, Math.PI * 2);
        ctx.fill();

        const sunGradient = ctx.createRadialGradient(
            sunX - radius * 0.3,
            sunY - radius * 0.3,
            radius * 0.1,
            sunX,
            sunY,
            radius
        );
        sunGradient.addColorStop(0, '#fffcde');
        sunGradient.addColorStop(1, '#f2ca51');
        ctx.beginPath();
        ctx.arc(sunX, sunY, radius, 0, Math.PI * 2);
        ctx.fillStyle = sunGradient;
        ctx.fill();

        ctx.restore();
    }

    // Tegn funklende stjerner
    function drawStars(frameScale) {
        stars.forEach(star => {
            star.phase += star.twinkleSpeed * frameScale;
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
    function updateAndDrawShootingStars(frameScale) {
        // Tilføj et nyt stjerneskud med tilfældige mellemrum
        if (Math.random() < 1 - Math.pow(1 - 0.008, frameScale) && shootingStars.length < 2) {
            shootingStars.push({
                x: Math.random() * canvas.width * 0.7,
                y: Math.random() * (canvas.height * 0.3),
                length: Math.random() * 90 + 50,
                speed: Math.random() * 10 + 6,
                angle: Math.PI / 4 + (Math.random() - 0.5) * 0.2, // Skråt nedad
                opacity: 1
            });
        }

        for (let index = shootingStars.length - 1; index >= 0; index--) {
            const ss = shootingStars[index];
            const tailX = ss.x - Math.cos(ss.angle) * ss.length;
            const tailY = ss.y - Math.sin(ss.angle) * ss.length;
            const trailGradient = ctx.createLinearGradient(tailX, tailY, ss.x, ss.y);
            trailGradient.addColorStop(0, 'rgba(190, 220, 255, 0)');
            trailGradient.addColorStop(1, 'rgba(255, 255, 255, 1)');

            ctx.save();
            ctx.lineCap = 'round';
            ctx.globalAlpha = ss.opacity;

            ctx.strokeStyle = 'rgba(175, 215, 255, 0.75)';
            ctx.lineWidth = 5;
            ctx.shadowBlur = 14;
            ctx.shadowColor = '#b9dcff';
            ctx.beginPath();
            ctx.moveTo(tailX, tailY);
            ctx.lineTo(ss.x, ss.y);
            ctx.stroke();

            ctx.shadowBlur = 0;
            ctx.strokeStyle = trailGradient;
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(tailX, tailY);
            ctx.lineTo(ss.x, ss.y);
            ctx.stroke();

            ctx.fillStyle = '#ffffff';
            ctx.shadowBlur = 12;
            ctx.shadowColor = '#ffffff';
            ctx.beginPath();
            ctx.arc(ss.x, ss.y, 2.2, 0, Math.PI * 2);
            ctx.fill();
            ctx.restore();

            ss.x += Math.cos(ss.angle) * ss.speed * frameScale;
            ss.y += Math.sin(ss.angle) * ss.speed * frameScale;
            ss.opacity -= 0.025 * frameScale;

            if (ss.opacity <= 0) {
                shootingStars.splice(index, 1);
            }
        }
    }

    function animateRain(timestamp = performance.now()) {
        if (document.hidden || !rainCtx) {
            rainAnimationFrameId = undefined;
            return;
        }

        const frameScale = lastRainFrameTime
            ? Math.min((timestamp - lastRainFrameTime) / (1000 / 60), 2)
            : 1;
        lastRainFrameTime = timestamp;

        rainCtx.clearRect(0, 0, rainCanvas.width, rainCanvas.height);
        rainCtx.strokeStyle = 'rgba(200, 225, 255, 0.6)';
        rainCtx.lineWidth = 1.2;
        rainCtx.beginPath();
        particles.forEach(p => {
            rainCtx.moveTo(p.x, p.y);
            rainCtx.lineTo(p.x, p.y + p.length);

            p.y += p.speedY * frameScale;
            if (p.y > rainCanvas.height) {
                p.y = -p.length;
                p.x = Math.random() * rainCanvas.width;
            }
        });
        rainCtx.stroke();
        rainAnimationFrameId = requestAnimationFrame(animateRain);
    }

    function drawRainClouds() {
        cloudCtx.clearRect(0, 0, cloudCanvas.width, cloudCanvas.height);
        clouds.forEach(cloud => drawCloud(cloud, cloudCtx));
    }

    // 4. Hoved-animationsloop
    function animate(timestamp = performance.now()) {
        if (document.hidden) {
            animationFrameId = undefined;
            return;
        }

        if (lastFrameTime && timestamp - lastFrameTime < frameInterval) {
            animationFrameId = requestAnimationFrame(animate);
            return;
        }

        const frameScale = lastFrameTime
            ? Math.min((timestamp - lastFrameTime) / (1000 / 60), 2)
            : 1;
        lastFrameTime = timestamp;

        ctx.clearRect(0, 0, canvas.width, canvas.height);

        if (config.mode === 'CLEAR_DAY' || config.mode === 'PARTLY_CLOUDY_DAY') {
            drawDayAtmosphere();
        }

        // A1. Tegn Stjerner (hvis relevant)
        if (config.showStars) {
            drawStars(frameScale);
            updateAndDrawShootingStars(frameScale);
        }

        // A2. Tegn Sol eller Måne (hvis relevant)
        if (config.showSun) {
            drawSun()
        } else if (config.showMoon) {
            drawMoon();
        }

        // B. Tegn og flyt skyer
        if (cloudCanvas) {
            drawRainClouds();
        }
        clouds.forEach(c => {
            if (!cloudCanvas) {
                drawCloud(c);
            }
            c.x += c.speed * frameScale;
            c.phase += 0.01 * frameScale;
            c.y = c.baseY + Math.sin(c.phase) * 3;
            // Loop skyen rundt, når den driver ud af skærmen
            if (c.x - c.radius * 2 > canvas.width) {
                c.x = -c.radius * 2;
                c.y = Math.random() * (canvas.height * 0.25);
                c.baseY = c.y;
            }
        });

        // C. Tegn og flyt sne
        if (config.mode.includes('SNOW')) {
            particles.forEach(p => {
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(255, 255, 255, ${p.opacity})`;
                ctx.fill();

                p.y += p.speedY * frameScale;
                p.x += p.speedX * frameScale;

                if (p.y > canvas.height) {
                    p.y = -p.radius;
                    p.x = Math.random() * canvas.width;
                }
            });
        }

        if (clouds.length || particles.length || stars.length) {
            animationFrameId = requestAnimationFrame(animate);
        }
    }

    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            if (animationFrameId !== undefined) {
                cancelAnimationFrame(animationFrameId);
                animationFrameId = undefined;
            }
            if (rainAnimationFrameId !== undefined) {
                cancelAnimationFrame(rainAnimationFrameId);
                rainAnimationFrameId = undefined;
            }
            return;
        }

        lastFrameTime = 0;
        animationFrameId = requestAnimationFrame(animate);
        if (rainCanvas) {
            lastRainFrameTime = 0;
            rainAnimationFrameId = requestAnimationFrame(animateRain);
        }
    });

    sceneInitialized = true;
    animate();
    if (rainCanvas) {
        animateRain();
    }
}