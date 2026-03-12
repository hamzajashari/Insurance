import React, { useCallback, useEffect, useState } from 'react';
import { Claim, Cover } from '../types';
import { ClaimsForm } from './ClaimsForm';
import { ClaimsTable } from './ClaimsTable';
import { CoversForm } from './CoversForm';
import { CoversTable } from './CoversTable';

const API_BASE = '';
type Tab = 'claims' | 'covers';

export const ClaimsDashboard: React.FC = () => {
  const [tab, setTab] = useState<Tab>('claims');

  const [claims, setClaims] = useState<Claim[]>([]);
  const [covers, setCovers] = useState<Cover[]>([]);
  const [claimsLoading, setClaimsLoading] = useState(false);
  const [coversLoading, setCoversLoading] = useState(false);
  const [claimsError, setClaimsError] = useState('');
  const [coversError, setCoversError] = useState('');

  const fetchClaims = useCallback(async () => {
    setClaimsLoading(true);
    setClaimsError('');
    try {
      const res = await fetch(`${API_BASE}/Claims`);
      if (!res.ok) throw new Error(`Failed to load claims (${res.status})`);
      setClaims(await res.json());
    } catch (err: any) {
      setClaimsError(err.message);
    } finally {
      setClaimsLoading(false);
    }
  }, []);

  const fetchCovers = useCallback(async () => {
    setCoversLoading(true);
    setCoversError('');
    try {
      const res = await fetch(`${API_BASE}/Covers`);
      if (!res.ok) throw new Error(`Failed to load covers (${res.status})`);
      setCovers(await res.json());
    } catch (err: any) {
      setCoversError(err.message);
    } finally {
      setCoversLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchClaims();
    fetchCovers();
  }, [fetchClaims, fetchCovers]);

  const deleteClaim = async (id: string) => {
    if (!window.confirm('Delete this claim?')) return;
    await fetch(`${API_BASE}/Claims/${id}`, { method: 'DELETE' });
    fetchClaims();
  };

  const deleteCover = async (id: string) => {
    if (!window.confirm('Delete this cover?')) return;
    await fetch(`${API_BASE}/Covers/${id}`, { method: 'DELETE' });
    fetchCovers();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-blue-700 text-white shadow">
        <div className="max-w-6xl mx-auto px-6 py-4 flex items-center gap-3">
          <svg className="w-7 h-7 text-blue-200" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
          </svg>
          <div>
            <h1 className="text-lg font-bold leading-tight">Insurance Claims</h1>
            <p className="text-xs text-blue-200">Management Dashboard</p>
          </div>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-6 space-y-6">
        {/* Tabs */}
        <div className="flex gap-1 bg-white border border-gray-200 rounded-lg p-1 w-fit shadow-sm">
          {(['claims', 'covers'] as Tab[]).map((t) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`px-5 py-2 rounded-md text-sm font-medium transition-colors capitalize ${
                tab === t
                  ? 'bg-blue-600 text-white shadow'
                  : 'text-gray-600 hover:bg-gray-100'
              }`}
            >
              {t}
            </button>
          ))}
        </div>

        {tab === 'claims' && (
          <div className="space-y-5">
            <ClaimsForm onCreated={fetchClaims} />
            <div className="bg-white border border-gray-200 rounded-lg p-5 shadow-sm">
              <h2 className="text-sm font-semibold text-gray-700 mb-3">
                All Claims {!claimsLoading && `(${claims.length})`}
              </h2>
              {claimsError && (
                <p className="text-red-500 text-sm mb-2">{claimsError}</p>
              )}
              <ClaimsTable claims={claims} onDelete={deleteClaim} loading={claimsLoading} />
            </div>
          </div>
        )}

        {tab === 'covers' && (
          <div className="space-y-5">
            <CoversForm onCreated={fetchCovers} />
            <div className="bg-white border border-gray-200 rounded-lg p-5 shadow-sm">
              <h2 className="text-sm font-semibold text-gray-700 mb-3">
                All Covers {!coversLoading && `(${covers.length})`}
              </h2>
              {coversError && (
                <p className="text-red-500 text-sm mb-2">{coversError}</p>
              )}
              <CoversTable covers={covers} onDelete={deleteCover} loading={coversLoading} />
            </div>
          </div>
        )}
      </main>
    </div>
  );
};
