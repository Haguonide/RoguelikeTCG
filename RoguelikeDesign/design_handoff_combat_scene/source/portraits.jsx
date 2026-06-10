/* Portraits + HP bar */

function PortraitArt({ kind }) {
  // Original flat-cartoon hero glyphs (no copyrighted likenesses)
  if (kind === 'player') {
    return (
      <svg viewBox="0 0 180 180" width="100%" height="100%">
        {/* Cape */}
        <path d="M40 80 Q 30 150 50 170 L 130 170 Q 150 150 140 80 Z" fill="#c83a22" stroke="#000" strokeWidth="4"/>
        {/* Body */}
        <rect x="60" y="100" width="60" height="60" fill="#ffd1c2" stroke="#000" strokeWidth="4"/>
        {/* Chest emblem */}
        <polygon points="90,115 100,130 90,145 80,130" fill="#ffc83a" stroke="#000" strokeWidth="3"/>
        {/* Head */}
        <circle cx="90" cy="70" r="32" fill="#fde4cf" stroke="#000" strokeWidth="4"/>
        {/* Mask */}
        <path d="M58 65 Q 90 50 122 65 L 122 78 Q 90 90 58 78 Z" fill="#1a1a1a" stroke="#000" strokeWidth="3"/>
        <circle cx="78" cy="72" r="3" fill="#fff"/>
        <circle cx="102" cy="72" r="3" fill="#fff"/>
        {/* Frown */}
        <path d="M78 90 Q 90 86 102 90" stroke="#000" strokeWidth="3" fill="none" strokeLinecap="round"/>
        {/* Coffee mug — workplace gag */}
        <rect x="30" y="120" width="22" height="22" fill="#fff" stroke="#000" strokeWidth="3"/>
        <path d="M52 125 Q 60 130 52 138" stroke="#000" strokeWidth="3" fill="none"/>
      </svg>
    );
  }
  return (
    <svg viewBox="0 0 180 180" width="100%" height="100%">
      {/* Body — corp suit */}
      <rect x="50" y="100" width="80" height="70" fill="#2c2440" stroke="#000" strokeWidth="4"/>
      {/* Tie */}
      <polygon points="90,100 82,115 90,160 98,115" fill="#ff5a3c" stroke="#000" strokeWidth="3"/>
      {/* Collar */}
      <polygon points="70,100 90,120 110,100 105,95 75,95" fill="#f5f5f5" stroke="#000" strokeWidth="3"/>
      {/* Head */}
      <circle cx="90" cy="65" r="34" fill="#e7d4ff" stroke="#000" strokeWidth="4"/>
      {/* Hair slick */}
      <path d="M58 60 Q 70 30 90 32 Q 115 30 125 55 Q 110 50 90 52 Q 75 55 58 60 Z" fill="#1a1a1a" stroke="#000" strokeWidth="3"/>
      {/* Sunglasses */}
      <rect x="62" y="66" width="22" height="10" rx="3" fill="#000"/>
      <rect x="96" y="66" width="22" height="10" rx="3" fill="#000"/>
      <line x1="84" y1="71" x2="96" y2="71" stroke="#000" strokeWidth="2"/>
      {/* Smug grin */}
      <path d="M76 88 Q 90 96 104 88" stroke="#000" strokeWidth="3" fill="none" strokeLinecap="round"/>
      {/* Badge */}
      <rect x="58" y="120" width="20" height="14" fill="#ffc83a" stroke="#000" strokeWidth="3"/>
      <text x="68" y="131" fontFamily="Bungee" fontSize="9" textAnchor="middle" fill="#000">№1</text>
    </svg>
  );
}

function HPBar({ current, max, side }) {
  const pct = Math.max(0, current / max) * 100;
  return (
    <div className="hp-bar">
      <div className="hp-fill-wrap">
        <div className={`hp-fill ${side}`} style={{ width: `${pct}%` }} />
      </div>
      <div className="hp-num">{current}/{max}</div>
    </div>
  );
}

function PortraitBlock({ side, name, role, hp, maxHp, shield = 0, status = [] }) {
  return (
    <div className={`portrait-block ${side}`}>
      <div className="portrait">
        <div className="pframe">
          <PortraitArt kind={side === 'left' ? 'player' : 'enemy'} />
        </div>
      </div>
      <div className="name-tag">{name}</div>
      <div className="role-tag">{role}</div>
      <HPBar current={hp} max={maxHp} side={side === 'left' ? 'player' : 'enemy'} />
      {(shield > 0 || status.length > 0) && (
        <div className="status-row">
          {shield > 0 && (
            <div className="status-chip shield">🛡 {shield}</div>
          )}
          {status.map((s, i) => (
            <div key={i} className={`status-chip ${s.kind}`}>{s.label}</div>
          ))}
        </div>
      )}
    </div>
  );
}

window.PortraitBlock = PortraitBlock;
