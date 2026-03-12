// String enums match the backend's JsonStringEnumConverter output
export enum ClaimType {
  Collision = 'Collision',
  Grounding = 'Grounding',
  BadWeather = 'BadWeather',
  Fire = 'Fire',
}

export enum CoverType {
  Yacht = 'Yacht',
  PassengerShip = 'PassengerShip',
  ContainerShip = 'ContainerShip',
  BulkCarrier = 'BulkCarrier',
  Tanker = 'Tanker',
}

export interface Claim {
  id: string;
  coverId: string;
  created: string;
  name: string;
  type: ClaimType;
  damageCost: number;
}

export interface Cover {
  id: string;
  startDate: string;
  endDate: string;
  type: CoverType;
  premium: number;
}

export interface CreateClaimRequest {
  coverId: string;
  created: string;
  name: string;
  type: ClaimType;
  damageCost: string;
}

export interface CreateCoverRequest {
  startDate: string;
  endDate: string;
  type: CoverType;
  premium: number;
}
