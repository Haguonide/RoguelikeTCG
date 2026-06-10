/* Main app — game state + composition */
const { useState, useEffect, useRef, useMemo } = React;

const INITIAL_PATTERNS = [
  { name: 'COLONNE PURE', pattern: [[1,0,0],[1,0,0],[1,0,0]], reward: 4, scored: false },
  { name: 'TRIANGLE', pattern: [[0,1,0],[1,0,1],[0,0,0]], reward: 6, scored: false },
  { name: 'DIAGONALE', pattern: [[1,0,0],[0,1,0],[0,0,1]], reward: 5, scored: false },
];

const STARTER_DECK = [
  { id: 'c1', name: 'INTERN HEROIQUE', cost: 1, hp: 2, cd: 1, atk: 'forward', tone: '', glyph: '🦸', keyword: 'STAGIAIRE' },
  { id: 'c2', name: 'EX-VILAIN PROBA', cost: 2, hp: 3, cd: 2, atk: 'cross', tone: 'purple', glyph: '🦹', keyword: 'CONDITIONNELLE' },
  { id: 'c3', name: 'RETRAITE FORCE', cost: 3, hp: 5, cd: 1, atk: 'all', tone: 'gold', glyph: '🧓', keyword: 'GROGNON' },
  { id: 'c4', name: 'CDI-MAN', cost: 2, hp: 4, cd: 2, atk: 'line', tone: 'blue', glyph: '👔' },
  { id: 'c5', name: 'CONSULTANT', cost: 4, hp: 6, cd: 3, atk: 'diag', tone: 'green', glyph: '💼', keyword: 'BURNOUT' },
];

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "playerColor": "#ff5a3c",
  "enemyColor": "#6c4bff",
  "goldColor": "#ffc83a",
  "showCellLabels": true,
  "portraitsSide": "side",
  "gridSize": "M",
  "showGuides": true
}/*EDITMODE-END*/;

function makeId() { return 'u' + Math.random().toString(36).slice(2, 8); }

function App() {
  const [tweaks, setTweak] = useTweaks(TWEAK_DEFAULTS);

  const [turn, setTurn] = useState(1);
  const [round, setRound] = useState(1);
  const [activeSide, setActiveSide] = useState('player'); // 'player' | 'enemy'

  const [pHp, setPHp] = useState(28);
  const [eHp, setEHp] = useState(30);
  const [pShield, setPShield] = useState(3);

  const [mana, setMana] = useState(3);
  const [maxMana, setMaxMana] = useState(3);
  const [gold, setGold] = useState(42);

  const [deckCount, setDeckCount] = useState(18);
  const [discardCount, setDiscardCount] = useState(4);

  const [pScore, setPScore] = useState(7);
  const [eScore, setEScore] = useState(4);

  const [patterns, setPatterns] = useState(INITIAL_PATTERNS);

  const [hand, setHand] = useState(STARTER_DECK.slice(0, 5).map(c => ({ ...c, uid: makeId() })));

  // grid: 9 cells, indexed 0..8 (rows top→bottom)
  // top row = enemy zone, bottom row = player zone, middle = contested
  const [grid, setGrid] = useState(() => {
    const g = Array(9).fill(null);
    g[1] = { uid: makeId(), name: 'CDI-MAN', hp: 4, cd: 2, atk: 'line', glyph: '👔', owner: 'enemy', tone: 'blue' };
    g[5] = { uid: makeId(), name: 'INTERN', hp: 2, cd: 1, atk: 'forward', glyph: '🦸', owner: 'player', tone: '' };
    return g;
  });

  const [dragging, setDragging] = useState(null);
  const [hoverCell, setHoverCell] = useState(null);
  const [toast, setToast] = useState(null);

  function showToast(t) {
    setToast(t);
    setTimeout(() => setToast(null), 1400);
  }

  const validCellForCard = (idx) => {
    if (!dragging) return false;
    if (grid[idx]) return false;
    // player can place in bottom row (6,7,8) and middle row (3,4,5)
    return idx >= 3;
  };

  function onDragStart(card) { setDragging(card); }
  function onDragEnd() { setDragging(null); setHoverCell(null); }

  function onCellDragOver(idx, e) {
    if (!dragging) return;
    if (validCellForCard(idx)) {
      e.preventDefault();
      setHoverCell(idx);
    }
  }
  function onCellDrop(idx) {
    if (!dragging || !validCellForCard(idx)) return;
    if (mana < dragging.cost) { showToast('PAS ASSEZ DE MANA'); return; }
    const next = [...grid];
    next[idx] = { uid: makeId(), name: dragging.name, hp: dragging.hp, cd: dragging.cd, atk: dragging.atk, glyph: dragging.glyph, owner: 'player', tone: dragging.tone };
    setGrid(next);
    setMana(m => m - dragging.cost);
    setHand(h => h.filter(c => c.uid !== dragging.uid));
    setDiscardCount(d => d + 1);
    setDragging(null); setHoverCell(null);
    showToast(`${dragging.name} DEPLOYE`);
    checkPatterns(next);
  }

  function checkPatterns(g) {
    setPatterns(prev => prev.map((p, idx) => {
      if (p.scored) return p;
      const flat = p.pattern.flat();
      const matches = flat.every((v, i) => v ? (g[i] && g[i].owner === 'player') : true);
      if (matches) {
        setTimeout(() => { setPScore(s => s + p.reward); showToast(`MOTIF SCORE +${p.reward}`); }, 300);
        return { ...p, scored: true };
      }
      return p;
    }));
  }

  function endTurn() {
    setActiveSide('enemy');
    showToast('TOUR ENNEMI');
    setTimeout(() => {
      // tiny enemy AI: try to fill an empty top-row cell
      setGrid(g => {
        const next = [...g];
        const slot = [0, 1, 2].find(i => !next[i]);
        if (slot !== undefined) {
          next[slot] = { uid: makeId(), name: 'AGENT №1', hp: 3, cd: 2, atk: 'forward', glyph: '🤵', owner: 'enemy', tone: 'purple' };
        }
        return next;
      });
    }, 700);
    setTimeout(() => {
      setActiveSide('player');
      const newTurn = turn + 1;
      setTurn(newTurn);
      const newMax = Math.min(maxMana + 1, 10);
      setMaxMana(newMax);
      setMana(newMax);
      // draw a card
      setHand(h => {
        if (h.length >= 7) return h;
        const draw = STARTER_DECK[Math.floor(Math.random() * STARTER_DECK.length)];
        return [...h, { ...draw, uid: makeId() }];
      });
      setDeckCount(d => Math.max(0, d - 1));
      showToast('TON TOUR');
    }, 1900);
  }

  // expose tweaks as CSS vars
  const styleVars = {
    '--player': tweaks.playerColor,
    '--enemy': tweaks.enemyColor,
    '--gold': tweaks.goldColor,
  };

  // grid scaling
  const gridDims = useMemo(() => ({
    S: { w: 640, h: 480, top: 320 },
    M: { w: 760, h: 560, top: 290 },
    L: { w: 880, h: 640, top: 250 },
  }[tweaks.gridSize] || { w: 760, h: 560, top: 290 }), [tweaks.gridSize]);

  return (
    <StageScaler>
      <div className="stage" style={styleVars}>
        <SuperheroBackground />

        {/* TOP BAR */}
        <div className="topbar">
          <div className="gold-pill">
            <div className="gold-coin">$</div>
            {gold}
          </div>
          <div className="relics">
            <div className="relic r1" title="Café de Combat">☕</div>
            <div className="relic r2" title="Badge Périmé">🪪</div>
            <div className="relic r3" title="Trombone Mythique">📎</div>
            <div className="relic r4" title="Tasse Brisée">🏆</div>
          </div>
          <div className="turn-pill">CH.03 — RD {round} — TR {turn}</div>
        </div>

        {/* CRT pattern board */}
        <CRTPatternDisplay patterns={patterns} turn={turn} round={round} />

        {/* Scoreboards flank the CRT */}
        <ScoreBar side="left" score={pScore} label="JOUEUR" />
        <ScoreBar side="right" score={eScore} label="ENNEMI" />

        {/* Portraits */}
        <PortraitBlock side="left"  name="MIDNIGHT MARV" role="EX-VILAIN, CDI" hp={pHp} maxHp={32} shield={pShield}
          status={[{ kind: 'poison', label: '🌿 2' }]} />
        <PortraitBlock side="right" name="DIRECTOR ZERO" role="HEROISME PRIVE" hp={eHp} maxHp={36} />

        {/* GRID arena */}
        <div className="arena" style={{ width: gridDims.w, height: gridDims.h, top: gridDims.top }}>
          {tweaks.showGuides && <div className="row-tag r-top">ENNEMI</div>}
          {tweaks.showGuides && <div className="row-tag r-bot">JOUEUR</div>}
          <div className="grid">
            {grid.map((unit, idx) => {
              const row = Math.floor(idx / 3);
              const rowClass = row === 0 ? 'row-enemy' : row === 2 ? 'row-player' : 'row-mid';
              const drop = hoverCell === idx;
              const invalid = dragging && !validCellForCard(idx);
              return (
                <div
                  key={idx}
                  className={`cell ${rowClass} ${drop ? 'droppable' : ''} ${invalid && hoverCell === idx ? 'invalid' : ''}`}
                  onDragOver={(e) => onCellDragOver(idx, e)}
                  onDragLeave={() => setHoverCell(null)}
                  onDrop={() => onCellDrop(idx)}
                >
                  {tweaks.showCellLabels && <div className="cell-label">{`R${row+1}C${(idx%3)+1}`}</div>}
                  {unit && <Unit data={unit} owner={unit.owner} />}
                </div>
              );
            })}
          </div>
        </div>

        {/* HAND */}
        <div className="hand-zone">
          <div className="hand">
            {hand.map(c => (
              <Card
                key={c.uid}
                card={c}
                dragging={dragging && dragging.uid === c.uid}
                unaffordable={c.cost > mana}
                onDragStart={onDragStart}
                onDragEnd={onDragEnd}
              />
            ))}
          </div>
        </div>

        {/* RESOURCES */}
        <div className="resources">
          <div className="mana-orb">
            <div className="num">{mana}/{maxMana}</div>
            <div className="label">MANA</div>
          </div>
          <div className="pile">
            <div className="pile-icon">🂠</div>
            <div className="pile-label">DECK</div>
            <div className="pile-num">{deckCount}</div>
          </div>
          <div className="pile discard">
            <div className="pile-icon">🗑</div>
            <div className="pile-label">DEFAUSSE</div>
            <div className="pile-num">{discardCount}</div>
          </div>
        </div>

        {/* END TURN */}
        <button
          className={`end-turn ${activeSide === 'enemy' ? 'enemy-turn' : ''}`}
          onClick={() => activeSide === 'player' && endTurn()}
        >
          <span className="small">{activeSide === 'enemy' ? 'ATTENDS...' : 'TON TOUR'}</span>
          {activeSide === 'enemy' ? 'ENNEMI JOUE' : 'FIN DE TOUR'}
        </button>

        {toast && <div className="toast">{toast}</div>}

        {/* TWEAKS */}
        <TweaksPanel title="Tweaks">
          <TweakSection title="Couleurs">
            <TweakColor label="Joueur" value={tweaks.playerColor} onChange={v => setTweak('playerColor', v)} />
            <TweakColor label="Ennemi" value={tweaks.enemyColor} onChange={v => setTweak('enemyColor', v)} />
            <TweakColor label="Or" value={tweaks.goldColor} onChange={v => setTweak('goldColor', v)} />
          </TweakSection>
          <TweakSection title="Layout">
            <TweakRadio label="Taille grille" value={tweaks.gridSize}
              options={[{ value: 'S', label: 'S' }, { value: 'M', label: 'M' }, { value: 'L', label: 'L' }]}
              onChange={v => setTweak('gridSize', v)} />
            <TweakToggle label="Coordonnées cellules" value={tweaks.showCellLabels} onChange={v => setTweak('showCellLabels', v)} />
            <TweakToggle label="Guides camp" value={tweaks.showGuides} onChange={v => setTweak('showGuides', v)} />
          </TweakSection>
          <TweakSection title="Debug">
            <TweakButton label="+5 mana" onClick={() => setMana(m => m + 5)} />
            <TweakButton label="Score motif aléatoire" onClick={() => setPScore(s => s + 4)} />
            <TweakButton label="Reset HP" onClick={() => { setPHp(28); setEHp(30); }} />
          </TweakSection>
        </TweaksPanel>
      </div>
    </StageScaler>
  );
}

function StageScaler({ children }) {
  const ref = useRef();
  useEffect(() => {
    const fit = () => {
      const el = ref.current;
      if (!el) return;
      const sx = window.innerWidth / 1920;
      const sy = window.innerHeight / 1080;
      const s = Math.min(sx, sy);
      el.style.transform = `scale(${s})`;
    };
    fit();
    window.addEventListener('resize', fit);
    return () => window.removeEventListener('resize', fit);
  }, []);
  return (
    <div className="stage-wrap">
      <div ref={ref} style={{ transformOrigin: 'center center' }}>
        {children}
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);
