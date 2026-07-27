import type { Assento } from '@/services/types';

interface SeatMapProps {
  assentos: Assento[];
  selecionado: number | null;
  onSelecionar: (numero: number) => void;
}

/** Mapa de assentos em layout de ônibus: 2 assentos, corredor, 2 assentos. */
export function SeatMap({ assentos, selecionado, onSelecionar }: SeatMapProps) {
  const fileiras: Assento[][] = [];
  for (let i = 0; i < assentos.length; i += 4) {
    fileiras.push(assentos.slice(i, i + 4));
  }

  return (
    <div className="seatmap" role="group" aria-label="Mapa de assentos">
      <div className="seatmap-legend">
        <span><span className="seat-sample livre" /> Livre</span>
        <span><span className="seat-sample selecionado" /> Selecionado</span>
        <span><span className="seat-sample ocupado" /> Ocupado</span>
      </div>
      <div className="seatmap-cabin">
        {fileiras.map((fileira, idx) => (
          <div className="seat-row" key={idx}>
            {fileira.map((assento, pos) => (
              <Seat
                key={assento.numero}
                assento={assento}
                selecionado={selecionado === assento.numero}
                onSelecionar={onSelecionar}
                comCorredor={pos === 2}
              />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}

interface SeatProps {
  assento: Assento;
  selecionado: boolean;
  comCorredor: boolean;
  onSelecionar: (numero: number) => void;
}

function Seat({ assento, selecionado, comCorredor, onSelecionar }: SeatProps) {
  const estado = assento.ocupado ? 'ocupado' : selecionado ? 'selecionado' : 'livre';
  const rotulo = assento.ocupado
    ? `Assento ${assento.numero}, ocupado`
    : `Assento ${assento.numero}, ${selecionado ? 'selecionado' : 'livre'}`;

  return (
    <button
      type="button"
      className={`seat seat-${estado}${comCorredor ? ' seat-aisle' : ''}`}
      onClick={() => !assento.ocupado && onSelecionar(assento.numero)}
      disabled={assento.ocupado}
      aria-disabled={assento.ocupado}
      aria-pressed={selecionado}
      aria-label={rotulo}
    >
      {assento.numero}
    </button>
  );
}
