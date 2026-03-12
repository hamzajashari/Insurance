import React, { useState } from 'react';
import { CoverType, CreateCoverRequest } from '../types';

interface Props {
  onCreated: () => void;
}

const API_BASE = '';

const offsetDate = (base: string, days: number): string => {
  const d = new Date(base);
  d.setDate(d.getDate() + days);
  return d.toISOString().split('T')[0];
};

export const CoversForm: React.FC<Props> = ({ onCreated }) => {
  const today = new Date().toISOString().split('T')[0];
  const [form, setForm] = useState<CreateCoverRequest>({
    startDate: today,
    endDate: '',
    type: CoverType.Yacht,
    premium: 0,
  });
  const [computedPremium, setComputedPremium] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [computing, setComputing] = useState(false);

  // Backend rules:
  //   startDate >= today            → min = today
  //   endDate > startDate           → min = startDate + 1 day
  //   endDate - startDate <= 365    → max = startDate + 365 days
  const endDateMin = form.startDate ? offsetDate(form.startDate, 1) : today;
  const endDateMax = form.startDate ? offsetDate(form.startDate, 365) : '';

  const handleStartDateChange = (startDate: string) => {
    const newMin = offsetDate(startDate, 1);
    const newMax = offsetDate(startDate, 365);
    const endOutOfRange = form.endDate && (form.endDate < newMin || form.endDate > newMax);
    setForm((f) => ({ ...f, startDate, endDate: endOutOfRange ? '' : f.endDate, premium: 0 }));
    setComputedPremium(null);
  };

  const handleEndDateChange = (endDate: string) => {
    setForm((f) => ({ ...f, endDate, premium: 0 }));
    setComputedPremium(null);
  };

  const computePremium = async () => {
    if (!form.startDate || !form.endDate) {
      setError('Select start and end dates first.');
      return;
    }
    setComputing(true);
    setError('');
    try {
      const params = new URLSearchParams({
        startDate: form.startDate,
        endDate: form.endDate,
        coverType: form.type,
      });
      const res = await fetch(`${API_BASE}/Covers/compute?${params}`, { method: 'POST' });
      if (!res.ok) throw new Error(await res.text());
      const premium = await res.json();
      setComputedPremium(premium);
      setForm((f) => ({ ...f, premium }));
    } catch (err: any) {
      setError(err.message);
    } finally {
      setComputing(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const res = await fetch(`${API_BASE}/Covers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(form),
      });
      if (!res.ok) throw new Error(await res.text() || `Error ${res.status}`);
      setForm({ startDate: today, endDate: '', type: CoverType.Yacht, premium: 0 });
      setComputedPremium(null);
      onCreated();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="bg-white border border-gray-200 rounded-lg p-5 space-y-4">
      <h3 className="text-base font-semibold text-gray-800">New Cover</h3>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded px-3 py-2">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Cover Type</label>
          <select
            value={form.type}
            onChange={(e) => setForm({ ...form, type: e.target.value as CoverType })}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value={CoverType.Yacht}>Yacht</option>
            <option value={CoverType.PassengerShip}>Passenger Ship</option>
            <option value={CoverType.ContainerShip}>Container Ship</option>
            <option value={CoverType.BulkCarrier}>Bulk Carrier</option>
            <option value={CoverType.Tanker}>Tanker</option>
          </select>
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Start Date</label>
          <input
            required
            type="date"
            value={form.startDate}
            min={today}
            onChange={(e) => handleStartDateChange(e.target.value)}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">
            End Date
            <span className="text-gray-400 font-normal ml-1">(max 365 days from start)</span>
          </label>
          <input
            required
            type="date"
            value={form.endDate}
            min={endDateMin}
            max={endDateMax}
            onChange={(e) => handleEndDateChange(e.target.value)}
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Premium ($)</label>
          <div className="flex gap-2">
            <input
              type="number"
              value={form.premium || ''}
              readOnly
              className="w-full border border-gray-200 bg-gray-50 rounded px-3 py-2 text-sm text-gray-700"
              placeholder="Compute to fill"
            />
            <button
              type="button"
              onClick={computePremium}
              disabled={computing || !form.endDate}
              className="whitespace-nowrap bg-teal-600 hover:bg-teal-700 disabled:opacity-50 text-white text-xs font-medium px-3 py-2 rounded transition-colors"
            >
              {computing ? '...' : 'Compute'}
            </button>
          </div>
          {computedPremium !== null && (
            <p className="text-xs text-teal-600 mt-1">
              Computed: ${computedPremium.toLocaleString()}
            </p>
          )}
        </div>
      </div>

      <button
        type="submit"
        disabled={submitting || form.premium === 0}
        className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium px-4 py-2 rounded transition-colors"
      >
        {submitting ? 'Creating...' : 'Create Cover'}
      </button>
    </form>
  );
};
