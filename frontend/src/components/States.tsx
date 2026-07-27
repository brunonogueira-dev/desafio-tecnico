interface ErrorStateProps {
  mensagem: string;
  onRetry?: () => void;
}

export function ErrorState({ mensagem, onRetry }: ErrorStateProps) {
  return (
    <div className="state state-error" role="alert">
      <p>{mensagem}</p>
      {onRetry && (
        <button type="button" className="btn btn-secondary" onClick={onRetry}>
          Tentar novamente
        </button>
      )}
    </div>
  );
}

export function EmptyState({ mensagem }: { mensagem: string }) {
  return (
    <div className="state state-empty">
      <p>{mensagem}</p>
    </div>
  );
}

export function ViagensSkeleton() {
  return (
    <div className="skeleton-list" aria-hidden="true" data-testid="skeleton">
      {[0, 1, 2].map((i) => (
        <div key={i} className="skeleton-card" />
      ))}
    </div>
  );
}

export function Spinner({ label = 'Carregando…' }: { label?: string }) {
  return (
    <div className="state" role="status" aria-live="polite">
      <span className="spinner" aria-hidden="true" />
      <span className="sr-only">{label}</span>
    </div>
  );
}
