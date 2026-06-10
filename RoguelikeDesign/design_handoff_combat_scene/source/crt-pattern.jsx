/* Retro CRT KPI display showing 3 placement patterns */

function PatternMini({ pattern, scored }) {
  return (
    <div className={`pattern ${scored ? 'scored' : ''}`}>
      <div className="pattern-mini">
        {pattern.flat().map((v, i) => (
          <div key={i} className={`pcell ${v ? 'on' : ''}`} />
        ))}
      </div>
      <div className="pattern-label">{scored ? '✓ DONE' : 'TARGET'}</div>
    </div>
  );
}

function PatternSlot({ name, pattern, reward, scored }) {
  return (
    <div className="pattern">
      <div className="pattern-label" style={{ marginBottom: 2 }}>{name}</div>
      <div className="pattern-mini">
        {pattern.flat().map((v, i) => (
          <div key={i} className={`pcell ${v ? 'on' : ''}`} />
        ))}
      </div>
      <div className="pattern-reward">+{reward}</div>
      {scored && <div className="pattern-label" style={{ color: '#fff' }}>✓ SCORED</div>}
    </div>
  );
}

function CRTPatternDisplay({ patterns, turn, round }) {
  return (
    <div className="crt">
      <div className="crt-shell">
        <div className="crt-knob" />
        <div className="crt-knob k2" />
        <div className="crt-bezel-top">
          <span><span className="crt-light"></span>NUMBER ONE INC. // OBJ TRANSMIT</span>
          <span>CH.03 • R{String(round).padStart(2, '0')} • T{String(turn).padStart(2, '0')}</span>
        </div>
        <div className="crt-screen">
          {patterns.map((p, i) => (
            <PatternSlot key={i} {...p} />
          ))}
        </div>
        <div className="crt-foot">
          <span>SIGNAL_OK</span>
          <span>KPI_BRIEF v2.7.1</span>
          <span>SCORE_AT_EOR</span>
        </div>
      </div>
    </div>
  );
}

window.CRTPatternDisplay = CRTPatternDisplay;
