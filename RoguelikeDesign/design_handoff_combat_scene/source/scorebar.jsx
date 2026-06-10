/* Split-flap mechanical scoreboard */
const { useState, useEffect, useRef } = React;

function FlipDigit({ value, side }) {
  const [display, setDisplay] = useState(value);
  const [flipping, setFlipping] = useState(false);
  const prev = useRef(value);

  useEffect(() => {
    if (prev.current !== value) {
      setFlipping(true);
      const t = setTimeout(() => {
        setDisplay(value);
      }, 175);
      const t2 = setTimeout(() => {
        setFlipping(false);
        prev.current = value;
      }, 350);
      return () => { clearTimeout(t); clearTimeout(t2); };
    }
  }, [value]);

  return (
    <div className={`flip ${side} ${flipping ? 'flipping' : ''}`}>
      <span className="flip-digit">{display}</span>
    </div>
  );
}

function ScoreBar({ side, score, label }) {
  const tens = Math.floor(score / 10);
  const ones = score % 10;
  return (
    <div className={`scorebar ${side}`}>
      <div className="score-label">{label}</div>
      <div className="flip-row">
        <FlipDigit value={tens} side={side === 'left' ? 'player' : 'enemy'} />
        <FlipDigit value={ones} side={side === 'left' ? 'player' : 'enemy'} />
      </div>
    </div>
  );
}

window.ScoreBar = ScoreBar;
