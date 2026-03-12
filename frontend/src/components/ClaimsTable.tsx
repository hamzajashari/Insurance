import React from 'react';
import { Claim, ClaimType } from '../types';

interface Props {
  claims: Claim[];
  onDelete: (id: string) => void;
  loading: boolean;
}

const claimTypeLabel: Record<ClaimType, string> = {
  [ClaimType.Collision]: 'Collision',
  [ClaimType.Grounding]: 'Grounding',
  [ClaimType.BadWeather]: 'Bad Weather',
  [ClaimType.Fire]: 'Fire',
};

export const ClaimsTable: React.FC<Props> = ({ claims, onDelete, loading }) => {
  if (loading) return <p className="text-gray-500 py-4">Loading claims...</p>;
  if (claims.length === 0) return <p className="text-gray-400 py-4">No claims found.</p>;

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            {['Name', 'Type', 'Damage Cost', 'Created', 'Cover ID', ''].map((h) => (
              <th
                key={h}
                className="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider"
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-100">
          {claims.map((claim) => (
            <tr key={claim.id} className="hover:bg-blue-50 transition-colors">
              <td className="px-4 py-3 text-sm font-medium text-gray-900">{claim.name}</td>
              <td className="px-4 py-3 text-sm text-gray-600">
                {claimTypeLabel[claim.type] ?? claim.type}
              </td>
              <td className="px-4 py-3 text-sm text-gray-700">
                ${claim.damageCost.toLocaleString()}
              </td>
              <td className="px-4 py-3 text-sm text-gray-600">
                {new Date(claim.created).toLocaleDateString()}
              </td>
              <td className="px-4 py-3 text-xs text-gray-400 font-mono truncate max-w-xs">
                {claim.coverId}
              </td>
              <td className="px-4 py-3 text-right">
                <button
                  onClick={() => onDelete(claim.id)}
                  className="text-red-500 hover:text-red-700 text-sm font-medium transition-colors"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
