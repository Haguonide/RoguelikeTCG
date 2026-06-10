/* Cards in hand + grid units + arrows */

const ATK_PATTERNS = {
  cross: [[0,1,0],[1,0,1],[0,1,0]],
  forward: [[0,0,0],[0,0,1],[0,0,0]],
  diag: [[1,0,1],[0,0,0],[1,0,1]],
  all: [[1,1,1],[1,0,1],[1,1,1]],
  line: [[0,1,0],[0,0,0],[0,1,0]],
};

function AtkArrows({ pattern }) {
  const arrows = ATK_PATTERNS[pattern] || ATK_PATTERNS.cross;
  const glyph = (r, c) => {
    if (r === 0 && c === 1) return '↑';
    if (r === 2 && c === 1) return '↓';
    if (r === 1 && c === 0) return '←';
    if (r === 1 && c === 2) return '→';
    if (r === 0 && c === 0) return '↖';
    if (r === 0 && c === 2) return '↗';
    if (r === 2 && c === 0) return '↙';
    if (r === 2 && c === 2) return '↘';
    return '';
  };
  return (
    <div className="atk-arrows">
      {arrows.flat().map((v, i) => {
        const r = Math.floor(i / 3); const c = i % 3;
        return (
          <div key={i} className="ax">
            {v ? glyph(r, c) : ''}
          </div>
        );
      })}
    </div>
  );
}

function HpDrops({ hp }) {
  return (
    <div className="unit-hp">
      {Array.from({ length: Math.min(hp, 5) }).map((_, i) => (
        <div key={i} className="hp-drop" />
      ))}
      {hp > 5 && (
        <div style={{ fontFamily: 'Bowlby One', color: '#fff', fontSize: 12, textShadow: '1px 1px 0 #000' }}>×{hp}</div>
      )}
    </div>
  );
}

function Unit({ data, owner }) {
  return (
    <div className={`unit ${owner}`}>
      <div className="cd-badge">{data.cd}</div>
      <HpDrops hp={data.hp} />
      <div className="unit-art">{data.glyph}</div>
      <AtkArrows pattern={data.atk} />
      <div className="unit-name">{data.name}</div>
    </div>
  );
}

function Card({ card, dragging, unaffordable, onDragStart, onDragEnd }) {
  return (
    <div
      className={`card ${dragging ? 'dragging' : ''} ${unaffordable ? 'unaffordable' : ''}`}
      draggable={!unaffordable}
      onDragStart={(e) => { if (!unaffordable) onDragStart(card, e); }}
      onDragEnd={onDragEnd}
      title={card.name}
    >
      <div className="card-cost">{card.cost}</div>
      <div className={`card-art ${card.tone || ''}`}>{card.glyph}</div>
      {card.keyword && <div className="card-keyword">{card.keyword}</div>}
      <div className="card-name">{card.name}</div>
      <div className="card-stats">
        <div className="card-stat"><span className="stat-icon hp">♥</span>{card.hp}</div>
        <div className="card-stat"><span className="stat-icon cd">⏱</span>{card.cd}</div>
        <div className="card-stat"><span className="stat-icon atk">⚔</span>{(ATK_PATTERNS[card.atk] || []).flat().filter(Boolean).length}</div>
      </div>
    </div>
  );
}

window.Unit = Unit;
window.Card = Card;
window.ATK_PATTERNS = ATK_PATTERNS;
