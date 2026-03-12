import React from 'react';
import { Cover, CoverType } from '../types';

interface Props {
  covers: Cover[];
  onDelete: (id: string) => void;
  loading: boolean;
}

const coverTypeLabel: Record<CoverType, string> = {
  [CoverType.Yacht]: 'Yacht',
  [CoverType.PassengerShip]: 'Passenger Ship',
  [CoverType.ContainerShip]: 'Container Ship',
  [CoverType.BulkCarrier]: 'Bulk Carrier',
  [CoverType.Tanker]: 'Tanker',
};

export const CoversTable: React.FC<Props> = ({ covers, onDelete, loading }) => {
  if (loading) return <p className="text-gray-500 py-4">Loading covers...</p>;
  if (covers.length === 0) return <p className="text-gray-400 py-4">No covers found.</p>;

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            {['Type', 'Start Date', 'End Date', 'Premium', ''].map((h) => (
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
          {covers.map((cover) => (
            <tr key={cover.id} className="hover:bg-blue-50 transition-colors">
              <td className="px-4 py-3 text-sm font-medium text-gray-900">
                {coverTypeLabel[cover.type] ?? cover.type}
              </td>
              <td className="px-4 py-3 text-sm text-gray-600">
                {new Date(cover.startDate).toLocaleDateString()}
              </td>
              <td className="px-4 py-3 text-sm text-gray-600">
                {new Date(cover.endDate).toLocaleDateString()}
              </td>
              <td className="px-4 py-3 text-sm font-semibold text-green-700">
                ${cover.premium.toLocaleString()}
              </td>
              <td className="px-4 py-3 text-right">
                <button
                  onClick={() => onDelete(cover.id)}
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
