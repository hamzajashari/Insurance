import React, { useEffect, useState } from 'react';
import { ClaimType, Cover, CoverType, CreateClaimRequest } from '../types';

interface Props {
  onCreated: () => void;
}

const API_BASE = '';

const coverTypeLabel: Record<CoverType, string> = {
  [CoverType.Yacht]: 'Yacht',
  [CoverType.PassengerShip]: 'Passenger Ship',
  [CoverType.ContainerShip]: 'Container Ship',
  [CoverType.BulkCarrier]: 'Bulk Carrier',
  [CoverType.Tanker]: 'Tanker',
};

const emptyForm = (): CreateClaimRequest => ({
  coverId: '',
  created: '',
  name: '',
  type: ClaimType.Collision,
  damageCost: '',
});

export const ClaimsForm: React.FC<Props> = ({ onCreated }) => {
  const [form, setForm] = useState<CreateClaimRequest>(emptyForm());
  const [covers, setCovers] = useState<Cover[]>([]);
  const [selectedCover, setSelectedCover] = useState<Cover | null>(null);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetch(`${API_BASE}/Covers`)
      .then((r) => r.json())
      .then(setCovers)
      .catch(() => { });
  }, []);

  const handleCoverChange = (coverId: string) => {
    const cover = covers.find((c) => c.id === coverId) ?? null;
    setSelectedCover(cover);
    setForm((f) => ({
      ...f,
      coverId,
      created: cover
        ? new Date(cover.startDate).toLocaleDateString('en-CA')
        : ''
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const res = await fetch(`${API_BASE}/Claims`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...form, damageCost: Number(form.damageCost) }),
      });
      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || `Error ${res.status}`);
      }
      setForm(emptyForm());
      setSelectedCover(null);
      onCreated();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const createdMin = selectedCover
    ? new Date(selectedCover.startDate).toLocaleDateString('en-CA')
    : undefined;

  const createdMax = selectedCover
    ? new Date(selectedCover.endDate).toLocaleDateString('en-CA')
    : undefined;
  return (
    <form onSubmit={handleSubmit} className="bg-white border border-gray-200 rounded-lg p-5 space-y-4">
      <h3 className="text-base font-semibold text-gray-800">New Claim</h3>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded px-3 py-2">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Name</label>
          <input
            required
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Cover</label>
          <select
            required
            value={form.coverId}
            onChange={(e) => handleCoverChange(e.target.value)}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">— Select a cover —</option>
            {covers.map((c) => (
              <option key={c.id} value={c.id}>
                {coverTypeLabel[c.type] ?? c.type} · {new Date(c.startDate).toLocaleDateString()} – {new Date(c.endDate).toLocaleDateString()}
              </option>
            ))}
          </select>
          {covers.length === 0 && (
            <p className="text-xs text-amber-500 mt-1">No covers available — create one first.</p>
          )}
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Claim Type</label>
          <select
            value={form.type}
            onChange={(e) => setForm({ ...form, type: e.target.value as ClaimType })}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value={ClaimType.Collision}>Collision</option>
            <option value={ClaimType.Grounding}>Grounding</option>
            <option value={ClaimType.BadWeather}>Bad Weather</option>
            <option value={ClaimType.Fire}>Fire</option>
          </select>
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Damage Cost ($)</label>
          <input
            required
            type="number"
            min={0}
            max={100000}
            value={form.damageCost}
            onChange={(e) => {
              const value = e.target.value;
              setForm({ ...form, damageCost: value });
            }}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">
            Created Date
            {selectedCover && (
              <span className="text-gray-400 font-normal ml-1">
                ({new Date(selectedCover.startDate).toLocaleDateString()} – {new Date(selectedCover.endDate).toLocaleDateString()})
              </span>
            )}
          </label>
          <input
            required
            type="date"
            value={form.created}
            min={createdMin}
            max={createdMax}
            disabled={!selectedCover}
            onChange={(e) => setForm({ ...form, created: e.target.value })}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50 disabled:text-gray-400"
          />
        </div>
      </div>

      <button
        type="submit"
        disabled={submitting}
        className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium px-4 py-2 rounded transition-colors"
      >
        {submitting ? 'Creating...' : 'Create Claim'}
      </button>
    </form>
  );
};
