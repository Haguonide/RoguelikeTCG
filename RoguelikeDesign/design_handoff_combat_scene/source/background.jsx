/* Superhero comic-book background — halftone sky, action lines, skyline + bolt */
const { useMemo: useMemoBg } = React;

function Skyscraper({ left, right, bottom, w, h, fill = '#0c1a2a', windowsCol = 4, windowsRow = 8, accent = '#3a2d6e' }) {
  const style = { position: 'absolute', left, right, bottom, width: w, height: h };
  const windows = [];
  for (let r = 0; r < windowsRow; r++) {
    for (let c = 0; c < windowsCol; c++) {
      // pseudo-random lit windows
      const lit = ((r * 7 + c * 13 + (left||right||0)) % 5) < 2;
      windows.push(
        <rect
          key={`${r}-${c}`}
          x={8 + c * ((100 - 16) / windowsCol)}
          y={20 + r * ((180 - 40) / windowsRow)}
          width={(100 - 16) / windowsCol - 4}
          height={(180 - 40) / windowsRow - 3}
          fill={lit ? '#ffd86b' : '#1a2540'}
        />
      );
    }
  }
  return (
    <svg style={style} viewBox="0 0 100 200" preserveAspectRatio="none">
      <rect x="0" y="10" width="100" height="190" fill={fill} />
      <rect x="0" y="6" width="100" height="6" fill={accent} />
      {windows}
    </svg>
  );
}

function WaterTower({ left, right, bottom, w = 60, h = 80, fill = '#0c1a2a' }) {
  const style = { position: 'absolute', left, right, bottom, width: w, height: h };
  return (
    <svg style={style} viewBox="0 0 60 80" preserveAspectRatio="none">
      <rect x="22" y="50" width="4" height="30" fill={fill} />
      <rect x="34" y="50" width="4" height="30" fill={fill} />
      <polygon points="10,30 50,30 30,5" fill={fill} />
      <rect x="12" y="30" width="36" height="22" fill={fill} />
    </svg>
  );
}

function Antenna({ left, right, bottom, h = 90, fill = '#0c1a2a' }) {
  const style = { position: 'absolute', left, right, bottom, width: 24, height: h };
  return (
    <svg style={style} viewBox="0 0 24 90" preserveAspectRatio="none">
      <line x1="12" y1="0" x2="12" y2="90" stroke={fill} strokeWidth="3" />
      <line x1="12" y1="20" x2="2" y2="40" stroke={fill} strokeWidth="2" />
      <line x1="12" y1="20" x2="22" y2="40" stroke={fill} strokeWidth="2" />
      <line x1="12" y1="40" x2="4" y2="58" stroke={fill} strokeWidth="2" />
      <line x1="12" y1="40" x2="20" y2="58" stroke={fill} strokeWidth="2" />
      <circle cx="12" cy="4" r="3" fill="#ff5a3c" />
    </svg>
  );
}

function ActionLines() {
  // SVG sunburst rays radiating from upper-center
  const rays = [];
  const cx = 960, cy = 120;
  for (let i = 0; i < 28; i++) {
    const a1 = (i / 28) * Math.PI * 2;
    const a2 = a1 + 0.05;
    const r = 1800;
    const x1 = cx + Math.cos(a1) * r;
    const y1 = cy + Math.sin(a1) * r;
    const x2 = cx + Math.cos(a2) * r;
    const y2 = cy + Math.sin(a2) * r;
    rays.push(
      <polygon
        key={i}
        points={`${cx},${cy} ${x1},${y1} ${x2},${y2}`}
        fill={i % 2 === 0 ? 'rgba(255,220,90,0.18)' : 'rgba(255,255,255,0.05)'}
      />
    );
  }
  return (
    <svg
      style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', pointerEvents: 'none' }}
      viewBox="0 0 1920 1080" preserveAspectRatio="none"
    >
      {rays}
    </svg>
  );
}

function SuperheroBackground() {
  return (
    <>
      {/* Comic sky gradient */}
      <div className="bg sh-sky" />
      {/* Halftone dot pattern */}
      <div className="sh-halftone" />
      {/* Sunburst action lines from above */}
      <ActionLines />
      {/* Big comic POW burst far back */}
      <div className="sh-pow sh-pow-1">
        <svg viewBox="0 0 200 200" width="100%" height="100%">
          <polygon
            points="100,4 116,40 156,18 142,60 196,64 156,92 192,128 144,124 156,168 116,144 104,196 88,144 48,168 60,124 12,128 48,92 8,64 60,60 46,18 86,40"
            fill="#ffd24a" stroke="#0c1a2a" strokeWidth="4"
          />
        </svg>
      </div>
      <div className="sh-pow sh-pow-2">
        <svg viewBox="0 0 200 200" width="100%" height="100%">
          <polygon
            points="100,8 124,52 168,26 152,72 198,72 158,100 192,140 144,128 154,176 110,148 100,196 88,148 46,176 56,128 10,140 44,100 4,72 50,72 32,26 78,52"
            fill="#ff5a3c" stroke="#0c1a2a" strokeWidth="4"
          />
        </svg>
      </div>

      {/* Distant skyline (back layer) */}
      <Skyscraper left={20}   bottom={140} w={180} h={420} fill="#1a2848" accent="#3a4a82" />
      <Skyscraper left={180}  bottom={140} w={210} h={520} fill="#15223e" accent="#324075" />
      <Skyscraper left={370}  bottom={140} w={160} h={380} fill="#1a2848" accent="#3a4a82" />
      <Skyscraper right={40}  bottom={140} w={200} h={500} fill="#15223e" accent="#324075" />
      <Skyscraper right={220} bottom={140} w={170} h={420} fill="#1a2848" accent="#3a4a82" />
      <Skyscraper right={380} bottom={140} w={150} h={360} fill="#15223e" accent="#324075" />

      {/* Foreground skyline (darker, larger) */}
      <Skyscraper left={-40} bottom={0} w={300} h={520} fill="#08111f" accent="#1a2848" windowsCol={4} windowsRow={10} />
      <Antenna   left={210}  bottom={520} h={120} fill="#08111f" />
      <Skyscraper left={240} bottom={0} w={260} h={460} fill="#0a1426" accent="#1a2848" windowsCol={4} windowsRow={9} />
      <WaterTower left={280} bottom={460} w={70} h={90} fill="#08111f" />
      <Skyscraper left={490} bottom={0} w={220} h={380} fill="#08111f" accent="#1a2848" windowsCol={3} windowsRow={8} />

      <Skyscraper right={-40} bottom={0} w={300} h={540} fill="#08111f" accent="#1a2848" windowsCol={4} windowsRow={10} />
      <Antenna    right={210} bottom={540} h={130} fill="#08111f" />
      <Skyscraper right={240} bottom={0} w={250} h={440} fill="#0a1426" accent="#1a2848" windowsCol={4} windowsRow={9} />
      <WaterTower right={280} bottom={440} w={70} h={90} fill="#08111f" />
      <Skyscraper right={480} bottom={0} w={210} h={360} fill="#08111f" accent="#1a2848" windowsCol={3} windowsRow={8} />

      {/* Caped silhouette flying mid-sky (small, dramatic) */}
      <svg
        style={{ position: 'absolute', left: '50%', top: 60, transform: 'translateX(-50%)', width: 200, height: 130 }}
        viewBox="0 0 200 130"
      >
        {/* cape */}
        <path d="M40 30 Q 70 70 100 60 Q 130 70 160 30 Q 150 90 100 90 Q 50 90 40 30 Z" fill="#c83a22" stroke="#0c1a2a" strokeWidth="3" />
        {/* body */}
        <ellipse cx="100" cy="60" rx="14" ry="22" fill="#1a2848" stroke="#0c1a2a" strokeWidth="3" />
        {/* head */}
        <circle cx="100" cy="38" r="9" fill="#0c1a2a" />
        {/* arm forward */}
        <path d="M100 50 Q 130 30 150 22" stroke="#0c1a2a" strokeWidth="6" fill="none" strokeLinecap="round" />
        <circle cx="152" cy="20" r="6" fill="#1a2848" stroke="#0c1a2a" strokeWidth="3" />
      </svg>

      {/* Smog fog at base */}
      <div className="fog sh-fog" style={{ zIndex: 4 }} />
    </>
  );
}

// Keep old name working in case anything else references it
window.ForestBackground = SuperheroBackground;
window.SuperheroBackground = SuperheroBackground;
